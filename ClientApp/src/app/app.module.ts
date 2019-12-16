import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { AppRoutes } from './app.routes';
import { NgHttpLoaderModule } from 'ng-http-loader';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule, NgbDateAdapter, NgbDateNativeAdapter } from '@ng-bootstrap/ng-bootstrap';
import { BreadcrumbService, BreadcrumbComponent } from 'angular-crumbs';
import { ErrorService } from './common/services/error.service';
import { NotFoundComponent } from './common/notfound.component';
import { AccessGuard } from './common/auth/accessguard';
import { AuthoriseRequestInterceptor, UnauthorisedResponseInterceptor } from './common/auth/auth.interceptors';
import { SharedModule } from './shared.module';
import { JsonDateInterceptor } from './common/services/interceptors';

@NgModule({
    declarations: [AppComponent, NotFoundComponent],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        HttpClientModule,
        RouterModule.forRoot(AppRoutes),
        ToastrModule.forRoot({
            "closeButton": true,
            "positionClass": "toast-bottom-right",
            "timeOut": 5000,
            "extendedTimeOut": 5000,
            progressBar: true
        }),
        NgHttpLoaderModule.forRoot(),
        BrowserAnimationsModule,
        FormsModule,
        ReactiveFormsModule,
        NgbModule,
        SharedModule
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthoriseRequestInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: UnauthorisedResponseInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: JsonDateInterceptor, multi: true },
        { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter },
        AccessGuard,
        BreadcrumbService,
        ErrorService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
