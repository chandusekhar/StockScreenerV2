import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Moment from 'moment';

class CircuitBreaker {
    nseSymbol: string;
    count_h: number;
    count_l: number;
}

let fetchedData: CircuitBreaker[] = [];

@Component
export default class companyListComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sym:<symbol>,sec:<sector>,ser:<series>,default companyName";
    page_header: string = "Circuit Breaker for today and last 5 days";
    // Update the status in statusMessage
    statusMessage: string = "Fetching list of companies in circuit breaker";

    // Component specific code
    displayItem: CircuitBreaker[] = [];

    constructor() {
        super();
        Vue.filter('formatDate', function(value:string)  {
            if (value) { return Moment(String(value)).format('DD/MM/YYYY'); }
        });
    }

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

        this.displayItem = fetchedData.filter(x => (x.nseSymbol.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
    }

    //Callback function to sort the list
    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch (sortKey) {
            case "nseSymbol":
                this.displayItem = this.displayItem.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
            case "count_h":
                this.displayItem = this.displayItem.sort((left, right): number => ((left.count_h - right.count_h) || (right.count_l - left.count_l))  * this.sortReverse);
                break;
            case "count_l":
                this.displayItem = this.displayItem.sort((left, right): number => (left[sortKey] - right[sortKey]) * this.sortReverse);
                break;
        }
    }
}
