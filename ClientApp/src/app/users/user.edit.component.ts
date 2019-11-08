import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { BreadcrumbService } from 'angular-crumbs';
import { ErrorService } from '../common/services/error.service';
import { PagingOptions } from '../common/models/http.model';
import { User } from '../common/models/user.model';
import { UserService } from '../common/services/user.service';

@Component({
   selector: 'user-edit',
   templateUrl: './user.edit.component.html'
})
export class UserEditComponent implements OnInit, OnDestroy {

   public user: User = new User();
   public isNew: boolean = true;
   public routerSubscription: Subscription;
   public roles = [{ name: 'Administrator', id: '470356a5-f7db-4e2e-9c99-62c2800dc2f4' }];

   constructor(
      private router: Router,
      public route: ActivatedRoute,
      private toastr: ToastrService,
      private breadcrumbService: BreadcrumbService,
      private errorService: ErrorService,
      private userService: UserService,
   ) {
   }

   ngOnInit(): void {

      this.route.params.subscribe(params => {

         let id = params["id"];
         this.isNew = id === "add";

         if (!this.isNew) {

            this.user.id = id;
            this.loadUser();

         }

         this.routerSubscription = this.router.events.subscribe(event => {
            if (event instanceof NavigationEnd && !this.route.firstChild) {
               this.loadUser();
            }
         });

      });

   }

   ngOnDestroy(): void {
      this.routerSubscription.unsubscribe();
   }

   private loadUser() {

      this.userService.get(this.user.id)
         .subscribe(
            user => {
               this.user = user;
               this.changeBreadcrumb();
            },
            err => {
               this.errorService.handleError(err, "User", "Load");
               if (err instanceof HttpErrorResponse && err.status === 404)
                  this.router.navigate(["../../"], { relativeTo: this.route });
            }
         );

   }

   save(form: NgForm): void {

      if (form.invalid) {

         this.toastr.error("The form has not been completed correctly.", "Form Error");
         return;

      }

      this.userService.save(this.user)
         .subscribe(
            user => {
               this.toastr.success("The user has been saved", "Save User");
               if (this.isNew) this.router.navigate(["../", user.id], { relativeTo: this.route });
            },
            err => {
               this.errorService.handleError(err, "User", "Save");
            }
         );

   }

   delete(): void {

      if (!confirm("Confirm delete?")) return;

      this.userService.delete(this.user.id)
         .subscribe(
            () => {
               this.toastr.success("The user has been deleted", "Delete User");
               this.router.navigate(["../../"], { relativeTo: this.route });
            },
            err => {
               this.errorService.handleError(err, "User", "Delete");
            }
         );

   }

   changeBreadcrumb(): void {
      this.breadcrumbService.changeBreadcrumb(this.route.snapshot, this.user.firstName != undefined ? this.user.firstName : "(new user)");
   }

}
