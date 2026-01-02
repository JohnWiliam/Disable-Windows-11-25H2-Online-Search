## 2024-05-23 - Caching WPF Brushes
**Learning:** Repeatedly creating `new SolidColorBrush` in UI update methods generates unnecessary allocations and GC pressure.
**Action:** Cache brushes as `static readonly` fields and call `.Freeze()` on them. This makes them thread-safe and allows the system to reuse the underlying resource, significantly reducing memory churn.
