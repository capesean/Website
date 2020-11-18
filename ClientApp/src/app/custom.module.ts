import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SharedModule } from './shared.module';
import { HomeComponent } from './home/home.component';
import { ChangePasswordComponent } from './users/changepassword.component';
import { CustomRoutes } from './custom.routes';
import { ErrorComponent } from './common/error/error.component';
import { ErrorsComponent } from './common/error/errors.component';
import { SettingsComponent } from './settings/settings.component';

@NgModule({
   declarations: [HomeComponent, ChangePasswordComponent, ErrorsComponent, ErrorComponent, SettingsComponent],
   imports: [
      CommonModule,
      FormsModule,
      RouterModule.forChild(CustomRoutes),
      NgbModule,
      DragDropModule,
      SharedModule
   ]
})
export class CustomModule { }
