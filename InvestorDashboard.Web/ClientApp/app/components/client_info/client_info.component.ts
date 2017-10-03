import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-client_info',
    templateUrl: './client_info.component.html',
    styleUrls: ['./client_info.component.scss']
})

export class ClientInfoComponent implements OnInit {
    
    menuItems = [{
        title: '',
        imgName: '',
        route: ''
    }];

    constructor() { }

    ngOnInit(): void { }

}