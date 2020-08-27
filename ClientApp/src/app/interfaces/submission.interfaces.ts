import * as moment from 'moment';
import { VerdictInfo } from '../consts/verdicts.consts';
import { LanguageInfo } from '../consts/languages.consts';

export interface Program {
  language: LanguageInfo | number;
  code: string;
}

export interface SubmissionInfoDto {
  id: number;
  userId: string;
  problemId: number;
  language: LanguageInfo | number;
  codeBytes: number;
  verdict: VerdictInfo | number;
  failedOn: number;
  score: number;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}

export interface SubmissionViewDto {
  id: number;
  userId: string;
  problemId: number;
  program: Program;
  verdict: VerdictInfo | number;
  failedOn: number;
  score: number;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}
