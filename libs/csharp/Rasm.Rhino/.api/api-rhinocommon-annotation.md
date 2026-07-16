# [RASM_RHINO_API_RHINOCOMMON_ANNOTATION]

This catalog owns the annotation model boundary: `DimensionStyle` drafting-config algebra with its field-override inheritance and the `DimStyleTable` transaction; `AnnotationBase` rich-text run editing and property-override projection shared by every annotation; `TextEntity` construction plus the text-to-curve/surface/polysurface/extrusion outlining family; `Leader` construction; the six-kind `Dimension` geometry family over one per-instance override surface; and `TextFields` formula evaluators feeding the rich-text field tokens. Drawn pixels cross through `api-rhinocommon-display.md`; hatch, linetype, font, and section resources those styles reference live in `api-rhinocommon-drafting-resources.md`; the block-attribute `TextFields` slice stays in `api-rhinocommon-blocks.md`; `TextDot` derives straight from `GeometryBase` and carries no `DimensionStyle`, so it stays outside this style-bound boundary; every live document-bound annotation resolves inside the owning session before detached values cross the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon annotation surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.Geometry`, `Rhino.Runtime`
- kernel: `Rasm` (host-agnostic numeric, unit, and vocabulary owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: annotation-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: style config and its table
- rail: annotation-boundary

| [INDEX] | [SYMBOL]         | [KIND]         | [CAPABILITY]                                                           |
| :-----: | :--------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `DimensionStyle` | model style    | drafting config, field-override algebra, parent/child inheritance      |
|  [02]   | `DimStyleTable`  | document table | style identity, authoring, current-style selection, reverse-projection |

[PUBLIC_TYPE_SCOPE]: annotation geometry and text
- rail: annotation-boundary

| [INDEX] | [SYMBOL]         | [KIND]          | [CAPABILITY]                                                      |
| :-----: | :--------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `AnnotationBase` | geometry base   | RTF run model, property-override projection, mask and plane state |
|  [02]   | `TextEntity`     | text geometry   | text construction and curve/surface/extrusion outlining           |
|  [03]   | `Leader`         | leader geometry | polyline/spline leader construction and explode                   |
|  [04]   | `TextFields`     | static surface  | text-field formula evaluators feeding rich-text field tokens      |

[PUBLIC_TYPE_SCOPE]: dimension family and doc-object projections
- rail: annotation-boundary

| [INDEX] | [SYMBOL]               | [KIND]             | [CAPABILITY]                                                               |
| :-----: | :--------------------- | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `Dimension`            | dimension base     | per-instance override of nearly the whole `DimensionStyle`                 |
|  [02]   | `LinearDimension`      | dimension geometry | aligned/rotated linear dimension and display lines                         |
|  [03]   | `AngularDimension`     | dimension geometry | angle-between construction from lines, points, or arc                      |
|  [04]   | `RadialDimension`      | dimension geometry | radius/diameter dimension and display lines                                |
|  [05]   | `OrdinateDimension`    | dimension geometry | x/y ordinate with kink-offset leader geometry                              |
|  [06]   | `Centermark`           | dimension geometry | radial centermark cross-and-lines                                          |
|  [07]   | `AnnotationObjectBase` | doc object         | `AnnotationGeometry` typed accessor over a document annotation             |
|  [08]   | `*DimensionObject`     | doc object         | typed geometry accessor per dimension kind and `TextObject`/`LeaderObject` |

[ENUM_ROSTERS]:
- `public enum DimensionStyle.ArrowType` — `None`, `UserBlock`, `SolidTriangle`, `Dot`, `Tick`, `ShortTriangle`, `OpenArrow`, `Rectangle`, `LongTriangle`, `LongerTriangle`, `SolidDatumTriangle` (0..10).
- `public enum DimensionStyle.ClippingArrowType` — `None`, `Triangle`, `OffsetTriangle`, `Arrow`, `OffsetArrow`, `OpenArrow`, `Rectangle`, `Ribbon`, `Line` (0..8).
- `public enum DimensionStyle.MaskType : byte` — `BackgroundColor`, `MaskColor`.
- `public enum DimensionStyle.MaskFrame : byte` — `NoFrame`, `RectFrame`, `CapsuleFrame`, `CircleFrame`, `SquareFrame`, `DiamondFrame`, `TriangleFrame`, `HexagonFrame`, `HexagonCapsuleFrame`, `RoundRectFrame`.
- `public enum DimensionStyle.LengthDisplay` — `ModelUnits = 0`, `InchesFractional = 1`, `FeetAndInches = 2`, `Millmeters = 3`, `Centimeters = 4`, `Meters = 5`, `Kilometers = 6`, `InchesDecimal = 7`, `FeetDecimal = 8`, `Miles = 9` (values `0..9` contiguous; host declares them out of value order, so a `[SmartEnum]` mapping keys on the explicit value, never declaration position; `Millmeters` spelling is the host spelling).
- `public enum DimensionStyle.ToleranceDisplayFormat : byte` — `None`, `Symmetrical`, `Deviation`, `Limits`.
- `public enum DimensionStyle.LeaderContentAngleStyle : byte` — `Horizontal`, `Aligned`, `Rotated`.
- `public enum DimensionStyle.LeaderCurveStyle : byte` — `None`, `Polyline`, `Spline`.
- `public enum DimensionStyle.AngleDisplayFormat : byte` — `DecimalDegrees`, `DegMinSec`, `Radians`, `Grads`.
- `public enum DimensionStyle.TextLocation : byte` — `AboveDimLine`, `InDimLine`, `BelowDimLine`.
- `public enum DimensionStyle.ZeroSuppression : byte` — `None = 0`, `SuppressLeading = 1`, `SuppressTrailing = 2`, `SuppressLeadingAndTrailing = 3`, `SuppressZeroFeet = 4`, `SuppressZeroInches = 8`, `SuppressZeroFeetAndZeroInches = 12` (bit flags).
- `public enum DimensionStyle.StackDisplayFormat : byte` — `None`, `StackHorizontal`, `StackDiagonal`.
- `public enum DimensionStyle.CenterMarkStyle : byte` — `None`, `Mark`, `MarkAndLines`.
- `public enum DimensionStyle.ArrowFit : byte` — `Auto`, `ArrowsInside`, `ArrowsOutside`.
- `public enum DimensionStyle.TextFit : byte` — `Auto`, `TextInside`, `TextRight`, `TextLeft`, `TextHintRight`, `TextHintLeft`.
- `public enum DimensionStyle.LengthDisplayFormat : byte` — `Decimal`, `Fractional`, `FeetInches`, `FeetDecimalInches`; obsolete, superseded by `LengthDisplay`.
- `public enum DimensionStyle.Field` — field-identifier vocabulary keyed by the override algebra: 117 members spanning `Unset = 0` through `Count = 120`, NON-contiguous (values `19`, `74`, `107`, `108` are skipped); most cases name one config property (`Arrowsize = 5`, `TextHeight = 9`, `LengthFactor = 16`, `ToleranceFormat = 48`, `DimensionScale = 59`, `MaskFrameType = 11`, `ArrowType1 = 78`, `ClippingArrowSize = 119`, and peers), while the color/plot group (`ExtLineColorSource = 28` through `DimLinePlotWeight_mm = 47`) plus `MaskFlags = 92`, `SignedOrdinate = 97`, and `UnitSystem = 98` carry NO CLR property on `DimensionStyle` and exist only as override-marking vocabulary; `Name = 1`/`Index = 2` cannot inherit from a parent.
- `public enum Dimension.ForceArrow` — `Auto`, `Inside`, `Outside`.
- `public enum Dimension.ForceText` — `Auto`, `Inside`, `Right`, `Left`, `HintRight`, `HintLeft`; marked `[Obsolete]` on the host, so text fit routes through the `TextFit`/`ArrowFit` fields.
- `public enum OrdinateDimension.MeasuredDirection : byte` — `Unset`, `Xaxis`, `Yaxis`.
- `public enum Rhino.Geometry.AnnotationType : byte` — `Unset`, `Aligned`, `Angular`, `Diameter`, `Radius`, `Rotated`, `Ordinate`, `ArcLen`, `CenterMark`, `Text`, `Leader`, `Angular3pt` (0..11); the discriminant `LinearDimension.Create` and `RadialDimension.Create` accept.
- `public enum Rhino.DocObjects.Tables.ModifyType` — `Modify`, `Override`, `NotSaved`; `DimStyleTable.Modify(DimensionStyle, AnnotationBase)` outcome.
- External bounded owners mapped at the edge: `Rhino.DocObjects.TextOrientation`, `TextVerticalAlignment`, `TextHorizontalAlignment`, `TextJustification` carry text placement referenced by `DimensionStyle`, `AnnotationBase`, and `Dimension`.

## [03]-[ENTRYPOINTS]

[STYLE_IDENTITY]:
- `Rhino.DocObjects.Tables.DimStyleTable.Current : DimensionStyle` / `CurrentDimensionStyle` — live snapshot of the current style; `CurrentIndex : int` and `CurrentId : Guid` carry its handles.
- `Rhino.DocObjects.Tables.DimStyleTable[int index] : DimensionStyle` — indexer read over the runtime component index.
- `Rhino.DocObjects.Tables.DimStyleTable.FindName(string name) : DimensionStyle` — resolves by name; `Find(string name, bool ignoreDeleted)` and `Find(Guid styleId, bool ignoreDeleted)` add the deleted-inclusion switch.
- `Rhino.DocObjects.Tables.DimStyleTable.FindIndex(int index) : DimensionStyle` — resolves by runtime index.
- `Rhino.DocObjects.Tables.DimStyleTable.FindRoot(Guid styleId, bool ignoreDeleted) : DimensionStyle` — walks the parent chain to the inheritance root.
- `Rhino.DocObjects.Tables.DimStyleTable.BuiltInStyles : DimensionStyle[]` — the built-in style roster.
- `Rhino.DocObjects.Tables.DimStyleTable.GetUnusedStyleName(string rootName) : string` — mints an unused style name.

[STYLE_AUTHORING]:
- `Rhino.DocObjects.Tables.DimStyleTable.Add(string name) : int` — adds a default-config style; `Add(string name, bool reference)` and `Add(DimensionStyle dimstyle, bool reference)` seed from config or reference-source flag.
- `Rhino.DocObjects.Tables.DimStyleTable.Modify(DimensionStyle newSettings, int dimstyleIndex, bool quiet) : bool` — writes new config by index; the `(DimensionStyle, Guid dimstyleId, bool quiet)` overload writes by id.
- `Rhino.DocObjects.Tables.DimStyleTable.Modify(DimensionStyle dimstyle, AnnotationBase annotation) : ModifyType` — reverse-projects a live annotation's overrides back onto a style, returning whether the write landed as `Modify`, `Override`, or `NotSaved`.
- `Rhino.DocObjects.Tables.DimStyleTable.SetCurrent(int index, bool quiet) : bool` / `SetCurrentDimensionStyleIndex(int index, bool quiet)` — sets the active style.
- `Rhino.DocObjects.Tables.DimStyleTable.Delete(int index, bool quiet) : bool` — deletes a style; `PurgeUnused() : int` reclaims unreferenced styles (the reclaim registered in `Document/tables.md`).

[STYLE_FIELD_ALGEBRA]:
- `Rhino.DocObjects.DimensionStyle.SetFieldOverride(Field field) : void` — marks one field overridden so its value comes from this child, not the parent; every config setter already passes `setOverride: true` natively, so writing a property marks its own field without a separate call.
- `Rhino.DocObjects.DimensionStyle.ClearFieldOverride(Field field) : void` / `ClearAllFieldOverrides() : void` — clears one or every override without touching id or parent id.
- `Rhino.DocObjects.DimensionStyle.IsFieldOverriden(Field field) : bool` — tests one field's override state; `HasFieldOverrides : bool` tests any.
- `Rhino.DocObjects.DimensionStyle.ParentId : Guid` — parent style id; `Guid.Empty` means no parent, `IsChild : bool` and `IsChildOf(Guid parentId) : bool` probe the relationship.
- `Rhino.DocObjects.DimensionStyle.Duplicate() : DimensionStyle` — deep copy; `Duplicate(string newName, Guid newId, Guid newParentId)` re-keys the copy in one call.
- `Rhino.DocObjects.DimensionStyle.CopyFrom(DimensionStyle source) : void` — copies every setting except name, id, and index.
- `Rhino.DocObjects.DimensionStyle.ScaleLengthValues(double scale) : void` — scales all length-valued fields; `DimensionLengthDisplayUnit(uint modelSerialNumber) : UnitSystem` and `AlternateDimensionLengthDisplayUnit(uint)` resolve the rendered unit under a model.
- `Rhino.DocObjects.DimensionStyle.CreatePreviewBitmap(int width, int height, bool transparent) : Bitmap` — renders a style swatch; the `(int, int)` overload defaults opaque.
- `Rhino.DocObjects.DimensionStyle.SetUserString(string key, string value) : bool` / `GetUserString` / `GetUserStrings` / `DeleteUserString` / `DeleteAllUserStrings` — the style's user-string bag.

[STYLE_CONFIG]:
- `Rhino.DocObjects.DimensionStyle` exposes ~96 get/set config properties, most backed by a `Field` case (each setter marks its field overridden; the color/plot-source `Field` group has no property counterpart): arrow axis (`ArrowType1`/`ArrowType2`/`LeaderArrowType`, `ArrowLength`/`LeaderArrowLength`/`ClippingArrowLength`, `ArrowBlockId1`/`ArrowBlockId2`/`LeaderArrowBlockId`, `ClippingArrowType1`/`ClippingArrowType2`, `FitArrow`, `SuppressArrow1`/`SuppressArrow2`), text axis (`TextHeight`, `TextGap`, `TextRotation`, `Font`, `TextVerticalAlignment`/`TextHorizontalAlignment`, `TextOrientation`/`LeaderTextOrientation`/`DimTextOrientation`/`DimRadialTextOrientation`, `DimTextLocation`/`DimRadialTextLocation`, `DimTextAngleType`/`DimRadialTextAngleType`, `FitText`, `UseKerning`, `TextUnderlined`, `LineSpaceScale`, `DrawForward`), length/tolerance axis (`LengthFactor`/`AlternateLengthFactor`, `LengthResolution`/`AlternateLengthResolution`/`AngleResolution`, `DimensionLengthDisplay`/`AlternateDimensionLengthDisplay`, `AngleFormat`, `ToleranceFormat`, `ToleranceResolution`/`AlternateToleranceResolution`, `ToleranceHeightScale`, `ToleranceUpperValue`/`ToleranceLowerValue`, `ToleranceZeroSuppress`, `ZeroSuppress`/`AlternateZeroSuppress`/`AngleZeroSuppress`, `Roundoff`/`AlternateRoundoff`/`AngularRoundoff`, `Prefix`/`Suffix`/`AlternatePrefix`/`AlternateSuffix`, `DecimalSeparator`, `StackFractionFormat`/`StackHeightScale`, `AlternateUnitsDisplay`/`AlternateBelowLine`), mask axis (`DrawTextMask`, `MaskColor`/`MaskColorSource`, `MaskFrameType`, `MaskOffset` pairing `Field.MaskBorder`), layout axis (`BaselineSpacing`, `DimensionScale`/`DimensionScaleValue`, `ScaleLeftLengthMillimeters`/`ScaleRightLengthMillimeters`, `CentermarkSize`/`CenterMarkType`, `ExtensionLineExtension`/`ExtensionLineOffset`, `DimensionLineExtension`, `SuppressExtension1`/`SuppressExtension2`, `FixedExtensionOn`/`FixedExtensionLength`, `TextMoveLeader`, `ArcLengthSymbol`, `ForceDimensionLineBetweenExtensionLines`), and leader axis (`LeaderHasLanding`/`LeaderLandingLength`, `LeaderContentAngleType`, `LeaderCurveType`, `LeaderTextVerticalAlignment`/`LeaderTextHorizontalAlignment`, `LeaderTextRotationRadians`/`LeaderTextRotationDegrees`). `UserStringCount : int` counts the user-string bag. Two anomalies: `ToleranceZeroSuppress` is an inert stub — constant `ZeroSuppression.None` getter, empty setter, no `Field` pairing — and `Field.LeaderContentAngle` backs both `LeaderContentAngleType` (`GetInt`/`SetInt`) and `LeaderTextRotationRadians`/`Degrees` (`GetDouble`/`SetDouble`) while `Field.LeaderContentAngleStyle` binds no accessor.

[ANNOTATION_TEXT]:
- `Rhino.Geometry.AnnotationBase.RichText : string` / `PlainText : string` — the RTF source and its flattened plain text; field tokens ride the rich text, and `Text` / `TextFormula` are `[Obsolete]` aliases of `PlainText` / `RichText`.
- `Rhino.Geometry.AnnotationBase.PlainTextWithFields : string` / `GetPlainTextWithRunMap(ref int[] map) : string` — plain text retaining field tokens, and plain text plus a run map of `(runIndex, charStart, length)` int triples.
- `Rhino.Geometry.AnnotationBase.DimensionStyleId : Guid` / `DimensionStyle : DimensionStyle` / `ParentDimensionStyle : DimensionStyle` — the bound style, its effective form, and its parent.
- `Rhino.Geometry.AnnotationBase.Plane : Plane` / `TextHeight : double` / `TextRotationRadians : double` / `TextRotationDegrees : double` / `TextIsWrapped : bool` / `FormatWidth : double` / `TextModelWidth : double` — text frame placement.
- `Rhino.Geometry.AnnotationBase.Font : Font` / `FirstCharFont : Font` — the annotation font and its first-run font; `FontIndex` is `[Obsolete]` and round-trips the ambient `RhinoDoc.ActiveDoc.Fonts`.
- `Rhino.Geometry.AnnotationBase.MaskEnabled : bool` / `MaskColor : Color` / `MaskColorSource : DimensionStyle.MaskType` / `MaskFrame : DimensionStyle.MaskFrame` / `MaskOffset : double` / `MaskUsesViewportColor : bool` / `DrawTextFrame : bool` — text-mask state.
- `Rhino.Geometry.AnnotationBase.HasPropertyOverrides : bool` / `IsPropertyOverridden(DimensionStyle.Field field) : bool` — per-instance override presence.
- `Rhino.Geometry.AnnotationBase.AnnotationType : AnnotationType` [virtual] — the annotation-kind discriminant read off the base.
- `Rhino.Geometry.AnnotationBase.DecimalSeparator : char` / `UseKerning : bool` / `LineSpaceScale : double` / `DimensionScale : double` / `DrawForward : bool` / `DimensionLengthDisplay : DimensionStyle.LengthDisplay` / `AlternateDimensionLengthDisplay : DimensionStyle.LengthDisplay` — per-instance style overrides mirroring the style fields.
- `Rhino.Geometry.AnnotationBase.GetBoundingBox(Transform xform) : BoundingBox` [override] — transformed bounds.
- `Rhino.Geometry.AnnotationBase.SetOverrideDimStyle(DimensionStyle overrideStyle) : bool` / `ClearPropertyOverrides() : bool` / `GetDimensionStyle(DimensionStyle parentDimStyle) : DimensionStyle` — attach, clear, and resolve the effective override style.

[RICH_TEXT_RUNS]:
- `Rhino.Geometry.AnnotationBase.SetRichText(string rtfText, DimensionStyle dimstyle) : void` — replaces content from an RTF string under a style.
- `Rhino.Geometry.AnnotationBase.RunReplace(string replaceString, int startRunIndex, int startRunPosition, int endRunIndex, int endRunPosition) : bool` — replaces a run-range span.
- `Rhino.Geometry.AnnotationBase.SetBold(bool setOn) : bool` / `SetItalic(bool) : bool` / `SetUnderline(bool) : bool` / `SetFacename(bool setOn, string facename) : bool` — whole-content run formatting (virtual).
- `Rhino.Geometry.AnnotationBase.IsAllBold() : bool` / `IsAllItalic() : bool` / `IsAllUnderlined() : bool` — uniform-formatting probes.
- `Rhino.Geometry.AnnotationBase.WrapText() : void` — wraps to `FormatWidth`; `TextHasRtfFormatting : bool` reports whether any run carries RTF.
- `Rhino.Geometry.AnnotationBase.PlainTextToRtf(string str) : string` [static] — wraps plain text as RTF.
- `Rhino.Geometry.AnnotationBase.FormatRtfString(string rtfIn, bool clearBold, bool setBold, bool clearItalic, bool setItalic, bool clearUnderline, bool setUnderline, bool clearFacename, bool setFacename, string facename) : string` [static] — applies a formatting delta to an RTF string.
- static `FirstCharProperties` is `[Obsolete]` — first-run evidence reads live `FirstCharFont`; the host run type is internal, so run manipulation reaches managed code only through `RunReplace` and `GetPlainTextWithRunMap`.
- `Rhino.Geometry.AnnotationBase.GetDimensionScale(RhinoDoc doc, DimensionStyle dimstyle, RhinoViewport vport) : double` [static] — the model-to-viewport annotation scale.

[TEXT_ENTITY]:
- `Rhino.Geometry.TextEntity.Create(string text, Plane plane, DimensionStyle style, bool wrapped, double rectWidth, double rotationRadians) : TextEntity` [static] — constructs plain text; `CreateWithRichText(string richTextString, Plane, DimensionStyle, bool wrapped, double rectWidth, double rotationRadians)` constructs from RTF.
- `Rhino.Geometry.TextEntity.GetTextTransform(double textscale, DimensionStyle dimstyle) : Transform` — the model-space text transform; `Transform(Transform transform, DimensionStyle style) : bool` applies one.
- `Rhino.Geometry.TextEntity.Explode() : Curve[]` — outlines every glyph as curves in one flat array.
- `Rhino.Geometry.TextEntity.CreateCurves(DimensionStyle dimstyle, bool allowOpen, double smallCapsScale = 1.0, double spacing = 0.0) : Curve[]` — glyph outlines; the `(DimensionStyle, bool allowOpen, bool makeSmallCaps, double smallCapsScale, double spacing)` overload adds small-caps control, and `CreateCurvesGrouped(...)` returns `List<Curve[]>` grouped per glyph.
- `Rhino.Geometry.TextEntity.CreateSurfaces(DimensionStyle dimstyle, double smallCapsScale = 1.0, double spacing = 0.0) : Brep[]` — capped glyph faces; `(DimensionStyle, bool makeSmallCaps, double smallCapsScale, double spacing)` and `CreateSurfacesGrouped(...) : List<Brep[]>` mirror the curve family.
- `Rhino.Geometry.TextEntity.CreatePolySurfaces(DimensionStyle dimstyle, double height, double smallCapsScale = 1.0, double spacing = 0.0) : Brep[]` — extruded-then-capped solids at `height`; small-caps and `CreatePolysurfacesGrouped(...) : List<Brep[]>` siblings follow.
- `Rhino.Geometry.TextEntity.CreateExtrusions(DimensionStyle dimstyle, double height, double smallCapsScale = 1.0, double spacing = 0.0) : Extrusion[]` — glyph extrusions at `height`; small-caps and `CreateExtrusionsGrouped(...) : List<Extrusion[]>` siblings follow.
- grouped-overload parameter transposition: `CreatePolysurfacesGrouped`/`CreateExtrusionsGrouped` order `(dimstyle, [makeSmallCaps,] smallCapsScale, height, spacing)` — `smallCapsScale` PRECEDES `height`, the opposite of the flat members — so call sites spell named arguments; the flat member spells `CreatePolySurfaces` (capital `S`) where the grouped sibling spells `CreatePolysurfacesGrouped`.
- `Rhino.Geometry.TextEntity.Justification : TextJustification` / `TextHorizontalAlignment : TextHorizontalAlignment` / `TextVerticalAlignment : TextVerticalAlignment` / `TextOrientation : TextOrientation` — per-entity placement overrides.

[LEADER]:
- `Rhino.Geometry.Leader.Create(string text, Plane plane, DimensionStyle dimstyle, Point3d[] points) : Leader` [static] — constructs a leader over a point run; `CreateWithRichText(string richText, Plane, DimensionStyle, Point3d[] points)` seeds from RTF.
- `Rhino.Geometry.Leader.Explode() : GeometryBase[]` — decomposes into curve and text geometry.
- `Rhino.Geometry.Leader.Curve : NurbsCurve` / `Points2D : Point2d[]` / `Points3D : Point3d[]` — the leader spline and its plane-relative and world points.
- `Rhino.Geometry.Leader.LeaderArrowType : DimensionStyle.ArrowType` / `LeaderArrowBlockId : Guid` / `LeaderArrowSize : double` / `LeaderCurveStyle : DimensionStyle.LeaderCurveStyle` / `LeaderContentAngleStyle : DimensionStyle.LeaderContentAngleStyle` / `LeaderHasLanding : bool` / `LeaderLandingLength : double` / `LeaderTextHorizontalAlignment : TextHorizontalAlignment` / `LeaderTextVerticalAlignment : TextVerticalAlignment` — leader-specific overrides.

[TEXT_FIELDS]:
- `Rhino.Runtime.TextFields.TryFormat(string text, RhinoDoc doc, out string result) : bool` [static] — evaluates a full field string to its resolved text; `TryParse(string text, RhinoDoc doc, out List<string> result) : bool` splits it into ordered field tokens. These two are the ONLY document-explicit members — every evaluator static below resolves against the AMBIENT document, so composed tokens evaluate through `TryFormat`, never through direct evaluator calls.
- `Rhino.Runtime.TextFields.Area(string id[, string unitSystem]) : double` / `Volume(string id[, string unitSystem[, string allowOpenObjects]]) : double` / `CurveLength(string id[, string unitSystem]) : double` — geometry measurements keyed by object id, unit-aware.
- `Rhino.Runtime.TextFields.Date([string dateFormat[, string languageId]]) : string` / `DateModified(...) : string` / `FileName([string options]) : string` / `Notes() : string` / `ModelUnits() : string` / `DocumentText(string key) : string` — document fields.
- `Rhino.Runtime.TextFields.PageNumber() : int` / `NumPages() : int` / `PageName([string id]) : string` / `PaperName() : string` / `PageWidth() : double` / `PageHeight() : double` / `DetailScale(string detailId, string scaleFormat) : string` / `LayoutUserText([string layoutId,] string key) : string` — layout fields; `PaperName` normalizes through a `StartsWith` ladder where the `Arch E` arm precedes `Arch E1`, so an Arch E1 paper answers `"Arch E"`.
- `Rhino.Runtime.TextFields.ObjectName([string id]) : string` / `ObjectLayer(string id) : string` / `LayerName(string layerId) : string` / `ObjectPageName(string id) : string` / `ObjectPageNumber(string id) : int` / `PointCoordinate(string pointId, string axis) : string` / `UserText(string id, string key[, string prompt[, string defaultValue]]) : string` — object fields.
- `Rhino.Runtime.TextFields.BlockName(string blockId) : string` / `BlockDescription(string definitionNameOrId) : string` / `BlockInstanceCount(string instanceDefinitionNameOrId) : int` / `BlockInsertionCoordinate(string blockId, string axis) : string` — block fields; `BlockInstanceName` is soft-deprecated onto `BlockName`, and `BlockAttributeText` and `GetInstanceAttributeFields` stay in `api-rhinocommon-blocks.md`.

[DIMENSION_OVERRIDES]:
- `Rhino.Geometry.Dimension` overrides ~52 `DimensionStyle` properties per instance plus dimension-only state: value (`NumericValue : double`, `PlainUserText : string`, `UseDefaultTextPoint : bool`, `TextPosition : Point2d`, `TextRotation : double`, `DetailMeasured : Guid`, `DistanceScale : double`), arrow (`ArrowheadType1`/`ArrowheadType2 : DimensionStyle.ArrowType`, `ArrowSize`, `ArrowBlockId1`/`ArrowBlockId2`, `ForceArrowPosition : ForceArrow`, `ArrowFit`), text placement (`TextLocation : DimensionStyle.TextLocation`, `TextOrientation`, `TextAngleType : DimensionStyle.LeaderContentAngleStyle`, `ForceTextPosition : ForceText`, `TextFit`), length/tolerance (`LengthFactor`, `LengthResolution`, `LengthRoundoff`, `Prefix`/`Suffix`, `ZeroSuppression`, `ToleranceFormat`/`ToleranceResolution`/`ToleranceUpperValue`/`ToleranceLowerValue`/`ToleranceHeightScale`), alt-unit twins (`AltUnitsDisplay`, `AltLengthFactor`, `AltLengthResolution`, `AltLengthRoundoff`, `AltPrefix`/`AltSuffix`, `AltZeroSuppression`, `AlternateBelowLine`, `AltToleranceResolution`), and line control (`ForceDimLine`, `ForceDimensionLineBetweenExtensionLines`, `DimensionLineExtension`, `ExtensionLineExtension`/`ExtensionLineOffset`, `SuppressExtension1`/`SuppressExtension2`, `FixedLengthExtensionOn`/`FixedExtensionLength`, `BaselineSpacing`, `CentermarkSize`/`CentermarkStyle`).
- `Rhino.Geometry.Dimension.GetTextTransform(ViewportInfo viewport, DimensionStyle style, double textScale, bool drawForward) : Transform` — the viewport-resolved text transform.
- `Rhino.Geometry.Dimension.UpdateDimensionText(DimensionStyle style, UnitSystem units) : void` — recomputes formatted text; the `(DimensionStyle, LengthUnit)` overload takes the finer unit.
- `Rhino.Geometry.Dimension.SetDimensionLengthDisplayWithZeroSuppressionReset(DimensionStyle.LengthDisplay ld) : void` / `SetAltDimensionLengthDisplayWithZeroSuppressionReset(...)` — sets length display and resets zero-suppression coherently.
- `Rhino.Geometry.Dimension.Explode() : GeometryBase[]` — decomposes into constituent curve, arc, and text geometry.

[DIMENSION_CONSTRUCT]:
- `Rhino.Geometry.LinearDimension.Create(AnnotationType dimtype, DimensionStyle dimStyle, Plane plane, Vector3d horizontal, Point3d defpoint1, Point3d defpoint2, Point3d dimlinepoint, double rotationInPlane) : LinearDimension` [static] — aligned or rotated linear dimension; `FromPoints(Point3d extensionLine1End, Point3d extensionLine2End, Point3d pointOnDimensionLine)` builds a default-style one.
- `Rhino.Geometry.LinearDimension.SetLocations(Point2d extensionLine1End, Point2d extensionLine2End, Point2d pointOnDimensionLine) : void` — repositions definition points; `Get3dPoints(out Point3d ×6) : bool` reads them back.
- `Rhino.Geometry.LinearDimension.GetDisplayLines(DimensionStyle style, double scale, out IEnumerable<Line> lines) : bool` — resolves drawable dimension lines; `GetTextRectangle(out Point3d[] corners) : bool` reads the text frame; `GetDistanceDisplayText(UnitSystem/LengthUnit, DimensionStyle) : string` formats the measured value.
- `Rhino.Geometry.AngularDimension.Create(DimensionStyle dimStyle, Plane plane, Vector3d horizontal, Point3d centerpoint, Point3d defpoint1, Point3d defpoint2, Point3d dimlinepoint) : AngularDimension` [static] — center-and-two-points construction; `Create(Guid styleId, Plane, Point3d extpoint1, Point3d extpoint2, Point3d dirpoint1, Point3d dirpoint2, Point3d dimlinepoint)`, `Create(DimensionStyle, Plane, Vector3d, Point3d ext1, Point3d ext2, Point3d dir1, Point3d dir2, Point3d dimline)`, and `Create(DimensionStyle, Line line1, Point3d pointOnLine1, Line line2, Point3d pointOnLine2, Point3d pointOnAngularDimensionArc, bool bSetExtensionPoints)` add the two-extension-point and line-pair forms.
- `Rhino.Geometry.AngularDimension.AdjustFromPoints(Plane, Point3d centerpoint, Point3d defpoint1, Point3d defpoint2, Point3d dimlinepoint) : bool` — the center-form re-fit; the extension-point overload mirrors the second `Create`; `Get3dPoints(out Point3d ×7) : bool`, `GetDisplayLines(DimensionStyle, double scale, out Line[] lines, out Arc[] arcs) : bool`, `GetTextRectangle(out Point3d[] corners) : bool`, `GetAngleDisplayText(DimensionStyle) : string`.
- `Rhino.Geometry.RadialDimension.Create(DimensionStyle dimStyle, AnnotationType dimtype, Plane plane, Point3d centerpoint, Point3d radiuspoint, Point3d dimlinepoint) : RadialDimension` [static] — `Radius` or `Diameter` per `dimtype`; `AdjustFromPoints(Plane, Point3d centerpoint, Point3d radiuspoint, Point3d dimlinepoint, double rotationInPlane) : bool`, `Get3dPoints(out Point3d ×4) : bool`, `GetDisplayLines(...)`, `GetTextRectangle(out Point3d[] corners) : bool`, `GetDistanceDisplayText(...)`.
- `Rhino.Geometry.OrdinateDimension.Create(DimensionStyle dimStyle, Plane plane, MeasuredDirection direction, Point3d basepoint, Point3d defpoint, Point3d leaderpoint, double kinkoffset1, double kinkoffset2) : OrdinateDimension` [static] — x/y ordinate with two kink offsets; `AdjustFromPoints(...) : bool`, `Get3dPoints(out Point3d ×5) : bool`, `GetDisplayLines(...)`, `GetTextRectangle(out Point3d[] corners) : bool`, `GetDistanceDisplayText(...)`.
- `Rhino.Geometry.Centermark.Create(DimensionStyle dimStyle, Plane plane, Point3d centerPoint, double radius) : Centermark` [static] — from a center and radius; `Create(DimensionStyle, Plane, Curve curve, double curveParameter)` from a curve point; `AdjustFromPoints(Plane, Point3d centerPoint) : bool`; `Radius : double`; `Centermark` carries no `Get3dPoints`, `GetDisplayLines`, or `GetTextRectangle`.
- per-kind instance accessors: `LinearDimension.Aligned : bool` (aligned-vs-rotated discriminant), `LinearDimension.DistanceBetweenArrowTips : double`, the plane-relative `Point2d` definition accessors (`ExtensionLine1End`/`ExtensionLine2End`/`Arrowhead1End`/`Arrowhead2End`/`DimensionLinePoint` on linear; `CenterPoint`/`DefPoint1`/`DefPoint2`/`DimlinePoint`/`ArrowPoint1`/`ArrowPoint2` on angular; `CenterPoint`/`RadiusPoint`/`DimlinePoint`/`KneePoint` on radial; `DefPoint`/`LeaderPoint`/`KinkPoint1`/`KinkPoint2` on ordinate), and the constructor forms `LinearDimension(Plane, Point2d, Point2d, Point2d)` and `AngularDimension(Arc arc, double offset)`.
- `Rhino.Geometry.AngularDimension.AngleFormat : DimensionStyle.AngleDisplayFormat` / `AngleResolution : int` / `AngleRoundoff : double` / `AngleZeroSuppression : DimensionStyle.ZeroSuppression` — the angular per-instance format quartet.
- `Rhino.Geometry.RadialDimension.IsDiameterDimension : bool` / `LeaderTextHorizontalAlignment : TextHorizontalAlignment` / `LeaderArrowType : DimensionStyle.ArrowType` / `LeaderArrowSize : double` / `LeaderArrowBlockId : Guid` / `LeaderCurveStyle : DimensionStyle.LeaderCurveStyle` — the radial leader quintet and kind discriminant.
- `Rhino.Geometry.OrdinateDimension.Direction : MeasuredDirection` / `KinkOffset1 : double` / `KinkOffset2 : double` — the ordinate axis and kink state.

[DOC_OBJECTS]:
- `Rhino.DocObjects.AnnotationObjectBase.AnnotationGeometry : AnnotationBase` — typed accessor over the document annotation geometry.
- `Rhino.DocObjects.AnnotationObjectBase.DisplayText : string` — the resolved display text; `HasMeasurableTextFields : bool` — whether the annotation carries measurable text fields (`Area`/`Volume`/`CurveLength`).
- `Rhino.DocObjects.DimensionObject : AnnotationObjectBase` — the per-kind dimension objects' base, carrying `DimensionStyle : DimensionStyle`.
- `Rhino.DocObjects.LinearDimensionObject.LinearDimensionGeometry : LinearDimension` / `AngularDimensionObject.AngularDimensionGeometry` / `RadialDimensionObject.RadialDimensionGeometry` / `OrdinateDimensionObject.OrdinateDimensionGeometry` / `CentermarkObject.CentermarkGeometry` / `TextObject.TextGeometry : TextEntity` / `LeaderObject.LeaderGeometry : Leader` — each casts `RhinoObject.Geometry` to its kind.

## [04]-[IMPLEMENTATION_LAW]

[ANNOTATION_TOPOLOGY]:
- `DimensionStyle` is the shared drafting config; a `Field` is the atom the override algebra addresses. Parent/child inheritance is per-field: a child holds a value only for fields marked overridden via `SetFieldOverride`, every other field inheriting the parent through `ParentId`. Per-object customization mints a child style, marks the differing fields, and points the annotation's `DimensionStyleId` at it — `SetOverrideDimStyle` on `AnnotationBase` attaches it and demands a nil-id style outside the table with its override fields marked, while `GetDimensionStyle(parent)` resolves the effective merge and construction binds the base through `ParentDimensionStyle`.
- annotation text carries two coupled forms — `RichText` (RTF source, also the field-token carrier) and `PlainText` (flattened) — edited through run operations (`RunReplace`, `SetBold`/`SetItalic`/`SetUnderline`/`SetFacename`) and re-flowed by `WrapText`; field tokens resolve through `TextFields.TryFormat` against the document, never string-concatenated.
- `Dimension` is `AnnotationBase` plus ~52 per-instance style overrides and the measured value; the six kinds differ only in construction geometry and display-line resolution (`GetDisplayLines`, `Get3dPoints`, `GetTextRectangle`), so one override surface and one doc-object accessor pattern serve all.
- text-to-geometry outlining is one parameterized family over four egress shapes (`CreateCurves`/`CreateSurfaces`/`CreatePolySurfaces`/`CreateExtrusions`), each with a flat and a per-glyph `...Grouped` sibling and a small-caps overload; `height` selects extruded solids over planar faces.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): table `bool`/`int` outcomes and `Modify`'s `ModifyType` project to `Fin<Unit>`/`Fin<int>`; nullable `Find*` reads lift to `Option<DimensionStyle>`; `Get3dPoints`/`GetDisplayLines`/`GetTextRectangle` `out` results fold into `Fin<A>` carrying the geometry only on `true`; roster and outline arrays land as `Seq<A>`; `TextFields.TryFormat`/`TryParse` become `Fin<string>`/`Fin<Seq<string>>`.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): every `DimensionStyle` nested enum and `AnnotationType`/`ModifyType`/`MeasuredDirection`/`ForceArrow`/`ForceText` maps at the edge to a `[SmartEnum]` owner, a style/annotation `Guid` to a `[ValueObject<Guid>]`, and `Field` to a `[SmartEnum]` whose cases own their config-property projection so the override algebra dispatches over closed cases.
- `Rasm` kernel: planes, base points, kink offsets, text transforms, unit scaling, and `DimensionScaleValue` compose the kernel numeric and unit owners; `Color`-valued mask and tolerance fields compose the kernel color rail, never a host channel average.
- `api-rhinocommon-drafting-resources.md`: a style's `Font`, arrow-block ids, and any linetype/hatch a section references resolve through that catalog's owners; `api-rhinocommon-display.md` draws `TextEntity`/`AnnotationBase`/`Hatch` through the pipeline.

[LOCAL_ADMISSION]:
- a style enters through `DimStyleTable.Add`/`Modify`; per-object overrides enter through `AnnotationBase.SetOverrideDimStyle` after the differing fields are marked, never by mutating a shared style in place. `DimStyleTable.Modify(style, annotation)` is the sole reverse-projection from a live annotation back to a style, its `ModifyType` outcome inspected before the write is treated as durable.
- live `DimensionStyle`, `AnnotationBase`, `Dimension`, and `*Object` values stay inside the document grant; downstream code receives bounded owners, detached geometry (outline curves/breps/extrusions, display lines), resolved formula strings, projected receipts, or explicitly owned bitmap leases.

[RAIL_LAW]:
- Surface: `Rhino.DocObjects` + `Rhino.DocObjects.Tables` + `Rhino.Geometry` + `Rhino.Runtime.TextFields` annotation reads
- Owns: drafting-style config and field-override inheritance, the style table transaction, annotation RTF run editing and property overrides, text/leader construction and text outlining, the six-kind dimension family, and text-field formula evaluation.
- Accept: style authoring and field-override algebra, annotation and dimension construction and display-line resolution, text outlining, and formula evaluation projected onto `Fin`/`Option`/`Seq` rails with host enums mapped to bounded owners at the edge.
- Reject: shared-style mutation standing in for a per-object override, assumed inheritance state, exception-style table outcomes, string-concatenated field text where `TextFields.TryFormat` evaluates, and live document-bound annotation objects crossing the session boundary.
