import {
  Component,
  OnInit,
} from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { Observable } from 'rxjs/Observable';

import { UserService } from '../user.service';
/**
 * We're loading this component asynchronously
 * We are using some magic with es6-promise-loader that will wrap the module with a Promise
 * see https://github.com/gdi2290/es6-promise-loader for more info
 */

@Component({  
    templateUrl: 'reset.component.html',
    providers: [UserService],
})
export class ResetComponent implements OnInit {
    id: string;
    password: string;
    message: string;
    email: string
    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private userService: UserService) {
    }
  public ngOnInit() {
      this.route
          .params
          .subscribe(params => {
              this.id = params['id'];
          });

  }

  public reset() {
      this.userService.resetPassword(this.id, this.email, this.password).subscribe(data => {
          if (data.json().valid == false) {
              this.message = "invalid info";
          } else {
              this.message = 'password reset';
          }
      }, error => {
          
          this.message = error.statusText;
      });
  }

}
