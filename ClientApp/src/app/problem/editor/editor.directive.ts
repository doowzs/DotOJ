import {Directive, EventEmitter, HostListener, Output} from '@angular/core';

@Directive({
  selector: '[appCodeEditor]'
})
export class CodeEditorDirective {
  @Output() ctrlS = new EventEmitter();

  @HostListener('document:keydown.control.s', ['$event'])
  onCtrlS(event: KeyboardEvent) {
    event.preventDefault();
    this.ctrlS.emit();
  }
}
