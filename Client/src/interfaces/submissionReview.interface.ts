import {SubmissionViewDto} from "./submission.interfaces";

export interface SubmissionReviewInfoDto {
  score: number;
  comments: string;
  submission: SubmissionViewDto;
}
