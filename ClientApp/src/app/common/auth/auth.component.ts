import { Component, OnInit, Inject } from '@angular/core'
import { DOCUMENT } from '@angular/common';

@Component({
   selector: 'auth-root',
   templateUrl: './auth.component.html'
})
export class AuthComponent implements OnInit {

   constructor(
      @Inject(DOCUMENT) private document: Document
   ) { }

   ngOnInit(): void {
      this.document.body.classList.add('login');
   }

}
