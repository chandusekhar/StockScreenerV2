import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface CompanyInfo {
    companyName: string;
    symbol: string;
    series: string;
    industry: string;
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
}

let fetchedData: CompanyInfo[] = [];

@Component
export default class companyListComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sym:<symbol>,sec:<sector>,ser:<series>,default companyName";
    page_header: string = "Company List in NSE";
    // Update the status in statusMessage
    statusMessage: string = "Fetching list of companies from server";

    // Component specific code
    displayItem: CompanyInfo[] = [];

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Company Name", data_field_name: "companyName", sort_link: true },
        { header_field_name: "Symbol", data_field_name: "symbol", sort_link: true },
        { header_field_name: "Series", data_field_name: "series", sort_link: false },
        { header_field_name: "Sector", data_field_name: "industry", sort_link: true }
    ];

    mounted(): void {
        this.displayItem = fetchedData;
        if(fetchedData.length == 0) {
            // Call the HTTP API to fetch company list in json format
            fetch('api/StockData/CompanyList')
                .then(response => response.json() as Promise<CompanyInfo[]>)
                .then(data => { fetchedData = this.displayItem = data; this.statusMessage = ""; })
                .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
        }
    }

    // Callback function on search
    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("ser:") == 0) this.displayItem = fetchedData.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sym:") == 0) this.displayItem = fetchedData.filter(x => x.symbol.toLowerCase().indexOf(searchParam) >= 0);
        else if (query.indexOf("sec:") == 0) this.displayItem = fetchedData.filter(x => x.industry.toLowerCase().indexOf(searchParam) >= 0);
        else this.displayItem = fetchedData.filter(x => (x.companyName.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
    }

    //Callback function to sort the list
    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            case "companyName":
            case "symbol":
            case "series":
            case "industry":
                this.displayItem = this.displayItem.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
        }
    }
}
