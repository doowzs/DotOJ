import { Directive, EventEmitter, HostListener, Output } from '@angular/core';

@Directive({
  selector: '[shortcutResponsive]'
})
export class ShortcutDirective {
  @Output() ctrlS = new EventEmitter();

  @HostListener('document:keydown.control.s', ['$event'])
  onCtrlS(event: KeyboardEvent) {
    event.preventDefault();
    this.ctrlS.emit();
  }
}
