import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Moment from 'moment';

interface StockHistory
{
    date: string;
    ltp: number;
    change: number;
    totalTrades: number;
    deliverableQty: number;
    deliveryPercentage: number;
    volumeChange: number;
}

interface StockMonthlyStats
{
    year: number;
    symbol: string;
    change: number[];
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
    show_total: boolean;
    color_value: boolean;
    has_link: boolean;
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
        .then(data => { this.year = year; cachedStats = this.stats = data; this.statusMessage = data.length == 0 ? ("No enteries for year " + year.toString()): ""; })
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
        if(this.year <= 2016) {

            return;
        }
        this.loadStats(this.year-1);
    }

    nextYear(): void {
        if(this.year > 2018) return;
        this.loadStats(this.year+1);
    }

    displayItemHistory: StockHistory[] = [];

    table_history_display_data: DisplayItems[] = [
            { header_field_name: "date", data_field_name: "date", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "ltp", data_field_name: "ltp", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "change", data_field_name: "change", sort_link: false, color_value: true, show_total: false, has_link:false },
            { header_field_name: "totalTrades", data_field_name: "totalTrades", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "deliverableQty", data_field_name: "deliverableQty", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "deliveryPercentage", data_field_name: "deliveryPercentage", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "Volume Change(Times)", data_field_name: "volumeChange", sort_link: false, color_value: true, show_total: false, has_link:false }
    ];

    stock_symbol:string = "";
    flag: boolean = true;
    loadHistory(symbol: string, month: number, year: number): void {
        this.flag = true;
        this.stock_symbol = symbol;
        this.displayItemHistory = [];
        // Call the HTTP API to fetch company list in json format
        fetch('api/StockData/GetStockHistoryForMonth?symbol='+symbol+"&month="+month+"&year="+year)
            .then(response => response.json() as Promise<StockHistory[]>)
            .then(data => {
                this.displayItemHistory = data;
                this.displayItemHistory.forEach(x => x.date =  Moment(String(x.date)).format('DD/MM/YYYY'));
            })
            .catch(reason => alert("Failed due to" + reason));
    }

    onClickChange(symbol: string, month: number) : void {
        this.loadHistory(symbol, month, this.year);
    }
}