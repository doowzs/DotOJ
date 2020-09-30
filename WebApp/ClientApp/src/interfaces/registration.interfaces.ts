import * as moment from 'moment';

export interface ProblemStatistics {
  problemId: number;
  penalties: number;
  acceptedAt: moment.Moment | string;
  score: number;
}

export interface RegistrationInfoDto {
  contestId: number;
  userId: string;
  contestantId: string;
  contestantName: string;
  isParticipant: boolean;
  isContestManager: boolean;
  statistics: ProblemStatistics[];
  rank?: number;
  solved?: number;
  score?: number;
  penalties?: number;
}
