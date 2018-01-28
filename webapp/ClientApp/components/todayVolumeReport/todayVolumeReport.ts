import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Moment from 'moment';

class StockHistory
{
    date: string;
    ltp: number;
    change: number;
    totalTrades: number;
    deliverableQty: number;
    deliveryPercentage: number;
    volumeChange: number;
    tradeValueChange: number;
}

interface StockStats {
    symbol: string;
    series: string;
    ltp: number;
    sector: string;
    avgPriceChange: number[];
    avgVolumeChage: number[];

    // Computed values
    priceChange: number;
    volume10dAvg: number;
    volume5dAvg: number;
    volume: number;
    priceChange5d: number;
    priceChange10d: number;
    marketCapChange: number;
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
    show_total: boolean;
    color_value: boolean;
    has_link: boolean;
}

let fetchedData: StockStats[] = [];
const page_size = 100;

@Component
export default class TodayComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = 1;
    searchPlaceHolder: string = "sec:<sector>,ser:<series>,evl:<expression>,default symbol";
    page_header: string = "Today's Volume Report";
    // Update the status in statusMessage
    statusMessage: string = "Fetching today's volume report from server";

    // Component specific code
    displayItem: StockStats[] = [];
    elements_per_page = page_size;

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Symbol", data_field_name: "symbol", sort_link: true, show_total: false, color_value: false, has_link: true },
        { header_field_name: "Last Price", data_field_name: "ltp", sort_link: true, show_total: false, color_value: false, has_link: false },
        { header_field_name: "Change", data_field_name: "priceChange", sort_link: true, show_total: true, color_value: true, has_link: false },
        { header_field_name: "5d Change", data_field_name: "priceChange5d", sort_link: true, show_total: true, color_value: true, has_link: false },
        { header_field_name: "10d Change", data_field_name: "priceChange10d", sort_link: true, show_total: true, color_value: true, has_link: false },
        { header_field_name: "Volume", data_field_name: "volume", sort_link: true, show_total: false, color_value: false, has_link: false },
        { header_field_name: "Volume/avg 5d", data_field_name: "volume5dAvg", sort_link: true, show_total: false, color_value: true, has_link: false},
        { header_field_name: "Volume/avg 10d", data_field_name: "volume10dAvg", sort_link: true, show_total: false, color_value: true, has_link: false}
    ];

    private updateFetchedData(x: StockStats) {
        x.priceChange = x.avgPriceChange[0];
        x.priceChange5d = x.avgPriceChange[1];
        x.priceChange10d = x.avgPriceChange[2];
        x.volume5dAvg = 1;
        if(x.avgVolumeChage[1] != 0)
            x.volume5dAvg = Number(((x.avgVolumeChage[0] - x.avgVolumeChage[1])/x.avgVolumeChage[1]).toFixed(2));
        if(x.avgVolumeChage[2] != 0)
            x.volume10dAvg = Number(((x.avgVolumeChage[0] - x.avgVolumeChage[2])/x.avgVolumeChage[2]).toFixed(2));

        x.volume = x.avgVolumeChage[0];
        x.marketCapChange = Number((x.ltp * x.volume / 10000000).toFixed(2));
        if(x.priceChange < 0)
            x.marketCapChange *= -1;
    }

    mounted(): void {
        if(fetchedData.length == 0) {
            // Call the HTTP API to fetch company list in json format
            fetch('api/StockData/TodayVolumeReport')
                .then(response => response.json() as Promise<StockStats[]>)
                .then(data => {
                    fetchedData = data;
                    fetchedData.forEach(x => this.updateFetchedData(x));
                    this.displayItem = fetchedData;
                    this.statusMessage = "";
                })
                .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
        } else {
            this.displayItem = fetchedData;
        }
    }

    // Callback function on search
    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("ser:") == 0) this.displayItem = fetchedData.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sec:") == 0) this.displayItem = fetchedData.filter(x => x.sector.toLowerCase().indexOf(searchParam) >= 0);
        else if(query.indexOf("evl:") == 0) {
            let expression:string = "fetchedData.filter(x => " + searchParam + ")";
            this.displayItem = eval(expression);
        }
        else this.displayItem = fetchedData.filter(x => (x.symbol.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
    }

    //Callback function to sort the list
    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            case "symbol":
            case "series":
            case "sector":
                this.displayItem = this.displayItem.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
            case "ltp":
            case "volume":
            case "volume10dAvg":
            case "volume5dAvg":
            case "priceChange":
            case "priceChange5d":
            case "priceChange10d":
            case "marketCapChange":
                this.displayItem = this.displayItem.sort((left, right): number => (left[sortKey] - right[sortKey]) * this.sortReverse);
                break;
        }
    }

    linkClick(indexRow: number, indexCol: number, key: string): void {
        if (indexCol != -1) {
            switch (this.table_display_data[indexCol].data_field_name) {
                case "symbol":
                    {
                        this.loadHistory(this.displayItem[indexRow].symbol);
                        break;
                    }
                default:
                    alert("wrong field in linkClick '" + this.table_display_data[indexCol].data_field_name + "'");
                    break;
            }
        } else {
            if (key == "sector") {
                this.searchQuery = "sec:" + this.displayItem[indexRow].sector;
                this.onSearch();
            }
            else {
                alert("wrong field in linkClick(key) '" + key + "'");
            }
        }
    }

    showMoreElements(sector: string) {
        this.elements_per_page += page_size;
        if(this.elements_per_page >= fetchedData.length)
            this.elements_per_page = fetchedData.length;
    }

    showLessElements(sector: string) {
        this.elements_per_page -= page_size;
        if(this.elements_per_page < page_size)
            this.elements_per_page = page_size;
    }

    displayItemHistory: StockHistory[] = [];
    table_history_display_data: DisplayItems[] = [
            { header_field_name: "date", data_field_name: "date", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "ltp", data_field_name: "ltp", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "change", data_field_name: "change", sort_link: false, color_value: true, show_total: false, has_link:false },
            { header_field_name: "totalTrades", data_field_name: "totalTrades", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "deliverableQty", data_field_name: "deliverableQty", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "deliveryPercentage", data_field_name: "deliveryPercentage", sort_link: false, color_value: false, show_total: false, has_link:false },
            { header_field_name: "Volume Change(Times)", data_field_name: "volumeChange", sort_link: false, color_value: true, show_total: false, has_link:false },
            { header_field_name: "Trade Value change CR", data_field_name: "tradeValueChange", sort_link: false, color_value: true, show_total: false, has_link:false }
    ];

    stock_symbol:string = "";
    loadHistory(symbol: string): void {
        //this.flag = true;
        this.stock_symbol = symbol;
        this.displayItemHistory = [];
        // Call the HTTP API to fetch company list in json format
        fetch('api/StockData/GetHistory?symbol='+symbol)
            .then(response => response.json() as Promise<StockHistory[]>)
            .then(data => {
                this.displayItemHistory = data;
                this.displayItemHistory.forEach(x => x.date =  Moment(String(x.date)).format('DD/MM/YYYY'));
                let i:number = 0;
                for(i = 0; i < this.displayItemHistory.length - 1; i++)
                {
                    this.displayItemHistory[i].tradeValueChange = Number(((this.displayItemHistory[i].volumeChange * this.displayItemHistory[i+1].deliverableQty *  this.displayItemHistory[i+1].ltp)/10000000).toFixed(2));
                }
            })
            .catch(reason => alert("Failed due to" + reason));
    }
}
