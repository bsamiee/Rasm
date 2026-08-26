# [RASM_FABRICATION_PROFILE_IMPORT]

`ProfileImport` owns DXF/DWG census, admission, topology healing, and projection. `ProfileFormat` dispatches every admitted path, `ProfilePolicy` admits unit, sampling, lane, fill, entity, notification, and closure decisions as one gated value, provider entities lower through their owning OCS frames into provenance-bearing contours and markings, and `ProfileTopology` stitches compatible endpoints, composes `ArcAlgebra` and `PolygonAlgebra`, and records each repair. `Ingress` seats here too, so every admitted geometry source reaches the fabrication pipeline through one entry, and `SourceSnapshot` is the one byte-to-path materialization every path-taking provider reader in the sub-domain composes.

`Loop`, `Context`, `PolygonFill`, `ArcAlgebra`, `PolygonAlgebra`, `ArcForest`, `RegionTopology`, and `DensifyEvidence` arrive settled from `Geometry2D/algebra` and `Geometry2D/arcs`. `ContentHash.Of` is the one kernel digest mint every fabrication egress key seeds from. `Process/faults` allocates this lane `IngressTranslation` and `IngressProviderUnavailable` over provider-neutral `SourceLocus.ProfileEntity`, `PolicyInadmissible` on `FabConcern.Ingress` for every declared-value refusal, and degenerate topology routes `GeometryFault.DegenerateInput`. Every owned vocabulary that corresponds to a provider enum carries the HOST ORDINALS as a column and admits by containment, so no lowering arm restates a provider roster. Public entries defer boundary work on `Eff`.

## [01]-[INDEX]

- [02]-[RAW_ADMISSION]: `ProfileSource` the one raw gate, unit/read/lane/entity/closure policy admitted as declared values, `ProfileFormat` encoding discrimination and provider read, `SourceSnapshot` the sub-domain's byte materialization, and the `ProfileCensus` survey leg.
- [03]-[CANONICAL_OWNER]: provenance-preserving lowering through owning frames, the `MarkingContent` annotation family with its host-ordinal admission and sampler evidence, the order-independent entity dispatch table, `ProfileTopology` stitching over one union-find pass, and the `ImportedProfile` digest, census, repair, and extent evidence.
- [04]-[PROJECTION_EGRESS]: `ProfileProjection` the closed egress row carrying its own view delegate, `ProfileView` carrying each row's result shape.
- [05]-[INGRESS_FOLD]: `IngressSource`, `AdmittedGeometry`, and the total `Ingress.Admit` dispatch every sibling ingress page terminates in.

## [02]-[RAW_ADMISSION]

- Owner: `ProfileSource` is the one raw profile gate over a `ProfilePath` and a `ProfilePolicy`; `ProfileFormat` binds each admitted extension to its provider read as a constructor delegate; `ProfileUnitPolicy`, `ProfileReadPolicy`, `ProfileEntityPolicy`, and `ProfileClosure` carry unit, reader, entity, and completion decisions while the lane table folds into `ProfilePolicy` as its own column block; `SourceSnapshot` owns byte materialization; `ProfileCensus` owns the pre-admission survey.
- Cases: `ProfileFormat` closes DXF and DWG; `ProfileEncoding` closes ascii and binary; `ProfileLane` closes cut · etch · score · bend · mark · reference over its `Contributes` and `Closes` columns; `ProfileUnitPolicy` closes declared · declared-or-fallback · override; `ProfileClosure` closes open · exact · healed; `ProfileEntityPolicy` closes ignore · reject; `ProfileNoticeKind` and `ProfileReadCapability` close the provider notice severities and the reader capability set.
- Law: every policy carrier ADMITS. A declared value that reaches the reader unproved lets a zero spline density, an empty lane map, or a rejects-everything notice set fail at the provider call with a provider message instead of at its own gate with a typed locus.
- Law: `SourceSnapshot` is the ONE byte-to-path materialization in `Ingress`. Both `ACadSharp` readers and the OCCT reader take a PATH, so the admitted bytes — exactly what `SourceDigest` identifies — materialize once and delete on every exit; re-reading the caller's original path admits edited bytes, and a second temp-file helper at a sibling page is the deleted duplicate.
- Entry: `ProfileImport.Probe(ProfileSource)` returns one deferred `Eff<ProfileCensus>` — the survey leg reading encoding, declared units, the complete layer table with its lane assignment, per-type entity counts, and provider notices without admitting one contour.
- Auto: `ProfileEncoding.Of` classifies the DXF byte layout through `DxfReader.IsBinary` before either read opens; the survey leg reads the layer table rather than the entity stream, so `ProfileLayerCoverage.CompleteTable` is a fact the census carries rather than a claim; `ProfileScale` is one `UnitsType`-keyed table so a survey dialect and a metric one resolve through one lookup, and `ProfileUnitPolicy.DeclaredOr` falls back on ANY unresolvable declaration rather than `Unitless` alone; `ProfilePolicy` holds ONE case-insensitive lane index keyed on each admitted `LayerName`'s own rendered text, so lane resolution costs a lookup per lowered entity rather than a scan of the declared map.
- Output: one `ProfileCensus` — encoding, declared `UnitsType`, `ProfileLayerCoverage`, the complete per-layer lane assignment with entity counts, per-type entity counts, and the provider notice stream.
- Packages: `ACadSharp` owns reader configuration, encoding classification, and partial reads; `UnitsNet` owns the millimeter scale every declared unit resolves to; `Thinktecture.Runtime.Extensions` owns the closed policy families and their admission; `LanguageExt.Core` owns the deferred effect, the notice cell, and the immutable carriers.
- Growth: a new file family is one `ProfileFormat` row carrying its extensions and its read delegate; a new fabrication intent is one `ProfileLane` row with its two columns; a new reader knob is one `ProfileReadCapability` row read at the provider call; a new declared unit is one `ProfileScale` row.
- Boundary: the notice cell, the byte snapshot, and the disposable partial readers are the provider statement kernel — every `CadDocument` and `Entity` terminates here and no provider type reaches the canonical owner; documented BCL file-availability exceptions lower to caused `IngressProviderUnavailable`, while ACadSharp and callback throws retain the exact exceptional `Error`. `ProfileCensus` holds the provider notifications once, so no later stage re-reads them; `ProfilePolicy` decides which lanes owe closure, so a bend or etch run never fails a healed import and a reference layer is censused then discarded; a rejected notice kind lowers to `IngressTranslation` on the source locus before any contour is built.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Types.Units;
using CSMath;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Collections;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;
using CadArc = ACadSharp.Entities.Arc;
using CadCircle = ACadSharp.Entities.Circle;
using CadEllipse = ACadSharp.Entities.Ellipse;
using CadLine = ACadSharp.Entities.Line;
using CadPoint = ACadSharp.Entities.Point;
using CadSpline = ACadSharp.Entities.Spline;
using CadText = ACadSharp.Entities.TextEntity;
using HatchArc = ACadSharp.Entities.Hatch.BoundaryPath.Arc;
using HatchEllipse = ACadSharp.Entities.Hatch.BoundaryPath.Ellipse;
using HatchLine = ACadSharp.Entities.Hatch.BoundaryPath.Line;
using HatchPolyline = ACadSharp.Entities.Hatch.BoundaryPath.Polyline;
using HatchSpline = ACadSharp.Entities.Hatch.BoundaryPath.Spline;

namespace Rasm.Fabrication.Ingress;

// --- [RAW_ADMISSION] -------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct ProfilePath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (!Witness.Keyed(value)) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { "profile-path:blank" }));
            return;
        }
        value = Path.GetFullPath(value);
    }

    public static Fin<ProfilePath> Admit(string value) => Admission.OfValue<ProfilePath, string>(value);
}

[ValueObject<int>]
public readonly partial struct SplineDensity {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 2 ? new ValidationError(string.Join(" | ", new object?[] { "spline-density:below-two" })) : null;

    public static Fin<SplineDensity> Admit(int value) => Admission.OfValue<SplineDensity, int>(value);
}

[ValueObject<Length>]
public readonly partial struct ProfileGap {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Length value) =>
        validationError = ValidityClaim.Positive(value.Millimeters) ? null : new ValidationError(string.Join(" | ", new object?[] { "profile-gap:non-positive" }));

    public static Fin<ProfileGap> Admit(Length value) => Admission.OfValue<ProfileGap, Length>(value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileUnitPolicy {
    private ProfileUnitPolicy() { }
    public sealed record Declared : ProfileUnitPolicy;
    public sealed record DeclaredOr(LengthUnit Unit) : ProfileUnitPolicy;
    public sealed record Override(LengthUnit Unit) : ProfileUnitPolicy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileClosure {
    private ProfileClosure() { }
    public sealed record Open : ProfileClosure;
    public sealed record Exact : ProfileClosure;
    public sealed record Healed(ProfileGap MaxGap) : ProfileClosure;
}

[SmartEnum<string>]
public sealed partial class ProfileEntityPolicy {
    public static readonly ProfileEntityPolicy Ignore = new("ignore");
    public static readonly ProfileEntityPolicy Reject = new("reject");
}

[SmartEnum<string>]
public sealed partial class ProfileNoticeKind {
    public static readonly ProfileNoticeKind None = new("none", NotificationType.None);
    public static readonly ProfileNoticeKind Warning = new("warning", NotificationType.Warning);
    public static readonly ProfileNoticeKind Error = new("error", NotificationType.Error);
    public static readonly ProfileNoticeKind NotImplemented = new("not-implemented", NotificationType.NotImplemented);

    public NotificationType Severity { get; }

    public static ProfileNoticeKind Of(NotificationType severity) =>
        toSeq(Items).Find(row => row.Severity == severity).IfNone(None);
}

[SmartEnum<string>]
public sealed partial class ProfileReadCapability {
    public static readonly ProfileReadCapability Recover = new("recover");
    public static readonly ProfileReadCapability UnknownEntities = new("unknown-entities");
    public static readonly ProfileReadCapability UnknownObjects = new("unknown-objects");
    public static readonly ProfileReadCapability Crc = new("crc");
}

[SmartEnum<string>]
public sealed partial class ProfileEncoding {
    public static readonly ProfileEncoding Ascii = new("ascii");
    public static readonly ProfileEncoding Binary = new("binary");

    public static ProfileEncoding Of(ProfileFormat format, ProfilePath path) => format.Switch(
        state: path,
        dxf: static value => DxfReader.IsBinary(value.Value) ? Binary : Ascii,
        dwg: static _ => Binary);
}

[SmartEnum<string>]
public sealed partial class ProfileLane {
    public static readonly ProfileLane Cut = new("cut", contributes: true, closes: true);
    public static readonly ProfileLane Etch = new("etch", contributes: true, closes: false);
    public static readonly ProfileLane Score = new("score", contributes: true, closes: false);
    public static readonly ProfileLane Bend = new("bend", contributes: true, closes: false);
    public static readonly ProfileLane Mark = new("mark", contributes: true, closes: false);
    public static readonly ProfileLane Reference = new("reference", contributes: false, closes: false);

    public bool Contributes { get; }
    public bool Closes { get; }
}

public static class ProfileScale {
    private static readonly FrozenDictionary<UnitsType, double> Rows = new Dictionary<UnitsType, double> {
        [UnitsType.Angstroms] = Millimeters(LengthUnit.Angstrom),
        [UnitsType.Nanometers] = Millimeters(LengthUnit.Nanometer),
        [UnitsType.Microns] = Millimeters(LengthUnit.Micrometer),
        [UnitsType.Millimeters] = Millimeters(LengthUnit.Millimeter),
        [UnitsType.Centimeters] = Millimeters(LengthUnit.Centimeter),
        [UnitsType.Decimeters] = Millimeters(LengthUnit.Decimeter),
        [UnitsType.Meters] = Millimeters(LengthUnit.Meter),
        [UnitsType.Decameters] = Millimeters(LengthUnit.Decameter),
        [UnitsType.Hectometers] = Millimeters(LengthUnit.Hectometer),
        [UnitsType.Kilometers] = Millimeters(LengthUnit.Kilometer),
        [UnitsType.Gigameters] = Millimeters(LengthUnit.Gigameter),
        [UnitsType.Microinches] = Millimeters(LengthUnit.Microinch),
        [UnitsType.Mils] = Millimeters(LengthUnit.Mil),
        [UnitsType.Inches] = Millimeters(LengthUnit.Inch),
        [UnitsType.Feet] = Millimeters(LengthUnit.Foot),
        [UnitsType.Yards] = Millimeters(LengthUnit.Yard),
        [UnitsType.Miles] = Millimeters(LengthUnit.Mile),
        [UnitsType.AstronomicalUnits] = Millimeters(LengthUnit.AstronomicalUnit),
        [UnitsType.LightYears] = Millimeters(LengthUnit.LightYear),
        [UnitsType.Parsecs] = Millimeters(LengthUnit.Parsec),
        [UnitsType.USSurveyFeet] = Millimeters(LengthUnit.UsSurveyFoot),
        [UnitsType.USSurveyInches] = Length.From(1d / 12d, LengthUnit.UsSurveyFoot).Millimeters,
        [UnitsType.USSurveyYards] = Length.From(3d, LengthUnit.UsSurveyFoot).Millimeters,
        [UnitsType.USSurveyMiles] = Length.From(5280d, LengthUnit.UsSurveyFoot).Millimeters,
    }.ToFrozenDictionary();

    public static Option<double> Of(UnitsType declared) =>
        Rows.TryGetValue(declared, out double scale) ? Some(scale) : None;

    public static double Of(LengthUnit unit) => Millimeters(unit);

    private static double Millimeters(LengthUnit unit) => Length.From(1d, unit).Millimeters;
}

[ComplexValueObject]
public sealed partial class ProfileReadPolicy {
    public Set<ProfileReadCapability> Capabilities { get; }
    public Set<ProfileNoticeKind> Rejects { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Set<ProfileReadCapability> capabilities,
        ref Set<ProfileNoticeKind> rejects) {
        if (rejects.Contains(ProfileNoticeKind.None))
            validationError = new ValidationError(string.Join(" | ", new object?[] { "profile-reader:rejects-none" }));
    }

    public static Fin<ProfileReadPolicy> Admit(
        Set<ProfileReadCapability> capabilities, Set<ProfileNoticeKind> rejects) =>
        Validate(capabilities, rejects, out ProfileReadPolicy policy).Admitted(policy);
}

[ComplexValueObject]
public sealed partial class ProfilePolicy {
    public SplineDensity Spline { get; }
    public ProfileUnitPolicy Units { get; }
    public ProfileClosure Closure { get; }
    public ProfileEntityPolicy Unsupported { get; }
    public ProfileReadPolicy Reader { get; }

    // --- [LANE_BLOCK]
    public Map<LayerName, ProfileLane> Layers { get; }
    public ProfileLane Fallback { get; }

    [IgnoreMember]
    private FrozenDictionary<string, ProfileLane>? lanes;

    private FrozenDictionary<string, ProfileLane> Index => lanes ??= Layers
        .AsIterable()
        .ToDictionary(static row => row.Key.Text, static row => row.Value, StringComparer.OrdinalIgnoreCase)
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public ProfileLane Lane(string layer) =>
        Index.TryGetValue(layer, out ProfileLane? lane) ? lane : Fallback;

    public PolygonFill Fill { get; }
    public Context Tolerance { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SplineDensity spline,
        ref ProfileUnitPolicy units,
        ref ProfileClosure closure,
        ref ProfileEntityPolicy unsupported,
        ref ProfileReadPolicy reader,
        ref Map<LayerName, ProfileLane> layers,
        ref ProfileLane fallback,
        ref PolygonFill fill,
        ref Context tolerance) {
        if (closure is ProfileClosure.Healed healed && healed.MaxGap.Value.Millimeters < tolerance.Absolute.Value)
            validationError = new ValidationError(string.Join(" | ", new object?[] { "profile-closure:gap-below-grid" }));
    }

    public static Fin<ProfilePolicy> Admit(
        SplineDensity spline,
        ProfileUnitPolicy units,
        ProfileClosure closure,
        ProfileEntityPolicy unsupported,
        ProfileReadPolicy reader,
        Map<LayerName, ProfileLane> layers,
        ProfileLane fallback,
        PolygonFill fill,
        Context tolerance) =>
        Validate(spline, units, closure, unsupported, reader, layers, fallback, fill, tolerance,
            out ProfilePolicy policy).Admitted(policy);
}

public sealed record ProfileSource(ProfilePath Path, ProfilePolicy Policy);

public static class SourceSnapshot {
    public static T With<T>(ReadOnlySpan<byte> payload, string extension, Func<string, T> read) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        try {
            File.WriteAllBytes(path, payload.ToArray());
            return read(path);
        }
        finally {
            File.Delete(path);
        }
    }
}

[SmartEnum<string>]
public sealed partial class ProfileFormat {
    public static readonly ProfileFormat Dxf = new("dxf", Arr(".dxf"), ReadDxf);
    public static readonly ProfileFormat Dwg = new("dwg", Arr(".dwg"), ReadDwg);

    public Arr<string> Extensions { get; }

    [UseDelegateFromConstructor]
    public partial CadDocument Read(byte[] payload, ProfileReadPolicy policy, NotificationEventHandler sink);

    public static Fin<ProfileFormat> Admit(ProfilePath path) =>
        toSeq(Items).Find(format => format.Extensions.Exists(extension =>
                string.Equals(extension, Path.GetExtension(path.Value), StringComparison.OrdinalIgnoreCase)))
            .ToFin(ProfileImport.Fault(path, "profile-format:unsupported"));

    private static CadDocument ReadDxf(byte[] payload, ProfileReadPolicy policy, NotificationEventHandler sink) =>
        SourceSnapshot.With(payload, ".dxf", path => DxfReader.Read(path, new DxfReaderConfiguration {
            Failsafe = policy.Capabilities.Contains(ProfileReadCapability.Recover),
            KeepUnknownEntities = policy.Capabilities.Contains(ProfileReadCapability.UnknownEntities),
            KeepUnknownNonGraphicalObjects = policy.Capabilities.Contains(ProfileReadCapability.UnknownObjects),
        }, sink));

    private static CadDocument ReadDwg(byte[] payload, ProfileReadPolicy policy, NotificationEventHandler sink) =>
        SourceSnapshot.With(payload, ".dwg", path => DwgReader.Read(path, new DwgReaderConfiguration {
            Failsafe = policy.Capabilities.Contains(ProfileReadCapability.Recover),
            KeepUnknownEntities = policy.Capabilities.Contains(ProfileReadCapability.UnknownEntities),
            KeepUnknownNonGraphicalObjects = policy.Capabilities.Contains(ProfileReadCapability.UnknownObjects),
            CrcCheck = policy.Capabilities.Contains(ProfileReadCapability.Crc),
        }, sink));
}

public sealed record ProfileNotification(ProfileNoticeKind Kind, string Message, Option<string> Exception);

public sealed record ProfileLayerCensus(string Name, ProfileLane Lane, int Entities);

[SmartEnum<string>]
public sealed partial class ProfileLayerCoverage {
    public static readonly ProfileLayerCoverage EntityBearing = new("entity-bearing");
    public static readonly ProfileLayerCoverage CompleteTable = new("complete-table");
}

public sealed record ProfileCensus(
    ProfileEncoding Encoding,
    UnitsType DeclaredUnits,
    ProfileLayerCoverage LayerCoverage,
    Map<string, ProfileLayerCensus> Layers,
    Map<string, int> Entities,
    Seq<ProfileNotification> Notifications);

public static partial class ProfileImport {
    private static readonly Op ReadOp = Op.Of(name: nameof(ProfileImport));

    public static Eff<ProfileCensus> Probe(ProfileSource source) => Eff.lift(() =>
        ProfileFormat.Admit(source.Path).Bind(format => Capture(source.Path, notices => format.Switch(
            state: (Source: source, Format: format, Notices: notices),
            dxf: static state => ProbeDxf(state.Source, state.Format, state.Notices),
            dwg: static state => ReadOp.Catch(() => Fin.Succ(File.ReadAllBytes(state.Source.Path.Value)))
                .MapFail(error => Classify(state.Source.Path, error))
                .Bind(payload => Open(ProfileFormat.Dwg, state.Source, payload, state.Notices))
                .Map(document => Census(
                    state.Format, state.Source, document.Header.InsUnits,
                    LayerNames(document), toSeq(document.Entities).Strict(), state.Notices.Value))))))
        .Bind(static result => result.ToEff());

    private static Fin<T> Capture<T>(ProfilePath path, Func<Atom<Seq<ProfileNotification>>, Fin<T>> use) {
        Atom<Seq<ProfileNotification>> notices = Atom(Seq<ProfileNotification>());
        return use(notices);
    }

    private static Fin<CadDocument> Open(
        ProfileFormat format,
        ProfileSource source,
        byte[] payload,
        Atom<Seq<ProfileNotification>> notices) =>
        ReadOp.Catch(() => Fin.Succ(format.Read(payload, source.Policy.Reader,
            (_, args) => notices.Swap(rows => rows.Add(Notice(args))))));

    private static Fin<ProfileCensus> ProbeDxf(
        ProfileSource source, ProfileFormat format, Atom<Seq<ProfileNotification>> notices) =>
        ReadOp.Catch(() => {
            NotificationEventHandler sink = (_, args) => notices.Swap(rows => rows.Add(Notice(args)));
            using DxfReader headerReader = new(source.Path.Value, sink);
            UnitsType units = headerReader.ReadHeader().InsUnits;
            using DxfReader tableReader = new(source.Path.Value, sink);
            Seq<string> layers = toSeq(tableReader.ReadTables().Layers).Map(static layer => layer.Name).Strict();
            using DxfReader entityReader = new(source.Path.Value, sink);
            return Fin.Succ(Census(format, source, units, layers, toSeq(entityReader.ReadEntities()).Strict(), notices.Value));
        }).MapFail(error => Classify(source.Path, error));

    private static ProfileCensus Census(
        ProfileFormat format,
        ProfileSource source,
        UnitsType units,
        Seq<string> layers,
        Seq<Entity> entities,
        Seq<ProfileNotification> notifications) {
        Map<string, int> counts = toSeq(entities.GroupBy(static entity => entity.Layer.Name))
            .Map(static group => (group.Key, group.Count())).ToMap();
        return new ProfileCensus(
            ProfileEncoding.Of(format, source.Path),
            units,
            ProfileLayerCoverage.CompleteTable,
            toSeq(layers.Distinct().OrderBy(static name => name, StringComparer.Ordinal))
                .Map(name => (name, new ProfileLayerCensus(
                    name, source.Policy.Lane(name), counts.Find(name).IfNone(0))))
                .ToMap(),
            toSeq(entities.GroupBy(static entity => entity.GetType().Name))
                .Map(static group => (group.Key, group.Count())).ToMap(),
            notifications);
    }

    private static Seq<string> LayerNames(CadDocument document) =>
        toSeq(document.Layers).Map(static layer => layer.Name).Strict();

    private static Fin<(double Scale, UnitsType Evidence)> Scale(
        UnitsType declared, ProfileUnitPolicy policy, ProfilePath path) => policy.Switch(
        state: declared,
        declared: static (unit, _) => ProfileScale.Of(unit).Map(scale => (scale, unit))
            .ToFin(FabricationFault.Inadmissible(FabConcern.Ingress, $"profile-unit:{unit}")),
        declaredOr: static (unit, fallback) => Fin.Succ((
            ProfileScale.Of(unit).IfNone(() => ProfileScale.Of(fallback.Unit)), unit)),
        @override: static (unit, forced) => Fin.Succ((ProfileScale.Of(forced.Unit), unit)));

    private static Fin<Unit> Reject(
        Atom<Seq<ProfileNotification>> notices, ProfileReadPolicy policy, ProfilePath path) =>
        notices.Value.Find(notice => policy.Rejects.Contains(notice.Kind))
            .Match(
                Some: notice => Fin.Fail<Unit>(FabricationFault.Sourced(
                    new SourceLocus.ProfileEntity(Path.GetFileName(path.Value)), notice.Message)),
                None: static () => Fin.Succ(unit));

    private static ProfileNotification Notice(NotificationEventArgs args) => new(
        ProfileNoticeKind.Of(args.NotificationType), args.Message, Optional(args.Exception?.Message));

    internal static Error Fault(ProfilePath path, string detail) => FabricationFault.Sourced(
        new SourceLocus.ProfileEntity(Path.GetFileName(path.Value)), detail);

    private static Error Fault(ProfilePath path, Error error) => FabricationFault.Unavailable(
        new SourceLocus.ProfileEntity(Path.GetFileName(path.Value)), error.Message, error);

    private static Error Classify(ProfilePath path, Error error) => error.Exception
        .Filter(static raised => raised is IOException or UnauthorizedAccessException)
        .Map(_ => Fault(path, error))
        .IfNone(error);

    private static Error Fault(ProfilePath path, Entity entity, string detail) =>
        FabricationFault.Sourced(
            new SourceLocus.ProfileEntity($"{Path.GetFileName(path.Value)}#{entity.Handle:x}"), detail);

}
```

## [03]-[CANONICAL_OWNER]

- Owner: `ProfileContour` carries one provider-lowered `Loop` with its `ProfileProvenance`; `ProfileMarking` carries a located, rotated annotation over the closed `MarkingContent` family and admits no loop; `MarkingType` carries the line sequence, height, style, and `MarkingAnchor` every text-bearing case shares; `ProfileEntity` owns the order-independent lowering dispatch; `ProfileTopology` owns stitching, closure sealing, cleanup, densification, and region derivation; `ImportedProfile` is the settled evidence carrier every projection reads.
- Cases: `ProfileRepair` closes joined · closed · cleaned · densified · sampled · topology, each carrying the provenance it repaired beside its measured delta; `MarkingContent` closes glyph · text · paragraph · tag; `MarkingSide`, `MarkingRung`, `MarkingFit`, and `MarkingFlag` re-close the provider justification, stretch, and attribute-flag vocabularies by carrying the host ordinals as columns; `SplineSampler` closes parametric · tessellated · refit.
- Law: entity lowering resolves by walking the provider type's OWN base chain from most-derived upward, so a table row for a base can never shadow the row for a type that derives from it. The prior `switch` ladder made three arms order-critical — placed attribute before single-line text, exploded unit ellipse before the general conic — and carried three comment warnings saying so; the walk makes declaration order decide nothing.
- Law: DXF codes 11/21/31 govern placement exactly where code 72 or 73 is nonzero, so `MarkingContent.Text.Align` places every `MarkingAnchor` outside `Left`/`Baseline` and `ProfileMarking.At` places the rest; both points survive lowering because `TextEntity.ApplyTransform` and `GetBoundingBox` read `InsertPoint` alone, leaving a transformed provider entity's alignment point stale.
- Law: stitching is ONE pairwise endpoint census, ONE union pass, and ONE ordered walk per component. Re-scoring every candidate pair after each merge paid cubic distance tests for a relation the endpoints already fix; the fork guard runs FIRST, so each surviving component is a simple chain whose order is the walk from either free end.
- Entry: `ProfileImport.Read(ProfileSource)` returns one deferred `Eff<ImportedProfile>` folding ONE byte read, ONE document open, notice rejection, unit resolution, entity lowering, census derivation, and topology repair on one effect over one entity snapshot.
- Auto: entity coordinates lower through their owning frame, so a mirrored normal inverts bulge sense wherever the frame maps; one hatch emits one contour per `Paths` row, its line, circular-arc, and polyline leaves preserving exact endpoints and bulges while its ellipse and spline leaves compose the provider's own samplers; the spline arm walks the owner's parametric evaluator first and falls back to its bulk tessellator then its fit-point rebuild in order, the surviving sampler landing as `ProfileRepair.Sampled` evidence; an insertion lowers the placed attribute collection beside its exploded children, because the provider's explode leg enumerates the block record alone; `ProfileProvenance` preserves the entity handle and ordinal set, so each fault names its entity and `Validation` accumulates every rejection in one pass; `ProfileBlock` preserves nested insert identity and replica indices through arbitrary depth, and the block-reference graph fails `IsDirectedAcyclicGraph` over bare edges read off the block TABLE before any lowering runs, so a cyclic block refuses typed rather than being caught by an ancestor set threaded down a walk that has already lowered half the drawing.
- Result: `ImportedProfile` carries the source digest minted from the file bytes through `ContentHash.Of`, the admitted format, the census, `ProfileUnitEvidence`, contours, markings, regions, extents, and the typed repair sequence; `Loops` projects the boundary set without re-walking provenance, and `ProfileMarking.Tag` names the one key a traveler or posted program looks a marking up by.
- Packages: `ArcAlgebra` composes `CavalierContours` for arc-native cleanup and densification; `PolygonAlgebra` composes `Clipper2` for fill-rule region topology; `QuikGraph` `ForestDisjointSet<int>` partitions the stitch candidates and `IsDirectedAcyclicGraph` fails the block-reference census; `Loop` owns bulge-bearing admission; `UnitsNet` carries every join distance, closure gap, glyph height, and area delta; `LanguageExt.Core` owns the result types and immutable carriers.
- Growth: a new provider entity is one dispatch ROW beside its loop factory; a new annotation modality is one `MarkingContent` case beside its lowering arm; a new sampler is one `SplineSampler` row; a new repair species is one `ProfileRepair` case carrying its measured evidence; a new grouping axis is one field on the provenance key the stitch and normalize folds already read.
- Boundary: closure is demanded only from lanes `ProfileLane.Closes` marks, so an open bend run reaches the result unhealed; provider justification, attachment, stretch, and attribute-flag rosters resolve through owned rows carrying those ordinals, so no provider enum reaches the result and no arm restates a roster; `ProfileTopology` reopens no source file and holds no provider handle, because admission already terminated every provider type.

```csharp
// --- [CANONICAL_OWNER] -----------------------------------------------------------------
public readonly record struct ProfileBlock(string Name, int Ordinal, int Row, int Column);

public sealed record ProfileProvenance(
    string Layer,
    ProfileLane Lane,
    Option<AciIndex> Colour,
    ulong Handle,
    Seq<ProfileBlock> Blocks,
    double Plane,
    Set<int> Ordinals);

public sealed record ProfileContour(Loop Loop, ProfileProvenance Provenance);

[SmartEnum<string>]
public sealed partial class MarkingSide {
    public static readonly MarkingSide Left = new("left",
        Set(TextHorizontalAlignment.Left, TextHorizontalAlignment.Aligned, TextHorizontalAlignment.Fit),
        Set(AttachmentPointType.TopLeft, AttachmentPointType.MiddleLeft, AttachmentPointType.BottomLeft));
    public static readonly MarkingSide Center = new("center",
        Set(TextHorizontalAlignment.Center, TextHorizontalAlignment.Middle),
        Set(AttachmentPointType.TopCenter, AttachmentPointType.MiddleCenter, AttachmentPointType.BottomCenter));
    public static readonly MarkingSide Right = new("right",
        Set(TextHorizontalAlignment.Right),
        Set(AttachmentPointType.TopRight, AttachmentPointType.MiddleRight, AttachmentPointType.BottomRight));

    public Set<TextHorizontalAlignment> Justification { get; }
    public Set<AttachmentPointType> Attachment { get; }

    public static Option<MarkingSide> Of(TextHorizontalAlignment value) =>
        toSeq(Items).Find(row => row.Justification.Contains(value));

    public static Option<MarkingSide> Of(AttachmentPointType value) =>
        toSeq(Items).Find(row => row.Attachment.Contains(value));
}

[SmartEnum<string>]
public sealed partial class MarkingRung {
    public static readonly MarkingRung Top = new("top",
        Set(TextVerticalAlignmentType.Top),
        Set(AttachmentPointType.TopLeft, AttachmentPointType.TopCenter, AttachmentPointType.TopRight));
    public static readonly MarkingRung Middle = new("middle",
        Set(TextVerticalAlignmentType.Middle),
        Set(AttachmentPointType.MiddleLeft, AttachmentPointType.MiddleCenter, AttachmentPointType.MiddleRight));
    public static readonly MarkingRung Bottom = new("bottom",
        Set(TextVerticalAlignmentType.Bottom),
        Set(AttachmentPointType.BottomLeft, AttachmentPointType.BottomCenter, AttachmentPointType.BottomRight));
    public static readonly MarkingRung Baseline = new("baseline",
        Set(TextVerticalAlignmentType.Baseline), Set<AttachmentPointType>());

    public Set<TextVerticalAlignmentType> Datum { get; }
    public Set<AttachmentPointType> Attachment { get; }

    public static Option<MarkingRung> Of(TextVerticalAlignmentType value) =>
        toSeq(Items).Find(row => row.Datum.Contains(value));

    public static Option<MarkingRung> Of(AttachmentPointType value) =>
        toSeq(Items).Find(row => row.Attachment.Contains(value));
}

public readonly record struct MarkingAnchor(MarkingSide Side, MarkingRung Rung) {
    public static Option<MarkingAnchor> Of(TextHorizontalAlignment horizontal, TextVerticalAlignmentType vertical) =>
        (MarkingSide.Of(horizontal), MarkingRung.Of(vertical))
            .Apply(static (side, rung) => new MarkingAnchor(side, rung)).As();

    public static Option<MarkingAnchor> Of(AttachmentPointType attachment) =>
        (MarkingSide.Of(attachment), MarkingRung.Of(attachment))
            .Apply(static (side, rung) => new MarkingAnchor(side, rung)).As();
}

[SmartEnum<string>]
public sealed partial class MarkingFit {
    public static readonly MarkingFit Natural = new("natural", Set<TextHorizontalAlignment>());
    public static readonly MarkingFit Aligned = new("aligned", Set(TextHorizontalAlignment.Aligned));
    public static readonly MarkingFit Fitted = new("fitted", Set(TextHorizontalAlignment.Fit));

    public Set<TextHorizontalAlignment> Justification { get; }

    public static MarkingFit Of(TextHorizontalAlignment value) =>
        toSeq(Items).Find(row => row.Justification.Contains(value)).IfNone(Natural);
}

[SmartEnum<string>]
public sealed partial class MarkingFlag {
    public static readonly MarkingFlag Hidden = new("hidden", AttributeFlags.Hidden);
    public static readonly MarkingFlag Constant = new("constant", AttributeFlags.Constant);
    public static readonly MarkingFlag Verify = new("verify", AttributeFlags.Verify);
    public static readonly MarkingFlag Preset = new("preset", AttributeFlags.Preset);

    public AttributeFlags Bit { get; }

    public static Set<MarkingFlag> Of(AttributeFlags flags) =>
        toSet(toSeq(Items).Filter(row => flags.HasFlag(row.Bit)));
}

public sealed record MarkingType(
    Seq<string> Lines, Length Height, LetteringForm Form, string Style, MarkingAnchor Anchor) {
    public string Text => string.Join('\n', Lines);

    public Fin<TextHeight> Rung => TextHeight.For(Height);

    public Fin<DraftingMetrics> Metrics => Rung.Map(Form.Metrics);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MarkingContent {
    private MarkingContent() { }
    public sealed record Glyph : MarkingContent;
    public sealed record Text(MarkingType Type, MarkingFit Fit, Point3d Align) : MarkingContent;
    public sealed record Paragraph(MarkingType Type, Length Column, double LineSpacing) : MarkingContent;
    public sealed record Tag(string Name, MarkingType Type, Set<MarkingFlag> Flags) : MarkingContent;

    public static readonly MarkingContent Mark = new Glyph();

    public MarkingContent Shift(Vector3d delta) => Switch(
        state: (Content: this, Delta: delta),
        glyph: static (state, _) => state.Content,
        text: static (state, arm) => (MarkingContent)(arm with { Align = arm.Align + state.Delta }),
        paragraph: static (state, _) => state.Content,
        tag: static (state, _) => state.Content);
}

public sealed record ProfileMarking(
    Point3d At, double Rotation, MarkingContent Content, ProfileProvenance Provenance) {
    public Option<string> Tag => Content is MarkingContent.Tag tag ? Some(tag.Name) : None;
}

[SmartEnum<string>]
public sealed partial class SplineSampler {
    public static readonly SplineSampler Parametric = new("parametric");
    public static readonly SplineSampler Tessellated = new("tessellated");
    public static readonly SplineSampler Refit = new("refit");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileRepair {
    private ProfileRepair() { }
    public sealed record Joined(ProfileProvenance Provenance, Length Distance, int Count) : ProfileRepair;
    public sealed record Closed(ProfileProvenance Provenance, Length Gap) : ProfileRepair;
    public sealed record Cleaned(ProfileProvenance Provenance, int Before, int After, int Segments) : ProfileRepair;
    public sealed record Sampled(ProfileProvenance Provenance, SplineSampler Sampler, int Points) : ProfileRepair;
    public sealed record Densified(
        ProfileProvenance Provenance, double ErrorBound, int SourceSpans, int OutputSpans) : ProfileRepair;
    public sealed record Topology(
        ProfileProvenance Provenance, int Before, int After, Area BeforeArea, Area AfterArea) : ProfileRepair;

    public ProfileRepair Stamped(Func<ProfileProvenance, ProfileProvenance> rewrite) => Switch(
        state: rewrite,
        joined: static (stamp, arm) => (ProfileRepair)(arm with { Provenance = stamp(arm.Provenance) }),
        closed: static (stamp, arm) => (ProfileRepair)(arm with { Provenance = stamp(arm.Provenance) }),
        cleaned: static (stamp, arm) => (ProfileRepair)(arm with { Provenance = stamp(arm.Provenance) }),
        sampled: static (stamp, arm) => (ProfileRepair)(arm with { Provenance = stamp(arm.Provenance) }),
        densified: static (stamp, arm) => (ProfileRepair)(arm with { Provenance = stamp(arm.Provenance) }),
        topology: static (stamp, arm) => (ProfileRepair)(arm with { Provenance = stamp(arm.Provenance) }));
}

public sealed record ProfileLowered(
    Seq<ProfileContour> Contours, Seq<ProfileMarking> Markings, Seq<ProfileRepair> Repairs) {
    public static readonly ProfileLowered Empty =
        new(Seq<ProfileContour>(), Seq<ProfileMarking>(), Seq<ProfileRepair>());

    public ProfileLowered Concat(ProfileLowered other) => new(
        Contours.Concat(other.Contours), Markings.Concat(other.Markings), Repairs.Concat(other.Repairs));

    public Fin<ProfileLowered> Translate(Vector3d delta, int row, int column) => Contours
            .Traverse(contour => (delta.IsZero
                ? Fin.Succ(contour.Loop)
                : Loop.Admit(
                    contour.Loop.Vertices.Map(vertex => vertex + delta),
                    contour.Loop.Closed, contour.Loop.Bulges, contour.Loop.Tolerance))
                .Map(loop => contour with { Loop = loop, Provenance = Replica(contour.Provenance, row, column) })
                .ToValidation()).As().ToFin()
            .Map(contours => new ProfileLowered(
                contours,
                Markings.Map(marking => marking with {
                    At = delta.IsZero ? marking.At : marking.At + delta,
                    Content = delta.IsZero ? marking.Content : marking.Content.Shift(delta),
                    Provenance = Replica(marking.Provenance, row, column),
                }),
                Repairs.Map(repair => repair.Stamped(provenance => Replica(provenance, row, column)))));

    private static ProfileProvenance Replica(ProfileProvenance provenance, int row, int column) => provenance with {
        Blocks = provenance.Blocks.IsEmpty
            ? provenance.Blocks
            : provenance.Blocks.Take(provenance.Blocks.Count - 1)
                .Add(provenance.Blocks.Last with { Row = row, Column = column }),
    };
}

internal readonly record struct HatchSpan(Point3d Start, Point3d End, double Bulge);

public sealed record ProfileRegion(ProfileProvenance Provenance, RegionTopology Topology);

public sealed record ProfileUnitEvidence(
    UnitsType Declared,
    ProfileUnitPolicy Resolution,
    LengthUnit Canonical,
    double MillimeterScale);

public sealed record ImportedProfile(
    UInt128 SourceDigest,
    ProfileFormat Format,
    ProfileCensus Census,
    ProfileUnitEvidence Units,
    Arr<ProfileContour> Contours,
    Arr<ProfileMarking> Markings,
    Arr<ProfileRegion> Regions,
    BoundingBox Extents,
    Seq<ProfileRepair> Repairs) {
    public Arr<Loop> Loops => Contours.Map(static contour => contour.Loop);
}

public readonly record struct EntityLowering(
    Entity Subject,
    int Ordinal,
    Seq<ProfileBlock> Blocks,
    ProfilePolicy Policy,
    ProfilePath Path,
    double Scale);

public static partial class ProfileImport {
    private static readonly FrozenDictionary<Type, Func<EntityLowering, Fin<ProfileLowered>>> Arms =
        Seq(
            Row<LwPolyline>(static (row, at) => Contour(at,
                row.Vertices.Map(vertex => Ocs(row.Normal, vertex.Location, row.Elevation, at.Scale)).ToArr(),
                row.IsClosed, Bulges(row.Vertices.Map(static vertex => vertex.Bulge), row.Normal))),
            Row<Polyline2D>(static (row, at) => Contour(at,
                row.Vertices.Map(vertex =>
                    Ocs(row.Normal, new XY(vertex.Location.X, vertex.Location.Y), row.Elevation, at.Scale)).ToArr(),
                row.IsClosed, Bulges(row.Vertices.Map(static vertex => vertex.Bulge), row.Normal))),
            Row<CadLine>(static (row, at) => Contour(at,
                Arr(Point(row.StartPoint, at.Scale), Point(row.EndPoint, at.Scale)), false, Arr(0d, 0d))),
            Row<CadArc>(static (row, at) => Planar(row.Normal, at)
                .Bind(_ => ArcLoop(row, at))
                .Bind(loop => Wrapped(at, loop))),
            Row<CadCircle>(static (row, at) => Planar(row.Normal, at)
                .Bind(_ => CircleLoop(row.Normal, row.Center, row.Radius, at))
                .Bind(loop => Wrapped(at, loop))),
            Row<CadEllipse>(static (row, at) => Planar(row.Normal, at)
                .Bind(_ => row.IsFullEllipse && row.RadiusRatio == 1d
                    ? CircleLoop(row.Normal, row.Center, row.MajorAxisEndPoint.GetLength(), at)
                    : CurveLoop(row.PolygonalVertexes(at.Policy.Spline.Value), row.IsFullEllipse, at))
                .Bind(loop => Wrapped(at, loop))),
            Row<CadSpline>(static (row, at) => SplineLoop(row, at)
                .Bind(sampled => Wrapped(at, sampled.Loop).Map(lowered => lowered with {
                    Repairs = Seq<ProfileRepair>(new ProfileRepair.Sampled(
                        Provenance(at, sampled.Loop.Plane), sampled.Sampler, sampled.Points)),
                }))),
            Row<Hatch>(static (row, at) => HatchContours(row, at)),
            Row<CadPoint>(static (row, at) => Fin.Succ(Marked(at,
                Point(row.Location, at.Scale), row.Rotation, MarkingContent.Mark, row.Location.Z * at.Scale))),
            Row<AttributeEntity>(static (row, at) =>
                MarkingAnchor.Of(row.HorizontalAlignment, row.VerticalAlignment)
                    .ToFin(Fault(at.Path, row, "profile-marking:anchor"))
                    .Map(anchor => Marked(at, Point(row.InsertPoint, at.Scale), row.Rotation,
                        new MarkingContent.Tag(row.Tag, Typography(row, TagLines(row), anchor, at.Scale),
                            MarkingFlag.Of(row.Flags)),
                        row.InsertPoint.Z * at.Scale))),
            Row<CadText>(static (row, at) =>
                MarkingAnchor.Of(row.HorizontalAlignment, row.VerticalAlignment)
                    .ToFin(Fault(at.Path, row, "profile-marking:anchor"))
                    .Map(anchor => Marked(at, Point(row.InsertPoint, at.Scale), row.Rotation,
                        new MarkingContent.Text(
                            Typography(row, Seq(row.Value), anchor, at.Scale),
                            MarkingFit.Of(row.HorizontalAlignment), Point(row.AlignmentPoint, at.Scale)),
                        row.InsertPoint.Z * at.Scale))),
            Row<MText>(static (row, at) => MarkingAnchor.Of(row.AttachmentPoint)
                .ToFin(Fault(at.Path, row, "profile-marking:anchor"))
                .Map(anchor => Marked(at, Point(row.InsertPoint, at.Scale), row.Rotation,
                    new MarkingContent.Paragraph(
                        Typography(row, toSeq(row.GetPlainTextLines()), anchor, at.Scale),
                        Length.FromMillimeters(row.RectangleWidth * at.Scale), row.LineSpacing),
                    row.InsertPoint.Z * at.Scale))),
            Row<Insert>(static (row, at) => Insertion(row, at)))
        .ToDictionary(static row => row.Subject, static row => row.Lower)
        .ToFrozenDictionary();

    public static Eff<ImportedProfile> Read(ProfileSource source) => Eff.lift(() =>
        from raw in ReadOp.Catch(() => Fin.Succ(File.ReadAllBytes(source.Path.Value)))
            .MapFail(error => Classify(source.Path, error))
        from format in ProfileFormat.Admit(source.Path)
        from result in Capture(source.Path, notices =>
            from document in Open(format, source, raw, notices)
            from _reject in Reject(notices, source.Policy.Reader, source.Path)
            from _acyclic in Acyclic(document, source.Path)
            from scale in Scale(document.Header.InsUnits, source.Policy.Units, source.Path)
            let entities = toSeq(document.Entities).Strict()
            from lowered in Entities(entities, source.Policy, source.Path, scale.Scale)
            from repaired in ProfileTopology.Repair(lowered.Contours, source.Policy)
            let census = Census(
                format, source, document.Header.InsUnits, LayerNames(document), entities, notices.Value)
            select new ImportedProfile(
                ContentHash.Of(raw), format, census,
                new ProfileUnitEvidence(scale.Evidence, source.Policy.Units, LengthUnit.Millimeter, scale.Scale),
                repaired.Contours, lowered.Markings.ToArr(), repaired.Regions,
                Extents(repaired.Contours, lowered.Markings), lowered.Repairs.Concat(repaired.Repairs)))
        select result)
        .Bind(static result => result.ToEff());

    private static Fin<Unit> Acyclic(CadDocument document, ProfilePath path) =>
        toSeq(document.BlockRecords)
            .Bind(record => toSeq(record.Entities)
                .Choose(entity => entity is Insert insert
                    ? Some(new SEdge<ulong>(record.Handle, insert.Block.Handle))
                    : Option<SEdge<ulong>>.None))
            .Distinct()
            .IsDirectedAcyclicGraph()
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.Inadmissible(FabConcern.Ingress, "profile-block:cycle"));

    private static (Type Subject, Func<EntityLowering, Fin<ProfileLowered>> Lower) Row<TEntity>(
        Func<TEntity, EntityLowering, Fin<ProfileLowered>> lower)
        where TEntity : Entity =>
        (typeof(TEntity), state => lower((TEntity)state.Subject, state));

    private static Option<Func<EntityLowering, Fin<ProfileLowered>>> Resolved(Type? subject) =>
        subject is null
            ? None
            : Arms.TryGetValue(subject, out Func<EntityLowering, Fin<ProfileLowered>>? arm)
                ? Some(arm)
                : Resolved(subject.BaseType);

    private static BoundingBox Extents(Arr<ProfileContour> contours, Seq<ProfileMarking> markings) =>
        markings.Map(static marking => new BoundingBox(marking.At, marking.At))
            .Concat(contours.Map(static contour => contour.Loop.Bound()).ToSeq())
            .Fold(BoundingBox.Empty, static (bounds, next) => BoundingBox.Union(bounds, next));

    private static Fin<ProfileLowered> Entities(
        Seq<Entity> entities, ProfilePolicy policy, ProfilePath path, double scale) =>
        entities
            .Map(static (entity, ordinal) => (Entity: entity, Ordinal: ordinal))
            .Traverse(row => Lower(
                new EntityLowering(row.Entity, row.Ordinal, Seq<ProfileBlock>(), policy, path, scale))
                .ToValidation()).As().ToFin()
            .Map(static rows => rows.Fold(ProfileLowered.Empty, static (state, row) => state.Concat(row)));

    private static Fin<ProfileLowered> Lower(EntityLowering at) =>
        at.Policy.Lane(at.Subject.Layer.Name) is { Contributes: false }
            ? Fin.Succ(ProfileLowered.Empty)
            : Resolved(at.Subject.GetType()).Match(
                Some: arm => arm(at),
                None: () => at.Policy.Unsupported.Switch(
                    state: at,
                    ignore: static (_, _) => Fin.Succ(ProfileLowered.Empty),
                    reject: static (state, _) => Fin.Fail<ProfileLowered>(
                        Fault(state.Path, state.Subject, "profile-entity:unsupported"))));

    private static Fin<ProfileLowered> HatchContours(Hatch row, EntityLowering at) =>
        Planar(row.Normal, at)
            .Bind(_ => ReadOp.Catch(() => Fin.Succ(
                toSeq(row.Paths).Map((boundary, index) => (boundary, index)).Strict())))
            .Bind(boundaries => boundaries.Traverse(item =>
                ReadOp.Catch(() => Fin.Succ(item.boundary.Edges.ToSeq()
                    .Bind(edge => HatchEdge(row, edge, at.Policy.Spline.Value, at.Scale)).Strict()))
                    .Bind(spans => spans.IsEmpty
                        ? Fin.Fail<Loop>(Fault(at.Path, row, $"hatch:{item.index}:empty"))
                        : Loop.Admit(
                            spans.Map(static span => span.Start).ToArr(),
                            closed: true, spans.Map(static span => span.Bulge).ToArr(), at.Policy.Tolerance))
                    .Map(loop => new ProfileContour(loop, Provenance(at, loop.Plane)))
                    .ToValidation()).As().ToFin())
            .Map(static contours => new ProfileLowered(
                contours.ToSeq(), Seq<ProfileMarking>(), Seq<ProfileRepair>()));

    private static Seq<HatchSpan> HatchEdge(
        Hatch hatch, Hatch.BoundaryPath.Edge edge, int precision, double scale) => edge switch {
        HatchLine line => Seq(new HatchSpan(
            Ocs(hatch.Normal, line.Start, hatch.Elevation, scale),
            Ocs(hatch.Normal, line.End, hatch.Elevation, scale), 0.0)),
        HatchArc arc => HatchArcSpans(hatch, arc, scale),
        HatchPolyline polyline => HatchPolylineSpans(hatch, polyline, scale),
        HatchEllipse ellipse => HatchSampled(hatch, ellipse.PolygonalVertexes(precision), scale),
        HatchSpline spline => HatchSampled(hatch, spline.PolygonalVertexes(precision), scale),
        _ => Seq<HatchSpan>(),
    };

    private static Seq<HatchSpan> HatchArcSpans(Hatch hatch, HatchArc arc, double scale) {
        double sweep = HatchSweep(arc.StartAngle, arc.EndAngle, arc.CounterClockWise);
        int parts = Math.Abs(sweep) == Math.Tau ? 4 : 1;
        double step = sweep / parts;
        return Range(0, parts).ToSeq().Map(index => {
            double from = arc.StartAngle + index * step;
            double to = from + step;
            return new HatchSpan(
                Ocs(hatch.Normal, new XY(arc.Center.X + Math.Cos(from) * arc.Radius, arc.Center.Y + Math.Sin(from) * arc.Radius), hatch.Elevation, scale),
                Ocs(hatch.Normal, new XY(arc.Center.X + Math.Cos(to) * arc.Radius, arc.Center.Y + Math.Sin(to) * arc.Radius), hatch.Elevation, scale),
                Math.Tan(step / 4.0) * Math.Sign(hatch.Normal.Z));
        });
    }

    private static double HatchSweep(double start, double end, bool counterClockwise) {
        double magnitude = counterClockwise
            ? (end - start + Math.Tau) % Math.Tau
            : (start - end + Math.Tau) % Math.Tau;
        double turn = magnitude == 0.0 ? Math.Tau : magnitude;
        return counterClockwise ? turn : -turn;
    }

    private static Seq<HatchSpan> HatchPolylineSpans(Hatch hatch, HatchPolyline polyline, double scale) {
        XYZ[] vertices = polyline.Vertices.ToArray();
        double[] bulges = polyline.Bulges.ToArray();
        int spans = polyline.IsClosed && vertices.Length > 1
            ? vertices.Length
            : Math.Max(0, vertices.Length - 1);
        if (bulges.Length < spans)
            throw new InvalidDataException($"hatch-polyline:bulges:{bulges.Length}:{spans}");
        return Range(0, spans).ToSeq().Map(index => new HatchSpan(
            Ocs(hatch.Normal, new XY(vertices[index].X, vertices[index].Y), hatch.Elevation, scale),
            Ocs(hatch.Normal, new XY(vertices[(index + 1) % vertices.Length].X, vertices[(index + 1) % vertices.Length].Y), hatch.Elevation, scale),
            bulges[index] * Math.Sign(hatch.Normal.Z)));
    }

    private static Seq<HatchSpan> HatchSampled(Hatch hatch, IEnumerable<XYZ> source, double scale) {
        Seq<XYZ> points = toSeq(source);
        return points.Zip(points.Skip(1)).Map(pair => new HatchSpan(
            Ocs(hatch.Normal, new XY(pair.First.X, pair.First.Y), hatch.Elevation + pair.First.Z, scale),
            Ocs(hatch.Normal, new XY(pair.Second.X, pair.Second.Y), hatch.Elevation + pair.Second.Z, scale), 0.0));
    }

    private static Fin<ProfileLowered> Insertion(Insert row, EntityLowering at) =>
        ReadOp.Catch(() => Fin.Succ(toSeq(row.Explode()).Strict()
                .Concat(toSeq(row.Attributes).Map(static attribute => (Entity)attribute)).Strict()))
            .Bind(children => children
                .Map(static (child, index) => (Child: child, Ordinal: index))
                .Traverse(child => Lower(at with {
                    Subject = child.Child,
                    Ordinal = child.Ordinal,
                    Blocks = at.Blocks.Add(new ProfileBlock(row.Block.Name, at.Ordinal, Row: 0, Column: 0)),
                }).ToValidation()).As().ToFin())
            .Map(rows => rows.Fold(ProfileLowered.Empty, static (state, part) => state.Concat(part)))
            .Bind(placed => Replicas(row, at.Scale)
                .Traverse(replica => placed
                    .Translate(replica.Delta, replica.Row, replica.Column).ToValidation()).As().ToFin()
                .Map(static replicas => replicas.Fold(
                    ProfileLowered.Empty, static (state, replica) => state.Concat(replica))));

    private static Seq<(Vector3d Delta, int Row, int Column)> Replicas(Insert row, double scale) {
        Matrix3 frame = Matrix3.ArbitraryAxis(row.Normal) * Matrix3.RotationZ(row.Rotation);
        return Range(0, row.RowCount).ToSeq().Bind(rowIndex => Range(0, row.ColumnCount).ToSeq().Map(columnIndex => {
            XYZ offset = frame * new XYZ(columnIndex * row.ColumnSpacing, rowIndex * row.RowSpacing, 0d);
            return (new Vector3d(offset.X * scale, offset.Y * scale, offset.Z * scale), rowIndex, columnIndex);
        }));
    }

    private static Fin<ProfileLowered> Contour(EntityLowering at, Arr<Point3d> points, bool closed, Arr<double> bulges) =>
        Loop.Admit(points, closed, bulges, at.Policy.Tolerance).Bind(loop => Wrapped(at, loop));

    private static Fin<ProfileLowered> Wrapped(EntityLowering at, Loop loop) =>
        Fin.Succ(new ProfileLowered(
            Seq(new ProfileContour(loop, Provenance(at, loop.Plane))), Seq<ProfileMarking>(), Seq<ProfileRepair>()));

    private static ProfileLowered Marked(
        EntityLowering at, Point3d point, double rotation, MarkingContent content, double plane) =>
        new(Seq<ProfileContour>(),
            Seq(new ProfileMarking(point, rotation, content, Provenance(at, plane))),
            Seq<ProfileRepair>());

    private static MarkingType Typography(IText text, Seq<string> lines, MarkingAnchor anchor, double scale) =>
        new(lines,
            Length.FromMillimeters(text.Height * scale),
            Math.Abs(text.Style.ObliqueAngle) >= double.DegreesToRadians(7.5)
                ? LetteringForm.TypeBItalic
                : LetteringForm.TypeB,
            text.Style.Name,
            anchor);

    private static Seq<string> TagLines(AttributeEntity attribute) => Optional(attribute.MText)
        .Filter(_ => attribute.AttributeType != AttributeType.SingleLine)
        .Map(static text => toSeq(text.GetPlainTextLines()))
        .IfNone(() => Seq(attribute.Value));

    private static Fin<(Loop Loop, SplineSampler Sampler, int Points)> SplineLoop(CadSpline row, EntityLowering at) {
        int density = at.Policy.Spline.Value;
        Seq<XYZ> walked = Range(0, density).ToSeq()
            .Map(index => (double)index / (density - 1))
            .Choose(parameter => row.TryPointOnSpline(parameter, out XYZ point) ? Some(point) : Option<XYZ>.None)
            .Strict();
        return walked.Count == density
            ? CurveLoop(walked, row.IsClosed, at).Map(loop => (loop, SplineSampler.Parametric, walked.Count))
            : row.TryPolygonalVertexes(density, out List<XYZ> points)
                ? CurveLoop(points, row.IsClosed, at).Map(loop => (loop, SplineSampler.Tessellated, points.Count))
                : row.UpdateFromFitPoints() && row.TryPolygonalVertexes(density, out points)
                    ? CurveLoop(points, row.IsClosed, at).Map(loop => (loop, SplineSampler.Refit, points.Count))
                    : Fin.Fail<(Loop, SplineSampler, int)>(Fault(at.Path, at.Subject, "profile-spline:untessellated"));
    }

    private static ProfileProvenance Provenance(EntityLowering at, double plane) => new(
        at.Subject.Layer.Name, at.Policy.Lane(at.Subject.Layer.Name),
        AciIndex.Of(at.Subject.Color.Index).ToOption(),
        at.Subject.Handle, at.Blocks, plane, Set(at.Ordinal));

    private static Fin<Loop> ArcLoop(CadArc arc, EntityLowering at) {
        arc.GetEndVertices(out XYZ start, out XYZ end);
        return Loop.Admit(
            Arr(Point(start, at.Scale), Point(end, at.Scale)), false,
            Arr(Math.Tan(-arc.Sweep / 4d) * Math.Sign(arc.Normal.Z), 0d), at.Policy.Tolerance);
    }

    private static Fin<Loop> CircleLoop(XYZ normal, XYZ center, double radius, EntityLowering at) {
        double bulge = (Math.Sqrt(2d) - 1d) * Math.Sign(normal.Z);
        Matrix3 frame = Matrix3.ArbitraryAxis(normal);
        return Loop.Admit(
            Arr(
                Point(frame * new XYZ(center.X + radius, center.Y, center.Z), at.Scale),
                Point(frame * new XYZ(center.X, center.Y + radius, center.Z), at.Scale),
                Point(frame * new XYZ(center.X - radius, center.Y, center.Z), at.Scale),
                Point(frame * new XYZ(center.X, center.Y - radius, center.Z), at.Scale)),
            true,
            Arr(bulge, bulge, bulge, bulge),
            at.Policy.Tolerance);
    }

    private static Fin<Loop> CurveLoop(IEnumerable<XYZ> points, bool closed, EntityLowering at) =>
        Loop.Admit(toSeq(points).Map(point => Point(point, at.Scale)).ToArr(), closed, Arr<double>(), at.Policy.Tolerance);

    private static Fin<Unit> Planar(XYZ normal, EntityLowering at) {
        Context tolerance = at.Policy.Tolerance;
        return Math.Abs(normal.X) <= Math.Sin(tolerance.Angle.Value)
            && Math.Abs(normal.Y) <= Math.Sin(tolerance.Angle.Value)
            && Math.Abs(Math.Abs(normal.Z) - 1d) <= 1d - Math.Cos(tolerance.Angle.Value)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(Fault(at.Path, at.Subject, "profile-entity:non-planar"));
    }

    private static Arr<double> Bulges(IEnumerable<double> bulges, XYZ normal) =>
        toSeq(bulges).Map(bulge => bulge * Math.Sign(normal.Z)).ToArr();

    private static Point3d Ocs(XYZ normal, XY point, double elevation, double scale) =>
        Point(Matrix3.ArbitraryAxis(normal) * new XYZ(point.X, point.Y, elevation), scale);

    private static Point3d Point(XYZ point, double scale) => new(point.X * scale, point.Y * scale, point.Z * scale);
}

public static class ProfileTopology {
    public sealed record RepairedProfile(
        Arr<ProfileContour> Contours,
        Arr<ProfileRegion> Regions,
        Seq<ProfileRepair> Repairs);

    private readonly record struct Stitched(Seq<ProfileContour> Contours, Seq<ProfileRepair> Repairs);

    private readonly record struct Normalized(
        Seq<ProfileContour> Contours, Option<ProfileRegion> Region, Seq<ProfileRepair> Repairs);

    private readonly record struct Link(int Left, int Right, double Distance);

    public static Fin<RepairedProfile> Repair(Seq<ProfileContour> contours, ProfilePolicy policy) => policy.Closure.Switch(
        state: (Contours: contours, Policy: policy),
        open: static (state, _) => Normalize(state.Contours, state.Policy, demandClosed: false, gap: 0d),
        exact: static (state, _) => state.Contours.IsEmpty
            ? Fin.Fail<RepairedProfile>(Degenerate("profile-topology:empty"))
            : Normalize(state.Contours, state.Policy, demandClosed: true, gap: 0d),
        healed: static (state, closure) => state.Contours.IsEmpty
            ? Fin.Fail<RepairedProfile>(Degenerate("profile-topology:empty"))
            : Normalize(state.Contours, state.Policy, demandClosed: true, closure.MaxGap.Value.Millimeters));

    private static Fin<RepairedProfile> Normalize(
        Seq<ProfileContour> contours, ProfilePolicy policy, bool demandClosed, double gap) =>
        from stitched in Stitch(contours, policy.Tolerance, gap)
        from closed in stitched.Contours.Find(row => demandClosed && row.Provenance.Lane.Closes && !row.Loop.Closed)
            .Match(
                Some: row => Fin.Fail<Seq<ProfileContour>>(
                    new FabricationFault.OpenLoop(FabConcern.Ingress, row.Loop.Count)),
                None: () => Fin.Succ(stitched.Contours))
        from groups in toSeq(closed.GroupBy(Key))
            .Traverse(group => NormalizeGroup(toSeq(group), policy).ToValidation()).As().ToFin()
        select new RepairedProfile(
            groups.Bind(static group => group.Contours).ToArr(),
            groups.Map(static group => group.Region).Somes().ToArr(),
            stitched.Repairs.Concat(groups.Bind(static group => group.Repairs)));

    private static (ProfileProvenance Provenance, bool Closed) Key(ProfileContour contour) =>
        (contour.Provenance with { Ordinals = Set<int>(), Handle = 0ul }, contour.Loop.Closed);

    private static Fin<Normalized> NormalizeGroup(Seq<ProfileContour> rows, ProfilePolicy policy) =>
        rows.Head.Filter(static head => head.Provenance.Lane.Closes && head.Loop.Closed).Match(
        Some: head =>
          from forest in ArcForest
              .Admit(rows.Map(static row => row.Loop), policy.Tolerance, head.Loop.Plane)
          from cleaned in ArcAlgebra.Apply(new ArcOp.Clean(forest))
          from evidence in cleaned switch {
              ArcTrace.Forest arm => Fin.Succ(arm),
              _ => Fin.Fail<ArcTrace.Forest>(Degenerate("profile-topology:clean")),
          }
          from lowered in evidence.Geometry.Loops
              .Traverse(loop => ArcAlgebra
                  .Densify(new ArcProjection.Lower(loop, policy.Tolerance.Absolute.Value))
                  .Bind(static trace => trace.Lowering(Inadmissible("profile-topology:densify")))
                  .ToValidation()).As().ToFin()
          from trace in PolygonAlgebra.Apply(
              new PolygonOp.Topology(lowered.Map(static evidence => evidence.Output), policy.Fill))
          from topology in trace.Regioned(Inadmissible("profile-topology:projection"))
          let provenance = head.Provenance
          let admitted = evidence.Geometry.Loops
          select new Normalized(
              admitted.Map(loop => new ProfileContour(loop, provenance)),
              Some(new ProfileRegion(provenance, topology)),
              Cleanup(provenance, rows, evidence.Evidence)
                  .Concat(lowered.Map(result => Densification(provenance, result)).Somes())
                  .Concat(Areas(provenance, rows, admitted))),
        None: () => Fin.Succ(new Normalized(rows, Option<ProfileRegion>.None, Seq<ProfileRepair>())));

    private static Fin<Stitched> Stitch(Seq<ProfileContour> contours, Context tolerance, double gap) =>
        toSeq(contours.GroupBy(Key))
            .Traverse(group => StitchLane(toSeq(group), tolerance, gap).ToValidation()).As().ToFin()
            .Map(static groups => new Stitched(
                groups.Bind(static group => group.Contours),
                groups.Bind(static group => group.Repairs)));

    private static Fin<Stitched> StitchLane(Seq<ProfileContour> group, Context tolerance, double gap) =>
        group.Head.Exists(static head => head.Provenance.Lane.Closes)
            ? StitchGroup(group, tolerance, gap)
            : Fin.Succ(new Stitched(group, Seq<ProfileRepair>()));

    private static Fin<Stitched> StitchGroup(Seq<ProfileContour> group, Context tolerance, double gap) {
        Seq<Loop> loops = group.Map(static row => row.Loop);
        if (Branching(loops, gap))
            return Fin.Fail<Stitched>(Degenerate("profile-topology:branch"));

        Seq<Link> links = Links(loops, tolerance, gap);
        ForestDisjointSet<int> forest = new(loops.Count);
        toSeq(Range(0, loops.Count)).Iter(forest.MakeSet);
        links.Iter(link => forest.Union(link.Left, link.Right));

        Map<int, Seq<int>> adjacency = links
            .Bind(static link => Seq((From: link.Left, To: link.Right), (From: link.Right, To: link.Left)))
            .Fold(Map<int, Seq<int>>(), static (index, pair) =>
                index.AddOrUpdate(pair.From, index.Find(pair.From).IfNone(Seq<int>()).Add(pair.To)));

        return group.Head
            .ToFin(Degenerate("profile-topology:group-empty"))
            .Bind(lead => toSeq(toSeq(Range(0, loops.Count)).GroupBy(forest.FindSet))
                .Traverse(component => Merge(
                    Ordered(toSeq(component), adjacency).Map(index => (Loop: loops[index], Ordinals: group[index].Provenance.Ordinals)),
                    tolerance, gap).ToValidation()).As().ToFin()
                .Map(chains => Sealed(chains, lead.Provenance, group, tolerance, gap)));
    }

    private static Seq<Link> Links(Seq<Loop> loops, Context tolerance, double gap) =>
        toSeq(Range(0, loops.Count)).Bind(left =>
            toSeq(Range(left + 1, loops.Count - left - 1))
                .Choose(right => Join(loops[left], loops[right], tolerance, gap)
                    .Map(joined => new Link(left, right, joined.Distance))));

    private static Seq<int> Ordered(Seq<int> members, Map<int, Seq<int>> adjacency) {
        int head = members
            .Find(member => adjacency.Find(member).Map(static row => row.Count).IfNone(0) <= 1)
            .Match(Some: identity, None: () => members.Head.IfNone(0));
        return toSeq(Range(1, Math.Max(members.Count - 1, 0)))
            .Fold(
                (Order: Seq(head), Current: head, Visited: Set(head)),
                (state, _) => adjacency.Find(state.Current)
                    .Bind(row => row.Find(next => !state.Visited.Contains(next)))
                    .Match(
                        Some: next => (state.Order.Add(next), next, state.Visited.Add(next)),
                        None: () => state))
            .Order;
    }

    private static Fin<(Loop Loop, Set<int> Ordinals, double Distance, int Joins)> Merge(
        Seq<(Loop Loop, Set<int> Ordinals)> ordered, Context tolerance, double gap) =>
        ordered.Head
            .ToFin(Degenerate("profile-topology:component-empty"))
            .Bind(head => ordered.Tail.Fold(
                Fin.Succ((head.Loop, head.Ordinals, Distance: 0d, Joins: 0)),
                (state, next) => state.Bind(held => Join(held.Loop, next.Loop, tolerance, gap)
                    .ToFin(Degenerate("profile-topology:join"))
                    .Map(joined => (joined.Loop, held.Ordinals.Union(next.Ordinals),
                        held.Distance + joined.Distance, held.Joins + 1)))));

    private static Stitched Sealed(
        Seq<(Loop Loop, Set<int> Ordinals, double Distance, int Joins)> chains,
        ProfileProvenance lead,
        Seq<ProfileContour> group,
        Context tolerance,
        double gap) {
        ProfileProvenance provenance = lead with {
            Ordinals = toSet(group.Bind(static row => row.Provenance.Ordinals)),
        };
        int joins = chains.Fold(0, static (count, chain) => count + chain.Joins);
        double distance = chains.Fold(0d, static (sum, chain) => sum + chain.Distance);
        Seq<(Loop Loop, Set<int> Ordinals, Option<double> Gap)> closures = chains
            .Map(chain => Seal(chain.Loop, tolerance, gap).Match(
                Some: loop => (loop, chain.Ordinals, Some(Span(chain.Loop))),
                None: () => (chain.Loop, chain.Ordinals, Option<double>.None)));
        return new Stitched(
            closures.Map(chain => new ProfileContour(chain.Loop, provenance with { Ordinals = chain.Ordinals })),
            (joins == 0
                ? Seq<ProfileRepair>()
                : Seq<ProfileRepair>(new ProfileRepair.Joined(
                    provenance, Length.FromMillimeters(distance), joins)))
                .Concat(closures.Map(chain => chain.Gap.Map(value =>
                    (ProfileRepair)new ProfileRepair.Closed(provenance, Length.FromMillimeters(value)))).Somes()));
    }

    private static Option<Loop> Seal(Loop loop, Context tolerance, double gap) =>
        !loop.Closed && loop.Count >= 3 && Span(loop) <= gap
            ? Loop.Admit(loop.Vertices, true, loop.Bulges, tolerance).ToOption()
            : None;

    private static double Span(Loop loop) => loop.Vertices[0].DistanceTo(loop.Vertices[loop.Count - 1]);

    private static Option<(Loop Loop, double Distance)> Join(Loop left, Loop right, Context tolerance, double gap) =>
        left.Closed || right.Closed || Math.Abs(left.Plane - right.Plane) > tolerance.Absolute.Value
        ? None
        : Seq(false, true).Bind(reverseLeft => Seq(false, true).Map(reverseRight => (
                Left: Orient(left, reverseLeft),
                Right: Orient(right, reverseRight))))
            .Choose(static pair => pair.Left.Bind(l => pair.Right.Map(r => (Left: l, Right: r))))
            .Map(pair => (
                pair.Left,
                pair.Right,
                Distance: pair.Left.Vertices[pair.Left.Vertices.Count - 1].DistanceTo(pair.Right.Vertices[0])))
            .Filter(candidate => candidate.Distance <= gap)
            .Fold(Option<(Loop Left, Loop Right, double Distance)>.None, static (best, candidate) =>
                best.Filter(held => held.Distance <= candidate.Distance).IsSome ? best : Some(candidate))
            .Bind(candidate => Loop.Admit(
                candidate.Left.Vertices.ToSeq().Concat(candidate.Right.Vertices.ToSeq().Skip(1)).ToArr(), false,
                candidate.Left.Bulges.ToSeq().Take(candidate.Left.Bulges.Count - 1)
                    .Concat(candidate.Right.Bulges).ToArr(), tolerance)
                .Map(loop => (loop, candidate.Distance)).ToOption());

    private static Option<Loop> Orient(Loop loop, bool reverse) => reverse
        ? Loop.Admit(
            loop.Vertices.Rev().ToArr(),
            false,
            Range(0, loop.Count).ToSeq().Map(index => index == loop.Count - 1
                ? 0d
                : -loop.BulgeAt(loop.Count - 2 - index)).ToArr(),
            loop.Tolerance).ToOption()
        : Some(loop);

    private static bool Branching(Seq<Loop> loops, double gap) {
        Seq<(int Loop, Point3d Point)> endpoints = loops.Map((loop, index) => Seq(
            (index, loop.Vertices[0]),
            (index, loop.Vertices[loop.Vertices.Count - 1]))).Bind(identity);
        return endpoints.Exists(endpoint => endpoints.Count(candidate =>
            candidate.Loop != endpoint.Loop && candidate.Point.DistanceTo(endpoint.Point) <= gap) > 1);
    }

    private static Seq<ProfileRepair> Cleanup(
        ProfileProvenance provenance, Seq<ProfileContour> before, ArcEvidence result) => result switch {
        ArcEvidence.Clean arm when arm.Loops.Count != before.Count
            || arm.Loops.Fold(0, static (sum, evidence) => sum + evidence.OutputSegments)
                != before.Fold(0, static (sum, row) => sum + row.Loop.Spans) =>
            Seq<ProfileRepair>(new ProfileRepair.Cleaned(
                provenance, before.Count, arm.Loops.Count,
                arm.Loops.Fold(0, static (sum, evidence) => sum + evidence.OutputSegments))),
        _ => Seq<ProfileRepair>(),
    };

    private static Option<ProfileRepair> Densification(ProfileProvenance provenance, DensifyEvidence result) =>
        result.SourceSpans == result.OutputSpans
            ? None
            : Some<ProfileRepair>(new ProfileRepair.Densified(
                provenance, result.ErrorBound, result.SourceSpans, result.OutputSpans));

    private static Seq<ProfileRepair> Areas(
        ProfileProvenance provenance, Seq<ProfileContour> before, Seq<Loop> after) =>
        before.Map(static row => row.Loop).Equals(after)
        ? Seq<ProfileRepair>()
        : Seq<ProfileRepair>(new ProfileRepair.Topology(
            provenance, before.Count, after.Count,
            Area.FromSquareMillimeters(before.Fold(0d, static (sum, row) => sum + Math.Abs(row.Loop.Area()))),
            Area.FromSquareMillimeters(after.Fold(0d, static (sum, loop) => sum + Math.Abs(loop.Area())))));

    private static Error Degenerate(string locus) =>
        new GeometryFault.DegenerateInput(Kind.Curve, None, locus);

    private static FabricationFault Inadmissible(string locus) =>
        FabricationFault.Inadmissible(FabConcern.Ingress, locus);
}
```

## [04]-[PROJECTION_EGRESS]

- Owner: `ProfileProjection` is the closed egress row carrying its own view delegate, and `ProfileView` carries each row's result shape.
- Cases: loops · lanes · layers · regions · markings · tags · bounds · repairs · census.
- Entry: `ProfileProjection.<row>.Project(ImportedProfile)` — the row is the dispatch.
- Auto: the lane and layer views group the admitted contours through one provenance-keyed fold, so a grouping axis is a key selector rather than a second projection body; the tag view keys placed attributes by their own name through `ProfileMarking.Tag`, so a traveler or posted program resolves a part mark or heat number by name rather than casting through the content family.
- Growth: a new egress is one `ProfileProjection` row carrying its delegate and one `ProfileView` case.
- Boundary: callers already holding `ImportedProfile` consume it directly; projections expose only derived views.

```csharp
// --- [PROJECTION_EGRESS] ---------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileView {
    private ProfileView() { }
    public sealed record Loops(Arr<Loop> Value) : ProfileView;
    public sealed record Lanes(Map<ProfileLane, Arr<ProfileContour>> Value) : ProfileView;
    public sealed record Layers(Map<string, Arr<ProfileContour>> Value) : ProfileView;
    public sealed record Regions(Arr<ProfileRegion> Value) : ProfileView;
    public sealed record Markings(Arr<ProfileMarking> Value) : ProfileView;
    public sealed record Tags(Map<string, Arr<ProfileMarking>> Value) : ProfileView;
    public sealed record Bounds(BoundingBox Value) : ProfileView;
    public sealed record Repairs(Seq<ProfileRepair> Value) : ProfileView;
    public sealed record Census(ProfileCensus Value) : ProfileView;
}

[SmartEnum<string>]
public sealed partial class ProfileProjection {
    public static readonly ProfileProjection Loops = new("loops",
        static result => new ProfileView.Loops(result.Loops));
    public static readonly ProfileProjection Lanes = new("lanes",
        static result => new ProfileView.Lanes(toSeq(result.Contours.GroupBy(static row => row.Provenance.Lane))
            .Map(static group => (group.Key, toSeq(group).ToArr())).ToMap()));
    public static readonly ProfileProjection Layers = new("layers",
        static result => new ProfileView.Layers(toSeq(result.Contours.GroupBy(static row => row.Provenance.Layer))
            .Map(static group => (group.Key, toSeq(group).ToArr())).ToMap()));
    public static readonly ProfileProjection Regions = new("regions",
        static result => new ProfileView.Regions(result.Regions));
    public static readonly ProfileProjection Markings = new("markings",
        static result => new ProfileView.Markings(result.Markings));
    public static readonly ProfileProjection Tags = new("tags",
        static result => new ProfileView.Tags(ProfileImport.TagsOf(result.Markings)));
    public static readonly ProfileProjection Bounds = new("bounds",
        static result => new ProfileView.Bounds(result.Extents));
    public static readonly ProfileProjection Repairs = new("repairs",
        static result => new ProfileView.Repairs(result.Repairs));
    public static readonly ProfileProjection Census = new("census",
        static result => new ProfileView.Census(result.Census));
    [UseDelegateFromConstructor]
    public partial ProfileView Project(ImportedProfile result);
}

public static partial class ProfileImport {
    public static Map<string, Arr<ProfileMarking>> TagsOf(Arr<ProfileMarking> markings) =>
        toSeq(markings.ToSeq()
            .Choose(static marking => marking.Tag.Map(name => (Name: name, Marking: marking)))
            .GroupBy(static row => row.Name))
            .Map(static group => (group.Key, toSeq(group).Map(static row => row.Marking).ToArr())).ToMap();
}
```

## [05]-[INGRESS_FOLD]

- Owner: `Ingress` is the sub-domain's one source-to-admitted-geometry dispatch; `IngressSource` closes every admitted raw source and `AdmittedGeometry` closes every admitted result.
- Cases: profile · solid · steel · element on both families, each arm binding its own page's reader and result.
- Entry: `Ingress.Admit(IngressSource)` returns one deferred `Eff<AdmittedGeometry>` — the `S1 Ingress` entry the folder `ARCHITECTURE.md` `[02]-[STRATA]` names.
- Growth: a new admitted source is one `IngressSource` case, one `AdmittedGeometry` case, and one total `Switch` arm bound to that page's reader.
- Boundary: the fold seats beside the profile owner because the sub-domain publishes one entry and earns no page of its own; every arm reaches a sibling page's public reader and none reaches a sibling's interior; `SteelImport.Read` takes its contour policy as a second argument the `IngressSource.Steel` case carries, so the fold never re-decides a page's own policy shape; `ElementImport.Admit` is synchronous and lifts here rather than widening its own signature for one consumer.

```csharp
// --- [INGRESS_FOLD] --------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IngressSource {
    private IngressSource() { }
    public sealed record Profile(ProfileSource Source) : IngressSource;
    public sealed record Solid(SolidSource Source) : IngressSource;
    public sealed record Steel(SteelSource Source, SteelContourPolicy Policy) : IngressSource;
    public sealed record Element(ElementSource Source) : IngressSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdmittedGeometry {
    private AdmittedGeometry() { }
    public sealed record Profiles(ImportedProfile Profile) : AdmittedGeometry;
    public sealed record Mesh(ImportedSolid Solid) : AdmittedGeometry;
    public sealed record Steel(ImportedSteel Import) : AdmittedGeometry;
    public sealed record Elements(ElementAdmission Admission) : AdmittedGeometry;
}

public static class Ingress {
    public static Eff<AdmittedGeometry> Admit(IngressSource source) => source.Switch(
        profile: static arm => ProfileImport.Read(arm.Source)
            .Map(result => (AdmittedGeometry)new AdmittedGeometry.Profiles(result)),
        solid: static arm => SolidImport.Read(arm.Source)
            .Map(result => (AdmittedGeometry)new AdmittedGeometry.Mesh(result)),
        steel: static arm => SteelImport.Read(arm.Source, arm.Policy)
            .Map(result => (AdmittedGeometry)new AdmittedGeometry.Steel(result)),
        element: static arm => Eff.lift(() => ElementImport.Admit(arm.Source))
            .Map(admission => (AdmittedGeometry)new AdmittedGeometry.Elements(admission)));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
