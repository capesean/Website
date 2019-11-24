import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PagerComponent } from './common/pager.component';
import { RouterModule } from '@angular/router';
import { MainComponent } from './main.component';
import { NavMenuComponent } from './common/nav-menu/nav-menu.component';
import { MomentPipe } from './common/pipes/momentPipe';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { UserSelectComponent } from './users/user.select.component';
import { UserModalComponent } from './users/user.modal.component';

@NgModule({
   imports: [
      CommonModule,
      FormsModule,
      RouterModule,
      NgbModule,
      DragDropModule,
      BreadcrumbModule
   ],
   declarations: [
      PagerComponent,
      MainComponent,
      NavMenuComponent,
      MomentPipe,
      UserSelectComponent, UserModalComponent
   ],
   exports: [
      PagerComponent,
      MainComponent,
      NavMenuComponent,
      NgbModule,
      MomentPipe,
      UserSelectComponent, UserModalComponent
   ]
})
export class SharedModule { }
