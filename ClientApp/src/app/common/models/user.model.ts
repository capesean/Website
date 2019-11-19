import { SearchOptions, PagingOptions } from './http.model';
import { Role } from './roles.model';

export class User {
   id: string;
   firstName: string;
   lastName: string;
   fullName: string;
   enabled: boolean;
   roles: Role[] = [];

   constructor() {
      this.id = "00000000-0000-0000-0000-000000000000";
   }
}

export class UserSearchOptions extends SearchOptions {
   q: string;
}

export class UserSearchResponse {
   users: User[] = [];
   headers: PagingOptions;
}
