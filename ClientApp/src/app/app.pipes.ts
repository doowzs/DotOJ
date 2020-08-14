import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'abbreviate'})
export class AbbreviatePipe implements PipeTransform {
  transform(value: string): string {
    return value.match(/\b[A-Z]/g).join('');
  }
}
