export interface DoctorClinicPatientListDto {
    id: string;
    doctorId: string; 
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
    id: string;
    patientFullName: string;
    patientPhone: string;
    notes?: string;
}