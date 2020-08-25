export interface ProblemInfoDto {
  id: number;
  contestId: number;
  label: string; // added in client service
  title: string;
  solved: boolean;
}
