export enum AssignmentMode {
  Practice = 0,  // Practice or exam
  OneShot = 1,   // OI (judge only once)
  UntilFail = 2, // ICPC (until first fail)
  SampleOnly = 3 // CF (judge samples only)
}

export interface AssignmentInfoDto {
  id: number;
  title: number;
  isPublic: boolean;
  mode: AssignmentMode;
  beginTime: Date;
  endTime: Date;
  registered: boolean;
}
