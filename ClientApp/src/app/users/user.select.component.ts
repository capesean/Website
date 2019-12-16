import { Component, OnInit, forwardRef, ViewChild, Input, EventEmitter, Output } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { UserModalComponent } from './user.modal.component';
import { User } from '../common/models/user.model';
import { Enum } from '../common/models/enums.model';

@Component({
   selector: 'user-select',
   templateUrl: './user.select.component.html',
   providers: [{
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => UserSelectComponent),
      multi: true
   }],
   host: { 'class': 'app-select' }
})
export class UserSelectComponent implements OnInit, ControlValueAccessor {

   @Input() id: string;
   @Input() user: User;
   @Output() userChange = new EventEmitter<User>();
   @Input() canRemoveFilters: boolean = false;
   @Input() multiple: boolean = false;

   showAddNew: boolean = false;
   disabled: boolean = false;
   placeholder = "Select a user";

   @ViewChild('modal', { static: false }) modal: UserModalComponent;

   constructor(
   ) {
   }

   ngOnInit(): void {
   }

   propagateChange = (_: any) => { };

   writeValue(id: string): void {
      if (id !== undefined) {
         this.id = id;
         this.propagateChange(this.id);
      }
   }

   registerOnChange(fn: any): void {
      this.propagateChange = fn;
   }

   registerOnTouched(fn: any): void {
   }

   setDisabledState?(isDisabled: boolean): void {
      this.disabled = isDisabled;
   }

   change(user: User) {
      if (this.disabled) return;
      this.user = user;
      this.userChange.emit(user);
      this.writeValue(user ? user.id : null);
   }

   getLabel() {
      return this.user ? this.user.firstName : "";
   }

   openModal() {
      if (this.disabled) return;
      this.modal.open();
   }
}
