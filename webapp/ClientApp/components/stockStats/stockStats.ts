import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Moment from 'moment';


interface StockMonthlyStats
{
    year: number;
    symbol: string;
    change: number[];
}


let cachedStats: StockMonthlyStats[] = [];

@Component
export default class StockStatsComponent extends Vue {
    stats: StockMonthlyStats[] = [];
    statusMessage: string = "Loading data from server";
    searchQuery: string = "";
    sortReverse: number = -1;
    year: number = 2017;

    constructor()
    {
        super();
        Vue.filter('monthToString', function(value:number)  {
            if (value) { return Moment().months(value-1).format('MMMM'); }
        });
    }

    loadStats(year:number) : void {
        fetch('api/StockData/GetStockMonthlyStats?year='+year.toString())
        .then(response => response.json() as Promise<StockMonthlyStats[]>)
        .then(data => { cachedStats = this.stats = data; this.statusMessage = data.length == 0 ? ("No enteries for year " + year.toString()): ""; })
        .catch(reason => this.statusMessage = "API 'StockData/GetStockMonthlyStats' failed with error \"" + reason + "\"");
    }

    mounted(): void {
        this.loadStats(this.year);
    }

    sortBy(key: string, index: number) {
        this.sortReverse *= -1;
        switch(key)
        {
            case 'change':
                this.stats.sort((left, right):number => {
                    return (left.change[index] - right.change[index]) * this.sortReverse;
                });
                break;
            case 'symbol':
                this.stats = this.stats.sort((left, right): number => left[key].localeCompare(right[key]) * this.sortReverse);
                break;
        }
    }

    private allPositive(change: number[]): boolean {
        let i:number = 0, count:number = 0;
        for(i = 0; i < change.length; i++) {
            if(change[i] <= 0)
            {
                count++;
            }
            if(count > 3)
                return false;
        }
        return true;
    }

    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("pos:") == 0)
            this.stats = cachedStats.filter(x => this.allPositive(x.change));
        else {
            this.stats = cachedStats.filter(x =>  x.symbol.toLowerCase().indexOf(query) >= 0);
        }
    }

    lastYear(): void {
        if(this.year == 2016) return;
        this.year--;
        this.loadStats(this.year);
    }

    nextYear(): void {
        if(this.year == 2018) return;
        this.year++;
        this.loadStats(this.year);
    }
}