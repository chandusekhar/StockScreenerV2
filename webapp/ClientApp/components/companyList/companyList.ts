import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface CompanyInfo
{
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

@Component
export default class companyListComponent extends Vue {
    searchQuery: string = "";
    sortReverse: number = -1;
    searchPlaceHolder: string = "sym:<symbol>,sec:<sector>,ser:<series>,default companyName";
    // Update the status in statusMessage
    statusMessage: string = "Fetching list of companies from server";

    // Component specific code
    companyList: CompanyInfo[] = [];
    displayItem: CompanyInfo[] = [];
    table_display_data: DisplayItems[] = [
                        { header_field_name: "Company Name", data_field_name: "companyName", sort_link: true },
                        { header_field_name: "Symbol", data_field_name: "symbol",sort_link: true },
                        { header_field_name: "Series", data_field_name: "series", sort_link: false },
                        { header_field_name: "Sector", data_field_name: "industry", sort_link: true }
                    ];



    mounted():void {
        fetch('api/StockData/CompanyList')
            .then(response => response.json() as Promise<CompanyInfo[]>)
            .then(data => this.companyList = this.displayItem = data)
            .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
    }

    onSearch(): void {
        let query: string = this.searchQuery.toLowerCase();
        let searchParam: string = query.substr(4, query.length);

        if(query.indexOf("ser:") == 0) this.displayItem = this.companyList.filter(x => x.series.toLowerCase().indexOf(searchParam) >= 0);
        else if(query.indexOf("sym:") == 0) this.displayItem = this.companyList.filter(x => x.symbol.toLowerCase().indexOf(searchParam) >= 0);
        else if(query.indexOf("sec:") == 0) this.displayItem = this.companyList.filter(x => x.industry.toLowerCase().indexOf(searchParam) >= 0);
        else this.displayItem = this.companyList.filter(x => (x.companyName.toLowerCase().indexOf(this.searchQuery.toLowerCase()) >= 0));
    }

    sortBy(sortKey: string): void {
        this.sortReverse *= -1;
        switch(sortKey)
        {
            case "companyName":
            case "symbol":
            case "series":
            case "industry":
                this.displayItem = this.companyList.sort((left, right): number => left[sortKey].localeCompare(right[sortKey]) * this.sortReverse);
                break;
        }
    }
}
