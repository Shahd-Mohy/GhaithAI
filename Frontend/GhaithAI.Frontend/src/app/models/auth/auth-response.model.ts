export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  profilePicture?: string;
  expiration: string;
  role: string;
  // Optional clinician fields returned by the backend on login
  doctorType?: string;       // e.g. "Psychiatrist", "Therapist", "Counselor"
  specialization?: string;   // e.g. "Child & Adolescent Psychiatry"
}
