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

let sectors_cached: string[] = [];
let stats_cached: SectorStats[] = [];

@Component
export default class SectorStatsComponent extends Vue {
    page_header: string = "Sector Stats";
    stats: SectorStats[] = [];
    sectors: string[] = [];

    sectorStats: MonthlyStats[] = [];
    statusMessage: string = "Loading Sector Stats";
    sortReverse: number = -1;
    searchQuery: string = "";

    year_2016: number[] = [];
    year_2017: number[] = [];
    year_2018: number[] = [];

    readonly month_list:number[] = [1,2,3,4,5,6,7,8,9,10,11,12];

    constructor() {
        super();
        Vue.filter('monthToString', function(value:number)  {
            if (value) { return Moment().months(value-1).format('MMMM'); }
        });
    }


    updateData() {

    }

    mounted(): void {
        fetch('api/StockData/GetSectorStats')
        .then(response => response.json() as Promise<SectorStats[]>)
        .then(data => {
                stats_cached = this.stats = data;
                data.forEach(x => { if(x.industry != "") this.sectors.push(x.industry)});
                sectors_cached = this.sectors;
                this.statusMessage = "";
                this.onSectorClick(this.sectors[0]);
        })
        .catch(reason => this.statusMessage = "API 'StockData/GetSectorStats' failed with error \"" + reason + "\"");
    }

    onSectorClick(sector: String) {
        let stat = this.stats.filter(x => x.industry == sector);
        this.sectorStats = stat[0].stats;
        let i:number = 0;
        for(i = 0; i < this.sectorStats.length; i++)
        {
            switch(this.sectorStats[i].year)
            {
                case 2016:
                    this.year_2016[this.sectorStats[i].month] = this.sectorStats[i].change;
                    console.log("SJJ");
                    break;
                case 2017:
                    this.year_2017[this.sectorStats[i].month] = this.sectorStats[i].change;
                    break;
                case 2018:
                    this.year_2018[this.sectorStats[i].month] = this.sectorStats[i].change;
                    break;
            }
        }
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

    onSearchSector(): void {
        let query: string = this.searchQuery.toLowerCase();
        this.sectors = sectors_cached.filter(x =>  x.toLowerCase().indexOf(query) >= 0);
    }
}