import * as moment from 'moment';

import { ProblemInfoDto } from './problem.interfaces';

export enum ContestMode {
  Practice = 0,  // Practice or exam
  OneShot = 1,   // OI (judge only once)
  UntilFail = 2, // ICPC (until first fail)
  SampleOnly = 3 // CF (judge samples only)
}

export interface ContestInfoDto {
  id: number;
  title: string;
  isPublic: boolean;
  mode: ContestMode;
  beginTime: string|moment.Moment;
  endTime: string|moment.Moment;
  registered: boolean;
}

export interface ContestViewDto {
  id: number;
  title: string;
  description: string;
  isPublic: boolean;
  mode: ContestMode;
  beginTime: string|moment.Moment;
  endTime: string|moment.Moment;
  problems: ProblemInfoDto[];
  notices: any[];
}
