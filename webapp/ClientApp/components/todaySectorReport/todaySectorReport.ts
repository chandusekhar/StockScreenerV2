import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface StockPrice {
    symbol: string;
    series: string;
    industry: string;
    change: number;
    lastPrice: number;
    qty: number;
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
    volumeChange: number;
    volume: number;
    priceChange5d: number;
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

let fetchedStockPrices: StockStats[] = [];
let fetchedSectorChange: SectorChange[] = [];

@Component
export default class TodayStockComponent extends Vue {
    searchQuery: string = "";
    searchQuerySector: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sec:<sector>,ser:<series>,default symbol";
    page_header: string = "Today's Sector Report";
    // Update the status in statusMessage
    statusMessage: string = "Fetching Sector report from server";

    // Component specific code
    displayItem: StockStats[] = [];
    displayItemSectorChange: SectorChange[] = [];

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Symbol", data_field_name: "symbol", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Last Price", data_field_name: "ltp", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Change", data_field_name: "priceChange", sort_link: true, show_total: true, color_value: true },
        { header_field_name: "AvgChange5d", data_field_name: "priceChange5d", sort_link: true, show_total: true, color_value: true },
        { header_field_name: "Volume", data_field_name: "volume", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Volume/5dVolume", data_field_name: "volumeChange", sort_link: true, show_total: false, color_value: true },
    ];

    table_sector_display_data: DisplayItems[] = [
        { header_field_name: "Sector", data_field_name: "sector", sort_link: true, show_total: false, color_value: false },
        { header_field_name: "Change", data_field_name: "change", sort_link: true, show_total: false, color_value: true },
    ];

    private updateFetchedData(x: StockStats) {
        x.priceChange = x.avgPriceChange[0];
        x.priceChange5d = x.avgPriceChange[1];
        x.volumeChange = 1;
        if(x.avgVolumeChage[1] != 0)
            x.volumeChange = Number(((x.avgVolumeChage[0] - x.avgVolumeChage[1])/x.avgVolumeChage[1]).toFixed(2));

        x.volume = x.avgVolumeChage[0];
    }

    mounted(): void {
        this.displayItemSectorChange = fetchedSectorChange;
        if(fetchedSectorChange.length == 0) {
            fetch('api/StockData/TodaySectorChange')
                .then(response => response.json() as Promise<SectorChange[]>)
                .then(data => fetchedSectorChange = this.displayItemSectorChange = data)
                .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
        }

        this.displayItem = fetchedStockPrices;
        if(fetchedStockPrices.length == 0) {
            // Call the HTTP API to fetch company list in json format
            fetch('api/StockData/TodayVolumeReport')
                .then(response => response.json() as Promise<StockStats[]>)
                .then(data => {
                    fetchedStockPrices = data;
                    fetchedStockPrices.forEach(x => this.updateFetchedData(x));
                    this.displayItem = fetchedStockPrices;
                    this.statusMessage = "";
                    this.onClick("Mining");
                })
                .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
        }
    }

    // Callback function on search
    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("ser:") == 0)  this.displayItem = fetchedStockPrices.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sec:") == 0) {
            this.displayItem = fetchedStockPrices.filter(x => x.sector.toLowerCase().indexOf(searchParam) >= 0)
                                                 .sort((a, b): number => b.priceChange-a.priceChange);
        }
        else {
            this.displayItem = fetchedStockPrices.filter(x => (x.symbol.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
        }
    }

    // Callback function on search in sector input box
    onSearchSector(): void {
        let query: string = this.searchQuerySector.toLowerCase();
        this.displayItemSectorChange = fetchedSectorChange.filter(x => (x.sector.toLowerCase().indexOf(query) >= 0));
    }

    //Callback function to sort the list
    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            //Sort string
            case "symbol":
            case "series":
                this.displayItem = this.displayItem.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
            //Sort numbers
            case "volume":
            case "priceChange":
            case "ltp":
            case "priceChange5d":
            case "volumeChange":
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
        this.page_header = "Today's Sector Report for '" + key + "'";
    }
}
