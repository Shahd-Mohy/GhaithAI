export interface DoctorBookingResponseDto {
    bookingId: string;
    patientName: string;
    patientPhone: string;
    bookingDate: string;
    slotTime: string;
    sessionType: string;
    bookingSource: string;
    status: string;
    notes: string;
}

export type TimeFilterType = 'all' | 'today' | 'tomorrow' | 'upcoming' | 'past' | (string & {});