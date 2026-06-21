# [RASM_APPUI_API_SILK_INPUT]

`Silk.NET.Input` is the cross-platform input abstraction: `IView.CreateInput()` mints an `IInputContext` whose `Gamepads`/`Joysticks`/`Keyboards`/`Mice` device lists surface live state and event streams, with the concrete devices supplied by a reflection-loaded backend. The public surface (`IInputContext`, `IGamepad`/`IJoystick` and their `Button`/`Thumbstick`/`Trigger`/`Axis`/`Hat` carriers, `IKeyboard`/`IMouse`, `Deadzone`) lives in `Silk.NET.Input.Common`; the SDL2 backend (`Silk.NET.Input.Sdl`) registers reflectively through the `InputPlatformAttribute` and owns the osx-arm64 controller/gamepad/joystick path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.Input`
- package: `Silk.NET.Input` (meta)
- package: `Silk.NET.Input.Common`
- package: `Silk.NET.Input.Glfw`
- package: `Silk.NET.Input.Sdl`
- assembly: `Silk.NET.Input.Common`
- namespace: `Silk.NET.Input`
- asset: runtime library + reflection-loaded backend assemblies
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and device contracts
- rail: input

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [RAIL]                             |
| :-----: | :--------------- | :--------------- | :--------------------------------- |
|  [01]   | `IInputContext`  | disposable root  | per-view device aggregator         |
|  [02]   | `IInputDevice`   | device base      | name/index/connected identity      |
|  [03]   | `IGamepad`       | device contract  | mapped controller (buttons/sticks) |
|  [04]   | `IJoystick`      | device contract  | raw axes/buttons/hats              |
|  [05]   | `IKeyboard`      | device contract  | key state, char/clipboard intake   |
|  [06]   | `IMouse`         | device contract  | position, buttons, scroll, cursor  |
|  [07]   | `IMotor`         | actuator         | per-motor rumble speed             |
|  [08]   | `ICursor`        | cursor handle    | cursor mode/type/image             |
|  [09]   | `IInputPlatform` | backend contract | applicability + context factory    |

[PUBLIC_TYPE_SCOPE]: input value carriers
- rail: input

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [RAIL]                                 |
| :-----: | :------------ | :-------------- | :------------------------------------- |
|  [01]   | `Button`      | struct          | `Name`/`Index`/`Pressed`               |
|  [02]   | `Thumbstick`  | struct          | `Index`/`X`/`Y`/`Position`/`Direction` |
|  [03]   | `Trigger`     | struct          | `Index`/`Position`                     |
|  [04]   | `Axis`        | readonly struct | `Index`/`Position`                     |
|  [05]   | `Hat`         | struct          | `Index`/`Position` (`Position2D`)      |
|  [06]   | `Deadzone`    | readonly struct | `Value`/`Method` + `Apply(raw)`        |
|  [07]   | `ScrollWheel` | struct          | `X`/`Y` scroll delta                   |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies
- rail: input

| [INDEX] | [SYMBOL]         | [KIND] | [RAIL]                                            |
| :-----: | :--------------- | :----- | :------------------------------------------------ |
|  [01]   | `ButtonName`     | enum   | `Unknown`/`A`/`B`/`X`/`Y`/bumpers/sticks/`DPad*`  |
|  [02]   | `Key`            | enum   | GLFW-style keyboard map (`Unknown`..`Menu`)       |
|  [03]   | `MouseButton`    | enum   | `Unknown`/`Left`/`Right`/`Middle`/`Button4`..`12` |
|  [04]   | `Position2D`     | enum   | hat direction bitfield (`Centered`..`DownRight`)  |
|  [05]   | `DeadzoneMethod` | enum   | `Traditional` / `AdaptiveGradient`                |
|  [06]   | `CursorMode`     | enum   | `Normal`/`Hidden`/`Disabled`/`Raw`                |
|  [07]   | `CursorType`     | enum   | `Standard` / `Custom`                             |
|  [08]   | `StandardCursor` | enum   | named OS cursor shapes                            |

[PUBLIC_TYPE_SCOPE]: backend registration and extensions
- rail: input

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :----------------------- | :------------ | :-------------------------------- |
|  [01]   | `InputWindowExtensions`  | static class  | `CreateInput` + platform registry |
|  [02]   | `GamepadExtensions`      | static class  | named-button/stick accessors      |
|  [03]   | `InputPlatformAttribute` | assembly attr | backend discovery marker          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: context creation and platform registry
- rail: input

| [INDEX] | [SURFACE]                                        | [SURFACE_ROOT]          | [RAIL]                  |
| :-----: | :----------------------------------------------- | :---------------------- | :---------------------- |
|  [01]   | `IView.CreateInput()`                            | `InputWindowExtensions` | mint `IInputContext`    |
|  [02]   | `Platforms`                                      | `InputWindowExtensions` | registered backend list |
|  [03]   | `ShouldLoadFirstPartyPlatforms(bool)`            | `InputWindowExtensions` | toggle reflection load  |
|  [04]   | `Add(IInputPlatform)` / `Remove(IInputPlatform)` | `InputWindowExtensions` | manual backend registry |
|  [05]   | `TryAdd(string assemblyName)`                    | `InputWindowExtensions` | reflective backend load |

[ENTRYPOINT_SCOPE]: gamepad named accessors
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

| [INDEX] | [SURFACE]                                                      | [SURFACE_ROOT]         | [RAIL]                  |
| :-----: | :------------------------------------------------------------- | :--------------------- | :---------------------- |
|  [01]   | `Gamepads` / `Joysticks` / `Keyboards` / `Mice`                | `IInputContext`        | live device lists       |
|  [02]   | `ConnectionChanged`                                            | `IInputContext`        | hotplug event           |
|  [03]   | `Buttons` / `Thumbsticks` / `Triggers` / `VibrationMotors`     | `IGamepad`             | mapped controller state |
|  [04]   | `ButtonDown` / `ButtonUp` / `ThumbstickMoved` / `TriggerMoved` | `IGamepad`             | gamepad event stream    |
|  [05]   | `Axes` / `Buttons` / `Hats`                                    | `IJoystick`            | raw joystick state      |
|  [06]   | `ButtonDown` / `ButtonUp` / `AxisMoved` / `HatMoved`           | `IJoystick`            | joystick event stream   |
|  [07]   | `Deadzone` (get/set)                                           | `IGamepad`/`IJoystick` | stick deadzone policy   |
|  [08]   | `IsKeyPressed(Key)` / `IsScancodePressed(int)`                 | `IKeyboard`            | key poll                |
|  [09]   | `KeyDown` / `KeyUp` / `KeyChar`                                | `IKeyboard`            | keyboard event stream   |
|  [10]   | `Position` / `IsButtonPressed(MouseButton)` / `ScrollWheels`   | `IMouse`               | mouse poll              |
|  [11]   | `MouseDown` / `MouseUp` / `Click` / `MouseMove` / `Scroll`     | `IMouse`               | mouse event stream      |
|  [12]   | `Speed` (get/set)                                              | `IMotor`               | rumble actuation        |

## [04]-[IMPLEMENTATION_LAW]

[INPUT_TOPOLOGY]:
- Single public namespace `Silk.NET.Input`; the meta-package `Silk.NET.Input` carries no assembly and folds to empty under decompile, so the consumable types live in `Silk.NET.Input.Common` (10 interfaces, 8 enums, 7 value-carrier structs, 3 static/attribute owners).
- Lifecycle is `IView` -> `CreateInput()` -> `IInputContext` -> device lists; `IInputContext` is `IDisposable` and `Handle` exposes the native backend pointer.
- `IInputContext` enumerates `Gamepads`, `Joysticks`, `Keyboards`, `Mice`, and `OtherDevices` as `IReadOnlyList<T>`; each device carries `Name`/`Index`/`IsConnected` from `IInputDevice` and raises through device-level `event Action<...>` delegates rather than a callback registry.
- A device is consumed two ways in parallel: poll the immutable state carriers (`Buttons`, `Thumbsticks`, `Triggers`, `Axes`, `Hats`, `IsKeyPressed`, `Position`) each frame, or subscribe to the per-device `event Action<TDevice, ...>` streams; both views observe the same backend-updated state.
- `IGamepad` is the mapped layer (`Button.Name` from `ButtonName`, ordered `Thumbsticks`/`Triggers`, `VibrationMotors`); `IJoystick` is the raw layer (indexed `Axes`/`Buttons`/`Hats`, no semantic naming). `GamepadExtensions` projects named `ButtonName`/stick lookups over `IGamepad.Buttons`, throwing `PlatformNotSupportedException` when the backend omits the requested control.
- `Deadzone` is a value-object policy on `IGamepad`/`IJoystick`: `Apply(raw)` folds `DeadzoneMethod.Traditional` (hard cutoff below `Value`) or `DeadzoneMethod.AdaptiveGradient` (scaled recentering) over a raw axis reading.
- `Hat.Position` is the `Position2D` bitfield (`Up|Left = UpLeft`); `Thumbstick` derives polar `Position`/`Direction` from `X`/`Y`; `Trigger`/`Axis` carry a single normalized `Position`.

[LOCAL_ADMISSION]:
- The backend is reflection-loaded: `InputWindowExtensions` scans for an `[InputPlatform]`-marked assembly via `TryAdd("Silk.NET.Input.Sdl")` (and `Silk.NET.Input.Glfw`), so the AppUi Shell references `Silk.NET.Input.Sdl` to make the SDL2 osx-arm64 backend resolvable before the first `CreateInput()`; `ShouldLoadFirstPartyPlatforms(false)` plus explicit `Add` pins a single backend.
- `IInputContext` is the one disposed input owner per view; the AppUi InputFabric pairs `CreateInput()` and `Dispose()` in a scoped boundary capsule and never holds device references past context disposal.
- Per-frame InputFabric reads the immutable state carriers (`Button`, `Thumbstick`, `Trigger`, `Axis`, `Hat`) and folds them into canonical input facts; the device `event Action<...>` streams feed edge-triggered actions, never a second polling loop over the same state.
- Rumble routes through `IGamepad.VibrationMotors[i].Speed`; haptic intensity beyond linear motor speed is a `Silk.NET.SDL` concern, not this abstraction.

[RAIL_LAW]:
- Package: `Silk.NET.Input` (+ `Silk.NET.Input.Common`, `Silk.NET.Input.Sdl`)
- Owns: the input device abstraction — `IInputContext` aggregation, `IGamepad`/`IJoystick`/`IKeyboard`/`IMouse` state and event streams, `Deadzone` policy, and the `CreateInput` entrypoint over the reflection-loaded SDL2 backend for osx-arm64.
- Accept: one `IInputContext` per view through `IView.CreateInput()`; both polled state carriers and per-device `event Action<...>` subscriptions over the same backend state; named gamepad controls through `GamepadExtensions`.
- Reject: a hand-rolled SDL controller poll loop beside `IInputContext`; renaming `IGamepad`/`IJoystick` into a parallel device model; mixing keyboard/mouse focus or windowing concerns into the input rail (those stay in `Silk.NET.Windowing`).
