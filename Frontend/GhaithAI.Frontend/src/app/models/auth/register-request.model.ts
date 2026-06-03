export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  countryCode: string;
  preferredLanguage: string;

  acceptedAiChat: boolean;
  acceptedMoodTracking: boolean;
  acceptedDataCollection: boolean;
  acceptedTerms: boolean;
  acceptedPrivacyPolicy: boolean;
}