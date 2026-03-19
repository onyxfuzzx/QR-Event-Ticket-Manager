export interface EventFormField {
  key: string;
  label: string;
  type: 'text' | 'email' | 'phone' | 'number' | 'dropdown' | 'textarea';
  required: boolean;
  options?: string[];
}
