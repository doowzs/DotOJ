import {SubmissionViewDto} from "./submission.interfaces";

export interface SubmissionReviewInfoDto {
  contestantId: string;
  score: number;
  timeComplexity: string;
  spaceComplexity: string;
  codeSpecification: string;
  comments: string;
  submission: SubmissionViewDto;
}
