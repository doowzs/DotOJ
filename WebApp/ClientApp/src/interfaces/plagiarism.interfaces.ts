import * as moment from 'moment';

export interface PlagiarismResult {
  name: string;
  count: number;
  path: string;
}

export interface PlagiarismInfoDto {
  id: number;
  problemId: number;
  results: PlagiarismResult[];
  outdated: boolean;
  createdAt: string;
  createdAtMoment: moment.Moment;
  checkedAt: string;
  checkedAtMoment: moment.Moment;
  checkedBy: string;
}

export const mapPlagiarismInfoDtoFields = (plagiarism: PlagiarismInfoDto): PlagiarismInfoDto => {
  if (plagiarism.createdAt) {
    plagiarism.createdAtMoment = moment.utc(plagiarism.createdAt).local();
  }
  if (plagiarism.checkedAt) {
    plagiarism.checkedAtMoment = moment.utc(plagiarism.checkedAt).local();
  }
  return plagiarism;
};
