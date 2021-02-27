import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatetimeComponent } from './datetime.component';

const sharedDeclarations = [
  DatetimeComponent
];

@NgModule({
  exports: sharedDeclarations,
  imports: [
    FormsModule
  ],
  declarations: sharedDeclarations
})
export class DatetimeModule {
  static forRoot(): ModuleWithProviders<DatetimeModule> {
    return {
      ngModule: DatetimeModule
    };
  }

  static forChild(): ModuleWithProviders<DatetimeModule> {
    return {
      ngModule: DatetimeModule
    };
  }
}
