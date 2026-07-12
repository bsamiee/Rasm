# [RASM_APPUI_API_SILK_SDL]

`Silk.NET.SDL` is the managed SDL2 binding generated against `SDL.h`: `Sdl.GetApi()` returns one static `Sdl` function-table root whose unsafe instance methods P/Invoke the native SDL2 runtime over raw pointers. The InputFabric consumes the `SDL_Haptic` force-feedback surface (open/close, rumble, `HapticEffect` upload/run) plus the joystick and game-controller `GUID` identity surface; the native SDL2 bundle is shared with `Silk.NET.Input`, so only one `Sdl` instance binds the runtime per process.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.SDL`

- package: `Silk.NET.SDL`
- license: `MIT`
- assembly: `Silk.NET.SDL`
- consumer-tfm: `net6.0` (package multi-targets `net6.0`/`net5.0`/`netcoreapp3.1`/`netstandard2.1`/`netstandard2.0`; `net10.0` binds the highest available `net6.0` asset)
- native: SDL2 runtime resolved through `Silk.NET.SDL.targets`; the bundle is shared with `Silk.NET.Input` (`api-silk-input`) and bound once per process
- namespace: `Silk.NET.SDL`
- asset: runtime library + shared native SDL2 binaries
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and native handles

- rail: input

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [RAIL]                        |
| :-----: | :--------------- | :-------------- | :---------------------------- |
|  [01]   | `Sdl`            | static API root | global entry, function table  |
|  [02]   | `Haptic`         | native handle   | force-feedback device         |
|  [03]   | `Joystick`       | native handle   | raw joystick device           |
|  [04]   | `GameController` | native handle   | mapped game-controller device |

[PUBLIC_TYPE_SCOPE]: haptic effect carriers

- rail: input

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                           |
| :-----: | :---------------- | :--------------- | :------------------------------- |
|  [01]   | `HapticEffect`    | union struct     | effect upload discriminant       |
|  [02]   | `HapticConstant`  | effect struct    | constant force                   |
|  [03]   | `HapticPeriodic`  | effect struct    | sine/triangle/sawtooth waveform  |
|  [04]   | `HapticCondition` | effect struct    | spring/damper/inertia/friction   |
|  [05]   | `HapticRamp`      | effect struct    | ramped force                     |
|  [06]   | `HapticLeftRight` | effect struct    | dual-motor rumble                |
|  [07]   | `HapticCustom`    | effect struct    | sample-defined force             |
|  [08]   | `HapticDirection` | direction struct | polar/cartesian/spherical vector |

[PUBLIC_TYPE_SCOPE]: identity and device enums

- rail: input

| [INDEX] | [SYMBOL]               | [KIND] | [RAIL]                         |
| :-----: | :--------------------- | :----- | :----------------------------- |
|  [01]   | `GUID`                 | struct | 16-byte stable device identity |
|  [02]   | `JoystickType`         | enum   | joystick classification        |
|  [03]   | `JoystickPowerLevel`   | enum   | battery state                  |
|  [04]   | `GameControllerType`   | enum   | controller classification      |
|  [05]   | `GameControllerAxis`   | enum   | axis identifier                |
|  [06]   | `GameControllerButton` | enum   | button identifier              |
|  [07]   | `SensorType`           | enum   | controller sensor kind         |
|  [08]   | `SdlBool`              | enum   | native tri-state boolean       |
|  [09]   | `SdlException`         | class  | typed wrap of native error     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: API root and subsystem lifecycle

- rail: input

| [INDEX] | [SURFACE]             | [SURFACE_ROOT] | [RAIL]                      |
| :-----: | :-------------------- | :------------- | :-------------------------- |
|  [01]   | `Sdl.GetApi()`        | `Sdl`          | API root load               |
|  [02]   | `Init(uint flags)`    | `Sdl`          | subsystem init              |
|  [03]   | `InitSubSystem(uint)` | `Sdl`          | `InitHaptic` subsystem init |
|  [04]   | `WasInit(uint)`       | `Sdl`          | subsystem presence query    |
|  [05]   | `QuitSubSystem(uint)` | `Sdl`          | subsystem teardown          |

[ENTRYPOINT_SCOPE]: haptic device lifecycle and query

- rail: input

| [INDEX] | [SURFACE]                              | [SURFACE_ROOT] | [RAIL]                     |
| :-----: | :------------------------------------- | :------------- | :------------------------- |
|  [01]   | `NumHaptics()`                         | `Sdl`          | device count               |
|  [02]   | `HapticName(int)` / `HapticNameS(int)` | `Sdl`          | device name by index       |
|  [03]   | `HapticOpen(int device_index)`         | `Sdl`          | open device by index       |
|  [04]   | `HapticOpenFromJoystick(Joystick*)`    | `Sdl`          | open device from joystick  |
|  [05]   | `HapticOpenFromMouse()`                | `Sdl`          | open mouse haptic          |
|  [06]   | `HapticOpened(int)` / `HapticIndex(*)` | `Sdl`          | open-state and index query |
|  [07]   | `JoystickIsHaptic(Joystick*)`          | `Sdl`          | joystick haptic capability |
|  [08]   | `MouseIsHaptic()`                      | `Sdl`          | mouse haptic capability    |
|  [09]   | `HapticQuery(Haptic*)`                 | `Sdl`          | supported-effect bitmask   |
|  [10]   | `HapticNumAxes(Haptic*)`               | `Sdl`          | axis count                 |
|  [11]   | `HapticNumEffects(Haptic*)`            | `Sdl`          | effect storage capacity    |
|  [12]   | `HapticNumEffectsPlaying(Haptic*)`     | `Sdl`          | active-effect count        |
|  [13]   | `HapticClose(Haptic*)`                 | `Sdl`          | device close               |

[ENTRYPOINT_SCOPE]: effect upload, run, and simple rumble

- rail: input

| [INDEX] | [SURFACE]                                                | [SURFACE_ROOT] | [RAIL]                       |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `HapticEffectSupported(Haptic*, ref HapticEffect)`       | `Sdl`          | effect-kind capability check |
|  [02]   | `HapticNewEffect(Haptic*, ref HapticEffect)`             | `Sdl`          | upload effect, return id     |
|  [03]   | `HapticUpdateEffect(Haptic*, int, ref HapticEffect)`     | `Sdl`          | re-upload effect data        |
|  [04]   | `HapticRunEffect(Haptic*, int, uint iterations)`         | `Sdl`          | play uploaded effect         |
|  [05]   | `HapticStopEffect(Haptic*, int)`                         | `Sdl`          | stop one effect              |
|  [06]   | `HapticGetEffectStatus(Haptic*, int)`                    | `Sdl`          | effect play-state query      |
|  [07]   | `HapticDestroyEffect(Haptic*, int)`                      | `Sdl`          | free effect slot             |
|  [08]   | `HapticSetGain(Haptic*, int)`                            | `Sdl`          | master gain set              |
|  [09]   | `HapticSetAutocenter(Haptic*, int)`                      | `Sdl`          | auto-center set              |
|  [10]   | `HapticPause` / `HapticUnpause` / `HapticStopAll`        | `Sdl`          | device-wide playback control |
|  [11]   | `HapticRumbleSupported(Haptic*)`                         | `Sdl`          | simple-rumble capability     |
|  [12]   | `HapticRumbleInit(Haptic*)`                              | `Sdl`          | init the simple-rumble path  |
|  [13]   | `HapticRumblePlay(Haptic*, float strength, uint length)` | `Sdl`          | play simple rumble           |
|  [14]   | `HapticRumbleStop(Haptic*)`                              | `Sdl`          | stop simple rumble           |

[ENTRYPOINT_SCOPE]: controller and joystick rumble shortcut

- rail: input

| [INDEX] | [SURFACE]                                                                    | [SURFACE_ROOT] | [RAIL]                      |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `GameControllerRumble(GameController*, ushort lo, ushort hi, uint ms)`       | `Sdl`          | dual-motor rumble           |
|  [02]   | `GameControllerRumbleTriggers(GameController*, ushort l, ushort r, uint ms)` | `Sdl`          | trigger rumble              |
|  [03]   | `GameControllerHasRumble(GameController*)`                                   | `Sdl`          | rumble capability           |
|  [04]   | `GameControllerHasRumbleTriggers(GameController*)`                           | `Sdl`          | trigger-rumble capability   |
|  [05]   | `JoystickRumble(Joystick*, ushort lo, ushort hi, uint ms)`                   | `Sdl`          | low-level dual-motor rumble |
|  [06]   | `JoystickRumbleTriggers(Joystick*, ushort l, ushort r, uint ms)`             | `Sdl`          | low-level trigger rumble    |
|  [07]   | `JoystickHasRumble(Joystick*)` / `JoystickHasRumbleTriggers(Joystick*)`      | `Sdl`          | rumble capability           |

[ENTRYPOINT_SCOPE]: device GUID identity surface

- rail: input

| [INDEX] | [SURFACE]                                                                           | [SURFACE_ROOT] | [RAIL]                           |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `NumJoysticks()` / `IsGameController(int)`                                          | `Sdl`          | enumerate and classify           |
|  [02]   | `JoystickGetDeviceGUID(int device_index)`                                           | `Sdl`          | identity before open             |
|  [03]   | `JoystickGetGUID(Joystick*)`                                                        | `Sdl`          | identity after open              |
|  [04]   | `GUIDToString(GUID, byte*, int)` / `JoystickGetGUIDString`                          | `Sdl`          | GUID to text                     |
|  [05]   | `GUIDFromString(byte*)` / `JoystickGetGUIDFromString`                               | `Sdl`          | text to GUID                     |
|  [06]   | `GetJoystickGUIDInfo(GUID, ushort* vendor, product, version, crc16)`                | `Sdl`          | decompose GUID fields            |
|  [07]   | `JoystickOpen(int)` / `GameControllerOpen(int)`                                     | `Sdl`          | device open                      |
|  [08]   | `GameControllerMappingForGUID(GUID)` / `GameControllerMappingForGUIDS(GUID)`        | `Sdl`          | resolve mapping (`byte*`/string) |
|  [09]   | `GameControllerAddMapping(string)` / `GameControllerAddMappingsFromRW(RWops*, int)` | `Sdl`          | register mapping(s)              |

[ENTRYPOINT_SCOPE]: native-status error lift

- rail: input

| [INDEX] | [SURFACE]                                 | [SURFACE_ROOT] | [RAIL]                                |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `GetError()` (`byte*`)                    | `Sdl`          | raw last-error pointer                |
|  [02]   | `GetErrorS()` (`string`)                  | `Sdl`          | managed last-error text               |
|  [03]   | `GetErrorAsException()` (`SdlException?`) | `Sdl`          | typed exception for the boundary lift |
|  [04]   | `ClearError()`                            | `Sdl`          | reset error state before a call       |

## [04]-[IMPLEMENTATION_LAW]

[SDL_TOPOLOGY]:

- `Sdl.GetApi()` returns the native function-table root; every native call is an unsafe instance method on that `Sdl` object taking raw pointers — the call site marshals `stackalloc`/`Span<T>` structs and passes pointers, never a managed wrapper object.
- `Init(InitHaptic)` (`4096u`) or `InitSubSystem(InitHaptic)` arms the haptic subsystem before any `HapticOpen` call; `WasInit` confirms and `QuitSubSystem` tears down.
- `HapticEffect` is the tagged union over `HapticConstant`, `HapticPeriodic`, `HapticCondition`, `HapticRamp`, `HapticLeftRight`, and `HapticCustom`; its leading `ushort Type` field selects the active member and must match one of the `HapticConstant`=`1`, `HapticSine`=`2`, `HapticLeftright`=`4`, `HapticTriangle`=`8`, `HapticSawtoothup`=`16`, `HapticSawtoothdown`=`32`, `HapticSpring`=`128`, `HapticDamper`=`256`, `HapticInertia`=`512`, `HapticFriction`=`1024`, `HapticCustom`=`2048` constants.
- `HapticQuery` returns the same effect-type bitmask plus the `HapticGain`=`4096`, `HapticAutocenter`=`8192`, and `HapticStatus`=`16384` feature flags; gate every `HapticNewEffect` upload behind a `HapticEffectSupported` or `HapticQuery` mask test.
- `HapticDirection` carries a `byte Type` coordinate code (`HapticPolar`=`0`, `HapticCartesian`=`1`, `HapticSpherical`=`2`, `HapticSteeringAxis`=`3`) and a `fixed int Dir[3]` vector; periodic, constant, condition, and ramp effects embed it.
- Effect lifecycle is `HapticNewEffect` (returns an int slot id) -> `HapticRunEffect`/`HapticUpdateEffect` -> `HapticStopEffect` -> `HapticDestroyEffect`; `HapticRunEffect` iterations of `HapticInfinity`=`uint.MaxValue` loop until stopped.
- The simple-rumble path (`HapticRumbleInit` then `HapticRumblePlay(strength, length)`) and the device-level `GameControllerRumble`/`JoystickRumble` shortcuts bypass `HapticEffect` upload entirely; capability is read through `HapticRumbleSupported`, `GameControllerHasRumble`, and `JoystickHasRumble`.
- `GUID` is a 16-byte `fixed byte Data[16]` value that survives reconnect; `JoystickGetDeviceGUID` reads identity before open and `JoystickGetGUID` after, with `GetJoystickGUIDInfo` decomposing vendor, product, version, and CRC for device matching against the `GameControllerMappingForGUID` mapping database.

[LOCAL_ADMISSION]:

- One `Sdl` instance binds the native runtime per process; the InputFabric shares the single SDL2 native bundle with `Silk.NET.Input`, so the boundary capsule owns exactly one `Sdl.GetApi()` handle rather than reloading the binding.
- The SDL2 haptic rail composes with the SDL2 gamepad stream, HidSharp SpaceMouse stream, and DryWetMidi device stream under one InputFabric edge that lifts every device into canonical input shapes.
- Haptic devices, joysticks, and controllers open through their `XxxOpen` call and close through `HapticClose`/`JoystickClose`/`GameControllerClose` matched in a scoped fold; there is no `IDisposable` on the native handles.
- Native call results carry SDL's int-status convention (`0` success, negative failure); the boundary capsule lifts a negative status through `GetErrorAsException()` (or reads `GetErrorS()` text), surfaces it as a typed input-rail failure, and never lets a raw negative status cross into domain code. `ClearError()` resets state before a sequence of unsafe calls when error attribution matters.
- Device identity persists as the `GUID` byte value, not the volatile device or instance index; reconnect matching resolves through `GetJoystickGUIDInfo` plus `GameControllerMappingForGUID`/`GameControllerMappingForGUIDS`.

[RAIL_LAW]:

- Package: `Silk.NET.SDL`
- Owns: the managed SDL2 binding for force-feedback (`SDL_Haptic` open/close, effect upload/run, simple rumble, controller and joystick rumble) and the joystick and game-controller `GUID` identity surface.
- Accept: raw-pointer unsafe calls on the single `Sdl.GetApi()` function-table root; `InitHaptic` subsystem arming before device open; native-handle scoped open-and-close pairs at the boundary capsule; native int-status lifted through `GetErrorAsException()`/`GetErrorS()`; composition under the one InputFabric edge beside `api-silk-input`, `api-hidsharp`, and `api-drywetmidi`.
- Reject: a second `Sdl` instance or re-loaded native bundle beside `Silk.NET.Input`; a managed convenience wrapper renaming the native surface; device-index identity persisted in place of the stable `GUID`; a raw negative native status crossing into domain code.
