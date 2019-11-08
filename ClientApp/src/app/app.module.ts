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
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { BreadcrumbService } from 'angular-crumbs';
import { ErrorService } from './common/services/error.service';
import { MainComponent } from './main.component';
import { NotFoundComponent } from './common/notfound.component';
import { HomeComponent } from './home/home.component';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { AccessGuard } from './common/auth/accessguard';
import { NavMenuComponent } from './common/nav-menu/nav-menu.component';
import { AuthoriseRequestInterceptor, UnauthorisedResponseInterceptor } from './common/auth/auth.interceptors';
import { GeneratedModule } from './generated.module';

@NgModule({
   declarations: [AppComponent, NavMenuComponent, MainComponent, HomeComponent, NotFoundComponent],
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
      BreadcrumbModule,
      FormsModule,
      ReactiveFormsModule,
      NgbModule,
      GeneratedModule
   ],
   providers: [
      { provide: HTTP_INTERCEPTORS, useClass: AuthoriseRequestInterceptor, multi: true },
      { provide: HTTP_INTERCEPTORS, useClass: UnauthorisedResponseInterceptor, multi: true },
      AccessGuard,
      BreadcrumbService,
      ErrorService
   ],
   bootstrap: [AppComponent]
})
export class AppModule { }
