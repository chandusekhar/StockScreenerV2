import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface CompanyInfo
{
    companyName: string;
    symbol: string;
    series: string;
    industry: string;
    ltp: number;
}

@Component
export default class companyListComponent extends Vue {
    companyList: CompanyInfo[] = [];
    statusMessage: string = "Fetching list of companies from server";
    async mounted() {

        fetch('api/StockData/CompanyList')
            .then(response => response.json() as Promise<CompanyInfo[]>)
            .then(data => this.companyList = data)
            .catch(reason => this.statusMessage = "API 'StockData/CompanyList' failed with error \"" + reason + "\"");
    }
}
