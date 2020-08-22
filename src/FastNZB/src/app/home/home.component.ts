import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import { AppState } from '../app.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from './title';
import { User } from '../user.component';
import { XLargeDirective } from './x-large';
import { UserService } from '../user.service';
import { Observable } from 'rxjs/Observable';
import { NgZone } from '@angular/core';

@Component({
  /**
   * The selector is what angular internally uses
   * for `document.querySelectorAll(selector)` in our index.html
   * where, in this case, selector is the string 'home'.
   */
  selector: 'home',  // <home></home>
  /**
   * We need to tell Angular's Dependency Injection which providers are in our app.
   */
  providers: [
    Title,
    UserService
  ],
  /**
   * Our list of styles in our component. We may add more to compose many styles together.
   */
  styleUrls: [ './home.component.css' ],
  /**
   * Every Angular template is first compiled by the browser before Angular runs it's compiler.
   */
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  /**
   * Set our default values
   */  
  public localState = { value: '' };
  model: any = {};
  loading = false;
  currentUser: Observable<User> = Observable.of<User>();
  loginError: string;
  registerMessage: string;
  donateMessage: string;
  /**
   * TypeScript public modifiers
   */
  constructor(
    public appState: AppState,
    public title: Title,
            private router: Router,
            public authenticationService: UserService            
  ) {
     
  }

  public ngOnInit() {
    
      this.authenticationService.session();
  }
  
  login() {
    this.loading = true;
    this.authenticationService.login(this.model.UserName, this.model.Password)
        .subscribe(
            data => {
                //this.router.navigate([this.returnUrl]);
                window.location.reload();
            },
            error => {
                let message = JSON.parse(error._body).responseStatus.message;
                //this.alertService.error(error);
                console.log(error);
                this.loginError  = message;
                this.loading = false;
            });
    }


  register() {
    this.loading = true;
    this.authenticationService.create(this.model)
        .subscribe(
            data => {
                this.login();                
            },
            error => {
                let message = JSON.parse(error._body).responseStatus.message;
                this.registerMessage = message;
                this.loading = false;
            });
    }

    logout() {
      this.authenticationService.logout().subscribe(data=> {
        window.location.reload();
        }
      );      
    }

    public reset() {
        this.authenticationService.reset(this.model.UserName).subscribe(data => {

              this.loginError = 'Check your Email';
            
        }, error => {

            this.loginError = error.statusText;
        });
    }
}
