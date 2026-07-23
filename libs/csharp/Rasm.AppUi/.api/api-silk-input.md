# [RASM_APPUI_API_SILK_INPUT]

`Silk.NET.Input` is the cross-platform input abstraction: `IView.CreateInput()` mints an `IInputContext` whose device lists expose immutable poll state and per-device event streams over one backend-updated state. `Silk.NET.Input.Common` owns the public contracts and carriers, while the meta-package carries no assembly and admits SDL2 or GLFW backends. `[InputPlatform]` registers `Silk.NET.Input.Sdl` reflectively for the osx-arm64 controller path; that backend and the `Silk.NET.SDL` haptic root P/Invoke one SDL2 runtime, so AppUi `InputFabric` owns a single per-process native binding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.Input`
- package: `Silk.NET.Input` (MIT)
- package: `Silk.NET.Input.Common` (MIT)
- package: `Silk.NET.Input.Glfw` (— `[InputPlatform]` backend, reflection-loaded)
- package: `Silk.NET.Input.Sdl` (— `[InputPlatform]` backend, osx-arm64 controller/haptic path)
- assembly: `Silk.NET.Input.Common`
- namespace: `Silk.NET.Input` (single public namespace)
- asset: runtime library + reflection-loaded backend assemblies (`Silk.NET.Input.Sdl`, `Silk.NET.Input.Glfw`)
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and device contracts
- rail: input

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [RAIL]                                                    |
| :-----: | :--------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `IInputContext`  | disposable root  | per-view device aggregator + `Handle`/`ConnectionChanged` |
|  [02]   | `IInputDevice`   | device base      | `Name`/`Index`/`IsConnected` identity                     |
|  [03]   | `IGamepad`       | device contract  | mapped controller (buttons/sticks/triggers/motors)        |
|  [04]   | `IJoystick`      | device contract  | raw axes/buttons/hats                                     |
|  [05]   | `IKeyboard`      | device contract  | key state, char intake, `ClipboardText`, IME              |
|  [06]   | `IMouse`         | device contract  | position, buttons, scroll, double-click, cursor           |
|  [07]   | `IMotor`         | actuator         | `Index` + per-motor rumble `Speed`                        |
|  [08]   | `ICursor`        | cursor handle    | mode/type/standard shape/image/confine/hotspot            |
|  [09]   | `IInputPlatform` | backend contract | `IsApplicable(IView)` + `CreateInput(IView)`              |

[PUBLIC_TYPE_SCOPE]: input value carriers (immutable poll state)
- rail: input

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]         | [RAIL]                                                |
| :-----: | :------------ | :-------------------- | :---------------------------------------------------- |
|  [01]   | `Button`      | struct                | `Name`(`ButtonName`)/`Index`/`Pressed`                |
|  [02]   | `Thumbstick`  | struct                | `Index`/`X`/`Y`; derived polar `Position`/`Direction` |
|  [03]   | `Trigger`     | struct                | `Index`/`Position`                                    |
|  [04]   | `Axis`        | readonly struct       | `Index`/`Position`                                    |
|  [05]   | `Hat`         | struct                | `Index`/`Position`(`Position2D` bitfield)             |
|  [06]   | `Deadzone`    | readonly struct       | `Value`/`Method` + `Apply(float raw)` fold            |
|  [07]   | `ScrollWheel` | struct (`IEquatable`) | `X`/`Y` scroll delta                                  |

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

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                               |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `InputWindowExtensions`  | static class  | `CreateInput` + platform registry                                    |
|  [02]   | `GamepadExtensions`      | static class  | named-button/stick accessors (throw `PlatformNotSupportedException`) |
|  [03]   | `InputPlatformAttribute` | assembly attr | backend discovery marker                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: context creation and platform registry
- rail: input

| [INDEX] | [SURFACE]                                        | [SURFACE_ROOT]          | [RAIL]                                         |
| :-----: | :----------------------------------------------- | :---------------------- | :--------------------------------------------- |
|  [01]   | `IView.CreateInput()`                            | `InputWindowExtensions` | mint `IInputContext`                           |
|  [02]   | `Platforms`                                      | `InputWindowExtensions` | registered backend list (lazy reflection load) |
|  [03]   | `ShouldLoadFirstPartyPlatforms(bool)`            | `InputWindowExtensions` | toggle reflection load (throws after load)     |
|  [04]   | `Add(IInputPlatform)` / `Remove(IInputPlatform)` | `InputWindowExtensions` | manual backend registry                        |
|  [05]   | `TryAdd(string assemblyName)`                    | `InputWindowExtensions` | reflective backend load                        |

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

| [INDEX] | [SURFACE]                                                                   | [ROOT]                 | [RAIL]                      |
| :-----: | :-------------------------------------------------------------------------- | :--------------------- | :-------------------------- |
|  [01]   | `Gamepads` / `Joysticks` / `Keyboards` / `Mice` / `OtherDevices`            | `IInputContext`        | live device lists           |
|  [02]   | `Handle`                                                                    | `IInputContext`        | native pointer              |
|  [03]   | `ConnectionChanged` (`Action<IInputDevice, bool>`)                          | `IInputContext`        | hotplug event               |
|  [04]   | `Buttons` / `Thumbsticks` / `Triggers` / `VibrationMotors`                  | `IGamepad`             | mapped controller state     |
|  [05]   | `ButtonDown` / `ButtonUp` / `ThumbstickMoved` / `TriggerMoved`              | `IGamepad`             | gamepad event stream        |
|  [06]   | `Axes` / `Buttons` / `Hats`                                                 | `IJoystick`            | raw joystick state          |
|  [07]   | `ButtonDown` / `ButtonUp` / `AxisMoved` / `HatMoved`                        | `IJoystick`            | joystick event stream       |
|  [08]   | `Deadzone` (get/set)                                                        | `IGamepad`/`IJoystick` | stick deadzone policy       |
|  [09]   | `SupportedKeys` / `IsKeyPressed(Key)` / `IsScancodePressed(int)`            | `IKeyboard`            | key roster and poll         |
|  [10]   | `KeyDown` / `KeyUp` / `KeyChar` / `ClipboardText`                           | `IKeyboard`            | key events and clipboard    |
|  [11]   | `BeginInput()` / `EndInput()`                                               | `IKeyboard`            | IME lifecycle               |
|  [12]   | `Position`(`Vector2`) / `SupportedButtons` / `IsButtonPressed(MouseButton)` | `IMouse`               | position and button poll    |
|  [13]   | `ScrollWheels` / `Cursor`                                                   | `IMouse`               | scroll and cursor state     |
|  [14]   | `MouseDown` / `MouseUp` / `Click` / `DoubleClick`                           | `IMouse`               | button event stream         |
|  [15]   | `MouseMove` / `Scroll`                                                      | `IMouse`               | motion event stream         |
|  [16]   | `DoubleClickTime` / `DoubleClickRange`                                      | `IMouse`               | double-click policy         |
|  [17]   | `Type` / `StandardCursor` / `CursorMode` / `IsConfined`                     | `ICursor`              | cursor mode                 |
|  [18]   | `HotspotX` / `HotspotY` / `Image`(`RawImage`) / `IsSupported(...)`          | `ICursor`              | custom cursor configuration |
|  [19]   | `Index` / `Speed` (get/set)                                                 | `IMotor`               | rumble actuation            |

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
- `InputWindowExtensions.Platforms` reflection-loads `[InputPlatform]` assemblies through lazy `TryAdd("Silk.NET.Input.Sdl")` and `TryAdd("Silk.NET.Input.Glfw")`; an AppUi Shell reference makes the SDL2 osx-arm64 backend resolvable before first `CreateInput()`, while `ShouldLoadFirstPartyPlatforms(false)` and explicit `Add` pin one backend before discovery.
- Reflection-loaded `Silk.NET.Input.Sdl` and the `Silk.NET.SDL` (`.api/api-silk-sdl.md`) `Sdl.GetApi()` haptic root P/Invoke one SDL2 runtime, and `InputFabric` binds one native bundle per process.
- `IInputContext` controller streams and `SDL_Haptic` force feedback consume that binding without reload; the boundary capsule owns it.
- `IInputContext` is the one disposed input owner per view; the AppUi InputFabric pairs `CreateInput()` and `Dispose()` in a scoped boundary capsule and never holds device references past context disposal.
- Per-frame InputFabric reads the immutable state carriers (`Button`, `Thumbstick`, `Trigger`, `Axis`, `Hat`) and folds them into canonical input facts; the device `event Action<...>` streams feed edge-triggered actions, never a second polling loop over the same state.
- Rumble routes through `IGamepad.VibrationMotors[i].Speed`; haptic intensity beyond linear motor speed is a `Silk.NET.SDL` force-feedback concern (`.api/api-silk-sdl.md`), not this abstraction.

[RAIL_LAW]:
- Package: `Silk.NET.Input` (+ `Silk.NET.Input.Common`, `Silk.NET.Input.Sdl`)
- Owns: the input device abstraction — `IInputContext` aggregation, `IGamepad`/`IJoystick`/`IKeyboard`/`IMouse` state and event streams, `Deadzone` policy, and the `CreateInput` entrypoint over the reflection-loaded SDL2 backend for osx-arm64.
- Stacks: `DeviceDriver` folds `Gamepad(Silk.NET.Input controller)` and `Haptic(Silk.NET.SDL force-feedback)` beside `Hid(HidSharp SpaceMouse)` and `Midi(Melanchall.DryWetMidi)`; all four capsules bind one `InputFabric` edge onto the `CommandIntent` table. Gamepad faults map to `InputDriverFault` `DeviceAbsent`/`OpenRejected`, polled carriers fold into the canonical input fact, and `Gamepad` and `Haptic` share one SDL2 binding because both packages P/Invoke the same runtime.
- Accept: one `IInputContext` per view through `IView.CreateInput()`; both polled state carriers and per-device `event Action<...>` subscriptions over the same backend state; named gamepad controls through `GamepadExtensions`.
- Reject: a hand-rolled SDL controller poll loop beside `IInputContext`; a second SDL2 native binding re-loaded against the `Silk.NET.SDL` haptic root (the InputFabric owns one shared SDL2 bundle per process); a parallel device→intent edge beside the single `InputFabric` fold; renaming `IGamepad`/`IJoystick` into a parallel device model; mixing keyboard/mouse focus or windowing concerns into the input rail (those stay in `Silk.NET.Windowing`).
