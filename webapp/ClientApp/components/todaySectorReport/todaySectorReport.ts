import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface StockPrice {
    symbol: string;
    series: string;
    industry: string;
    change: number;
    lastPrice: number;
}

interface SectorChange {
    sector: string;
    change: number;
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
    show_total: boolean;
    color_value: boolean;
}

@Component
export default class TodayStockComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sec:<sector>,ser:<series>,default symbol";
    page_header: string = "Today's Sector Report";
    // Update the status in statusMessage
    statusMessage: string = "Fetching Sector report from server";

    // Component specific code
    fetchedStockPrices: StockPrice[] = [];
    displayItem: StockPrice[] = [];

    fetchedSectorChange: SectorChange[] = [];
    displayItemSectorChange: SectorChange[] = [];

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Symbol", data_field_name: "symbol", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Sector", data_field_name: "industry", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Change", data_field_name: "change", sort_link: true, show_total: true, color_value: true },
        { header_field_name: "Last Price", data_field_name: "lastPrice", sort_link: true, show_total: false, color_value: false }
    ];

    table_sector_display_data: DisplayItems[] = [
        { header_field_name: "Sector", data_field_name: "sector", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Change", data_field_name: "change", sort_link: true, show_total: false, color_value: true },
    ];

    mounted(): void {
        // Call the HTTP API to fetch company list in json format
        fetch('api/StockData/TodayStockReport')
            .then(response => response.json() as Promise<StockPrice[]>)
            .then(data => { this.fetchedStockPrices = this.displayItem = data; this.statusMessage = "";})
            .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");

        fetch('api/StockData/TodaySectorChange')
            .then(response => response.json() as Promise<SectorChange[]>)
            .then(data => this.fetchedSectorChange = this.displayItemSectorChange = data)
            .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
    }

    // Callback function on search
    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("ser:") == 0) this.displayItem = this.fetchedStockPrices.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sec:") == 0) this.displayItem = this.fetchedStockPrices.filter(x => x.industry.toLowerCase().indexOf(searchParam) >= 0);
        else this.displayItem = this.fetchedStockPrices.filter(x => (x.symbol.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
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
                break;
        }
    }

    LeftPanelsortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            case "sector":
                this.displayItemSectorChange = this.displayItemSectorChange.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
            case "change":
                this.displayItemSectorChange = this.displayItemSectorChange.sort((left, right): number => (left[sortKey] - right[sortKey]) * this.sortReverse);
                break;
        }
    }

    onClick(key: string): void {
        this.searchQuery = "sec:" + key;
        this.onSearch();
    }
}
