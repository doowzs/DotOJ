import {Component} from "@angular/core";

@Component({
  selector: 'app-assignment-view',
  templateUrl: './assignment-view.component.html'
})
export class AssignmentViewComponent {
  public assignment: AssignmentViewDto;

  constructor() {
    //
  }
}

interface AssignmentViewDto {
  id: number;
}
