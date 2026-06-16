export interface DoctorClinicPatientListDto {
    id: string; // Guid
    doctorId: string; // Guid
    patientFullName: string;
    patientPhone: string;
    notes: string;
}

export interface CreateClinicPatientDto {
    patientFullName: string;
    patientPhone: string;
    notes?: string;
}

export interface UpdateClinicPatientDto {
    id: string; // Guid
    patientFullName: string;
    patientPhone: string;
    notes?: string;
}