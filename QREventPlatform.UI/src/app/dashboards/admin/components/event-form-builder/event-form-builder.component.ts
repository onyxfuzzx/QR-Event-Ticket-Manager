import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventFormField } from '../../../../core/models/event-form-field';
import { EventFormService } from '../../../../core/services/event-form.service';

@Component({
  selector: 'app-event-form-builder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './event-form-builder.component.html',
  styleUrls: ['./event-form-builder.component.scss']
})
export class EventFormBuilderComponent implements OnInit, OnChanges {

  @Input() eventId!: string;

  fields: EventFormField[] = [];
  saving = false;
  saveSuccess = false;
  saveError = '';
  activeFieldIndex: number | null = null;
  showPreview = false;
  dragIndex: number | null = null;
  dragOverIndex: number | null = null;

  // Default locked fields
  readonly lockedKeys = ['firstName', 'lastName', 'email', 'phone'];

  // Type icon mapping
  readonly typeIcons: Record<string, string> = {
    text: 'Aa',
    email: '@',
    phone: '#',
    number: '123',
    dropdown: '▾',
    textarea: '¶'
  };

  constructor(private formService: EventFormService) {}

  ngOnInit() {
    this.loadForm();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['eventId'] && !changes['eventId'].firstChange) {
      this.loadForm();
    }
  }

  loadForm() {
    this.formService.getForm(this.eventId).subscribe((schema: any) => {
      if (schema && schema.length) {
        this.fields = schema;
      } else {
        this.fields = [
          { key: 'firstName', label: 'First Name', type: 'text', required: true },
          { key: 'lastName', label: 'Last Name', type: 'text', required: true },
          { key: 'email', label: 'Email', type: 'email', required: true },
          { key: 'phone', label: 'Phone', type: 'phone', required: true }
        ];
      }
      this.activeFieldIndex = null;
    });
  }

  isLocked(field: EventFormField): boolean {
    return this.lockedKeys.includes(field.key);
  }

  toggleField(index: number) {
    this.activeFieldIndex = this.activeFieldIndex === index ? null : index;
  }

  addField() {
    const newField: EventFormField = {
      key: '',
      label: '',
      type: 'text',
      required: false
    };
    this.fields.push(newField);
    this.activeFieldIndex = this.fields.length - 1;
  }

  removeField(index: number) {
    this.fields.splice(index, 1);
    this.activeFieldIndex = null;
  }

  moveField(from: number, to: number) {
    if (to < 0 || to >= this.fields.length) return;
    const [field] = this.fields.splice(from, 1);
    this.fields.splice(to, 0, field);
    this.activeFieldIndex = to;
  }

  duplicateField(index: number) {
    const original = this.fields[index];
    const copy: EventFormField = {
      ...original,
      key: original.key + '_copy',
      label: original.label + ' (Copy)'
    };
    this.fields.splice(index + 1, 0, copy);
    this.activeFieldIndex = index + 1;
  }

  autoGenerateKey(field: EventFormField) {
    if (!this.isLocked(field) && field.label) {
      field.key = field.label
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '_')
        .replace(/^_|_$/g, '');
    }
  }

  addOption(field: EventFormField) {
    if (!field.options) field.options = [];
    field.options.push('');
  }

  removeOption(field: EventFormField, optIndex: number) {
    if (field.options) {
      field.options.splice(optIndex, 1);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }

  // Drag & Drop
  onDragStart(index: number) {
    this.dragIndex = index;
  }

  onDragOver(event: DragEvent, index: number) {
    event.preventDefault();
    this.dragOverIndex = index;
  }

  onDragLeave() {
    this.dragOverIndex = null;
  }

  onDrop(index: number) {
    if (this.dragIndex !== null && this.dragIndex !== index) {
      this.moveField(this.dragIndex, index);
    }
    this.dragIndex = null;
    this.dragOverIndex = null;
  }

  onDragEnd() {
    this.dragIndex = null;
    this.dragOverIndex = null;
  }

  save() {
    // Validate
    const invalid = this.fields.find(f => !f.key || !f.label);
    if (invalid) {
      this.saveError = 'All fields must have a label and key';
      setTimeout(() => this.saveError = '', 3000);
      return;
    }

    // Check for duplicate keys
    const keys = this.fields.map(f => f.key);
    const dupes = keys.filter((k, i) => keys.indexOf(k) !== i);
    if (dupes.length) {
      this.saveError = `Duplicate field key: "${dupes[0]}"`;
      setTimeout(() => this.saveError = '', 3000);
      return;
    }

    this.saving = true;
    this.saveError = '';
    this.formService.saveForm(this.eventId, this.fields).subscribe({
      next: () => {
        this.saving = false;
        this.saveSuccess = true;
        setTimeout(() => this.saveSuccess = false, 3000);
      },
      error: () => {
        this.saving = false;
        this.saveError = 'Failed to save form. Please try again.';
        setTimeout(() => this.saveError = '', 3000);
      }
    });
  }
}
