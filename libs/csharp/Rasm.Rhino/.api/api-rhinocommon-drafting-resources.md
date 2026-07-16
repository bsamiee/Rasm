# [RASM_RHINO_API_RHINOCOMMON_DRAFTING_RESOURCES]

This catalog owns the drafting-resource boundary the annotation styles reference: `Hatch` geometry construction, gradient fill, and pattern scaling, with `HatchPattern` line definitions, `HatchLine` dash geometry, and the `HatchPatternTable` transaction; `Linetype` segment/shape/taper definitions and the `LinetypeTable`; `Font` and `FontQuartet` typeface resolution behind the `FontTable`; and `SectionStyle` section-cut fill/boundary presentation behind the `SectionStyleTable`, which ties back to hatch and linetype indices. `DimensionStyle` and every annotation consuming these resources live in `api-rhinocommon-annotation.md`; drawn pixels cross through `api-rhinocommon-display.md` (`DrawHatch`, `DisplayPen.FromLinetype`); the generic purge-reclaim of each table stays registered in `Document/tables.md`; live document-bound values resolve inside the owning session before detached values cross the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon drafting-resource surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`, `Rhino.DocObjects`, `Rhino.DocObjects.Tables`
- kernel: `Rasm` (host-agnostic numeric, unit, and color owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: drafting-resource boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hatch geometry and pattern model
- rail: drafting-resource boundary

| [INDEX] | [SYMBOL]            | [KIND]         | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `Hatch`             | fill geometry  | boundary-loop hatch, gradient fill, pattern scale, brep egress |
|  [02]   | `HatchPattern`      | model resource | hatch-line definition set, defaults catalog, file read/write   |
|  [03]   | `HatchLine`         | line def       | one dash-line generator (angle, base, offset, dash run)        |
|  [04]   | `HatchPatternTable` | document table | pattern identity, authoring, rename, and reclaim               |

[PUBLIC_TYPE_SCOPE]: linetype segment/shape model
- rail: drafting-resource boundary

| [INDEX] | [SYMBOL]        | [KIND]         | [CAPABILITY]                                                     |
| :-----: | :-------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Linetype`      | model resource | dash-segment run, embedded shapes, taper, caps/joins, width      |
|  [02]   | `LinetypeTable` | document table | linetype identity, authoring, default loading, current selection |

[PUBLIC_TYPE_SCOPE]: typeface and section-cut resources
- rail: drafting-resource boundary

| [INDEX] | [SYMBOL]            | [KIND]         | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `Font`              | typeface       | installed-font resolution, quartet lookup, substitution        |
|  [02]   | `FontQuartet`       | family group   | regular/bold/italic/bold-italic availability of one family     |
|  [03]   | `FontTable`         | document table | font find-or-create keyed to a `DimStyles` template projection |
|  [04]   | `SectionStyle`      | model resource | section-cut background fill, boundary, and hatch presentation  |
|  [05]   | `SectionStyleTable` | document table | section-style identity, authoring, and usage census            |

[ENUM_ROSTERS]:
- `public enum Rhino.DocObjects.HatchPatternFillType` — `Solid`, `Lines`, `Gradient`.
- `public enum Rhino.DocObjects.Font.FontWeight : byte` — `Unset`, `Thin`, `Ultralight`, `Light`, `Normal`, `Medium`, `Semibold`, `Bold`, `Ultrabold`, `Heavy`.
- `public enum Rhino.DocObjects.Font.FontStyle : byte` — `Unset`, `Upright`, `Italic`, `Oblique`.
- `public enum Rhino.DocObjects.Font.FontStretch : byte` — `Unset`, `Ultracondensed`, `Extracondensed`, `Condensed`, `Semicondensed`, `Medium`, `Semiexpanded`, `Expanded`, `Extraexpanded`, `Ultraexpanded`.
- `public enum Rhino.DocObjects.Font.FontOrigin : byte` — `Unset = 0`, `Unknown = 1`, `WindowsFont = 2`, `AppleFont = 3`; `public enum Rhino.DocObjects.Font.FontType : byte` — `Unset = 0`, `ManagedFont = 1`, `InstalledFont = 2` (font-list availability). Neither enum surfaces through a public `Font` property.
- `public enum Rhino.DocObjects.SectionBackgroundFillMode : byte` — `None`, `Viewport`, `SolidColor`.
- `public enum Rhino.DocObjects.ObjectSectionFillRule` — `ClosedCurves`, `SolidObjects`.
- External bounded owners referenced at the edge: `Rhino.Display.LineCapStyle` and `LineJoinStyle` carry a `Linetype`'s caps and joins; `Rhino.UnitSystem` carries pattern and width units.

## [03]-[ENTRYPOINTS]

[HATCH_CONSTRUCT]:
- `Rhino.Geometry.Hatch.Create(Plane hatchPlane, Curve outerLoop, IEnumerable<Curve> innerLoops, int hatchPatternIndex, double rotationRadians, double scale) : Hatch` [static] — one hatch from an explicit outer loop and inner holes.
- `Rhino.Geometry.Hatch.Create(IEnumerable<Curve> curves, int hatchPatternIndex, double rotationRadians, double scale[, double tolerance]) : Hatch[]` [static] — resolves nested closed curves into one-or-more hatches; the `Curve`-singular overloads (`Create(Curve curve, ...)`) take one curve.
- `Rhino.Geometry.Hatch.CreateFromBrep(Brep brep, int brepFaceIndex, int hatchPatternIndex, double rotationRadians, double scale, Point3d basePoint) : Hatch` [static] — hatches one planar brep face.
- `Rhino.Geometry.Hatch.Get2dCurves(bool outer) : Curve[]` / `Get3dCurves(bool outer) : Curve[]` — boundary loops in hatch-plane 2d or world 3d, `outer` selecting perimeter versus holes.
- `Rhino.Geometry.Hatch.CreateDisplayGeometry(HatchPattern pattern, double patternScale, out Curve[] bounds, out Line[] lines, out Brep solidBrep) : void` — resolves drawable boundary curves, pattern lines, and a solid-fill brep in one call.
- `Rhino.Geometry.Hatch.ToBrep() : Brep` / `Explode() : GeometryBase[]` — planar-region brep and decomposition into boundary and pattern geometry.
- `Rhino.Geometry.Hatch.GetGradientFill() : ColorGradient` / `SetGradientFill(ColorGradient fill) : void` — gradient-fill read and write.
- `Rhino.Geometry.Hatch.ScalePattern(Transform xform) : void` — rescales the pattern independent of the boundary.
- `Rhino.Geometry.Hatch.PatternIndex : int` / `PatternRotation : double` / `PatternScale : double` / `BasePoint : Point3d` / `Plane : Plane` — the fill parameters.

[HATCH_PATTERN]:
- `Rhino.DocObjects.HatchPattern.GetDefaultHatchPatterns() : HatchPattern[]` [static] — the built-in pattern roster.
- `Rhino.DocObjects.HatchPattern.ReadFromFile(string filename, bool quiet) : HatchPattern[]` [static] / `WriteToFile(string filename, HatchPattern hatchPattern) : bool` / `WriteToFile(string filename, IEnumerable<HatchPattern> hatchPatterns) : bool` [static] — `.pat` import and export.
- `Rhino.DocObjects.HatchPattern.CreatePreviewGeometry(int width, int height, double angle) : Line[]` — swatch preview lines.
- `Rhino.DocObjects.HatchPattern.AddHatchLine(HatchLine hatchLine) : int` / `HatchLineAt(int hatchLineIndex) : HatchLine` / `RemoveHatchLine(int hatchLineIndex) : bool` / `RemoveAllHatchLines() : void` / `SetHatchLines(IEnumerable<HatchLine> hatchLines) : int` — the line-definition set; `HatchLineCount : int` and `HatchLines : IEnumerable<HatchLine>` read it.
- `Rhino.DocObjects.HatchPattern.FillType : HatchPatternFillType` / `Description : string` / `PatternUnitSystem : UnitSystem` / `AlwaysModelDistances : bool` / `InUse : bool` — pattern config and usage.
- `Rhino.DocObjects.HatchPattern.Defaults` — static catalog: `Solid`, `Hatch1`, `Hatch2`, `Hatch3`, `Dash`, `Grid`, `Grid60`, `Plus`, `Squares`, each a fresh built-in `HatchPattern`.
- `Rhino.DocObjects.HatchPattern.SetUserString(string key, string value) : bool` / `GetUserString(string key) : string` / `GetUserStrings() : NameValueCollection` / `DeleteUserString(string key) : bool` / `DeleteAllUserStrings() : void` — the pattern's user-string bag.
- `Rhino.DocObjects.HatchLine.SetDashes(IEnumerable<double> dashes) : void` / `AppendDash(double dash) : void` / `DashAt(int dashIndex) : double` / `GetDashes : IEnumerable<double>` — the dash run; `Angle : double`, `BasePoint : Point2d`, `Offset : Vector2d`, `DashCount : int`, `PatternLength : double` define one generator line.

[HATCH_TABLE]:
- `Rhino.DocObjects.Tables.HatchPatternTable.FindName(string name) : HatchPattern` — resolves by name; `Find(string name, bool ignoreDeleted) : int`, `FindIndex(int index) : HatchPattern`, `FindNameHash(NameHash nameHash) : HatchPattern` add the index and hash routes.
- `Rhino.DocObjects.Tables.HatchPatternTable.Add(HatchPattern pattern) : int` — adds a pattern; `Modify(HatchPattern hatchPattern, int hatchPatternIndex, bool quiet) : bool` rewrites one.
- `Rhino.DocObjects.Tables.HatchPatternTable.Rename(int hatchPatternIndex, string hatchPatternName) : bool` — renames by index; the `(HatchPattern, string)` overload renames by handle.
- `Rhino.DocObjects.Tables.HatchPatternTable.Delete(int hatchPatternIndex, bool quiet) : bool` — deletes one; `Delete(IEnumerable<int> hatchPatternIndices, bool quiet) : int` batch-deletes; `PurgeUnused() : int` reclaims unreferenced patterns; `CurrentHatchPatternIndex : int` and `GetUnusedHatchPatternName() : string` read table state.

[LINETYPE_DEFINITION]:
- `Rhino.DocObjects.Linetype.AppendSegment(double length, bool isSolid) : int` / `SetSegment(int index, double length, bool isSolid) : bool` / `GetSegment(int index, out double length, out bool isSolid) : void` / `RemoveSegment(int index) : bool` / `SetSegments(IEnumerable<double> segments) : bool` — the dash/gap segment run; `SegmentCount : int` and `PatternLength : double` read it.
- `Rhino.DocObjects.Linetype.AddShape(Curve shapeCurve, double offset) : bool` / `AddShape(TextEntity text, double offset) : bool` / `RemoveAllShapes() : void` — embedded curve and text shapes along the pattern; `HasShapes : bool`, `ShapeSpacing : double`, `ShapeGap : double`, `ShapeLocalOffset : Vector2d`, `ShapeBounds : BoundingBox` position them.
- `Rhino.DocObjects.Linetype.SetTaper(double startWidth, double endWidth) : void` / `SetTaper(double startWidth, Point2d taperPoint, double endWidth) : void` / `GetTaperPoints() : Point2d[]` / `RemoveTaper() : void` — width taper along the line.
- `Rhino.DocObjects.Linetype.PatternString(bool millimeters) : string` / `CreateFromPatternString(string patternString, bool millimeters) : Linetype` [static] / `ReadFromFile(string path) : Linetype[]` [static] — `.lin` string and file interchange.
- `Rhino.DocObjects.Linetype.LineCapStyle : LineCapStyle` / `LineJoinStyle : LineJoinStyle` / `Width : double` / `WidthUnits : UnitSystem` / `AlwaysModelDistances : bool` / `IsPatternLocked : bool` — stroke config; `Default() : void`, `DuplicateLinetype() : Linetype`, `CommitChanges() : bool` maintain the definition.
- `Rhino.DocObjects.Linetype.Name : string` / `LinetypeIndex : int` / `InUse : bool` / `IsModified : bool` / `UserStringCount : int` — identity and state; no `LinetypeSegment` shape enum exists (a segment is the `(double length, bool isSolid)` pair) and no static default linetypes exist — named defaults live only on the table (`ContinuousLinetypeName`/`ByLayerLinetypeName`/`ByParentLinetypeName` + `LoadDefaultLinetypes`).
- `Rhino.DocObjects.Linetype.SetUserString(string key, string value) : bool` / `GetUserString` / `GetUserStrings` / `DeleteUserString` / `DeleteAllUserStrings` — the linetype user-string bag.

[LINETYPE_TABLE]:
- `Rhino.DocObjects.Tables.LinetypeTable.FindName(string name) : Linetype` — resolves by name; `Find(string name[, bool ignoreDeletedLinetypes]) : int`, `Find(Guid id, bool ignoreDeletedLinetypes) : int`, `FindIndex(int index) : Linetype` add the index and id routes.
- `Rhino.DocObjects.Tables.LinetypeTable.Add(Linetype linetype) : int` / `Add(string name, IEnumerable<double> segmentLengths) : int` / `AddReferenceLinetype(Linetype linetype) : int` — adds from a definition, a name-plus-segments pair, or a reference source.
- `Rhino.DocObjects.Tables.LinetypeTable.Modify(Linetype linetype, int index, bool quiet) : bool` / `UndoModify(int index) : bool` — rewrite and rollback.
- `Rhino.DocObjects.Tables.LinetypeTable.Delete(int index, bool quiet) : bool` / `Delete(IEnumerable<int> indices, bool quiet) : bool` / `Undelete(int index) : bool` / `PurgeUnused() : int` — lifecycle and reclaim.
- `Rhino.DocObjects.Tables.LinetypeTable.LoadDefaultLinetypes([bool ignoreDeleted]) : int` — loads the built-in set; `SetCurrentLinetypeIndex(int linetypeIndex, bool quiet) : bool`, `LinetypeIndexForObject(RhinoObject rhinoObject) : int`, `GetUnusedLinetypeName([bool ignoreDeleted]) : string` read/write table state.
- `Rhino.DocObjects.Tables.LinetypeTable.CurrentLinetype : Linetype` / `CurrentLinetypeIndex : int` / `CurrentLinetypeSource : ObjectLinetypeSource` / `LinetypeScale : double` / `ContinuousLinetypeName : string` / `ByLayerLinetypeName : string` / `ByParentLinetypeName : string` / `ActiveCount : int` — the current-linetype, named-default, and non-deleted-count state.

[FONT_RESOLUTION]:
- `Rhino.DocObjects.Font.FromQuartetProperties(string quartetName, bool bold, bool italic) : Font` [static] — resolves one face from a family quartet.
- `Rhino.DocObjects.Font.InstalledFonts() : Font[]` [static] / `InstalledFonts(string familyName) : Font[]` [static] / `InstalledFontsAsQuartets() : FontQuartet[]` [static] / `AvailableFontFaceNames() : string[]` [static] — the installed-font census by face, family, and quartet.
- `Rhino.DocObjects.Font.GetSubstituteFont() : Font` — the substitute for an uninstalled font; `IsInstalled : bool` reports installation.
- `Rhino.DocObjects.Font(string familyName[, FontWeight weight, FontStyle style[, FontStretch stretch], bool underlined, bool strikethrough])` — direct construction from family and style axes.
- `Rhino.DocObjects.Font.FaceName : string` / `FamilyName : string` / `FamilyPlusFaceName : string` / `PostScriptName : string` / `LogfontName : string` / `QuartetName : string` / `RichTextFontName : string` / `Description : string` — the name projections; exactly three carry English twins (`EnglishFaceName`, `EnglishFamilyName`, `EnglishQuartetName`); `Weight : FontWeight`, `Style : FontStyle`, `Stretch : FontStretch`, `Bold`/`Italic`/`Underlined`/`Strikeout : bool`, `PointSize : double`, `IsEngravingFont`/`IsSymbolFont`/`IsSingleStrokeFont`/`IsGeometricToleranceFont`/`IsSimulated : bool` carry the resolved metrics.
- `Rhino.DocObjects.FontQuartet.QuartetName : string` / `HasRegularFont : bool` / `HasBoldFont : bool` / `HasItalicFont : bool` / `HasBoldItalicFont : bool` — one family's four-face availability.
- `Rhino.DocObjects.Tables.FontTable.FindOrCreate(string face, bool bold, bool italic) : int` / `FindOrCreate(string face, bool bold, bool italic, DimensionStyle template_style) : int` — resolves or mints a font, seeding from a template style; `Count`, `CurrentIndex`, and enumeration project the `DimStyles` table (`FontTable` is a face over dimension styles).

[SECTION_STYLE]:
- `Rhino.DocObjects.SectionStyle.BackgroundFillMode : SectionBackgroundFillMode` / `BackgroundFillColor : Color` / `BackgroundFillPrintColor : Color` — cut-face fill.
- `Rhino.DocObjects.SectionStyle.BoundaryVisible : bool` / `BoundaryColor : Color` / `BoundaryPrintColor : Color` / `BoundaryWidthScale : double` / `BoundaryPlotWeightMillimeters : double` / `BoundaryLinetypeIndex : int` — cut-boundary stroke, its linetype index resolving through `LinetypeTable`.
- `Rhino.DocObjects.SectionStyle.HatchIndex : int` / `HatchScale : double` / `HatchRotationRadians : double` / `HatchPatternColor : Color` / `HatchPatternPrintColor : Color` — cut-fill hatch, its index resolving through `HatchPatternTable`.
- `Rhino.DocObjects.SectionStyle.SectionFillRule : ObjectSectionFillRule` / `InUse : bool` / `IsUnset : bool` — fill rule and usage.
- `Rhino.DocObjects.SectionStyle.GetBoundaryLinetype() : Linetype` / `SetBoundaryLinetype(Linetype linetype) : void` / `RemoveBoundaryLinetype() : void` — boundary-linetype resolution.
- `Rhino.DocObjects.SectionStyle.ReadFromFile(string filename, out SectionStyle[] sectionStyles, out HatchPattern[] hatchPatterns) : bool` [static] — reads styles and their referenced patterns together.

[SECTION_TABLE]:
- `Rhino.DocObjects.Tables.SectionStyleTable.FindName(string name) : SectionStyle` — resolves by name; `Find(string name) : int`, `Find(Guid id, bool ignoreDeletedSectionStyles) : int`, `FindIndex(int index) : SectionStyle` add the index and id routes.
- `Rhino.DocObjects.Tables.SectionStyleTable.Add(SectionStyle sectionstyle) : int` / `AddReferenceSectionStyle(SectionStyle sectionstyle) : int` — adds from a definition or reference source.
- `Rhino.DocObjects.Tables.SectionStyleTable.Modify(SectionStyle sectionstyle, int index, bool quiet) : bool` — rewrites one.
- `Rhino.DocObjects.Tables.SectionStyleTable.Delete(int index, bool quiet) : bool` / `Delete(IEnumerable<int> sectionStyleIndices, bool quiet[, int deleteWarning]) : int` — lifecycle.
- `Rhino.DocObjects.Tables.SectionStyleTable.InUse(int index, out int instanceDefinitionCount, out int objectCount, out int layerCount) : bool` — the three-way usage census before delete; `GetUnusedSectionStyleName() : string` and `ActiveCount : int` read table state.

## [04]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- `Hatch` is boundary geometry plus a pattern reference (`PatternIndex`) plus placement (`Plane`, `BasePoint`, `PatternRotation`, `PatternScale`); the pattern content is a `HatchPattern` owning an ordered `HatchLine` set, each line one dash-run generator. Gradient fill is orthogonal state set through `SetGradientFill`, and `CreateDisplayGeometry` resolves the boundary, pattern lines, and solid-fill brep the display pipeline draws.
- `Linetype` is a dash-segment run plus embedded curve/text shapes plus optional taper plus stroke config (caps, joins, width); `.lin` pattern strings and files interchange the definition, and shapes embed a `TextEntity` from `api-rhinocommon-annotation.md`.
- `Font` is a resolved installed typeface identified by many name projections and style axes; construction and `FromQuartetProperties` resolve against the installed set, `GetSubstituteFont` covers an absent one, and `FontTable` is a thin find-or-create face projecting the `DimStyles` table rather than a distinct store.
- `SectionStyle` composes the other resources: `HatchIndex` binds a `HatchPattern`, `BoundaryLinetypeIndex` a `Linetype`; its presentation is background fill plus boundary stroke plus cut-fill hatch, and `InUse` censuses definitions, objects, and layers before any delete.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): table `bool`/`int` outcomes project to `Fin<Unit>`/`Fin<int>`; nullable `Find*`/`GetBoundaryLinetype` reads lift to `Option<A>`; `Create` returning `Hatch[]` and roster reads (`InstalledFonts`, `GetDefaultHatchPatterns`, `ReadFromFile`) land as `Seq<A>`; `GetSegment`/`InUse`/`CreateDisplayGeometry` `out` results fold into `Fin<A>` carrying the payload only on success.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `HatchPatternFillType`, `FontWeight`/`FontStyle`/`FontStretch`/`FontOrigin`/`FontType`, `SectionBackgroundFillMode`, `ObjectSectionFillRule`, and the referenced `LineCapStyle`/`LineJoinStyle`/`UnitSystem`/`ObjectLinetypeSource` map at the edge to `[SmartEnum]` owners; a pattern, linetype, font, or section-style `Guid` is a `[ValueObject<Guid>]`; a table index is a bounded index owner, so an index never crosses as a bare `int`.
- `Rasm` kernel: hatch planes, base points, pattern transforms, dash lengths, taper points, and font point sizes compose the kernel numeric and unit owners; every `Color`-valued fill, boundary, and hatch field composes the kernel color rail, never a host channel average.
- `api-rhinocommon-annotation.md`: a `DimensionStyle.Font` and an embedded linetype shape resolve here; `api-rhinocommon-display.md` draws hatches (`DrawHatch`) and builds a `DisplayPen` from a `Linetype` (`FromLinetype`).

[LOCAL_ADMISSION]:
- a resource enters through its table `Add`/`Modify`; a `HatchPattern`/`Linetype`/`SectionStyle` is fully composed (lines, segments, shapes, indices) before the add, never mutated in the table after the fact except through `Modify`. A section style's `HatchIndex`/`BoundaryLinetypeIndex` binds only after the referenced pattern and linetype resolve in their tables.
- live `Hatch`, `HatchPattern`, `Linetype`, `Font`, and `SectionStyle` values stay inside the document grant; downstream code receives bounded owners, detached geometry (boundary curves, pattern lines, solid breps, preview lines), resolved font handles, or projected receipts.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` + `Rhino.DocObjects` + `Rhino.DocObjects.Tables` drafting-resource reads
- Owns: hatch geometry and pattern definitions, linetype segment/shape/taper definitions, font resolution, and section-style presentation, each behind its table transaction.
- Accept: resource authoring and file interchange, hatch construction and display-geometry resolution, font resolution and substitution, and section-style composition projected onto `Fin`/`Option`/`Seq` rails with host enums and indices mapped to bounded owners at the edge.
- Reject: in-table resource mutation standing in for `Modify`, a section index bound before its referenced pattern or linetype resolves, exception-style table outcomes, a re-derived color blend where the kernel color rail is composed, and live document-bound resources crossing the session boundary.
