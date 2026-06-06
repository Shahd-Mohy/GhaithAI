import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

/**
 * Lightweight bus — mood/journal components call moodLogged()
 * after a successful save; the dashboard listens and reloads.
 */
@Injectable({ providedIn: 'root' })
export class DashboardRefreshService {
    private readonly _refresh$ = new Subject<void>();
    readonly refresh$ = this._refresh$.asObservable();

    moodLogged(): void { this._refresh$.next(); }
}