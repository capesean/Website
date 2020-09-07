import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';

@Pipe({
    name: 'momentPipe'
})
export class MomentPipe implements PipeTransform {
    transform(value: Date | moment.Moment, ...args: string[]): string {
        if (value === undefined || value === null) return '';
        const [format] = args;
        return moment(value).format(format);
    }
}
