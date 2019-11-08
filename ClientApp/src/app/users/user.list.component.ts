import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { PagingOptions } from '../common/models/http.model';
import { ErrorService } from '../common/services/error.service';
import { UserSearchOptions, UserSearchResponse, User } from '../common/models/user.model';
import { UserService } from '../common/services/user.service';

@Component({
   selector: 'user-list',
   templateUrl: './user.list.component.html'
})
export class UserListComponent implements OnInit {

   private users: User[];
   public searchOptions = new UserSearchOptions();
   public headers = new PagingOptions();
   public routerSubscription: Subscription;

   constructor(
      public route: ActivatedRoute,
      private router: Router,
      private errorService: ErrorService,
      private userService: UserService
   ) {
   }

   ngOnInit(): void {
      this.routerSubscription = this.router.events.subscribe(event => {
         if (event instanceof NavigationEnd && !this.route.firstChild) {
            this.runSearch();
         }
      });
      this.runSearch();
   }

   ngOnDestroy(): void {
      this.routerSubscription.unsubscribe();
   }

   runSearch(pageIndex: number = 0): Observable<UserSearchResponse> {

      this.searchOptions.pageIndex = pageIndex;

      var observable = this.userService
         .search(this.searchOptions);

      observable.subscribe(
         response => {
            this.users = response.users;
            this.headers = response.headers;
         },
         err => {

            this.errorService.handleError(err, "Users", "Load");

         }
      );

      return observable;

   }

   goToUser(user: User): void {
      this.router.navigate(['/users', user.id]);
   }
}

