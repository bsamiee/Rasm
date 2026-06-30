# [ELEMENT_GEO_REFERENCE]

The host-neutral coordinate-reference owner: one `GeoReference` record carrying the full twelve-tuple map-conversion-and-CRS state — the eastings/northings/orthogonal-height translation, the X-axis abscissa/ordinate rotation cosine pair, the per-axis `ScaleX`/`ScaleY`/`ScaleZ`, the `GeodeticDatum`/`VerticalDatum` names, the `ProjectedCrsName`, and the parsed `Epsg` — and the `ProjectedCrs` `[ValueObject<string>]` whose EPSG parse spans the `EPSG:NNNN`, OGC URN, and authority forms. The reference is HOST-NEUTRAL and PURE DATA: it carries the parameters a downstream `Rasm.Bim` projector folds into a rigid map-conversion transform (over the kernel transform algebra) and a `ProjNET` datum-to-datum reprojection — the seam references NO ProjNET, NO kernel `Transform` type, and materializes no geometry, because geometry is referenced by content hash. The `Admit` factory FAULTS (not silently skips) when a CRS name is present but its EPSG cannot be resolved, so an unresolvable georeference surfaces as `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` rather than a silently-mislocated model. `GeoReference` rides ONLY the `Graph/element#ELEMENT_GRAPH` `Header` and the `Geospatial/coverage#COVERAGE_NODE` `Coverage` node — it is DROPPED from the `Object` node, because an object's placement is geometry the kernel owns by content hash, and the model-wide georeference is a header fact, not a per-object one. The page composes the kernel `Op` op-key and, for the `Geospatial/coverage#COVERAGE_NODE` coverage-node content identity, the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` (a coverage's `NodeId` derives from its CRS, so `GeoReference` owns the `CanonicalBytes` projection of its twelve-tuple).

## [01]-[INDEX]

- [01]-[GEO_REFERENCE]: the `GeoReference` twelve-tuple record, the `ProjectedCrs` `[ValueObject<string>]` EPSG parse, the `Admit` fault-on-unresolvable factory, and the `RotationRadians` direction-cosine projection.

## [02]-[GEO_REFERENCE]

- Owner: `GeoReference` the host-neutral coordinate-reference record carrying the twelve-tuple map-conversion-and-CRS state; `ProjectedCrs` the `[ValueObject<string>]` CRS identity carrying the EPSG parse across the `EPSG:NNNN`/`urn:ogc:def:crs:EPSG::NNNN`/`urn:ogc:def:crs:EPSG:6.18.3:NNNN`/authority forms.
- Entry: `GeoReference.Admit(...)` admits the twelve-tuple, parsing `ProjectedCrsName` into `Epsg` and `Fin<T>` railing `ElementFault.ValueRejected` when a non-blank CRS name yields no EPSG code (never a silent skip); `GeoReference.Identity` the non-georeferenced default a model without a map-conversion carries so ingest never blocks; `IsGeoreferenced` tests a resolved CRS or a non-zero origin; `RotationRadians` projects the X-axis abscissa/ordinate direction-cosine pair onto a rotation angle (`Atan2(ordinate, abscissa)`), the host-neutral scalar a downstream projector folds into the rigid transform.
- Auto: `Admit` reads the translation, rotation cosine pair, per-axis scale, datum names, and CRS name into the record, resolving the CRS through `ProjectedCrs.TryCreate(name, out var crs)` and reading `crs.Epsg` — a present-but-unresolvable name (non-blank, no EPSG) faults `ElementFault.ValueRejected`, a blank name yields the no-CRS `None`/`None` state (valid, so a non-georeferenced model never blocks); `ProjectedCrs.ValidateFactoryArguments` trims and rejects a blank identifier so the parse never runs over an empty string; `ProjectedCrs.Epsg` matches the `EPSG:` prefix, the OGC URN tail, and a bare integer, returning `Option<int>`; `RotationRadians` is the pure direction-cosine-to-angle projection (the IFC convention carrying the rotation as a direction rather than an angle), no kernel transform materialized.
- Receipt: the `GeoReference` is the coordinate-reference evidence the `Header` and a `Coverage` node carry; a downstream `Rasm.Bim` projector reads the twelve-tuple to build the rigid map-conversion transform over the kernel `Transform` algebra and to drive the `ProjNET` `CoordinateSystemServices` datum-to-datum reprojection (the source/target EPSG codes), the seam carrying only the parameters and the host-neutral rotation scalar.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject<string>]` + the generated `TryCreate(value, out item)`/`Create`/`Value`, the `ValidateFactoryArguments` admission hook, `ValidationError`, and `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]` for the case-insensitive CRS-identity equality), LanguageExt.Core (`Option`/`Fin` + the implicit `Error`→`Fin` lift the bare `ElementFault` case rides), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter` the `CanonicalBytes` CRS projection writes through), `Rasm` (the kernel `Op` op-key).
- Growth: a new map-conversion parameter is one column on `GeoReference` a projector folds into the transform; a new CRS is one `ProjectedCrs` value the EPSG parse resolves; a new datum is the projector's `ProjNET` concern resolved from the EPSG code; never a per-CRS class and never a transform owner on the seam.
- Boundary: `GeoReference` is HOST-NEUTRAL pure data — a kernel `Transform` field, a `ProjNET` `MathTransform`, or a host coordinate type on the seam is the named seam violation, the transform materialization and the datum reprojection being the `Rasm.Bim` projector's concern over the kernel transform algebra and the admitted `ProjNET` engine; the twelve-tuple is the full map-conversion-and-CRS state (eastings/northings/orthogonal-height, the abscissa/ordinate rotation cosine, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, geodetic/vertical datum, CRS name, parsed EPSG) so a federation reconciles every model onto one frame from one record; the translation/scale doubles carry the resolved CRS's native linear unit (the `IfcProjectedCRS.MapUnit` the `Epsg` implies — metre for the overwhelming majority of EPSG projected CRS, US-survey-foot for some State Plane zones), so the `Rasm.Bim` projector composes the model-unit↔CRS-unit factor when it builds the transform and a bare-double field's unit is fixed by the `Epsg`, never left ambiguous (a unit-bearing `MeasureValue` on the translation is the rejected form — the rotation cosines and scales are dimensionless, so the tuple stays a flat parameter record); the `Admit` factory FAULTS when a CRS name is present but unresolvable rather than silently skipping, so a mislocated model is a typed fault the ingest surfaces; `GeoReference` rides ONLY the `Header` and the `Coverage` node and is DROPPED from the `Object` node — an object's placement is content-hashed geometry the kernel owns, the model-wide georeference a header fact.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ProjectedCrs {
 // The one Thinktecture admission hook (normalize-and-validate in place): trim, then reject a blank
 // identifier — a CRS value with no identity is the deleted form. `Admit` never reaches here for a blank
 // IfcProjectedCRS name (it short-circuits to the no-CRS GeoReference first), so this value always carries
 // a real CRS identity; a caller passing an empty reference-system string gets `false`/`None`, never an empty CRS.
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim();
  if (value.Length == 0) { validationError = new ValidationError("projected CRS identifier must be non-blank"); }
 }

 // EPSG resolution across the EPSG:NNNN prefix, the double-colon URN tail (urn:ogc:def:crs:EPSG::NNNN),
 // the single-colon versioned URN tail (urn:ogc:def:crs:EPSG:6.18.3:NNNN), and a bare authority code.
 public Option<int> Epsg =>
 Value.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) && int.TryParse(Value.AsSpan(5), out int prefixed)
 ? Some(prefixed)
 : Value.LastIndexOf("::", StringComparison.Ordinal) is var urn and >= 0 && int.TryParse(Value.AsSpan(urn + 2), out int urnCode)
 ? Some(urnCode)
 : Value.LastIndexOf(':') is var sc and >= 0 && int.TryParse(Value.AsSpan(sc + 1), out int scCode)
 ? Some(scCode)
 : int.TryParse(Value, out int bare)
 ? Some(bare)
 : None;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record GeoReference(
 double Eastings,
 double Northings,
 double OrthogonalHeight,
 double XAxisAbscissa,
 double XAxisOrdinate,
 double ScaleX,
 double ScaleY,
 double ScaleZ,
 string GeodeticDatum,
 string VerticalDatum,
 Option<ProjectedCrs> ProjectedCrsName,
 Option<int> Epsg) {

 public static readonly GeoReference Identity =
 new(0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 1.0, "", "", None, None);

 public bool IsGeoreferenced => Epsg.IsSome || Eastings != 0.0 || Northings != 0.0 || OrthogonalHeight != 0.0;

 // IFC carries the map-conversion rotation as a direction cosine, not an angle.
 public double RotationRadians => Math.Atan2(XAxisOrdinate, XAxisAbscissa);

 // The CRS content projection a Geospatial/coverage#COVERAGE_NODE CoverageGrid delegates to for its node identity:
 // the map-conversion translation/rotation/per-axis scale, the datum names, and the parsed Epsg through the shared
 // Projection/address#CONTENT_ADDRESS IEEE-754 canon — the resolved ProjectedCrsName is EXCLUDED (Epsg is the identity),
 // so two georeferences resolving the same EPSG code address identically whether or not the name string differs.
 public void CanonicalBytes(CanonicalWriter w) {
  w.Double(Eastings).Double(Northings).Double(OrthogonalHeight)
   .Double(XAxisAbscissa).Double(XAxisOrdinate)
   .Double(ScaleX).Double(ScaleY).Double(ScaleZ)
   .String(GeodeticDatum).String(VerticalDatum)
   .Bool(Epsg.IsSome);
  Epsg.IfSome(e => w.Ordinal(e));
 }

 public static Fin<GeoReference> Admit(
 double eastings, double northings, double orthogonalHeight,
 double abscissa, double ordinate, double scaleX, double scaleY, double scaleZ,
 string geodeticDatum, string verticalDatum, string projectedCrsName, Op key) =>
 string.IsNullOrWhiteSpace(projectedCrsName)
 ? Fin.Succ(new GeoReference(eastings, northings, orthogonalHeight, abscissa, ordinate, scaleX, scaleY, scaleZ, geodeticDatum, verticalDatum, None, None))
 : ProjectedCrs.TryCreate(projectedCrsName, out ProjectedCrs? crs) && crs.Epsg is { IsSome: true } epsg
 ? Fin.Succ(new GeoReference(eastings, northings, orthogonalHeight, abscissa, ordinate, scaleX, scaleY, scaleZ, geodeticDatum, verticalDatum, Some(crs), epsg))
 : ElementFault.ValueRejected(key, $"<crs-name-unresolvable:{projectedCrsName}>");
}
```

## [03]-[RESEARCH]

- [MAP_CONVERSION_TUPLE]: the twelve-tuple extends the migration source's eight-field `Rasm.Bim` `GeoReference` — adding the per-axis `ScaleX`/`ScaleY`/`ScaleZ` (the IFC `IfcMapConversion` carries a single `Scale` plus optional `ScaleY`/`ScaleZ`), the `GeodeticDatum`/`VerticalDatum` names (from `IfcProjectedCRS.GeodeticDatum`/`VerticalDatum`), and the parsed `Epsg` so a federation reconciles onto one frame from one record; the `Rasm.Bim` projector reads `IfcMapConversion`/`IfcProjectedCRS` into this tuple at ingest, builds the rigid transform over the kernel transform algebra, and drives the `ProjNET` datum-to-datum reprojection — the seam carrying the parameters and the host-neutral rotation scalar, never the transform or the reprojection.
- [FAULT_ON_UNRESOLVABLE]: a present-but-unresolvable CRS name is a FAULT, not a silent skip — `Admit` parses `ProjectedCrsName` into `Epsg` across the `EPSG:`/URN/authority forms and rails `ElementFault.ValueRejected` when a non-blank name yields no code, so a mislocated model surfaces at ingest rather than producing silently-wrong coordinates downstream; a blank name yields `Identity`-style no-CRS state so a non-georeferenced model never blocks.
