import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';

import { Release } from './release.component';

//import 'rxjs/add/operator/toPromise';

@Injectable()
export class ReleaseService {
  
  constructor(private http: Http) {}

  search(term: string): Observable<Release[]> {
    return this.http
               .get(`/api/query/titles?NameContains=${term}&format=json&take=100`)
               .map((r: Response) => r.json() as Release[]);
  }

  private handleError(error: any): Promise<any> {
    //console.error('An error occurred', error); // for demo purposes only
    return Promise.reject(error.message || error);
  }

}