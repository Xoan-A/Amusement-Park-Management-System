import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumToDisplay',
  standalone: true,
})
export class EnumToDisplayPipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';

    return value
      .replace(/([A-Z])/g, ' $1')
      .trim()
      .replace(/^\w/, (c) => c.toUpperCase());
  }
}
