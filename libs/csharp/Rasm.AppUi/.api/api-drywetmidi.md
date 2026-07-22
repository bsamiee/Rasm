# [RASM_APPUI_API_DRYWETMIDI]

`Melanchall.DryWetMidi` owns the AppUi MIDI surface: the Standard MIDI File and chunk object model, the channel and meta event family, the `TempoMap`-anchored timed note/chord interaction layer, the grid transform tools, and the device/recording/playback rails a native multimedia clock drives. Managed `Core`/`Interaction`/`Tools` bind on every host; the `Multimedia` device rails bind only where the native clock is present, so the headless-Linux path stays managed-only. `Midi` folds onto the single `InputFabric` edge every device rail shares.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Melanchall.DryWetMidi`
- package: `Melanchall.DryWetMidi` (MIT)
- assembly: `Melanchall.DryWetMidi`
- consumer-tfm: `netstandard2.0` (package ships `netstandard2.0`/`net45`; `net10.0` binds the `netstandard2.0` asset)
- namespace: `Melanchall.DryWetMidi.Multimedia`, `.Core`, `.Interaction`, `.Tools`, `.Common`
- asset: managed runtime library only; the package ships no native binary
- abi: `Multimedia` rails P/Invoke the native clock `Melanchall_DryWetMidi_Native32`/`_Native64`, supplied out-of-band with no Linux build, so `Playback`/`Recording`/`InputDevice`/`OutputDevice` bind only on the native-clock desktop host and the headless-Linux path uses managed `Core`/`Interaction`/`Tools` alone
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: multimedia devices, clock, and rails

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [CAPABILITY]               |
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

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]    | [CAPABILITY]                  |
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

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [CAPABILITY]          |
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

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [CAPABILITY]                  |
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

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :--------------------------------- | :------------ | :--------------------------- |
|  [01]   | `Quantizer` / `QuantizerUtilities` | transform     | snap to grid                 |
|  [02]   | `Splitter`                         | transform     | split notes/objects by grid  |
|  [03]   | `Merger`                           | transform     | merge adjacent / whole files |
|  [04]   | `Resizer`                          | transform     | scale durations              |
|  [05]   | `Repeater` / `RepeaterUtilities`   | transform     | repeat object range          |
|  [06]   | `TimedObjectUtilities`             | transform     | time/length set algebra      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: device intake, send, and hot-plug

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]          |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `InputDevice.GetAll()` / `.GetByName(string)` / `.GetByIndex(int)` | static   | resolve input         |
|  [02]   | `InputDevice.StartEventsListening()` / `.StopEventsListening()`    | instance | intake lifecycle      |
|  [03]   | `InputDevice.EventReceived` (`MidiEventReceivedEventArgs`)         | event    | received event signal |
|  [04]   | `OutputDevice.GetAll()` / `.GetByName(string)`                     | static   | resolve output        |
|  [05]   | `OutputDevice.SendEvent(MidiEvent)` / `.PrepareForEventsSending()` | instance | emit/warm send        |
|  [06]   | `OutputDevice.TurnAllNotesOff()`                                   | instance | panic note release    |
|  [07]   | `OutputDevice.EventSent` (`MidiEventSentEventArgs`)                | event    | sent event signal     |
|  [08]   | `DevicesWatcher.DeviceAdded` / `.DeviceRemoved`                    | event    | hot-plug signals      |
|  [09]   | `DevicesConnector.Connect()`                                       | instance | hardware MIDI-thru    |

[ENTRYPOINT_SCOPE]: file read, write, and lazy tokens

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `MidiFile.Read(string, ReadingSettings)`                                    | static   | parse file path       |
|  [02]   | `MidiFile.Read(Stream, ReadingSettings)`                                    | static   | parse stream          |
|  [03]   | `MidiFile.ReadLazy(string, ReadingSettings) -> MidiTokensReader`            | static   | streaming token read  |
|  [04]   | `MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)`             | instance | serialize to path     |
|  [05]   | `MidiFile.Write(Stream, MidiFileFormat, WritingSettings)`                   | instance | serialize to stream   |
|  [06]   | `MidiFile.WriteLazy(string, bool, MidiFileFormat, ...) -> MidiTokensWriter` | static   | streaming token write |
|  [07]   | `MidiFile.Chunks` / `.TimeDivision` / `.OriginalFormat`                     | property | model + read-format   |
|  [08]   | `MidiFile.Clone()`                                                          | instance | deep copy             |
|  [09]   | `MidiFile.Equals(MidiFile, MidiFile, out string)`                           | static   | structural compare    |

[ENTRYPOINT_SCOPE]: event construction and compare

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]           |
| :-----: | :--------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `NoteOnEvent(SevenBitNumber, SevenBitNumber)`        | ctor     | note start build       |
|  [02]   | `NoteOffEvent(SevenBitNumber, SevenBitNumber)`       | ctor     | note stop build        |
|  [03]   | `ControlChangeEvent(SevenBitNumber, SevenBitNumber)` | ctor     | controller build       |
|  [04]   | `ControlChangeEvent.ControlNumber` / `.ControlValue` | property | controller index/value |
|  [05]   | `NoteEvent.NoteNumber` / `.Velocity`                 | property | pitch/velocity byte    |
|  [06]   | `ChannelEvent.Channel` (`FourBitNumber`)             | property | channel index          |
|  [07]   | `MidiEvent.EventType` / `.Clone()`                   | instance | kind/copy              |
|  [08]   | `MidiEvent.Equals(MidiEvent, MidiEvent, out string)` | static   | structural compare     |

[ENTRYPOINT_SCOPE]: interaction extraction and timed model

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `GetObjectsUtilities.GetObjects(ObjectType, ObjectDetectionSettings)` | static   | polymorphic extract    |
|  [02]   | `TimedEventsManagingUtilities.GetTimedEvents(...)`                    | static   | timed event extract    |
|  [03]   | `NotesManagingUtilities.GetNotes(...)`                                | static   | note extract           |
|  [04]   | `new TimedObjectsManager<TObject>(EventsCollection, ...)`             | ctor     | mutable edit view      |
|  [05]   | `TimedEvent(MidiEvent, long)`                                         | ctor     | event plus tick build  |
|  [06]   | `TimedEvent.Event` / `.Time`                                          | property | event/tick read        |
|  [07]   | `Note(SevenBitNumber, long, long)`                                    | ctor     | timed note build       |
|  [08]   | `Note.GetTimedNoteOnEvent()` / `.GetTimedNoteOffEvent()`              | instance | note on/off projection |
|  [09]   | `Note.NoteName` / `.Octave` / `.OffVelocity`                          | property | pitch projection       |
|  [10]   | `TempoMapManagingUtilities.GetTempoMap(MidiFile)`                     | static   | tempo-map source       |

[ENTRYPOINT_SCOPE]: transform tools

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]    |
| :-----: | :--------------------------------------------------------------------- | :------ | :-------------- |
|  [01]   | `QuantizerUtilities.QuantizeObjects(ObjectType, IGrid, TempoMap, ...)` | static  | snap to grid    |
|  [02]   | `Splitter.SplitObjectsByGrid(ObjectType, IGrid, TempoMap, ...)`        | static  | grid split      |
|  [03]   | `Splitter.SplitObjectsAtDistance(...)`                                 | static  | distance split  |
|  [04]   | `Splitter.SplitObjectsByStep(...)`                                     | static  | step split      |
|  [05]   | `Splitter.SplitObjectsByPartsNumber(...)`                              | static  | partition split |
|  [06]   | `Merger.MergeObjects(ObjectType, TempoMap, ...)`                       | static  | object merge    |
|  [07]   | `Merger.MergeSequentially(IEnumerable<MidiFile>, ...)`                 | static  | file sequence   |
|  [08]   | `Merger.MergeSimultaneously(IEnumerable<MidiFile>, ...)`               | static  | file overlay    |
|  [09]   | `Resizer.Resize(ITimeSpan, TempoMap)`                                  | static  | duration scale  |
|  [10]   | `Resizer.ResizeObjectsGroup(...)`                                      | static  | group scale     |
|  [11]   | `RepeaterUtilities.Repeat(int, ...)`                                   | static  | range repeat    |

[ENTRYPOINT_SCOPE]: playback scheduling and capture

| [INDEX] | [SURFACE]                                                                                         | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------------------ | :------- | :---------------------- |
|  [01]   | `Playback(IEnumerable<ITimedObject>, TempoMap, IOutputDevice, PlaybackSettings)`                  | ctor     | construct scheduler     |
|  [02]   | `Playback.Start()` / `.Stop()` / `.MoveToStart()`                                                 | instance | transport               |
|  [03]   | `Playback.MoveToTime(ITimeSpan)` / `.MoveForward(...)` / `.MoveBack(...)`                         | instance | seek                    |
|  [04]   | `Playback.GetCurrentTime<TTimeSpan>()` / `.GetDuration<TTimeSpan>()`                              | instance | position/length read    |
|  [05]   | `Playback.Loop` / `.Speed` / `.InterruptNotesOnStop`                                              | property | transport policy        |
|  [06]   | `Playback.TrackNotes` / `.TrackProgram` / `.TrackPitchValue` / `.TrackControlValue`               | property | state-tracking on seek  |
|  [07]   | `Playback.NoteCallback` / `.EventCallback`                                                        | property | live note/event rewrite |
|  [08]   | `Playback.IsSnappingEnabled` / `.SnapToEvents(Predicate<MidiEvent>) -> SnapPointsGroup`           | instance | snap navigation         |
|  [09]   | `Playback.MoveToNextSnapPoint(...)` / `.MoveToPreviousSnapPoint(...)` / `.MoveToFirstSnapPoint()` | instance | snap seek               |
|  [10]   | `Playback.NotesPlaybackStarted` / `.EventPlayed` / `.Finished`                                    | event    | playback signals        |
|  [11]   | `Recording(TempoMap, IInputDevice)` / `.Start()` / `.Stop()`                                      | ctor     | capture lifecycle       |
|  [12]   | `Recording.GetEvents() -> ICollection<TimedEvent>` / `.GetDuration<TTimeSpan>()`                  | instance | capture extraction      |
|  [13]   | `Recording.EventRecorded` (`MidiEventRecordedEventArgs`)                                          | event    | capture signal          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Five namespaces partition the surface: `Multimedia` (device/playback/recording/clock), `Core` (file/chunk/event/lazy-token), `Interaction` (timed/note/chord lens, `TempoMap`), `Tools` (transforms), `Common` (bounded bytes).
- `MidiFile` owns `TimeDivision`, `OriginalFormat`, and a `ChunksCollection`; each `TrackChunk` owns an `EventsCollection` of raw `MidiEvent`; the lazy-token pair streams files too large to hold in memory.
- Every data byte is `SevenBitNumber` (0..127) or `Channel` `FourBitNumber` (0..15); out-of-range construction throws before an event forms.
- One tick timeline underlies every position; metric, musical, and bar/beat views project only through `TempoMap` and `ITimeSpan`, never ad hoc tick math.
- `GetObjectsUtilities.GetObjects` is the one polymorphic extraction entry; `ObjectType` is a `[Flags]` discriminant, so one call yields a mixed timed-object stream and every transform tool operates over the same selection against a `TempoMap` grid.
- `Playback` schedules against the native `MidiClock`; `Track`-prefixed state replays accumulated controller/program/pitch state after a seek so the synth never desyncs, and `NoteCallback`/`EventCallback` rewrite or suppress events live.

[STACKING]:
- `api-silk-input.md` / `api-silk-sdl.md` / `api-hidsharp.md`: the `Midi` case joins `Gamepad`/`Haptic`/`Hid` in the `DeviceDriver` `[Union]`; all four capsules bind delegate columns on the single `InputFabric` edge that folds every device onto the one `CommandIntent` table, so a MIDI control surface raises the existing parameter intents through that shared fold, never a parallel MIDI device->intent edge.
- DryWetMidi carries its own native multimedia clock and no SDL2 dependency, so the `Midi` capsule shares no native bundle with the SDL2 `Gamepad`/`Haptic` pair; its only shared surface is the canonical fold at the edge.
- within-lib: `MidiFile.Read` -> `GetObjects(ObjectType.Note, ...)` -> `TempoMapManagingUtilities.GetTempoMap` chains file bytes into a timed note model, and the `Quantizer`/`Splitter`/`Merger`/`Resizer`/`Repeater` tools fold over one `ObjectType` selection against that `TempoMap`, so grid-snap, length-scale, split, merge, and repeat replace hand-rolled tick math.

[LOCAL_ADMISSION]:
- Native-clock gate: the InputFabric arms `Playback`/`Recording`/`InputDevice`/`OutputDevice` only on the native-clock desktop host; the headless-Linux path consumes only managed `Core`/`Interaction`/`Tools`.
- Every opened device, `DevicesWatcher`, `Recording`, and `Playback` (`IDisposable`) disposes in a scoped fold; boundary intake reads `MidiEventReceivedEventArgs.Event` and maps to the canonical input shape at the edge, raw `MidiEvent` types stopping there.
- File intake uses `MidiFile.Read`/`ReadLazy`; note and timed projection run through the `Interaction`/`Tools` lenses against an explicit `TempoMap`.
- Data-byte fields cross as `SevenBitNumber`/`FourBitNumber`; raw `int` pitch, velocity, channel, or program is rejected before event construction.

[RAIL_LAW]:
- Package: `Melanchall.DryWetMidi`
- Owns: MIDI device intake/send/hot-plug, file/chunk read/write eager and lazy-token, the channel/meta event family, the timed note/event/chord interaction model, the transform tools, and native-clock playback/recording.
- Accept: lifecycle-scoped devices, `TempoMap`-anchored time projection, bounded-byte event fields, transform-tool grid operations, the native-clock gate for the `Multimedia` rails, and the `Midi` case on the single `InputFabric` edge.
- Reject: hand-rolled MIDI parsing or tick math, ambient device ownership, raw integer pitch/velocity/channel values, opening the `Multimedia` rails with no native clock, and a parallel MIDI device->intent edge beside the shared `InputFabric` fold.
