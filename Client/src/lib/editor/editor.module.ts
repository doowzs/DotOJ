import { ModuleWithProviders, NgModule } from '@angular/core';
import { EditorComponent } from './editor.component';
import { ShortcutDirective } from './shortcut.directive';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

const sharedDeclarations = [
  EditorComponent,
  ShortcutDirective
];

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    FontAwesomeModule
  ],
  exports: sharedDeclarations,
  declarations: sharedDeclarations
})
export class EditorModule {
  static forRoot(): ModuleWithProviders<EditorModule> {
    return {
      ngModule: EditorModule
    };
  }

  static forChild(): ModuleWithProviders<EditorModule> {
    return {
      ngModule: EditorModule
    };
  }
}
