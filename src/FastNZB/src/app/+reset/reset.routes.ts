import { ResetComponent } from './reset.component';

export const routes = [
  { path: '', children: [
    { path: '', component: ResetComponent },
  ]},
];
