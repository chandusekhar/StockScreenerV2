import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component
export default class homeComponent extends Vue {
    flag: boolean = false;
    count: number = 0;
    onClick(): void {
        this.flag = true;
    }
}