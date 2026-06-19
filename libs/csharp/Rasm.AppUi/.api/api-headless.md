# [RASM_APPUI_API_HEADLESS]

`Avalonia.Headless` and `Avalonia.Headless.XUnit` supply the headless windowing platform, unit-test sessions, isolation levels, synthetic input, frame capture, and xUnit fact/theory attributes for UI evidence.

## [01]-[PACKAGE_SURFACE]

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

## [02]-[PUBLIC_TYPES]

[PLATFORM_TYPES]: headless platform and session surfaces
- rail: evidence

| [INDEX] | [SYMBOL]                             | [RAIL]            |
| :-----: | :----------------------------------- | :---------------- |
|  [01]   | `AvaloniaHeadlessPlatform`           | platform owner    |
|  [02]   | `AvaloniaHeadlessPlatformOptions`    | platform options  |
|  [03]   | `AvaloniaHeadlessPlatformExtensions` | builder extension |
|  [04]   | `HeadlessUnitTestSession`            | session owner     |
|  [05]   | `AvaloniaTestApplicationAttribute`   | app binding       |
|  [06]   | `AvaloniaTestIsolationAttribute`     | isolation binding |
|  [07]   | `AvaloniaTestIsolationLevel`         | isolation level   |

[WINDOW_TYPES]: headless window, input, and capture surfaces
- rail: evidence

| [INDEX] | [SYMBOL]                          | [RAIL]            |
| :-----: | :-------------------------------- | :---------------- |
|  [01]   | `HeadlessWindowExtensions`        | input and capture |
|  [02]   | `HeadlessWindowImpl`              | window impl       |
|  [03]   | `IHeadlessWindow`                 | window contract   |
|  [04]   | `HeadlessPlatformRenderInterface` | render stub       |
|  [05]   | `HeadlessClipboardImplStub`       | clipboard stub    |
|  [06]   | `HeadlessCursorFactoryStub`       | cursor stub       |
|  [07]   | `HeadlessPlatformTypeface`        | typeface stub     |

[XUNIT_TYPES]: xUnit fact, theory, and framework surfaces
- rail: evidence

| [INDEX] | [SYMBOL]                                | [RAIL]            |
| :-----: | :-------------------------------------- | :---------------- |
|  [01]   | `AvaloniaFactAttribute`                 | UI fact           |
|  [02]   | `AvaloniaTheoryAttribute`               | UI theory         |
|  [03]   | `AvaloniaTestFrameworkAttribute`        | framework binding |
|  [04]   | `AvaloniaTestFramework`                 | framework owner   |
|  [05]   | `AvaloniaTestCase`                      | dispatched case   |
|  [06]   | `AvaloniaDelayEnumeratedTheoryTestCase` | delayed theory    |
|  [07]   | `AvaloniaTestCaseRunner`                | case runner       |
|  [08]   | `AvaloniaTestRunner`                    | test runner       |

## [03]-[ENTRYPOINTS]

[SESSION_ENTRYPOINTS]: platform boot and session dispatch
- rail: evidence

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]                       | [RAIL]            |
| :-----: | :---------------------- | :----------------------------------- | :---------------- |
|  [01]   | `UseHeadless`           | `AvaloniaHeadlessPlatformExtensions` | platform boot     |
|  [02]   | `UseHeadlessDrawing`    | `AvaloniaHeadlessPlatformOptions`    | stub vs Skia draw |
|  [03]   | `FrameBufferFormat`     | `AvaloniaHeadlessPlatformOptions`    | pixel format      |
|  [04]   | `StartNew`              | `HeadlessUnitTestSession`            | session start     |
|  [05]   | `GetOrStartForAssembly` | `HeadlessUnitTestSession`            | shared session    |
|  [06]   | `Dispatch`              | `HeadlessUnitTestSession`            | UI-thread run     |
|  [07]   | `DisposeAsync`          | `HeadlessUnitTestSession`            | session teardown  |
|  [08]   | `ForceRenderTimerTick`  | `AvaloniaHeadlessPlatform`           | manual frame tick |

[CAPTURE_ENTRYPOINTS]: frame capture and scaling
- rail: evidence
- surface-root: `HeadlessWindowExtensions`

| [INDEX] | [SURFACE]              | [RAIL]            |
| :-----: | :--------------------- | :---------------- |
|  [01]   | `CaptureRenderedFrame` | render and grab   |
|  [02]   | `GetLastRenderedFrame` | last frame bitmap |
|  [03]   | `SetRenderScaling`     | DPI scaling       |

[INPUT_ENTRYPOINTS]: synthetic keyboard, mouse, and drag input
- rail: evidence
- surface-root: `HeadlessWindowExtensions`

| [INDEX] | [SURFACE]        | [RAIL]       |
| :-----: | :--------------- | :----------- |
|  [01]   | `KeyPress`       | key down     |
|  [02]   | `KeyRelease`     | key up       |
|  [03]   | `KeyPressQwerty` | physical key |
|  [04]   | `KeyTextInput`   | text input   |
|  [05]   | `MouseDown`      | button down  |
|  [06]   | `MouseUp`        | button up    |
|  [07]   | `MouseMove`      | pointer move |
|  [08]   | `MouseWheel`     | wheel delta  |
|  [09]   | `DragDrop`       | drag event   |

[XUNIT_ENTRYPOINTS]: test attribute surfaces
- rail: evidence

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]                     | [RAIL]             |
| :-----: | :-------------------------- | :--------------------------------- | :----------------- |
|  [01]   | `[AvaloniaFact]`            | `AvaloniaFactAttribute`            | UI-thread fact     |
|  [02]   | `[AvaloniaTheory]`          | `AvaloniaTheoryAttribute`          | UI-thread theory   |
|  [03]   | `FrameworkType`             | `AvaloniaTestFrameworkAttribute`   | framework redirect |
|  [04]   | `[AvaloniaTestApplication]` | `AvaloniaTestApplicationAttribute` | app entry type     |
|  [05]   | `[AvaloniaTestIsolation]`   | `AvaloniaTestIsolationAttribute`   | isolation pick     |

## [04]-[IMPLEMENTATION_LAW]

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
