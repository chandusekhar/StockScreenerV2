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
}

interface DisplayItems {
    header_field_name: string;
    data_field_name: string;
    sort_link: boolean;
    color_value: boolean;
}

@Component
export default class homeComponent extends Vue {
    flag: boolean = false;
    count: number = 0;

    constructor() {
        super();
        Vue.filter('formatDate', function(value:string)  {
            if (value) { return Moment(String(value)).format('DD/MM/YYYY'); }
        });
    }

    // Component specific code
    displayItem: StockHistory[] = [];

    table_display_data: DisplayItems[] = [
        { header_field_name: "date", data_field_name: "date", sort_link: false, color_value: false },
        { header_field_name: "ltp", data_field_name: "ltp", sort_link: false, color_value: false },
        { header_field_name: "change", data_field_name: "change", sort_link: false, color_value: true },
        { header_field_name: "totalTrades", data_field_name: "totalTrades", sort_link: false, color_value: false },
        { header_field_name: "deliverableQty", data_field_name: "deliverableQty", sort_link: false, color_value: false },
        { header_field_name: "deliveryPercentage", data_field_name: "deliveryPercentage", sort_link: false, color_value: false }
    ];

    onClick(): void {
        this.flag = true;
        // Call the HTTP API to fetch company list in json format
        fetch('api/StockData/GetHistory?symbol=TVSMOTOR')
            .then(response => response.json() as Promise<StockHistory[]>)
            .then(data => {
                this.displayItem = data;
                this.displayItem.forEach(x => x.date =  Moment(String(x.date)).format('DD/MM/YYYY'));
            })
            .catch(reason => alert("Failed due to" + reason));

    }
}