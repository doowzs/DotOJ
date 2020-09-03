import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'noComma'
})
export class NoCommaPipe implements PipeTransform {
  transform(val: number | string): string {
    if (!!val) {
      return val.toString().replace(/,/g, '');
    } else {
      return '';
    }
  }
}
