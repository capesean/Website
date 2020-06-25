import { Directive, HostListener, ElementRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Directive({
    //selector: 'input[type=file]',
    selector: '[myTest]',
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: AppFileInputDirective, multi: true }
    ]
})
export class AppFileInputDirective /*implements ControlValueAccessor*/ {
    @HostListener('change', ['$event.target.files']) onChange = (_: any) => { debugger; };
    @HostListener('blur') onTouched = () => { debugger; };

    writeValue(value: any) { debugger; }
    registerOnChange(fn: (_: any) => void) { this.onChange = fn; debugger; }
    registerOnTouched(fn: () => void) { this.onTouched = fn; debugger; }

    constructor(el: ElementRef) {
        debugger;
        el.nativeElement.style.backgroundColor = 'yellow';
    }

}
