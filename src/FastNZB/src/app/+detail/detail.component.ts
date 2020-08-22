import {
  Component,
  OnInit,
} from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { Observable }        from 'rxjs/Observable';

import { NZB } from '../nzb.component';
import { ReleaseSearchService } from '../release-search.service';
/**
 * We're loading this component asynchronously
 * We are using some magic with es6-promise-loader that will wrap the module with a Promise
 * see https://github.com/gdi2290/es6-promise-loader for more info
 */

@Component({
  selector: 'detail',
  templateUrl: 'detail.component.html',
  /*template: `
    <h1>Hello from Detail</h1>
    <span>
      <a [routerLink]=" ['./child-detail'] ">
        Child Detail
      </a>
    </span>
    <router-outlet></router-outlet>
  `,*/
  providers: [ReleaseSearchService],
})
export class DetailComponent implements OnInit {
  nzb: NZB;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private releaseSearchService: ReleaseSearchService) {     
  }

  public ngOnInit() {
    //console.log('hello `Detail` component');
    this.nzb = new NZB();
    this.route
        .params
        .subscribe(params => {            
            this.releaseSearchService.getNZB(params['id'])
            .subscribe(data=> { this.nzb = data; });
    });
    
  }

  upVote(nzb: NZB): void {
    this.releaseSearchService.vote(nzb.id, 1).subscribe(function(result) {
      nzb._Votes = result;
    });
  }

  downVote(nzb: NZB): void {
    this.releaseSearchService.vote(nzb.id, -1).subscribe(function(result) {
        nzb._Votes = result;
    });
  }

}
