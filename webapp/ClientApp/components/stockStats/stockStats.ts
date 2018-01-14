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
    openPrice: number;
}

interface StockMonthlyStats
{
    year: number;
    symbol: string;
    change: number[];
    sector: string;
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
const page_size = 100;

@Component
export default class StockStatsComponent extends Vue {
    stats: StockMonthlyStats[] = [];
    statusMessage: string = "Loading data from server";
    searchQuery: string = "";
    sortReverse = -1;

    // Requried for saving sort order when moving between years
    sortKey = '';
    sortIndex = 0;
    sortDirection = 0;

    year = Moment().year();
    month = 0;


    elements_per_page = page_size;


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
        .then(data => {
                this.year = year;
                cachedStats = data;
                this.stats = cachedStats;
                this.statusMessage = data.length == 0 ? ("No enteries for year " + year.toString()): "";
                this.onSearch();
                this.sortBy(this.sortKey, this.sortIndex, this.sortDirection);
            })
        .catch(reason => this.statusMessage = "API 'StockData/GetStockMonthlyStats' failed with error \"" + reason + "\"");
    }

    mounted(): void {
        this.loadStats(this.year);
    }

    sortBy(key: string, index: number, direction: number = 0) {
        if(direction == 0)
            this.sortReverse *= -1;
        else
            this.sortReverse = direction;
        this.sortKey = key;
        this.sortIndex = index;
        this.sortDirection = this.sortReverse;
        switch(key)
        {
            case 'change':
                this.stats = this.stats.sort((left, right):number => { return (left.change[index] - right.change[index]) * this.sortReverse;});
                break;
            case 'sector':
            case 'symbol':
                this.stats = this.stats.sort((left, right): number => left[key].localeCompare(right[key]) * this.sortReverse);
                break;
        }
    }

    private allPositive(change: number[], threshold: number): boolean {
        let i:number = 0, count:number = 0;
        for(i = 0; i < change.length; i++) {
            if(change[i] <= 0)
                count++;
            if(count > threshold)
                return false;
        }
        return true;
    }

    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("pos:") == 0) this.stats = cachedStats.filter(x => this.allPositive(x.change, 2));
        else if(query.indexOf("sec:") == 0) this.stats = cachedStats.filter(x => x.sector.toLowerCase().indexOf(searchParam) >= 0);
        else this.stats = cachedStats.filter(x =>  x.symbol.toLowerCase().indexOf(query) >= 0 || x.sector.toLowerCase().indexOf(query) >= 0);

        this.sortBy(this.sortKey, this.sortIndex, this.sortDirection);
    }

    showMoreElements(sector: string) {
        this.elements_per_page += page_size;
        if(this.elements_per_page >= cachedStats.length)
            this.elements_per_page = cachedStats.length;
    }

    showLessElements(sector: string) {
        this.elements_per_page -= page_size;
        if(this.elements_per_page < page_size)
            this.elements_per_page = page_size;
    }

    onSectorClick(sector: string): void {
        this.searchQuery = "sec:" + sector;
        this.onSearch();
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

    /* Stock histry dialog box releated query operations*/

    displayItemHistory: StockHistory[] = [];

    table_history_display_data: DisplayItems[] = [
            { header_field_name: "date", data_field_name: "date", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "O Price", data_field_name: "openPrice", sort_link: false, color_value: false, show_total: false, has_link:false },
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
                this.month = month;
            })
            .catch(reason => alert("Failed due to" + reason));
    }

    onClickChange(symbol: string, month: number) : void {
        this.loadHistory(symbol, month, this.year);
    }

    loadMoreStatsData(): void {
        var month: number = this.month - 1;
        var year: number = this.year;

        if (month == 0) {
            year--;
            month = 12;
        }

        // Call the HTTP API to fetch company list in json format
        fetch('api/StockData/GetStockHistoryForMonth?symbol=' + this.stock_symbol + "&month=" + month + "&year=" + year)
            .then(response => response.json() as Promise<StockHistory[]>)
            .then(data => {
                data.forEach(x => x.date = Moment(String(x.date)).format('DD/MM/YYYY'));
                this.displayItemHistory = this.displayItemHistory.concat(data);
            })
            .catch(reason => alert("Failed due to" + reason));
    }
}