import { Route, } from '@angular/router';
import { MainComponent } from './main.component';

export const AppRoutes: Route[] = [
   {
      path: 'auth',
      loadChildren: './common/auth/auth.module#AuthModule'
   },
   {
      path: '',
      component: MainComponent,
      data: {},
      loadChildren: './generated.module#GeneratedModule',
   }
];
