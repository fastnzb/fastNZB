import { Component } from '@angular/core';

@Component({
    selector: 'user',
    template: '<h1>My First Angular 2 App</h1>'
})

export class User {
    sessionId: string;
    userId: string;
    userName: string;    
    apiKey: string;
    nl: boolean;
}
