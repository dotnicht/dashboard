import { Injectable } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable()
export class OtherService {
    showMainComponent: boolean;
    constructor() {
        this.showMainComponent = true;
    }
}
