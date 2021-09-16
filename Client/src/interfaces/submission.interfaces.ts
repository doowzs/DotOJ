import * as moment from 'moment';
import { VerdictInfo, Verdicts } from '../consts/verdicts.consts';
import { LanguageInfo, Languages } from '../consts/languages.consts';
import { Base64 } from 'js-base64';

export interface Program {
  language: number;
  languageInfo?: LanguageInfo;
  code: string;
  input?: string;
}

export interface SubmissionInfoDto {
  id: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  problemId: number;
  language: number;
  languageInfo?: LanguageInfo;
  codeBytes: number;
  hasInput: boolean;
  verdict: number;
  verdictInfo?: VerdictInfo;
  time: number;
  memory: number;
  failedOn: number;
  score: number;
  progress: number;
  hasMessage: boolean;
  viewable: boolean;
  judgedAt: moment.Moment | string;
  judgedAtMoment?: moment.Moment;
  createdAt: moment.Moment | string;
  createdAtMoment?: moment.Moment;
}

export interface SubmissionViewDto {
  id: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  problemId: number;
  program: Program;
  hasInput: boolean;
  codeBytes: number;
  verdict: number;
  verdictInfo: VerdictInfo;
  time: number;
  memory: number;
  failedOn: number;
  score: number;
  progress: number;
  message: string;
  comments: string;
  judgedBy: string;
  judgedAt: moment.Moment | string;
  judgedAtMoment?: moment.Moment;
  createdAt: moment.Moment | string;
  createdAtMoment?: moment.Moment;
}

export interface SubmissionEditDto {
  id: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  problemId: number;
  program: Program;
  verdict: number;
  verdictInfo: VerdictInfo;
  time: number;
  memory: number;
  failedOn: number[];
  score: number;
  message: string;
  judgedBy: string;
  judgedAt: moment.Moment | string;
  judgedAtMoment?: moment.Moment;
  createdAt: moment.Moment | string;
  createdAtMoment?: moment.Moment;
}

export const mapSubmissionInfoDtoFields = (submission: SubmissionInfoDto): SubmissionInfoDto => {
  submission.verdictInfo = Verdicts.find(v => v.code === submission.verdict);
  submission.languageInfo = Languages.find(l => l.code === submission.language);
  submission.createdAt = moment.utc(submission.createdAt).local();
  submission.createdAtMoment = moment.utc(submission.createdAt).local();
  if (submission.judgedAt) {
    submission.judgedAt = moment.utc(submission.judgedAt).local();
    submission.judgedAtMoment = moment.utc(submission.judgedAt).local();
  }
  return submission;
};

export const mapSubmissionViewDtoListFields = (submissions: SubmissionViewDto[]): SubmissionViewDto[] => {
  for (let submission of submissions) {
    submission.verdictInfo = Verdicts.find(v => v.code === submission.verdict);
    submission.program.languageInfo = Languages.find(l => l.code === submission.program.language);
    submission.program.code = Base64.decode(submission.program.code);
    submission.codeBytes = new Blob([submission.program.code]).size;
    submission.hasInput = !!submission.program.input;
    console.log(submission.hasInput);
    submission.message = Base64.decode(submission.message ?? '');
    submission.createdAt = moment.utc(submission.createdAt).local();
    submission.createdAtMoment = moment.utc(submission.createdAt).local();
    if (submission.judgedAt) {
      submission.judgedAt = moment.utc(submission.judgedAt).local();
      submission.judgedAtMoment = moment.utc(submission.judgedAt).local();
    }
  }
  return submissions;
};

export const mapSubmissionViewDtoFields = (submission: SubmissionViewDto): SubmissionViewDto => {
  submission.verdictInfo = Verdicts.find(v => v.code === submission.verdict);
  submission.program.languageInfo = Languages.find(l => l.code === submission.program.language);
  submission.program.code = Base64.decode(submission.program.code);
  submission.codeBytes = new Blob([submission.program.code]).size;
  submission.hasInput = !!submission.program.input;
  console.log(submission.hasInput);
  submission.message = Base64.decode(submission.message ?? '');
  submission.createdAt = moment.utc(submission.createdAt).local();
  submission.createdAtMoment = moment.utc(submission.createdAt).local();
  if (submission.judgedAt) {
    submission.judgedAt = moment.utc(submission.judgedAt).local();
    submission.judgedAtMoment = moment.utc(submission.judgedAt).local();
  }
  return submission;
};

export const mapSubmissionEditDtoFields = (submission: SubmissionEditDto): SubmissionEditDto => {
  submission.verdictInfo = Verdicts.find(v => v.code === submission.verdict);
  submission.program.languageInfo = Languages.find(l => l.code === submission.program.language);
  submission.program.code = Base64.decode(submission.program.code);
  submission.message = Base64.decode(submission.message ?? '');
  submission.createdAt = moment.utc(submission.createdAt).local();
  submission.createdAtMoment = moment.utc(submission.createdAt).local();
  if (submission.judgedAt) {
    submission.judgedAt = moment.utc(submission.judgedAt).local();
    submission.judgedAtMoment = moment.utc(submission.judgedAt).local();
  }
  return submission;
};
