import { SubmissionEditDto, SubmissionInfoDto, SubmissionViewDto } from '../interfaces/submission.interfaces';

export enum VerdictStage {
  ERROR, RUNNING, ACCEPTED, REJECTED
}

export interface VerdictInfo {
  code: number;
  name: string;
  showCase: boolean;
  stage: VerdictStage;
  color: string;
  explain: string;
}

export const Verdicts: VerdictInfo[] = [
  {
    code: -2, name: 'Voided', showCase: false, stage: VerdictStage.ERROR,
    color: 'gray', explain: 'This submission is voided by a newer submission or manually.'
  },
  {
    code: -1, name: 'Service Failed', showCase: false, stage: VerdictStage.ERROR,
    color: 'gray', explain: 'An internal error occurred in the judging service.'
  },
  {
    code: 0, name: 'Pending', showCase: false, stage: VerdictStage.RUNNING,
    color: 'blue', explain: 'Your code is submitted and is waiting to be processed.'
  },
  {
    code: 1, name: 'In Queue', showCase: false, stage: VerdictStage.RUNNING,
    color: 'blue', explain: 'Your code is submitted and is waiting to be processed.'
  },
  {
    code: 2, name: 'Running', showCase: true, stage: VerdictStage.RUNNING,
    color: 'blue', explain: 'Your program is running on test cases by the judging service.'
  },
  {
    code: 3, name: 'Accepted', showCase: false, stage: VerdictStage.ACCEPTED,
    color: 'green', explain: 'Your program passed all test cases.'
  },
  {
    code: 4, name: 'Wrong Answer', showCase: true, stage: VerdictStage.REJECTED,
    color: 'red', explain: 'The output of your program does not match expected output.'
  },
  {
    code: 5, name: 'Time Limit Exceeded', showCase: true, stage: VerdictStage.REJECTED,
    color: 'red', explain: 'Your program did not terminate before hitting the time limit.'
  },
  {
    code: 6, name: 'Memory Limit Exceeded', showCase: true, stage: VerdictStage.REJECTED,
    color: 'red', explain: 'Your program hit the memory limit.'
  },
  {
    code: 7, name: 'Compilation Error', showCase: false, stage: VerdictStage.REJECTED,
    color: 'red', explain: 'Your code cannot compile.'
  },
  {
    code: 8, name: 'Runtime Error', showCase: true, stage: VerdictStage.REJECTED,
    color: 'red', explain: 'Your program did not exit normally with exit code 0.'
  }
];
