import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { ProfileModel } from '../auth/auth.models';
import { Roles } from '../models/roles.model';
import { BehaviorSubject } from 'rxjs';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {

    public isExpanded = false;
    public profile$ = new BehaviorSubject<ProfileModel>(undefined);
    public isAdmin = false;

    constructor(
        private authService: AuthService,
        private toastr: ToastrService,
        private router: Router
    ) {
    }

    ngOnInit(): void {
        this.authService.getProfile().subscribe(profile => {
            this.profile$.next(profile);
            this.isAdmin = this.authService.isInRole(profile, Roles.Administrator);
        });
    }

    logout() {
        this.authService.logout();
        this.toastr.success("You have been logged out successfully", "Log Out");
        this.router.navigate(["/auth/login"]);
    }

    toggle() {
        this.isExpanded = !this.isExpanded;
    }
}
