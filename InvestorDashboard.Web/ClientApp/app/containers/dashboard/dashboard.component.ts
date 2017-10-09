import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})
/** dashboard component*/
export class DashboardComponent implements OnInit
{
    /** dashboard ctor */
    constructor() { }

    /** Called by Angular after dashboard component initialized */
    ngOnInit(): void { }
}