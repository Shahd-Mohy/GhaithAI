export interface AdminSelfHelpGetAllDto {
  id: string;
  title: string;
  type: string;
  durationMinutes: number | null;
  difficultyLevel: string;
}


export interface ExerciseTipDto {
  id: string;
  text: string;
}

export interface AdminSelfHelpDetailsDto {
  id: string;
  title: string;
  type: string;
  description: string;
  contentUrl: string | null;
  durationMinutes: number | null;
  difficultyLevel: string;
  isActive: boolean;
  exerciseTips: ExerciseTipDto[];
}


export interface CreateExerciseTipDto {
  text: string;
}

export interface AdminSelfHelpSaveDto {
  title: string;
  type: string;
  description: string;
  contentUrl: string | null;
  durationMinutes: number | null;
  difficultyLevel: string;
  isActive: boolean;
  exerciseTips: CreateExerciseTipDto[];
}

export interface UpdateExerciseTipDto {
  id: string | null;
  text: string;
}

export interface AdminSelfHelpUpdateDto {
  id: string;
  title: string;
  type: string;
  description: string;
  contentUrl: string | null;
  durationMinutes: number;
  difficultyLevel: string;
  isActive: boolean;
  exerciseTips: UpdateExerciseTipDto[];
}
