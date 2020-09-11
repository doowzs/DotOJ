import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';

import { ContestService } from '../../../services/contest.service';
import { RegistrationInfoDto } from '../../../interfaces/registration.interfaces';
import { ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ProblemInfoDto } from '../../../interfaces/problem.interfaces';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-contest-standings',
  templateUrl: './standings.component.html',
  styleUrls: ['./standings.component.css']
})
export class ContestStandingsComponent implements OnInit {
  public loading = true;
  public contestId: number;
  public contest: ContestViewDto;
  public registrations: RegistrationInfoDto[];

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: ContestService
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.title.setTitle(contest.title + ' - Standings');
        this.loadRegistrations();
      });
  }

  public loadRegistrations() {
    this.loading = true;
    this.service.getRegistrations(this.contestId)
      .subscribe(registrations => {
        this.registrations = registrations.sort((a, b) => {
          const scoreA = this.getTotalScore(a), scoreB = this.getTotalScore(b);
          if (scoreA !== scoreB) {
            return scoreB - scoreA; // descending in score order
          } else {
            return this.getTotalPenalties(a) - this.getTotalPenalties(b); // ascending in penalty order
          }
        });
        this.loading = false;
      });
  }

  public getProblemPenalty(registration: RegistrationInfoDto, problem: ProblemInfoDto): number {
    const statistic = registration.statistics.find(s => s.problemId === problem.id);
    if (statistic && statistic.acceptedAt) {
      const minutes = (statistic.acceptedAt as moment.Moment).diff(this.contest.beginTime, 'minutes');
      return minutes + 20 * statistic.penalties;
    } else {
      return 0;
    }
  }

  public getProblemItem(registration: RegistrationInfoDto, problem: ProblemInfoDto): string[] | null {
    const statistic = registration.statistics.find(s => s.problemId === problem.id);
    if (statistic) {
      if (statistic.acceptedAt) {
        const minutes = (statistic.acceptedAt as moment.Moment).diff(this.contest.beginTime, 'minutes');
        const penalties = statistic.penalties + 1;
        return [minutes.toString(), penalties.toString() + ' ' + (penalties === 1 ? 'try' : 'tries')];
      } else {
        return ['-', (statistic.penalties).toString() + ' ' + (statistic.penalties === 1 ? 'try' : 'tries')];
      }
    } else {
      return null;
    }
  }

  public getTotalScore(registration: RegistrationInfoDto): number {
    return registration.statistics.reduce((total, statistic) => total + statistic.score, 0);
  }

  public getTotalPenalties(registration: RegistrationInfoDto): number {
    return this.contest.problems.reduce((total, problem) => total + this.getProblemPenalty(registration, problem), 0);
  }
}
