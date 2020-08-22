import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {MatSnackBar} from '@angular/material/snack-bar';

@Component({
  selector: 'app-admin-assignment-create',
  templateUrl: './create.component.html'
})
export class AdminAssignmentCreateComponent {
  constructor(
    private router: Router,
    private snackBar: MatSnackBar
  ) {
  }

  public created(id: number) {
    this.snackBar.open(`Assignment ${id} created.`, 'OK');
    this.router.navigate(['/admin/assignment', id]);
  }
}
