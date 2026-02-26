import { TestBed } from '@angular/core/testing';

import { AdminSignalrService } from './admin-signalr.service';

describe('AdminSignalrService', () => {
  let service: AdminSignalrService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminSignalrService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
