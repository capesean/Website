import { Input, Directive, AfterViewInit, ViewContainerRef, TemplateRef } from "@angular/core";
import { ProfileModel } from "../auth/auth.models";
import { AuthService } from "../auth/auth.service";

@Directive({
    selector: '[appHasRole]'
})
export class AppHasRoleDirective implements AfterViewInit {

    @Input('appHasRole')
    public role: string;

    @Input('appHasRoleProfile')
    public profile: ProfileModel;

    constructor(
        private viewContainer: ViewContainerRef,
        private templateRef: TemplateRef<unknown>,
        private authService: AuthService
    ) {
    }

    ngAfterViewInit(): void {
        if (this.role && this.profile && this.authService.isInRole(this.profile, this.role)) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
            this.viewContainer.clear();
        }
    }
}
