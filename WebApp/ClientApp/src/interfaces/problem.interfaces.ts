import { Program } from './submission.interfaces';

export interface TestCase {
  input: string;
  output: string;
}

export interface ProblemStatistics {
  totalSubmissions: number;
  acceptedSubmissions: number;
  totalContestants: number;
  acceptedContestants: number;
  byVerdict: { number: number };
  updatedAt: string; // not used now
}

export interface ProblemInfoDto {
  id: number;
  contestId: number;
  label: string; // added in client service
  title: string;
  attempted: boolean;
  solved: boolean;
  accepted: number;
}

export interface ProblemViewDto {
  id: number;
  contestId: number;
  title: string;
  description: string;
  inputFormat: string;
  outputFormat: string;
  footNote: string;
  timeLimit: number;
  memoryLimit: number;
  hasSpecialJudge: boolean;
  hasHacking: boolean;
  sampleCases: TestCase[];
  statistics: ProblemStatistics;
}

export interface ProblemEditDto {
  id: number;
  contestId: number;
  title: string;
  description: string;
  inputFormat: string;
  outputFormat: string;
  footNote: string;
  timeLimit: number;
  memoryLimit: number;
  hasSpecialJudge: boolean;
  specialJudgeProgram: Program;
  hasHacking: boolean;
  sampleCases: TestCase[];
}
