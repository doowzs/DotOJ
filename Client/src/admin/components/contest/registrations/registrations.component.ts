﻿import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PaginatedList } from '../../../../interfaces/pagination.interfaces';
import { UserInfoDto } from '../../../../interfaces/user.interfaces';
import { ContestEditDto } from '../../../../interfaces/contest.interfaces';
import { RegistrationInfoDto } from '../../../../interfaces/registration.interfaces';
import { AdminContestService } from '../../../services/contest.service';
import { AdminUserService } from '../../../services/user.service';
import { faCopy, faPlus, faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-contest-registrations',
  templateUrl: './registrations.component.html',
  styleUrls: ['./registrations.component.css']
})
export class AdminContestRegistrationsComponent implements OnInit {
  faCopy = faCopy;
  faPlus = faPlus;
  faTimes = faTimes;

  public list: PaginatedList<UserInfoDto>;
  public pageIndex: number;

  public contestId: number;
  public contest: ContestEditDto;
  public registrations: RegistrationInfoDto[] = [];

  public isParticipant = true;
  public isContestManager = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: AdminUserService,
    private contestService: AdminContestService
  ) {
    this.contestId = this.route.snapshot.parent.params.contestId;
    this.pageIndex = this.route.snapshot.queryParams.pageIndex;
  }

  ngOnInit() {
    this.loadUsers();
    this.contestService.getSingle(this.contestId)
      .subscribe(contest => this.contest = contest);
    this.contestService.getRegistrations(this.contestId)
      .subscribe(registrations => this.registrations = registrations);
  }

  public loadUsers() {
    this.userService.getPaginatedList(this.pageIndex ?? 1)
      .subscribe(list => this.list = list);
  }

  public onPageIndexChange(value: number) {
    this.pageIndex = value;
    this.router.navigate(['/admin/contest', this.contestId, 'registrations'], {
      queryParams: {
        pageIndex: value
      }
    });
    this.loadUsers();
  }

  public getUserRegistrationType(userId: string): string[] {
    const registration = this.registrations.find(r => r.userId === userId);
    if (!registration) {
      return null;
    } else {
      return registration.isParticipant ? ['text-primary', 'Participant']
        : (registration.isContestManager ? ['text-danger', 'Manager'] : ['text-secondary', 'Observer']);
    }
  }

  public addRegistration(userId: string) {
    this.contestService.addRegistrations(this.contest.id, [userId], this.isParticipant, this.isContestManager)
      .subscribe(registrations => {
        this.registrations = this.registrations.concat(registrations);
      });
  }

  public removeRegistration(userId: string) {
    this.contestService.removeRegistrations(this.contest.id, [userId])
      .subscribe(() => {
        this.registrations.splice(this.registrations.findIndex(r => r.userId === userId), 1);
      });
  }

  public copyRegistrations(fromId: string) {
    if (confirm(`Are you sure to copy registrations from contest #${fromId} to #${this.contestId}?`
      + ` This will override all existing registrations.`)) {
      this.contestService.copyRegistrations(this.contestId, Number(fromId))
        .subscribe(registrations => {
          this.registrations = registrations;
        });
    }
  }
}
