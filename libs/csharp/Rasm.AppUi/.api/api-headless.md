# [RASM_APPUI_API_HEADLESS]

`Avalonia.Headless` boots a windowless Avalonia platform — manually-ticked render timer, synthetic input, rendered-frame capture — and `Avalonia.Headless.XUnit` wraps it in xUnit.v3 fact/theory discoverers marshalling test bodies onto the dispatcher thread. Together they own the UI-evidence rail: a `[AvaloniaFact]` drives view interaction with `KeyPress`/`MouseDown`/`DragDrop`, advances frames with `ForceRenderTimerTick`, and asserts on a `WriteableBitmap` from `CaptureRenderedFrame`. Only the public test-author surface is a consumer path; `internal` window impls stay off it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Headless`
- package: `Avalonia.Headless` (MIT)
- assembly: `Avalonia.Headless` (AnyCPU IL, managed-only, `net10.0`)
- namespace: `Avalonia.Headless`
- rail: evidence
- depends: `Avalonia.Base` (`WriteableBitmap`, input/raw types), `HarfBuzzSharp` (`UseHeadless` binds text shaping); `Avalonia.Skia` binds only when `UseHeadlessDrawing=false`

[PACKAGE_SURFACE]: `Avalonia.Headless.XUnit`
- package: `Avalonia.Headless.XUnit` (MIT)
- assembly: `Avalonia.Headless.XUnit` (`net10.0`)
- namespace: `Avalonia.Headless.XUnit`
- rail: evidence
- depends: `xunit.v3.extensibility.core` — the attributes derive from `FactAttribute`/`TheoryAttribute` and implement `ITestFrameworkAttribute`

## [02]-[PUBLIC_TYPES]

[PLATFORM_TYPES]: public platform, options, and session surfaces (`Avalonia.Headless`)

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `AvaloniaHeadlessPlatform`           | static class  | platform owner; `ForceRenderTimerTick` frame pump     |
|  [02]   | `AvaloniaHeadlessPlatformOptions`    | class         | drawing-backend and framebuffer options               |
|  [03]   | `AvaloniaHeadlessPlatformExtensions` | static class  | `UseHeadless` builder extension                       |
|  [04]   | `HeadlessUnitTestSession`            | sealed class  | dispatcher session (`IDisposable`/`IAsyncDisposable`) |
|  [05]   | `AvaloniaTestApplicationAttribute`   | attribute     | assembly app-entry binding                            |
|  [06]   | `AvaloniaTestIsolationAttribute`     | attribute     | assembly isolation binding                            |
|  [07]   | `AvaloniaTestIsolationLevel`         | enum          | `PerAssembly` / `PerTest` (default `PerTest`)         |
|  [08]   | `HeadlessWindowExtensions`           | static class  | synthetic input and capture on `TopLevel`             |

[INTERNAL_PLATFORM_TYPES]: `internal` plumbing behind `TopLevel`; window behavior reaches consumers only through `HeadlessWindowExtensions`, never these.

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `IHeadlessWindow`                                          | interface     | per-window input/capture sink behind `TopLevel`   |
|  [02]   | `HeadlessWindowImpl`                                       | class         | `IWindowImpl` headless window                     |
|  [03]   | `HeadlessPlatformRenderInterface`                          | class         | stub render interface (`UseHeadlessDrawing=true`) |
|  [04]   | `HeadlessClipboardImplStub`                                | class         | clipboard stub                                    |
|  [05]   | `HeadlessCursorFactoryStub`                                | class         | cursor stub                                       |
|  [06]   | `HeadlessPlatformTypeface`                                 | class         | typeface stub                                     |
|  [07]   | `HeadlessFontManagerStub` / `…WithMultipleSystemFontsStub` | class         | font-manager stubs                                |
|  [08]   | `HeadlessIconLoaderStub` / `HeadlessScreensStub`           | class         | icon/screen stubs                                 |

[XUNIT_TYPES]: xUnit.v3 attributes and framework wiring (`Avalonia.Headless.XUnit`)

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `AvaloniaFactAttribute`                 | attribute     | UI fact (`: FactAttribute`, `[CallerFilePath]` ctor) |
|  [02]   | `AvaloniaTheoryAttribute`               | attribute     | UI theory (`: TheoryAttribute`)                      |
|  [03]   | `AvaloniaTestFrameworkAttribute`        | attribute     | framework redirect (`FrameworkType`)                 |
|  [04]   | `AvaloniaTestFramework`                 | class         | xunit.v3 `ITestFramework` owner                      |
|  [05]   | `AvaloniaTestCase`                      | class         | dispatched test case                                 |
|  [06]   | `AvaloniaDelayEnumeratedTheoryTestCase` | class         | dispatched delay-enumerated theory case              |
|  [07]   | `AvaloniaTestCaseRunner`                | class         | case runner pumping the dispatcher                   |
|  [08]   | `AvaloniaTestRunner`                    | class         | test runner pumping the dispatcher                   |

## [03]-[ENTRYPOINTS]

[SESSION_ENTRYPOINTS]: platform boot and dispatcher-marshalled execution

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `AvaloniaHeadlessPlatformExtensions.UseHeadless(AvaloniaHeadlessPlatformOptions)`       | static   | platform boot -> `AppBuilder` |
|  [02]   | `HeadlessUnitTestSession.StartNew(Type, AvaloniaTestIsolationLevel?)`                   | factory  | PerTest session start         |
|  [03]   | `HeadlessUnitTestSession.GetOrStartForAssembly(Assembly?)`                              | factory  | shared per-assembly session   |
|  [04]   | `HeadlessUnitTestSession.Dispatch(Action / Func<T> / Func<Task<T>>, CancellationToken)` | instance | UI-thread run                 |
|  [05]   | `AvaloniaHeadlessPlatform.ForceRenderTimerTick(int = 1)`                                | static   | manual frame tick             |
|  [06]   | `HeadlessUnitTestSession.Dispose() / DisposeAsync()`                                    | instance | session teardown              |

- `Dispatch` resumes the awaited continuation on the dispatcher thread, so `await`-ed UI code keeps its main-thread affinity; every overload requires a `CancellationToken`.
- `StartNew(Type)` takes the `BuildAvaloniaApp`-shaped or `Application` entry-point type matching `[AvaloniaTestApplication]`; `GetOrStartForAssembly` is the `PerAssembly` shared path, `StartNew` the `PerTest` path.

[OPTION_ENTRYPOINTS]: drawing backend and frame control on `AvaloniaHeadlessPlatformOptions`

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `UseHeadlessDrawing : bool`       | property | stub render vs Skia (default `true`)      |
|  [02]   | `FrameBufferFormat : PixelFormat` | property | capture pixel layout (default `Rgba8888`) |
|  [03]   | `Fps : int`                       | property | render-timer rate (default `60`)          |
|  [04]   | `ShouldRenderOnUIThread : bool`   | property | UI-thread render loop (default `true`)    |

[CAPTURE_ENTRYPOINTS]: frame capture and DPI on `HeadlessWindowExtensions` (`TopLevel` extension)

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]              |
| :-----: | :--------------------------------------------------- | :------ | :------------------------ |
|  [01]   | `CaptureRenderedFrame(TopLevel) -> WriteableBitmap?` | static  | render then grab          |
|  [02]   | `GetLastRenderedFrame(TopLevel) -> WriteableBitmap?` | static  | prior frame, no re-render |
|  [03]   | `SetRenderScaling(TopLevel, double)`                 | static  | DPI scaling               |

[INPUT_ENTRYPOINTS]: synthetic input on `HeadlessWindowExtensions`; every method extends `TopLevel`, returns `void`, injects one raw event

| [INDEX] | [SURFACE]                                                                                        | [SHAPE] | [CAPABILITY]  |
| :-----: | :----------------------------------------------------------------------------------------------- | :------ | :------------ |
|  [01]   | `KeyPress(Key, RawInputModifiers, PhysicalKey, string?)`                                         | static  | key down      |
|  [02]   | `KeyRelease(Key, RawInputModifiers, PhysicalKey, string?)`                                       | static  | key up        |
|  [03]   | `KeyPressQwerty(PhysicalKey, RawInputModifiers)`                                                 | static  | physical down |
|  [04]   | `KeyReleaseQwerty(PhysicalKey, RawInputModifiers)`                                               | static  | physical up   |
|  [05]   | `KeyTextInput(string)`                                                                           | static  | text input    |
|  [06]   | `MouseDown(Point, MouseButton, RawInputModifiers = default)`                                     | static  | button down   |
|  [07]   | `MouseUp(Point, MouseButton, RawInputModifiers = default)`                                       | static  | button up     |
|  [08]   | `MouseMove(Point, RawInputModifiers = default)`                                                  | static  | pointer move  |
|  [09]   | `MouseWheel(Point, Vector, RawInputModifiers = default)`                                         | static  | wheel delta   |
|  [10]   | `DragDrop(Point, RawDragEventType, IDataTransfer, DragDropEffects, RawInputModifiers = default)` | static  | drag event    |

- `DragDrop` sequences a `RawDragEventType` (`DragEnter`/`DragOver`/`Drop`/`DragLeave`) with a `DragDropEffects` mask over an `IDataTransfer` payload.
- `KeyPressQwerty`/`KeyReleaseQwerty` map a `PhysicalKey` to its QWERTY logical key, so a test skips `Key`+key-symbol resolution.

[XUNIT_ENTRYPOINTS]: fact/theory and assembly wiring attributes

| [INDEX] | [SURFACE]                                                       | [SHAPE]       | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `[AvaloniaFact]`                                                | method-attr   | UI-thread fact; inherits `Skip`/`Timeout`  |
|  [02]   | `[AvaloniaTheory]`                                              | method-attr   | UI-thread `[InlineData]`/`[MemberData]`    |
|  [03]   | `[assembly: AvaloniaTestApplication(Type)]`                     | assembly-attr | binds `BuildAvaloniaApp` entry (mandatory) |
|  [04]   | `[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel)]` | assembly-attr | isolation pick                             |
|  [05]   | `[assembly: AvaloniaTestFramework]`                             | assembly-attr | framework redirect via `FrameworkType`     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- UI state mutates only inside the dispatched body — `[AvaloniaFact]`/`[AvaloniaTheory]` or an explicit `session.Dispatch(...)`; render asserts read a `WriteableBitmap` from `CaptureRenderedFrame`/`GetLastRenderedFrame`.
- `ForceRenderTimerTick` replaces the wall-clock render loop, so frame production is explicit and reproducible; `WithInterFont` removes host-font drift so text metrics hold across macOS and headless-Linux lanes.
- `PerTest` recreates `Application`+`Dispatcher` per method; `PerAssembly` shares one and disposes nothing between runs, so it pairs only with state-clean tests.

[STACKING]:
- boot chain: `[assembly: AvaloniaTestApplication(typeof(App))]` names the `BuildAvaloniaApp` type chaining `UseHeadless(new(){ UseHeadlessDrawing=false }).UseSkia().WithInterFont()`; a `[AvaloniaFact]` body drives `HeadlessWindowExtensions` input, `ForceRenderTimerTick` advances frames, `CaptureRenderedFrame` grabs the buffer, and the AppUi capture/evidence pages assert on it.
- `Verify.XunitV3`(`.api/api-verify.md`): the captured `WriteableBitmap` serializes via `WriteableBitmap.Save(Stream)` to PNG bytes that `Verify(bytes, "png")` snapshots under `UseStreamComparer`, and `VerifyJson` proves a settled layout or command receipt.
- `Avalonia.Skia`(`.api/api-avalonia-skia.md`): `UseSkia` supplies the render backend real-pixel evidence needs; `Avalonia.Fonts.Inter`(`.api/api-avalonia-fonts.md`): `WithInterFont` embeds `fonts:Inter` for stable text metrics.
- `Avalonia`(`.api/api-avalonia.md`): the injected `TopLevel`, `IDataTransfer`, `RawInputModifiers`, `Key`/`PhysicalKey`, and `RawDragEventType` types the extension methods carry.

[LOCAL_ADMISSION]:
- Pixel evidence is admitted only when `UseHeadlessDrawing=false` and `Avalonia.Skia` renders, with `FrameBufferFormat` fixing the comparison layout; under the default stub render the page asserts logical and visual-tree state, never pixels.
- Isolation is one assembly-level `[AvaloniaTestIsolation]` decision, never per-method.

[RAIL_LAW]:
- Package: `Avalonia.Headless`, `Avalonia.Headless.XUnit`
- Owns: the windowless UI-evidence rail — dispatcher-marshalled test bodies, synthetic input injection, manual frame ticking, and rendered-frame capture.
- Accept: UI mutation inside the dispatched body; render asserts reading a `WriteableBitmap` from the capture API; `[AvaloniaFact]`/`[AvaloniaTheory]` for per-method dispatch.
- Reject: UI mutation off the dispatcher thread; binding `internal` window impls (`IHeadlessWindow`, `HeadlessWindowImpl`); screenshot tooling outside the headless platform; a hand-rolled render loop when `ForceRenderTimerTick` owns frame production.
