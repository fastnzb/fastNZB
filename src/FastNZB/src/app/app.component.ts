/**
 * Angular 2 decorators and services
 */
import {
  Component,
  OnInit,
  ViewEncapsulation
} from '@angular/core';
import { AppState } from './app.service';
import { Angulartics2, Angulartics2Piwik } from 'angulartics2';
/**
 * App Component
 * Top Level Component
 */
@Component({
  selector: 'app',
  encapsulation: ViewEncapsulation.None,
  styleUrls: [
    './app.style.css',
    '../../node_modules/bootstrap/dist/css/bootstrap.min.css'
  ],
  template: '<main><router-outlet></router-outlet></main>'
})
export class AppComponent implements OnInit {
  //public angularclassLogo = 'assets/img/angularclass-avatar.png';
  public name = 'FastNZB';
  public url = 'https://fastnzb.com';

  constructor(
      public appState: AppState,
      angulartics2Pwiik: Angulartics2Piwik
  ) {}

  public ngOnInit() {
    console.log('Initial App State', this.appState.state);
  }

}

/**
 * Please review the https://github.com/AngularClass/angular2-examples/ repo for
 * more angular app examples that you may copy/paste
 * (The examples may not be updated as quickly. Please open an issue on github for us to update it)
 * For help or questions please contact us at @AngularClass on twitter
 * or our chat on Slack at https://AngularClass.com/slack-join
 */
