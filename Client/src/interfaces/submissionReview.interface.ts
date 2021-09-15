import {SubmissionViewDto} from "./submission.interfaces";

export interface SubmissionReviewInfoDto {
  contestantId: string;
  score: number;
  comments: string;
  submission: SubmissionViewDto;
}
