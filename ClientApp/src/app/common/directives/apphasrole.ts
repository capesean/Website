import { Input, Directive, AfterViewInit, ViewContainerRef, TemplateRef } from "@angular/core";
import { AuthService } from "../auth/auth.service";

@Directive({
    selector: '[appHasRole]'
})
export class AppHasRoleDirective implements AfterViewInit {

    @Input('appHasRole')
    public role: string;

    constructor(
        private viewContainer: ViewContainerRef,
        private templateRef: TemplateRef<unknown>,
        private authService: AuthService
    ) {
    }

    ngAfterViewInit(): void {
        this.authService.getProfile().subscribe(
            profile => {

                if (this.role && profile && this.authService.isInRole(profile, this.role)) {
                    this.viewContainer.createEmbeddedView(this.templateRef);
                } else {
                    this.viewContainer.clear();
                }

            }
        );
    }
}
