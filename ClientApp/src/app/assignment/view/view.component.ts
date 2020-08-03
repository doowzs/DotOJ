import {Component, OnInit, OnDestroy} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ProgressBarMode} from '@angular/material/progress-bar/progress-bar';
import {timer, Subscription} from 'rxjs';
import {DateTime} from 'luxon';

import {AssignmentService} from '../assignment.service';
import {AssignmentViewDto} from '../../app.interfaces';

@Component({
  selector: 'app-assignment-view',
  templateUrl: './view.component.html'
})
export class AssignmentViewComponent implements OnInit, OnDestroy {
  public id: number;
  public assignment: AssignmentViewDto;
  public progressBarMode: ProgressBarMode = 'indeterminate';
  public progressBarValue: number;
  public progressBarSubscribe: Subscription;
  public activeRouteLink: string;
  public navigationLinks: any[] = [
    {link: '', label: 'Content'}
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: AssignmentService
  ) {
    this.id = this.route.snapshot.params.assignmentId;
    this.service.getSingle(this.id)
      .subscribe(assignment => {
        this.assignment = assignment;
        this.startCountdown();
      }, error => console.error(error));
  }

  ngOnInit() {
    this.activeRouteLink = this.route.firstChild.routeConfig.path;
    this.router.events.subscribe(() => {
      this.activeRouteLink = this.route.firstChild.routeConfig.path;
    });
  }

  ngOnDestroy() {
    if (this.progressBarSubscribe) {
      this.progressBarSubscribe.unsubscribe();
    }
  }

  private startCountdown() {
    const begin = DateTime.fromISO(this.assignment.beginTime);
    const end = DateTime.fromISO(this.assignment.endTime);
    const updateCountdown = () => {
      const now = DateTime.local();
      this.progressBarMode = 'determinate';
      this.progressBarValue = (now - begin) * 100 / (end - begin);
      if (DateTime.local() >= end && this.progressBarSubscribe) {
        this.progressBarSubscribe.unsubscribe();
      }
    };
    this.progressBarSubscribe = timer(0, 1000).subscribe(() => updateCountdown());
  }

  public navigateToLink(link: string) {
    if (link === '') {
      this.router.navigate(['/assignment', this.id]);
    } else {
      this.router.navigate(['/assignment', this.id, link]);
    }
  }
}
