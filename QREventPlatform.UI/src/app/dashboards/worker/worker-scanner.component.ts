import {
  Component,
  EventEmitter,
  Output,
  OnDestroy,
  AfterViewInit,
  ViewChild,
  ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserMultiFormatReader } from '@zxing/browser';

@Component({
  standalone: true,
  selector: 'app-worker-scanner',
  imports: [CommonModule],
  templateUrl: './worker-scanner.component.html',
  styleUrls: ['./worker-scanner.component.scss']
})
export class WorkerScannerComponent implements AfterViewInit, OnDestroy {

  @ViewChild('video', { static: true })
  video!: ElementRef<HTMLVideoElement>;

  @Output() scanned = new EventEmitter<string>();
  @Output() close = new EventEmitter<void>();

  private reader = new BrowserMultiFormatReader();
  private stream: MediaStream | null = null;

  scanning = false;
  error = '';

  async ngAfterViewInit() {
    this.startCamera();
  }

  async startCamera() {
    try {
      this.scanning = true;

      this.stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });

      this.video.nativeElement.srcObject = this.stream;
      await this.video.nativeElement.play();

      this.reader.decodeFromVideoElement(
        this.video.nativeElement,
        (result) => {
          if (result) {
            this.scanned.emit(result.getText());
            this.stopCamera();
          }
        }
      );

    } catch {
      this.error = 'Camera permission denied';
      this.scanning = false;
    }
  }

  stopCamera() {
    this.scanning = false;

    if (this.stream) {
      this.stream.getTracks().forEach(t => t.stop());
      this.stream = null;
    }

    this.close.emit();
  }

  ngOnDestroy() {
    this.stopCamera();
  }
}
