import { Injectable } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { IClientInfo } from '../models/client-info.model';

@Injectable()
export class ClientInfoService {
    public clientInfo: IClientInfo;

    constructor() { 
        this.Initialize();
    }
    private Initialize() {
        this.clientInfo = { balance: 40.28 } as IClientInfo;
    }
}
