# [MATERIALS_INTERCHANGE]

## [01]-[INDEX]

- [03]-[MATERIALX_DOCUMENT]: `MtlxDocument`/`MtlxNode` shape the MaterialX 1.39 node graph, `NodeCategory` carries the per-category typed port, and `Mtlx` projects `AppearanceNode` over per-op category rows, source-typed edge polarities, probed constants, the texture-source binding, and the BAKED-FILENAME binding filling each image node's `file` slot from the set's own egress leaf, its `.mtlx` serialize/admit fold railing every unprojectable node.

## [02]-[MATERIAL_WIRE]

- Entry: `public static Fin<AppearanceSummary> Summary(MaterialParameters parameters, Op key)` lowers a library row to the SEAM `AppearanceSummary` through the seam-owned `AppearanceSummary.Of` factory — the neutral PBR scalars with the `AppearanceKey` the factory mints (the kernel seed-zero `XxHash128` over the canonical PBR bytes, the ONE hasher) on the factory's own `Fin` rail, since it gates every channel to the unit range and takes the `Op` key rather than a tolerance. It is the CONTRACTED entry `Projection/component#COMPONENT_SUBGRAPH` `ComponentSubgraph.Capture` composes and the SAME factory `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` composes; the channel triple crosses as the landed Element `[ComplexValueObject]` `AppearanceVector.Create(...)`, whose accumulated slot gate names every offending channel at once.
- Entry: `AppearanceEgress.Project` mints the full OpenPBR material; `Set` projects an admitted baked surface into the generated `Set.baked` arm; `AppearanceEgress.Ibl` projects the resolved dome into `Set.environment.ibl`. Each completed document crosses `WireAdmission.Admit` once after its product is final.
- Law: every closed vocabulary crosses as its GENERATED ENUM through ONE `WireVocabulary` bridge per roster (`LicenceVocabulary` for the one frontier roster a Raster page cannot name) — derived by parsing each row's own key against the enum's `OriginalName` spelling, so no hand row table exists to drift — and the derivation is PROVED at type init: a `[SmartEnum]` row with no enum member, or an enum value of zero, throws before the first egress. `RasterFormat`→`Container` is the one PARTIAL bridge (a non-wire container such as `jpeg` has no enum row) and answers on the `Fin` rail at the egress that asks, never a total map with a fabricated arm.
- Law: `AppearanceWireMap` is the completeness gate for total reader-free mappings. Presence-sensitive assessment, chromaticity, card, ingest, and proto3 optional scalars lower explicitly in `AppearanceEgress`; no generated mapper is asked to construct evidence whose presence depends on a domain case or `Option`.
- Law: `[MapDerivedType]` is REFUSED here and the reason is structural: its unregistered-case arm throws at RUNTIME where the generated Thinktecture `Switch` breaks the BUILD, and a protobuf `oneof` envelope is not a class hierarchy (RMG036). Union-case dispatch stays the generated total `Switch`.
- Law: JSON text is NOT produced here — the app root's `Rasm.AppHost/Runtime/ports#WIRE_LAW` `WireJson` renders ProtoJSON over every generated family, so an S2 member holds messages and bytes alone, and a `JsonSerializerOptions` beside the appearance family is the deleted form.
- Law: protobuf serialization is used DIRECTLY at the carrying edge; no Materials codec renames `ToByteArray` or `Parser.ParseFrom`. `Raster/set#SET_INGEST` owns the one bounded peer parse because it owns that intake, while `WireAdmission` owns the generated descriptor verdict every boundary shares.
- Law: seam-owned `AppearanceSummary.Of` mints the `AppearanceKey` — the kernel seed-zero `XxHash128` over the canonical PBR bytes via the seam `ContentAddress`/`CanonicalWriter`, NOT a second hasher and NOT a non-zero seed — so the `Summary` lowering here and the `Rasm.Bim` `AppearanceProjection.Project` lowering compose the SAME factory and produce the SAME key for one surface.
- Law: plane bytes never enter a document. Every `PlaneRef` carries its logical leaf and one required `ArtifactRef`; the canonical artifact owner carries address and extent together. `Set.key` identifies the complete document and never substitutes for payload identity.
- Law: `Set` rides BEHIND the `AppearanceKey`, never inside it. `AppearanceSummary` takes its preimage from the frozen seven-value PBR vector, so the set key is a PAYLOAD column: one appearance key covers a material with and without a baked set — the set refines the same appearance rather than describing a different one.
- Boundary: `Material`/`AppearanceSummary` is the ONE appearance wire — a per-consumer material DTO is the deleted form. `AppearanceSummary` crosses NEUTRAL (the `UInt128 AppearanceKey` plus scene-linear `BaseColorR`/`G`/`B`, `Metallic`, `Roughness`, `Opacity`, `Transmissive`), flat for a consumer reading without the lobe graph. The full `Material` is the payload BEHIND that key — the `MaterialId` `family.name` key, the `OpenPbr` vector, the optional `conductor` key, the `Provenance`, the resolved `preview` colour, and the optional admitted `Emission`. Colour crosses as the scene-linear `Color` triple so a peer renders without re-deriving ACEScg; NAMED LOSS: the hex rendering beside the triple, which a peer renders at its own edge.
- Boundary: conductor-ness derives structurally at `Project`. Capture sampling, assessment, chromaticity, card, ingest, and calibration cross only when their typed evidence exists; rank deficiency is `rank < parameter_count`, never `+Inf`, and an unobserved result never becomes a zero-filled message. `Card` carries card identity and licence; no model-body address crosses without an artifact extent and redemption consumer.
- Boundary: `Set.press` carries `Press` WITHOUT a backend column because every press run reaching the wire is CPU-minted — `AppearanceEgress.Set` proves `PressRun.Backend.ContentAuthoritative` before the document mints, so the accelerator lane is structurally absent from the wire rather than a column a reader trusts. `Press.graph_key` carries presence only for a shaded press; a graphless field or slab press leaves the optional column absent. `Set.ibl.luminance_cdf` likewise carries the stored guide only after a guided prefilter and stays absent after an unguided run.
- Boundary: `Ibl` is the ONE environment document and it mirrors the resolved `EnvironmentLight` row — the frozen band-major `sh9`, the six product planes by `PlaneRef`, the roughness ladder, and the READ-TIME `intensity`/`rotation` pair a consumer applies and a producer never bakes. The model key rides `Set.source` (the generator of a synthesized dome, absent for an ingested HDRI); NAMED LOSS: the two Hosek-Wilkie asset digests and the authored intensity unit pair, which stay on the domain row and reach the analytics plane off that row rather than off the wire.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using NodaTime.Serialization.Protobuf;
using Rasm.AppHost.Runtime;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Materials.Appearance;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Photometric;
using Rasm.Materials.Appearance.Surface;
using Rasm.Materials.Appearance.Texture;
using Rasm.Materials.Raster;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using Wacton.Unicolour;
using static LanguageExt.Prelude;
// Contracts are retired from this logic.

[assembly: MapperDefaults(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]

namespace Rasm.Materials.Appearance.Interchange;

// --- [TYPES] ---------------------------------------------------------------------------
internal static class LicenceVocabulary {
    static readonly Lazy<FrozenDictionary<LicenseClass, Wire.LicenseClass>> Licences =
        WireVocabulary.Total<LicenseClass, Wire.LicenseClass>(static () => LicenseClass.Items, static r => r.Key);

    public static Wire.LicenseClass Wire(LicenseClass row) => Licences.Value[row];
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LevelEgress(EgressVariant Variant, ArtifactContent Artifact);

public readonly record struct PlaneEgress(
    TextureChannel Channel, RasterFormat Format, BlockFormat Block, KtxPayload Payload, Seq<LevelEgress> Levels);

public readonly record struct PackEgress(ChannelPack Pack, RasterFormat Format, Seq<LevelEgress> Levels);

public readonly record struct EnvironmentProductEgress(
    string File, ContentAddress Blob, ArtifactContent Artifact, TexturePlane Plane,
    RasterFormat Container, KtxPayload Payload, LayerLaw LayerLaw, uint Mips);

public sealed record IblStorage(
    EnvironmentProductEgress Equirect, EnvironmentProductEgress Cubemap, EnvironmentProductEgress Preview,
    Seq<EnvironmentProductEgress> Specular, EnvironmentProductEgress BrdfLut,
    Option<EnvironmentProductEgress> LuminanceCdf);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AppearanceEgress {
    static RgbSpectrum Linear(Unicolour colour) { var lin = colour.RgbLinear; return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)); }

    public static Fin<AppearanceSummary> Summary(MaterialParameters parameters, Op key) =>
        Linear(parameters.BaseColor) switch {
            var baseLinear => AppearanceSummary.Of(AppearanceVector.Create(
                baseColorR: baseLinear.R, baseColorG: baseLinear.G, baseColorB: baseLinear.B,
                metallic: Math.Clamp(parameters.Metalness, 0.0, 1.0),
                roughness: Math.Clamp(parameters.Roughness, 0.0, 1.0),
                opacity: Math.Clamp(1.0 - parameters.Transmission, 0.0, 1.0),
                transmissive: parameters.Transmission > 0.0), key),
        };

    public static Wire.Material Project(MaterialId id, MaterialParameters parameters, CaptureProvenance provenance, SurfaceShade preview) {
        (string family, string name) = Lens(id);
        Option<ConductorMetal> resolved = ConductorMetal.Resolve(family, name);
        Wire.Material wire = new() {
            Id = id.Value,
            OpenPbr = AppearanceWireMap.ToWire(OpenPbrSurface.Of(parameters, resolved)),
            Provenance = Provenance(provenance),
            Preview = AppearanceWireMap.Color(preview.BaseColorLinear),
        };
        resolved.Filter(_ => parameters.Metalness >= 1.0).Iter(metal => wire.Conductor = metal.Key);
        parameters.EmissionProvenance.Iter(admitted => wire.Emission = AppearanceWireMap.ToWire(admitted));
        return wire;
    }

    public static Wire.Provenance Provenance(CaptureProvenance provenance) {
        Wire.Provenance.Types.Capture capture = new() {
            Device = provenance.Device,
            Method = provenance.Method.Key,
            Measured = provenance.Measured,
            Calibrated = provenance.Calibrated,
        };
        provenance.CalibrationDeltaE.Iter(delta => capture.CalibrationDeltaE = delta);
        provenance.WavelengthCount.Iter(count => capture.WavelengthCount = checked((uint)count));
        provenance.AngularSamples.Iter(count => capture.AngularSamples = checked((uint)count));
        Wire.Provenance wire = new() { Capture = capture };
        wire = provenance.Assessment.Map(assessment => Assessment(wire, assessment)).IfNone(wire);
        provenance.Chromaticity.Iter(observed => wire.Chromaticity = Chromaticity(observed));
        provenance.ModelCard.Iter(card => {
            Wire.Provenance.Types.Card row = new() { ModelCard = card.Value, License = LicenceVocabulary.Wire(provenance.License.IfNone(LicenseClass.Blocked)) };
            wire.Card = row;
        });
        provenance.Ingest.Iter(ingest => {
            Wire.Provenance.Types.Ingest row = new() { Source = ingest.Source };
            ingest.LicenceDeclared.Iter(licence => row.Licence = licence);
            ingest.Reference.Iter(reference => row.Reference = reference);
            wire.Ingest = row;
        });
        return wire;
    }

    static Wire.Provenance Assessment(Wire.Provenance wire, CaptureAssessment assessment) =>
        assessment.Switch(
            state: wire,
            fit: static (target, fit) => {
                Wire.Provenance.Types.Fit row = new() {
                    Residual = fit.Residual,
                    Rank = checked((uint)fit.Rank),
                    ParameterCount = checked((uint)fit.ParameterCount),
                };
                fit.ConditionNumber.Iter(condition => row.ConditionNumber = condition);
                target.Fit = row;
                return target;
            },
            inference: static (target, inference) => {
                target.Inference = new Wire.Provenance.Types.Inference {
                    Tiles = checked((ulong)inference.Tiles),
                    ReferenceDeltaMax = inference.ReferenceDeltaMax,
                };
                return target;
            });

    internal static Wire.Provenance.Types.Chromaticity Chromaticity(ChromaticityEvidence evidence) {
        Wire.Provenance.Types.Chromaticity wire = new();
        evidence.Dominance.Iter(observed => wire.Dominance = new Wire.Provenance.Types.Chromaticity.Types.Dominance {
            WavelengthNm = observed.WavelengthNm,
            Purity = observed.Purity,
        });
        evidence.Temperature.Iter(observed => wire.Temperature = new Wire.Provenance.Types.Chromaticity.Types.Temperature {
            CctKelvin = observed.Cct,
            Duv = observed.Duv,
        });
        return wire;
    }

    public static Fin<Wire.Material> Mint(MaterialId id, Op key) =>
        from parameters in MaterialLibrary.Lookup(id, key)
        from point in ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Option<Vector3d>.None, 0.5, 0.5, Context.Canonical, key)
        from preview in MaterialGraph.Default.Evaluate(point, parameters, key)
        select Project(id, parameters, CaptureProvenance.Authored, preview);

    public static Fin<Wire.Set> Set(
        TextureSet set, AppearanceSummary summary, Seq<PlaneEgress> planes, Seq<PackEgress> packs,
        CaptureProvenance provenance, Option<PressRun> press, LicenseClass licence, Op key) =>
        Baked(set, summary, planes, packs, provenance, press, licence, key)
            .Bind(document => WireAdmission.Admit(document, WireBoundary.OutboundPayload, key));

    static Fin<Wire.Set> Baked(
        TextureSet set, AppearanceSummary summary, Seq<PlaneEgress> planes, Seq<PackEgress> packs,
        CaptureProvenance provenance, Option<PressRun> press, LicenseClass licence, Op key) =>
        from _ in guard(press.ForAll(static run => run.Backend.ContentAuthoritative),
                new MaterialFault.Parameter(key, "<set-wire-gpu-minted>"))
        from __ in guard(licence.Grants, new MaterialFault.Parameter(key, $"<set-wire-licence-blocked:{licence.Key}>"))
        from ___ in guard(!planes.Exists(plane => set.Packs.Exists(pack => pack.Present.Contains(plane.Channel))),
                new MaterialFault.Parameter(key, "<set-wire-channel-both-packed-and-standalone>"))
        from rows in toSeq(TextureChannel.Items)
            .Choose(channel => planes.Find(plane => plane.Channel == channel).Map(plane => (Channel: channel, Plane: plane)))
            .Traverse(entry => Plane(set, entry.Channel, entry.Plane, key)).As()
        from packed in packs.Traverse(pack => Pack(set, pack, key)).As()
        let surface = Fill(new Wire.SurfaceSet {
            Width = (uint)set.Width.Value, Height = (uint)set.Height.Value, Layers = (uint)set.Layers.Value,
            LayerLaw = WireVocabulary.Law(set.Law),
            NormalConvention = WireVocabulary.Convention(set.Convention),
            AlphaMode = WireVocabulary.Alpha(set.Alpha),
            Tiled = set.Tiled.Value().Exists(static proof => proof.Accepted),
            Udim = set.Udim.IsEmpty ? Wire.Udim.None : Wire.Udim.Mari,
            UdimTiles = { set.Udim.Map(static tile => (uint)tile.Value) },
            Planes = { rows }, Packs = { packed },
        }, set)
        select Fill(new Wire.Set {
            Key = ContentHash.Wire(set.Key),
            LicenseClass = LicenceVocabulary.Wire(licence),
            Baked = new Wire.BakedSet {
                Surface = surface,
                AppearanceKey = ContentHash.Wire(summary.AppearanceKey),
                Provenance = Provenance(provenance),
            },
        }, press);

    static Wire.SurfaceSet Fill(Wire.SurfaceSet wire, TextureSet set) {
        set.Material.Iter(id => wire.MaterialId = id.Value);
        set.Conductor.Iter(metal => wire.Conductor = metal.Key);
        set.HeightScaleMm.Iter(mm => wire.HeightScaleMm = mm);
        return wire;
    }

    static Wire.Set Fill(Wire.Set wire, Option<PressRun> press) {
        press.Iter(run => wire.Baked.Press = AppearanceWireMap.ToWire(run));
        return wire;
    }

    public static Fin<Wire.Set> Set(
        UdimSheet sheet, AppearanceSummary summary,
        Seq<(UdimTile Tile, Seq<PlaneEgress> Planes, Seq<PackEgress> Packs)> storage,
        CaptureProvenance provenance, Option<PressRun> press, LicenseClass licence, Op key) =>
        from _ in guard(storage.Count == sheet.Tiles.Count,
                new MaterialFault.Parameter(key, "<set-wire-udim-storage-mismatch>"))
        from tiles in sheet.Tiles.Traverse(pair =>
            storage.Find(entry => entry.Tile == pair.Tile)
                .ToFin(new MaterialFault.Parameter(key, $"<set-wire-udim-missing-tile:{pair.Tile.Value}>"))
                .Bind(entry => Baked(pair.Set, summary, entry.Planes, entry.Packs, provenance, press, licence, key)
                    .Map(wire => (pair.Tile, Wire: wire)))).As()
        from head in tiles.Head.ToFin(new MaterialFault.Parameter(key, "<set-wire-udim-empty>"))
        from admitted in WireAdmission.Admit(Sheet(head.Wire, sheet, tiles), WireBoundary.OutboundPayload, key)
        select admitted;

    static Wire.Set Sheet(Wire.Set head, UdimSheet sheet, Seq<(UdimTile Tile, Wire.Set Wire)> tiles) {
        Wire.SurfaceSet surface = head.Baked.Surface;
        head.Key = ContentHash.Wire(sheet.Key);
        surface.Udim = Wire.Udim.Mari;
        surface.UdimTiles.Clear();
        surface.UdimTiles.AddRange(tiles.Map(static entry => (uint)entry.Tile.Value));
        surface.Planes.Clear();
        surface.Planes.AddRange(tiles.Bind(static entry => toSeq(entry.Wire.Baked.Surface.Planes)));
        surface.Packs.Clear();
        surface.Packs.AddRange(tiles.Bind(static entry => toSeq(entry.Wire.Baked.Surface.Packs)));
        return head;
    }

    static Fin<Wire.Plane> Plane(TextureSet set, TextureChannel channel, PlaneEgress plane, Op key) =>
        from pyramid in set.Channels.Find(channel).ToFin(new MaterialFault.Parameter(key, $"<set-wire-unbound-channel:{channel.Key}>"))
        from legal in guard(plane.Payload.Traits.Admits(CodecCapability.WireLegal), new MaterialFault.Parameter(key, $"<set-wire-payload-illegal:{channel.Key}:{plane.Payload.Key}>"))
        from scene in guard(pyramid.Base.Transfer.SceneReferred, new MaterialFault.Parameter(key, $"<set-wire-display-referred:{channel.Key}:{pyramid.Base.Transfer.Key}>"))
        from law in guard(plane.Levels.Count == (plane.Format.HoldsPyramid ? 1 : pyramid.Levels.Count),
                new MaterialFault.Parameter(key, $"<set-wire-levels-unaddressed:{channel.Key}:{plane.Levels.Count}:{pyramid.Levels.Count}>"))
        from container in WireVocabulary.Container(plane.Format, key)
        from levels in plane.Levels.Traverse(level =>
            set.Egress(new EgressSlot.Channel(channel), level.Variant, plane.Format, key)
                .Map(leaf => Reference(leaf, level.Artifact))).As()
        select new Wire.Plane {
            Role = WireVocabulary.Role(channel),
            Format = WireVocabulary.Format(pyramid.Base.Format),
            Container = container,
            Channels = (uint)channel.Components,
            Mips = (uint)pyramid.Levels.Count,
            KtxPayload = WireVocabulary.Payload(plane.Payload),
            Levels = { levels },
            Transfer = WireVocabulary.Transfer(pyramid.Base.Transfer),
            AlphaMode = WireVocabulary.Alpha(pyramid.Base.Alpha),
            MipPolicy = WireVocabulary.Mip(pyramid.Policy),
            BlockFormat = WireVocabulary.Block(plane.Block),
        };

    static Fin<Wire.PackRow> Pack(TextureSet set, PackEgress pack, Op key) =>
        from plane in set.Packs.Find(seated => seated.Pack == pack.Pack)
            .ToFin(new MaterialFault.Parameter(key, $"<set-wire-unbound-pack:{pack.Pack.Key}>"))
        from law in guard(pack.Levels.Count == (pack.Format.HoldsPyramid ? 1 : plane.Plane.Levels.Count),
                new MaterialFault.Parameter(key, $"<set-wire-pack-levels-unaddressed:{pack.Pack.Key}:{pack.Levels.Count}:{plane.Plane.Levels.Count}>"))
        from container in WireVocabulary.Container(pack.Format, key)
        from levels in pack.Levels.Traverse(level =>
            set.Egress(new EgressSlot.Pack(pack.Pack), level.Variant, pack.Format, key)
                .Map(leaf => Reference(leaf, level.Artifact))).As()
        select new Wire.PackRow {
            Pack = WireVocabulary.Pack(pack.Pack),
            Present = { plane.Pack.Slots.Filter(plane.Present.Contains).Map(WireVocabulary.Role) },
            Format = WireVocabulary.Format(plane.Plane.Base.Format),
            Container = container,
            Mips = (uint)plane.Plane.Levels.Count,
            Levels = { levels },
        };

    static Wire.PlaneRef Reference(string file, ArtifactContent artifact) =>
        new() {
            File = file,
            Artifact = new Artifact.ArtifactRef {
                Sha256 = ByteString.CopyFrom(Convert.FromHexString(artifact.Sha256)),
                ArtifactBytes = artifact.Bytes,
            },
        };

    public static Fin<Wire.Set> Ibl(EnvironmentLight light, IblStorage storage, LicenseClass licence, Op key) =>
        from _ in guard(licence.Grants, new MaterialFault.Parameter(key, $"<ibl-wire-licence-blocked:{licence.Key}>"))
        from __ in guard(storage.Equirect.Blob == light.Blobs.Equirect && storage.Cubemap.Blob == light.Blobs.Cubemap
                && storage.Preview.Blob == light.Blobs.Preview && storage.BrdfLut.Blob == light.Blobs.BrdfLut
                && storage.Specular.Count == light.Products.Specular.Count
                && storage.LuminanceCdf.Map(static product => product.Blob) == light.Blobs.LuminanceCdf,
                new MaterialFault.Parameter(key, "<ibl-wire-storage-diverges-from-blobs>"))
        from equirect in Product(storage.Equirect, key)
        from cubemap in Product(storage.Cubemap, key)
        from preview in Product(storage.Preview, key)
        from specular in storage.Specular.Traverse(product => Product(product, key)).As()
        from brdf in Product(storage.BrdfLut, key)
        from guide in storage.LuminanceCdf.Traverse(product => Product(product, key))
        let source = new Wire.EnvironmentSource {
            Sh9 = { light.Products.Irradiance.Bands.ToArray() },
            Equirect = equirect, Cubemap = cubemap, Preview = preview,
            Intensity = light.Map.Intensity.RadiometricSi, Rotation = light.Map.Rotation,
        }
        let environment = new Wire.EnvironmentSet {
            Ibl = Fill(new Wire.Ibl {
                Source = source,
                Specular = { specular },
                RoughnessPerMip = { light.Products.RoughnessPerMip },
                BrdfLut = brdf,
            }, guide),
        }
        let document = Fill(new Wire.Set {
            Key = ContentHash.Wire(light.LightKey),
            LicenseClass = LicenceVocabulary.Wire(licence),
            Environment = environment,
        }, light)
        from admitted in WireAdmission.Admit(document, WireBoundary.OutboundPayload, key)
        select admitted;

    static Wire.Set Fill(Wire.Set wire, EnvironmentLight light) {
        light.SkyModelKey.Iter(model => wire.Environment.Source = model);
        return wire;
    }

    static Wire.Ibl Fill(Wire.Ibl wire, Option<Wire.EnvironmentPlane> guide) {
        guide.Iter(product => wire.LuminanceCdf = product);
        return wire;
    }

    static Fin<Wire.EnvironmentPlane> Product(EnvironmentProductEgress product, Op key) =>
        WireVocabulary.Container(product.Container, key).Map(container => new Wire.EnvironmentPlane {
            Plane = Reference(product.File, product.Artifact),
            Container = container,
            Format = WireVocabulary.Format(product.Plane.Format),
            Transfer = WireVocabulary.Transfer(product.Plane.Transfer),
            Primaries = WireVocabulary.Primaries(product.Plane.Primaries),
            Depth = WireVocabulary.Depth(product.Plane.Format.Depth),
            Channels = checked((uint)product.Plane.Format.Components),
            Width = checked((uint)product.Plane.Grid.Columns.Value),
            Height = checked((uint)product.Plane.Grid.Rows.Value),
            Layers = checked((uint)product.Plane.Layers.Value),
            LayerLaw = WireVocabulary.Law(product.LayerLaw),
            Mips = product.Mips,
            KtxPayload = WireVocabulary.Payload(product.Payload),
            AlphaMode = WireVocabulary.Alpha(product.Plane.Alpha),
        });

    static (string Family, string Name) Lens(MaterialId id) =>
        id.Value.Split('.') switch {
            [var family, var name, ..] => (family, name),
            [var family] => (family, id.Value),
            [] => (string.Empty, id.Value),
        };
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both, EnabledConversions = MappingConversionType.None)]
public static partial class AppearanceWireMap {
    // --- [COLOUR_CONVERTERS]
    [UserMapping] public static Wire.Color Color(RgbSpectrum rgb) => new() { R = rgb.R, G = rgb.G, B = rgb.B };
    [UserMapping] public static Wire.Color Color(Unicolour colour) =>
        colour.RgbLinear.Triplet switch { var lin => new Wire.Color { R = lin.First, G = lin.Second, B = lin.Third } };
    [UserMapping] static Wire.Color Band(SubsurfaceRadius radius) => new() { R = radius.R, G = radius.G, B = radius.B };
    [UserMapping] static uint Unsigned(int value) => checked((uint)value);
    [UserMapping] static ulong Unsigned(long value) => checked((ulong)value);

    // --- [MATERIAL_VECTOR]
    [MapValue(nameof(Wire.OpenPbr.GeometryOpacity), 1.0)]
    [MapperIgnoreSource(nameof(OpenPbrSurface.Conductor))]
    public static partial Wire.OpenPbr ToWire(OpenPbrSurface surface);

    public static Wire.Emission ToWire(EmissionInput admitted) {
        Wire.Emission wire = new() {
            Unit = admitted.Provenance.Measure.CanonicalUnit,
            Value = admitted.Provenance.Measure.CanonicalValue,
            Readout = new Wire.EmissionReadout {
                RelativeLuminance = admitted.RelativeLuminance,
                GamutMapped = admitted.GamutMapped,
            },
        };
        ChromaticityEvidence.Of(admitted.DominantWavelengthNm, admitted.ExcitationPurity, admitted.Temperature)
            .Iter(observed => wire.Readout.Chromaticity = AppearanceEgress.Chromaticity(observed));
        return wire;
    }

    [MapProperty(nameof(PressRun.PlanKey), nameof(Wire.Press.PlanKey), Use = nameof(Key))]
    [MapProperty(nameof(PressRun.ElapsedMs), nameof(Wire.Press.Elapsed), Use = nameof(Elapsed))]
    [MapProperty(nameof(PressRun.Downgraded), nameof(Wire.Press.Downgraded), Use = nameof(Tally))]
    [MapProperty(nameof(PressRun.Faulted), nameof(Wire.Press.FaultedTexels), Use = nameof(Summed))]
    [MapperIgnoreSource(nameof(PressRun.Backend))]
    [MapperIgnoreSource(nameof(PressRun.Planes))]
    [MapperIgnoreSource(nameof(PressRun.Aging))]
    [MapperIgnoreSource(nameof(PressRun.GraphKey))]
    [MapperIgnoreSource(nameof(PressRun.GpuDeltaMax))]
    [MapperIgnoreTarget(nameof(Wire.Press.GraphKey))]
    [MapperIgnoreTarget(nameof(Wire.Press.GpuDeltaMax))]
    private static partial Wire.Press Press(PressRun run);

    public static Wire.Press ToWire(PressRun run) {
        Wire.Press wire = Press(run);
        run.GraphKey.Iter(graph => wire.GraphKey = Key(graph));
        run.GpuDeltaMax.Iter(delta => wire.GpuDeltaMax = delta);
        return wire;
    }

    [UserMapping] static ByteString Key(UInt128 key) => ContentHash.Wire(key);
    [UserMapping] static Duration Elapsed(double milliseconds) => Duration.FromTimeSpan(TimeSpan.FromMilliseconds(milliseconds));
    [UserMapping] static uint Tally(Seq<TextureChannel> downgraded) => (uint)downgraded.Count;
    [UserMapping] static ulong Summed(HashMap<TextureChannel, ulong> faulted) =>
        faulted.Values.Fold(0UL, static (acc, texels) => acc + texels);

}

```

## [03]-[MATERIALX_DOCUMENT]

- Owner: `MtlxDocument` the MaterialX 1.39 document; `MtlxNode`/`MtlxInput` the node-graph element shapes; `NodeCategory` `[SmartEnum<string>]` the MaterialX node-category axis carrying each category's DEFAULT output port; `Mtlx` the static serialize/admit fold owning the per-node category+port resolution and the input projection.
- Entry: `public static Fin<MtlxDocument> FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters, Op key, HashMap<PortId, TextureSource> textures = default)` projects the `graph#MATERIAL_GRAPH` node DAG to the MaterialX node-graph document. `parameters` PROBES each `Input` node's pulled `PortValue` for its constant polarity and rendered `value` attribute, since a `constant` node with no value is meaningless MaterialX; `textures` is the wiring-site binding recovering each `Texture` node's `TextureSource` — the graph case erases the source into its total sampler closure, so the map IS the recoverable spelling and an unbound node floors to `image`; an unprojectable node rails `MaterialFault.Graph`.
- Entry: `public static Fin<MtlxDocument> ToOpenPbr(Wire.Material wire, Option<Wire.Set> planes, Op key)` emits the `open_pbr_surface` document by FOLDING the `OpenPbrPorts` schema table — one row per OpenPBR Surface 1.1 input naming port, polarity, and wire column. Where a baked set is supplied it REPLACES each covered constant input with a `tiledimage` edge whose `file` is the set's own egress leaf under the channel's `MtlxBinding` row, and APPENDS the texture-only geometry ports (a bound normal routes through a `normalmap` node, a tangent binds its vector image direct), so ONE entry serves the textured and untextured documents alike and a baked normal map reaches the surface rather than orphaning. Both target the MaterialX 1.39 `<materialx version="1.39">` root; the `.mtlx` XML serialization rides `System.Xml.Linq` at the host boundary.
- Packages: Rasm.Element (the SEAM `MaterialId` identity), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Xml.Linq` at the serialize boundary).
- Growth: a new `MathOp` row breaks the total generated `Switch` in `MathRow` at compile time and lands as one category+port arm; a new `MixOp` row resolves through the `MixRows` `BlendMode` disposition table — an unlisted mode rails `MaterialFault.Graph` loud at projection, and its native/lowered disposition is one table row; a new MaterialX node category is one `NodeCategory` row; a new `TextureSource` case lands through the `texture#TEXTURE_UV` `TextureSource.MtlxCategory` + `MtlxParameters` projections this page resolves via `Mtlx.CategoryOf(TextureSource)` and folds into inputs via `TextureInputs`; a new OpenPBR wire column is one `OpenPbrPorts` row — never a per-node serializer, never a second MaterialX schema, never a new call expression in a hand-listed input chain.
- Law: a projected `.mtlx` validates through the MaterialX RUNTIME, never against a schema artefact — MaterialX 1.39 publishes no XSD, JSON Schema, or RelaxNG, and the format is specified in prose alone, so the admission at the host boundary is `readFromXmlFile` into a document whose data library is attached (`setDataLibrary` over the loaded standard libraries) followed by `validate`, which returns the verdict beside its own message; the shipped `mxvalidate` script is that same pair and nothing more. A local well-formedness check standing in for it passes documents whose node categories, port names, and types the library alone can refute, so this page projects and the host validates through the owner.
- Law: the projection is HONEST or LOUD, never silently wrong. `MathOp.Power` projects `power`, `MathOp.OneMinus` the amount-minus-in `invert`, `MathOp.Scale` the vector3 `multiply` variant, and the widened rows their stdlib categories (`modulo`/`sqrt`/`absval`/`sin`/`cos`/`crossproduct`/`normalize`); every UNARY op crosses on the stdlib `in` port, never `in1`. `MathOp.Fresnel` is a pbr-level concept with NO 1.39 stdlib category and RAILS — the prior form lowered every unmapped op to `multiply`, a silent semantic corruption. `MixOp.Lerp` projects `mix`, `MixOp.Multiply` the math `multiply` (its factor unread by construction), and `Screen`/`Overlay` the blend-compositing nodes, never a blanket `mix` mislabeling a screen composite as a lerp.
- Law: `OpenPbrPorts` keeps the `ToOpenPbr` table NODEDEF-HONEST. `base_specular_tint` is NOT an `open_pbr_surface` input, so the Disney tint lowers INTO `specular_color` at `surface#OPENPBR_SLAB` and this table ships that resolved column verbatim — re-applying the bias here tinted twice. `subsurface_radius` is `float` and `subsurface_radius_scale` `color3` in the nodedef, so the mm bands split into max-band distance with per-channel ratio. `TransmissionRoughness` has no input and never crosses; an invented input name is the same silent-corruption class as a mislabeled node.
- Law: node OUTPUT types resolve PER INSTANCE — a math or mix node's concrete type from its OP row (MaterialX math nodes are type-polymorphic), a texture node's from its resolved category row, an `Input` constant's from the probed `PortValue` case — and an interior EDGE carries its SOURCE node's resolved output type while a surface-slot edge carries the SLOT's declared type. A blanket `color3` on every edge is the deleted form. `Mtlx` runs this same projection over the `standard_surface` category for the Standard-Surface translation.
- Boundary: `MtlxDocument` is the ONE MaterialX shape — a per-tool encoding is the deleted form. The document is the 1.39 root carrying one `<nodegraph>` of `<node>` elements with `<input>` children and one `<surfacematerial>` binding. Node names derive from the `PortId` ordinal as `node{id}` so the graph topology is recoverable and a consumer rewires the same DAG, the surface emit path keying its sink as `node0`. A duplicate node id, a missing or non-`BsdfOutput` sink, and a dangling edge each rail `MaterialFault.Graph` BEFORE any node emits — the same malformation set `MaterialGraph.Compile` rejects, railed here because `FromGraph` admits the raw graph.
- Boundary: colour values cross as the MaterialX `color3` scene-linear triple consistent with the generated `Color` linear projection, never an sRGB byte triple. `file` on an image-family node is `filename`-typed, crossing EMPTY from `FromGraph` (where `TextureSource.Image` carries in-memory levels the host edge writes as sidecar assets) and FILLED from `ToOpenPbr` when a `Wire.Set` is supplied — those leaf names come from `Raster/set#TEXTURE_SET` `TextureSet.Egress`, rendered once against the set's own lowered key, so the `.mtlx` attribute, the object-store path, and the wire column are ONE string rather than three renderings of one grammar.
- Boundary: the `.mtlx` port name IS the canonical channel name for every OpenPBR-owned row, which is why the binding needs no translation column and a `Scaled` row carries its unit fork as a real `multiply` node rather than a silent rename; a procedural node instead carries its `texture#TEXTURE_UV` `MtlxParameters` rows so it round-trips its parameters and not a bare category, and the `normalmap` node carries its `scale` input from `AppearanceNode.Normal.Strength`. `MtlxDocument` crosses as portable data the host `System.Xml.Linq` serializer renders and admits through the same node-category map, this owner never holding an XML reader at an interior signature and never binding a native MaterialX runtime; `MtlxInput.Value` renders through `ToString("R", CultureInfo.InvariantCulture)` because the schema requires the invariant round-trip literal. An unmapped category, a dangling edge, or a malformed port rails `MaterialFault.Graph`, never a partial document.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class MixTrait : ICapability<MixTrait> {
    public static readonly MixTrait Lowered = new("lowered", rank: 0);
    public static readonly MixTrait Swapped = new("swapped", rank: 1);
    public int Rank { get; }
}

[SmartEnum<string>]
public sealed partial class MtlxPort {
    public static readonly MtlxPort Float    = new("float");
    public static readonly MtlxPort Integer  = new("integer");
    public static readonly MtlxPort Boolean  = new("boolean");
    public static readonly MtlxPort Color3   = new("color3");
    public static readonly MtlxPort Vector3  = new("vector3");
    public static readonly MtlxPort Filename = new("filename");
    public static readonly MtlxPort Surface  = new("surfaceshader");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeCategory {
    public static readonly NodeCategory Constant        = new("constant", MtlxPort.Float);
    public static readonly NodeCategory Image           = new("image", MtlxPort.Color3);
    public static readonly NodeCategory TiledImage      = new("tiledimage", MtlxPort.Color3);
    public static readonly NodeCategory Triplanar       = new("triplanarprojection", MtlxPort.Color3);
    public static readonly NodeCategory Perlin2D        = new("noise2d", MtlxPort.Float);
    public static readonly NodeCategory Perlin3D        = new("noise3d", MtlxPort.Float);
    public static readonly NodeCategory Fractal2D       = new("fractal2d", MtlxPort.Float);
    public static readonly NodeCategory CellNoise       = new("cellnoise2d", MtlxPort.Float);
    public static readonly NodeCategory Worley          = new("worleynoise2d", MtlxPort.Float);
    public static readonly NodeCategory UnifiedNoise    = new("unifiednoise2d", MtlxPort.Float);
    public static readonly NodeCategory Checkerboard    = new("checkerboard", MtlxPort.Color3);
    public static readonly NodeCategory RampLr          = new("ramplr", MtlxPort.Color3);
    public static readonly NodeCategory RampTb          = new("ramptb", MtlxPort.Color3);
    public static readonly NodeCategory Multiply        = new("multiply", MtlxPort.Color3);
    public static readonly NodeCategory Add             = new("add", MtlxPort.Vector3);
    public static readonly NodeCategory Subtract        = new("subtract", MtlxPort.Vector3);
    public static readonly NodeCategory Divide          = new("divide", MtlxPort.Float);
    public static readonly NodeCategory Modulo          = new("modulo", MtlxPort.Float);
    public static readonly NodeCategory Sqrt            = new("sqrt", MtlxPort.Float);
    public static readonly NodeCategory Absval          = new("absval", MtlxPort.Float);
    public static readonly NodeCategory Sin             = new("sin", MtlxPort.Float);
    public static readonly NodeCategory Cos             = new("cos", MtlxPort.Float);
    public static readonly NodeCategory Min             = new("min", MtlxPort.Float);
    public static readonly NodeCategory Max             = new("max", MtlxPort.Float);
    public static readonly NodeCategory Power           = new("power", MtlxPort.Float);
    public static readonly NodeCategory Invert          = new("invert", MtlxPort.Float);
    public static readonly NodeCategory DotProduct      = new("dotproduct", MtlxPort.Float);
    public static readonly NodeCategory CrossProduct    = new("crossproduct", MtlxPort.Vector3);
    public static readonly NodeCategory Normalize       = new("normalize", MtlxPort.Vector3);
    public static readonly NodeCategory Clamp           = new("clamp", MtlxPort.Float);
    public static readonly NodeCategory Atan2           = new("atan2", MtlxPort.Float);
    public static readonly NodeCategory Exp             = new("exp", MtlxPort.Float);
    public static readonly NodeCategory Ln              = new("ln", MtlxPort.Float);
    public static readonly NodeCategory Sign            = new("sign", MtlxPort.Float);
    public static readonly NodeCategory Floor           = new("floor", MtlxPort.Float);
    public static readonly NodeCategory Ceil            = new("ceil", MtlxPort.Float);
    public static readonly NodeCategory Round           = new("round", MtlxPort.Float);
    public static readonly NodeCategory Magnitude       = new("magnitude", MtlxPort.Float);
    public static readonly NodeCategory Distance        = new("distance", MtlxPort.Float);
    public static readonly NodeCategory Smoothstep      = new("smoothstep", MtlxPort.Float);
    public static readonly NodeCategory Remap           = new("remap", MtlxPort.Float);
    public static readonly NodeCategory Range           = new("range", MtlxPort.Float);
    public static readonly NodeCategory Contrast        = new("contrast", MtlxPort.Float);
    public static readonly NodeCategory IfGreater       = new("ifgreater", MtlxPort.Float);
    public static readonly NodeCategory IfEqual         = new("ifequal", MtlxPort.Float);
    public static readonly NodeCategory Switch          = new("switch", MtlxPort.Float);
    public static readonly NodeCategory Mix             = new("mix", MtlxPort.Color3);
    public static readonly NodeCategory Screen          = new("screen", MtlxPort.Color3);
    public static readonly NodeCategory Overlay         = new("overlay", MtlxPort.Color3);
    public static readonly NodeCategory Dodge           = new("dodge", MtlxPort.Color3);
    public static readonly NodeCategory Burn            = new("burn", MtlxPort.Color3);
    public static readonly NodeCategory Difference      = new("difference", MtlxPort.Color3);
    public static readonly NodeCategory Normalmap       = new("normalmap", MtlxPort.Vector3);
    public static readonly NodeCategory OpenPbrSurface  = new("open_pbr_surface", MtlxPort.Surface);
    public static readonly NodeCategory StandardSurface = new("standard_surface", MtlxPort.Surface);
    public MtlxPort Port { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct MtlxInput(string Name, MtlxPort Type, string Value, Option<string> NodeName);

public sealed record MtlxNode(string Name, NodeCategory Category, MtlxPort Type, Seq<MtlxInput> Inputs);

public sealed record MtlxDocument(string Version, Seq<MtlxNode> Nodes, string SurfaceNode, string MaterialName, Seq<string> LossyEdges) {
    public const string Schema = "1.39";
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Mtlx {
    static readonly FrozenDictionary<NodeCategory, ImmutableArray<string>> FileSlots =
        new Dictionary<NodeCategory, ImmutableArray<string>> {
            [NodeCategory.Image] = ["file"],
            [NodeCategory.TiledImage] = ["file"],
            [NodeCategory.Triplanar] = ["filex", "filey", "filez"],
        }.ToFrozenDictionary();

    static readonly FrozenSet<TextureChannel> GeometryPorts = new[] {
        TextureChannel.GeometryNormal, TextureChannel.GeometryCoatNormal,
        TextureChannel.GeometryTangent, TextureChannel.GeometryCoatTangent,
    }.ToFrozenSet();

    static readonly FrozenSet<TextureChannel> NormalWrapped = new[] {
        TextureChannel.GeometryNormal, TextureChannel.GeometryCoatNormal,
    }.ToFrozenSet();

    public static NodeCategory CategoryOf(TextureSource source) =>
        NodeCategory.TryGet(source.MtlxCategory, out NodeCategory? category) ? category! : NodeCategory.Image;

    public static Fin<MtlxDocument> FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters, Op key, HashMap<PortId, TextureSource> textures = default) =>
        graph.Nodes.TraverseM(n => Row(n, parameters, textures, key).Map(row => (n.Id, row))).As().Map(toHashMap)
            .Bind(ports =>
                from _ in guard(ports.Count == graph.Nodes.Count, new MaterialFault.Graph(key, "<mtlx-duplicate-node-id>"))
                from __ in guard(graph.Nodes.Exists(n => n.Id == graph.Sink && n is AppearanceNode.BsdfOutput),
                    new MaterialFault.Graph(key, "<mtlx-sink-not-bsdf-output>"))
                from ___ in guard(graph.Nodes.ForAll(n => n.Dependencies.ForAll(ports.ContainsKey)),
                    new MaterialFault.Graph(key, "<mtlx-dangling-edge>"))
                from emitted in graph.Nodes.TraverseM(n => Emit(n, ports, parameters, textures, key)).As()
                let lossy = toSeq(ports.Values).Bind(shape => shape.Mix.Bind(static row => row.Lossy).ToSeq())
                          + toSeq(textures.Values).Bind(static source => source.MtlxLossy.ToSeq())
                select new MtlxDocument(MtlxDocument.Schema, emitted.Bind(static group => group),
                    $"node{graph.Sink.Value}", id.Value.Replace('.', '_'), lossy.Strict()));

    static Fin<(NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> Row(AppearanceNode node, MaterialParameters parameters, HashMap<PortId, TextureSource> textures, Op key) =>
        node.Switch(
            state: (Parameters: parameters, Textures: textures, Key: key),
            input:      static (s, i) => Fin.Succ((NodeCategory.Constant, PortOf(i.Pull(s.Parameters)), Option<MixProjection>.None)),
            texture:    static (s, t) => Fin.Succ(Categorized(s.Textures.Find(t.Id))),
            math:       static (s, m) => MathRow(m.Op, s.Key).Map(static row => (row.Item1, row.Item2, Option<MixProjection>.None)),
            mix:        static (s, x) => MixRow(x.Op, s.Key).Map(static row =>
                            (row.Traits.Admits(MixTrait.Lowered) ? NodeCategory.Mix : row.Category, MtlxPort.Color3, Some(row))),
            normal:     static (_, _) => Fin.Succ((NodeCategory.Normalmap, MtlxPort.Vector3, Option<MixProjection>.None)),
            bsdfOutput: static (_, _) => Fin.Succ((NodeCategory.OpenPbrSurface, MtlxPort.Surface, Option<MixProjection>.None)));

    static readonly FrozenDictionary<MathOp, ImmutableArray<string>> NamedPorts = new (MathOp Row, string[] Ports)[] {
        (MathOp.Smoothstep, ["in", "low", "high"]),
        (MathOp.Contrast,   ["in", "amount", "pivot"]),
        (MathOp.Remap,      ["in", "inlow", "inhigh", "outlow", "outhigh"]),
        (MathOp.Range,      ["in", "inlow", "inhigh", "gamma", "outlow", "outhigh"]),
        (MathOp.IfGreater,  ["value1", "value2", "in1", "in2"]),
        (MathOp.IfEqual,    ["value1", "value2", "in1", "in2"]),
    }.ToFrozenDictionary(static e => e.Row, static e => ImmutableArray.Create(e.Ports));

    static class MtlxPorts {
        public static Seq<string> Of(MathOp op, int operands) =>
            op == MathOp.Pick
                ? Seq("which") + toSeq(Enumerable.Range(1, Math.Max(operands - 1, 0)).Select(static slot => $"in{slot}"))
                : NamedPorts.TryGetValue(op, out ImmutableArray<string> named)
                    ? toSeq(named.Take(operands))
                    : operands == 1
                        ? Seq("in")
                        : toSeq(Enumerable.Range(1, operands).Select(static slot => $"in{slot}"));
    }

    static Fin<(NodeCategory, MtlxPort)> MathRow(MathOp op, Op key) =>
        op.Switch(
            add:          () => Fin.Succ((NodeCategory.Add, MtlxPort.Vector3)),
            subtract:     () => Fin.Succ((NodeCategory.Subtract, MtlxPort.Vector3)),
            multiply:     () => Fin.Succ((NodeCategory.Multiply, MtlxPort.Float)),
            divide:       () => Fin.Succ((NodeCategory.Divide, MtlxPort.Float)),
            modulo:       () => Fin.Succ((NodeCategory.Modulo, MtlxPort.Float)),
            scale:        () => Fin.Succ((NodeCategory.Multiply, MtlxPort.Vector3)),
            power:        () => Fin.Succ((NodeCategory.Power, MtlxPort.Float)),
            sqrt:         () => Fin.Succ((NodeCategory.Sqrt, MtlxPort.Float)),
            abs:          () => Fin.Succ((NodeCategory.Absval, MtlxPort.Float)),
            sin:          () => Fin.Succ((NodeCategory.Sin, MtlxPort.Float)),
            cos:          () => Fin.Succ((NodeCategory.Cos, MtlxPort.Float)),
            min:          () => Fin.Succ((NodeCategory.Min, MtlxPort.Float)),
            max:          () => Fin.Succ((NodeCategory.Max, MtlxPort.Float)),
            dotProduct:   () => Fin.Succ((NodeCategory.DotProduct, MtlxPort.Float)),
            crossProduct: () => Fin.Succ((NodeCategory.CrossProduct, MtlxPort.Vector3)),
            normalize:    () => Fin.Succ((NodeCategory.Normalize, MtlxPort.Vector3)),
            clamp01:      () => Fin.Succ((NodeCategory.Clamp, MtlxPort.Float)),
            oneMinus:     () => Fin.Succ((NodeCategory.Invert, MtlxPort.Float)),
            atan2:        () => Fin.Succ((NodeCategory.Atan2, MtlxPort.Float)),
            exp:          () => Fin.Succ((NodeCategory.Exp, MtlxPort.Float)),
            ln:           () => Fin.Succ((NodeCategory.Ln, MtlxPort.Float)),
            sign:         () => Fin.Succ((NodeCategory.Sign, MtlxPort.Float)),
            floor:        () => Fin.Succ((NodeCategory.Floor, MtlxPort.Float)),
            ceil:         () => Fin.Succ((NodeCategory.Ceil, MtlxPort.Float)),
            round:        () => Fin.Succ((NodeCategory.Round, MtlxPort.Float)),
            magnitude:    () => Fin.Succ((NodeCategory.Magnitude, MtlxPort.Float)),
            distance:     () => Fin.Succ((NodeCategory.Distance, MtlxPort.Float)),
            smoothstep:   () => Fin.Succ((NodeCategory.Smoothstep, MtlxPort.Float)),
            remap:        () => Fin.Succ((NodeCategory.Remap, MtlxPort.Float)),
            range:        () => Fin.Succ((NodeCategory.Range, MtlxPort.Float)),
            contrast:     () => Fin.Succ((NodeCategory.Contrast, MtlxPort.Float)),
            ifGreater:    () => Fin.Succ((NodeCategory.IfGreater, MtlxPort.Float)),
            ifEqual:      () => Fin.Succ((NodeCategory.IfEqual, MtlxPort.Float)),
            pick:         () => Fin.Succ((NodeCategory.Switch, MtlxPort.Float)),
            fresnel:      () => Fin.Fail<(NodeCategory, MtlxPort)>(new MaterialFault.Graph(key, "<mtlx-no-category:fresnel-weight>")));

    static readonly CapabilitySet<MixTrait> Lowered = CapabilitySet<MixTrait>.Of(MixTrait.Lowered);
    static readonly CapabilitySet<MixTrait> LoweredSwapped = CapabilitySet<MixTrait>.Of(MixTrait.Lowered, MixTrait.Swapped);

    readonly record struct MixProjection(
        NodeCategory Category, CapabilitySet<MixTrait> Traits = default, Option<string> Lossy = default);

    static readonly FrozenDictionary<BlendMode, MixProjection> MixRows = new Dictionary<BlendMode, MixProjection> {
        [BlendMode.Normal]      = new(NodeCategory.Mix),
        [BlendMode.Multiply]    = new(NodeCategory.Multiply, Lowered),
        [BlendMode.Screen]      = new(NodeCategory.Screen),
        [BlendMode.Overlay]     = new(NodeCategory.Overlay),
        [BlendMode.Darken]      = new(NodeCategory.Min, Lowered),
        [BlendMode.Lighten]     = new(NodeCategory.Max, Lowered),
        [BlendMode.ColourDodge] = new(NodeCategory.Dodge),
        [BlendMode.ColourBurn]  = new(NodeCategory.Burn),
        [BlendMode.HardLight]   = new(NodeCategory.Overlay, LoweredSwapped),
        [BlendMode.Difference]  = new(NodeCategory.Difference),
        [BlendMode.SoftLight]   = new(NodeCategory.Overlay, Lossy: "soft-light-as-overlay"),
        [BlendMode.Exclusion]   = new(NodeCategory.Difference, Lossy: "exclusion-as-difference"),
        [BlendMode.Hue]         = new(NodeCategory.Mix, Lossy: "hue-as-lerp"),
        [BlendMode.Saturation]  = new(NodeCategory.Mix, Lossy: "saturation-as-lerp"),
        [BlendMode.Colour]      = new(NodeCategory.Mix, Lossy: "colour-as-lerp"),
        [BlendMode.Luminosity]  = new(NodeCategory.Mix, Lossy: "luminosity-as-lerp"),
    }.ToFrozenDictionary();

    static Fin<MixProjection> MixRow(MixOp op, Op key) =>
        MixRows.TryGetValue(op.Mode, out MixProjection row)
            ? Fin.Succ(row)
            : Fin.Fail<MixProjection>(new MaterialFault.Graph(key, $"<mtlx-no-category:{op.Key}>"));

    static (NodeCategory, MtlxPort, Option<MixProjection>) Categorized(Option<TextureSource> source) {
        NodeCategory category = source.Map(CategoryOf).IfNone(NodeCategory.Image);
        return (category, category.Port, Option<MixProjection>.None);
    }

    static Fin<Seq<MtlxNode>> Emit(AppearanceNode node, HashMap<PortId, (NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> ports, MaterialParameters parameters, HashMap<PortId, TextureSource> textures, Op key) =>
        node.Switch(
            state: (Ports: ports, Parameters: parameters, Textures: textures, Key: key),
            input:      static (s, i) => Fin.Succ(One(i.Id, s.Ports, Constant(i.Pull(s.Parameters)))),
            texture:    static (s, t) => Fin.Succ(One(t.Id, s.Ports,
                (FileSlots.TryGetValue(s.Ports[t.Id].Category, out ImmutableArray<string> slots)
                    ? toSeq(slots.Select(static slot => new MtlxInput(slot, MtlxPort.Filename, string.Empty, Option<string>.None)))
                    : s.Textures.Find(t.Id).Map(TextureInputs).IfNone(Seq<MtlxInput>()))
                + t.Parameter.Map(driver => Seq(Edge("texcoord", driver, s.Ports))).IfNone(Seq<MtlxInput>()))),
            math:       static (s, m) => Fin.Succ(One(m.Id, s.Ports,
                MtlxPorts.Of(m.Op, m.Operands.Count)
                    .Map((slot, index) => Edge(slot, m.Operands[index], s.Ports)))),
            mix:        static (s, x) => s.Ports[x.Id].Mix
                            .Map(row => Fin.Succ(MixNodes(x, row, s.Ports)))
                            .IfNone(() => Fin.Fail<Seq<MtlxNode>>(new MaterialFault.Graph(s.Key, $"<mtlx-mix-unresolved:{x.Id.Value}>"))),
            normal:     static (s, n) => Fin.Succ(One(n.Id, s.Ports, Seq(Edge("in", n.Source, s.Ports), Value("scale", MtlxPort.Float, n.Strength)))),
            bsdfOutput: static (s, o) => Fin.Succ(One(o.Id, s.Ports, Seq(
                Edge("base_color", o.BaseColor, s.Ports, Some(MtlxPort.Color3)),
                Edge("base_metalness", o.Metalness, s.Ports, Some(MtlxPort.Float)),
                Edge("specular_roughness", o.Roughness, s.Ports, Some(MtlxPort.Float)),
                Edge("geometry_normal", o.NormalFrame, s.Ports, Some(MtlxPort.Vector3)),
                Edge("emission_color", o.Emission, s.Ports, Some(MtlxPort.Color3))))));

    static Seq<MtlxNode> One(PortId id, HashMap<PortId, (NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> ports, Seq<MtlxInput> inputs) =>
        Seq(new MtlxNode($"node{id.Value}", ports[id].Category, ports[id].Port, inputs));

    static Seq<MtlxInput> TextureInputs(TextureSource source) =>
        source.MtlxParameters.Map(static row =>
            new MtlxInput(row.Name, MtlxPort.TryGet(row.Type, out MtlxPort? port) ? port! : MtlxPort.Float, row.Value, Option<string>.None));

    static Seq<MtlxNode> MixNodes(AppearanceNode.Mix x, MixProjection projection, HashMap<PortId, (NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> ports) =>
        projection switch {
            var row when !row.Traits.Admits(MixTrait.Lowered) => Seq(new MtlxNode($"node{x.Id.Value}", row.Category, MtlxPort.Color3,
                Seq(Edge("fg", x.B, ports), Edge("bg", x.A, ports), Edge("mix", x.Factor, ports)))),
            var row => Seq(
                new MtlxNode($"node{x.Id.Value}b", row.Category, MtlxPort.Color3, row.Traits.Admits(MixTrait.Swapped)
                    ? Seq(Edge("fg", x.A, ports), Edge("bg", x.B, ports))
                    : Seq(Edge("in1", x.A, ports), Edge("in2", x.B, ports))),
                new MtlxNode($"node{x.Id.Value}", NodeCategory.Mix, MtlxPort.Color3, Seq(
                    new MtlxInput("fg", MtlxPort.Color3, string.Empty, Some($"node{x.Id.Value}b")),
                    Edge("bg", x.A, ports),
                    Edge("mix", x.Factor, ports)))),
        };

    static Seq<MtlxInput> Constant(PortValue pulled) =>
        Seq(new MtlxInput("value", PortOf(pulled), Render(pulled), Option<string>.None));

    static MtlxPort PortOf(PortValue value) => value.Switch(
        scalar: static _ => MtlxPort.Float,
        color:  static _ => MtlxPort.Color3,
        vector: static _ => MtlxPort.Vector3,
        frame:  static _ => MtlxPort.Vector3);

    static string Render(PortValue value) => value.Switch(
        scalar: static v => Num(v.Value),
        color:  static c => Triple(c.Linear.RgbLinear.R, c.Linear.RgbLinear.G, c.Linear.RgbLinear.B),
        vector: static v => Triple(v.Value.X, v.Value.Y, v.Value.Z),
        frame:  static f => Triple(f.Value.ZAxis.X, f.Value.ZAxis.Y, f.Value.ZAxis.Z));

    static MtlxInput Edge(string name, PortId source, HashMap<PortId, (NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> ports, Option<MtlxPort> slot = default) =>
        new(name, slot.IfNone(() => ports[source].Port), string.Empty, Some($"node{source.Value}"));

    public static Fin<MtlxDocument> ToOpenPbr(Wire.Material wire, Option<Wire.Set> planes, Op key) =>
        string.IsNullOrWhiteSpace(wire.Id)
            ? Fin.Fail<MtlxDocument>(new MaterialFault.Graph(key, "<mtlx-empty-material-id>"))
            : planes
                .Map(set => toSeq(set.Planes).Traverse(row => Bound(row, key)).As()
                    .Map(bound => Document(wire, bound.Bind(static entry => entry.Nodes).Add(Textured(wire, bound)))))
                .IfNone(() => Fin.Succ(Document(wire, Seq(SurfaceNode(wire)))));

    static MtlxDocument Document(Wire.Material wire, Seq<MtlxNode> nodes) =>
        new(MtlxDocument.Schema, nodes, "node0", wire.Id.Replace('.', '_'), Seq<string>());

    static readonly ImmutableArray<(string Port, MtlxPort Type, Func<Wire.OpenPbr, string> Text)> OpenPbrPorts = [
        ("base_weight", MtlxPort.Float, static g => Num(g.BaseWeight)),
        ("base_color", MtlxPort.Color3, static g => Rgb(g.BaseColor)),
        ("base_metalness", MtlxPort.Float, static g => Num(g.BaseMetalness)),
        ("base_diffuse_roughness", MtlxPort.Float, static g => Num(g.BaseDiffuseRoughness)),
        ("specular_weight", MtlxPort.Float, static g => Num(g.SpecularWeight)),
        ("specular_color", MtlxPort.Color3, static g => Rgb(g.SpecularColor)),
        ("specular_roughness", MtlxPort.Float, static g => Num(g.SpecularRoughness)),
        ("specular_ior", MtlxPort.Float, static g => Num(g.SpecularIor)),
        ("specular_roughness_anisotropy", MtlxPort.Float, static g => Num(g.SpecularAnisotropy)),
        ("specular_roughness_anisotropy_rotation", MtlxPort.Float, static g => Num(g.SpecularRotation)),
        ("transmission_weight", MtlxPort.Float, static g => Num(g.TransmissionWeight)),
        ("subsurface_weight", MtlxPort.Float, static g => Num(g.SubsurfaceWeight)),
        ("subsurface_radius", MtlxPort.Float, static g => Num(RadiusMax(g))),
        ("subsurface_radius_scale", MtlxPort.Color3, RadiusScale),
        ("coat_weight", MtlxPort.Float, static g => Num(g.CoatWeight)),
        ("coat_color", MtlxPort.Color3, static g => Rgb(g.CoatColor)),
        ("coat_roughness", MtlxPort.Float, static g => Num(g.CoatRoughness)),
        ("coat_ior", MtlxPort.Float, static g => Num(g.CoatIor)),
        ("fuzz_weight", MtlxPort.Float, static g => Num(g.FuzzWeight)),
        ("fuzz_color", MtlxPort.Color3, static g => Rgb(g.FuzzColor)),
        ("fuzz_roughness", MtlxPort.Float, static g => Num(g.FuzzRoughness)),
        ("thin_film_weight", MtlxPort.Float, static g => Num(g.ThinFilmWeight)),
        ("thin_film_thickness", MtlxPort.Float, static g => Num(g.ThinFilmThickness / 1000.0)),
        ("thin_film_ior", MtlxPort.Float, static g => Num(g.ThinFilmIor)),
        ("emission_color", MtlxPort.Color3, static g => Rgb(g.EmissionColor)),
        ("emission_luminance", MtlxPort.Float, static g => Num(g.EmissionLuminance)),
        ("geometry_opacity", MtlxPort.Float, static g => Num(g.GeometryOpacity)),
        ("geometry_thin_walled", MtlxPort.Boolean, static g => g.GeometryThinWalled ? "true" : "false"),
    ];

    static MtlxNode SurfaceNode(Wire.Material wire) =>
        new("node0", NodeCategory.OpenPbrSurface, MtlxPort.Surface,
            toSeq(OpenPbrPorts.Select(row => new MtlxInput(row.Port, row.Type, row.Text(wire.OpenPbr), Option<string>.None))));

    static double RadiusMax(Wire.OpenPbr g) => Math.Max(g.SubsurfaceRadius.R, Math.Max(g.SubsurfaceRadius.G, g.SubsurfaceRadius.B));

    static string RadiusScale(Wire.OpenPbr g) =>
        RadiusMax(g) is > 0.0 and var max
            ? Triple(g.SubsurfaceRadius.R / max, g.SubsurfaceRadius.G / max, g.SubsurfaceRadius.B / max)
            : Triple(1.0, 1.0, 1.0);

    static Fin<(TextureChannel Channel, Seq<MtlxNode> Nodes, string Source)> Bound(Wire.Plane row, Op key) =>
        WireVocabulary.Channel(row.Role).Case is TextureChannel channel
            ? BaseLeaf(row, key).Map(file => channel.Mtlx switch {
                MtlxBinding.Scaled scaled => (channel, Seq(
                        Image(channel, file),
                        new MtlxNode($"scale_{channel.Key}", NodeCategory.Multiply, Lane(channel), Seq(
                            new MtlxInput("in1", Lane(channel), string.Empty, Some($"tex_{channel.Key}")),
                            Value("in2", MtlxPort.Float, scaled.Factor)))),
                    $"scale_{channel.Key}"),
                MtlxBinding.Absent => (channel, Seq<MtlxNode>(), string.Empty),
                _ when NormalWrapped.Contains(channel) => (channel, Seq(
                        Image(channel, file),
                        new MtlxNode($"nrm_{channel.Key}", NodeCategory.Normalmap, MtlxPort.Vector3,
                            Seq(new MtlxInput("in", MtlxPort.Vector3, string.Empty, Some($"tex_{channel.Key}"))))),
                    $"nrm_{channel.Key}"),
                _ => (channel, Seq(Image(channel, file)), $"tex_{channel.Key}"),
            })
            : Fin.Fail<(TextureChannel, Seq<MtlxNode>, string)>(new MaterialFault.Graph(key, $"<mtlx-unknown-channel:{row.Role}>"));

    static Fin<string> BaseLeaf(Wire.Plane row, Op key) =>
        toSeq(row.Levels).Head
            .Map(static level => level.File)
            .ToFin(new MaterialFault.Graph(key, $"<mtlx-channel-unaddressed:{row.Role}>"));

    static MtlxNode Image(TextureChannel channel, string file) =>
        new($"tex_{channel.Key}", NodeCategory.TiledImage, Lane(channel),
            Seq(new MtlxInput("file", MtlxPort.Filename, file, Option<string>.None)));

    static MtlxNode Textured(Wire.Material wire, Seq<(TextureChannel Channel, Seq<MtlxNode> Nodes, string Source)> bound) =>
        bound.Fold(SurfaceNode(wire), static (node, entry) => entry.Channel.Mtlx is MtlxBinding.Absent
            ? node
            : node.Inputs.Exists(input => input.Name == PortNameOf(entry.Channel))
                ? node with {
                    Inputs = node.Inputs.Map(input => input.Name == PortNameOf(entry.Channel)
                        ? input with { Value = string.Empty, NodeName = Some(entry.Source) }
                        : input),
                }
                : node with {
                    Inputs = node.Inputs.Add(new MtlxInput(PortNameOf(entry.Channel), Lane(entry.Channel), string.Empty, Some(entry.Source))),
                });

    static string PortNameOf(TextureChannel channel) =>
        channel.Mtlx switch {
            MtlxBinding.Lowered lowered => lowered.Input,
            MtlxBinding.Split split => split.ScaleInput,
            _ => channel.Key,
        };

    static MtlxPort Lane(TextureChannel channel) =>
        GeometryPorts.Contains(channel) ? MtlxPort.Vector3
        : channel.Components is 1 ? MtlxPort.Float : MtlxPort.Color3;

    static MtlxInput Value(string name, MtlxPort type, double v) => new(name, type, Num(v), Option<string>.None);
    static string Num(double v) => v.ToString("R", CultureInfo.InvariantCulture);
    static string Rgb(Wire.Color c) => Triple(c.R, c.G, c.B);
    static string Triple(double r, double g, double b) => $"{Num(r)}, {Num(g)}, {Num(b)}";
}
```

## [04]-[STAGE_CROSSING]

- Entry: `public static Fin<StageRequestWire> Request(StageRequest request)` transcribes, fills the four columns no generated mapper may construct, and crosses `WireAdmission.Admit` once, keying every refusal on the request's own `Op`; `public static Fin<StageResult> Admit(StageResultWire wire, ModelCard card, StageRequest request, Op key)` admits the inbound message, proves the correlation echo, lifts every vocabulary onto its Materials row, and hands the reconstructed result to the `neural#STAGE_PLAN` `StageResult.Admit` registry gate.
- Law: the GENERATED MESSAGES are the one vocabulary and no key string crosses for a closed roster. `defined_only` plus `not_in: [0]` refuse an undefined or unspecified ordinal inside `WireAdmission.Admit`, BEFORE any row lift runs, so the inbound `switch` over the generated enum answers a value the corpus already proved rostered and its `_` arm names a value only a generation skew can produce.
- Law: the CHANNEL ROLE crosses as the appearance roster's own string spelling because that roster is Materials-owned and open to growth, where the stage, prior, and score rosters are corpus-closed. `TextureChannel.TryGet` is its admission and the `^[a-z][a-z0-9_]*$` field rule its shape gate, so a channel landing at `Raster/set#TEXTURE_CHANNEL` reaches this crossing with no proto edit while every closed roster still breaks the build when it grows.
- Law: the two rosters the product oneof discriminates stay DISJOINT. `StageProduct.Parse` resolves CHANNELS first, so a key both rosters claim would make the prior unreachable at the specifying end and mis-tagged at the executing end's inverse; disjointness is the invariant that keeps one product key readable from either direction, and a `PriorField` or `ScoreField` row spelling an appearance channel is the declaration that breaks it.
- Law: outbound and inbound read ONE correspondence in two directions and neither is a hand `(key, enum)` table. Row → wire enum is the row's generated total `Map`, one arm per row, so a new `PbrStage`, `LicenseClass`, `InferenceProvider`, or `TensorPrecision` row breaks THIS BUILD until the corpus enum carries its value; wire enum → row is one `switch` expression per roster refusing on the rail with the value named. The `[02]` `WireVocabulary.Total` derivation is deliberately not reached here: it answers a roster gap as a type-initializer throw, where this crossing's growth law demands a compile break in the same change as the corpus edit.
- Law: no generated mapper is asked to construct a value whose construction can REFUSE. The optional artefact, the fixed bucket, the pad mode, and the input row set each lower by hand at `Request` — the first because proto3 `optional bytes` sits behind a null-rejecting setter that cannot spell absence, the second and third because they arrive as producer-interior strings the corpus types, and the fourth because a get-only `RepeatedField` fill is not an admitted conversion at `MappingConversionType.None`. `[MapperIgnoreSource]`/`[MapperIgnoreTarget]` name each at the mapper, so the RMG completeness gate still proves every remaining column.
- Law: `Layout` never crosses. The tensor dimension order is the leased model card's fact at the executing end, so a request column carrying it could only ever contradict the lease; the waiver is a compiler-proved `[MapperIgnoreSource]` row rather than authored inventory, because no `[MapPropertyFromSource]` reader rides this mapping.
- Law: identity crosses as BYTES and never as text. Every content key rides the sixteen big-endian bytes `ContentHash.Wire` publishes and re-enters through `ContentHash.Admit`, so the seam's `X32` rendering and the kernel's `x32` rendering never meet on this wire and a width other than sixteen refuses at the field rule.
- Law: the correlation echo is proved BEFORE any vocabulary lift. The executor echoes `op` verbatim, so a result naming another request refuses at the first guard; without it the correlation column is write-only and a transposed result would be admitted against the wrong card.
- Law: completeness, extent congruence, plane shape, partition bound, and the residual band are proved by their OWNER. `StageResult.Admit` at `neural#STAGE_PLAN` reads the card and the minting request; this cluster reconstructs the typed result and decides none of those, so a second copy of the gate cannot drift from the first.
- Growth: a new stage COLUMN is one numbered proto field, regenerated at both ends, with the RMG completeness diagnostics forcing its transcription here and the peer's own mapper forcing the counterpart. A new stage, provider, or precision ROW is one enum value at the corpus, one `Map` arm outbound, and one `switch` arm inbound — each of the three a compile break rather than a runtime surprise. A new appearance channel is a `Raster/set#TEXTURE_CHANNEL` row alone.
- Boundary: this cluster PROJECTS and never decides. `neural#MODEL_REGISTRY` owns the stage, licence, provider, precision, prior, and score vocabularies and `Raster/set#TEXTURE_CHANNEL` the channel roster; the relaying root moves proto-binary bytes with no `WireJson` rendering and no decode, so the only two surfaces reading a stage message are the two ends the manifest case names.

```csharp

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StageWire {
    public static Fin<StageRequestWire> Request(StageRequest request) =>
        from bucket in Bucket(request.Bucket, request.Op)
        from pad in Pad(request.PadMode, request.Op)
        let wire = Fill(AppearanceWireMap.ToWire(request), request, bucket, pad)
        from admitted in WireAdmission.Admit(wire, WireBoundary.OutboundPayload, request.Op)
        select admitted;

    static StageRequestWire Fill(StageRequestWire wire, StageRequest request, BucketWire bucket, PadModeWire pad) {
        wire.Bucket = bucket;
        wire.Pad = pad;
        wire.Inputs.AddRange(request.Inputs.Map(AppearanceWireMap.Input));
        request.Artefact.Iter(digest => wire.Artefact = ContentHash.Wire(digest.Value));
        return wire;
    }

    static Fin<BucketWire> Bucket(string spelling, Op key) =>
        spelling.Split('x') switch {
            [var w, var h] when uint.TryParse(w, NumberStyles.None, CultureInfo.InvariantCulture, out uint width)
                             && uint.TryParse(h, NumberStyles.None, CultureInfo.InvariantCulture, out uint height)
                             && width > 0 && height > 0 =>
                Fin.Succ(new BucketWire { Width = width, Height = height }),
            _ => new MaterialFault.Parameter(key, $"<stage-bucket-unparsable:{spelling}>"),
        };

    static Fin<PadModeWire> Pad(string spelling, Op key) =>
        Enum.TryParse(spelling, ignoreCase: true, out PadModeWire pad) && pad is not PadModeWire.Unspecified
            ? Fin.Succ(pad)
            : new MaterialFault.Parameter(key, $"<stage-pad-unrostered:{spelling}>");

    public static Fin<StageResult> Admit(StageResultWire wire, ModelCard card, StageRequest request, Op key) =>
        from message in WireAdmission.Admit(wire, WireBoundary.InboundPayload, key)
        from _echo in guard(message.Op == key.ToString(), new MaterialFault.Parameter(key, $"<stage-op-echo:{message.Op}>"))
        from stage in Row(message.Stage, key)
        from provider in Row(message.ProviderUsed, key)
        from artefact in Address(message.Artefact, key)
        from outputs in toSeq(message.Outputs).Traverse(output => Output(output, key)).As()
        from scores in toSeq(message.Scores).Traverse(score =>
            Product(score.Role, key).Map(product => new StageScore(product, score.Value))).As()
        from echoed in key.AcceptValidated<ModelCardId>(ModelCardId.Validate(message.ModelCardId, null, out ModelCardId id), id)
        from admitted in StageResult.Admit(
            new StageResult(stage, echoed, artefact, outputs, scores, provider,
                checked((int)message.PartitionCount), message.Elapsed.ToNodaDuration().TotalMilliseconds,
                message.ReferenceDelta, message.ParityFresh, message.Coverage, checked((int)message.TilesEmitted), key),
            card, request, key)
        select admitted;

    static Fin<StageOutput> Output(StageOutputWire output, Op key) =>
        from product in Product(output.Role, key)
        from transfer in Row(output.Transfer, key)
        from format in Row(output.Format, key)
        from blob in Address(output.Blob, key)
        select new StageOutput(product, blob, Dimension.Create(checked((int)output.Width)), Dimension.Create(checked((int)output.Height)), transfer, format);

    static Fin<StageProduct> Product(StageProductWire role, Op key) =>
        role.RoleCase switch {
            StageProductWire.RoleOneofCase.Channel =>
                TextureChannel.TryGet(role.Channel, out TextureChannel? channel)
                    ? Fin.Succ<StageProduct>(new StageProduct.Channel(channel!))
                    : Refused<StageProduct>(key, "channel", role.Channel),
            StageProductWire.RoleOneofCase.Prior => Row(role.Prior, key).Map(static field => (StageProduct)new StageProduct.Prior(field)),
            StageProductWire.RoleOneofCase.Measure => Row(role.Measure, key).Map(static field => (StageProduct)new StageProduct.Measure(field)),
            var absent => Refused<StageProduct>(key, "product", absent.ToString()),
        };

    static Fin<ContentAddress> Address(ByteString bytes, Op key) =>
        ContentHash.Admit(bytes.Span, key).Map(ContentAddress.Of);

    static Fin<PbrStage> Row(PbrStageWire wire, Op key) =>
        wire switch {
            PbrStageWire.Delight => PbrStage.Delight,
            PbrStageWire.Albedo => PbrStage.Albedo,
            PbrStageWire.Normals => PbrStage.Normals,
            PbrStageWire.Depth => PbrStage.Depth,
            PbrStageWire.Svbrdf => PbrStage.Svbrdf,
            PbrStageWire.IntrinsicAppearance => PbrStage.IntrinsicAppearance,
            PbrStageWire.SpectralReflectance => PbrStage.SpectralReflectance,
            PbrStageWire.SuperResolve => PbrStage.SuperResolve,
            PbrStageWire.Tileability => PbrStage.Tileability,
            _ => Refused<PbrStage>(key, "stage", wire.ToString()),
        };

    static Fin<InferenceProvider> Row(InferenceProviderWire wire, Op key) =>
        wire switch {
            InferenceProviderWire.Cpu => InferenceProvider.Cpu,
            InferenceProviderWire.CoreMl => InferenceProvider.CoreMl,
            InferenceProviderWire.WebGpu => InferenceProvider.WebGpu,
            _ => Refused<InferenceProvider>(key, "provider", wire.ToString()),
        };

    static Fin<PriorField> Row(PriorFieldWire wire, Op key) =>
        wire switch {
            PriorFieldWire.Delit => PriorField.Delit,
            PriorFieldWire.Depth => PriorField.Depth,
            PriorFieldWire.Spectral => PriorField.Spectral,
            _ => Refused<PriorField>(key, "prior", wire.ToString()),
        };

    static Fin<ScoreField> Row(ScoreFieldWire wire, Op key) =>
        wire switch {
            ScoreFieldWire.Tileability => ScoreField.Tileability,
            _ => Refused<ScoreField>(key, "score", wire.ToString()),
        };

    static Fin<PlaneTransfer> Row(PlaneTransferWire wire, Op key) =>
        wire switch {
            PlaneTransferWire.Linear => PlaneTransfer.Linear,
            PlaneTransferWire.Srgb => PlaneTransfer.Srgb,
            PlaneTransferWire.Raw => PlaneTransfer.Raw,
            PlaneTransferWire.Pq => PlaneTransfer.Pq,
            PlaneTransferWire.Hlg => PlaneTransfer.Hlg,
            _ => Refused<PlaneTransfer>(key, "transfer", wire.ToString()),
        };

    static Fin<PlaneFormat> Row(PlaneFormatWire wire, Op key) =>
        wire switch {
            PlaneFormatWire.R8 => PlaneFormat.R8,
            PlaneFormatWire.R16 => PlaneFormat.R16,
            PlaneFormatWire.R16F => PlaneFormat.R16F,
            PlaneFormatWire.R32F => PlaneFormat.R32F,
            PlaneFormatWire.Rg8 => PlaneFormat.Rg8,
            PlaneFormatWire.Rg16 => PlaneFormat.Rg16,
            PlaneFormatWire.Rg16F => PlaneFormat.Rg16F,
            PlaneFormatWire.Rg32F => PlaneFormat.Rg32F,
            PlaneFormatWire.Rgba8 => PlaneFormat.Rgba8,
            PlaneFormatWire.Rgba16 => PlaneFormat.Rgba16,
            PlaneFormatWire.Rgba16F => PlaneFormat.Rgba16F,
            PlaneFormatWire.Rgba32F => PlaneFormat.Rgba32F,
            _ => Refused<PlaneFormat>(key, "format", wire.ToString()),
        };

    static MaterialFault Refused(Op key, string axis, string value) => new MaterialFault.Graph(key, $"<stage-result-unknown-{axis}:{value}>");

    static Fin<T> Refused<T>(Op key, string axis, string value) => Fin.Fail<T>(Refused(key, axis, value));
}

public static partial class AppearanceWireMap {
    // --- [STAGE_REQUEST]
    [MapProperty(nameof(StageRequest.Stage), nameof(StageRequestWire.Stage), Use = nameof(Stage))]
    [MapProperty("ModelCardId.Value", nameof(StageRequestWire.ModelCardId))]
    [MapProperty(nameof(StageRequest.LicenseClass), nameof(StageRequestWire.License), Use = nameof(Licence))]
    [MapProperty("InputWidth.Value", nameof(StageRequestWire.InputWidth), Use = nameof(Unsigned))]
    [MapProperty("InputHeight.Value", nameof(StageRequestWire.InputHeight), Use = nameof(Unsigned))]
    [MapProperty("OutputWidth.Value", nameof(StageRequestWire.OutputWidth), Use = nameof(Unsigned))]
    [MapProperty("OutputHeight.Value", nameof(StageRequestWire.OutputHeight), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageRequest.TileWidth), nameof(StageRequestWire.TileWidth), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageRequest.TileHeight), nameof(StageRequestWire.TileHeight), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageRequest.Overlap), nameof(StageRequestWire.Overlap), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageRequest.Provider), nameof(StageRequestWire.Provider), Use = nameof(Provider))]
    [MapProperty(nameof(StageRequest.Precision), nameof(StageRequestWire.Precision), Use = nameof(Precision))]
    [MapProperty(nameof(StageRequest.Op), nameof(StageRequestWire.Op), Use = nameof(OpKey))]
    [MapperIgnoreSource(nameof(StageRequest.Inputs))]
    [MapperIgnoreSource(nameof(StageRequest.Artefact))]
    [MapperIgnoreSource(nameof(StageRequest.Bucket))]
    [MapperIgnoreSource(nameof(StageRequest.PadMode))]
    [MapperIgnoreSource(nameof(StageRequest.Layout))]
    [MapperIgnoreTarget(nameof(StageRequestWire.Inputs))]
    [MapperIgnoreTarget(nameof(StageRequestWire.Artefact))]
    [MapperIgnoreTarget(nameof(StageRequestWire.Bucket))]
    [MapperIgnoreTarget(nameof(StageRequestWire.Pad))]
    public static partial StageRequestWire ToWire(StageRequest request);

    static string OpKey(Op op) => op.ToString();

    // --- [STAGE_VOCABULARY]
    [UserMapping] static PbrStageWire Stage(PbrStage row) => row.Map(
        delight: PbrStageWire.Delight, albedo: PbrStageWire.Albedo, normals: PbrStageWire.Normals,
        depth: PbrStageWire.Depth, svbrdf: PbrStageWire.Svbrdf, intrinsicAppearance: PbrStageWire.IntrinsicAppearance,
        spectralReflectance: PbrStageWire.SpectralReflectance, superResolve: PbrStageWire.SuperResolve,
        tileability: PbrStageWire.Tileability);

    [UserMapping] static LicenseClassWire Licence(LicenseClass row) => row.Map(
        permissive: LicenseClassWire.Permissive, copyleft: LicenseClassWire.Copyleft, openRail: LicenseClassWire.OpenRail,
        research: LicenseClassWire.Research, blocked: LicenseClassWire.Blocked);

    [UserMapping] static InferenceProviderWire Provider(InferenceProvider row) => row.Map(
        cpu: InferenceProviderWire.Cpu, coreMl: InferenceProviderWire.CoreMl, webGpu: InferenceProviderWire.WebGpu);

    [UserMapping] static TensorPrecisionWire Precision(TensorPrecision row) => row.Map(
        fp32: TensorPrecisionWire.Fp32, fp16: TensorPrecisionWire.Fp16);

    static PriorFieldWire Field(PriorField row) => row.Map(
        delit: PriorFieldWire.Delit, depth: PriorFieldWire.Depth, spectral: PriorFieldWire.Spectral);

    static ScoreFieldWire Grade(ScoreField row) => row.Map(tileability: ScoreFieldWire.Tileability);

    // --- [STAGE_PRODUCTS]
    public static StageInputWire Input(StageInput input) => input.Switch<StageInputWire>(
        source: static row => new StageInputWire { Source = new StageInputWire.Types.Source { Key = ContentHash.Wire(row.Key.Value) } },
        produced: static row => new StageInputWire {
            Produced = new StageInputWire.Types.Produced { Stage = Stage(row.Stage), Product = Product(row.Product) },
        });

    static StageProductWire Product(StageProduct product) => product.Switch<StageProductWire>(
        channel: static row => new StageProductWire { Channel = row.Field.Key },
        prior: static row => new StageProductWire { Prior = Field(row.Field) },
        measure: static row => new StageProductWire { Measure = Grade(row.Field) });
}
```

## [05]-[RESEARCH]

(none)
