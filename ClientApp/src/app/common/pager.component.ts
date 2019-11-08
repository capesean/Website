import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { PagingOptions } from './models/http.model';

@Component({
   selector: 'pager',
   templateUrl: './pager.component.html'
})
export class PagerComponent {

   @Input()
   set headers(headers: PagingOptions) {
      this.pagerHeaders = headers;
      if (headers == undefined || headers.totalRecords===undefined) 
         this.pagerInfo = "";
      else if (headers.totalRecords === 0)
         this.pagerInfo = "No records found";
      else if (headers.totalRecords === 1)
         this.pagerInfo = "Showing the only record";
      else if (headers.totalPages === 1 || headers.pageSize === 0)
         this.pagerInfo = "Showing all " + headers.totalRecords + " records";
      else if (headers.pageIndex === headers.totalPages - 1)
         this.pagerInfo = (headers.pageIndex * headers.pageSize + 1) + " to " + headers.totalRecords + " of " + headers.totalRecords + " records";
      else
         this.pagerInfo = (headers.pageIndex * headers.pageSize + 1) + " to " + ((headers.pageIndex + 1) * headers.pageSize) + " of " + headers.totalRecords + " records";
   }

   @Output() pageChanged: EventEmitter<number> = new EventEmitter();
   pagerInfo: string;
   pagerHeaders: PagingOptions;

   constructor() { }

   changePage(pageIndex: number) {
      this.pageChanged.emit(pageIndex);
   }
}
