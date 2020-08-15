import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'abbreviate'})
export class AbbreviatePipe implements PipeTransform {
  transform(value: string): string {
    return value.match(/\b[A-Z]/g).join('');
  }
}

@Pipe({name: 'countBytes'})
export class CountBytesPipe implements PipeTransform {
  transform(value: string): number {
    return new Blob([value]).size;
  }
}
