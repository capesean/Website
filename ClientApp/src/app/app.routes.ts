import { Route, } from '@angular/router';
import { MainComponent } from './main.component';

export const AppRoutes: Route[] = [
    {
        path: 'auth',
        loadChildren: () => import('./common/auth/auth.module').then(m => m.AuthModule)
    },
    {
        path: '',
        component: MainComponent,
        data: {},
        children: [
            {
                path: '',
                loadChildren: () => import('./custom.module').then(m => m.CustomModule)
            },
            {
                path: '',
                loadChildren: () => import('./generated.module').then(m => m.GeneratedModule)
            },
            { path: '404', component: NotFoundComponent },
            { path: '**', redirectTo: '/404' }
        ]
    }
];
