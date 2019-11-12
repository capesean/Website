import { Route } from '@angular/router';
import { AccessGuard } from './common/auth/accessguard';
import { NotFoundComponent } from './common/notfound.component';
//import { XYZComponent } from './xyz/xyz.component';

export const CustomRoutes: Route[] = [
   //{
   //   path: 'XYZ',
   //   canActivate: [AccessGuard],
   //   canActivateChild: [AccessGuard],
   //   component: XYZComponent,
   //   data: { breadcrumb: 'XYZ' }
   //},
   {
      path: "**",
      component: NotFoundComponent
   }
];
