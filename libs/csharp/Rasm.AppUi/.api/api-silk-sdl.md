# [RASM_APPUI_API_SILK_SDL]

`Silk.NET.SDL` is the managed SDL2 binding generated against `SDL.h`: `Sdl.GetApi()` returns one static `Sdl` function-table root whose unsafe instance methods P/Invoke the native SDL2 runtime over raw pointers. `InputFabric` consumes the `SDL_Haptic` force-feedback surface — device open/close, rumble, `HapticEffect` upload and run — with the joystick and game-controller `GUID` identity surface, over the single SDL2 native bundle shared with `Silk.NET.Input`, one `Sdl` handle per process.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.SDL`
- package: `Silk.NET.SDL` (MIT)
- assembly: `Silk.NET.SDL`
- namespace: `Silk.NET.SDL`
- asset: managed runtime library + native SDL2 binaries resolved through `Silk.NET.SDL.targets`
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and native handles

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :--------------- | :------------ | :---------------------------- |
|  [01]   | `Sdl`            | class         | function-table root           |
|  [02]   | `Haptic`         | struct        | force-feedback device handle  |
|  [03]   | `Joystick`       | struct        | raw joystick device handle    |
|  [04]   | `GameController` | struct        | mapped game-controller handle |

[PUBLIC_TYPE_SCOPE]: haptic effect carriers

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :---------------- | :------------ | :------------------------------- |
|  [01]   | `HapticEffect`    | union         | effect upload discriminant       |
|  [02]   | `HapticConstant`  | struct        | constant force                   |
|  [03]   | `HapticPeriodic`  | struct        | sine/triangle/sawtooth waveform  |
|  [04]   | `HapticCondition` | struct        | spring/damper/inertia/friction   |
|  [05]   | `HapticRamp`      | struct        | ramped force                     |
|  [06]   | `HapticLeftRight` | struct        | dual-motor rumble                |
|  [07]   | `HapticCustom`    | struct        | sample-defined force             |
|  [08]   | `HapticDirection` | struct        | polar/cartesian/spherical vector |

[PUBLIC_TYPE_SCOPE]: identity and device enums

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :--------------------- | :------------ | :----------------------------- |
|  [01]   | `GUID`                 | struct        | 16-byte stable device identity |
|  [02]   | `JoystickType`         | enum          | joystick classification        |
|  [03]   | `JoystickPowerLevel`   | enum          | battery state                  |
|  [04]   | `GameControllerType`   | enum          | controller classification      |
|  [05]   | `GameControllerAxis`   | enum          | axis identifier                |
|  [06]   | `GameControllerButton` | enum          | button identifier              |
|  [07]   | `SensorType`           | enum          | controller sensor kind         |
|  [08]   | `SdlBool`              | enum          | native tri-state boolean       |
|  [09]   | `SdlException`         | class         | typed wrap of native error     |

## [03]-[ENTRYPOINTS]

Every surface is an unsafe instance method on the `Sdl.GetApi()` root; only the static `GetApi()` factory mints it.

[ENTRYPOINT_SCOPE]: API root and subsystem lifecycle

| [INDEX] | [SURFACE]             | [SHAPE]  | [CAPABILITY]             |
| :-----: | :-------------------- | :------- | :----------------------- |
|  [01]   | `Sdl.GetApi()`        | static   | API root load            |
|  [02]   | `Init(uint)`          | instance | subsystem init           |
|  [03]   | `InitSubSystem(uint)` | instance | haptic subsystem init    |
|  [04]   | `WasInit(uint)`       | instance | subsystem presence query |
|  [05]   | `QuitSubSystem(uint)` | instance | subsystem teardown       |

[ENTRYPOINT_SCOPE]: haptic device lifecycle and query

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------- | :------- | :------------------------- |
|  [01]   | `NumHaptics()`                               | instance | device count               |
|  [02]   | `HapticName(int)` / `HapticNameS(int)`       | instance | device name by index       |
|  [03]   | `HapticOpen(int)`                            | instance | open device by index       |
|  [04]   | `HapticOpenFromJoystick(Joystick*)`          | instance | open device from joystick  |
|  [05]   | `HapticOpenFromMouse()`                      | instance | open mouse haptic          |
|  [06]   | `HapticOpened(int)` / `HapticIndex(Haptic*)` | instance | open-state and index query |
|  [07]   | `JoystickIsHaptic(Joystick*)`                | instance | joystick haptic capability |
|  [08]   | `MouseIsHaptic()`                            | instance | mouse haptic capability    |
|  [09]   | `HapticQuery(Haptic*)`                       | instance | supported-effect bitmask   |
|  [10]   | `HapticNumAxes(Haptic*)`                     | instance | axis count                 |
|  [11]   | `HapticNumEffects(Haptic*)`                  | instance | effect storage capacity    |
|  [12]   | `HapticNumEffectsPlaying(Haptic*)`           | instance | active-effect count        |
|  [13]   | `HapticClose(Haptic*)`                       | instance | device close               |

[ENTRYPOINT_SCOPE]: effect upload, run, and simple rumble

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `HapticEffectSupported(Haptic*, ref HapticEffect)`                           | instance | effect-kind capability check |
|  [02]   | `HapticNewEffect(Haptic*, ref HapticEffect)`                                 | instance | upload effect, return id     |
|  [03]   | `HapticUpdateEffect(Haptic*, int, ref HapticEffect)`                         | instance | re-upload effect data        |
|  [04]   | `HapticRunEffect(Haptic*, int, uint)`                                        | instance | play uploaded effect         |
|  [05]   | `HapticStopEffect(Haptic*, int)`                                             | instance | stop one effect              |
|  [06]   | `HapticGetEffectStatus(Haptic*, int)`                                        | instance | effect play-state query      |
|  [07]   | `HapticDestroyEffect(Haptic*, int)`                                          | instance | free effect slot             |
|  [08]   | `HapticSetGain(Haptic*, int)`                                                | instance | master gain set              |
|  [09]   | `HapticSetAutocenter(Haptic*, int)`                                          | instance | auto-center set              |
|  [10]   | `HapticPause(Haptic*)` / `HapticUnpause(Haptic*)` / `HapticStopAll(Haptic*)` | instance | device-wide playback control |
|  [11]   | `HapticRumbleSupported(Haptic*)`                                             | instance | simple-rumble capability     |
|  [12]   | `HapticRumbleInit(Haptic*)`                                                  | instance | init the simple-rumble path  |
|  [13]   | `HapticRumblePlay(Haptic*, float, uint)`                                     | instance | play simple rumble           |
|  [14]   | `HapticRumbleStop(Haptic*)`                                                  | instance | stop simple rumble           |

[ENTRYPOINT_SCOPE]: controller and joystick rumble shortcut

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `GameControllerRumble(GameController*, ushort, ushort, uint)`           | instance | dual-motor rumble           |
|  [02]   | `GameControllerRumbleTriggers(GameController*, ushort, ushort, uint)`   | instance | trigger rumble              |
|  [03]   | `GameControllerHasRumble(GameController*)`                              | instance | rumble capability           |
|  [04]   | `GameControllerHasRumbleTriggers(GameController*)`                      | instance | trigger-rumble capability   |
|  [05]   | `JoystickRumble(Joystick*, ushort, ushort, uint)`                       | instance | low-level dual-motor rumble |
|  [06]   | `JoystickRumbleTriggers(Joystick*, ushort, ushort, uint)`               | instance | low-level trigger rumble    |
|  [07]   | `JoystickHasRumble(Joystick*)` / `JoystickHasRumbleTriggers(Joystick*)` | instance | rumble capability query     |

[ENTRYPOINT_SCOPE]: device GUID identity surface

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `NumJoysticks()` / `IsGameController(int)`                                          | instance | enumerate and classify        |
|  [02]   | `JoystickGetDeviceGUID(int)`                                                        | instance | identity before open          |
|  [03]   | `JoystickGetGUID(Joystick*)`                                                        | instance | identity after open           |
|  [04]   | `GUIDToString(GUID, byte*, int)` / `JoystickGetGUIDString`                          | instance | GUID to text                  |
|  [05]   | `GUIDFromString(byte*)` / `JoystickGetGUIDFromString`                               | instance | text to GUID                  |
|  [06]   | `GetJoystickGUIDInfo(GUID, ushort*, ushort*, ushort*, ushort*)`                     | instance | decompose GUID fields         |
|  [07]   | `JoystickOpen(int)` / `GameControllerOpen(int)`                                     | instance | device open                   |
|  [08]   | `GameControllerMappingForGUID(GUID)` / `GameControllerMappingForGUIDS(GUID)`        | instance | resolve mapping (raw/managed) |
|  [09]   | `GameControllerAddMapping(string)` / `GameControllerAddMappingsFromRW(RWops*, int)` | instance | register mapping(s)           |

[ENTRYPOINT_SCOPE]: native-status error lift

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `GetError() -> byte*`                    | instance | raw last-error pointer                |
|  [02]   | `GetErrorS() -> string`                  | instance | managed last-error text               |
|  [03]   | `GetErrorAsException() -> SdlException?` | instance | typed exception for the boundary lift |
|  [04]   | `ClearError()`                           | instance | reset error state before a call       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Sdl.GetApi()` mints the native function-table root; every native call is an unsafe instance method taking raw pointers, so the call site marshals `stackalloc`/`Span<T>` structs and passes pointers, never a managed wrapper.
- `Init(InitHaptic)` or `InitSubSystem(InitHaptic)` arms the haptic subsystem before any `HapticOpen`; `WasInit` confirms and `QuitSubSystem` tears down.
- `HapticEffect` is the tagged union over the constant, periodic, condition, ramp, left-right, and custom effect structs; its leading `ushort Type` field selects the active member from the effect-type bitmask.
- `HapticQuery` returns the supported-effect bitmask with the gain, auto-center, and status feature flags; every `HapticNewEffect` upload gates behind a `HapticEffectSupported` or `HapticQuery` mask test.
- `HapticDirection` carries a coordinate-code `byte Type` and a `fixed int Dir[3]` vector; the periodic, constant, condition, and ramp effects embed it.
- Effect lifecycle runs `HapticNewEffect` (returns an int slot id), then `HapticRunEffect`/`HapticUpdateEffect`, then `HapticStopEffect`, then `HapticDestroyEffect`; a `HapticInfinity` iteration count loops until stopped.
- Simple rumble (`HapticRumbleInit` then `HapticRumblePlay`) and the device-level `GameControllerRumble`/`JoystickRumble` shortcuts bypass `HapticEffect` upload; capability reads through `HapticRumbleSupported`, `GameControllerHasRumble`, and `JoystickHasRumble`.
- `GUID` is a 16-byte `fixed byte Data[16]` value surviving reconnect; `JoystickGetDeviceGUID` reads identity before open and `JoystickGetGUID` after, with `GetJoystickGUIDInfo` decomposing vendor, product, version, and CRC for matching against the `GameControllerMappingForGUID` database.

[STACKING]:
- `api-silk-input`(`.api/api-silk-input.md`): `Silk.NET.Input.Sdl`'s controller backend and this haptic root P/Invoke one shared SDL2 native bundle, so `InputFabric` binds a single `Sdl.GetApi()` handle per process across both packages, never two.
- `api-hidsharp`(`.api/api-hidsharp.md`) + `api-drywetmidi`(`.api/api-drywetmidi.md`): SDL2 haptic and gamepad streams co-lift with the HidSharp SpaceMouse stream and the DryWetMidi device stream under one `InputFabric` edge into canonical input shapes.
- `InputFabric`: folds haptic device open/close, effect upload/run, and rumble onto the input rail as one boundary capsule, lifting SDL's int-status through `GetErrorAsException()` and never crossing a raw negative status into domain code.

[LOCAL_ADMISSION]:
- Haptic devices, joysticks, and controllers open through their `XxxOpen` call and close through `HapticClose`/`JoystickClose`/`GameControllerClose` matched in a scoped fold; the native handles carry no `IDisposable`.
- Native results carry SDL's int-status convention (`0` success, negative failure); the boundary capsule lifts a negative status through `GetErrorAsException()` or `GetErrorS()` into a typed input-rail failure, and `ClearError()` resets state before an attribution-sensitive sequence.
- Device identity persists as the `GUID` value, never the volatile device or instance index; reconnect matching resolves through `GetJoystickGUIDInfo` with `GameControllerMappingForGUID`.

[RAIL_LAW]:
- Package: `Silk.NET.SDL`
- Owns: the managed SDL2 binding for force-feedback — `SDL_Haptic` open/close, effect upload and run, simple rumble, controller and joystick rumble — and the joystick and game-controller `GUID` identity surface.
- Accept: raw-pointer unsafe calls on the single `Sdl.GetApi()` root; `InitHaptic` arming before device open; native-handle scoped open-close pairs; native int-status lifted through `GetErrorAsException()`; composition under the one `InputFabric` edge beside `api-silk-input`, `api-hidsharp`, and `api-drywetmidi`.
- Reject: a second `Sdl` instance or reloaded native bundle beside `Silk.NET.Input`; a managed wrapper renaming the native surface; device-index identity persisted over the stable `GUID`; a raw negative native status crossing into domain code.
