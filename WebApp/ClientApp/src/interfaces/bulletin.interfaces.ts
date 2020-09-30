import * as moment from 'moment';

export interface BulletinInfoDto {
  id: number;
  weight: number;
  content: string;
  publishAt: string | moment.Moment;
  expireAt: string | moment.Moment;
}

export interface BulletinEditDto {
  id: number;
  weight: number;
  content: string;
  publishAt: string | moment.Moment;
  expireAt: string | moment.Moment;
}
