import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { AuthService } from './auth.service';
import { LoginModel } from './auth.models';
import { Router } from '@angular/router';
import { ErrorService } from '../../common/services/error.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'login',
    templateUrl: './login.component.html',
    styleUrls: ['./auth.css'],
})
export class LoginComponent implements OnInit {

    public login: LoginModel = { username: undefined, password: undefined };

    constructor(
        private toastr: ToastrService,
        private authService: AuthService,
        private router: Router,
        private errorService: ErrorService
    ) { }

    ngOnInit() {
    }

    submit(form: NgForm) {

        if (form.invalid) {

            this.toastr.error("The form has not been completed correctly.");
            return;

        }

        // todo: use ngForm
        // todo: this needs to return a promise, and if success, THEN navigate, else route won't be allowed...
        this.authService.login(this.login)
            .subscribe(
                () => {
                    this.router.navigate(['/']);
                },
                (err: HttpErrorResponse) => {
                    this.errorService.handleError(err, "User", "Login");
                }
            );

    }

}
