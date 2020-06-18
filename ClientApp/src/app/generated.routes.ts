import { Route } from '@angular/router';
import { AccessGuard } from './common/auth/accessguard';
import { UserListComponent } from './users/user.list.component';
import { UserEditComponent } from './users/user.edit.component';

export const GeneratedRoutes: Route[] = [
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
];
