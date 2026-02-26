export interface EventFormField {
  key: string;
  label: string;
  type: 'text' | 'email' | 'phone' | 'number' | 'dropdown';
  required: boolean;
  options?: string[];
}
