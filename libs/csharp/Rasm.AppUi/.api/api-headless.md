# [RASM_APPUI_API_HEADLESS]

`Avalonia.Headless` and `Avalonia.Headless.XUnit` supply the headless windowing platform, unit-test sessions, isolation levels, synthetic input, frame capture, and xUnit fact/theory attributes for UI evidence.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Headless`
- package: `Avalonia.Headless`
- assembly: `Avalonia.Headless`
- namespace: `Avalonia.Headless`
- asset: runtime library
- asset: test-scoped restore under `tests/csharp/libs/Rasm.AppUi`
- rail: evidence

[PACKAGE_SURFACE]: `Avalonia.Headless.XUnit`
- package: `Avalonia.Headless.XUnit`
- assembly: `Avalonia.Headless.XUnit`
- namespace: `Avalonia.Headless.XUnit`
- asset: runtime library
- asset: test-scoped restore under `tests/csharp/libs/Rasm.AppUi`
- rail: evidence

## [2]-[PUBLIC_TYPES]

[PLATFORM_TYPES]: headless platform and session surfaces
- rail: evidence

| [INDEX] | [SYMBOL]                             | [RAIL]            |
| :-----: | :----------------------------------- | :---------------- |
|   [1]   | `AvaloniaHeadlessPlatform`           | platform owner    |
|   [2]   | `AvaloniaHeadlessPlatformOptions`    | platform options  |
|   [3]   | `AvaloniaHeadlessPlatformExtensions` | builder extension |
|   [4]   | `HeadlessUnitTestSession`            | session owner     |
|   [5]   | `AvaloniaTestApplicationAttribute`   | app binding       |
|   [6]   | `AvaloniaTestIsolationAttribute`     | isolation binding |
|   [7]   | `AvaloniaTestIsolationLevel`         | isolation level   |

[WINDOW_TYPES]: headless window, input, and capture surfaces
- rail: evidence

| [INDEX] | [SYMBOL]                          | [RAIL]            |
| :-----: | :-------------------------------- | :---------------- |
|   [1]   | `HeadlessWindowExtensions`        | input and capture |
|   [2]   | `HeadlessWindowImpl`              | window impl       |
|   [3]   | `IHeadlessWindow`                 | window contract   |
|   [4]   | `HeadlessPlatformRenderInterface` | render stub       |
|   [5]   | `HeadlessClipboardImplStub`       | clipboard stub    |
|   [6]   | `HeadlessCursorFactoryStub`       | cursor stub       |
|   [7]   | `HeadlessPlatformTypeface`        | typeface stub     |

[XUNIT_TYPES]: xUnit fact, theory, and framework surfaces
- rail: evidence

| [INDEX] | [SYMBOL]                                | [RAIL]            |
| :-----: | :-------------------------------------- | :---------------- |
|   [1]   | `AvaloniaFactAttribute`                 | UI fact           |
|   [2]   | `AvaloniaTheoryAttribute`               | UI theory         |
|   [3]   | `AvaloniaTestFrameworkAttribute`        | framework binding |
|   [4]   | `AvaloniaTestFramework`                 | framework owner   |
|   [5]   | `AvaloniaTestCase`                      | dispatched case   |
|   [6]   | `AvaloniaDelayEnumeratedTheoryTestCase` | delayed theory    |
|   [7]   | `AvaloniaTestCaseRunner`                | case runner       |
|   [8]   | `AvaloniaTestRunner`                    | test runner       |

## [3]-[ENTRYPOINTS]

[SESSION_ENTRYPOINTS]: platform boot and session dispatch
- rail: evidence

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]                       | [RAIL]            |
| :-----: | :---------------------- | :----------------------------------- | :---------------- |
|   [1]   | `UseHeadless`           | `AvaloniaHeadlessPlatformExtensions` | platform boot     |
|   [2]   | `UseHeadlessDrawing`    | `AvaloniaHeadlessPlatformOptions`    | stub vs Skia draw |
|   [3]   | `FrameBufferFormat`     | `AvaloniaHeadlessPlatformOptions`    | pixel format      |
|   [4]   | `StartNew`              | `HeadlessUnitTestSession`            | session start     |
|   [5]   | `GetOrStartForAssembly` | `HeadlessUnitTestSession`            | shared session    |
|   [6]   | `Dispatch`              | `HeadlessUnitTestSession`            | UI-thread run     |
|   [7]   | `DisposeAsync`          | `HeadlessUnitTestSession`            | session teardown  |
|   [8]   | `ForceRenderTimerTick`  | `AvaloniaHeadlessPlatform`           | manual frame tick |

[CAPTURE_ENTRYPOINTS]: frame capture and scaling
- rail: evidence
- surface-root: `HeadlessWindowExtensions`

| [INDEX] | [SURFACE]              | [RAIL]            |
| :-----: | :--------------------- | :---------------- |
|   [1]   | `CaptureRenderedFrame` | render and grab   |
|   [2]   | `GetLastRenderedFrame` | last frame bitmap |
|   [3]   | `SetRenderScaling`     | DPI scaling       |

[INPUT_ENTRYPOINTS]: synthetic keyboard, mouse, and drag input
- rail: evidence
- surface-root: `HeadlessWindowExtensions`

| [INDEX] | [SURFACE]        | [RAIL]       |
| :-----: | :--------------- | :----------- |
|   [1]   | `KeyPress`       | key down     |
|   [2]   | `KeyRelease`     | key up       |
|   [3]   | `KeyPressQwerty` | physical key |
|   [4]   | `KeyTextInput`   | text input   |
|   [5]   | `MouseDown`      | button down  |
|   [6]   | `MouseUp`        | button up    |
|   [7]   | `MouseMove`      | pointer move |
|   [8]   | `MouseWheel`     | wheel delta  |
|   [9]   | `DragDrop`       | drag event   |

[XUNIT_ENTRYPOINTS]: test attribute surfaces
- rail: evidence

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]                     | [RAIL]             |
| :-----: | :-------------------------- | :--------------------------------- | :----------------- |
|   [1]   | `[AvaloniaFact]`            | `AvaloniaFactAttribute`            | UI-thread fact     |
|   [2]   | `[AvaloniaTheory]`          | `AvaloniaTheoryAttribute`          | UI-thread theory   |
|   [3]   | `FrameworkType`             | `AvaloniaTestFrameworkAttribute`   | framework redirect |
|   [4]   | `[AvaloniaTestApplication]` | `AvaloniaTestApplicationAttribute` | app entry type     |
|   [5]   | `[AvaloniaTestIsolation]`   | `AvaloniaTestIsolationAttribute`   | isolation pick     |

## [4]-[IMPLEMENTATION_LAW]

[EVIDENCE_LAW]:
- Package: `Avalonia.Headless` + `Avalonia.Headless.XUnit`
- Owns: headless platform boot, UI-thread sessions, synthetic input, frame capture, and xUnit dispatch
- Accept: UI assertions run inside `[AvaloniaFact]`/`[AvaloniaTheory]` or an explicit session dispatch
- Reject: UI state mutation off the session thread

[CAPTURE_LAW]:
- Package: `Avalonia.Headless`
- Owns: rendered-frame bitmaps as visual evidence when `UseHeadlessDrawing` is disabled and Skia renders
- Accept: pixel evidence comes from `CaptureRenderedFrame`/`GetLastRenderedFrame`
- Reject: screenshot tooling outside the headless platform
