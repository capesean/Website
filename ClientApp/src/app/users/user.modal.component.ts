import { Component, OnInit, ViewChild, Output, EventEmitter, TemplateRef, Input } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs';
import { UserSearchOptions, UserSearchResponse, User } from '../common/models/user.model';
import { UserService } from '../common/services/user.service';
import { PagingOptions } from '../common/models/http.model';
import { ErrorService } from '../common/services/error.service';

@Component({
    selector: 'user-modal',
    templateUrl: './user.modal.component.html'
})
export class UserModalComponent implements OnInit {

    modal: NgbModalRef;
    user: User;
    selectedItems: User[] = [];
    headers: PagingOptions = new PagingOptions();
    searchOptions: UserSearchOptions = new UserSearchOptions();
    users: User[];
    showAddNew: boolean = false;
    allSelected: boolean = false;

    @ViewChild('content') content: TemplateRef<any>;
    @Output() change: EventEmitter<User> = new EventEmitter<User>();
    @Input() canRemoveFilters: boolean = false;
    @Input() multiple: boolean = false;

    constructor(
        private modalService: NgbModal,
        private userService: UserService,
        private errorService: ErrorService
    ) {
    }

    ngOnInit(): void {
        this.searchOptions.includeEntities = true;
    }

    open() {
        //this.selectedItems = []; <-- allow multiple selects to re-open with selection in tact
        this.modal = this.modalService.open(this.content, { size: 'xl', centered: true, scrollable: true });
        this.runSearch();
        this.modal.result.then((user: User) => {
            this.user = user;
            this.change.emit(user);
        }, () => {
            // dismissed
        });
    }

    private runSearch(pageIndex: number = 0): Observable<UserSearchResponse> {

        this.searchOptions.pageIndex = pageIndex;

        var observable = this.userService
            .search(this.searchOptions);

        observable.subscribe(
            response => {
                this.users = response.users;
                this.headers = response.headers;
            },
            err => {

                this.errorService.handleError(err, "Users", "Load");

            }
        );

        return observable;

    }

    close() {
        if (this.multiple) this.modal.close(this.selectedItems);
        else this.modal.dismiss();
    }

    clear() {
        if (this.multiple) this.modal.close([]);
        else this.modal.close(undefined);
    }

    select(user: User) {
        if (this.multiple) {
            if (this.isSelected(user)) {
                for (var i = 0; i < this.selectedItems.length; i++) {
                    if (this.selectedItems[i].id == user.id) {
                        this.selectedItems.splice(i, 1);
                        break;
                    }
                }
            } else {
                this.selectedItems.push(user);
            }
        } else {
            this.modal.close(user);
        }
    }

    isSelected(user: User) {
        if (!this.multiple) return false;
        return this.selectedItems.filter(item => item.id === user.id).length > 0;
    }

    toggleAll() {
        this.allSelected = !this.allSelected;
        this.users.forEach(user => {
            let isSelected = this.isSelected(user);
			if (isSelected && !this.allSelected) {
                for (var i = 0; i < this.selectedItems.length; i++) {
                    if (this.selectedItems[i].id == user.id) {
                        this.selectedItems.splice(i, 1);
                        break;
                    }
                }
            } else if (!isSelected && this.allSelected) {
                this.selectedItems.push(user);
            }
        });
    }

    selectAll() {

        this.searchOptions.pageSize = 0;
        this.searchOptions.pageIndex = 0;

        this.userService.search(this.searchOptions)

            .subscribe(
                response => {
                    this.modal.close(response.users);
                    this.users = response.users;
                },
                err => {
                    this.errorService.handleError(err, "Users", "Load");
                }
            );

    }
}
