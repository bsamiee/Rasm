# [RASM_APPUI_API_DRYWETMIDI]

`Melanchall.DryWetMidi` supplies the MIDI file/chunk object model, the channel and meta event family, the timed-event/note/chord interaction layer, the high-level transform tools (quantize/split/merge/resize/repeat), and the multimedia device, recording, and playback rails driven by a high-precision native multimedia clock. AppUi Shell input composes the device + interaction subset into the InputFabric MIDI surface alongside the Silk.NET SDL2 (`api-silk-sdl`), Silk.NET.Input (`api-silk-input`), and HidSharp (`api-hidsharp`) device rails: device intake, event send, file read/write, and the `TempoMap`-anchored timed note model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Melanchall.DryWetMidi`
- package: `Melanchall.DryWetMidi`
- license: `MIT`
- assembly: `Melanchall.DryWetMidi`
- consumer-tfm: `netstandard2.0` (package ships only `netstandard2.0`/`net45`; `net10.0` binds the `netstandard2.0` asset)
- namespace: `Melanchall.DryWetMidi.Multimedia`
- namespace: `Melanchall.DryWetMidi.Core`
- namespace: `Melanchall.DryWetMidi.Interaction`
- namespace: `Melanchall.DryWetMidi.Tools`
- namespace: `Melanchall.DryWetMidi.Common`
- asset: managed runtime library
- asset: native multimedia clock interop (`Melanchall_DryWetMidi_Native32.dll`, `_Native64.dll`, `_Native64.dylib`) copied to output by the package `targets`
- abi: native clock ships Windows x86/x64 and macOS x64 only — there is NO Linux `.so`; the multimedia `Playback`/`Recording`/`InputDevice`/`OutputDevice` rails are unavailable on the headless-Linux render path, where only the managed `Core`/`Interaction`/`Tools` (file, event, timed-model, transform) surface is usable
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: multimedia devices, clock, and rails
- rail: input

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [RAIL]                     |
| :-----: | :--------------------------------------------- | :--------------- | :------------------------- |
|  [01]   | `InputDevice`                                  | input device     | event intake               |
|  [02]   | `OutputDevice`                                 | output device    | event send                 |
|  [03]   | `IInputDevice` / `IOutputDevice`               | device contracts | device abstraction         |
|  [04]   | `DevicesConnector`                             | device link      | input-to-output bridge     |
|  [05]   | `DevicesWatcher`                               | device monitor   | hot-plug add/remove signal |
|  [06]   | `Recording`                                    | capture rail     | timed event capture        |
|  [07]   | `Playback`                                     | playback rail    | timed event scheduling     |
|  [08]   | `PlaybackSettings`                             | playback policy  | clock/snap/state config    |
|  [09]   | `MidiClock` / `MidiClockSettings`              | timer            | high-precision tick source |
|  [10]   | `PlaybackCurrentTimeWatcher`                   | time observer    | polled current-time signal |
|  [11]   | `MidiEventReceivedEventArgs`                   | event payload    | received event carrier     |
|  [12]   | `MidiEventSentEventArgs`                       | event payload    | sent event carrier         |
|  [13]   | `MidiEventPlayedEventArgs`                     | event payload    | played event carrier       |
|  [14]   | `NotesEventArgs`                               | event payload    | playback note carrier      |
|  [15]   | `InputDeviceProperty` / `OutputDeviceProperty` | device property  | capability query           |

[PUBLIC_TYPE_SCOPE]: file, chunk, and lazy-token model
- rail: input

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]    | [RAIL]                        |
| :-----: | :-------------------------------------- | :--------------- | :---------------------------- |
|  [01]   | `MidiFile`                              | file root        | read/write surface            |
|  [02]   | `MidiChunk`                             | chunk base       | chunk model                   |
|  [03]   | `TrackChunk`                            | track chunk      | event container               |
|  [04]   | `HeaderChunk`                           | header chunk     | format/division               |
|  [05]   | `UnknownChunk`                          | unknown chunk    | passthrough chunk             |
|  [06]   | `ChunksCollection`                      | chunk collection | file chunk list               |
|  [07]   | `EventsCollection`                      | event collection | track event list              |
|  [08]   | `MidiFileFormat`                        | format enum      | single/multi track            |
|  [09]   | `TimeDivision`                          | time division    | tick resolution               |
|  [10]   | `ReadingSettings` / `WritingSettings`   | policy           | parse/serialize configuration |
|  [11]   | `MidiTokensReader` / `MidiTokensWriter` | streaming token  | low-memory lazy IO            |

[PUBLIC_TYPE_SCOPE]: event family
- rail: input

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [RAIL]                |
| :-----: | :----------------------- | :-------------- | :-------------------- |
|  [01]   | `MidiEvent`              | event base      | abstract event root   |
|  [02]   | `MidiEventType`          | event kind enum | event discriminant    |
|  [03]   | `ChannelEvent`           | channel base    | channel-scoped event  |
|  [04]   | `NoteEvent`              | note base       | note number/velocity  |
|  [05]   | `NoteOnEvent`            | note-on event   | note start            |
|  [06]   | `NoteOffEvent`           | note-off event  | note stop             |
|  [07]   | `ControlChangeEvent`     | control event   | controller value      |
|  [08]   | `ProgramChangeEvent`     | program event   | patch select          |
|  [09]   | `PitchBendEvent`         | pitch event     | bend value            |
|  [10]   | `ChannelAftertouchEvent` | pressure event  | channel pressure      |
|  [11]   | `NoteAftertouchEvent`    | pressure event  | per-note pressure     |
|  [12]   | `MetaEvent`              | meta base       | non-channel metadata  |
|  [13]   | `SetTempoEvent`          | tempo meta      | tempo change          |
|  [14]   | `TimeSignatureEvent`     | signature meta  | meter change          |
|  [15]   | `SysExEvent`             | sysex base      | system-exclusive data |

[PUBLIC_TYPE_SCOPE]: interaction, note, and detection model
- rail: input

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [RAIL]                        |
| :-----: | :--------------------------------- | :--------------- | :---------------------------- |
|  [01]   | `TimedEvent`                       | timed event      | event plus tick time          |
|  [02]   | `Note`                             | note object      | timed lengthed note           |
|  [03]   | `Chord`                            | chord object     | simultaneous notes            |
|  [04]   | `ITimedObject` / `ILengthedObject` | contracts        | tick-positioned/duration      |
|  [05]   | `TempoMap`                         | tempo map        | tick<->metric/musical convert |
|  [06]   | `TimedObjectsManager`              | mutable view     | edit timed objects in place   |
|  [07]   | `TimedEventsManagingUtilities`     | timed-event lens | event extraction              |
|  [08]   | `NotesManagingUtilities`           | note lens        | note extraction               |
|  [09]   | `GetObjectsUtilities`              | object lens      | polymorphic extraction        |
|  [10]   | `ObjectType`                       | `[Flags]` kind   | extraction selector           |
|  [11]   | `ObjectDetectionSettings`          | detection policy | note/chord pairing rules      |
|  [12]   | `SevenBitNumber`                   | bounded value    | 0..127 data byte              |
|  [13]   | `FourBitNumber`                    | bounded value    | 0..15 channel index           |

[PUBLIC_TYPE_SCOPE]: high-level transform tools
- rail: input

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :--------------------------------- | :------------ | :--------------------------- |
|  [01]   | `Quantizer` / `QuantizerUtilities` | transform     | snap to grid                 |
|  [02]   | `Splitter`                         | transform     | split notes/objects by grid  |
|  [03]   | `Merger`                           | transform     | merge adjacent / whole files |
|  [04]   | `Resizer`                          | transform     | scale durations              |
|  [05]   | `Repeater` / `RepeaterUtilities`   | transform     | repeat object range          |
|  [06]   | `TimedObjectUtilities`             | transform     | time/length set algebra      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: device intake, send, and hot-plug
- rail: input

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]     | [RAIL]                |
| :-----: | :--------------------------------------------------- | :----------------- | :-------------------- |
|  [01]   | `GetAll()` / `GetByName(name)` / `GetByIndex(index)` | `InputDevice`      | resolve input         |
|  [02]   | `StartEventsListening()` / `StopEventsListening()`   | `InputDevice`      | intake lifecycle      |
|  [03]   | `EventReceived` (`MidiEventReceivedEventArgs`)       | `InputDevice`      | received event signal |
|  [04]   | `GetAll()` / `GetByName(name)`                       | `OutputDevice`     | resolve output        |
|  [05]   | `SendEvent(midiEvent)` / `PrepareForEventsSending()` | `OutputDevice`     | emit/warm send        |
|  [06]   | `TurnAllNotesOff()`                                  | `OutputDevice`     | panic note release    |
|  [07]   | `EventSent` (`MidiEventSentEventArgs`)               | `OutputDevice`     | sent event signal     |
|  [08]   | `DeviceAdded` / `DeviceRemoved`                      | `DevicesWatcher`   | hot-plug signals      |
|  [09]   | `Connect()` (`InputDevice`->`OutputDevice`)          | `DevicesConnector` | hardware MIDI-thru    |

[ENTRYPOINT_SCOPE]: file read, write, and lazy tokens
- rail: input

| [INDEX] | [SURFACE]                                                                     | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :-------------------- |
|  [01]   | `Read(string filePath, ReadingSettings)`                                      | `MidiFile`     | parse file path       |
|  [02]   | `Read(Stream, ReadingSettings)`                                               | `MidiFile`     | parse stream          |
|  [03]   | `ReadLazy(...)` -> `MidiTokensReader`                                         | `MidiFile`     | streaming token read  |
|  [04]   | `Write(string filePath, bool overwriteFile, MidiFileFormat, WritingSettings)` | `MidiFile`     | serialize to path     |
|  [05]   | `Write(Stream, MidiFileFormat, WritingSettings)`                              | `MidiFile`     | serialize to stream   |
|  [06]   | `WriteLazy(...)` -> `MidiTokensWriter`                                        | `MidiFile`     | streaming token write |
|  [07]   | `Chunks` / `TimeDivision` / `OriginalFormat`                                  | `MidiFile`     | model + read-format   |
|  [08]   | `Clone()`                                                                     | `MidiFile`     | deep copy             |
|  [09]   | `Equals(file1, file2, [settings,] out string message)`                        | `MidiFile`     | structural compare    |

[ENTRYPOINT_SCOPE]: event construction and compare
- rail: input

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]       | [RAIL]                      |
| :-----: | :--------------------------------------------------- | :------------------- | :-------------------------- |
|  [01]   | `NoteOnEvent(SevenBitNumber, SevenBitNumber)`        | `NoteOnEvent`        | note start build            |
|  [02]   | `NoteOffEvent(SevenBitNumber, SevenBitNumber)`       | `NoteOffEvent`       | note stop build             |
|  [03]   | `ControlChangeEvent(SevenBitNumber, SevenBitNumber)` | `ControlChangeEvent` | controller build            |
|  [04]   | `ControlNumber` / `ControlValue` (`SevenBitNumber`)  | `ControlChangeEvent` | controller index/value read |
|  [05]   | `NoteNumber` / `Velocity`                            | `NoteEvent`          | pitch/velocity byte         |
|  [06]   | `Channel` (`FourBitNumber`)                          | `ChannelEvent`       | channel index               |
|  [07]   | `EventType` / `Clone()`                              | `MidiEvent`          | kind/copy                   |
|  [08]   | `Equals(event1, event2, out string message)`         | `MidiEvent`          | structural compare          |

[ENTRYPOINT_SCOPE]: interaction extraction and timed model
- rail: input

| [INDEX] | [SURFACE]                                                 | [SURFACE_ROOT]                 | [RAIL]                 |
| :-----: | :-------------------------------------------------------- | :----------------------------- | :--------------------- |
|  [01]   | `GetObjects(ObjectType, ObjectDetectionSettings)`         | `GetObjectsUtilities`          | polymorphic extract    |
|  [02]   | `GetTimedEvents(...)`                                     | `TimedEventsManagingUtilities` | timed event extract    |
|  [03]   | `GetNotes(...)`                                           | `NotesManagingUtilities`       | note extract           |
|  [04]   | `new TimedObjectsManager<TObject>(EventsCollection, ...)` | `TimedObjectsManager`          | mutable edit view      |
|  [05]   | `TimedEvent(midiEvent, long time)`                        | `TimedEvent`                   | event plus tick build  |
|  [06]   | `Event` / `Time`                                          | `TimedEvent`                   | event/tick read        |
|  [07]   | `Note(SevenBitNumber, long length, long time)`            | `Note`                         | timed note build       |
|  [08]   | `GetTimedNoteOnEvent()` / `GetTimedNoteOffEvent()`        | `Note`                         | note on/off projection |
|  [09]   | `NoteName` / `Octave` / `OffVelocity`                     | `Note`                         | pitch projection       |
|  [10]   | `GetTempoMap()`                                           | `MidiFile`                     | tempo-map source       |

[ENTRYPOINT_SCOPE]: transform tools
- rail: input

| [INDEX] | [SURFACE]                                               | [SURFACE_ROOT]       | [RAIL]          |
| :-----: | :------------------------------------------------------ | :------------------- | :-------------- |
|  [01]   | `QuantizeObjects(objectType, grid, tempoMap, settings)` | `QuantizerUtilities` | snap to grid    |
|  [02]   | `SplitObjectsByGrid(...)`                               | `Splitter`           | grid split      |
|  [03]   | `SplitObjectsAtDistance(...)`                           | `Splitter`           | distance split  |
|  [04]   | `SplitObjectsByStep(...)`                               | `Splitter`           | step split      |
|  [05]   | `SplitObjectsByPartsNumber(...)`                        | `Splitter`           | partition split |
|  [06]   | `MergeObjects(...)`                                     | `Merger`             | object merge    |
|  [07]   | `MergeSequentially(...)`                                | `Merger`             | file sequence   |
|  [08]   | `MergeSimultaneously(...)`                              | `Merger`             | file overlay    |
|  [09]   | `Resize(...)`                                           | `Resizer`            | duration scale  |
|  [10]   | `ResizeObjectsGroup(...)`                               | `Resizer`            | group scale     |
|  [11]   | `Repeat(...)`                                           | `RepeaterUtilities`  | range repeat    |

[ENTRYPOINT_SCOPE]: playback scheduling and capture
- rail: input

| [INDEX] | [SURFACE]                                                                              | [SURFACE_ROOT] | [RAIL]                  |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :---------------------- |
|  [01]   | `Playback(IEnumerable<ITimedObject>, TempoMap, IOutputDevice, PlaybackSettings)`       | `Playback`     | construct scheduler     |
|  [02]   | `Start()` / `Stop()` / `MoveToStart()`                                                 | `Playback`     | transport               |
|  [03]   | `MoveToTime(ITimeSpan)` / `MoveForward(...)` / `MoveBack(...)`                         | `Playback`     | seek                    |
|  [04]   | `GetCurrentTime<TTimeSpan>()` / `GetDuration<TTimeSpan>()`                             | `Playback`     | position/length read    |
|  [05]   | `Loop` / `Speed` / `InterruptNotesOnStop`                                              | `Playback`     | transport policy        |
|  [06]   | `TrackNotes` / `TrackProgram` / `TrackPitchValue` / `TrackControlValue`                | `Playback`     | state-tracking on seek  |
|  [07]   | `NoteCallback` (`NoteCallback`) / `EventCallback` (`EventCallback`)                    | `Playback`     | live note/event rewrite |
|  [08]   | `IsSnappingEnabled` / `SnapToEvents(Predicate<MidiEvent>)` -> `SnapPointsGroup`        | `Playback`     | snap navigation         |
|  [09]   | `MoveToNextSnapPoint(...)` / `MoveToPreviousSnapPoint(...)` / `MoveToFirstSnapPoint()` | `Playback`     | snap seek               |
|  [10]   | `NotesPlaybackStarted` / `EventPlayed` / `Finished`                                    | `Playback`     | playback signals        |
|  [11]   | `Recording(TempoMap, IInputDevice)` / `Start()` / `Stop()`                             | `Recording`    | capture lifecycle       |
|  [12]   | `GetEvents()` (`ICollection<TimedEvent>`) / `GetDuration<TTimeSpan>()`                 | `Recording`    | capture extraction      |
|  [13]   | `EventRecorded` (`MidiEventRecordedEventArgs`)                                         | `Recording`    | capture signal          |

## [04]-[IMPLEMENTATION_LAW]

[MIDI_TOPOLOGY]:
- namespaces: `Multimedia` (devices, playback, recording, clock), `Core` (file, chunk, event model, lazy tokens), `Interaction` (timed/note/chord lens, `TimedObjectsManager`), `Tools` (quantize/split/merge/resize/repeat), `Common` (`SevenBitNumber`, `FourBitNumber` bounded bytes)
- file model: `MidiFile` owns `TimeDivision` + `OriginalFormat` + a `ChunksCollection`; each `TrackChunk` owns an `EventsCollection` of raw `MidiEvent`. `ReadLazy`/`WriteLazy` stream through `MidiTokensReader`/`MidiTokensWriter` for files too large to hold in memory.
- event hierarchy: `MidiEvent` -> `ChannelEvent` -> `NoteEvent` -> `NoteOnEvent`/`NoteOffEvent`, with `ControlChangeEvent`, `ProgramChangeEvent`, `PitchBendEvent`, `ChannelAftertouchEvent`, `NoteAftertouchEvent` as sibling channel events; `MetaEvent` and `SysExEvent` are non-channel branches.
- byte discipline: pitch, velocity, controller, and program fields are `SevenBitNumber` (0..127); `Channel` is `FourBitNumber` (0..15); out-of-range construction throws before an event forms.
- interaction layer: `GetObjectsUtilities.GetObjects(ObjectType, ObjectDetectionSettings)` is the polymorphic extraction entry — `ObjectType` is a `[Flags]` discriminant (`TimedEvent | Note | Chord`) so one call extracts a mixed timed-object stream; `TimedEventsManagingUtilities`/`NotesManagingUtilities` are the typed lenses; `TimedObjectsManager<T>` gives a mutable edit-in-place view over an `EventsCollection`.
- note model: `Note` carries `NoteNumber`, `Velocity`, `OffVelocity`, `Channel`, `Time`, `Length`, and derived `NoteName`/`Octave`; it decomposes to a `NoteOn`/`NoteOff` `TimedEvent` pair via `GetTimedNoteOnEvent`/`GetTimedNoteOffEvent`.
- transform tools: `Quantizer`, `Splitter`, `Merger`, `Resizer`, `Repeater` operate over `ObjectType` selections against a `TempoMap` and a musical/metric grid — the canonical home for grid-snap, length-scale, split, and repeat instead of hand-rolled tick math; `Merger` also folds whole files (`MergeSequentially`/`MergeSimultaneously`).
- device intake: `InputDevice` raises `EventReceived` with `MidiEventReceivedEventArgs.Event` between `StartEventsListening`/`StopEventsListening`; `OutputDevice.SendEvent` emits one `MidiEvent` and raises `EventSent`; `DevicesWatcher` raises `DeviceAdded`/`DeviceRemoved` for hot-plug.
- playback model: `Playback(IEnumerable<ITimedObject>, TempoMap, IOutputDevice, PlaybackSettings)` schedules against the native `MidiClock` (configured via `PlaybackSettings.ClockSettings`/`MidiClockSettings`); `NoteCallback`/`EventCallback` rewrite or suppress events live; `SnapToEvents` builds a `SnapPointsGroup` for transport navigation; `TrackNotes`/`TrackProgram`/`TrackPitchValue`/`TrackControlValue` replay accumulated controller state after a seek so the synth never desyncs.
- time units: raw event positions are tick `long` values; metric, musical, and bar/beat projections route through `TempoMap` and `ITimeSpan`/`TimeSpanType` converters — `GetCurrentTime<MetricTimeSpan>()` etc.

[LOCAL_ADMISSION]:
- Native ABI gate: the high-precision multimedia clock has no Linux native, so the InputFabric arms the `Multimedia` device/playback/recording rails only on the macOS desktop host; the headless-Linux render path consumes only the managed `Core`/`Interaction`/`Tools` surface (file read/write, event model, timed projection, transforms) and never opens an `InputDevice`/`Playback`.
- Device handles are lifecycle-scoped; the InputFabric disposes every `InputDevice`, `OutputDevice`, `DevicesWatcher`, `Recording`, and `Playback` it opens in a scoped fold (all are `IDisposable`).
- Boundary intake reads `MidiEventReceivedEventArgs.Event` and maps it to the canonical InputFabric event shape at the edge.
- The `Midi` arm joins `Gamepad` and `Haptic` SDL2 (`api-silk-input`, `api-silk-sdl`) with `Hid` HidSharp (`api-hidsharp`) SpaceMouse streams on the single `InputFabric` edge, and every device folds onto the one `CommandIntent` table.
- A MIDI control surface raises existing parameter intents through the shared fold, never through a parallel MIDI device→intent edge.
- DryWetMidi carries its own native multimedia clock and no SDL2 dependency, so the `Midi` capsule shares no native bundle with its SDL2 peers, and raw `MidiEvent` types stop at the boundary.
- File intake uses `MidiFile.Read` (or `ReadLazy` for large files); note and timed-event projection runs through the `Interaction`/`Tools` lenses against an explicit `TempoMap`, never ad hoc tick math.
- Data-byte fields cross the boundary as `SevenBitNumber`/`FourBitNumber`; raw `int` velocity, pitch, channel, or program values are rejected before event construction.

[RAIL_LAW]:
- Package: `Melanchall.DryWetMidi`
- Owns: MIDI device intake/send/hot-plug, file/chunk read/write (eager and lazy-token), the channel/meta event family, the timed note/event/chord interaction model, the quantize/split/merge/resize/repeat transform tools, and clock-driven playback/recording
- Accept: lifecycle-scoped devices, `TempoMap`-anchored time projection, bounded-byte event fields, transform-tool grid operations, the macOS-only native-clock gate for the multimedia rails, and composition as the `Midi` case on the single `InputFabric` edge beside the `Gamepad`/`Haptic`/`Hid` peers (`api-silk-input.md`, `api-silk-sdl.md`, `api-hidsharp.md`), folding onto the one `CommandIntent` table
- Reject: hand-rolled MIDI parsing or tick math, ambient device ownership, raw integer pitch/velocity/channel values, opening the multimedia rails on a host with no native clock, or a parallel MIDI device→intent edge beside the single `InputFabric` fold the other three device rails share
