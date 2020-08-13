export const Languages: { code, name, mode, env, option }[] = [
  {code: 50, name: 'C', mode: 'c_cpp', env: 'GCC 9.2', option: '-DONLINE_JUDGE --static -O2 --std=c11'},
  {code: 51, name: 'C#', mode: 'csharp', env: 'Mono 6.6', option: ''},
  {code: 54, name: 'C++', mode: 'c_cpp', env: 'GCC 9.2', option: '-DONLINE_JUDGE --static -O2 --std=c++17'},
  {code: 60, name: 'Golang', mode: 'golang', env: 'Go 1.13', option: ''},
  {code: 61, name: 'Haskell', mode: 'haskell', env: 'GHC 8.8', option: ''},
  {code: 62, name: 'Java 11', mode: 'java', env: 'OpenJDK 13.0', option: '-J-Xms32m -J-Xmx256m'},
  {code: 63, name: 'JavaScript', mode: 'javascript', env: 'Node.js 12.14', option: ''},
  {code: 64, name: 'Lua', mode: 'lua', env: 'Lua 5.3', option: ''},
  {code: 68, name: 'PHP', mode: 'php', env: 'PHP 7.4', option: ''},
  {code: 71, name: 'Python 3', mode: 'python', env: 'Python 3.8', option: ''},
  {code: 72, name: 'Ruby', mode: 'ruby', env: 'Ruby 2.7', option: ''},
  {code: 73, name: 'Rust', mode: 'rust', env: 'Rust 1.40', option: ''},
  {code: 74, name: 'TypeScript', mode: 'typescript', env: 'TypeScript 3.7', option: ''}
];

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
