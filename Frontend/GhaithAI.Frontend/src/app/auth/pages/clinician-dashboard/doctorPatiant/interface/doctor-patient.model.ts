export interface PatientListItemDto {
  patientId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  riskLevel: string;
  lastBookingDate: string | null;
  totalBookingsCount: number;
}

export interface DoctorPatientsDashboardDto {
  totalPatients: number;
  highRiskCount: number;
  sessionsThisWeek: number;
  totalRiskAlerts: number;
  totalFilteredPatients: number;
  patients: PatientListItemDto[];
}
