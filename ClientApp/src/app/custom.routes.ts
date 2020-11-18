import { Route } from '@angular/router';
import { AccessGuard } from './common/auth/accessguard';
import { HomeComponent } from './home/home.component';
import { ChangePasswordComponent } from './users/changepassword.component';
import { ErrorComponent } from './common/error/error.component';
import { ErrorsComponent } from './common/error/errors.component';
import { SettingsComponent } from './settings/settings.component';

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
    },
    {
        path: 'settings',
        canActivate: [AccessGuard],
        canActivateChild: [AccessGuard],
        component: SettingsComponent,
        pathMatch: 'full',
        data: { breadcrumb: 'Settings' },
    },
    {
        path: 'errors',
        canActivate: [AccessGuard],
        canActivateChild: [AccessGuard],
        component: ErrorsComponent,
        data: { breadcrumb: 'Errors' },
        children: [
            {
                path: ':id',
                component: ErrorComponent,
                canActivate: [AccessGuard],
                canActivateChild: [AccessGuard],
                data: {
                    breadcrumb: 'Error'
                }
            }
        ]
    }
];
