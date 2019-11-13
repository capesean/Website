import { Route } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AccessGuard } from './common/auth/accessguard';
import { MainComponent } from './main.component';
import { CustomRoutes } from './custom.routes';
import { UserListComponent } from './users/user.list.component';
import { UserEditComponent } from './users/user.edit.component';

export const GeneratedRoutes: Route[] = [
   {
      path: '',
      component: MainComponent,
      data: {},
      children: (<Route[]>[
         {
            path: '',
            canActivate: [AccessGuard],
            canActivateChild: [AccessGuard],
            component: HomeComponent,
            pathMatch: 'full',
            data: { breadcrumb: 'Home' },
         },
         {
            path: 'users',
            canActivate: [AccessGuard],
            canActivateChild: [AccessGuard],
            component: UserListComponent,
            data: { breadcrumb: 'Users' },
            children: [
               {
                  path: ':id',
                  component: UserEditComponent,
                  canActivate: [AccessGuard],
                  canActivateChild: [AccessGuard],
                  data: {
                     breadcrumb: 'Add User'
                  }
               }
            ]
         }
      ]).concat(CustomRoutes)
   }
];
