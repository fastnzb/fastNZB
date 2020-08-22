import { Component } from '@angular/core';

@Component({
    selector: 'nzb',
    template: '<h1>My First Angular 2 App</h1>'
})

export class NZB {
    id: number;
    name: string;
    parts: number;
    size: number;
    guid: string;   
    _Days: number;
    _Votes: number; 
}
