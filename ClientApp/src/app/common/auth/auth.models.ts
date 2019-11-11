export interface LoginModel {
   username: string;
   password: string;
}

export interface ResetPasswordModel {
   username: string;
}

export interface ResetModel {
   token: string;
   username: string;
   newPassword: string;
   confirmPassword: string;
}

export interface RegisterModel {
   username: string;
   password: string;
   // todo: add this
   //confirmPassword: string;
}

export interface AuthStateModel {
   tokens?: AuthTokenModel;
   profile?: ProfileModel;
   authReady?: boolean;
}

export interface AuthTokenModel {
   access_token: string;
   refresh_token: string;
   id_token: string;
   expires_in: number;
   token_type: string;
   expiration_date: string;
}

export interface RefreshGrantModel {
   refresh_token: string;
}

export interface ProfileModel {
   sub: string;
   jti: string;
   useage: string;
   at_hash: string;
   nbf: number;
   exp: number;
   iat: number;
   iss: string;

   unique_name: string;
   email_confirmed: boolean;
   role: string[] | string;
}
