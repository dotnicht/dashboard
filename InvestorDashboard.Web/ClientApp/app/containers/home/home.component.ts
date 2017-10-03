import { Component, OnInit, Inject } from '@angular/core';


@Component({
    selector: 'app-home',
    templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {

    title: string = 'Welcome to Investores.... some tool';

    // Use "constructor"s only for dependency injection
    constructor(
    ) { }

    // Here you want to handle anything with @Input()'s @Output()'s
    // Data retrieval / etc - this is when the Component is "ready" and wired up
    ngOnInit() { }

    public setLanguage(lang) {
       
    }
}
