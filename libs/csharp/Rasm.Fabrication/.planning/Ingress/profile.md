# [RASM_FABRICATION_PROFILE_IMPORT]

`ProfileImport` owns DXF/DWG census, admission, topology healing, and projection. `ProfileFormat` dispatches admitted paths, and `ProfilePolicy` carries unit, sampling, lane, fill, entity, notification, and closure decisions. Provider entities lower through OCS frames into provenance-bearing contours and markings. `ProfileTopology` partitions source planes, stitches compatible endpoints, composes `ArcAlgebra` and `PolygonAlgebra`, and records each repair. Public entries defer boundary work on `Eff`.

## [01]-[INDEX]

| [INDEX] | [OWNER]                | [OWNS]                                       |
| :-----: | :--------------------- | :------------------------------------------- |
|  [01]   | `ProfileSource`        | admitted path and import policy              |
|  [02]   | `ProfileLane`          | per-layer fabrication intent and closure duty |
|  [03]   | `ProfileFormat`        | DXF/DWG read, encoding, and census dispatch  |
|  [04]   | `ProfileTopology`      | contour stitching, healing, and nesting      |
|  [05]   | `ProfileImportReceipt` | source, census, repairs, and admitted loops  |
|  [06]   | `ProfileProjection`    | parameterized profile egress                 |
|  [07]   | `Ingress`              | total source-to-admitted-geometry dispatch   |

## [02]-[RAW_ADMISSION]

`ProfileSource` is the only raw profile gate. `ProfileUnitPolicy` resolves source units through `UnitsNet.Length`, and `ProfileUnitPolicy.DeclaredOr` falls back on any unresolvable declaration, not on `UnitsType.Unitless` alone. `ProfileReadPolicy` carries a capability set and rejected notice set so `Failsafe`, unknown-entity retention, and DWG CRC checking are declared values, never literals at the reader call. `ProfileEntityPolicy` owns unsupported entities and `ProfileClosure` owns contour completion.

`ProfileImport.Probe` reads DXF headers, tables, and entities through separate `DxfReader` partial-read instances; `ReadTables().Layers` preserves declared layers with zero entities, while DWG uses its configured full reader because no equivalent partial entity surface exists. `ProfileEncoding.Of` discriminates ASCII and binary DXF through `DxfReader.IsBinary` before either read. Census egress returns encoding, declared units, complete per-layer lane assignment, entity counts, and provider notices before contour admission.

`ProfileLane` carries fabrication intent per layer: cut geometry closes, etch/score/bend/mark geometry contributes open runs, and reference geometry is censused and discarded. `ProfileLanePolicy` resolves an entity's layer name to its lane, so `ProfileClosure` demands closure only from lanes that close and a bend line never fails a healed import.

## [03]-[CANONICAL_OWNER]

`ProfileContour` carries one provider-lowered `Loop` with structural provenance, and `ProfileMarking` carries point locations that admit no loop. `ProfileProvenance` preserves the entity handle, so each fault names its entity and `Validation` reports all rejections. `ProfileBlock` preserves nested insert identity and replica indices.

Entity coordinates lower through their owning frame. `Matrix3.ArbitraryAxis` maps OCS vertices into WCS, and mirrored normals invert bulge sense. One hatch emits one contour per `Paths` row: line, circular-arc, and polyline leaves preserve exact endpoints and bulges; ellipse and spline leaves compose their verified provider samplers. `Insert.Explode` resolves one placement, and every row/column instance records replica provenance even when its displacement is zero.

`ProfileTopology` groups contours by provenance, plane, and closure; joins compatible endpoints; cleans and densifies through `ArcAlgebra`; and derives nesting through `PolygonAlgebra`. `ProfileClosure.Exact` admits no endpoint gap, while `Healed` authorizes its positive gap. Region derivation is a fill-rule query because union under one winding absorbs holes. `ProfileRepair` carries join distance, closure gap, cleanup, densification error, and area delta. Indexed endpoint assembly is the bounded statement kernel.

`ProfileImportReceipt` carries the source digest minted from file bytes, format, census, unit evidence, contours, markings, regions, extents, and typed repairs. `ProfileCensus` owns provider notifications once.

## [04]-[PROJECTION_EGRESS]

`ProfileProjection` is one closed egress family over the canonical receipt, and `ProfileView` carries each case's result shape. A new egress is one case and one total `Switch` arm.

## [05]-[IMPLEMENTATION]

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.IO;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Types.Units;
using CSMath;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
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
using HatchArc = ACadSharp.Entities.Hatch.BoundaryPath.Arc;
using HatchEllipse = ACadSharp.Entities.Hatch.BoundaryPath.Ellipse;
using HatchLine = ACadSharp.Entities.Hatch.BoundaryPath.Line;
using HatchPolyline = ACadSharp.Entities.Hatch.BoundaryPath.Polyline;
using HatchSpline = ACadSharp.Entities.Hatch.BoundaryPath.Spline;

namespace Rasm.Fabrication.Ingress;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct ProfilePath {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "profile-path:blank");
            return;
        }
        value = Path.GetFullPath(value);
    }
}

[ValueObject<int>]
public readonly partial struct SplineDensity {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 2 ? new ValidationError(message: "spline-density:below-two") : null;
}

[ValueObject<Length>]
public readonly partial struct ProfileGap {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Length value) =>
        validationError = double.IsFinite(value.Millimeters) && value.Millimeters > 0d
            ? null
            : new ValidationError(message: "profile-gap:non-positive");
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileEntityPolicy {
    private ProfileEntityPolicy() { }
    public sealed record Ignore : ProfileEntityPolicy;
    public sealed record Reject : ProfileEntityPolicy;
}

[SmartEnum<string>]
public sealed partial class ProfileNoticeKind {
    public static readonly ProfileNoticeKind None = new("none");
    public static readonly ProfileNoticeKind Warning = new("warning");
    public static readonly ProfileNoticeKind Error = new("error");
    public static readonly ProfileNoticeKind NotImplemented = new("not-implemented");
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

public sealed record ProfileLanePolicy(Map<string, ProfileLane> Layers, ProfileLane Fallback) {
    public ProfileLane Resolve(string layer) => Layers
        .Find(row => string.Equals(row.Key, layer, StringComparison.OrdinalIgnoreCase))
        .Map(static row => row.Value)
        .IfNone(Fallback);
}

public sealed record ProfileReadPolicy(
    Set<ProfileReadCapability> Capabilities,
    Set<ProfileNoticeKind> Rejects);

public sealed record ProfilePolicy(
    SplineDensity Spline,
    ProfileUnitPolicy Units,
    ProfileClosure Closure,
    ProfileEntityPolicy Unsupported,
    ProfileReadPolicy Reader,
    ProfileLanePolicy Lanes,
    PolygonFill Fill,
    Context Tolerance);

public sealed record ProfileSource(ProfilePath Path, ProfilePolicy Policy);

[SmartEnum<string>]
public sealed partial class ProfileFormat {
    public static readonly ProfileFormat Dxf = new("dxf", Arr(".dxf"), ReadDxf);
    public static readonly ProfileFormat Dwg = new("dwg", Arr(".dwg"), ReadDwg);

    public Arr<string> Extensions { get; }

    [UseDelegateFromConstructor]
    public partial CadDocument Read(byte[] payload, ProfileReadPolicy policy, NotificationEventHandler sink);

    public static Fin<ProfileFormat> Admit(ProfilePath path) =>
        Items.Find(format => format.Extensions.Exists(extension =>
                string.Equals(extension, Path.GetExtension(path.Value), StringComparison.OrdinalIgnoreCase)))
            .ToFin(ProfileImport.Fault(path));

    static CadDocument ReadDxf(byte[] payload, ProfileReadPolicy policy, NotificationEventHandler sink) =>
        Snapshot(payload, ".dxf", path => DxfReader.Read(path, new DxfReaderConfiguration {
            Failsafe = policy.Capabilities.Contains(ProfileReadCapability.Recover),
            KeepUnknownEntities = policy.Capabilities.Contains(ProfileReadCapability.UnknownEntities),
            KeepUnknownNonGraphicalObjects = policy.Capabilities.Contains(ProfileReadCapability.UnknownObjects),
        }, sink));

    static CadDocument ReadDwg(byte[] payload, ProfileReadPolicy policy, NotificationEventHandler sink) =>
        Snapshot(payload, ".dwg", path => DwgReader.Read(path, new DwgReaderConfiguration {
            Failsafe = policy.Capabilities.Contains(ProfileReadCapability.Recover),
            KeepUnknownEntities = policy.Capabilities.Contains(ProfileReadCapability.UnknownEntities),
            KeepUnknownNonGraphicalObjects = policy.Capabilities.Contains(ProfileReadCapability.UnknownObjects),
            CrcCheck = policy.Capabilities.Contains(ProfileReadCapability.Crc),
        }, sink));

    static T Snapshot<T>(byte[] payload, string extension, Func<string, T> read) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        try {
            File.WriteAllBytes(path, payload);
            return read(path);
        }
        finally {
            File.Delete(path);
        }
    }
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

public readonly record struct ProfileBlock(string Name, int Ordinal, int Row, int Column);

public sealed record ProfileProvenance(
    string Layer,
    ProfileLane Lane,
    short Color,
    ulong Handle,
    Seq<ProfileBlock> Blocks,
    double Plane,
    Set<int> Ordinals);

public sealed record ProfileContour(Loop Loop, ProfileProvenance Provenance);

public sealed record ProfileMarking(Point3d At, double Rotation, ProfileProvenance Provenance);

public sealed record ProfileLowered(Seq<ProfileContour> Contours, Seq<ProfileMarking> Markings) {
    public static readonly ProfileLowered Empty = new(Seq<ProfileContour>(), Seq<ProfileMarking>());

    public ProfileLowered Concat(ProfileLowered other) => new(
        Contours.Concat(other.Contours), Markings.Concat(other.Markings));

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
                    Provenance = Replica(marking.Provenance, row, column),
                })));

    static ProfileProvenance Replica(ProfileProvenance provenance, int row, int column) => provenance with {
        Blocks = provenance.Blocks.IsEmpty
            ? provenance.Blocks
            : provenance.Blocks.Take(provenance.Blocks.Count - 1)
                .Add(provenance.Blocks.Last with { Row = row, Column = column }),
    };
}

internal readonly record struct HatchSpan(Point3d Start, Point3d End, double Bulge);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileRepair {
    private ProfileRepair() { }
    public sealed record Joined(ProfileProvenance Provenance, Length Distance, int Count) : ProfileRepair;
    public sealed record Closed(ProfileProvenance Provenance, Length Gap) : ProfileRepair;
    public sealed record Cleaned(ProfileProvenance Provenance, int Before, int After, int Segments) : ProfileRepair;
    public sealed record Densified(
        ProfileProvenance Provenance, double ErrorBound, int SourceSpans, int OutputSpans, int Simplified) : ProfileRepair;
    public sealed record Topology(
        ProfileProvenance Provenance, int Before, int After, Area BeforeArea, Area AfterArea) : ProfileRepair;
}

public sealed record ProfileRegion(ProfileProvenance Provenance, TopologyReceipt Topology);

public sealed record ProfileUnitEvidence(
    UnitsType Declared,
    ProfileUnitPolicy Resolution,
    LengthUnit Canonical,
    double MillimeterScale);

public sealed record ProfileImportReceipt(
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileProjection {
    private ProfileProjection() { }
    public sealed record Loops : ProfileProjection;
    public sealed record Lanes : ProfileProjection;
    public sealed record Layers : ProfileProjection;
    public sealed record Regions : ProfileProjection;
    public sealed record Markings : ProfileProjection;
    public sealed record Bounds : ProfileProjection;
    public sealed record Repairs : ProfileProjection;
    public sealed record Census : ProfileProjection;
    public sealed record Receipt : ProfileProjection;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileView {
    private ProfileView() { }
    public sealed record Loops(Arr<Loop> Value) : ProfileView;
    public sealed record Lanes(Map<ProfileLane, Arr<ProfileContour>> Value) : ProfileView;
    public sealed record Layers(Map<string, Arr<ProfileContour>> Value) : ProfileView;
    public sealed record Regions(Arr<ProfileRegion> Value) : ProfileView;
    public sealed record Markings(Arr<ProfileMarking> Value) : ProfileView;
    public sealed record Bounds(BoundingBox Value) : ProfileView;
    public sealed record Repairs(Seq<ProfileRepair> Value) : ProfileView;
    public sealed record Census(ProfileCensus Value) : ProfileView;
    public sealed record Receipt(ProfileImportReceipt Value) : ProfileView;
}

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
    public sealed record Profiles(ProfileImportReceipt Receipt) : AdmittedGeometry;
    public sealed record Mesh(SolidImportReceipt Receipt) : AdmittedGeometry;
    public sealed record Steel(SteelImportReceipt Receipt) : AdmittedGeometry;
    public sealed record Elements(ElementAdmission Admission) : AdmittedGeometry;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ProfileImport {
    static readonly FrozenDictionary<UnitsType, LengthUnit> Units = new Dictionary<UnitsType, LengthUnit> {
        [UnitsType.Angstroms] = LengthUnit.Angstrom,
        [UnitsType.Nanometers] = LengthUnit.Nanometer,
        [UnitsType.Microns] = LengthUnit.Micrometer,
        [UnitsType.Millimeters] = LengthUnit.Millimeter,
        [UnitsType.Centimeters] = LengthUnit.Centimeter,
        [UnitsType.Decimeters] = LengthUnit.Decimeter,
        [UnitsType.Meters] = LengthUnit.Meter,
        [UnitsType.Decameters] = LengthUnit.Decameter,
        [UnitsType.Hectometers] = LengthUnit.Hectometer,
        [UnitsType.Kilometers] = LengthUnit.Kilometer,
        [UnitsType.Gigameters] = LengthUnit.Gigameter,
        [UnitsType.Microinches] = LengthUnit.Microinch,
        [UnitsType.Mils] = LengthUnit.Mil,
        [UnitsType.Inches] = LengthUnit.Inch,
        [UnitsType.Feet] = LengthUnit.Foot,
        [UnitsType.Yards] = LengthUnit.Yard,
        [UnitsType.Miles] = LengthUnit.Mile,
        [UnitsType.AstronomicalUnits] = LengthUnit.AstronomicalUnit,
        [UnitsType.LightYears] = LengthUnit.LightYear,
        [UnitsType.Parsecs] = LengthUnit.Parsec,
        [UnitsType.USSurveyFeet] = LengthUnit.UsSurveyFoot,
    }.ToFrozenDictionary();

    public static Eff<ProfileCensus> Probe(ProfileSource source) => Eff.lift(() =>
        ProfileFormat.Admit(source.Path).Bind(format => Capture(source.Path, notices => format.Switch(
            state: (Source: source, Format: format, Notices: notices),
            dxf: static state => ProbeDxf(state.Source, state.Format, state.Notices),
            dwg: static state => Try.lift(() => File.ReadAllBytes(state.Source.Path.Value)).Run()
                .MapFail(error => Fault(state.Source.Path, error))
                .Bind(payload => Open(ProfileFormat.Dwg, state.Source, payload, state.Notices))
                .Map(document => Census(
                    state.Format, state.Source, document.Header.InsUnits, LayerNames(document), document.Entities, state.Notices.Value))))))
        .MapFail(error => error.IsExceptional ? Fault(source.Path, error) : error);

    public static Eff<ProfileImportReceipt> Read(ProfileSource source) => Eff.lift(() =>
        from raw in Try.lift(() => File.ReadAllBytes(source.Path.Value)).Run()
            .MapFail(error => Fault(source.Path, error))
        from format in ProfileFormat.Admit(source.Path)
        from result in Capture(source.Path, notices =>
            from document in Open(format, source, raw, notices)
            from _ in Reject(notices, source.Policy.Reader, source.Path)
            from scale in Scale(document.Header.InsUnits, source.Policy.Units, source.Path)
            from lowered in Entities(document.Entities, source.Policy, source.Path, scale.Scale)
            from repaired in ProfileTopology.Repair(lowered.Contours, source.Policy)
            let census = Census(format, source, document.Header.InsUnits, LayerNames(document), document.Entities, notices.Value)
            select new ProfileImportReceipt(
                ContentHash.Of(raw), format, census,
                new ProfileUnitEvidence(scale.Evidence, source.Policy.Units, LengthUnit.Millimeter, scale.Scale),
                repaired.Contours, lowered.Markings.ToArr(), repaired.Regions,
                Extents(repaired.Contours, lowered.Markings), repaired.Repairs))
        select result).MapFail(error => error.IsExceptional ? Fault(source.Path, error) : error);

    public static ProfileView Project(ProfileImportReceipt receipt, ProfileProjection projection) => projection.Switch(
        state: receipt,
        loops: static value => new ProfileView.Loops(value.Loops),
        lanes: static value => new ProfileView.Lanes(Group(value.Contours, static row => row.Provenance.Lane)),
        layers: static value => new ProfileView.Layers(Group(value.Contours, static row => row.Provenance.Layer)),
        regions: static value => new ProfileView.Regions(value.Regions),
        markings: static value => new ProfileView.Markings(value.Markings),
        bounds: static value => new ProfileView.Bounds(value.Extents),
        repairs: static value => new ProfileView.Repairs(value.Repairs),
        census: static value => new ProfileView.Census(value.Census),
        receipt: static value => new ProfileView.Receipt(value));

    static Map<TKey, Arr<ProfileContour>> Group<TKey>(Arr<ProfileContour> rows, Func<ProfileContour, TKey> key)
        where TKey : notnull =>
        rows.GroupBy(key).Map(static group => (group.Key, group.ToArr())).ToMap();

    static BoundingBox Extents(Arr<ProfileContour> contours, Seq<ProfileMarking> markings) =>
        markings.Map(static marking => new BoundingBox(marking.At, marking.At))
            .Concat(contours.Map(static contour => contour.Loop.Bound()).ToSeq())
            .Fold(BoundingBox.Empty, static (bounds, next) => BoundingBox.Union(bounds, next));

    static Fin<T> Capture<T>(ProfilePath path, Func<Atom<Seq<ProfileNotification>>, Fin<T>> use) {
        Atom<Seq<ProfileNotification>> notices = Atom(Seq<ProfileNotification>());
        return use(notices).MapFail(error => error.IsExceptional ? Fault(path, error) : error);
    }

    static Fin<CadDocument> Open(
        ProfileFormat format,
        ProfileSource source,
        byte[] payload,
        Atom<Seq<ProfileNotification>> notices) =>
        Try.lift(() => format.Read(payload, source.Policy.Reader,
            (_, args) => notices.Swap(rows => rows.Add(Notice(args)))))
        .Run().MapFail(error => Fault(source.Path, error));

    static Fin<ProfileCensus> ProbeDxf(
        ProfileSource source, ProfileFormat format, Atom<Seq<ProfileNotification>> notices) =>
        Try.lift(() => {
            NotificationEventHandler sink = (_, args) => notices.Swap(rows => rows.Add(Notice(args)));
            using DxfReader headerReader = new(source.Path.Value, sink);
            UnitsType units = headerReader.ReadHeader().InsUnits;
            using DxfReader tableReader = new(source.Path.Value, sink);
            Seq<string> layers = toSeq(tableReader.ReadTables().Layers).Map(static layer => layer.Name).Strict();
            using DxfReader entityReader = new(source.Path.Value, sink);
            return Census(format, source, units, layers, entityReader.ReadEntities(), notices.Value);
        }).Run().MapFail(error => Fault(source.Path, error));

    static ProfileCensus Census(
        ProfileFormat format,
        ProfileSource source,
        UnitsType units,
        Seq<string> layers,
        IEnumerable<Entity> entities,
        Seq<ProfileNotification> notifications) {
        Seq<Entity> rows = toSeq(entities).Strict();
        Map<string, int> counts = rows.GroupBy(static entity => entity.Layer.Name)
            .Map(static group => (group.Key, group.Count())).ToMap();
        return new ProfileCensus(
            ProfileEncoding.Of(format, source.Path),
            units,
            ProfileLayerCoverage.CompleteTable,
            toSeq(layers.Distinct().OrderBy(static name => name, StringComparer.Ordinal))
                .Map(name => (name, new ProfileLayerCensus(
                    name, source.Policy.Lanes.Resolve(name), counts.Find(name).IfNone(0))))
                .ToMap(),
            rows.GroupBy(static entity => entity.GetType().Name).Map(static group => (group.Key, group.Count())).ToMap(),
            notifications);
    }

    static Seq<string> LayerNames(CadDocument document) =>
        toSeq(document.Layers).Map(static layer => layer.Name).Strict();

    static Fin<(double Scale, UnitsType Evidence)> Scale(UnitsType declared, ProfileUnitPolicy policy, ProfilePath path) =>
        policy.Switch(
            state: declared,
            declared: static unit => Unit(unit).Map(scale => (scale, unit)),
            declaredOr: static (unit, fallback) => Fin.Succ((
                Unit(unit).IfFail(_ => Length.From(1d, fallback.Unit).Millimeters), unit)),
            @override: static (unit, forced) => Fin.Succ((Length.From(1d, forced.Unit).Millimeters, unit)))
        .MapFail(error => Fault(path, error));

    static Fin<double> Unit(UnitsType unit) => unit switch {
        UnitsType.USSurveyInches => Fin.Succ(Length.From(1d / 12d, LengthUnit.UsSurveyFoot).Millimeters),
        UnitsType.USSurveyYards => Fin.Succ(Length.From(3d, LengthUnit.UsSurveyFoot).Millimeters),
        UnitsType.USSurveyMiles => Fin.Succ(Length.From(5280d, LengthUnit.UsSurveyFoot).Millimeters),
        UnitsType.Unitless => Fin.Fail<double>(Error.New("profile-unit:unitless")),
        _ => Units.TryGetValue(unit, out LengthUnit mapped)
            ? Fin.Succ(Length.From(1d, mapped).Millimeters)
            : Fin.Fail<double>(Error.New($"profile-unit:{unit}")),
    };

    static Fin<ProfileLowered> Entities(
        IEnumerable<Entity> entities, ProfilePolicy policy, ProfilePath path, double scale) =>
        toSeq(entities).Map((entity, ordinal) => (entity, ordinal))
            .Traverse(row => Entity(row.entity, row.ordinal, Seq<ProfileBlock>(), Set<ulong>(), policy, path, scale)
                .ToValidation()).As().ToFin()
            .Map(static rows => rows.Fold(ProfileLowered.Empty, static (state, row) => state.Concat(row)));

    static Fin<ProfileLowered> Entity(
        Entity entity,
        int ordinal,
        Seq<ProfileBlock> blocks,
        Set<ulong> ancestors,
        ProfilePolicy policy,
        ProfilePath path,
        double scale) =>
        policy.Lanes.Resolve(entity.Layer.Name) is { Contributes: false }
            ? Fin.Succ(ProfileLowered.Empty)
            : entity switch {
                LwPolyline row => Contour(entity, ordinal, blocks, policy,
                    row.Vertices.Map(vertex => Ocs(row.Normal, vertex.Location, row.Elevation, scale)).ToArr(),
                    row.IsClosed, Bulges(row.Vertices.Map(static vertex => vertex.Bulge), row.Normal), path),
                Polyline2D row => Contour(entity, ordinal, blocks, policy,
                    row.Vertices.Map(vertex => Point(vertex.Location, scale)).ToArr(),
                    row.IsClosed, row.Vertices.Map(static vertex => vertex.Bulge).ToArr(), path),
                CadLine row => Contour(entity, ordinal, blocks, policy,
                    Arr(Point(row.StartPoint, scale), Point(row.EndPoint, scale)), false, Arr(0d, 0d), path),
                CadArc row => Planar(row.Normal, policy.Tolerance, entity, path)
                    .Bind(_ => ArcLoop(row, policy.Tolerance, entity, path, scale))
                    .Bind(loop => Wrapped(entity, ordinal, blocks, policy, loop)),
                CadCircle row => Planar(row.Normal, policy.Tolerance, entity, path)
                    .Bind(_ => CircleLoop(row.Normal, row.Center, row.Radius, policy.Tolerance, entity, path, scale))
                    .Bind(loop => Wrapped(entity, ordinal, blocks, policy, loop)),
                CadEllipse row when row.IsFullEllipse && row.RadiusRatio == 1d =>
                    Planar(row.Normal, policy.Tolerance, entity, path)
                        .Bind(_ => CircleLoop(
                            row.Normal, row.Center, row.MajorAxisEndPoint.GetLength(),
                            policy.Tolerance, entity, path, scale))
                        .Bind(loop => Wrapped(entity, ordinal, blocks, policy, loop)),
                CadEllipse row => Planar(row.Normal, policy.Tolerance, entity, path)
                    .Bind(_ => CurveLoop(
                        row.PolygonalVertexes(policy.Spline.Value), row.IsFullEllipse, policy.Tolerance, entity, path, scale))
                    .Bind(loop => Wrapped(entity, ordinal, blocks, policy, loop)),
                CadSpline row => row.TryPolygonalVertexes(policy.Spline.Value, out List<XYZ> points)
                    || row.UpdateFromFitPoints() && row.TryPolygonalVertexes(policy.Spline.Value, out points)
                        ? CurveLoop(points, row.IsClosed, policy.Tolerance, entity, path, scale)
                            .Bind(loop => Wrapped(entity, ordinal, blocks, policy, loop))
                        : Fin.Fail<ProfileLowered>(Fault(path, entity, "profile-spline:untessellated")),
                Hatch row => HatchContours(row, ordinal, blocks, policy, path, scale),
                CadPoint row => Fin.Succ(new ProfileLowered(
                    Seq<ProfileContour>(),
                    Seq(new ProfileMarking(
                        Point(row.Location, scale), row.Rotation,
                        Provenance(entity, ordinal, blocks, policy, row.Location.Z * scale, Set(ordinal)))))),
                Insert row => Insertion(row, ordinal, blocks, ancestors, policy, path, scale),
                _ => policy.Unsupported.Switch(
                    ignore: static _ => Fin.Succ(ProfileLowered.Empty),
                    reject: _ => Fin.Fail<ProfileLowered>(Fault(path, entity, "profile-entity:unsupported"))),
            };

    static Fin<ProfileLowered> HatchContours(
        Hatch row, int ordinal, Seq<ProfileBlock> blocks, ProfilePolicy policy, ProfilePath path, double scale) =>
        Planar(row.Normal, policy.Tolerance, row, path)
            .Bind(_ => Try.lift(() => toSeq(row.Paths).Map((boundary, index) => (boundary, index)).Strict())
                .Run().MapFail(error => Fault(path, row, error.Message)))
            .Bind(boundaries => boundaries.Traverse(item =>
                Try.lift(() => item.boundary.Edges.ToSeq()
                    .Bind(edge => HatchEdge(row, edge, policy.Spline.Value, scale)).Strict()).Run()
                    .MapFail(error => Fault(path, row, error.Message))
                    .Bind(spans => spans.IsEmpty
                        ? Fin.Fail<Loop>(Fault(path, row, $"hatch:{item.index}:empty"))
                        : Loop.Admit(
                        spans.Map(static span => span.Start).ToArr(),
                        closed: true, spans.Map(static span => span.Bulge).ToArr(), policy.Tolerance)
                        .MapFail(error => Fault(path, row, $"hatch:{item.index}:{error.Message}")))
                    .Map(loop => new ProfileContour(loop,
                        Provenance(row, ordinal, blocks, policy, loop.Plane, Set(ordinal))))
                    .ToValidation()).As().ToFin())
            .Map(static contours => new ProfileLowered(contours.ToSeq(), Seq<ProfileMarking>()));

    static Seq<HatchSpan> HatchEdge(Hatch hatch, Hatch.BoundaryPath.Edge edge, int precision, double scale) => edge switch {
        HatchLine line => Seq(new HatchSpan(
            Ocs(hatch.Normal, line.Start, hatch.Elevation, scale),
            Ocs(hatch.Normal, line.End, hatch.Elevation, scale), 0.0)),
        HatchArc arc => HatchArcSpans(hatch, arc, scale),
        HatchPolyline polyline => HatchPolylineSpans(hatch, polyline, scale),
        HatchEllipse ellipse => HatchSampled(hatch, ellipse.PolygonalVertexes(precision), scale),
        HatchSpline spline => HatchSampled(hatch, spline.PolygonalVertexes(precision), scale),
        _ => Seq<HatchSpan>(),
    };

    static Seq<HatchSpan> HatchArcSpans(Hatch hatch, HatchArc arc, double scale) {
        double sweep = HatchSweep(arc.StartAngle, arc.EndAngle, arc.CounterClockWise);
        int parts = Math.Abs(sweep) == Math.PI * 2.0 ? 4 : 1;
        double step = sweep / parts;
        return Range(0, parts).Map(index => {
            double from = arc.StartAngle + index * step;
            double to = from + step;
            return new HatchSpan(
                Ocs(hatch.Normal, new XY(arc.Center.X + Math.Cos(from) * arc.Radius, arc.Center.Y + Math.Sin(from) * arc.Radius), hatch.Elevation, scale),
                Ocs(hatch.Normal, new XY(arc.Center.X + Math.Cos(to) * arc.Radius, arc.Center.Y + Math.Sin(to) * arc.Radius), hatch.Elevation, scale),
                Math.Tan(step / 4.0) * Math.Sign(hatch.Normal.Z));
        });
    }

    static double HatchSweep(double start, double end, bool counterClockwise) {
        double turn = Math.PI * 2.0;
        double magnitude = counterClockwise ? (end - start + turn) % turn : (start - end + turn) % turn;
        if (magnitude == 0.0) magnitude = turn;
        return counterClockwise ? magnitude : -magnitude;
    }

    static Seq<HatchSpan> HatchPolylineSpans(Hatch hatch, HatchPolyline polyline, double scale) {
        XYZ[] vertices = polyline.Vertices.ToArray();
        double[] bulges = polyline.Bulges.ToArray();
        int spans = polyline.IsClosed && vertices.Length > 1
            ? vertices.Length
            : Math.Max(0, vertices.Length - 1);
        if (bulges.Length < spans)
            throw new System.IO.InvalidDataException($"hatch-polyline:bulges:{bulges.Length}:{spans}");
        return Range(0, spans).Map(index => new HatchSpan(
            Ocs(hatch.Normal, new XY(vertices[index].X, vertices[index].Y), hatch.Elevation, scale),
            Ocs(hatch.Normal, new XY(vertices[(index + 1) % vertices.Length].X, vertices[(index + 1) % vertices.Length].Y), hatch.Elevation, scale),
            bulges[index] * Math.Sign(hatch.Normal.Z)));
    }

    static Seq<HatchSpan> HatchSampled(Hatch hatch, IEnumerable<XYZ> source, double scale) {
        Seq<XYZ> points = toSeq(source);
        return points.Zip(points.Skip(1)).Map(pair => new HatchSpan(
            Ocs(hatch.Normal, new XY(pair.First.X, pair.First.Y), hatch.Elevation + pair.First.Z, scale),
            Ocs(hatch.Normal, new XY(pair.Second.X, pair.Second.Y), hatch.Elevation + pair.Second.Z, scale), 0.0));
    }

    // `Insert.Explode` resolves ONE placement, so a MINSERT row/column array expands here as translated replicas.
    static Fin<ProfileLowered> Insertion(
        Insert row,
        int ordinal,
        Seq<ProfileBlock> blocks,
        Set<ulong> ancestors,
        ProfilePolicy policy,
        ProfilePath path,
        double scale) => ancestors.Contains(row.Block.Handle)
        ? Fin.Fail<ProfileLowered>(Fault(path, row, "profile-block:cycle"))
        : Try.lift(() => toSeq(row.Explode()).Strict()).Run().MapFail(error => Fault(path, error))
            .Bind(children => children.Map((child, index) => (child, index))
                .Traverse(child => Entity(
                    child.child, child.index,
                    blocks.Add(new ProfileBlock(row.Block.Name, ordinal, Row: 0, Column: 0)),
                    ancestors.Add(row.Block.Handle), policy, path, scale).ToValidation()).As().ToFin())
            .Map(rows => rows.Fold(ProfileLowered.Empty, static (state, part) => state.Concat(part)))
            .Bind(placed => Replicas(row, scale)
                .Traverse(replica => placed
                    .Translate(replica.Delta, replica.Row, replica.Column).ToValidation()).As().ToFin()
                .Map(static replicas => replicas.Fold(
                    ProfileLowered.Empty, static (state, replica) => state.Concat(replica))));

    static Seq<(Vector3d Delta, int Row, int Column)> Replicas(Insert row, double scale) {
        Matrix3 frame = Matrix3.ArbitraryAxis(row.Normal) * Matrix3.RotationZ(row.Rotation);
        return Range(0, row.RowCount).Bind(rowIndex => Range(0, row.ColumnCount).Map(columnIndex => {
            XYZ offset = frame * new XYZ(columnIndex * row.ColumnSpacing, rowIndex * row.RowSpacing, 0d);
            return (new Vector3d(offset.X * scale, offset.Y * scale, offset.Z * scale), rowIndex, columnIndex);
        }));
    }

    static Fin<ProfileLowered> Contour(
        Entity entity, int ordinal, Seq<ProfileBlock> blocks, ProfilePolicy policy,
        Arr<Point3d> points, bool closed, Arr<double> bulges, ProfilePath path) =>
        Loop.Admit(points, closed, bulges, policy.Tolerance).MapFail(error => Fault(path, entity, error.Message))
            .Bind(loop => Wrapped(entity, ordinal, blocks, policy, loop));

    static Fin<ProfileLowered> Wrapped(
        Entity entity, int ordinal, Seq<ProfileBlock> blocks, ProfilePolicy policy, Loop loop) =>
        Fin.Succ(new ProfileLowered(
            Seq(new ProfileContour(loop, Provenance(entity, ordinal, blocks, policy, loop.Plane, Set(ordinal)))),
            Seq<ProfileMarking>()));

    static ProfileProvenance Provenance(
        Entity entity, int ordinal, Seq<ProfileBlock> blocks, ProfilePolicy policy, double plane, Set<int> ordinals) =>
        new(entity.Layer.Name, policy.Lanes.Resolve(entity.Layer.Name), entity.Color.Index,
            entity.Handle, blocks, plane, ordinals);

    static Fin<Loop> ArcLoop(CadArc arc, Context tolerance, Entity entity, ProfilePath path, double scale) {
        arc.GetEndVertices(out XYZ start, out XYZ end);
        return Loop.Admit(
            Arr(Point(start, scale), Point(end, scale)), false,
            Arr(Math.Tan(arc.Sweep / 4d) * Math.Sign(arc.Normal.Z), 0d), tolerance)
            .MapFail(error => Fault(path, entity, error.Message));
    }

    static Fin<Loop> CircleLoop(
        XYZ normal, XYZ center, double radius, Context tolerance, Entity entity, ProfilePath path, double scale) {
        double bulge = (Math.Sqrt(2d) - 1d) * Math.Sign(normal.Z);
        Matrix3 frame = Matrix3.ArbitraryAxis(normal);
        return Loop.Admit(
            Arr(
                Point(frame * new XYZ(center.X + radius, center.Y, center.Z), scale),
                Point(frame * new XYZ(center.X, center.Y + radius, center.Z), scale),
                Point(frame * new XYZ(center.X - radius, center.Y, center.Z), scale),
                Point(frame * new XYZ(center.X, center.Y - radius, center.Z), scale)),
            true,
            Arr(bulge, bulge, bulge, bulge),
            tolerance).MapFail(error => Fault(path, entity, error.Message));
    }

    static Fin<Loop> CurveLoop(
        IEnumerable<XYZ> points, bool closed, Context tolerance, Entity entity, ProfilePath path, double scale) =>
        Loop.Admit(toSeq(points).Map(point => Point(point, scale)).ToArr(), closed, Arr<double>(), tolerance)
            .MapFail(error => Fault(path, entity, error.Message));

    static Fin<Unit> Planar(XYZ normal, Context tolerance, Entity entity, ProfilePath path) =>
        Math.Abs(normal.X) <= Math.Sin(tolerance.Angle.Value)
        && Math.Abs(normal.Y) <= Math.Sin(tolerance.Angle.Value)
        && Math.Abs(Math.Abs(normal.Z) - 1d) <= 1d - Math.Cos(tolerance.Angle.Value)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Fault(path, entity, "profile-entity:non-planar"));

    static Arr<double> Bulges(IEnumerable<double> bulges, XYZ normal) =>
        toSeq(bulges).Map(bulge => bulge * Math.Sign(normal.Z)).ToArr();

    static Point3d Ocs(XYZ normal, XY point, double elevation, double scale) =>
        Point(Matrix3.ArbitraryAxis(normal) * new XYZ(point.X, point.Y, elevation), scale);

    static Point3d Point(XYZ point, double scale) => new(point.X * scale, point.Y * scale, point.Z * scale);

    static Fin<Unit> Reject(Atom<Seq<ProfileNotification>> notices, ProfileReadPolicy policy, ProfilePath path) =>
        notices.Value.Find(notice => policy.Rejects.Contains(notice.Kind))
            .Match(
                Some: notice => Fin.Fail<Unit>(new FabricationFault.IngressProviderUnavailable(
                    new SourceLocus.DxfEntity(Path.GetFileName(path.Value)), notice.Message)),
                None: static () => Fin.Succ(unit));

    static ProfileNotification Notice(NotificationEventArgs args) => new(
        args.NotificationType switch {
            NotificationType.Warning => ProfileNoticeKind.Warning,
            NotificationType.Error => ProfileNoticeKind.Error,
            NotificationType.NotImplemented => ProfileNoticeKind.NotImplemented,
            _ => ProfileNoticeKind.None,
        },
        args.Message,
        Optional(args.Exception?.Message));

    internal static Error Fault(ProfilePath path) => new FabricationFault.IngressTranslation(
        new SourceLocus.DxfEntity(Path.GetFileName(path.Value)));

    static Error Fault(ProfilePath path, Error error) => new FabricationFault.IngressProviderUnavailable(
        new SourceLocus.DxfEntity(Path.GetFileName(path.Value)), error.Message);

    static Error Fault(ProfilePath path, Entity entity, string detail) =>
        new FabricationFault.IngressProviderUnavailable(
            new SourceLocus.DxfEntity($"{Path.GetFileName(path.Value)}#{entity.Handle:x}"), detail);
}

public static class ProfileTopology {
    public sealed record Receipt(Arr<ProfileContour> Contours, Arr<ProfileRegion> Regions, Seq<ProfileRepair> Repairs);
    readonly record struct Stitched(Seq<ProfileContour> Contours, Seq<ProfileRepair> Repairs);
    readonly record struct Normalized(
        Seq<ProfileContour> Contours, Option<ProfileRegion> Region, Seq<ProfileRepair> Repairs);

    public static Fin<Receipt> Repair(Seq<ProfileContour> contours, ProfilePolicy policy) => policy.Closure.Switch(
        state: (Contours: contours, Policy: policy),
        open: static state => Normalize(state.Contours, state.Policy, demandClosed: false, gap: 0d),
        exact: static state => state.Contours.IsEmpty
            ? Fin.Fail<Receipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "profile-topology:empty").ToError())
            : Normalize(state.Contours, state.Policy, demandClosed: true, gap: 0d),
        healed: static (state, closure) => state.Contours.IsEmpty
            ? Fin.Fail<Receipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "profile-topology:empty").ToError())
            : Normalize(state.Contours, state.Policy, demandClosed: true, closure.MaxGap.Value.Millimeters));

    static Fin<Receipt> Normalize(Seq<ProfileContour> contours, ProfilePolicy policy, bool demandClosed, double gap) =>
        from stitched in Stitch(contours, policy.Tolerance, gap)
        from closed in stitched.Contours.Find(row => demandClosed && row.Provenance.Lane.Closes && !row.Loop.Closed)
            .Match(
                Some: row => Fin.Fail<Seq<ProfileContour>>(
                    new FabricationFault.OpenLoop(FabConcern.Profile, row.Loop.Count)),
                None: () => Fin.Succ(stitched.Contours))
        from groups in closed.GroupBy(static row => (
                Provenance: row.Provenance with { Ordinals = Set<int>(), Handle = 0ul }, row.Loop.Closed))
            .Traverse(group => NormalizeGroup(group.ToSeq(), policy).ToValidation()).As().ToFin()
        select new Receipt(
            groups.Bind(static group => group.Contours).ToArr(),
            groups.Map(static group => group.Region).Somes().ToArr(),
            stitched.Repairs.Concat(groups.Bind(static group => group.Repairs)));

    // A closed provenance group is the region unit: clean and densify through the arc owner, then read nesting
    // from the fill rule — a boolean union under one winding absorbs every hole into its outer boundary.
    static Fin<Normalized> NormalizeGroup(Seq<ProfileContour> rows, ProfilePolicy policy) =>
        rows.Head.Provenance.Lane.Closes && rows.Head.Loop.Closed
        ? from forest in ArcForest
              .Admit(rows.Map(static row => row.Loop), policy.Tolerance, rows.Head.Loop.Plane).ToFin()
          from cleaned in ArcAlgebra.Apply(new ArcOp.Clean(forest))
          from evidence in cleaned switch {
              ArcTrace.Forest arm => Fin.Succ(arm),
              _ => Fin.Fail<ArcTrace.Forest>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "profile-topology:clean").ToError()),
          }
          from lowered in evidence.Result.Loops
              .Traverse(loop => ArcAlgebra
                  .Densify(new ArcProjection.Lower(loop, policy.Tolerance.Absolute.Value))
                  .Bind(static trace => trace switch {
                      ArcTrace.Densified arm => Fin.Succ(arm.Receipt),
                      _ => Fin.Fail<DensifyReceipt>(
                          new GeometryFault.DegenerateInput(Kind.Curve, -1, "profile-topology:densify").ToError()),
                  }).ToValidation()).As().ToFin()
          from trace in PolygonAlgebra.Apply(
              new PolygonOp.Topology(lowered.Map(static receipt => receipt.Result), policy.Fill))
          from topology in trace switch {
              PolygonTrace.Regions arm => Fin.Succ(arm.Result),
              _ => Fin.Fail<TopologyReceipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "profile-topology:projection").ToError()),
          }
          let provenance = rows.Head.Provenance
          let admitted = evidence.Result.Loops
          select new Normalized(
              admitted.Map(loop => new ProfileContour(loop, provenance)),
              Some(new ProfileRegion(provenance, topology)),
              Cleanup(provenance, rows, evidence.Receipt)
                  .Concat(lowered.Map(receipt => Densification(provenance, receipt)).Somes())
                  .Concat(Areas(provenance, rows, admitted)))
        : Fin.Succ(new Normalized(rows, Option<ProfileRegion>.None, Seq<ProfileRepair>()));

    static Fin<Stitched> Stitch(Seq<ProfileContour> contours, Context tolerance, double gap) =>
        contours.GroupBy(static contour => (
                Provenance: contour.Provenance with { Ordinals = Set<int>(), Handle = 0ul }, contour.Loop.Closed))
            .Traverse(group => StitchLane(group.ToSeq(), tolerance, gap).ToValidation()).As().ToFin()
            .Map(static groups => new Stitched(
                groups.Bind(static group => group.Contours),
                groups.Bind(static group => group.Repairs)));

    static Fin<Stitched> StitchLane(Seq<ProfileContour> group, Context tolerance, double gap) =>
        group.Head.Provenance.Lane.Closes
            ? StitchGroup(group, tolerance, gap)
            : Fin.Succ(new Stitched(group, Seq<ProfileRepair>()));

    static Fin<Stitched> StitchGroup(Seq<ProfileContour> group, Context tolerance, double gap) {
        List<(Loop Loop, Set<int> Ordinals)> chains = group
            .Map(static row => (row.Loop, row.Provenance.Ordinals)).ToList();
        if (Branching(chains.Map(static chain => chain.Loop).ToList(), gap))
            return Fin.Fail<Stitched>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "profile-topology:branch").ToError());
        int joins = 0;
        double joinedDistance = 0d;
        bool merged;
        do {
            Option<(int Left, int Right, Loop Loop, double Distance)> next = Range(0, chains.Count)
                .Bind(left => Range(left + 1, chains.Count - left - 1).Choose(right =>
                    Join(chains[left].Loop, chains[right].Loop, tolerance, gap)
                        .Map(joined => (left, right, joined.Loop, joined.Distance))))
                .OrderBy(static candidate => candidate.Distance)
                .Head;
            merged = next.Match(
                Some: candidate => {
                    chains[candidate.Left] = (
                        candidate.Loop,
                        chains[candidate.Left].Ordinals.Union(chains[candidate.Right].Ordinals));
                    chains.RemoveAt(candidate.Right);
                    joinedDistance += candidate.Distance;
                    joins++;
                    return true;
                },
                None: static () => false);
        } while (merged);

        ProfileProvenance provenance = group.Head.Provenance with {
            Ordinals = group.Bind(static row => row.Provenance.Ordinals).ToSet(),
        };
        Seq<(Loop Loop, Set<int> Ordinals, Option<double> Gap)> closures = toSeq(chains)
            .Map(chain => Seal(chain.Loop, tolerance, gap)
                .Match(
                    Some: loop => (loop, chain.Ordinals, Some(Span(chain.Loop))),
                    None: () => (chain.Loop, chain.Ordinals, Option<double>.None)));
        return Fin.Succ(new Stitched(
            closures.Map(chain => new ProfileContour(chain.Loop, provenance with { Ordinals = chain.Ordinals })),
            (joins == 0
                ? Seq<ProfileRepair>()
                : Seq<ProfileRepair>(new ProfileRepair.Joined(
                    provenance, Length.FromMillimeters(joinedDistance), joins)))
                .Concat(closures.Map(chain => chain.Gap.Map(value => (ProfileRepair)new ProfileRepair.Closed(
                    provenance, Length.FromMillimeters(value)))).Somes())));
    }

    // An open chain whose ends meet within the gap is the canonical drawn profile: close it or the closure demand
    // rejects a drawing whose only defect is that its perimeter was drawn as separate entities.
    static Option<Loop> Seal(Loop loop, Context tolerance, double gap) =>
        !loop.Closed && loop.Count >= 3 && Span(loop) <= gap
            ? Loop.Admit(
                loop.Vertices, true,
                loop.Bulges, tolerance).ToOption()
            : None;

    static double Span(Loop loop) => loop.Vertices[0].DistanceTo(loop.Vertices[loop.Count - 1]);

    static Option<(Loop Loop, double Distance)> Join(Loop left, Loop right, Context tolerance, double gap) {
        if (left.Closed || right.Closed || Math.Abs(left.Plane - right.Plane) > tolerance.Absolute.Value)
            return None;
        return Seq(false, true).Bind(reverseLeft => Seq(false, true).Map(reverseRight => (
                Left: Orient(left, reverseLeft),
                Right: Orient(right, reverseRight))))
            .Choose(static pair => pair.Left.Bind(l => pair.Right.Map(r => (Left: l, Right: r))))
            .Map(pair => (
                pair.Left,
                pair.Right,
                Distance: pair.Left.Vertices[pair.Left.Vertices.Count - 1].DistanceTo(pair.Right.Vertices[0])))
            .Filter(candidate => candidate.Distance <= gap)
            .OrderBy(static candidate => candidate.Distance)
            .Head
            .Bind(candidate => Loop.Admit(
                candidate.Left.Vertices.ToSeq().Concat(candidate.Right.Vertices.ToSeq().Skip(1)).ToArr(), false,
                candidate.Left.Bulges.ToSeq().Take(candidate.Left.Bulges.Count - 1)
                    .Concat(candidate.Right.Bulges).ToArr(), tolerance)
                .Map(loop => (loop, candidate.Distance)).ToOption());
    }

    static Option<Loop> Orient(Loop loop, bool reverse) => reverse
        ? Loop.Admit(
            loop.Vertices.Rev().ToArr(),
            false,
            Range(0, loop.Count).Map(index => index == loop.Count - 1
                ? 0d
                : -loop.BulgeAt(loop.Count - 2 - index)).ToArr(),
            loop.Tolerance).ToOption()
        : Some(loop);

    static bool Branching(List<Loop> loops, double gap) {
        Seq<(int Loop, Point3d Point)> endpoints = toSeq(loops).Map((loop, index) => Seq(
            (index, loop.Vertices[0]),
            (index, loop.Vertices[loop.Vertices.Count - 1]))).Bind(identity);
        return endpoints.Exists(endpoint => endpoints.Count(candidate =>
            candidate.Loop != endpoint.Loop && candidate.Point.DistanceTo(endpoint.Point) <= gap) > 1);
    }

    static Seq<ProfileRepair> Cleanup(
        ProfileProvenance provenance, Seq<ProfileContour> before, ArcReceipt receipt) => receipt switch {
        ArcReceipt.Clean arm when arm.Loops.Count != before.Count
            || arm.Loops.Fold(0, static (sum, evidence) => sum + evidence.OutputSegments)
                != before.Fold(0, static (sum, row) => sum + row.Loop.Spans) =>
            Seq<ProfileRepair>(new ProfileRepair.Cleaned(
                provenance, before.Count, arm.Loops.Count,
                arm.Loops.Fold(0, static (sum, evidence) => sum + evidence.OutputSegments))),
        _ => Seq<ProfileRepair>(),
    };

    static Option<ProfileRepair> Densification(ProfileProvenance provenance, DensifyReceipt receipt) =>
        receipt.Simplified == 0 && receipt.SourceSpans == receipt.OutputSpans
            ? None
            : Some<ProfileRepair>(new ProfileRepair.Densified(
                provenance, receipt.ErrorBound, receipt.SourceSpans, receipt.OutputSpans, receipt.Simplified));

    static Seq<ProfileRepair> Areas(ProfileProvenance provenance, Seq<ProfileContour> before, Seq<Loop> after) =>
        before.Map(static row => row.Loop).Equals(after)
        ? Seq<ProfileRepair>()
        : Seq<ProfileRepair>(new ProfileRepair.Topology(
            provenance, before.Count, after.Count,
            Area.FromSquareMillimeters(before.Fold(0d, static (sum, row) => sum + Math.Abs(row.Loop.Area()))),
            Area.FromSquareMillimeters(after.Fold(0d, static (sum, loop) => sum + Math.Abs(loop.Area())))));
}

public static class Ingress {
    public static Eff<AdmittedGeometry> Admit(IngressSource source) => source.Switch(
        profile: static arm => ProfileImport.Read(arm.Source)
            .Map(receipt => (AdmittedGeometry)new AdmittedGeometry.Profiles(receipt)),
        solid: static arm => SolidImport.Read(arm.Source)
            .Map(receipt => (AdmittedGeometry)new AdmittedGeometry.Mesh(receipt)),
        steel: static arm => SteelImport.Read(arm.Source, arm.Policy)
            .Map(receipt => (AdmittedGeometry)new AdmittedGeometry.Steel(receipt)),
        element: static arm => Eff.lift(() => ElementImport.Admit(arm.Source))
            .Map(admission => (AdmittedGeometry)new AdmittedGeometry.Elements(admission)));
}
```

## [06]-[RESEARCH]

- `Polyline2D` inherits its extrusion direction from the open-generic `Polyline<Vertex2D>` base, which `ilspycmd` cannot resolve by name; until that base decompiles, the `Polyline2D` arm lowers `Vertex2D.Location` as WCS and applies no OCS map or bulge-sense inversion, unlike the verified `LwPolyline.Normal` arm.
