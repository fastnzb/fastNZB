import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';

import { Release } from './release.component';
import { NZB } from './nzb.component';

//import 'rxjs/add/operator/toPromise';

@Injectable()
export class ReleaseSearchService {
  private apiUrl = 'api.php?search=';  // URL to web api

  constructor(private http: Http) {}


  search(term: string, take: number, offset: number): Observable<Release[]> {
    return this.http
               .get(`/api/query/titles?NameQuery=${term}&Take=${take}&Skip=${offset}&format=json`)
               .map((r: Response) => r.json().results as Release[]);
  }

  getNZBs(id: number): Observable<NZB[]> {
    return this.http
               .get(`/api/title/${id}?format=json`)
               .map((r: Response) => r.json() as NZB[]);
  }


  getNZB(id: string): Observable<NZB> {
    return this.http
               .get(`/api/nzb/detail/${id}?format=json`)
               .map((r: Response) => r.json() as NZB);
  }

  vote(id: number, val: number): Observable<number> {
    return this.http
      .post(`/api/vote/${id}?format=json`, { val: val })
      .map((r: Response) => r.json() as number);;    
  }

  private handleError(error: any): Promise<any> {
    //console.error('An error occurred', error); // for demo purposes only
    return Promise.reject(error.message || error);
  }

}