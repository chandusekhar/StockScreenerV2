import Vue from 'vue';
import { Component } from 'vue-property-decorator';

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
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
    show_total: boolean;
    color_value: boolean;
}

let fetchedData: StockStats[] = [];

@Component
export default class TodayComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sec:<sector>,ser:<series>,default symbol";
    page_header: string = "Today's Volume Report";
    // Update the status in statusMessage
    statusMessage: string = "Fetching today's volume report from server";

    // Component specific code
    displayItem: StockStats[] = [];

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Symbol", data_field_name: "symbol", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Sector", data_field_name: "sector", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Last Price", data_field_name: "ltp", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Change", data_field_name: "priceChange", sort_link: true, show_total: true, color_value: true },
        { header_field_name: "5d Change", data_field_name: "priceChange5d", sort_link: true, show_total: false, color_value: true },
        { header_field_name: "10d Change", data_field_name: "priceChange10d", sort_link: true, show_total: false, color_value: true },
        { header_field_name: "Volume", data_field_name: "volume", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Volume/avg 5d", data_field_name: "volume5dAvg", sort_link: true, show_total: false, color_value: true},
        { header_field_name: "Volume/avg 10d", data_field_name: "volume10dAvg", sort_link: true, show_total: false, color_value: true}
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
                this.displayItem = this.displayItem.sort((left, right): number => (left[sortKey] - right[sortKey]) * this.sortReverse);
                break;
        }
    }
}
