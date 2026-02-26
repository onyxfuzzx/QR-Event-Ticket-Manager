import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { EventRegisterService } from '../../core/services/event-register.service';


@Component({
  standalone: true,
  selector: 'app-event-register',
  imports: [CommonModule, FormsModule],
  templateUrl: './event-register.component.html'
})
export class EventRegisterComponent implements OnInit {
  eventId!: string;
  eventName = '';
  fields: any[] = [];
  formData: Record<string, string> = {};
  success = false;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private service: EventRegisterService
  ) { }

  ngOnInit() {
    this.eventId = this.route.snapshot.paramMap.get('eventId')!;
    this.loadPage();
  }

  loadPage() {
    this.loading = true;

    this.service.getEvent(this.eventId).subscribe(ev => {
      this.eventName = ev.name;
    });

    this.service.getForm(this.eventId).subscribe(form => {
      this.fields = form;
      this.loading = false;
    });
  }

  submit() {
    this.service.submitForm(this.eventId, this.formData)
      .subscribe(() => this.success = true);
  }
}
