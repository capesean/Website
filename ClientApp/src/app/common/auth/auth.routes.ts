import { Route } from '@angular/router';
import { AuthComponent } from './auth.component';
import { LoginComponent } from './login.component';
import { ResetPasswordComponent } from './resetpassword.component';
import { ResetComponent } from './reset.component';

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
            path: 'resetpassword',
            component: ResetPasswordComponent
         },
         {
            path: 'reset',
            component: ResetComponent
         },
         {
            path: '**',
            redirectTo: 'login'
         }
      ]
   }
];

