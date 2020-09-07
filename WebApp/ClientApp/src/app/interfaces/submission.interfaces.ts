import * as moment from 'moment';
import { fixSubmissionREVerdictCode, VerdictInfo, Verdicts } from '../consts/verdicts.consts';
import { LanguageInfo, Languages } from '../consts/languages.consts';
import { Base64 } from 'js-base64';

export interface Program {
  language: LanguageInfo | number;
  code: string;
}

export interface SubmissionInfoDto {
  id: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  problemId: number;
  language: LanguageInfo | number;
  codeBytes: number;
  verdict: VerdictInfo | number;
  verdictInfo: VerdictInfo;
  time: number;
  memory: number;
  failedOn: number;
  score: number;
  progress: number;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}

export interface SubmissionViewDto {
  id: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  problemId: number;
  program: Program;
  codeBytes: number;
  verdict: VerdictInfo | number;
  verdictInfo: VerdictInfo;
  time: number;
  memory: number;
  failedOn: number;
  score: number;
  progress: number;
  message: string;
  judgedBy: string;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}

export interface SubmissionEditDto {
  id: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  problemId: number;
  program: Program;
  verdict: VerdictInfo | number;
  verdictInfo: VerdictInfo;
  time: number;
  memory: number;
  failedOn: number;
  score: number;
  message: string;
  judgedBy: string;
  judgedAt: moment.Moment | string;
  createdAt: moment.Moment | string;
}

export const mapSubmissionInfoDtoFields = (submission: SubmissionInfoDto): SubmissionInfoDto => {
  fixSubmissionREVerdictCode(submission);
  submission.verdict = submission.verdictInfo = Verdicts.find(v => v.code === submission.verdict);
  submission.language = Languages.find(l => l.code === submission.language);
  submission.createdAt = moment.utc(submission.createdAt).local();
  submission.judgedAt = moment.utc(submission.judgedAt).local();
  return submission;
};

export const mapSubmissionViewDtoFields = (submission: SubmissionViewDto): SubmissionViewDto => {
  fixSubmissionREVerdictCode(submission);
  submission.verdict = submission.verdictInfo  = Verdicts.find(v => v.code === submission.verdict);
  submission.program.language = Languages.find(l => l.code === submission.program.language);
  submission.program.code = Base64.decode(submission.program.code);
  submission.codeBytes = new Blob([submission.program.code]).size;
  submission.message = Base64.decode(submission.message);
  submission.createdAt = moment.utc(submission.createdAt).local();
  if (submission.judgedAt) {
    submission.judgedAt = moment.utc(submission.judgedAt).local();
  }
  return submission;
};

export const mapSubmissionEditDtoFields = (submission: SubmissionEditDto): SubmissionEditDto => {
  fixSubmissionREVerdictCode(submission);
  submission.verdict = submission.verdictInfo  = Verdicts.find(v => v.code === submission.verdict);
  submission.program.language = Languages.find(l => l.code === submission.program.language);
  submission.program.code = Base64.decode(submission.program.code);
  submission.message = Base64.decode(submission.message);
  submission.createdAt = moment.utc(submission.createdAt).local();
  if (submission.judgedAt) {
    submission.judgedAt = moment.utc(submission.judgedAt).local();
  }
  return submission;
};
