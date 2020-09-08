import { CanActivateChild, ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable()
export class AccessGuard implements CanActivateChild, CanActivate {

    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
        // todo: if can't active, then go to login?
        return this.checkParents(childRoute, state);
    }

    canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
        // todo: if can't active, then go to login?
        return this.checkParents(next, state);
    }

    private checkParents(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
        //var requiresLogin: boolean = route.data.requiresLogin || false;

        //// check up through parents for login requirements
        //var parent = route.parent;
        //while (parent && !requiresLogin) {
        //   requiresLogin = parent.data.requiresLogin || false;
        //   parent = parent.parent;
        //}

        const requiresLogin = true;

        // todo: send to login with returnUrl: https://www.tektutorialshub.com/angular/angular-canactivate-guard-example/
        if (requiresLogin) {
            return this.authService.loggedIn$
                .pipe(
                    tap(loggedIn => {
                        if (!loggedIn) {
                            let url = state.url.startsWith("/auth") ? "" : state.url;
                            url = "/auth/login" + (url ? "?path=" + encodeURIComponent(url) : "");
                            this.router.navigate([url], {queryParamsHandling: "preserve"});
                        }
                    })
                );
        }

        return true;
    }

}
