import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReportReviewComponent } from './report-review/report-review.component';

@Component({
  selector: 'app-session-report',
  standalone: true,
  imports: [CommonModule, ReportReviewComponent],
  templateUrl: './session-report.html',
  styleUrls: ['./session-report.css']
})
export class SessionReportComponent implements OnInit {
  sessionId = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';
  }

  goBack(): void {
    this.router.navigate(['/clinical-session', this.sessionId, 'transcript']);
  }
}
