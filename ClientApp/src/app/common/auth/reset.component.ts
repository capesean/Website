import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { AuthService } from './auth.service';
import { ResetModel } from './auth.models';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
   selector: 'reset',
   templateUrl: './reset.component.html',
   styleUrls: ['./auth.css'],
})
export class ResetComponent implements OnInit {

   public reset: ResetModel = { username: undefined, token: undefined, newPassword: undefined, confirmPassword: undefined };

   constructor(
      private toastr: ToastrService,
      private authService: AuthService,
      private route: ActivatedRoute,
      private router: Router
   ) { }

   ngOnInit() {
       this.reset.username = this.route.snapshot.queryParams.e;
       this.reset.token = this.route.snapshot.queryParams.t;
   }

   submit(form: NgForm) {

      if (form.invalid) {

         this.toastr.error("The form has not been completed correctly.", "Error");
         return;

      }

      if (this.reset.newPassword !== this.reset.confirmPassword) {

         this.toastr.error("The two passwords are different.", "Error");
         return;

      }

      // todo: use ngForm
      // todo: this needs to return a promise, and if success, THEN navigate, else route won't be allowed...
      this.authService.reset(this.reset)
         .subscribe(
            () => {
               this.toastr.success("Your password has been reset", "Reset Password");
               this.router.navigate(['/auth/login']);
            },
            err => {
               // todo: reason?
               this.toastr.error("Reset password failed", "Reset Password");
            }
         );

   }

}
