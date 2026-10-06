class ResizeObserverMock implements ResizeObserver {
  public constructor(callback: ResizeObserverCallback) {
    void callback;
  }

  public disconnect(): void {}

  public observe(target: Element, options?: ResizeObserverOptions): void {
    void target;
    void options;
  }

  public unobserve(target: Element): void {
    void target;
  }
}

globalThis.ResizeObserver = ResizeObserverMock;

// Quasar's Screen plugin reads ScreenOrientation during installation. jsdom does
// not implement this browser API, so provide it for component tests.
const orientation = new EventTarget();
Object.assign(orientation, { type: 'landscape-primary', angle: 0 });
Object.defineProperty(globalThis.screen, 'orientation', {
  configurable: true,
  value: orientation,
});
