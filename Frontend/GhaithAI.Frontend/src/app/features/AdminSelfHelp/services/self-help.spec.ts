import { TestBed } from '@angular/core/testing';

import { SelfHelp } from './self-help';

describe('SelfHelp', () => {
  let service: SelfHelp;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SelfHelp);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
