import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Moment from 'moment';

interface MonthlyStats
{
    month: number;
    year: number;
    change: number;
}

interface SectorStats
{
    industry: string;
    stats: MonthlyStats[];
}


@Component
export default class SectorStatsComponent extends Vue {
    page_header: string = "Sector Stats";
    stats: SectorStats[] = [];
    sectors: string[] = [];
    sectorStats: MonthlyStats[] = [];
    statusMessage: string = "Loading Sector Stats";
    sortReverse: number = -1;

    constructor() {
        super();
        Vue.filter('monthToString', function(value:number)  {
            if (value) { return Moment().months(value-1).format('MMMM'); }
        });
    }

    mounted(): void {
        fetch('api/StockData/GetSectorStats')
        .then(response => response.json() as Promise<SectorStats[]>)
        .then(data => {
                this.stats = data;
                data.forEach(x => { if(x.industry != "") this.sectors.push(x.industry)});
                this.statusMessage = "";
        })
        .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
    }

    onSectorClick(sector: String) {
        let stat = this.stats.filter(x => x.industry == sector);
        this.sectorStats = stat[0].stats;
        this.page_header = "Sector stats for '"  + sector + "'";
        this.sortReverse = -1;
        this.sortBy('month');
    }

    sortBy(key: string) {
        this.sortReverse *= -1;
        switch(key)
        {
            case 'month':
                this.sectorStats.sort((left, right):number => {
                    return (left.month - right.month) * this.sortReverse || right.year - left.year;
                });
                break;
        }
    }
}