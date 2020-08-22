import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import 'rxjs/Rx';
import { Angulartics2, Angulartics2Piwik } from 'angulartics2';

import { Release } from '../release.component';
import { NZB } from '../nzb.component';
import { ReleaseSearchService } from '../release-search.service';
import { User } from '../user.component';
import { UserService } from '../user.service';
import { Location } from '@angular/common';

@Component({
  selector: 'search',
  templateUrl: './search.component.html',
  providers: [   
    UserService, ReleaseSearchService, Angulartics2Piwik
  ]
})
export class SearchComponent implements OnInit, OnDestroy {  
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
    donateMessage: String;
  constructor(
    public route: ActivatedRoute,
    public router: Router,
     public authenticationService: UserService,
     private releaseSearchService: ReleaseSearchService, private angulartics2: Angulartics2, private location: Location     
  ) {    
  }

  public ngOnInit() {
    this.authenticationService.session();
    this.releaseList = this.searchTerms
        .debounceTime(300)        // wait for 300ms pause in events
        //.distinctUntilChanged()   // ignore if next search term is same as previous
        .switchMap(term => term   // switch to new observable each time
            // return the http search observable
            ? this.releaseSearchService.search(term, this.take, this.offset).finally(function () { this.loading = false; }.bind(this))
            // or the observable of empty heroes if no search term
            : Observable.of<Array<Release>>([]))
        .catch(error => {
            // TODO: real error handling
            if (error.status == 401)
              this.router.navigateByUrl('/');
            console.log(error);
            this.loading = false;
            return Observable.of<Array<Release>>([]);
        });

    this.releaseList.subscribe((releases) => {
        this.releases = new Array<Release>();
        releases.forEach(element => {
            this.releases.push(element);
        });

    });
    
    this.currentUser = this.authenticationService.currentUser;
    this.route
        .params
        .subscribe(params => {      
          if (params['val']) {
            this.search(params['val']);   
            this.searchTerm = params['val'];
          }
           
    });
  }
  
      // Push a search term into the observable stream.
    search(term: string): void {    
        if (term) {
        this.angulartics2.eventTrack.next({ action: 'search', properties: {} });
        this.router.navigateByUrl('/search/' + term);
        this.loading = true;
        } else {
          this.router.navigateByUrl('/search');
        }
        this.offset = 0;
        this.releases = new Array<Release>();
        this.searchTerms.next(term);
        this.showResults = true;
        this.searchTerm = term;
    }
    showNZBs(release: Release): void {
        if (release.open) {
            release.open = false;
            return;
        } else if (release.nzbs) {
            release.open = true;
            return;
        }
        release.loading = true;
        this.releaseSearchService.getNZBs(release.id).subscribe(function (nzbs) { release.nzbs = nzbs; release.open = true; release.loading = false; });
        this.angulartics2.eventTrack.next({ action: 'title', properties: {} });
    }
    onScroll(): void {
        this.offset = this.offset + this.take;
        this.releaseSearchService.search(this.searchTerm, this.take, this.offset).subscribe((releases) => {
            releases.forEach(element => {
                this.releases.push(element);
            });
        });
    }

    upVote(nzb: NZB): void {
        this.releaseSearchService.vote(nzb.id, 1).subscribe(function (result) {
            nzb._Votes = result;
        });
        this.angulartics2.eventTrack.next({ action: 'vote', properties: {} });
    }

    downVote(nzb: NZB): void {
        this.releaseSearchService.vote(nzb.id, -1).subscribe(function (result) {
            nzb._Votes = result;
        });
        this.angulartics2.eventTrack.next({ action: 'vote', properties: {} });
    }

    logout() {
      this.authenticationService.logout().subscribe(data=> {
        window.location.reload();
        }
      );      
    }

    public ngOnDestroy() {
      window['angularComponent'] = null;
    }
}
