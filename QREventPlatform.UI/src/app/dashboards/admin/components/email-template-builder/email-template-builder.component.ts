import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { SafeHtmlPipe } from '../../../../core/pipes/safe-html.pipe';

interface TemplateBlock {
  type: 'header' | 'details' | 'ticket' | 'text' | 'footer';
  content: any;
}

@Component({
  selector: 'app-email-template-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, SafeHtmlPipe],
  templateUrl: './email-template-builder.component.html',
  styleUrls: ['./email-template-builder.component.scss']
})
export class EmailTemplateBuilderComponent implements OnInit {
  @Input() eventId!: string;

  blocks: TemplateBlock[] = [];
  saving = false;
  testing = false;
  testEmail = '';
  showPreview = false;

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.loadTemplate();
  }

  loadTemplate() {
    this.adminService.getEventEmailTemplate(this.eventId).subscribe(res => {
      try {
        this.blocks = JSON.parse(res.layoutJson) || this.getDefaultBlocks();
      } catch {
        this.blocks = this.getDefaultBlocks();
      }
    });
  }

  getDefaultBlocks(): TemplateBlock[] {
    return [
      { type: 'header', content: { title: 'Your Digital Ticket', bgColor: '#6366f1', textColor: '#ffffff' } },
      { type: 'details', content: { showDate: true, showLocation: true } },
      { type: 'ticket', content: { mode: 'button', btnText: 'View QR Code', btnColor: '#6366f1' } },
      { type: 'text', content: { body: 'Hi there! Your ticket is ready. Please present the QR code at the entrance.' } },
      { type: 'footer', content: { text: 'This is an automated message. Please do not reply.' } }
    ];
  }

  addBlock(type: TemplateBlock['type']) {
    const defaultContents = {
      header: { title: 'New Header', bgColor: '#6366f1', textColor: '#ffffff' },
      details: { showDate: true, showLocation: true },
      ticket: { mode: 'button', btnText: 'View QR Code', btnColor: '#6366f1' },
      text: { body: 'Extra information goes here...' },
      footer: { text: 'Platform Footer' }
    };
    this.blocks.push({ type, content: defaultContents[type] });
  }

  removeBlock(index: number) {
    this.blocks.splice(index, 1);
  }

  moveBlock(index: number, dir: number) {
    const target = index + dir;
    if (target < 0 || target >= this.blocks.length) return;
    const temp = this.blocks[index];
    this.blocks[index] = this.blocks[target];
    this.blocks[target] = temp;
  }

  saveTemplate() {
    this.saving = true;
    const html = this.renderHtml();
    this.adminService.saveEventEmailTemplate(this.eventId, {
      layoutJson: JSON.stringify(this.blocks),
      htmlContent: html
    }).subscribe(() => {
      this.saving = false;
      alert('Template saved successfully!');
    });
  }

  sendTest() {
    if (!this.testEmail) return;
    this.testing = true;
    this.adminService.testEmailTemplate(this.eventId, {
      toEmail: this.testEmail,
      htmlContent: this.renderHtml()
    }).subscribe(() => {
      this.testing = false;
      alert('Test email sent!');
    });
  }

  renderHtml(): string {
    let bodyHtml = '';
    
    this.blocks.forEach(block => {
      switch (block.type) {
        case 'header':
          bodyHtml += `
            <div style="background-color: ${block.content.bgColor}; padding: 30px; text-align: center;">
              <h2 style="color: ${block.content.textColor}; margin: 0; font-size: 24px;">${block.content.title}</h2>
            </div>`;
          break;
        case 'details':
          bodyHtml += `
            <div style="padding: 24px 30px; background-color: #ffffff;">
              <h3 style="margin: 0 0 12px 0; color: #1e293b; font-size: 20px;">{{event_name}}</h3>
              ${block.content.showDate ? `
                <div style="display: flex; align-items: center; margin-bottom: 8px; color: #64748b;">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 8px;"><rect width="18" height="18" x="3" y="4" rx="2" ry="2"/><line x1="16" x2="16" y1="2" y2="6"/><line x1="8" x2="8" y1="2" y2="6"/><line x1="3" x2="21" y1="10" y2="10"/></svg>
                  <span style="font-size: 14px;">{{event_date}}</span>
                </div>` : ''}
              ${block.content.showLocation ? `
                <div style="display: flex; align-items: center; color: #64748b;">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 8px;"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>
                  <span style="font-size: 14px;">{{location}}</span>
                </div>` : ''}
            </div>`;
          break;
        case 'text':
          bodyHtml += `
            <div style="padding: 10px 30px; background-color: #ffffff;">
              <p style="font-size: 16px; line-height: 1.6; color: #334155;">${block.content.body}</p>
            </div>`;
          break;
        case 'ticket':
          bodyHtml += `
            <div style="padding: 30px; background-color: #ffffff; text-align: center;">
              <div style="background-color: #f1f5f9; border-radius: 8px; padding: 20px; border: 1px dashed #cbd5e1; text-align: center; margin-bottom: 24px;">
                <p style="font-size: 14px; color: #64748b; margin: 0 0 8px 0; text-transform: uppercase;">Ticket ID</p>
                <code style="font-size: 18px; font-weight: bold; color: #1e293b;">{{ticket_id}}</code>
              </div>
              ${block.content.mode === 'button' ? 
                `<a href="{{qr_code}}" style="display: inline-block; background-color: ${block.content.btnColor}; color: #ffffff; padding: 14px 28px; border-radius: 8px; text-decoration: none; font-weight: bold;">${block.content.btnText}</a>` :
                `<img src="{{qr_code}}" style="width: 200px; height: 200px;" alt="QR Code" />`
              }
            </div>`;
          break;
        case 'footer':
          bodyHtml += `
            <div style="background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px; text-align: center;">
              <p style="font-size: 12px; color: #94a3b8; margin: 0;">${block.content.text}</p>
            </div>`;
          break;
      }
    });

    return `
      <div style="font-family: sans-serif; background-color: #f1f5f9; padding: 40px 10px;">
        <div style="max-width: 600px; margin: 0 auto; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);">
          ${bodyHtml}
        </div>
      </div>
    `;
  }
}
