import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface StockPrice {
    symbol: string;
    series: string;
    industry: string;
    change: number;
    lastPrice: number;
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
}

@Component
export default class TodayComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sym:<symbol>,sec:<sector>,ser:<series>,default companyName";
    // Update the status in statusMessage
    statusMessage: string = "Fetching list of companies from server";

    // Component specific code
    stockPrices: StockPrice[] = [];
    displayItem: StockPrice[] = [];

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Symbol", data_field_name: "symbol", sort_link: true },
        { header_field_name: "Series", data_field_name: "series", sort_link: false },
        { header_field_name: "Sector", data_field_name: "industry", sort_link: true },
        { header_field_name: "Change", data_field_name: "change", sort_link: true },
        { header_field_name: "Last Price", data_field_name: "lastPrice", sort_link: true },
    ];

    mounted(): void {
        // Call the HTTP API to fetch company list in json format
        fetch('api/StockData/TodayReport')
            .then(response => response.json() as Promise<StockPrice[]>)
            .then(data => this.stockPrices = this.displayItem = data)
            .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
    }

    // Callback function on search
    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("ser:") == 0) this.displayItem = this.stockPrices.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sym:") == 0) this.displayItem = this.stockPrices.filter(x => x.symbol.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sec:") == 0) this.displayItem = this.stockPrices.filter(x => x.industry.toLowerCase().indexOf(searchParam) >= 0);
        else this.displayItem = this.stockPrices.filter(x => (x.symbol.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
    }

    //Callback function to sort the list
    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            case "symbol":
            case "series":
            case "industry":
                this.displayItem = this.displayItem.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
            case "change":
            case "lastPrice":
            this.displayItem = this.displayItem.sort((left, right): number => (left[sortKey] - right[sortKey]) * this.sortReverse);
        }
    }
}
