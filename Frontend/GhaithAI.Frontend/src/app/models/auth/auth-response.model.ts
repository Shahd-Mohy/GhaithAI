export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  profilePicture?: string;
  expiration: string;
  role: string;
}
