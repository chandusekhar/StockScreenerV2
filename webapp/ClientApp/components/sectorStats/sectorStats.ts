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
    MonthlyStats: MonthlyStats[];
}


@Component
export default class SectorStatsComponent extends Vue {
    page_header: string = "Sector Stats";
    stats: SectorStats[] = [];
    sectors: string[] = [];
    sectorStats: MonthlyStats[] = [];
    statusMessage: string = "Loading Sector Stats";

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
        stat[0].MonthlyStats.forEach(x => {
            alert(x.month);
        });
    }
}