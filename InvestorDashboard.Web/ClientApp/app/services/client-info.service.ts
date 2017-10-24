import { Injectable } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ClientInfo } from '../models/client-info.model';

@Injectable()
export class ClientInfoEndpointService {
    public clientInfo: ClientInfo;

    constructor() { 
        this.Initialize();
    }
    private Initialize() {
        this.clientInfo = { balance: 40.28 } as ClientInfo;
    }
}
