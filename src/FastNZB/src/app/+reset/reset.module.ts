import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AlertModule } from 'ngx-bootstrap/alert';

import { routes } from './reset.routes';
import { ResetComponent } from './reset.component';

@NgModule({
  declarations: [
    /**
     * Components / Directives/ Pipes
     */
    ResetComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes),
    AlertModule.forRoot(),
  ],
})
export class ResetModule {
  public static routes = routes;
}
