export interface EmergencyContact {
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

  firstContact: EmergencyContact;
  secondContact: EmergencyContact;

  age?: number;

  concerns: string[];

  sleepQuality?: string;

  stressLevel?: string;

  hasTherapyHistory: boolean;

  takesMedication: boolean;
}
