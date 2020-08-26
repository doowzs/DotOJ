export interface Program {
  language: number;
  code: string;
}

export interface SubmissionInfoDto {
  id: number;
  userId: string;
  problemId: number;
  verdict: number;
  lastTestCase: number;
  judgedAt: string;
  createdAt: string;
}

export interface SubmissionViewDto {
  id: number;
  userId: string;
  problemId: number;
  program: Program;
  verdict: number;
  lastTestCase: number;
  judgedAt: string;
  createdAt: string;
}
