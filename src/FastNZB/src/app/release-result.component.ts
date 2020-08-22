import { Component } from '@angular/core';
import { Release } from './release.component';
import { Observable }        from 'rxjs/Observable';
import 'rxjs/Rx';

@Component({
    selector: 'query',
    template: '<h1>My First Angular 2 App</h1>'
})

export class ReleaseResult {
    offset: number;
    total: number;
    results: Observable<Release[]>;    
}
