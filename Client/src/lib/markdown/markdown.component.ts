import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnChanges, Output } from '@angular/core';

import { MarkdownService } from './markdown.service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'markdown, [markdown]',
  template: '<ng-content></ng-content>',
})
export class MarkdownComponent implements OnChanges, AfterViewInit {

  protected static ngAcceptInputType_emoji: boolean | '';
  protected static ngAcceptInputType_katex: boolean | '';
  protected static ngAcceptInputType_lineHighlight: boolean | '';
  protected static ngAcceptInputType_lineNumbers: boolean | '';

  @Input() data: string | undefined;
  @Input() src: string | undefined;

  // Plugin - emoji
  @Input()
  get emoji(): boolean { return this._emoji; }
  set emoji(value: boolean) { this._emoji = this.coerceBooleanProperty(value); }

  // Plugin - katex
  @Input()
  get katex(): boolean { return this._katex; }
  set katex(value: boolean) { this._katex = this.coerceBooleanProperty(value); }

  // Plugin - lineHighlight
  @Input()
  get lineHighlight(): boolean { return this._lineHighlight; }
  set lineHighlight(value: boolean) { this._lineHighlight = this.coerceBooleanProperty(value); }
  @Input() line: string | string[] | undefined;
  @Input() lineOffset: number | undefined;

  // Plugin - lineNumbers
  @Input()
  get lineNumbers(): boolean { return this._lineNumbers; }
  set lineNumbers(value: boolean) { this._lineNumbers = this.coerceBooleanProperty(value); }
  @Input() start: number | undefined;

  // Event emitters
  @Output() error = new EventEmitter<string>();
  @Output() load = new EventEmitter<string>();
  @Output() ready = new EventEmitter<void>();

  private _emoji = false;
  private _katex = false;
  private _lineHighlight = false;
  private _lineNumbers = false;

  constructor(
    public element: ElementRef<HTMLElement>,
    public markdownService: MarkdownService,
  ) { }

  ngOnChanges(): void {
    if (this.data != null) {
      this.handleData();
      return;
    }
    if (this.src != null) {
      this.handleSrc();
      return;
    }
  }

  ngAfterViewInit(): void {
    if (!this.data && !this.src) {
      this.handleTransclusion();
    }
  }

  render(markdown: string, decodeHtml = false): void {
    this.element.nativeElement.innerHTML = this.markdownService.compile(markdown, decodeHtml, this.emoji);
    if (this.katex) {
      this.markdownService.renderKatex(this.element.nativeElement);
    }
    this.handlePlugins();
    this.markdownService.highlight(this.element.nativeElement);
    this.ready.emit();
  }

  private coerceBooleanProperty(value: boolean | ''): boolean {
    return value != null && `${value}` !== 'false';
  }

  private handleData(): void {
    this.render(this.data!);
  }

  private handleSrc(): void {
    this.markdownService
      .getSource(this.src!)
      .subscribe(
        markdown => {
          this.render(markdown);
          this.load.emit(markdown);
        },
        error => this.error.emit(error),
      );
  }

  private handleTransclusion(): void {
    this.render(this.element.nativeElement.innerHTML, true);
  }

  private handlePlugins(): void {
  }

  private setPluginClass(element: HTMLElement, plugin: string | string[]): void {
    const preElements = element.querySelectorAll('pre');
    for (let i = 0; i < preElements.length; i++) {
      const classes = plugin instanceof Array ? plugin : [plugin];
      preElements.item(i).classList.add(...classes);
    }
  }

  private setPluginOptions(element: HTMLElement, options: { [key: string]: any }): void {
    const preElements = element.querySelectorAll('pre');
    for (let i = 0; i < preElements.length; i++) {
      Object.keys(options).forEach(option => {
        const attributeValue = options[option];
        if (!!attributeValue) {
          const attributeName = this.toLispCase(option);
          preElements.item(i).setAttribute(attributeName, attributeValue.toString());
        }
      });
    }
  }

  private toLispCase(value: string): string {
    const upperChars = value.match(/([A-Z])/g);
    if (!upperChars) {
      return value;
    }
    let str = value.toString();
    for (let i = 0, n = upperChars.length; i < n; i++) {
      str = str.replace(new RegExp(upperChars[i]), '-' + upperChars[i].toLowerCase());
    }
    if (str.slice(0, 1) === '-') {
      str = str.slice(1);
    }
    return str;
  }
}
