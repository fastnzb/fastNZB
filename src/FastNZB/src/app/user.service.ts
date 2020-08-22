import { Injectable, OnInit } from '@angular/core';
import { Http, Headers, Response } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map'

import { User } from './user.component';

@Injectable()
export class UserService implements OnInit {
    currentUser: User = null;
    sessionChecked: boolean = false;
    mean: number = 0;
    message: string = "";
    constructor(private http: Http, private location: Location, private router: Router) { }

    ngOnInit(): void {
        this.session();
    }

    login(username: string, password: string) {
        return this.http.post('/auth/credentials?format=json', { UserName: username, Password: password})
            .map((response: Response) => {
                // login successful if there's a jwt token in the response
                let user = response.json();
                if (user.sessionId) {                    
                    // store user details and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('currentUser', JSON.stringify(user));
                    
                }
            });
    }

    create(user: any) {
        return this.http.post('/api/register?format=json', user).map((response: Response) => response.json());
    }

    logout() {        
        // remove user from local storage to log user out
        localStorage.removeItem('currentUser');
        return this.http.get('/auth/logout?format=json');
    }

    session(): void {
        if (!this.currentUser) {
            var request = this.http.get('/api/session-info?format=json')
            .map((r: Response) => r.json() as User);          
            request.subscribe(q => { this.currentUser = q; this.sessionChecked = true; }, error => { 
                this.sessionChecked = true;
                if (this.location.path() != '' && this.location.path()!='/') {
                    this.router.navigateByUrl('/');
                }
                
             });
        }
    }

    reset(email: string) {
        return this.http.post('/api/users/passwordreset?format=json', { Email: email });
    }

    resetPassword(id: string, email: string,  password: string) {
        return this.http.put('/api/users/passwordreset/'+id+'?format=json', { Email: email, NewPassword: password });
    }

}