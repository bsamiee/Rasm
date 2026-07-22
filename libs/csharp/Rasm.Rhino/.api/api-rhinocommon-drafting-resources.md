# [RASM_RHINO_API_RHINOCOMMON_DRAFTING_RESOURCES]

`RhinoCommon` owns the drafting-resource boundary the annotation styles reference: `Hatch` geometry with gradient and pattern fill behind `HatchPattern`/`HatchLine` definitions and the `HatchPatternTable`, `Linetype` segment/shape/taper definitions behind the `LinetypeTable`, `Font`/`FontQuartet` typeface resolution behind the `FontTable`, and `SectionStyle` section-cut presentation behind the `SectionStyleTable`. Every live document-bound resource resolves inside the owning session before a detached value crosses the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`, `Rhino.DocObjects`, `Rhino.DocObjects.Tables`
- rail: drafting-resource boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hatch geometry and pattern model

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `Hatch`             | fill geometry  | boundary-loop hatch, gradient fill, pattern scale, brep egress |
|  [02]   | `HatchPattern`      | model resource | hatch-line definition set, defaults catalog, file read/write   |
|  [03]   | `HatchLine`         | line model     | one dash-line generator (angle, base, offset, dash run)        |
|  [04]   | `HatchPatternTable` | document table | pattern identity, authoring, rename, and reclaim               |

[PUBLIC_TYPE_SCOPE]: linetype segment/shape model

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [CAPABILITY]                                                     |
| :-----: | :-------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Linetype`      | model resource | dash-segment run, embedded shapes, taper, caps/joins, width      |
|  [02]   | `LinetypeTable` | document table | linetype identity, authoring, default loading, current selection |

[PUBLIC_TYPE_SCOPE]: typeface and section-cut resources

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `Font`              | typeface       | installed-font resolution, quartet lookup, substitution        |
|  [02]   | `FontQuartet`       | family group   | regular/bold/italic/bold-italic availability of one family     |
|  [03]   | `FontTable`         | document table | font find-or-create keyed to a `DimStyles` template projection |
|  [04]   | `SectionStyle`      | model resource | section-cut background fill, boundary, and hatch presentation  |
|  [05]   | `SectionStyleTable` | document table | section-style identity, authoring, and usage census            |

[ENUM_ROSTERS]:
- `HatchPatternFillType` — `Solid` `Lines` `Gradient`.
- `Font.FontWeight : byte` — `Unset` `Thin` `Ultralight` `Light` `Normal` `Medium` `Semibold` `Bold` `Ultrabold` `Heavy`.
- `Font.FontStyle : byte` — `Unset` `Upright` `Italic` `Oblique`.
- `Font.FontStretch : byte` — `Unset` `Ultracondensed` `Extracondensed` `Condensed` `Semicondensed` `Medium` `Semiexpanded` `Expanded` `Extraexpanded` `Ultraexpanded`.
- `Font.FontOrigin : byte` — `Unset=0` `Unknown=1` `WindowsFont=2` `AppleFont=3`; `Font.FontType : byte` — `Unset=0` `ManagedFont=1` `InstalledFont=2`; neither surfaces through a public `Font` property.
- `SectionBackgroundFillMode : byte` — `None` `Viewport` `SolidColor`.
- `ObjectSectionFillRule` — `ClosedCurves` `SolidObjects`.
- Edge-mapped external owners: `Rhino.Display.LineCapStyle`/`LineJoinStyle` carry a `Linetype`'s caps and joins; `Rhino.UnitSystem` carries pattern and width units.

## [03]-[ENTRYPOINTS]

[HATCH_CONSTRUCT]:

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Hatch.Create(Plane, Curve, IEnumerable<Curve>, int, double, double)`         | static   | outer loop + inner holes to one hatch      |
|  [02]   | `Hatch.Create(IEnumerable<Curve>, int, double, double) -> Hatch[]`            | static   | nested closed curves into hatches          |
|  [03]   | `Hatch.CreateFromBrep(Brep, int, int, double, double, Point3d)`               | static   | hatch one planar brep face                 |
|  [04]   | `Hatch.Get2dCurves(bool)` / `Get3dCurves(bool) -> Curve[]`                    | instance | boundary loops, plane-2d or world-3d       |
|  [05]   | `Hatch.CreateDisplayGeometry(HatchPattern, double, out Curve[]/Line[]/Brep)`  | instance | boundary curves, pattern lines, solid brep |
|  [06]   | `Hatch.ToBrep()` / `Explode() -> GeometryBase[]`                              | instance | planar brep; boundary/pattern explode      |
|  [07]   | `Hatch.GetGradientFill() -> ColorGradient` / `SetGradientFill(ColorGradient)` | instance | gradient fill read/write                   |
|  [08]   | `Hatch.ScalePattern(Transform)`                                               | instance | rescale pattern independent of boundary    |

- `Hatch.Create`: `Curve`-singular overloads take one boundary curve; the array overload carries an optional trailing `tolerance`.
[HATCH_FILL]: `PatternIndex` `PatternRotation` `PatternScale` `BasePoint` `Plane` — placement and fill parameters

[HATCH_PATTERN]:

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `new HatchPattern()` / `new HatchPattern(HatchPattern)`                     | ctor     | fresh + copy, detached before `Add`   |
|  [02]   | `HatchPattern.GetDefaultHatchPatterns() -> HatchPattern[]`                  | static   | built-in pattern roster               |
|  [03]   | `HatchPattern.ReadFromFile(string, bool) -> HatchPattern[]`                 | static   | `.pat` import to a roster             |
|  [04]   | `HatchPattern.WriteToFile(string, HatchPattern\|IEnumerable<HatchPattern>)` | static   | `.pat` export, one or many            |
|  [05]   | `HatchPattern.CreatePreviewGeometry(int, int, double) -> Line[]`            | instance | swatch preview lines                  |
|  [06]   | `HatchPattern.AddHatchLine(HatchLine) -> int` / `HatchLineAt(int)`          | instance | append a line; get one by index       |
|  [07]   | `HatchPattern.RemoveHatchLine(int)` / `RemoveAllHatchLines()`               | instance | remove one line or all                |
|  [08]   | `HatchPattern.SetHatchLines(IEnumerable<HatchLine>) -> int`                 | instance | replace the whole line set            |
|  [09]   | `new HatchLine()` / `new HatchLine(HatchLine)`                              | ctor     | one dash-line generator, fresh + copy |
|  [10]   | `HatchLine.SetDashes(IEnumerable<double>)` / `AppendDash(double)`           | instance | set the dash run; append one dash     |
|  [11]   | `HatchLine.DashAt(int) -> double` / `GetDashes -> IEnumerable<double>`      | instance | read one dash; read the run           |

[HATCH_LINE_SET]: `HatchLineCount` `HatchLines` — count and enumerate the pattern's lines
[HATCH_PATTERN_CONFIG]: `Name` `Description` `FillType` `PatternUnitSystem` `AlwaysModelDistances` `Index` `InUse` — settable config and usage
[HATCH_PATTERN_DEFAULTS]: `Solid` `Hatch1` `Hatch2` `Hatch3` `Dash` `Grid` `Grid60` `Plus` `Squares` — static `Defaults` catalog, each a fresh built-in
[HATCH_LINE_GEOMETRY]: `Angle` `BasePoint` `Offset` `DashCount` `PatternLength` — one generator line's geometry
- `HatchPattern` user-string bag: `SetUserString`/`GetUserString`/`GetUserStrings`/`DeleteUserString`/`DeleteAllUserStrings`.

[HATCH_TABLE]:

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `HatchPatternTable.FindName(string)` / `FindIndex(int)` / `FindNameHash(NameHash)` | instance | resolve by name, index, or hash |
|  [02]   | `HatchPatternTable.Add(HatchPattern) -> int` / `Modify(HatchPattern, int, bool)`   | instance | add a pattern; rewrite by index |
|  [03]   | `HatchPatternTable.Rename(int, string)` / `Rename(HatchPattern, string)`           | instance | rename by index or handle       |
|  [04]   | `HatchPatternTable.Delete(int, bool)` / `Delete(IEnumerable<int>, bool)`           | instance | delete one or batch             |
|  [05]   | `HatchPatternTable.PurgeUnused() -> int`                                           | instance | reclaim unreferenced patterns   |

[HATCH_TABLE_STATE]: `CurrentHatchPatternIndex` `GetUnusedHatchPatternName()` — current-pattern and name-mint state

[LINETYPE_DEFINITION]:

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `Linetype.AppendSegment(double, bool) -> int` / `SetSegment(int, double, bool)` | instance | append or rewrite a segment        |
|  [02]   | `Linetype.GetSegment(int, out double, out bool)` / `RemoveSegment(int)`         | instance | read or remove a segment           |
|  [03]   | `Linetype.SetSegments(IEnumerable<double>)`                                     | instance | replace the whole segment run      |
|  [04]   | `Linetype.AddShape(Curve, double)` / `AddShape(TextEntity, double)`             | instance | embed a curve or text shape        |
|  [05]   | `Linetype.RemoveAllShapes()`                                                    | instance | clear embedded shapes              |
|  [06]   | `Linetype.SetTaper(double, double)` / `SetTaper(double, Point2d, double)`       | instance | linear or point-kinked width taper |
|  [07]   | `Linetype.GetTaperPoints() -> Point2d[]` / `RemoveTaper()`                      | instance | read taper points; clear taper     |
|  [08]   | `Linetype.PatternString(bool) -> string`                                        | instance | `.lin` string of this definition   |
|  [09]   | `Linetype.CreateFromPatternString(string, bool) -> Linetype`                    | static   | build from a `.lin` string         |
|  [10]   | `Linetype.ReadFromFile(string) -> Linetype[]`                                   | static   | read a `.lin` file                 |
|  [11]   | `Linetype.DuplicateLinetype() -> Linetype` / `Default()` / `CommitChanges()`    | instance | copy, reset, commit the definition |

- `Linetype.DuplicateLinetype`: copy clears name and id — re-stamp the name before `Modify`.
- a segment is the `(double length, bool isSolid)` pair (no segment-shape enum); named defaults live only on `LinetypeTable` (`ContinuousLinetypeName`/`ByLayerLinetypeName`/`ByParentLinetypeName` + `LoadDefaultLinetypes`).
- `Linetype` user-string bag: `SetUserString`/`GetUserString`/`GetUserStrings`/`DeleteUserString`/`DeleteAllUserStrings`.
[LINETYPE_SEGMENT_READ]: `SegmentCount` `PatternLength` — segment-run measures
[LINETYPE_SHAPES]: `HasShapes` `ShapeSpacing` `ShapeGap` `ShapeLocalOffset` `ShapeBounds` — embedded-shape placement
[LINETYPE_STROKE]: `LineCapStyle` `LineJoinStyle` `Width` `WidthUnits` `AlwaysModelDistances` `IsPatternLocked` — stroke config
[LINETYPE_IDENTITY]: `Name` `LinetypeIndex` `InUse` `IsModified` `UserStringCount` — identity and state

[LINETYPE_TABLE]:

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------ | :------- | :----------------------------------- |
|  [01]   | `LinetypeTable.FindName(string)` / `FindIndex(int)`                       | instance | resolve by name or index             |
|  [02]   | `LinetypeTable.Find(string, bool) -> int` / `Find(Guid, bool) -> int`     | instance | resolve to index by name or id       |
|  [03]   | `LinetypeTable.Add(Linetype) -> int` / `Add(string, IEnumerable<double>)` | instance | add from definition or name+segments |
|  [04]   | `LinetypeTable.AddReferenceLinetype(Linetype) -> int`                     | instance | add from a reference source          |
|  [05]   | `LinetypeTable.Modify(Linetype, int, bool)` / `UndoModify(int)`           | instance | rewrite and rollback                 |
|  [06]   | `LinetypeTable.Delete(int, bool)` / `Delete(IEnumerable<int>, bool)`      | instance | delete one or batch                  |
|  [07]   | `LinetypeTable.Undelete(int)` / `PurgeUnused() -> int`                    | instance | restore one; reclaim unreferenced    |
|  [08]   | `LinetypeTable.LoadDefaultLinetypes(bool) -> int`                         | instance | load the built-in set                |
|  [09]   | `LinetypeTable.SetCurrentLinetypeIndex(int, bool)`                        | instance | set the active linetype              |
|  [10]   | `LinetypeTable.LinetypeIndexForObject(RhinoObject) -> int`                | instance | resolve an object's effective index  |
|  [11]   | `LinetypeTable.GetUnusedLinetypeName(bool) -> string`                     | instance | mint an unused name                  |

[LINETYPE_TABLE_STATE]: `CurrentLinetype` `CurrentLinetypeIndex` `CurrentLinetypeSource` `LinetypeScale` `ActiveCount` — current and active-count state
[LINETYPE_NAMED_DEFAULTS]: `ContinuousLinetypeName` `ByLayerLinetypeName` `ByParentLinetypeName` — the built-in name anchors

[FONT_RESOLUTION]:

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `Font.FromQuartetProperties(string, bool, bool) -> Font`                        | static   | resolve one face from a family quartet |
|  [02]   | `Font.InstalledFonts()` / `InstalledFonts(string) -> Font[]`                    | static   | installed faces, all or by family      |
|  [03]   | `Font.InstalledFontsAsQuartets() -> FontQuartet[]` / `AvailableFontFaceNames()` | static   | installed quartets and face names      |
|  [04]   | `Font.GetSubstituteFont() -> Font`                                              | instance | substitute for an uninstalled font     |
|  [05]   | `new Font(string, FontWeight, FontStyle, FontStretch, bool, bool)`              | ctor     | construct from family and style axes   |

[FONT_NAMES]: `FaceName` `FamilyName` `FamilyPlusFaceName` `PostScriptName` `LogfontName` `QuartetName` `RichTextFontName` `Description` — name projections; `EnglishFaceName`/`EnglishFamilyName`/`EnglishQuartetName` the three English twins
[FONT_METRICS]: `Weight` `Style` `Stretch` `Bold` `Italic` `Underlined` `Strikeout` `PointSize` `IsInstalled` `IsEngravingFont` `IsSymbolFont` `IsSingleStrokeFont` `IsGeometricToleranceFont` `IsSimulated` — resolved style axes and metrics
[FONT_QUARTET]: `QuartetName` `HasRegularFont` `HasBoldFont` `HasItalicFont` `HasBoldItalicFont` — one family's four-face availability
- document face binding resolves through the `DimStyles` table (`DimensionStyle.Font` get/set, `DimStyleTable.Add(dimstyle, reference) -> int`); `FontTable` (`Count`, `CurrentIndex`, indexer, enumeration) is a read-only projection of `DimStyles` with no font-authoring surface.

[SECTION_STYLE]:

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `new SectionStyle()` / `new SectionStyle(SectionStyle)`                     | ctor     | fresh + copy                       |
|  [02]   | `SectionStyle.GetBoundaryLinetype()` / `SetBoundaryLinetype(Linetype)`      | instance | read or bind the boundary linetype |
|  [03]   | `SectionStyle.RemoveBoundaryLinetype()`                                     | instance | clear the boundary linetype        |
|  [04]   | `SectionStyle.ReadFromFile(string, out SectionStyle[], out HatchPattern[])` | static   | read styles + referenced patterns  |

- no `Duplicate()` member exists; the copy constructor is the duplicate seed.
[SECTION_FILL]: `BackgroundFillMode` `BackgroundFillColor` `BackgroundFillPrintColor` — cut-face fill
[SECTION_BOUNDARY]: `BoundaryVisible` `BoundaryColor` `BoundaryPrintColor` `BoundaryWidthScale` `BoundaryPlotWeightMillimeters` `BoundaryLinetypeIndex` — cut-boundary stroke, index through `LinetypeTable`
[SECTION_HATCH]: `HatchIndex` `HatchScale` `HatchRotationRadians` `HatchPatternColor` `HatchPatternPrintColor` — cut-fill hatch, index through `HatchPatternTable`
[SECTION_STATE]: `SectionFillRule` `InUse` `IsUnset` — fill rule and usage

[SECTION_TABLE]:

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `SectionStyleTable.FindName(string)` / `FindIndex(int)`                                 | instance | resolve by name or index         |
|  [02]   | `SectionStyleTable.Find(string) -> int` / `Find(Guid, bool) -> int`                     | instance | resolve to index by name or id   |
|  [03]   | `SectionStyleTable.Add(SectionStyle) -> int` / `AddReferenceSectionStyle(SectionStyle)` | instance | add from definition or reference |
|  [04]   | `SectionStyleTable.Modify(SectionStyle, int, bool)`                                     | instance | rewrite one                      |
|  [05]   | `SectionStyleTable.Delete(int, bool)` / `Delete(IEnumerable<int>, bool, int)`           | instance | delete one or batch              |
|  [06]   | `SectionStyleTable.InUse(int, out int, out int, out int) -> bool`                       | instance | usage census before delete       |

- `SectionStyleTable.InUse`: three-way census of definitions, objects, and layers before a delete.
[SECTION_TABLE_STATE]: `GetUnusedSectionStyleName()` `ActiveCount` — name-mint and active-count state

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Hatch` is boundary geometry, a `PatternIndex` reference, and placement (`Plane`, `BasePoint`, `PatternRotation`, `PatternScale`); pattern content is a `HatchPattern` owning an ordered `HatchLine` set, each line one dash-run generator. Gradient fill is orthogonal state set through `SetGradientFill`; `CreateDisplayGeometry` resolves the boundary, pattern lines, and solid-fill brep the display pipeline draws.
- `Linetype` is a dash-segment run with embedded curve/text shapes, optional taper, and stroke config (caps, joins, width); `.lin` strings and files interchange the definition, and an embedded shape carries a `TextEntity`.
- `Font` is a resolved installed typeface behind many name projections and style axes; construction and `FromQuartetProperties` resolve against the installed set, `GetSubstituteFont` covers an absent one, and document binding projects the `DimStyles` table rather than a distinct store.
- `SectionStyle` composes the other resources: `HatchIndex` binds a `HatchPattern`, `BoundaryLinetypeIndex` a `Linetype`; presentation is background fill, boundary stroke, and cut-fill hatch, and `InUse` censuses definitions, objects, and layers before a delete.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): table `bool`/`int` outcomes project to `Fin<Unit>`/`Fin<int>`; nullable `Find*`/`GetBoundaryLinetype` reads lift to `Option<A>`; `Create -> Hatch[]` and roster reads (`InstalledFonts`, `GetDefaultHatchPatterns`, `ReadFromFile`) land as `Seq<A>`; `GetSegment`/`InUse`/`CreateDisplayGeometry` `out` results fold into `Fin<A>` carrying the payload only on success.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `HatchPatternFillType`, `FontWeight`/`FontStyle`/`FontStretch`/`FontOrigin`/`FontType`, `SectionBackgroundFillMode`, `ObjectSectionFillRule`, and the referenced `LineCapStyle`/`LineJoinStyle`/`UnitSystem`/`ObjectLinetypeSource` map at the edge to `[SmartEnum]` owners; a pattern/linetype/font/section-style `Guid` is a `[ValueObject<Guid>]`; a table index is a bounded index owner, so an index never crosses as a bare `int`.
- `Rasm` kernel: hatch planes, base points, pattern transforms, dash lengths, taper points, and font point sizes compose the kernel numeric and unit owners; every `Color`-valued fill, boundary, and hatch field composes the kernel color rail, never a host channel average.
- `api-rhinocommon-annotation.md`: a `DimensionStyle.Font` and an embedded linetype shape resolve here; `api-rhinocommon-display.md` draws hatches (`DrawHatch`) and builds a `DisplayPen` from a `Linetype` (`FromLinetype`).

[LOCAL_ADMISSION]:
- a resource enters through its table `Add`/`Modify`, fully composed (lines, segments, shapes, indices) before the add and mutated only through `Modify` after. A section style's `HatchIndex`/`BoundaryLinetypeIndex` binds only after the referenced pattern and linetype resolve in their tables.
- live `Hatch`, `HatchPattern`, `Linetype`, `Font`, and `SectionStyle` values stay inside the document grant; downstream code receives bounded owners, detached geometry (boundary curves, pattern lines, solid breps, preview lines), resolved font handles, or projected receipts.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: hatch geometry and pattern definitions, linetype segment/shape/taper definitions, font resolution, and section-style presentation, each behind its table transaction.
- Accept: resource authoring and file interchange, hatch construction and display-geometry resolution, font resolution and substitution, and section-style composition projected onto `Fin`/`Option`/`Seq` rails with host enums and indices mapped to bounded owners at the edge.
- Reject: in-table resource mutation standing in for `Modify`, a section index bound before its referenced pattern or linetype resolves, exception-style table outcomes, a re-derived color blend where the kernel color rail is composed, and live document-bound resources crossing the session boundary.
