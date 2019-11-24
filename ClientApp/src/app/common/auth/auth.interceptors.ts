import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap, first } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable()
export class AuthoriseRequestInterceptor implements HttpInterceptor {

   constructor(private authService: AuthService) { }

   intercept(req: HttpRequest<any>,
      next: HttpHandler): Observable<HttpEvent<any>> {

      if (req.url.endsWith("connect/token"))
         return next.handle(req);

      return this.authService.tokens$
         .pipe(first())
         .pipe(
            switchMap(token => {
               if (token) {
                  const cloned = req.clone({
                     headers: req.headers.set("Authorization", `Bearer ${token.access_token}`)
                  });
                  return next.handle(cloned);
               } else {
                  return next.handle(req);
               }

            })
         );
   }
}

@Injectable()
export class UnauthorisedResponseInterceptor implements HttpInterceptor {

   constructor(
      private router: Router,
      private authService: AuthService
   ) { }

   intercept(request: HttpRequest<any>, next: HttpHandler): Observable<any> {

      return next.handle(request)
         .pipe(
            catchError(
               (err) => {
                  if (err.status === 401) {
                     this.router.navigate(['/auth/login']);
                  }
                  else if (err.status === 403) {
                     this.router.navigate(['/']);
                  } else {
                     //const error = err.message || err.statusText;
                     return throwError(err);
                  }
               }
            )
         );
   }
}
