import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PagerComponent } from './common/pager.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { UserListComponent } from './users/user.list.component';
import { UserEditComponent } from './users/user.edit.component';
import { UserSelectComponent } from './users/user.select.component';
import { UserModalComponent } from './users/user.modal.component';
import { GeneratedRoutes } from './generated.routes';
import { CustomComponents } from './custom.components';
import { MomentPipe } from './common/pipes/momentPipe';

@NgModule({
   declarations: [PagerComponent, UserListComponent, UserEditComponent, UserSelectComponent, UserModalComponent, MomentPipe].concat(CustomComponents),
   imports: [
      CommonModule,
      FormsModule,
      RouterModule.forChild(GeneratedRoutes),
      NgbModule
   ]
})
export class GeneratedModule { }
