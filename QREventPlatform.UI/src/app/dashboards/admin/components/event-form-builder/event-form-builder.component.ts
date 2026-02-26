import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventFormField } from '../../../../core/models/event-form-field';
import { EventFormService } from '../../../../core/services/event-form.service';



@Component({
  selector: 'app-event-form-builder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './event-form-builder.component.html',
  styleUrl: './event-form-builder.component.scss'
})
export class EventFormBuilderComponent implements OnInit {

  @Input() eventId!: string;

  fields: EventFormField[] = [];

  constructor(private formService: EventFormService) { }

  ngOnInit() {
    this.formService.getForm(this.eventId).subscribe(schema => {
      if (schema) {
        this.fields = schema;
      } else {
        this.fields = [
          { key: 'firstName', label: 'First Name', type: 'text', required: true },
          { key: 'lastName', label: 'Last Name', type: 'text', required: true },
          { key: 'email', label: 'Email', type: 'email', required: true },
          { key: 'phone', label: 'Phone', type: 'phone', required: true }
        ];
      }
    });
  }

  trackByIndex(index: number): number {
    return index;
  }


  addField() {
    this.fields.push({
      key: '',
      label: '',
      type: 'text',
      required: false
    });
  }

  removeField(i: number) {
    this.fields.splice(i, 1);
  }

  save() {
    this.formService.saveForm(this.eventId, this.fields).subscribe(() => {
      alert('Event form saved successfully');
    });
  }
}
