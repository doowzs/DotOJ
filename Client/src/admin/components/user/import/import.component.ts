﻿import { Component } from "@angular/core";
import { AdminUserService } from "../../../services/user.service";
import { faUpload } from "@fortawesome/free-solid-svg-icons";

@Component({
  selector: 'app-admin-user-import',
  templateUrl: './import.component.html',
  styleUrls: ['./import.component.css']
})
export class AdminUserImportComponent {
  faUpload = faUpload;

  public input: string;
  public result: string;

  constructor(private service: AdminUserService) {
  }

  public importUsers(): void {
    this.service.importUsers(this.input)
      .subscribe(result => this.result = result);
  }
}
