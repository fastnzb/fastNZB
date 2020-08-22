import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home';
import { SearchComponent } from './search';
import { NoContentComponent } from './no-content';

import { DataResolver } from './app.resolver';

export const ROUTES: Routes = [
  { path: '',      component: HomeComponent },
  { path: 'home',  component: HomeComponent },
  { path: 'search', component: SearchComponent },
  { path: 'search/:val', component: SearchComponent },
  { path: 'details/:id', loadChildren: './+detail#DetailModule'},
  { path: 'reset/:id', loadChildren: './+reset#ResetModule'},
  { path: '**',    component: NoContentComponent },
];
