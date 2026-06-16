export interface RegisterClinicianRequest {
  fullName: string;
  email: string;
  password: string;
  phoneNumber: string;
  countryCode: string;
  preferredLanguage: string;
  gender: number;
  doctorType: number;
  specialization: string;
  bio: string;
  yearsOfExperience: number;
  documentsPdf: File;
}
