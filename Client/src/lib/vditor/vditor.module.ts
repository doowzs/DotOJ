import { ModuleWithProviders, NgModule } from '@angular/core';
import { VditorComponent } from './vditor.component';

const sharedDeclarations = [
  VditorComponent
];

@NgModule({
  exports: sharedDeclarations,
  declarations: sharedDeclarations
})
export class VditorModule {
  static forRoot(): ModuleWithProviders<VditorModule> {
    return {
      ngModule: VditorModule
    };
  }

  static forChild(): ModuleWithProviders<VditorModule> {
    return {
      ngModule: VditorModule
    };
  }
}
