import { Component, OnInit } from '@angular/core';
import { BulletinService } from '../../../services/bulletin.service';
import { BulletinInfoDto } from '../../../interfaces/bulletin.interfaces';

@Component({
  selector: 'app-welcome-bulletins',
  templateUrl: './bulletins.component.html',
  styleUrls: ['./bulletins.component.css']
})
export class WelcomeBulletinsComponent implements OnInit {
  public bulletins: BulletinInfoDto[];

  constructor(private service: BulletinService) {
  }

  ngOnInit() {
    this.service.getList()
      .subscribe(bulletins => this.bulletins = bulletins.sort((a, b) => a.weight - b.weight));
  }
}
