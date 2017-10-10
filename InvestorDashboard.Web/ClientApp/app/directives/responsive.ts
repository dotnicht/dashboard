import { Directive, OnInit, Renderer, ElementRef, HostListener, TemplateRef, ViewContainerRef, Input, NgZone } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ResizeService } from '../services/resize.service';

@Directive({
    selector: '[showFor]'
})
export class ResponsiveDirective {
    private hasView = true;
    private states: string[];

    constructor(private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef, private ngZone: NgZone,
        private resizeService: ResizeService) { }

    @Input() set showFor(value: string[]) {
        this.states = value;
        if (this.resizeService.width > 420) {
            if (this.states.includes('desktop')) {
                this.viewContainer.createEmbeddedView(this.templateRef);
                this.hasView = true;
            } else if (this.states.includes('mobile')) {
                this.viewContainer.clear();
                this.hasView = false;
            }
        } else {
            if (this.states.includes('mobile')) {
                this.viewContainer.createEmbeddedView(this.templateRef);
                this.hasView = true;
            } else if (this.states.includes('desktop')) {
                this.viewContainer.clear();
                this.hasView = false;
            }
        }
        // if (!condition && !this.hasView) {
        // this.viewContainer.createEmbeddedView(this.templateRef);
        // this.hasView = true;
        // } else if (condition && this.hasView) {
        //     this.viewContainer.clear();
        //     this.hasView = false;
        // }
    }
    checkState() {

    }
}