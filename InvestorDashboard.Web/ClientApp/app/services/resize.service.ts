import { Injectable } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable()
export class ResizeService {
    public width: number;
    get isMobile(): boolean {
        if (this.width <= 420) {
            return true;
        } else {
            return false;
        }
    }
    get isTab(): boolean {
        if (this.width < 1025) {
            return true;
        } else {
            return false;
        }
    }
    constructor() { }
}
