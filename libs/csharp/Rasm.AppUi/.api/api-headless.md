# [RASM_APPUI_API_HEADLESS]

`Avalonia.Headless` boots a windowless Avalonia platform with a manually-ticked render timer, synthetic input injection, and rendered-frame capture; `Avalonia.Headless.XUnit` wraps that platform in xUnit.v3 fact/theory discoverers that marshal each test body onto the Avalonia dispatcher thread. Together they are the UI-evidence rail: a `[AvaloniaFact]` runs view-model + view interaction on a real dispatcher, drives it with `KeyPress`/`MouseDown`/`DragDrop`, advances frames with `ForceRenderTimerTick`, and asserts on a `WriteableBitmap` from `CaptureRenderedFrame`. This catalog marks the public test-author surface against the internal platform plumbing so the capture/evidence design pages compose the supported entrypoints and never reach for `internal` window impls.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Headless`
- package: `Avalonia.Headless`
- license: `MIT`
- assembly: `Avalonia.Headless` (AnyCPU IL, managed-only)
- build-floor: `net10.0` (consumer-bound; `net8.0` fallback present, not bound)
- namespace: `Avalonia.Headless`
- asset: runtime library; test-scoped restore under `tests/csharp/libs/Rasm.AppUi`
- rail: evidence
- depends: `Avalonia.Base` (platform interfaces, `WriteableBitmap`, input/raw types) plus `HarfBuzzSharp` (text shaping; `UseHeadless` always installs HarfBuzz). Pixel evidence additionally requires `Avalonia.Skia` when `UseHeadlessDrawing=false`.

[PACKAGE_SURFACE]: `Avalonia.Headless.XUnit`
- package: `Avalonia.Headless.XUnit`
- license: `MIT`
- assembly: `Avalonia.Headless.XUnit`
- build-floor: `net10.0`
- namespace: `Avalonia.Headless.XUnit`
- asset: runtime library; test-scoped
- rail: evidence
- depends: `xunit.v3.extensibility.core` / `xunit.v3.common` — the attributes derive from xunit.v3 `FactAttribute`/`TheoryAttribute` and implement `ITestFrameworkAttribute`. This is why the central manifest floors `xunit.v3.mtp-v2` at `3.2.2`: `Avalonia.Headless.XUnit 12.0.5` binds that extensibility surface.

## [02]-[PUBLIC_TYPES]

[PLATFORM_TYPES]: public platform, options, and session surfaces (`Avalonia.Headless`)
- rail: evidence

| [INDEX] | [SYMBOL]                             | [SIGNATURE]                                                      | [RAIL]            |
| :-----: | :----------------------------------- | :-------------------------------------------------------------- | :---------------- |
|  [01]   | `AvaloniaHeadlessPlatform`           | `static class` (`Compositor` internal; `ForceRenderTimerTick`)  | platform owner    |
|  [02]   | `AvaloniaHeadlessPlatformOptions`    | `class { bool UseHeadlessDrawing=true; PixelFormat FrameBufferFormat=Rgba8888 }` | platform options  |
|  [03]   | `AvaloniaHeadlessPlatformExtensions` | `static class` (`UseHeadless`)                                  | builder extension |
|  [04]   | `HeadlessUnitTestSession`            | `sealed class : IDisposable, IAsyncDisposable`                  | session owner     |
|  [05]   | `AvaloniaTestApplicationAttribute`   | `sealed : Attribute` (`[AttributeUsage(Assembly)]`)             | app entry binding |
|  [06]   | `AvaloniaTestIsolationAttribute`     | `sealed : Attribute` (`[AttributeUsage(Assembly)]`)             | isolation binding |
|  [07]   | `AvaloniaTestIsolationLevel`         | `enum { PerAssembly, PerTest }` (default `PerTest`)             | isolation level   |
|  [08]   | `HeadlessWindowExtensions`           | `static class` — synthetic input + capture on `TopLevel`        | input and capture |

[INTERNAL_PLATFORM_TYPES]: implementation plumbing — `internal`, NOT a consumer contract (do not bind)
- rail: evidence

| [INDEX] | [SYMBOL]                          | [VISIBILITY] | [ROLE]                                            |
| :-----: | :-------------------------------- | :----------- | :------------------------------------------------ |
|  [01]   | `IHeadlessWindow`                 | `internal`   | per-window input/capture sink behind `TopLevel`   |
|  [02]   | `HeadlessWindowImpl`              | `internal`   | `IWindowImpl` headless window                      |
|  [03]   | `HeadlessPlatformRenderInterface` | `internal`   | stub render interface (`UseHeadlessDrawing=true`)  |
|  [04]   | `HeadlessClipboardImplStub`       | `internal`   | clipboard stub                                     |
|  [05]   | `HeadlessCursorFactoryStub`       | `internal`   | cursor stub                                        |
|  [06]   | `HeadlessPlatformTypeface`        | `internal`   | typeface stub                                      |
|  [07]   | `HeadlessFontManagerStub` / `…WithMultipleSystemFontsStub` | `internal` | font-manager stubs                 |
|  [08]   | `HeadlessIconLoaderStub` / `HeadlessScreensStub` | `internal` | icon/screen stubs                          |

Consumers reach window behavior exclusively through the `HeadlessWindowExtensions` methods on a `TopLevel`/`Window`; the catalog records the internal set so a design page does not mistake `IHeadlessWindow` for a public extension point.

[XUNIT_TYPES]: xUnit.v3 attributes and framework wiring (`Avalonia.Headless.XUnit`)
- rail: evidence

| [INDEX] | [SYMBOL]                                | [SIGNATURE]                                                  | [RAIL]            |
| :-----: | :-------------------------------------- | :---------------------------------------------------------- | :---------------- |
|  [01]   | `AvaloniaFactAttribute`                 | `sealed : FactAttribute` (`[CallerFilePath]`/`[CallerLineNumber]` ctor) | UI fact |
|  [02]   | `AvaloniaTheoryAttribute`               | `sealed : TheoryAttribute`                                  | UI theory         |
|  [03]   | `AvaloniaTestFrameworkAttribute`        | `sealed : Attribute, ITestFrameworkAttribute` (`FrameworkType`) | framework redirect |
|  [04]   | `AvaloniaTestFramework`                 | xunit.v3 `ITestFramework` owner                            | framework owner   |
|  [05]   | `AvaloniaTestCase` / `AvaloniaDelayEnumeratedTheoryTestCase` | dispatched (delay-enumerated) test cases   | dispatched case   |
|  [06]   | `AvaloniaTestCaseRunner` / `AvaloniaTestRunner` | case/test runners that pump the dispatcher         | dispatcher pump   |

## [03]-[ENTRYPOINTS]

[SESSION_ENTRYPOINTS]: platform boot and dispatcher-marshalled execution
- rail: evidence

| [INDEX] | [SURFACE]               | [SIGNATURE]                                                                 | [SURFACE_ROOT]                       | [RAIL]            |
| :-----: | :---------------------- | :------------------------------------------------------------------------- | :----------------------------------- | :---------------- |
|  [01]   | `UseHeadless`           | `AppBuilder UseHeadless(this AppBuilder, AvaloniaHeadlessPlatformOptions)`  | `AvaloniaHeadlessPlatformExtensions` | platform boot     |
|  [02]   | `StartNew`              | `HeadlessUnitTestSession StartNew(Type entryPointType[, AvaloniaTestIsolationLevel])` | `HeadlessUnitTestSession`  | session start     |
|  [03]   | `GetOrStartForAssembly` | `HeadlessUnitTestSession GetOrStartForAssembly(Assembly?)`                  | `HeadlessUnitTestSession`            | shared session    |
|  [04]   | `Dispatch`              | `Task Dispatch(Action, CancellationToken)` / `Task<T> Dispatch<T>(Func<T>\|Func<Task<T>>, CancellationToken)` | `HeadlessUnitTestSession` | UI-thread run |
|  [05]   | `ForceRenderTimerTick`  | `static void ForceRenderTimerTick(int count = 1)`                          | `AvaloniaHeadlessPlatform`           | manual frame tick |
|  [06]   | `Dispose` / `DisposeAsync` | `void Dispose()` / `ValueTask DisposeAsync()`                           | `HeadlessUnitTestSession`            | session teardown  |

`Dispatch` requires a `CancellationToken` and resumes the awaited body on the dispatcher thread — the property that lets `await`-ed UI code keep its "main thread". `StartNew(Type)` takes the app entry-point type (a `BuildAvaloniaApp`-shaped method or an `Application` subclass), so the `entryPointType` matches `[AvaloniaTestApplication]`. `GetOrStartForAssembly` shares one session per assembly under `PerAssembly` isolation; `StartNew` is the `PerTest` path.

[OPTION_ENTRYPOINTS]: drawing backend and pixel format selection on `AvaloniaHeadlessPlatformOptions`
- rail: evidence

| [INDEX] | [SURFACE]              | [SIGNATURE]                                  | [RAIL]            |
| :-----: | :--------------------- | :------------------------------------------- | :---------------- |
|  [01]   | `UseHeadlessDrawing`   | `bool` (default `true` — stub render)        | stub vs Skia draw |
|  [02]   | `FrameBufferFormat`    | `PixelFormat` (default `Rgba8888`)           | capture format    |

Set `UseHeadlessDrawing = false` AND reference `Avalonia.Skia` (`UseSkia`) for real pixels; with the default stub interface `CaptureRenderedFrame` returns a blank/`null` surface. `FrameBufferFormat` fixes the capture pixel layout for golden-image comparison.

[CAPTURE_ENTRYPOINTS]: frame capture and DPI on `HeadlessWindowExtensions` (extension of `TopLevel`)
- rail: evidence

| [INDEX] | [SURFACE]              | [SIGNATURE]                                                  | [RAIL]            |
| :-----: | :--------------------- | :---------------------------------------------------------- | :---------------- |
|  [01]   | `CaptureRenderedFrame` | `WriteableBitmap? CaptureRenderedFrame(this TopLevel)`      | render then grab  |
|  [02]   | `GetLastRenderedFrame` | `WriteableBitmap? GetLastRenderedFrame(this TopLevel)`      | last frame bitmap |
|  [03]   | `SetRenderScaling`     | `void SetRenderScaling(this TopLevel, double scaling)`      | DPI scaling       |

`CaptureRenderedFrame` forces a render pass then returns the framebuffer; `GetLastRenderedFrame` returns the prior buffer without re-rendering. Both yield a `WriteableBitmap` that `WriteableBitmap.Save(Stream)` serializes to PNG for the evidence artifact.

[INPUT_ENTRYPOINTS]: synthetic keyboard, pointer, and drag injection on `HeadlessWindowExtensions`
- rail: evidence

| [INDEX] | [SURFACE]           | [SIGNATURE]                                                                                  | [RAIL]        |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------- | :------------ |
|  [01]   | `KeyPress`          | `void KeyPress(this TopLevel, Key, RawInputModifiers, PhysicalKey, string? keySymbol)`        | key down      |
|  [02]   | `KeyRelease`        | `void KeyRelease(this TopLevel, Key, RawInputModifiers, PhysicalKey, string? keySymbol)`      | key up        |
|  [03]   | `KeyPressQwerty`    | `void KeyPressQwerty(this TopLevel, PhysicalKey, RawInputModifiers)`                          | physical down |
|  [04]   | `KeyReleaseQwerty`  | `void KeyReleaseQwerty(this TopLevel, PhysicalKey, RawInputModifiers)`                        | physical up   |
|  [05]   | `KeyTextInput`      | `void KeyTextInput(this TopLevel, string text)`                                              | text input    |
|  [06]   | `MouseDown`         | `void MouseDown(this TopLevel, Point, MouseButton, RawInputModifiers = default)`            | button down   |
|  [07]   | `MouseUp`           | `void MouseUp(this TopLevel, Point, MouseButton, RawInputModifiers = default)`              | button up     |
|  [08]   | `MouseMove`         | `void MouseMove(this TopLevel, Point, RawInputModifiers = default)`                          | pointer move  |
|  [09]   | `MouseWheel`        | `void MouseWheel(this TopLevel, Point, Vector delta, RawInputModifiers = default)`          | wheel delta   |
|  [10]   | `DragDrop`          | `void DragDrop(this TopLevel, Point, RawDragEventType, IDataTransfer, DragDropEffects, RawInputModifiers = default)` | drag event |

`DragDrop` takes the Avalonia-12 `IDataTransfer` payload (not the legacy `IDataObject`) and a `RawDragEventType` (`DragEnter`/`DragOver`/`Drop`/`DragLeave`) plus a `DragDropEffects` mask — the design page wires a drag scenario by sequencing those raw types. `KeyPressQwerty`/`KeyReleaseQwerty` map a `PhysicalKey` to the QWERTY logical key so a test need not pre-resolve `Key` + key-symbol.

[XUNIT_ENTRYPOINTS]: assembly-level wiring and per-method dispatch attributes
- rail: evidence

| [INDEX] | [SURFACE]                   | [SHAPE]                                                       | [RAIL]             |
| :-----: | :-------------------------- | :----------------------------------------------------------- | :----------------- |
|  [01]   | `[AvaloniaFact]`            | method attr; runs on dispatcher; `Skip`/`Timeout` from base  | UI-thread fact     |
|  [02]   | `[AvaloniaTheory]`          | method attr; `[InlineData]`/`[MemberData]` rows on dispatcher | UI-thread theory   |
|  [03]   | `[assembly: AvaloniaTestApplication(typeof(App))]` | binds the `AppBuilder` entry-point type   | app entry          |
|  [04]   | `[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]` | picks isolation       | isolation pick     |
|  [05]   | `[assembly: AvaloniaTestFramework]` / `FrameworkType` | redirect to `AvaloniaTestFramework` whole-assembly | framework redirect |

`[AvaloniaTestApplication]` is mandatory at assembly scope — it names the type whose `BuildAvaloniaApp()` configures Avalonia (where `UseHeadless`, `UseSkia`, `WithInterFont` are chained). `[AvaloniaFact]`/`[AvaloniaTheory]` then need no per-method app reference. `AvaloniaFactAttribute`'s `[CallerFilePath]`/`[CallerLineNumber]` ctor preserves the source location for xunit.v3 reporting.

## [04]-[INTEGRATION_LAW]

[EVIDENCE_RAIL_LAW]:
- Stack: `[AvaloniaTestApplication]` -> `BuildAvaloniaApp().UseHeadless(new(){ UseHeadlessDrawing=false }).UseSkia().WithInterFont()` -> `[AvaloniaFact]` body running on the dispatcher -> `HeadlessWindowExtensions` drives input -> `ForceRenderTimerTick()` advances frames -> `CaptureRenderedFrame()` -> `WriteableBitmap.Save` -> golden-image assert (Verify.XunitV3). This is the single owned UI-proof rail.
- Accept: UI state is mutated only inside the dispatched body (`[AvaloniaFact]`/`[AvaloniaTheory]` or an explicit `session.Dispatch(...)`); render asserts read a `WriteableBitmap` from the capture API.
- Reject: UI mutation off the session/dispatcher thread; reaching for `internal` window impls; screenshot tooling outside the headless platform.

[CAPTURE_LAW]:
- Pixel evidence is meaningful only when `UseHeadlessDrawing=false` and `Avalonia.Skia` renders — the design page that asserts on pixels must declare both, and the `FrameBufferFormat` (`Rgba8888`) fixes the comparison layout. With the default stub render interface, capture is structural-only (layout/visual-tree assertions), and the page asserts on logical state, not pixels.
- Determinism: `ForceRenderTimerTick` replaces the wall-clock render loop, so frame production is explicit and reproducible; `WithInterFont` (embedded `fonts:Inter`) removes host-font drift so text metrics are stable across the macOS desktop and headless-Linux CI lanes.

[ISOLATION_LAW]:
- `PerTest` (default) recreates `Application`+`Dispatcher` per method — choose it when a test mutates global app state; `PerAssembly` shares one `Application` for speed and must be paired with state-clean tests (the headless framework disposes nothing between `PerAssembly` runs). The choice is an assembly-level `[AvaloniaTestIsolation]` decision, never per-method.
