# [RASM_RHINO_API_RHINOCOMMON_COMMANDS]

Catalog scope: the command lifecycle and the full interactive getter/option surface — `Rhino.Commands` plus every `Rhino.Input`/`Rhino.Input.Custom` acquisition family the package composes.

[NAMESPACES]:
- `Rhino.Commands` — `Command` (`EnglishName`, `RunCommand`, `ReplayHistory`), `Result`, `RunMode`, `ReplayHistoryData`.
- `Rhino.Input` — `RhinoGet` (`GetBool`/`GetLine`/`GetPolyline`/`GetCircle`/`GetArc`/`GetPlane`/`GetRectangle`/`GetBox`/`GetColor`/`GetView`/`GetViewports`), `StringParser`, `StringParserSettings`, `LengthValue`.
- `Rhino.Input.Custom` — `GetBaseClass` (prompts, defaults, option registration, transparency), `GetObject` (geometry/attribute filters, pre/post-select policy, `GetMultiple`, `PickObjects`), `GetPoint` (constraint families, snap/construction points, dynamic-draw events, `GetPlanarConstraint`, `OsnapEventType`), `GetTransform`, `GetOption`, `GetInteger`, `GetNumber`, `GetString`, `GetBoxMode`, `GeometryAttributeFilter`, `GetPointMouseEventArgs`/`GetPointDrawEventArgs`.
- `Rhino.Input.Custom` option carriers — `CommandLineOption`/`CommandLineOptionType`, `OptionToggle`, `OptionDouble`, `OptionInteger`, `OptionString`, `OptionColor`, list/enum option registration.
- `Rhino.ApplicationSettings` — `OsnapModes`.
- `Rhino.UI` — `LocalizeStringPair`, `CursorStyle` (command-prompt adjacency).
