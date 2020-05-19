import { Component, OnInit } from '@angular/core';
import { Spinkit } from 'ng-http-loader';
import { Observable } from 'rxjs';
import { AuthStateModel } from './common/auth/auth.models';
import { AuthService } from './common/auth/auth.service';
import { Title } from '@angular/platform-browser';
import { environment } from '../environments/environment';
import * as moment from 'moment';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

    spinkit = Spinkit.skChasingDots;
    authState$: Observable<AuthStateModel>;

    constructor(
        private authService: AuthService,
        private titleService: Title
    ) {
        titleService.setTitle(environment.siteName);
        //const locale = window.navigator.language;
        //if (locale) moment.locale(locale);
        moment.locale("en-gb");
    }

    ngOnInit(): void {

        this.authState$ = this.authService.state$;
        // start the auth service (so tokens can be automatically refreshed)
        this.authService
            .init()
            .subscribe();
    }
}

