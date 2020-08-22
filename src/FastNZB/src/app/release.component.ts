import { Component } from '@angular/core';
import { NZB } from './nzb.component';
import { Observable } from 'rxjs';

@Component({
    selector: 'releases',
    template: '<h1>My First Angular 2 App</h1>'
})

export class Release {
    id: number;
    title: string;
    totalpart: number;
    group_id: number;
    size: number;
    guid: string;
    nzb_guid: string;
    nzbs: NZB[];
    open: boolean;
    loading: boolean;
}
