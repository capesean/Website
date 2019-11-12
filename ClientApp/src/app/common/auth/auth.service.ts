import { environment } from './../../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { Observable, Subscription, BehaviorSubject, of, interval, throwError } from 'rxjs';
import { AuthStateModel, AuthTokenModel, ProfileModel, LoginModel, RefreshGrantModel, RegisterModel, ResetPasswordModel, ResetModel, ChangePasswordModel } from './auth.models';
import { filter, map, first, flatMap, catchError, tap, mergeMap } from 'rxjs/operators';
import { JwtHelperService } from "@auth0/angular-jwt";

const jwt = new JwtHelperService();

@Injectable({ providedIn: 'root' })
export class AuthService {

   private initalState: AuthStateModel = { profile: null, tokens: null, authReady: false };
   private authReady$ = new BehaviorSubject<boolean>(false);
   private state: BehaviorSubject<AuthStateModel>;
   private refreshSubscription$: Subscription;

   state$: Observable<AuthStateModel>;
   tokens$: Observable<AuthTokenModel>;
   profile$: Observable<ProfileModel>;
   loggedIn$: Observable<boolean>;

   constructor(
      private http: HttpClient,
   ) {
      this.state = new BehaviorSubject<AuthStateModel>(this.initalState);
      this.state$ = this.state.asObservable();

      this.tokens$ = this.state
         .pipe(filter(state => state.authReady))
         .pipe(map(state => state.tokens));

      this.profile$ = this.state
         .pipe(filter(state => state.authReady))
         .pipe(map(state => state.profile));

      this.loggedIn$ = this.tokens$
         .pipe(map(tokens => !!tokens));
   }

   init(): Observable<AuthTokenModel> {
      return this.startupTokenRefresh()
         .pipe(tap<AuthTokenModel>(() => this.scheduleRefresh()));
   }

   register(data: RegisterModel): Observable<Response> {
      return this.http.post<Response>(`${environment.baseApiUrl}accounts/register`, data);
   }

   resetPassword(data: ResetPasswordModel): Observable<void> {
      return this.http.post<void>(`${environment.baseApiUrl}accounts/resetpassword`, data);
   }

   reset(data: ResetModel): Observable<void> {
      return this.http.post<void>(`${environment.baseApiUrl}accounts/reset`, data);
   }

   login(user: LoginModel): Observable<any> {
      return this.getTokens(user, 'password')
         .pipe(catchError(res => throwError(res)))
         .pipe(tap(res => this.scheduleRefresh()));
   }

   logout(): void {
      this.updateState({ profile: null, tokens: null });
      if (this.refreshSubscription$) {
         this.refreshSubscription$.unsubscribe();
      }
      this.removeToken();
   }

   changePassword(changePassword: ChangePasswordModel): Observable<void> {
      return this.http.post<void>(`${environment.baseApiUrl}accounts/changepassword`, changePassword);
   }

   isInRole(profile: ProfileModel, role: string): boolean {
      if (!profile) return false;
      if (typeof (profile.role) === "string") return role === profile.role;
      return profile.role.indexOf(role) !== -1;
   }

   refreshTokens(): Observable<AuthTokenModel> {
      return this.state
         .pipe(first())
         .pipe(map(state => state.tokens))
         .pipe(mergeMap(tokens => this.getTokens({ refresh_token: tokens.refresh_token }, 'refresh_token')
            .pipe(catchError(error => throwError('Session Expired')))
         ));
   }

   private storeToken(tokens: AuthTokenModel): void {
      const previousTokens = this.retrieveTokens();
      if (previousTokens != null && tokens.refresh_token == null) {
         tokens.refresh_token = previousTokens.refresh_token;
      }

      localStorage.setItem('auth-tokens', JSON.stringify(tokens));
   }

   private retrieveTokens(): AuthTokenModel {
      const tokensString = localStorage.getItem('auth-tokens');
      const tokensModel: AuthTokenModel = tokensString == null ? null : JSON.parse(tokensString);
      return tokensModel;
   }

   private removeToken(): void {
      localStorage.removeItem('auth-tokens');
   }

   private updateState(newState: AuthStateModel): void {
      const previousState = this.state.getValue();
      this.state.next(Object.assign({}, previousState, newState));
   }

   private getTokens(data: RefreshGrantModel | LoginModel, grantType: string): Observable<AuthTokenModel> {
      const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
      const options = { headers: headers };

      Object.assign(data, { grant_type: grantType, scope: 'openid offline_access profile roles' });

      const params = new URLSearchParams();
      Object.keys(data)
         .forEach(key => params.append(key, data[key]));
      return this.http.post(`${environment.baseAuthUrl}connect/token`, params.toString(), options)
         .pipe(tap<AuthTokenModel>(tokens => {
            const now = new Date();
            tokens.expiration_date = new Date(now.getTime() + tokens.expires_in * 1000).getTime().toString();

            const profile: ProfileModel = jwt.decodeToken(tokens.id_token);

            this.storeToken(tokens);
            this.updateState({ authReady: true, tokens, profile });
         }));
   }

   private startupTokenRefresh(): Observable<AuthTokenModel> {
      return of(this.retrieveTokens())
         .pipe(mergeMap((tokens: AuthTokenModel) => {
            if (!tokens) {
               this.updateState({ authReady: true });
               return throwError('No token in Storage');
            }
            const profile: ProfileModel = jwt.decodeToken(tokens.id_token);
            this.updateState({ tokens, profile });

            if (+tokens.expiration_date > new Date().getTime()) {
               this.updateState({ authReady: true });
            }

            return this.refreshTokens();
         }))
         .pipe(catchError(error => {
            this.logout();
            this.updateState({ authReady: true });
            return throwError(error);
         }));
   }

   //private setCookie(name, tokens, days) {
   //   var expires = "";
   //   if (days) {
   //      var date = new Date();
   //      date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
   //      expires = "; expires=" + date.toUTCString();
   //   }
   //   debugger;
   //   document.cookie = name + "=" + (tokens || "") + expires + "; path=/";
   //}

   private scheduleRefresh(): void {
      this.refreshSubscription$ = this.tokens$
         .pipe(first())
         // refresh every half the total expiration time
         .pipe(flatMap(tokens => {
            let i = interval(tokens.expires_in / 2 * 1000);
            // todo: shouldn't go here, in it's own setCookie method?
            //this.setCookie("tokens", JSON.stringify(tokens), 30);
            return i;
         }))
         .pipe(flatMap(() => this.refreshTokens()))
         .subscribe();
   }
}
