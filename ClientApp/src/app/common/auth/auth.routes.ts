import { Route } from '@angular/router';
import { AuthComponent } from './auth.component';
import { LoginComponent } from './login.component';

export const AuthRoutes: Route[] = [
   {
      path: '',
      component: AuthComponent,
      children: [
         {
            path: 'login',
            component: LoginComponent
         },
         {
            path: '**',
            redirectTo: 'login'
         }
      ]
   }
];

