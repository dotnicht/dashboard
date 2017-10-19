import { Directive, HostListener, ElementRef, Input } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
    selector: '[case]'
})
export class CaseFormatterDirective {

    constructor(private el: ElementRef) { }
    @HostListener('keyup') onKeyUp() {
        let nativeEl = this.el.nativeElement;
        if (nativeEl.getAttribute('case') == 'uppercase') {
            this.el.nativeElement.value = this.el.nativeElement.value.toUpperCase();
        }
        if (nativeEl.getAttribute('case') == 'lowercase') {
            this.el.nativeElement.value = this.el.nativeElement.value.toLowerCase();
        }
    }
}