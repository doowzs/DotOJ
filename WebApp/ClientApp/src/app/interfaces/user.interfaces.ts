export interface UserInfoDto {
  id: string;
  email: string;
  userName: string;
  contestantId: string;
  contestantName: string;
}

export interface UserEditDto {
  id: string;
  email: string;
  userName: string;
  contestantId: string;
  contestantName: string;
  isAdministrator: boolean;
  isUserManager: boolean;
  isContestManager: boolean;
  isSubmissionManager: boolean;
}
