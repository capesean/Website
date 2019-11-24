import { Route } from '@angular/router';
import { AccessGuard } from './common/auth/accessguard';
import { HomeComponent } from './home/home.component';
import { ChangePasswordComponent } from './users/changepassword.component';

export const CustomRoutes: Route[] = [
   {
      path: '',
      canActivate: [AccessGuard],
      canActivateChild: [AccessGuard],
      component: HomeComponent,
      pathMatch: 'full',
      data: { breadcrumb: 'Home' },
   },
   {
      path: 'changepassword',
      canActivate: [AccessGuard],
      canActivateChild: [AccessGuard],
      component: ChangePasswordComponent,
      pathMatch: 'full',
      data: { breadcrumb: 'Change Password' },
   }
];
