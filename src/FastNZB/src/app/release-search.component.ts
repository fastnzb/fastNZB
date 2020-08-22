import { Component, OnInit } from '@angular/core';
import { NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import 'rxjs/Rx';
import { Angulartics2, Angulartics2Piwik } from 'angulartics2';
import { Location } from '@angular/common';

import { Release } from './release.component';
import { NZB } from './nzb.component';
import { ReleaseSearchService } from './release-search.service';
import { User } from './user.component';
import { UserService } from './user.service';

@Component({
    selector: 'release-search',
    templateUrl: 'release-search.component.html',
    providers: [ReleaseSearchService, Angulartics2Piwik]
})

export class ReleaseSearchComponent implements OnInit {
    showResults: boolean = false;
    loading: boolean = false;
    currentUser: User = null;
    nl: boolean = false;
    self: Object = this;
    searchTerm: string = '';
    releaseList: Observable<Array<Release>>;
    releases: Release[];
    take: number = 50;
    offset: number = 0;
    private searchTerms = new Subject<string>();
    constructor(private router: Router) {
    }
    // Push a search term into the observable stream.
    search(term: string): void {                
        if (term) {
            this.router.navigateByUrl('/search/' + term);
        } else  {
            this.router.navigateByUrl('/search');
        }
    }
    ngOnInit(): void {
    }
}

