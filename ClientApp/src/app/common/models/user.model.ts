import { SearchOptions, PagingOptions } from './http.model';

export class User {
   id: string;
   firstName: string;
   lastName: string;

   constructor() {
      this.id = "00000000-0000-0000-0000-000000000000";
   }
}

export class UserSearchOptions extends SearchOptions {
   q: string;
}

export class UserSearchResponse {
   users: User[];
   headers: PagingOptions;
}
