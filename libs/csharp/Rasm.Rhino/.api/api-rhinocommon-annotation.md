# [RASM_RHINO_API_RHINOCOMMON_ANNOTATION]

`RhinoCommon` owns the annotation-model boundary: `DimensionStyle` drafting config with its per-`Field` override inheritance and the `DimStyleTable` transaction, `AnnotationBase` RTF run editing and property-override projection, `TextEntity`/`Leader` construction and text-to-geometry outlining, the six-kind `Dimension` family over one per-instance override surface, and `TextFields` formula evaluation feeding the rich-text tokens. Every live document-bound annotation resolves inside the owning session before a detached value crosses the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespace: `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.Geometry`, `Rhino.Runtime`
- rail: annotation-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: style config and its table

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :--------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `DimensionStyle` | class          | drafting config, `Field` override algebra, parent/child chain |
|  [02]   | `DimStyleTable`  | document table | style identity, authoring, current selection, reverse-project |

[PUBLIC_TYPE_SCOPE]: annotation geometry and text

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :--------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `AnnotationBase` | geometry base  | RTF run model, property-override projection, mask/plane |
|  [02]   | `TextEntity`     | geometry       | text construction and curve/surface/extrusion outlining |
|  [03]   | `Leader`         | geometry       | polyline/spline leader construction and explode         |
|  [04]   | `TextFields`     | static surface | field-formula evaluators feeding rich-text tokens       |

[PUBLIC_TYPE_SCOPE]: dimension family and doc-object projections

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Dimension`            | geometry base | per-instance override of nearly the whole style               |
|  [02]   | `LinearDimension`      | geometry      | aligned/rotated linear dimension and display lines            |
|  [03]   | `AngularDimension`     | geometry      | angle-between from lines, points, or arc                      |
|  [04]   | `RadialDimension`      | geometry      | radius/diameter dimension and display lines                   |
|  [05]   | `OrdinateDimension`    | geometry      | x/y ordinate with kink-offset leader geometry                 |
|  [06]   | `Centermark`           | geometry      | radial centermark cross-and-lines                             |
|  [07]   | `AnnotationObjectBase` | doc object    | `AnnotationGeometry` accessor over a doc annotation           |
|  [08]   | `*DimensionObject`     | doc object    | typed geometry accessor per kind, `TextObject`/`LeaderObject` |

[ENUM_ROSTERS]:
- `DimensionStyle.ArrowType` — `None` `UserBlock` `SolidTriangle` `Dot` `Tick` `ShortTriangle` `OpenArrow` `Rectangle` `LongTriangle` `LongerTriangle` `SolidDatumTriangle` (0..10).
- `DimensionStyle.ClippingArrowType` — `None` `Triangle` `OffsetTriangle` `Arrow` `OffsetArrow` `OpenArrow` `Rectangle` `Ribbon` `Line` (0..8).
- `DimensionStyle.MaskType : byte` — `BackgroundColor` `MaskColor`.
- `DimensionStyle.MaskFrame : byte` — `NoFrame` `RectFrame` `CapsuleFrame` `CircleFrame` `SquareFrame` `DiamondFrame` `TriangleFrame` `HexagonFrame` `HexagonCapsuleFrame` `RoundRectFrame`.
- `DimensionStyle.LengthDisplay` — `ModelUnits=0` `InchesFractional=1` `FeetAndInches=2` `Millmeters=3` `Centimeters=4` `Meters=5` `Kilometers=6` `InchesDecimal=7` `FeetDecimal=8` `Miles=9`; host declares them out of value order, so a `[SmartEnum]` keys on the explicit value, never declaration position (`Millmeters` is the host spelling).
- `DimensionStyle.ToleranceDisplayFormat : byte` — `None` `Symmetrical` `Deviation` `Limits`.
- `DimensionStyle.LeaderContentAngleStyle : byte` — `Horizontal` `Aligned` `Rotated`.
- `DimensionStyle.LeaderCurveStyle : byte` — `None` `Polyline` `Spline`.
- `DimensionStyle.AngleDisplayFormat : byte` — `DecimalDegrees` `DegMinSec` `Radians` `Grads`.
- `DimensionStyle.TextLocation : byte` — `AboveDimLine` `InDimLine` `BelowDimLine`.
- `DimensionStyle.ZeroSuppression : byte` — `None=0` `SuppressLeading=1` `SuppressTrailing=2` `SuppressLeadingAndTrailing=3` `SuppressZeroFeet=4` `SuppressZeroInches=8` `SuppressZeroFeetAndZeroInches=12` (bit flags).
- `DimensionStyle.StackDisplayFormat : byte` — `None` `StackHorizontal` `StackDiagonal`.
- `DimensionStyle.CenterMarkStyle : byte` — `None` `Mark` `MarkAndLines`.
- `DimensionStyle.ArrowFit : byte` — `Auto` `ArrowsInside` `ArrowsOutside`.
- `DimensionStyle.TextFit : byte` — `Auto` `TextInside` `TextRight` `TextLeft` `TextHintRight` `TextHintLeft`.
- `DimensionStyle.Field` — field-identifier vocabulary keyed by the override algebra: 117 members `Unset=0` through `Count=120`, non-contiguous (values `19`, `74`, `107`, `108` skipped). Most name one config property (`Arrowsize=5`, `TextHeight=9`, `LengthFactor=16`, `DimensionScale=59`, `ArrowType1=78`, `ClippingArrowSize=119`, peers); the color/plot group (`ExtLineColorSource=28` through `DimLinePlotWeight_mm=47`), `MaskFlags=92`, `SignedOrdinate=97`, and `UnitSystem=98` carry no CLR property and exist only as override-marking vocabulary; `Name=1`/`Index=2` cannot inherit from a parent.
- `Dimension.ForceArrow` — `Auto` `Inside` `Outside`.
- `OrdinateDimension.MeasuredDirection : byte` — `Unset` `Xaxis` `Yaxis`.
- `Rhino.Geometry.AnnotationType : byte` — `Unset` `Aligned` `Angular` `Diameter` `Radius` `Rotated` `Ordinate` `ArcLen` `CenterMark` `Text` `Leader` `Angular3pt` (0..11); the discriminant `LinearDimension.Create` and `RadialDimension.Create` accept (`AlignedDimension` has no type — aligned is `LinearDimension` + `AnnotationType.Aligned`).
- `Rhino.DocObjects.Tables.ModifyType` — `Modify` `Override` `NotSaved`; `DimStyleTable.Modify(DimensionStyle, AnnotationBase)` outcome.
- Edge-mapped external owners: `Rhino.DocObjects.TextOrientation`, `TextVerticalAlignment`, `TextHorizontalAlignment`, `TextJustification` carry text placement `DimensionStyle`/`AnnotationBase`/`Dimension` reference.

## [03]-[ENTRYPOINTS]

[STYLE_IDENTITY]: `DimStyleTable` reads.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Current` / `CurrentIndex` / `CurrentId` | property | current-style snapshot and handles        |
|  [02]   | `this[int]`                              | property | indexer over the component index          |
|  [03]   | `FindName(string)`                       | instance | resolve by name                           |
|  [04]   | `Find(string, bool)`                     | instance | resolve by name or `Guid`, deleted-switch |
|  [05]   | `FindIndex(int)`                         | instance | resolve by runtime index                  |
|  [06]   | `FindRoot(Guid, bool)`                   | instance | walk parents to inheritance root          |
|  [07]   | `BuiltInStyles`                          | property | built-in style roster                     |
|  [08]   | `GetUnusedStyleName(string)`             | instance | mint an unused style name                 |

[STYLE_AUTHORING]: `DimStyleTable` writes.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Add(string)`                                          | instance | add style; overloads seed config or reference |
|  [02]   | `Modify(DimensionStyle, int, bool)`                    | instance | write config by index; `Guid` overload by id  |
|  [03]   | `Modify(DimensionStyle, AnnotationBase) -> ModifyType` | instance | reverse-project overrides onto a style        |
|  [04]   | `SetCurrent(int, bool)`                                | instance | set the active style                          |
|  [05]   | `Delete(int, bool)`                                    | instance | delete a style; `PurgeUnused()` reclaims      |

[STYLE_FIELD_ALGEBRA]: `DimensionStyle` override and identity.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `SetFieldOverride(Field)`                                | instance | mark one field overridden          |
|  [02]   | `ClearFieldOverride(Field)` / `ClearAllFieldOverrides()` | instance | clear one or all overrides         |
|  [03]   | `IsFieldOverriden(Field)` / `HasFieldOverrides`          | instance | test one or any override           |
|  [04]   | `ParentId` / `IsChild` / `IsChildOf(Guid)`               | property | parent id, `Guid.Empty` means none |
|  [05]   | `Duplicate(string, Guid, Guid)`                          | instance | deep copy re-keyed in one call     |
|  [06]   | `CopyFrom(DimensionStyle)`                               | instance | copy all except name/id/index      |
|  [07]   | `ScaleLengthValues(double)`                              | instance | scale all length fields            |
|  [08]   | `DimensionLengthDisplayUnit(uint) -> UnitSystem`         | instance | rendered unit under a model        |
|  [09]   | `CreatePreviewBitmap(int, int, bool) -> Bitmap`          | instance | render a style swatch              |
|  [10]   | `SetUserString(string, string)`                          | instance | user-string bag (Get/Delete/All)   |

[STYLE_CONFIG]: `DimensionStyle` exposes ~96 get/set config properties, each backed by a `Field` case whose setter marks the field overridden; the color/plot-source `Field` group carries no property counterpart.

[ARROW]: `ArrowType1` `ArrowType2` `LeaderArrowType` `ArrowLength` `LeaderArrowLength` `ClippingArrowLength` `ArrowBlockId1` `ArrowBlockId2` `LeaderArrowBlockId` `ClippingArrowType1` `ClippingArrowType2` `FitArrow` `SuppressArrow1` `SuppressArrow2`
[TEXT]: `TextHeight` `TextGap` `TextRotation` `Font` `TextVerticalAlignment` `TextHorizontalAlignment` `TextOrientation` `LeaderTextOrientation` `DimTextOrientation` `DimRadialTextOrientation` `DimTextLocation` `DimRadialTextLocation` `DimTextAngleType` `DimRadialTextAngleType` `FitText` `UseKerning` `TextUnderlined` `LineSpaceScale` `DrawForward`
[LENGTH]: `LengthFactor` `AlternateLengthFactor` `LengthResolution` `AlternateLengthResolution` `AngleResolution` `DimensionLengthDisplay` `AlternateDimensionLengthDisplay` `AngleFormat` `Roundoff` `AlternateRoundoff` `AngularRoundoff` `Prefix` `Suffix` `AlternatePrefix` `AlternateSuffix` `DecimalSeparator` `StackFractionFormat` `StackHeightScale` `AlternateUnitsDisplay` `AlternateBelowLine`
[TOLERANCE]: `ToleranceFormat` `ToleranceResolution` `AlternateToleranceResolution` `ToleranceHeightScale` `ToleranceUpperValue` `ToleranceLowerValue` `ZeroSuppress` `AlternateZeroSuppress` `AngleZeroSuppress`
[MASK]: `DrawTextMask` `MaskColor` `MaskColorSource` `MaskFrameType` `MaskOffset`
[LAYOUT]: `BaselineSpacing` `DimensionScale` `DimensionScaleValue` `ScaleLeftLengthMillimeters` `ScaleRightLengthMillimeters` `CentermarkSize` `CenterMarkType` `ExtensionLineExtension` `ExtensionLineOffset` `DimensionLineExtension` `SuppressExtension1` `SuppressExtension2` `FixedExtensionOn` `FixedExtensionLength` `TextMoveLeader` `ArcLengthSymbol` `ForceDimensionLineBetweenExtensionLines` `UserStringCount`
[LEADER]: `LeaderHasLanding` `LeaderLandingLength` `LeaderContentAngleType` `LeaderCurveType` `LeaderTextVerticalAlignment` `LeaderTextHorizontalAlignment` `LeaderTextRotationRadians` `LeaderTextRotationDegrees`
- `DimensionStyle.ToleranceZeroSuppress` is an inert stub — constant `ZeroSuppression.None` getter, empty setter, no `Field` pairing.
- `Field.LeaderContentAngle` backs both `LeaderContentAngleType` (`GetInt`/`SetInt`) and `LeaderTextRotationRadians`/`Degrees` (`GetDouble`/`SetDouble`); `Field.LeaderContentAngleStyle` binds no accessor.

[ANNOTATION_TEXT]: `AnnotationBase` style binding and text.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `RichText` / `PlainText`                                         | property | RTF source and flattened text       |
|  [02]   | `PlainTextWithFields` / `GetPlainTextWithRunMap(int[])`          | instance | text keeping tokens, with a run map |
|  [03]   | `DimensionStyleId` / `DimensionStyle` / `ParentDimensionStyle`   | property | bound style, effective, parent      |
|  [04]   | `Font` / `FirstCharFont`                                         | property | annotation and first-run font       |
|  [05]   | `HasPropertyOverrides` / `IsPropertyOverridden(Field)`           | property | per-instance override presence      |
|  [06]   | `AnnotationType`                                                 | property | kind discriminant (virtual)         |
|  [07]   | `GetBoundingBox(Transform) -> BoundingBox`                       | instance | transformed bounds (override)       |
|  [08]   | `SetOverrideDimStyle(DimensionStyle)`                            | instance | attach an override style            |
|  [09]   | `ClearPropertyOverrides()` / `GetDimensionStyle(DimensionStyle)` | instance | clear or resolve the override style |

[FRAME]: `Plane` `TextHeight` `TextRotationRadians` `TextRotationDegrees` `TextIsWrapped` `FormatWidth` `TextModelWidth`
[MASK_STATE]: `MaskEnabled` `MaskColor` `MaskColorSource` `MaskFrame` `MaskOffset` `MaskUsesViewportColor` `DrawTextFrame`
[INSTANCE_OVERRIDE]: `DecimalSeparator` `UseKerning` `LineSpaceScale` `DimensionScale` `DrawForward` `DimensionLengthDisplay` `AlternateDimensionLengthDisplay`

[RICH_TEXT_RUNS]: `AnnotationBase` run editing.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `SetRichText(string, DimensionStyle)`                                        | instance | replace content from RTF               |
|  [02]   | `RunReplace(string, int, int, int, int)`                                     | instance | replace a run-range span               |
|  [03]   | `SetBold(bool)` / `SetItalic` / `SetUnderline` / `SetFacename(bool, string)` | instance | whole-content run formatting (virtual) |
|  [04]   | `IsAllBold()` / `IsAllItalic()` / `IsAllUnderlined()`                        | instance | uniform-formatting probes              |
|  [05]   | `WrapText()`                                                                 | instance | wrap to `FormatWidth`                  |
|  [06]   | `PlainTextToRtf(string) -> string`                                           | static   | wrap plain text as RTF                 |
|  [07]   | `FormatRtfString(string, bool ×8, string) -> string`                         | static   | formatting delta on an RTF string      |
|  [08]   | `GetDimensionScale(RhinoDoc, DimensionStyle, RhinoViewport) -> double`       | static   | model-to-viewport scale                |

- Host run type is internal, so run manipulation reaches managed code only through `RunReplace` and `GetPlainTextWithRunMap`; `TextHasRtfFormatting` reports whether any run carries RTF.

[TEXT_ENTITY]: `TextEntity` construction and outlining.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `Create(string, Plane, DimensionStyle, bool, double, double)`             | static   | plain text; RTF via `CreateWithRichText` |
|  [02]   | `GetTextTransform(double, DimensionStyle) -> Transform`                   | instance | model-space text transform               |
|  [03]   | `Explode() -> Curve[]`                                                    | instance | outline glyphs as curves                 |
|  [04]   | `CreateCurves(DimensionStyle, bool, double, double) -> Curve[]`           | instance | glyph outline curves                     |
|  [05]   | `CreateSurfaces(DimensionStyle, double, double) -> Brep[]`                | instance | capped glyph faces                       |
|  [06]   | `CreatePolySurfaces(DimensionStyle, double, double, double) -> Brep[]`    | instance | extruded-capped solids at `height`       |
|  [07]   | `CreateExtrusions(DimensionStyle, double, double, double) -> Extrusion[]` | instance | glyph extrusions at `height`             |

[PLACEMENT]: `Justification` `TextHorizontalAlignment` `TextVerticalAlignment` `TextOrientation`
- Each outline family (`Curves`/`Surfaces`/`PolySurfaces`/`Extrusions`) carries a per-glyph `...Grouped` sibling returning `List<T[]>` and a small-caps overload. `CreatePolysurfacesGrouped`/`CreateExtrusionsGrouped` order `(dimstyle, [makeSmallCaps,] smallCapsScale, height, spacing)` — `smallCapsScale` precedes `height`, opposite the flat members — so call sites spell named arguments; the flat member spells `CreatePolySurfaces` (capital `S`), the grouped sibling `CreatePolysurfacesGrouped`.

[LEADER]: `Leader` construction.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Create(string, Plane, DimensionStyle, Point3d[]) -> Leader` | static   | over a point run; RTF via `CreateWithRichText` |
|  [02]   | `Explode() -> GeometryBase[]`                                | instance | decompose to curve and text                    |
|  [03]   | `Curve` / `Points2D` / `Points3D`                            | property | spline, plane-relative, world points           |

[OVERRIDES]: `LeaderArrowType` `LeaderArrowBlockId` `LeaderArrowSize` `LeaderCurveStyle` `LeaderContentAngleStyle` `LeaderHasLanding` `LeaderLandingLength` `LeaderTextHorizontalAlignment` `LeaderTextVerticalAlignment`

[TEXT_FIELDS]: `TextFields` evaluators.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `TryFormat(string, RhinoDoc, out string)`                                   | static  | evaluate a field string; `TryParse` splits |
|  [02]   | `Area` / `Volume` / `CurveLength`                                           | static  | geometry measures by id, unit-aware        |
|  [03]   | `Date` / `FileName` / `Notes` / `ModelUnits` / `DocumentText`               | static  | document fields                            |
|  [04]   | `PageNumber` / `PageName` / `PaperName` / `DetailScale` / `LayoutUserText`  | static  | layout fields                              |
|  [05]   | `ObjectName` / `ObjectLayer` / `LayerName` / `PointCoordinate` / `UserText` | static  | object fields                              |
|  [06]   | `BlockName` / `BlockDescription` / `BlockInstanceCount`                     | static  | block fields                               |

- `TryFormat`/`TryParse` are the only document-explicit members; every evaluator resolves against the ambient document, so composed tokens evaluate through `TryFormat`, never direct evaluator calls.
- `PaperName` normalizes through a `StartsWith` ladder where the `Arch E` arm precedes `Arch E1`, so an Arch E1 paper answers `"Arch E"`.

[DIMENSION_OVERRIDES]: `Dimension` overrides ~52 `DimensionStyle` properties per instance with dimension-only state.

[VALUE]: `NumericValue` `PlainUserText` `UseDefaultTextPoint` `TextPosition` `TextRotation` `DetailMeasured` `DistanceScale`
[ARROW]: `ArrowheadType1` `ArrowheadType2` `ArrowSize` `ArrowBlockId1` `ArrowBlockId2` `ForceArrowPosition` `ArrowFit`
[TEXT]: `TextLocation` `TextOrientation` `TextAngleType` `TextFit`
[LENGTH_TOLERANCE]: `LengthFactor` `LengthResolution` `LengthRoundoff` `Prefix` `Suffix` `ZeroSuppression` `ToleranceFormat` `ToleranceResolution` `ToleranceUpperValue` `ToleranceLowerValue` `ToleranceHeightScale`
[ALT_UNITS]: `AltUnitsDisplay` `AltLengthFactor` `AltLengthResolution` `AltLengthRoundoff` `AltPrefix` `AltSuffix` `AltZeroSuppression` `AlternateBelowLine` `AltToleranceResolution`
[LINE]: `ForceDimLine` `ForceDimensionLineBetweenExtensionLines` `DimensionLineExtension` `ExtensionLineExtension` `ExtensionLineOffset` `SuppressExtension1` `SuppressExtension2` `FixedLengthExtensionOn` `FixedExtensionLength` `BaselineSpacing` `CentermarkSize` `CentermarkStyle`

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `GetTextTransform(ViewportInfo, DimensionStyle, double, bool) -> Transform` | instance | viewport-resolved text transform      |
|  [02]   | `UpdateDimensionText(DimensionStyle, UnitSystem)`                           | instance | recompute text; `LengthUnit` overload |
|  [03]   | `SetDimensionLengthDisplayWithZeroSuppressionReset(LengthDisplay)`          | instance | length display + zero-suppress reset  |
|  [04]   | `Explode() -> GeometryBase[]`                                               | instance | decompose to curve, arc, text         |

- `SetDimensionLengthDisplayWithZeroSuppressionReset` carries a `SetAlt...` twin for the alternate-unit display.

[DIMENSION_CONSTRUCT]:

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `LinearDimension.Create(AnnotationType, DimensionStyle, Plane, Vector3d, Point3d×3, double)` | static   | aligned/rotated linear       |
|  [02]   | `LinearDimension.SetLocations(Point2d×3)`                                                    | instance | reposition definition points |
|  [03]   | `LinearDimension.GetDisplayLines(DimensionStyle, double, out IEnumerable<Line>)`             | instance | drawable dimension lines     |
|  [04]   | `AngularDimension.Create(DimensionStyle, Plane, Vector3d, Point3d×4)`                        | static   | center-and-two-points        |
|  [05]   | `AngularDimension.AdjustFromPoints(Plane, Point3d×4) -> bool`                                | instance | center-form re-fit           |
|  [06]   | `RadialDimension.Create(DimensionStyle, AnnotationType, Plane, Point3d×3)`                   | static   | radius or diameter per type  |
|  [07]   | `OrdinateDimension.Create(DimensionStyle, Plane, MeasuredDirection, Point3d×3, double×2)`    | static   | x/y ordinate + kink offsets  |
|  [08]   | `Centermark.Create(DimensionStyle, Plane, Point3d, double)`                                  | static   | from center and radius       |

- `LinearDimension.FromPoints` builds a default-style linear; `AngularDimension.Create` adds id, two-extension-point, and line-pair overloads; `Centermark.Create(…, Curve, double)` builds from a curve point.
- `RadialDimension`/`OrdinateDimension` mirror the linear `AdjustFromPoints`/`Get3dPoints`/`GetDisplayLines`/`GetTextRectangle`/`GetDistanceDisplayText` set; `Centermark` carries only `AdjustFromPoints` and `Radius`, no display-line or point readers.
- Per-kind format accessors: `LinearDimension.Aligned` (aligned-vs-rotated discriminant) and `DistanceBetweenArrowTips`; `AngularDimension.AngleFormat`/`AngleResolution`/`AngleRoundoff`/`AngleZeroSuppression`; `RadialDimension.IsDiameterDimension` and the leader quintet; `OrdinateDimension.Direction`/`KinkOffset1`/`KinkOffset2`; each kind carries plane-relative `Point2d` definition-point accessors.

[DOC_OBJECTS]:

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `AnnotationObjectBase.AnnotationGeometry -> AnnotationBase`    | property | accessor over the doc annotation    |
|  [02]   | `AnnotationObjectBase.DisplayText` / `HasMeasurableTextFields` | property | display text, measurable-field flag |
|  [03]   | `DimensionObject.DimensionStyle`                               | property | per-kind dimension base style       |

[GEOMETRY_ACCESSORS]: `LinearDimensionObject.LinearDimensionGeometry` `AngularDimensionObject.AngularDimensionGeometry` `RadialDimensionObject.RadialDimensionGeometry` `OrdinateDimensionObject.OrdinateDimensionGeometry` `CentermarkObject.CentermarkGeometry` `TextObject.TextGeometry` `LeaderObject.LeaderGeometry` — each casts `RhinoObject.Geometry` to its kind.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DimensionStyle` is the shared drafting config and a `Field` is the atom the override algebra addresses; inheritance is per-field, a child holding a value only for fields marked via `SetFieldOverride` and inheriting every other through `ParentId`. Per-object customization mints a child style, marks the differing fields, and points `DimensionStyleId` at it.
- `SetOverrideDimStyle` demands an override style carrying `Guid.Empty` id, living outside `DimStyleTable`, and override-marked — mint via `Duplicate(newName, Guid.Empty, DimensionStyleId)`; a table style or a plain `Duplicate()` copy keeps the source id and the host rejects it. `GetDimensionStyle(parent)` resolves the effective merge, construction binds the base through `ParentDimensionStyle`, and writing any config property with `setOverride: true` auto-marks its field.
- Annotation text carries two coupled forms — `RichText` (RTF source, field-token carrier) and `PlainText` (flattened) — edited through run operations and re-flowed by `WrapText`; field tokens resolve through `TextFields.TryFormat` against the document, never string-concatenated.
- `Dimension` extends `AnnotationBase` with ~52 per-instance overrides and the measured value; the six kinds differ only in construction geometry and display-line resolution, so one override surface and one doc-object accessor pattern serve all. Text-to-geometry outlining is one parameterized family over four egress shapes, each with a flat and a per-glyph `...Grouped` sibling; `height` selects extruded solids over planar faces.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): table `bool`/`int` and `Modify`'s `ModifyType` project to `Fin<Unit>`/`Fin<int>`; nullable `Find*` reads lift to `Option<DimensionStyle>`; `Get3dPoints`/`GetDisplayLines`/`GetTextRectangle` `out` results fold into `Fin<A>` carrying geometry only on `true`; roster and outline arrays land as `Seq<A>`; `TextFields.TryFormat`/`TryParse` become `Fin<string>`/`Fin<Seq<string>>`.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): every `DimensionStyle` nested enum and `AnnotationType`/`ModifyType`/`MeasuredDirection`/`ForceArrow` maps at the edge to a `[SmartEnum]`, a style/annotation `Guid` to a `[ValueObject<Guid>]`, and `Field` to a `[SmartEnum]` whose cases own their config-property projection so the override algebra dispatches over closed cases.
- `Rasm` kernel: planes, base points, kink offsets, text transforms, unit scaling, and `DimensionScaleValue` compose the kernel numeric and unit owners; `Color`-valued mask and tolerance fields compose the kernel color rail, never a host channel average.
- `api-rhinocommon-drafting-resources.md`: a style's `Font`, arrow-block ids, and any linetype/hatch a section references resolve through that catalog; `api-rhinocommon-display.md` draws `TextEntity`/`AnnotationBase`/`Hatch` through the pipeline.

[LOCAL_ADMISSION]:
- A style enters through `DimStyleTable.Add`/`Modify`; per-object overrides enter through `AnnotationBase.SetOverrideDimStyle` after the differing fields are marked, never by mutating a shared style in place. `DimStyleTable.Modify(style, annotation)` is the sole reverse-projection from a live annotation to a style, its `ModifyType` inspected before the write is treated as durable.
- Live `DimensionStyle`, `AnnotationBase`, `Dimension`, and `*Object` values stay inside the document grant; downstream code receives bounded owners, detached geometry, resolved formula strings, projected receipts, or explicitly owned bitmap leases.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: drafting-style config and field-override inheritance, the style-table transaction, annotation RTF run editing and property overrides, text/leader construction and text outlining, the six-kind dimension family, and text-field formula evaluation.
- Accept: style authoring and field-override algebra, annotation and dimension construction and display-line resolution, text outlining, and formula evaluation projected onto `Fin`/`Option`/`Seq` rails with host enums mapped to bounded owners at the edge.
- Reject: shared-style mutation standing in for a per-object override, assumed inheritance state, exception-style table outcomes, string-concatenated field text where `TextFields.TryFormat` evaluates, and a live document-bound annotation crossing the session boundary.
