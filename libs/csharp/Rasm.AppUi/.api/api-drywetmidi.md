# [RASM_APPUI_API_DRYWETMIDI]

`Melanchall.DryWetMidi` supplies the MIDI file/chunk object model, the channel and meta event family, timed-event and note interaction layers, and the multimedia input/output device, recording, and playback rails. AppUi Shell input composes these into the InputFabric MIDI surface: device intake, event send, file read/write, and the timed note model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Melanchall.DryWetMidi`
- package: `Melanchall.DryWetMidi`
- assembly: `Melanchall.DryWetMidi`
- namespace: `Melanchall.DryWetMidi.Multimedia`
- namespace: `Melanchall.DryWetMidi.Core`
- namespace: `Melanchall.DryWetMidi.Interaction`
- namespace: `Melanchall.DryWetMidi.Common`
- asset: managed runtime library
- asset: native multimedia interop libraries
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: multimedia devices and rails
- rail: input

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [RAIL]                  |
| :-----: | :--------------------------- | :-------------- | :---------------------- |
|  [01]   | `InputDevice`                | input device    | event intake            |
|  [02]   | `OutputDevice`               | output device   | event send              |
|  [03]   | `IInputDevice`               | device contract | input abstraction       |
|  [04]   | `IOutputDevice`              | device contract | output abstraction      |
|  [05]   | `DevicesConnector`           | device link     | input-to-output bridge  |
|  [06]   | `DevicesWatcher`             | device monitor  | hot-plug detection      |
|  [07]   | `Recording`                  | capture rail    | timed event capture     |
|  [08]   | `Playback`                   | playback rail   | timed event scheduling  |
|  [09]   | `MidiEventReceivedEventArgs` | event payload   | received event carrier  |
|  [10]   | `MidiEventSentEventArgs`     | event payload   | sent event carrier      |
|  [11]   | `MidiEventRecordedEventArgs` | event payload   | recorded event carrier  |
|  [12]   | `MidiEventPlayedEventArgs`   | event payload   | played event carrier    |
|  [13]   | `NotesEventArgs`             | event payload   | playback note carrier   |
|  [14]   | `InputDeviceProperty`        | device property | input capability query  |
|  [15]   | `OutputDeviceProperty`       | device property | output capability query |

[PUBLIC_TYPE_SCOPE]: file and chunk model
- rail: input

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [RAIL]                  |
| :-----: | :----------------- | :--------------- | :---------------------- |
|  [01]   | `MidiFile`         | file root        | read/write surface      |
|  [02]   | `MidiChunk`        | chunk base       | chunk model             |
|  [03]   | `TrackChunk`       | track chunk      | event container         |
|  [04]   | `HeaderChunk`      | header chunk     | format/division         |
|  [05]   | `UnknownChunk`     | unknown chunk    | passthrough chunk       |
|  [06]   | `ChunksCollection` | chunk collection | file chunk list         |
|  [07]   | `EventsCollection` | event collection | track event list        |
|  [08]   | `MidiFileFormat`   | format enum      | single/multi track      |
|  [09]   | `TimeDivision`     | time division    | tick resolution         |
|  [10]   | `ReadingSettings`  | read policy      | parse configuration     |
|  [11]   | `WritingSettings`  | write policy     | serialize configuration |

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

[PUBLIC_TYPE_SCOPE]: interaction and note model
- rail: input

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [RAIL]                  |
| :-----: | :----------------------------- | :---------------- | :---------------------- |
|  [01]   | `TimedEvent`                   | timed event       | event plus tick time    |
|  [02]   | `Note`                         | note object       | timed lengthed note     |
|  [03]   | `Chord`                        | chord object      | simultaneous notes      |
|  [04]   | `ITimedObject`                 | timed contract    | tick-positioned object  |
|  [05]   | `ILengthedObject`              | lengthed contract | duration-bearing object |
|  [06]   | `TempoMap`                     | tempo map         | time conversion source  |
|  [07]   | `TimedEventsManagingUtilities` | timed-event lens  | event extraction        |
|  [08]   | `NotesManagingUtilities`       | note lens         | note extraction         |
|  [09]   | `GetObjectsUtilities`          | object lens       | polymorphic extraction  |
|  [10]   | `ObjectType`                   | object kind enum  | extraction selector     |
|  [11]   | `NoteDetectionSettings`        | detection policy  | note pairing rules      |
|  [12]   | `SevenBitNumber`               | bounded value     | 0..127 data byte        |
|  [13]   | `FourBitNumber`                | bounded value     | 0..15 channel index     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: device intake and send
- rail: input

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT] | [RAIL]                   |
| :-----: | :-------------------------- | :------------- | :----------------------- |
|  [01]   | `GetAll()`                  | `InputDevice`  | enumerate input devices  |
|  [02]   | `GetByName(name)`           | `InputDevice`  | resolve input by name    |
|  [03]   | `GetByIndex(index)`         | `InputDevice`  | resolve input by index   |
|  [04]   | `StartEventsListening()`    | `InputDevice`  | begin intake             |
|  [05]   | `StopEventsListening()`     | `InputDevice`  | end intake               |
|  [06]   | `EventReceived`             | `InputDevice`  | received event signal    |
|  [07]   | `GetAll()`                  | `OutputDevice` | enumerate output devices |
|  [08]   | `GetByName(name)`           | `OutputDevice` | resolve output by name   |
|  [09]   | `SendEvent(midiEvent)`      | `OutputDevice` | emit one event           |
|  [10]   | `TurnAllNotesOff()`         | `OutputDevice` | panic note release       |
|  [11]   | `PrepareForEventsSending()` | `OutputDevice` | warm send path           |
|  [12]   | `EventSent`                 | `OutputDevice` | sent event signal        |

[ENTRYPOINT_SCOPE]: file read and write
- rail: input

| [INDEX] | [SURFACE]                            | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :----------------------------------- | :------------- | :-------------------- |
|  [01]   | `Read(filePath, settings)`           | `MidiFile`     | parse file path       |
|  [02]   | `Read(stream, settings)`             | `MidiFile`     | parse stream          |
|  [03]   | `ReadLazy(filePath, settings)`       | `MidiFile`     | streaming token read  |
|  [04]   | `Write(filePath, overwrite, format)` | `MidiFile`     | serialize to path     |
|  [05]   | `Write(stream, format, settings)`    | `MidiFile`     | serialize to stream   |
|  [06]   | `WriteLazy(filePath, ..)`            | `MidiFile`     | streaming token write |
|  [07]   | `Chunks`                             | `MidiFile`     | chunk collection      |
|  [08]   | `TimeDivision`                       | `MidiFile`     | tick resolution       |
|  [09]   | `Clone()`                            | `MidiFile`     | deep copy             |
|  [10]   | `Equals(file1, file2, out message)`  | `MidiFile`     | structural compare    |

[ENTRYPOINT_SCOPE]: event construction and compare
- rail: input

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]       | [RAIL]             |
| :-----: | :----------------------------------------- | :------------------- | :----------------- |
|  [01]   | `NoteOnEvent(noteNumber, velocity)`        | `NoteOnEvent`        | note start build   |
|  [02]   | `NoteOffEvent(noteNumber, velocity)`       | `NoteOffEvent`       | note stop build    |
|  [03]   | `ControlChangeEvent(controlNumber, value)` | `ControlChangeEvent` | controller build   |
|  [04]   | `NoteNumber`                               | `NoteEvent`          | pitch byte         |
|  [05]   | `Velocity`                                 | `NoteEvent`          | velocity byte      |
|  [06]   | `Channel`                                  | `ChannelEvent`       | channel index      |
|  [07]   | `EventType`                                | `MidiEvent`          | kind discriminant  |
|  [08]   | `Clone()`                                  | `MidiEvent`          | event copy         |
|  [09]   | `Equals(event1, event2, out message)`      | `MidiEvent`          | structural compare |

[ENTRYPOINT_SCOPE]: interaction extraction and timed model
- rail: input

| [INDEX] | [SURFACE]                        | [SURFACE_ROOT]                 | [RAIL]                |
| :-----: | :------------------------------- | :----------------------------- | :-------------------- |
|  [01]   | `GetTimedEvents(file, settings)` | `TimedEventsManagingUtilities` | timed event extract   |
|  [02]   | `GetTimedEvents(trackChunk, ..)` | `TimedEventsManagingUtilities` | per-chunk extract     |
|  [03]   | `GetNotes(file, settings)`       | `NotesManagingUtilities`       | note extract          |
|  [04]   | `GetNotes(midiEvents, ..)`       | `NotesManagingUtilities`       | event-stream notes    |
|  [05]   | `TimedEvent(midiEvent, time)`    | `TimedEvent`                   | event plus tick build |
|  [06]   | `Event` / `Time`                 | `TimedEvent`                   | event/tick read       |
|  [07]   | `Note(noteNumber, length, time)` | `Note`                         | timed note build      |
|  [08]   | `GetTimedNoteOnEvent()`          | `Note`                         | note-on projection    |
|  [09]   | `GetTimedNoteOffEvent()`         | `Note`                         | note-off projection   |
|  [10]   | `NoteName` / `Octave`            | `Note`                         | pitch projection      |

[ENTRYPOINT_SCOPE]: playback scheduling
- rail: input

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :----------------------------------------- | :------------- | :-------------------- |
|  [01]   | `Playback(timedObjects, tempoMap, device)` | `Playback`     | construct scheduler   |
|  [02]   | `Start()`                                  | `Playback`     | begin playback        |
|  [03]   | `Stop()`                                   | `Playback`     | halt playback         |
|  [04]   | `MoveToStart()`                            | `Playback`     | seek to start         |
|  [05]   | `MoveToTime(time)`                         | `Playback`     | seek to instant       |
|  [06]   | `MoveForward(step)` / `MoveBack(step)`     | `Playback`     | relative seek         |
|  [07]   | `GetCurrentTime<TTimeSpan>()`              | `Playback`     | current position read |
|  [08]   | `Loop` / `Speed` / `TrackNotes`            | `Playback`     | playback policy       |
|  [09]   | `EventPlayed` / `Finished`                 | `Playback`     | playback signals      |
|  [10]   | `Start()` / `Stop()`                       | `Recording`    | capture lifecycle     |
|  [11]   | `EventRecorded`                            | `Recording`    | capture signal        |

## [04]-[IMPLEMENTATION_LAW]

[MIDI_TOPOLOGY]:
- namespaces: `Multimedia` (devices, playback, recording), `Core` (file, chunk, event model), `Interaction` (timed/note/chord lens), `Common` (`SevenBitNumber`, `FourBitNumber` bounded bytes)
- file model: `MidiFile` owns `TimeDivision` plus a `ChunksCollection`; each `TrackChunk` owns an `EventsCollection` of raw `MidiEvent`
- event hierarchy: `MidiEvent` -> `ChannelEvent` -> `NoteEvent` -> `NoteOnEvent`/`NoteOffEvent`, with `ControlChangeEvent`, `ProgramChangeEvent`, and `PitchBendEvent` as sibling channel events; `MetaEvent` and `SysExEvent` are non-channel branches
- byte discipline: pitch, velocity, and controller fields are `SevenBitNumber` (0..127); `Channel` is `FourBitNumber` (0..15); out-of-range construction raises before an event forms
- interaction layer: `TimedEventsManagingUtilities` and `NotesManagingUtilities` are extension lenses over `MidiFile`, `TrackChunk`, and `IEnumerable<MidiEvent>`, projecting raw events into `TimedEvent` and `Note` against a `TempoMap`
- note model: `Note` carries `NoteNumber`, `Velocity`, `OffVelocity`, `Channel`, `Time`, `Length`, and derived `NoteName`/`Octave`; it decomposes to a `NoteOn`/`NoteOff` `TimedEvent` pair
- device intake: `InputDevice` raises `EventReceived` with `MidiEventReceivedEventArgs.Event` between `StartEventsListening` and `StopEventsListening`; `OutputDevice.SendEvent` emits one `MidiEvent` and raises `EventSent`
- device resolution: `InputDevice`/`OutputDevice` resolve via `GetAll`, `GetByName`, and `GetByIndex`; every device, recording, and playback is `IDisposable`
- playback model: `Playback` schedules `IEnumerable<ITimedObject>` against a `TempoMap` to an `IOutputDevice`, exposing `Loop`, `Speed`, `TrackNotes`, seek operations, and lifecycle signals
- time units: raw event positions are tick `long` values; metric, musical, and bar/beat projections route through `TempoMap` time-span converters

[LOCAL_ADMISSION]:
- Device handles are lifecycle-scoped; the InputFabric disposes every `InputDevice`, `OutputDevice`, `Recording`, and `Playback` it opens.
- Boundary intake reads `MidiEventReceivedEventArgs.Event` and maps to canonical input shapes at the edge; raw `MidiEvent` types do not propagate into domain code.
- File intake uses `MidiFile.Read`; note and timed-event projection runs through the `Interaction` lenses against an explicit `TempoMap`, never ad hoc tick math.
- Data-byte fields cross the boundary as `SevenBitNumber`/`FourBitNumber`; raw `int` velocity, pitch, or channel values are rejected before event construction.

[RAIL_LAW]:
- Package: `Melanchall.DryWetMidi`
- Owns: MIDI device intake/send, file/chunk read/write, the channel/meta event family, and the timed note/event interaction model
- Accept: lifecycle-scoped devices, `TempoMap`-anchored time projection, and bounded-byte event fields
- Reject: hand-rolled MIDI parsing, ambient device ownership, or raw integer pitch/velocity/channel values
