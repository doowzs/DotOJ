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
