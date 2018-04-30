import { Injectable, ErrorHandler } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/do';
 
import { BaseService } from '../../shared/base.service';

@Injectable()
export class DataService extends BaseService {

    constructor(private http: HttpClient) {
        super();
    }

    getClaims(): Observable<string[]> {
        return this.http.get<string[]>(`${this.apiUrl}/claims`);
    }
}
