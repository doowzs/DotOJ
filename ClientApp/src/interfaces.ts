export interface LanguageInfo {
  code: number;
  name: string;
  mode: string;
  env: string;
  option: string;
}

export interface VerdictInfo {
  code: number;
  name: string;
  showCase: boolean;
  stage: number;
  explain: string;
}

export interface Program {
  language: number;
  code: string;
}

export interface TestCaseDto {
  input: string;
  output: string;
}

export interface ProblemInfoDto {
  id: number;
  label: string; // added in client
  title: string;
  solved: boolean;
}

export interface ProblemViewDto {
  id: number;
  assignmentId: number;
  title: string;
  description: string;
  inputFormat: string;
  outputFormat: string;
  footNote: string;
  timeLimit: number;
  memoryLimit: number;
  hasSpecialJudge: boolean;
  hasHacking: boolean;
  sampleCases: TestCaseDto[];
  acceptedSubmissions: number;
  totalSubmissions: number;
}

export interface AssignmentInfoDto {
  id: number;
  title: string;
  isPublic: boolean;
  mode: number;
  beginTime: Date;
  endTime: Date;
  registered: boolean;
}

export interface AssignmentListPagination {
  pageIndex: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  items: AssignmentInfoDto[];
}

export interface AssignmentNoticeDto {
  id: number;
  assignmentId: number;
  content: string;
}

export interface AssignmentViewDto {
  id: number;
  title: string;
  description: string;
  isPublic: boolean;
  mode: number;
  beginTime: string;
  endTime: string;
  problems: ProblemInfoDto[];
  notices: AssignmentNoticeDto[];
}

export interface AssignmentEditDto {
  id: number;
  title: string;
  description: string;
  isPublic: boolean;
  mode: number;
  beginTime: Date;
  endTime: Date;
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
