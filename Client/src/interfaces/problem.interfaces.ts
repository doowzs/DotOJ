import { Program } from './submission.interfaces';

export enum ProblemType {
  Ordinary = 0,
  TestKitLab = 1
}

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
  type: ProblemType;
  title: string;
  attempted: boolean;
  solved: boolean;
  totalContestants: number;
  acceptedContestants: number;
}

export interface ProblemViewDto {
  id: number;
  contestId: number;
  type: ProblemType;
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
  type: ProblemType;
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
