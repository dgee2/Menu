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
