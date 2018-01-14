import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Moment from 'moment';

class CircuitBreaker {
    nseSymbol: string;
    series: string;
    high_low: string;
    date: Date;
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
}

let fetchedData: CircuitBreaker[] = [];

@Component
export default class companyListComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sym:<symbol>,sec:<sector>,ser:<series>,default companyName";
    page_header: string = "Today's Circuit Breaker";
    // Update the status in statusMessage
    statusMessage: string = "Fetching list of companies from server";

    // Component specific code
    displayItem: CircuitBreaker[] = [];

    constructor() {
        super();
        Vue.filter('formatDate', function(value:string)  {
            if (value) { return Moment(String(value)).format('DD/MM/YYYY'); }
        });
    }

    // List of columns and the respective data fields
    table_display_data: DisplayItems[] = [
        { header_field_name: "Symbol", data_field_name: "nseSymbol", sort_link: true },
        { header_field_name: "High/Low", data_field_name: "high_low", sort_link: true },
        { header_field_name: "Series", data_field_name: "series", sort_link: false },
        { header_field_name: "Date", data_field_name: "date", sort_link: false },
    ];

    mounted(): void {
        this.displayItem = fetchedData;
        if(fetchedData.length == 0) {
            // Call the HTTP API to fetch company list in json format
            fetch('api/StockData/TodayCircuitBreaker')
                .then(response => response.json() as Promise<CircuitBreaker[]>)
                .then(data => { fetchedData = this.displayItem = data; this.statusMessage = ""; })
                .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
        }
    }

    // Callback function on search
    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if (query.indexOf("ser:") == 0) this.displayItem = fetchedData.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else this.displayItem = fetchedData.filter(x => (x.nseSymbol.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
    }

    //Callback function to sort the list
    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            case "nseSymbol":
            case "series":
            case "high_low":
                this.displayItem = this.displayItem.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
        }
    }
}
