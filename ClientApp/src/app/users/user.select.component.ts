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

    @Input() id: string | string[];
    @Input() user: User | User[];
    @Output() userChange = new EventEmitter<User | User[]>();
    @Input() canRemoveFilters = false;
    @Input() multiple = false;
    @Input() showAddNew = false;

    disabled = false;
    placeholder = this.multiple ? "Select users" : "Select an user";

    @ViewChild('modal', { static: false }) modal: UserModalComponent;

    constructor(
    ) {
    }

    ngOnInit(): void {
    }

    propagateChange = (_) => { };

    writeValue(id: string | string[]): void {
        if (id !== undefined) {
            this.id = id;
            this.propagateChange(this.id);
        }
    }

    registerOnChange(fn): void {
        this.propagateChange = fn;
    }

    registerOnTouched(): void {
    }

    setDisabledState?(isDisabled: boolean): void {
        this.disabled = isDisabled;
    }

    changed(user: User | User[]) {
        if (this.disabled) return;
        this.user = user;
        this.userChange.emit(user);
        if (this.multiple)
            this.writeValue(user ? (user as User[]).map(o => o.id) : null);
        else
            this.writeValue(user ? (user as User).id : null);
    }

    getLabel() {
        if (this.multiple) {
            let label = "";
            (this.user as User[]).forEach(user => label += (label === "" ? "" : ", ") + user.firstName);
            return label;
        }
        return this.user ? (this.user as User).firstName : "";
    }

    openModal() {
        if (this.disabled) return;
        this.modal.open();
    }
}
