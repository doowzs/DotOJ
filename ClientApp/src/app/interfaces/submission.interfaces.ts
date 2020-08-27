import * as moment from 'moment';
import { VerdictInfo } from '../consts/verdicts.consts';
import { LanguageInfo } from '../consts/languages.consts';

export interface Program {
  language: number;
  code: string;
}

export interface SubmissionInfoDto {
  id: number;
  userId: string;
  problemId: number;
  language: LanguageInfo | number;
  codeLength: number;
  verdict: VerdictInfo | number;
  lastTestCase: number;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}

export interface SubmissionViewDto {
  id: number;
  userId: string;
  problemId: number;
  program: Program;
  verdict: VerdictInfo | number;
  lastTestCase: number;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}
