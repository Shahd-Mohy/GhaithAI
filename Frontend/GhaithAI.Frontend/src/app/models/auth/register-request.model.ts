export interface EmergencyContactRequest {
  fullName: string;
  phoneNumber: string;
  relationship: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  phoneNumber: string;
  countryCode: string;
  preferredLanguage: string;
  acceptedTerms: boolean;
  acceptedPrivacyPolicy: boolean;
  acceptedAiChat: boolean;
  acceptedMoodTracking: boolean;
  acceptedDataCollection: boolean;
  firstContact: EmergencyContactRequest;
  secondContact?: EmergencyContactRequest | null; 
  age?: number;
  concerns: string[];
  sleepQuality: string;
  stressLevel: string;
  hasTherapyHistory: boolean;
  takesMedication: boolean;
}
