import { AuthTokenModel } from './auth-tokens-model';
import { User } from './user.model';

export interface AuthStateModel {
  tokens?: AuthTokenModel;
  profile?: User;
  authReady?: boolean;
}
