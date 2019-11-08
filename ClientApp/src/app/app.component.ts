import { Component, OnInit } from '@angular/core';
import { Spinkit } from 'ng-http-loader';
import { Observable } from 'rxjs';
import { AuthStateModel } from './common/auth/auth.models';
import { AuthService } from './common/auth/auth.service';

@Component({
   selector: 'app-root',
   templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

   spinkit = Spinkit.skChasingDots;
   authState$: Observable<AuthStateModel>;

   constructor(
      private authService: AuthService
   ) { }

   ngOnInit(): void {

      this.authState$ = this.authService.state$;
      // start the auth service (so tokens can be automatically refreshed)
      this.authService
         .init()
         .subscribe();
   }
}

