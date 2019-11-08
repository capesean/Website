import { HttpParams } from '@angular/common/http';

export class SearchQuery {

   buildQueryParams(source: Object): HttpParams {
      let target: HttpParams = new HttpParams();
      Object.keys(source).forEach((key: string) => {
         const value: string | number | boolean | Date = source[key];
         if ((typeof value !== 'undefined') && (value !== null)) {
            target = target.append(key, value.toString());
         }
      });
      return target;
   }
}

export class SearchOptions {
   includeEntities: boolean = false;
   pageIndex: number = 0;
   pageSize: number = 10;
   orderBy: string = undefined;
   orderByAscending: boolean = undefined;
}

export class PagingOptions {
   pageIndex: number = 0;
   pageSize: number = 10;
   records: number;
   totalRecords: number;
   totalPages: number;
   first: number;
}
