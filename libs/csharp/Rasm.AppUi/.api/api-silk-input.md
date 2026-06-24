# [RASM_APPUI_API_SILK_INPUT]

`Silk.NET.Input` is the cross-platform input abstraction: `IView.CreateInput()` mints an `IInputContext` whose `Gamepads`/`Joysticks`/`Keyboards`/`Mice`/`OtherDevices` lists surface both immutable poll state and per-device `event Action<...>` streams over the same backend-updated state, with the concrete devices supplied by a reflection-loaded backend. The consumable public surface (`IInputContext`, `IGamepad`/`IJoystick` and their `Button`/`Thumbstick`/`Trigger`/`Axis`/`Hat` carriers, `IKeyboard`/`IMouse`/`ICursor`/`IMotor`, `Deadzone`) lives in `Silk.NET.Input.Common`; the meta-package `Silk.NET.Input` carries no assembly and only pulls `Common` + the SDL2/GLFW backends. The SDL2 backend (`Silk.NET.Input.Sdl`) registers reflectively through `[InputPlatform]` and owns the osx-arm64 controller/gamepad/joystick path; it is the haptic peer of `Silk.NET.SDL` (`.api/api-silk-sdl.md`) and P/Invokes the SAME SDL2 native runtime, so the AppUi InputFabric binds one shared SDL2 native bundle across both packages rather than two — the `Silk.NET.Input.Sdl` controller backend and the `Silk.NET.SDL` `Sdl.GetApi()` haptic root sit over a single per-process SDL2 binding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.Input`
- package: `Silk.NET.Input` (2.23.0, MIT, meta — no assembly; nuspec deps are `Silk.NET.Input.Common`/`.Glfw`/`.Sdl`, all `exclude=Build,Analyzers`)
- package: `Silk.NET.Input.Common` (2.23.0, MIT — carries the assembly; consumer net10 binds `lib/net5.0`)
- package: `Silk.NET.Input.Glfw` (2.23.0 — `[InputPlatform]` backend, reflection-loaded)
- package: `Silk.NET.Input.Sdl` (2.23.0 — `[InputPlatform]` backend, osx-arm64 controller/haptic path)
- assembly: `Silk.NET.Input.Common`
- namespace: `Silk.NET.Input` (single public namespace)
- asset: runtime library + reflection-loaded backend assemblies (`Silk.NET.Input.Sdl`, `Silk.NET.Input.Glfw`)
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and device contracts
- rail: input

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [RAIL]                                       |
| :-----: | :--------------- | :--------------- | :------------------------------------------- |
|  [01]   | `IInputContext`  | disposable root  | per-view device aggregator + `Handle`/`ConnectionChanged` |
|  [02]   | `IInputDevice`   | device base      | `Name`/`Index`/`IsConnected` identity        |
|  [03]   | `IGamepad`       | device contract  | mapped controller (buttons/sticks/triggers/motors) |
|  [04]   | `IJoystick`      | device contract  | raw axes/buttons/hats                        |
|  [05]   | `IKeyboard`      | device contract  | key state, char intake, `ClipboardText`, IME |
|  [06]   | `IMouse`         | device contract  | position, buttons, scroll, double-click, cursor |
|  [07]   | `IMotor`         | actuator         | `Index` + per-motor rumble `Speed`           |
|  [08]   | `ICursor`        | cursor handle    | mode/type/standard shape/image/confine/hotspot |
|  [09]   | `IInputPlatform` | backend contract | `IsApplicable(IView)` + `CreateInput(IView)` |

[PUBLIC_TYPE_SCOPE]: input value carriers (immutable poll state)
- rail: input

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [RAIL]                                            |
| :-----: | :------------ | :-------------- | :------------------------------------------------ |
|  [01]   | `Button`      | struct          | `Name`(`ButtonName`)/`Index`/`Pressed`            |
|  [02]   | `Thumbstick`  | struct          | `Index`/`X`/`Y`; derived polar `Position`/`Direction` |
|  [03]   | `Trigger`     | struct          | `Index`/`Position`                                |
|  [04]   | `Axis`        | readonly struct | `Index`/`Position`                                |
|  [05]   | `Hat`         | struct          | `Index`/`Position`(`Position2D` bitfield)         |
|  [06]   | `Deadzone`    | readonly struct | `Value`/`Method` + `Apply(float raw)` fold        |
|  [07]   | `ScrollWheel` | struct (`IEquatable`) | `X`/`Y` scroll delta                        |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies
- rail: input

| [INDEX] | [SYMBOL]         | [KIND] | [RAIL]                                            |
| :-----: | :--------------- | :----- | :------------------------------------------------ |
|  [01]   | `ButtonName`     | enum   | `Unknown`/`A`/`B`/`X`/`Y`/bumpers/sticks/`DPad*`  |
|  [02]   | `Key`            | enum   | GLFW-style keyboard map (`Unknown`..`Menu`)       |
|  [03]   | `MouseButton`    | enum   | `Unknown`/`Left`/`Right`/`Middle`/`Button4`..`12` |
|  [04]   | `Position2D`     | enum   | hat-direction bitfield (`Up\|Left = UpLeft`)      |
|  [05]   | `DeadzoneMethod` | enum   | `Traditional` / `AdaptiveGradient`                |
|  [06]   | `CursorMode`     | enum   | `Normal`/`Hidden`/`Disabled`/`Raw`                |
|  [07]   | `CursorType`     | enum   | `Standard` / `Custom`                             |
|  [08]   | `StandardCursor` | enum   | named OS cursor shapes                            |

[PUBLIC_TYPE_SCOPE]: backend registration and extensions
- rail: input

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :----------------------- | :------------ | :-------------------------------- |
|  [01]   | `InputWindowExtensions`  | static class  | `CreateInput` + platform registry |
|  [02]   | `GamepadExtensions`      | static class  | named-button/stick accessors (throw `PlatformNotSupportedException`) |
|  [03]   | `InputPlatformAttribute` | assembly attr | backend discovery marker          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: context creation and platform registry
- rail: input

| [INDEX] | [SURFACE]                                        | [SURFACE_ROOT]          | [RAIL]                  |
| :-----: | :----------------------------------------------- | :---------------------- | :---------------------- |
|  [01]   | `IView.CreateInput()`                            | `InputWindowExtensions` | mint `IInputContext`    |
|  [02]   | `Platforms`                                      | `InputWindowExtensions` | registered backend list (lazy reflection load) |
|  [03]   | `ShouldLoadFirstPartyPlatforms(bool)`            | `InputWindowExtensions` | toggle reflection load (throws after load) |
|  [04]   | `Add(IInputPlatform)` / `Remove(IInputPlatform)` | `InputWindowExtensions` | manual backend registry |
|  [05]   | `TryAdd(string assemblyName)`                    | `InputWindowExtensions` | reflective backend load |

[ENTRYPOINT_SCOPE]: gamepad named accessors (`GamepadExtensions`)
- rail: input

| [INDEX] | [SURFACE]                                        | [SURFACE_ROOT]      | [RAIL]               |
| :-----: | :----------------------------------------------- | :------------------ | :------------------- |
|  [01]   | `A` / `B` / `X` / `Y`                            | `GamepadExtensions` | face `Button` lookup |
|  [02]   | `LeftBumper` / `RightBumper`                     | `GamepadExtensions` | shoulder `Button`    |
|  [03]   | `Back` / `Start` / `Home`                        | `GamepadExtensions` | system `Button`      |
|  [04]   | `LeftThumbstickButton` / `RightThumbstickButton` | `GamepadExtensions` | stick-click `Button` |
|  [05]   | `DPadUp` / `DPadRight` / `DPadDown` / `DPadLeft` | `GamepadExtensions` | d-pad `Button`       |
|  [06]   | `LeftThumbstick` / `RightThumbstick`             | `GamepadExtensions` | `Thumbstick` lookup  |

[ENTRYPOINT_SCOPE]: device state and event streams
- rail: input

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]         | [RAIL]                  |
| :-----: | :-------------------------------------------------------------- | :--------------------- | :---------------------- |
|  [01]   | `Gamepads` / `Joysticks` / `Keyboards` / `Mice` / `OtherDevices` / `Handle` | `IInputContext` | live device lists + native ptr |
|  [02]   | `ConnectionChanged` (`Action<IInputDevice, bool>`)             | `IInputContext`        | hotplug event           |
|  [03]   | `Buttons` / `Thumbsticks` / `Triggers` / `VibrationMotors`     | `IGamepad`             | mapped controller state |
|  [04]   | `ButtonDown` / `ButtonUp` / `ThumbstickMoved` / `TriggerMoved` | `IGamepad`             | gamepad event stream    |
|  [05]   | `Axes` / `Buttons` / `Hats`                                    | `IJoystick`            | raw joystick state      |
|  [06]   | `ButtonDown` / `ButtonUp` / `AxisMoved` / `HatMoved`           | `IJoystick`            | joystick event stream   |
|  [07]   | `Deadzone` (get/set)                                           | `IGamepad`/`IJoystick` | stick deadzone policy   |
|  [08]   | `SupportedKeys` / `IsKeyPressed(Key)` / `IsScancodePressed(int)` | `IKeyboard`          | key roster + poll       |
|  [09]   | `KeyDown` / `KeyUp` / `KeyChar` / `ClipboardText` / `BeginInput()` / `EndInput()` | `IKeyboard` | keyboard event stream + IME + clipboard |
|  [10]   | `Position`(`Vector2`) / `SupportedButtons` / `IsButtonPressed(MouseButton)` / `ScrollWheels` / `Cursor` | `IMouse` | mouse poll + cursor |
|  [11]   | `MouseDown` / `MouseUp` / `Click` / `DoubleClick` / `MouseMove` / `Scroll` / `DoubleClickTime` / `DoubleClickRange` | `IMouse` | mouse event stream |
|  [12]   | `Type` / `StandardCursor` / `CursorMode` / `IsConfined` / `HotspotX` / `HotspotY` / `Image`(`RawImage`) / `IsSupported(...)` | `ICursor` | cursor configuration |
|  [13]   | `Index` / `Speed` (get/set)                                   | `IMotor`               | rumble actuation        |

## [04]-[IMPLEMENTATION_LAW]

[INPUT_TOPOLOGY]:
- Single public namespace `Silk.NET.Input`; the meta-package carries no assembly and folds to empty under decompile, so the consumable types (10 interfaces, 8 enums, 7 value-carrier structs, 3 static/attribute owners) live in `Silk.NET.Input.Common`.
- Lifecycle is `IView` -> `CreateInput()` -> `IInputContext` -> device lists; `IInputContext` is `IDisposable` and `Handle` (`nint`) exposes the native backend pointer.
- `IInputContext` enumerates `Gamepads`/`Joysticks`/`Keyboards`/`Mice`/`OtherDevices` as `IReadOnlyList<T>`; each device carries `Name`/`Index`/`IsConnected` from `IInputDevice` and raises through device-level `event Action<...>` delegates rather than a callback registry.
- A device is consumed two ways in parallel: poll the immutable state carriers (`Buttons`, `Thumbsticks`, `Triggers`, `Axes`, `Hats`, `IsKeyPressed`, `Position`) each frame, or subscribe to the per-device `event Action<TDevice, ...>` streams; both views observe the same backend-updated state.
- `IGamepad` is the mapped layer (`Button.Name` from `ButtonName`, ordered `Thumbsticks`/`Triggers`, `VibrationMotors`); `IJoystick` is the raw layer (indexed `Axes`/`Buttons`/`Hats`, no semantic naming). `GamepadExtensions` projects named `ButtonName`/stick lookups over `IGamepad.Buttons`, throwing `PlatformNotSupportedException` when the backend omits the requested control.
- `Deadzone` is a value-object policy on `IGamepad`/`IJoystick`: `Apply(raw)` folds `DeadzoneMethod.Traditional` (hard cutoff below `Value`) or `DeadzoneMethod.AdaptiveGradient` (`(1-d)x + d·sgn(x)`) over a raw axis reading.
- `Hat.Position` is the `Position2D` bitfield (`Up|Left = UpLeft`); `Thumbstick` derives polar `Position = √(x²+y²)` / `Direction = atan2(y,x)` from `X`/`Y`; `Trigger`/`Axis` carry a single normalized `Position`; `IMouse.Position` is `Vector2` from `System.Numerics`.

[LOCAL_ADMISSION]:
- The backend is reflection-loaded: `InputWindowExtensions.Platforms` lazily calls `TryAdd("Silk.NET.Input.Sdl")` and `TryAdd("Silk.NET.Input.Glfw")` for `[InputPlatform]`-marked assemblies, so the AppUi Shell references `Silk.NET.Input.Sdl` to make the SDL2 osx-arm64 backend resolvable before the first `CreateInput()`; `ShouldLoadFirstPartyPlatforms(false)` (which throws if platforms already loaded) plus explicit `Add` pins a single backend.
- The reflection-loaded `Silk.NET.Input.Sdl` backend and the `Silk.NET.SDL` (`.api/api-silk-sdl.md`) `Sdl.GetApi()` haptic root P/Invoke ONE shared SDL2 native runtime: the InputFabric binds a single SDL2 native bundle per process, so the controller/gamepad stream from `IInputContext` and the `SDL_Haptic` force-feedback rail never re-load the SDL2 binding against each other. This is the reciprocal of the `api-silk-sdl` shared-native-bundle law — both packages name the same SDL2 bundle and the boundary capsule owns one binding.
- `IInputContext` is the one disposed input owner per view; the AppUi InputFabric pairs `CreateInput()` and `Dispose()` in a scoped boundary capsule and never holds device references past context disposal.
- Per-frame InputFabric reads the immutable state carriers (`Button`, `Thumbstick`, `Trigger`, `Axis`, `Hat`) and folds them into canonical input facts; the device `event Action<...>` streams feed edge-triggered actions, never a second polling loop over the same state.
- Rumble routes through `IGamepad.VibrationMotors[i].Speed`; haptic intensity beyond linear motor speed is a `Silk.NET.SDL` force-feedback concern (`.api/api-silk-sdl.md`), not this abstraction.

[RAIL_LAW]:
- Package: `Silk.NET.Input` (+ `Silk.NET.Input.Common`, `Silk.NET.Input.Sdl`)
- Owns: the input device abstraction — `IInputContext` aggregation, `IGamepad`/`IJoystick`/`IKeyboard`/`IMouse` state and event streams, `Deadzone` policy, and the `CreateInput` entrypoint over the reflection-loaded SDL2 backend for osx-arm64.
- Stacks: the `DeviceDriver` `[Union]` (`Shell/input` `[07]-[DEVICE_DRIVERS]`) folds `Gamepad(Silk.NET.Input controller)` and `Haptic(Silk.NET.SDL force-feedback)` cases into one driver union beside `Hid(HidSharp SpaceMouse)` (`api-hidsharp.md`) and `Midi(Melanchall.DryWetMidi)` (`api-drywetmidi.md`); all four capsules bind delegate columns on the single `InputFabric` edge that folds every device onto the one `CommandIntent` table — the gamepad poll state and the SDL2 haptic rail are two cases on the same edge, not two edges. A gamepad fault maps to the `InputDriverFault` `DeviceAbsent`/`OpenRejected` rows in the 4150 code band; the polled state carriers fold into the same canonical input fact the HID/MIDI/haptic drivers emit. The `Gamepad` and `Haptic` cases share one SDL2 native binding because `Silk.NET.Input.Sdl` and `Silk.NET.SDL` P/Invoke the same runtime.
- Accept: one `IInputContext` per view through `IView.CreateInput()`; both polled state carriers and per-device `event Action<...>` subscriptions over the same backend state; named gamepad controls through `GamepadExtensions`.
- Reject: a hand-rolled SDL controller poll loop beside `IInputContext`; a second SDL2 native binding re-loaded against the `Silk.NET.SDL` haptic root (the InputFabric owns one shared SDL2 bundle per process); a parallel device→intent edge beside the single `InputFabric` fold; renaming `IGamepad`/`IJoystick` into a parallel device model; mixing keyboard/mouse focus or windowing concerns into the input rail (those stay in `Silk.NET.Windowing`).
