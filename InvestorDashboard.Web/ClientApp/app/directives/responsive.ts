import { Directive, OnInit, Renderer, ElementRef, HostListener, TemplateRef, ViewContainerRef, Input } from '@angular/core';

@Directive({
    selector: '[isMobile], [isDesktop]'
})
export class ResponsiveDirective {
    private hasView = false;
    get size() {
        return window.innerWidth;
    }
    constructor(private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef) { }


    @HostListener('window:resize', ['$event'])
    onResize() {
        let a = window.innerWidth;
        console.log(a);
    }

    @Input() isMobile() {
        // if (!condition && !this.hasView) {
        //     this.viewContainer.createEmbeddedView(this.templateRef);
        //     this.hasView = true;
        // } else if (condition && this.hasView) {
        //     this.viewContainer.clear();
        //     this.hasView = false;
        // }
    }
    checkState() {

    }
}