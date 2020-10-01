import { ModuleWithProviders, NgModule } from '@angular/core';
import { VerdictComponent } from './verdict.component';
import { CommonModule } from '@angular/common';

const sharedDeclarations = [
  VerdictComponent
];

@NgModule({
  imports: [
    CommonModule
  ],
  exports: sharedDeclarations,
  declarations: sharedDeclarations
})
export class VerdictModule {
  static forRoot(): ModuleWithProviders<VerdictModule> {
    return {
      ngModule: VerdictModule
    };
  }

  static forChild(): ModuleWithProviders<VerdictModule> {
    return {
      ngModule: VerdictModule
    };
  }
}
