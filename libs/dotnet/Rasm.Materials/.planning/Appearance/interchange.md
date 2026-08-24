# [MATERIALS_INTERCHANGE]

This page owns the generated appearance egress and the MaterialX node-graph interchange. `AppearanceEgress.Project` mints the full `Material`; `AppearanceEgress.Set` mints `Set.baked`; `AppearanceEgress.Ibl` mints `Set.environment.ibl`. The generated `appearance` siblings own product identity and storage metadata, while `AppearanceWireMap` keeps the C# material transcription complete. MaterialX remains a separate graph interchange over the admitted material graph, and the stage crossing remains branch-interior MessagePack because no peer runtime decodes it.

## [01]-[INDEX]

- [02]-[MATERIAL_WIRE]: the generated `Rasm.Contracts.Appearance` family as the wire, `AppearanceEgress` lowering a library row to the seam `AppearanceSummary`, minting `Material`, and descriptor-admitting the completed `Set`/`Ibl` behind the appearance key, `AppearanceWireMap` the ONE `[Mapper]` under the RMG completeness gate, and `WireVocabulary` the type-init-proved bridges onto the generated enums.
- [03]-[MATERIALX_DOCUMENT]: `MtlxDocument`/`MtlxNode` shape the MaterialX 1.39 node graph, `NodeCategory` carries the per-category typed port, and `Mtlx` projects `AppearanceNode` over per-op category rows, source-typed edge polarities, probed constants, the texture-source binding, and the BAKED-FILENAME binding filling each image node's `file` slot from the set's own egress leaf, its `.mtlx` serialize/admit fold railing every unprojectable node.
- [04]-[STAGE_CROSSING]: the branch-interior photo-to-PBR crossing — `StageRequestRow`/`StageResultRow` and their input, output, and score rows, the `StageCodec` MessagePack leg lawful because no peer decodes it, the `StageRoster` derived from the rows' own `[Key(n)]` attributes, and the `Checksum` fold the relaying root compares against Compute's.

## [02]-[MATERIAL_WIRE]

- Owner: the generated `Rasm.Contracts.Appearance` family — `Material`, `Set`, and their appearance payloads — is the appearance wire; `Rasm.Contracts.Artifact.ArtifactRef` owns stored payload identity and extent inside every `PlaneRef`. `AppearanceEgress` owns the presence-sensitive material, provenance, set, and IBL projections; `AppearanceWireMap` owns reader-free transcriptions; `WireVocabulary` and `LicenceVocabulary` own enum admission, while the app spine's neutral `WireAdmission` owns the descriptor verdict.
- Entry: `public static Fin<AppearanceSummary> Summary(MaterialParameters parameters, Op key)` lowers a library row to the SEAM `AppearanceSummary` through the seam-owned `AppearanceSummary.Of` factory — the neutral PBR scalars with the `AppearanceKey` the factory mints (the kernel seed-zero `XxHash128` over the canonical PBR bytes, the ONE hasher) on the factory's own `Fin` rail, since it gates every channel to the unit range and takes the `Op` key rather than a tolerance. It is the CONTRACTED entry `Projection/component#COMPONENT_SUBGRAPH` `ComponentSubgraph.Capture` composes and the SAME factory `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` composes; the channel triple crosses as the landed Element `[ComplexValueObject]` `AppearanceVector.Create(...)`, whose accumulated slot gate names every offending channel at once.
- Entry: `AppearanceEgress.Project` mints the full OpenPBR material; `Set` projects an admitted baked surface into the generated `Set.baked` arm; `AppearanceEgress.Ibl` projects the resolved dome into `Set.environment.ibl`. Each completed document crosses `WireAdmission.Admit` once after its product is final.
- Packages: Rasm.Contracts (project — the generated `rasm.contracts.appearance` messages), Rasm.AppHost (project — neutral `WireAdmission` over the one descriptor-root evaluator), Google.Protobuf (`ByteString`/`RepeatedField<T>`/`WellKnownTypes.Duration.FromTimeSpan`), `Rasm` (`ArtifactContent` the SHA-256-plus-extent coordinate, `ContentHash.Of` the independent stage checksum, `Op`), Riok.Mapperly (composed at rung 3 — ONE `[Mapper] static partial class` owning the whole appearance seam, every method `static partial`, per-TYPE `[UserMapping]` converters reached by `[MapProperty(Use = …)]`, the segment overload for nested generated paths, `[MapValue]` for the constant column, `[MapDerivedType]` refused with its reason stated, and `[assembly: MapperDefaults]` carrying the conversion posture; `PrivateAssets="all"`), Wacton.Unicolour, Rasm.Element (the SEAM `MaterialId`, `AppearanceVector`, `AppearanceSummary`, `ContentAddress`), `Rasm.Materials.Raster` (composed — `TextureSet`/`UdimSheet`/`TextureChannel`/`ChannelPack`/`ChannelPackPlane`/`EgressSlot`/`EgressVariant`/`TexturePyramid`/`PlaneFormat`/`PlaneTransfer`/`AlphaMode`/`MipPolicy`/`NormalConvention`/`LayerLaw`/`UdimTile`/`RasterFormat`/`BlockFormat`/`KtxPayload`/`PressReceipt`), `environment#ENVIRONMENT_LIGHT` (composed — `EnvironmentLight`/`EnvironmentBlobs`/`IblProducts`), `neural#MODEL_REGISTRY` (composed — `LicenseClass`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `Lazy<T>`).
- Growth: a new appearance document is one message plus one `AppearanceEgress` fold; a new OpenPBR parameter lands beside its `OpenPbrSurface` column and breaks the Mapperly completeness gate until mapped. A new capture result shape is one `CaptureAssessment` and protobuf assessment case. Generated peers move through `assay contracts generate`, never a hand-maintained mirror.
- Law: the generated message IS the wire vocabulary and C# is the baked/material producer. The `appearance` sibling set — shared vocabulary, environment products, set document, and material model — is the frozen corpus shape; peers consume generated bindings and keep no mirror.
- Law: every closed vocabulary crosses as its GENERATED ENUM through ONE `WireVocabulary` bridge per roster (`LicenceVocabulary` for the one frontier roster a Raster page cannot name) — derived by parsing each row's own key against the enum's `OriginalName` spelling, so no hand row table exists to drift — and the derivation is PROVED at type init: a `[SmartEnum]` row with no enum member, or an enum value of zero, throws before the first egress. `RasterFormat`→`Container` is the one PARTIAL bridge (a non-wire container such as `jpeg` has no enum row) and answers on the `Fin` rail at the egress that asks, never a total map with a fabricated arm.
- Law: `AppearanceWireMap` is the completeness gate for total reader-free mappings. Presence-sensitive assessment, chromaticity, card, ingest, and proto3 optional scalars lower explicitly in `AppearanceEgress`; no generated mapper is asked to construct evidence whose presence depends on a domain case or `Option`.
- Law: `[MapDerivedType]` is REFUSED here and the reason is structural: its unregistered-case arm throws at RUNTIME where the generated Thinktecture `Switch` breaks the BUILD, and a protobuf `oneof` envelope is not a class hierarchy (RMG036). Union-case dispatch stays the generated total `Switch`.
- Law: JSON text is NOT produced here — the app root's `Rasm.AppHost/Runtime/ports#WIRE_LAW` `WireJson` renders ProtoJSON over every generated family, so an S2 member holds messages and bytes alone, and a `JsonSerializerOptions` beside the appearance family is the deleted form.
- Law: protobuf serialization is used DIRECTLY at the carrying edge; no Materials codec renames `ToByteArray` or `Parser.ParseFrom`. `Raster/set#SET_INGEST` owns the one bounded peer parse because it owns that intake, while `WireAdmission` owns the generated descriptor verdict every boundary shares.
- Law: seam-owned `AppearanceSummary.Of` mints the `AppearanceKey` — the kernel seed-zero `XxHash128` over the canonical PBR bytes via the seam `ContentAddress`/`CanonicalWriter`, NOT a second hasher and NOT a non-zero seed — so the `Summary` lowering here and the `Rasm.Bim` `AppearanceProjection.Project` lowering compose the SAME factory and produce the SAME key for one surface.
- Law: plane bytes never enter a document. Every `PlaneRef` carries its logical leaf and one required `ArtifactRef`; the canonical artifact owner carries address and extent together. `Set.key` identifies the complete document and never substitutes for payload identity.
- Law: `Set` rides BEHIND the `AppearanceKey`, never inside it. `AppearanceSummary` takes its preimage from the frozen seven-value PBR vector, so the set key is a PAYLOAD column: one appearance key covers a material with and without a baked set — the set refines the same appearance rather than describing a different one.
- Boundary: `Material`/`AppearanceSummary` is the ONE appearance wire — a per-consumer material DTO is the deleted form. `AppearanceSummary` crosses NEUTRAL (the `UInt128 AppearanceKey` plus scene-linear `BaseColorR`/`G`/`B`, `Metallic`, `Roughness`, `Opacity`, `Transmissive`), flat for a consumer reading without the lobe graph. The full `Material` is the payload BEHIND that key — the `MaterialId` `family.name` key, the `OpenPbr` vector, the optional `conductor` key, the `Provenance` receipt, the resolved `preview` colour, and the optional admitted `Emission`. Colour crosses as the scene-linear `Color` triple so a peer renders without re-deriving ACEScg; NAMED LOSS: the hex rendering beside the triple, which a peer renders at its own edge.
- Boundary: conductor-ness derives structurally at `Project`. Capture sampling, assessment, chromaticity, card, ingest, and calibration cross only when their typed evidence exists; rank deficiency is `rank < parameter_count`, never `+Inf`, and an unobserved result never becomes a zero-filled message. `Card` carries card identity and licence; no model-body address crosses without an artifact extent and redemption consumer.
- Boundary: `Set.press` carries `Press` WITHOUT a backend column because every receipt reaching the wire is CPU-minted — `AppearanceEgress.Set` proves `PressReceipt.Backend.ContentAuthoritative` before the document mints, so the accelerator lane is structurally absent from the wire rather than a column a reader trusts. `Press.graph_key` carries presence only for a shaded press; a graphless field or slab press leaves the optional column absent. `Set.ibl.luminance_cdf` likewise carries the stored guide only after a guided prefilter and stays absent after an unguided run.
- Boundary: `Ibl` is the ONE environment document and it mirrors the resolved `EnvironmentLight` row — the frozen band-major `sh9`, the six product planes by `PlaneRef`, the roughness ladder, and the READ-TIME `intensity`/`rotation` pair a consumer applies and a producer never bakes. The model key rides `Set.source` (the generator of a synthesized dome, absent for an ingested HDRI); NAMED LOSS: the two Hosek-Wilkie asset digests and the authored intensity unit pair, which stay on the domain row and reach the analytics plane off that row rather than off the wire.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using MessagePack;
using MessagePack.Resolvers;
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
using Thinktecture.Formatters;
using Wacton.Unicolour;
using static LanguageExt.Prelude;
// The generated namespace is never imported bare: `AlphaMode`, `MipPolicy`, `NormalConvention`, `PlaneFormat`,
// `LayerLaw`, and `LicenseClass` spell both a Materials roster and a generated enum, so every wire spelling rides
// the alias one qualified hop from its domain counterpart — exactly the boundary the mapper crosses.
using Wire = Rasm.Contracts.Appearance;
using Artifact = Rasm.Contracts.Artifact;

// THE ASSEMBLY-WIDE MAPPER POSTURE, declared HERE because this page owns the assembly's heaviest wire seam and the
// row is load-bearing rather than hygiene: with the default conversion set, Mapperly binds LanguageExt's THROWING
// explicit `Option<T>` -> `T` cast and PREFERS it over a registered user mapping, so an absent column would throw at
// runtime where the converter it shadowed would have written the wire's own typed absence. Clearing ExplicitCast is
// what makes every per-type Option converter on this page reachable at all.
[assembly: MapperDefaults(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]

namespace Rasm.Materials.Appearance.Interchange;

// --- [TYPES] -------------------------------------------------------------------------------
// The ONE Appearance-tier vocabulary bridge: `LicenseClass` is `neural#MODEL_REGISTRY`'s roster and a Raster
// stratum names no frontier type, so its enum bridge seats here over the same derived, type-init-proved
// `Raster/set#SET_INGEST` `WireVocabulary.Total` fold every Raster vocabulary rides.
internal static class LicenceVocabulary {
    static readonly Lazy<FrozenDictionary<LicenseClass, Wire.LicenseClass>> Licences =
        WireVocabulary.Total<LicenseClass, Wire.LicenseClass>(static () => LicenseClass.Items, static r => r.Key);

    public static Wire.LicenseClass Wire(LicenseClass row) => Licences.Value[row];
}

// --- [MODELS] ------------------------------------------------------------------------------
// `RasterCodec.Encode` returns BYTES, and the app root's write-once object store is where those bytes come to rest,
// so the evidence arrives from the step that wrote it — a projection re-deriving any column asserts a fact about a
// store it never touched. The level list is ASCENDING: ONE entry for a self-pyramiding container, one per level otherwise.
public readonly record struct LevelEgress(EgressVariant Variant, ArtifactContent Artifact);

public readonly record struct PlaneEgress(
    TextureChannel Channel, RasterFormat Format, BlockFormat Block, KtxPayload Payload, Seq<LevelEgress> Levels);

public readonly record struct PackEgress(ChannelPack Pack, RasterFormat Format, Seq<LevelEgress> Levels);

// One stored environment product carries the semantic XXH3 blob key used to prove it belongs to the resolved
// light and the disjoint SHA-256-plus-extent artifact coordinate emitted on the wire.
public readonly record struct EnvironmentProductEgress(
    string File, ContentAddress Blob, ArtifactContent Artifact, TexturePlane Plane,
    RasterFormat Container, KtxPayload Payload, LayerLaw LayerLaw, uint Mips);

// The whole `Ibl` storage roster. The guide is `Option` because an unguided prefilter wrote none; its optional wire
// presence follows that stored evidence and never fabricates a reference.
public sealed record IblStorage(
    EnvironmentProductEgress Equirect, EnvironmentProductEgress Cubemap, EnvironmentProductEgress Preview,
    Seq<EnvironmentProductEgress> Specular, EnvironmentProductEgress BrdfLut,
    Option<EnvironmentProductEgress> LuminanceCdf);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The egress folds are co-located in ONE static owner so the contracted Projection/component#COMPONENT_SUBGRAPH
// spelling is AppearanceEgress.Summary exactly and every appearance document mints from one roster of entries.
public static class AppearanceEgress {
    static RgbSpectrum Linear(Unicolour colour) { var lin = colour.RgbLinear; return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)); }

    // The seven-value preimage crosses as ONE landed Element [ComplexValueObject] AppearanceVector, whose MEMBER
    // ORDER is the frozen preimage order. AppearanceSummary.Of seeds its CanonicalWriter at zero internally, so no
    // caller passes a tolerance and none can pass a wrong one.
    public static Fin<AppearanceSummary> Summary(MaterialParameters parameters, Op key) =>
        Linear(parameters.BaseColor) switch {
            var baseLinear => AppearanceSummary.Of(AppearanceVector.Create(
                baseColorR: baseLinear.R, baseColorG: baseLinear.G, baseColorB: baseLinear.B,
                metallic: Math.Clamp(parameters.Metalness, 0.0, 1.0),
                roughness: Math.Clamp(parameters.Roughness, 0.0, 1.0),
                // Opacity is the transmission complement, so a transmissive row carries sub-unit opacity and the GLB
                // KHR_materials_transmission channel reads it; the refractive bit beside it is a DISTINCT signal,
                // because an opaque-alpha glass still transmits.
                opacity: Math.Clamp(1.0 - parameters.Transmission, 0.0, 1.0),
                transmissive: parameters.Transmission > 0.0), key),
        };

    // The MaterialParameters->OpenPBR correspondence is the DERIVED_LOGIC primary surface.md declares once; this wire
    // NEVER re-mints the column mapping. The three optional slots write by hand because proto3 `optional` sits behind
    // null-rejecting setters the generator cannot express — the named host demand at the arm.
    public static Wire.Material Project(MaterialId id, MaterialParameters parameters, CaptureProvenance provenance, SurfaceShade preview) {
        (string family, string name) = Lens(id);
        Option<ConductorMetal> resolved = ConductorMetal.Resolve(family, name);
        Wire.Material wire = new() {
            Id = id.Value,
            OpenPbr = AppearanceWireMap.ToWire(OpenPbrSurface.Of(parameters, resolved)),
            Provenance = Provenance(provenance),
            Preview = AppearanceWireMap.Color(preview.BaseColorLinear),
        };
        // Conductor-ness is STRUCTURAL — a rostered metal at full metalness — and absence crosses absent: substituting
        // a rostered neighbour once shipped its (eta, k) into every unrostered row's shading.
        resolved.Filter(_ => parameters.Metalness >= 1.0).Iter(metal => wire.Conductor = metal.Key);
        // The admitted-emission receipt crosses whole or not at all — never a zero-filled record standing in for an
        // emission no admission resolved.
        parameters.EmissionProvenance.Iter(admitted => wire.Emission = AppearanceWireMap.ToWire(admitted));
        return wire;
    }

    // The generated Provenance transcription fills Capture/Fit/Chromaticity by nested target path; the two optional
    // sub-messages and the one optional scalar lower here, each from the Option column the receipt already carries.
    public static Wire.Provenance Provenance(CaptureProvenance receipt) {
        Wire.Provenance.Types.Capture capture = new() {
            Device = receipt.Device,
            Method = receipt.Method.Key,
            Measured = receipt.Measured,
            Calibrated = receipt.Calibrated,
        };
        receipt.CalibrationDeltaE.Iter(delta => capture.CalibrationDeltaE = delta);
        receipt.WavelengthCount.Iter(count => capture.WavelengthCount = checked((uint)count));
        receipt.AngularSamples.Iter(count => capture.AngularSamples = checked((uint)count));
        Wire.Provenance wire = new() { Capture = capture };
        wire = receipt.Assessment.Map(assessment => Assessment(wire, assessment)).IfNone(wire);
        receipt.Chromaticity.Iter(observed => wire.Chromaticity = Chromaticity(observed));
        receipt.ModelCard.Iter(card => {
            Wire.Provenance.Types.Card row = new() { ModelCard = card.Value, License = LicenceVocabulary.Wire(receipt.License.IfNone(LicenseClass.Blocked)) };
            wire.Card = row;
        });
        receipt.Ingest.Iter(ingest => {
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
                    GoldenDeltaMax = inference.GoldenDeltaMax,
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

    // A registered library row IS authored; a measured material routes Project directly with its acquisition
    // CaptureProvenance. Summary needs no graph evaluation, so a projector that only needs the AppearanceSummary
    // never pays the shade evaluation.
    public static Fin<Wire.Material> Mint(MaterialId id, Op key) =>
        from parameters in MaterialLibrary.Lookup(id, key)
        from point in ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Option<Vector3d>.None, 0.5, 0.5, Context.Canonical, key)
        from preview in MaterialGraph.Default.Evaluate(point, parameters, key)
        select Project(id, parameters, CaptureProvenance.Authored, preview);

    // THE BAKED SET DOCUMENT — the generated `baked` arm behind the appearance key. Channel rows emit in ROSTER order, the same
    // order TextureSet.Of keyed the set under, so the document's row sequence IS the key preimage order. `tiled` is
    // the PROJECTION of the set's Evidence<TileProof> measured-AND-accepted read. GPU-backed press REFUSES here — the
    // content-identity veto is structural upstream and this gate is the proof no path around it exists — which is
    // also why `Press` carries no backend column. A blocked licence never ships: `LicenseClass.Grants` is the gate.
    public static Fin<Wire.Set> Set(
        TextureSet set, AppearanceSummary summary, Seq<PlaneEgress> planes, Seq<PackEgress> packs,
        CaptureProvenance provenance, Option<PressReceipt> press, LicenseClass licence, Op key) =>
        Baked(set, summary, planes, packs, provenance, press, licence, key)
            .Bind(document => WireAdmission.Admit(document, WireBoundary.OutboundPayload, key));

    static Fin<Wire.Set> Baked(
        TextureSet set, AppearanceSummary summary, Seq<PlaneEgress> planes, Seq<PackEgress> packs,
        CaptureProvenance provenance, Option<PressReceipt> press, LicenseClass licence, Op key) =>
        from _ in guard(press.ForAll(static receipt => receipt.Backend.ContentAuthoritative),
                new MaterialFault.Parameter(key, "<set-wire-gpu-minted>"))
        from __ in guard(licence.Grants, new MaterialFault.Parameter(key, $"<set-wire-licence-blocked:{licence.Key}>"))
        // The frozen packed-slot law holds at the WIRE too: planes is caller-supplied storage evidence, and a
        // duplicate here would key one field twice.
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

    // The four optional scalars and the optional press message write by hand — proto3 optional behind null-rejecting
    // setters — so the one set of presence decisions lives at one site rather than per egress shape.
    static Wire.SurfaceSet Fill(Wire.SurfaceSet wire, TextureSet set) {
        set.Material.Iter(id => wire.MaterialId = id.Value);
        set.Conductor.Iter(metal => wire.Conductor = metal.Key);
        set.HeightScaleMm.Iter(mm => wire.HeightScaleMm = mm);
        return wire;
    }

    static Wire.Set Fill(Wire.Set wire, Option<PressReceipt> press) {
        press.Iter(receipt => wire.Baked.Press = AppearanceWireMap.ToWire(receipt));
        return wire;
    }

    // A UDIM sheet crosses as ONE document: the tile axis is the frozen `<variant>` slot, so plane rows repeat per
    // ascending tile under each tile's own Udim egress leaf, `udim_tiles` carries the Mari indices the rows group by,
    // and the sheet key replaces the per-tile set key. Extent and vocabulary columns are any tile's — UdimSheet.Of
    // proved roster and vocabulary agreement before the sheet keyed, so the head tile speaks for the sheet.
    public static Fin<Wire.Set> Set(
        UdimSheet sheet, AppearanceSummary summary,
        Seq<(UdimTile Tile, Seq<PlaneEgress> Planes, Seq<PackEgress> Packs)> storage,
        CaptureProvenance provenance, Option<PressReceipt> press, LicenseClass licence, Op key) =>
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

    // The frozen level-list length law holds at the PRODUCER — one triple for a self-pyramiding container whatever
    // the chain declares, one per level otherwise — so a document naming files it cannot address is unrepresentable.
    // A baked row carries transfer, alpha mode, mip policy, and block format; the ingest-side primaries/depth/tool
    // columns stay absent, since this producer bakes and never ingests.
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
            // `present` names the channels genuinely seated, in the pack row's own slot order.
            Present = { plane.Pack.Slots.Filter(plane.Present.Contains).Map(WireVocabulary.Role) },
            Format = WireVocabulary.Format(plane.Plane.Base.Format),
            Container = container,
            Mips = (uint)plane.Plane.Levels.Count,
            Levels = { levels },
        };

    // The ONE PlaneRef mint: semantic XXH3 keys never alias onto the SHA-256 artifact coordinate.
    static Wire.PlaneRef Reference(string file, ArtifactContent artifact) =>
        new() {
            File = file,
            Artifact = new Artifact.ArtifactRef {
                Sha256 = ByteString.CopyFrom(Convert.FromHexString(artifact.Sha256)),
                ArtifactBytes = artifact.Bytes,
            },
        };

    // THE ENVIRONMENT DOCUMENT — the generated `environment.ibl` arm mirroring the resolved light row. Storage evidence is cross-checked
    // against the row's own blob custody, so a caller cannot key a wire to bytes the light never resolved; the
    // model key rides `source`, the generator of a synthesized dome, and stays absent for an ingested HDRI. The
    // dome's planes are always `gl` by construction of the two mints and carry no alpha, so the two set-level
    // vocabulary columns the corpus requires read the estate's structural constants rather than a declared fact.
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

    // family.name lensing over the seam MaterialId string — the codebase metal.<name> convention the conductor table
    // keys on, ONE pure string read over the SEAM identity, NEVER a parallel MaterialId declaration.
    static (string Family, string Name) Lens(MaterialId id) =>
        id.Value.Split('.') switch {
            [var family, var name, ..] => (family, name),
            [var family] => (family, id.Value),
            [] => (string.Empty, id.Value),
        };
}

// ONE [Mapper] OWNS THE WHOLE APPEARANCE SEAM — the material vector here and the stage crossing at [04] — because
// two mappers over one namespace duplicate the knob set, and the day one grew a converter the other lacked, two wire
// columns of the same source type crossed under two rules.
// MappingConversionType.None is strictly narrower than the assembly's All & ~ExplicitCast: every cross-type hop here
// is an explicit user mapping, so an implicit numeric widening cannot silently mask a source/target mismatch on a
// wire whose field numbers are frozen. No [MapPropertyFromSource] reader rides any mapping, so every
// [MapperIgnoreSource] row below is a compiler PROOF naming the hand-lowered column, never authored inventory.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both, EnabledConversions = MappingConversionType.None)]
public static partial class AppearanceWireMap {
    // --- [COLOUR_CONVERTERS]
    // Colour crosses scene-linear: the working-space triplet is the message, so a peer renders without re-deriving
    // ACEScg and no hex rendering rides beside it.
    [UserMapping] public static Wire.Color Color(RgbSpectrum rgb) => new() { R = rgb.R, G = rgb.G, B = rgb.B };
    [UserMapping] public static Wire.Color Color(Unicolour colour) =>
        colour.RgbLinear.Triplet switch { var lin => new Wire.Color { R = lin.First, G = lin.Second, B = lin.Third } };
    [UserMapping] static Wire.Color Band(SubsurfaceRadius radius) => new() { R = radius.R, G = radius.G, B = radius.B };
    // Counts cross unsigned; a negative count is a producer defect the checked narrowing surfaces, never a wrap.
    [UserMapping] static uint Unsigned(int value) => checked((uint)value);
    [UserMapping] static ulong Unsigned(long value) => checked((ulong)value);

    // --- [MATERIAL_VECTOR]
    // Every OpenPbr column auto-maps by NAME from its own OpenPbrSurface column — the three tints included, since a
    // synthesized White would ship a constant through the .mtlx coat_color and specular_color ports. geometry_opacity
    // is COVERAGE, never the summary's transmission-derived opacity: it holds no OpenPbrSurface column because it is
    // a TEXTURABLE geometry input whose only producer is the per-texel plane, so its vector-borne value is the
    // OpenPBR untextured default. Conductor is the ONE waived source member — it crosses as Material.conductor.
    [MapValue(nameof(Wire.OpenPbr.GeometryOpacity), 1.0)]
    [MapperIgnoreSource(nameof(OpenPbrSurface.Conductor))]
    public static partial Wire.OpenPbr ToWire(OpenPbrSurface surface);

    // Emission chromaticity is presence-sensitive, so this boundary constructs its optional evidence explicitly
    // while the admitted unit magnitude and the always-observed luminance/gamut facts remain required.
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

    // The press receipt: both keys through the kernel byte projection, the elapsed millisecond onto the well-known
    // Duration, and the two quality tallies at wire grain. Backend is WAIVED because every wire press is CPU-minted
    // by the egress gate; Planes and Aging are waived because per-plane receipts ride the Plane rows and a ladder
    // census is press telemetry; GraphKey and GpuDeltaMax lower by hand so absence stays protobuf absence.
    [MapProperty(nameof(PressReceipt.PlanKey), nameof(Wire.Press.PlanKey), Use = nameof(Key))]
    [MapProperty(nameof(PressReceipt.ElapsedMs), nameof(Wire.Press.Elapsed), Use = nameof(Elapsed))]
    [MapProperty(nameof(PressReceipt.Downgraded), nameof(Wire.Press.Downgraded), Use = nameof(Tally))]
    [MapProperty(nameof(PressReceipt.Faulted), nameof(Wire.Press.FaultedTexels), Use = nameof(Summed))]
    [MapperIgnoreSource(nameof(PressReceipt.Backend))]
    [MapperIgnoreSource(nameof(PressReceipt.Planes))]
    [MapperIgnoreSource(nameof(PressReceipt.Aging))]
    [MapperIgnoreSource(nameof(PressReceipt.GraphKey))]
    [MapperIgnoreSource(nameof(PressReceipt.GpuDeltaMax))]
    [MapperIgnoreTarget(nameof(Wire.Press.GraphKey))]
    [MapperIgnoreTarget(nameof(Wire.Press.GpuDeltaMax))]
    private static partial Wire.Press Press(PressReceipt receipt);

    public static Wire.Press ToWire(PressReceipt receipt) {
        Wire.Press wire = Press(receipt);
        receipt.GraphKey.Iter(graph => wire.GraphKey = Key(graph));
        receipt.GpuDeltaMax.Iter(delta => wire.GpuDeltaMax = delta);
        return wire;
    }

    [UserMapping] static ByteString Key(UInt128 key) => ContentHash.Wire(key);
    [UserMapping] static Duration Elapsed(double milliseconds) => Duration.FromTimeSpan(TimeSpan.FromMilliseconds(milliseconds));
    [UserMapping] static uint Tally(Seq<TextureChannel> downgraded) => (uint)downgraded.Count;
    // Dataset columns are SET-grained, so the per-channel tally sums to one wire scalar and the split stays on the
    // interior receipt for the observability arm that fans it by channel.
    [UserMapping] static ulong Summed(HashMap<TextureChannel, ulong> faulted) =>
        faulted.Values.Fold(0UL, static (acc, texels) => acc + texels);

    // The STAGE-CROSSING half of this same seam continues as a PARTIAL of this class at [04] — one type, one knob
    // set, one converter roster. The class attributes ride this part alone, because attributes on any part of a
    // partial type apply to the whole.
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

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// MtlxPort closes the MaterialX 1.39 typed-port axis — float/integer/boolean/color3/vector3/filename/surfaceshader; every MtlxInput carries the REAL
// type of the slot it fills, a file reference is filename-typed (never a color3), and the fractal2d octaves count is integer-typed (never a
// float a strict validator rejects).
// MixTrait closes the LOWERING DISPOSITIONS a blend projection can carry, so the disposition row holds ONE
// capability column instead of a bool per behaviour and a third disposition is a ROW rather than a widened record.
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

// Each row carries the category's DEFAULT output port; a type-polymorphic node instance (math/mix) overrides it
// from its op row, so the category port is the floor, never the per-instance truth.
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

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct MtlxInput(string Name, MtlxPort Type, string Value, Option<string> NodeName);

public sealed record MtlxNode(string Name, NodeCategory Category, MtlxPort Type, Seq<MtlxInput> Inputs);

// LossyEdges names every blend the projection could not carry exactly, one token per degraded node — the DECLARED
// half of the disposition table. A document is only honest about its own fidelity if it says so in the document:
// a consumer re-importing a soft-light graph reads that the edge arrived as an overlay instead of diffing a picture
// against a source it does not have, and an EMPTY roster is the positive statement that the whole graph crossed
// exactly. The `.mtlx` render carries it as document comment text, never as a schema element MaterialX does not define.
public sealed record MtlxDocument(string Version, Seq<MtlxNode> Nodes, string SurfaceNode, string MaterialName, Seq<string> LossyEdges) {
    public const string Schema = "1.39";
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Mtlx {
    // FileSlots declares the image-family categories and their filename-typed input SLOTS as POLICY data the texture emit arm folds, never an
    // inline category-equality chain re-deriving the set per node. Triplanar carries THREE per-axis file slots (`filex`/`filey`/`filez` — the
    // stdlib nodedef), image/tiledimage the single `file`.
    static readonly FrozenDictionary<NodeCategory, ImmutableArray<string>> FileSlots =
        new Dictionary<NodeCategory, ImmutableArray<string>> {
            [NodeCategory.Image] = ["file"],
            [NodeCategory.TiledImage] = ["file"],
            [NodeCategory.Triplanar] = ["filex", "filey", "filez"],
        }.ToFrozenDictionary();

    // Four TEXTURABLE open_pbr_surface GEOMETRY inputs carry no OpenPbr column and therefore no
    // constant — the nodedef default IS the shading frame — so SurfaceNode emits none of them and a bound channel
    // APPENDS its edge onto the surface node. `geometry_thin_walled` is that group's fifth input and the one
    // exception in both directions: a set-level shell boolean no texel field carries, so it rides the wire as a
    // column and emits as a CONSTANT port off the OpenPbrPorts row rather than as a bindable edge.
    // GeometryPorts is the ROSTER and NormalWrapped the SUBSET routing through a `normalmap` node, because a
    // tangent-space texel becomes a shading-frame perturbation while a tangent binds its vector image direct.
    // MEMBERSHIP is the fact, so the two false entries a bool payload carried — rows that stated nothing the roster
    // did not already say — delete with it, and a fifth geometry port lands on whichever set it belongs to.
    static readonly FrozenSet<TextureChannel> GeometryPorts = new[] {
        TextureChannel.GeometryNormal, TextureChannel.GeometryCoatNormal,
        TextureChannel.GeometryTangent, TextureChannel.GeometryCoatTangent,
    }.ToFrozenSet();

    static readonly FrozenSet<TextureChannel> NormalWrapped = new[] {
        TextureChannel.GeometryNormal, TextureChannel.GeometryCoatNormal,
    }.ToFrozenSet();

    // CategoryOf resolves the per-TextureSource-case MaterialX category string to the closed NodeCategory row; texture#TEXTURE_UV owns the
    // case→category projection itself (TextureSource.MtlxCategory). FromGraph composes it through the wiring-site binding map — the graph's
    // Texture case carries only the erased sampler closure, so the source is recoverable ONLY from the binding.
    public static NodeCategory CategoryOf(TextureSource source) =>
        NodeCategory.TryGet(source.MtlxCategory, out NodeCategory? category) ? category! : NodeCategory.Image;

    // A duplicate node id is two `<node name="nodeN">` elements — an invalid document, and toHashMap silently keeps
    // the last; a sink that is missing or not the BsdfOutput would bind a phantom surface node.
    public static Fin<MtlxDocument> FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters, Op key, HashMap<PortId, TextureSource> textures = default) =>
        graph.Nodes.TraverseM(n => Row(n, parameters, textures, key).Map(row => (n.Id, row))).As().Map(toHashMap)
            .Bind(ports =>
                from _ in guard(ports.Count == graph.Nodes.Count, new MaterialFault.Graph(key, "<mtlx-duplicate-node-id>"))
                from __ in guard(graph.Nodes.Exists(n => n.Id == graph.Sink && n is AppearanceNode.BsdfOutput),
                    new MaterialFault.Graph(key, "<mtlx-sink-not-bsdf-output>"))
                from ___ in guard(graph.Nodes.ForAll(n => n.Dependencies.ForAll(ports.ContainsKey)),
                    new MaterialFault.Graph(key, "<mtlx-dangling-edge>"))
                from emitted in graph.Nodes.TraverseM(n => Emit(n, ports, parameters, textures, key)).As()
                // The lossy roster DERIVES from the resolved shapes rather than being accumulated at emit: the
                // disposition is already a column on the row the shape pass settled, so a second accumulator would
                // be a chance for the document and the table to disagree about what crossed. BOTH degradation axes
                // fold here — the blend substitutions off each resolved Mix row and the texture-source substitutions
                // off each bound source's own `texture#TEXTURE_UV` `MtlxLossy` column — because a document is only
                // honest about its fidelity if every axis that can degrade reports on the same roster.
                let lossy = toSeq(ports.Values).Bind(shape => shape.Mix.Bind(static row => row.Lossy).ToSeq())
                          + toSeq(textures.Values).Bind(static source => source.MtlxLossy.ToSeq())
                select new MtlxDocument(MtlxDocument.Schema, emitted.Bind(static group => group),
                    $"node{graph.Sink.Value}", id.Value.Replace('.', '_'), lossy.Strict()));

    // An unbound Texture floors to `image`. The six blends MaterialX cannot express are NOT rails — each carries its
    // declared lossy disposition and the document names it. Lowered mix rows register their OUTER wrapper (the `mix`
    // node downstream edges reference and type against).
    static Fin<(NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> Row(AppearanceNode node, MaterialParameters parameters, HashMap<PortId, TextureSource> textures, Op key) =>
        node.Switch(
            state: (Parameters: parameters, Textures: textures, Key: key),
            input:      static (s, i) => Fin.Succ((NodeCategory.Constant, PortOf(i.Pull(s.Parameters)), Option<MixProjection>.None)),
            texture:    static (s, t) => Fin.Succ(Categorized(s.Textures.Find(t.Id))),
            math:       static (s, m) => MathRow(m.Op, s.Key).Map(static row => (row.Item1, row.Item2, Option<MixProjection>.None)),
            // The projection RESOLVES here, once, on the rail that can refuse — and rides the shape map so the emit
            // arm reads it rather than indexing MixRows a second time. A raw re-lookup at emit re-asks a settled
            // question on a total indexer that cannot refuse, which is where a row absent from the table becomes a
            // throw instead of the fault this pass already owns.
            mix:        static (s, x) => MixRow(x.Op, s.Key).Map(static row =>
                            (row.Traits.Admits(MixTrait.Lowered) ? NodeCategory.Mix : row.Category, MtlxPort.Color3, Some(row))),
            normal:     static (_, _) => Fin.Succ((NodeCategory.Normalmap, MtlxPort.Vector3, Option<MixProjection>.None)),
            bsdfOutput: static (_, _) => Fin.Succ((NodeCategory.OpenPbrSurface, MtlxPort.Surface, Option<MixProjection>.None)));

    // MathRow holds the MathOp→(category, output type) correspondence — TOTAL generated Switch, so a new MathOp row breaks HERE at compile time; the
    // op row pins the polymorphic node variant (Scale is the vector3 multiply, Multiply the float).
    // The MaterialX stdlib INPUT ROSTER per math row, carved to the rows whose schema names its inputs. Every other
    // row takes the positional family, which is MaterialX's own convention for the arithmetic set, so this table
    // carries only what genuinely diverges and a coverage proof would report noise. `Of` answers the declared roster
    // when one exists and derives the positional one otherwise, truncated to the operands a node actually carries —
    // an arity the op's own Admits column already gated, so the projection is total here.
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
                // `switch` names its SELECTOR `which` and its branches `in1..inN`, and the selector is this row's
                // FIRST operand — a straight positional emit would bind the selector to the first branch.
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

    // One MixOp→MaterialX row: the projected category, the closed disposition traits, and Lossy naming the FIDELITY
    // the substitution costs where MaterialX 1.39 stdlib carries no equivalent node.
    // Lossy is typed absence rather than an empty string: an exact projection and a declared approximation are
    // different facts, and a document whose consumer cannot tell them apart re-imports a blend the estate never
    // exported. A row is EITHER exact or carries the loss it took, never silent about either.
    static readonly CapabilitySet<MixTrait> Lowered = CapabilitySet<MixTrait>.Of(MixTrait.Lowered);
    static readonly CapabilitySet<MixTrait> LoweredSwapped = CapabilitySet<MixTrait>.Of(MixTrait.Lowered, MixTrait.Swapped);

    // Both behaviour columns ride ONE CapabilitySet rather than a bool per disposition, so a third lowering
    // behaviour is a trait ROW and the record's shape never moves: Lowered marks a factorless projection the emitter
    // wraps in a `mix` node so the Factor lerp survives, Swapped the W3C operand exchange (hard-light(a,b) IS
    // overlay(b,a)).
    readonly record struct MixProjection(
        NodeCategory Category, CapabilitySet<MixTrait> Traits = default, Option<string> Lossy = default);

    // The four NON-SEPARABLE modes — hue, saturation, colour, luminosity — degrade to the plain factor lerp, because
    // a channel-independent node cannot express a blend defined over the whole triple and substituting a separable
    // lookalike would ship a different picture wearing the right name. Railing them instead made an authored
    // sixteen-mode graph unexportable at the first non-separable blend, which is a worse answer than an export whose
    // document NAMES what it could not carry.
    // Authoring stays SIXTEEN modes — this table is the egress projection, never a narrowing of the graph algebra.
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

    // An image-family texture carries filename-typed EMPTY file slots (in-memory levels — the host edge writes the
    // sidecar assets). A UNARY op crosses on the stdlib `in` port — invert/clamp/sqrt/absval/sin/cos/normalize declare
    // `in`, never `in1`, and emitting `in1` on a unary node is the schema violation the prior form carried. Every arm
    // emits ONE node except a Lowered mix, which expands through MixNodes into its inner node plus the `mix` wrapper
    // at node{id}.
    static Fin<Seq<MtlxNode>> Emit(AppearanceNode node, HashMap<PortId, (NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> ports, MaterialParameters parameters, HashMap<PortId, TextureSource> textures, Op key) =>
        node.Switch(
            state: (Ports: ports, Parameters: parameters, Textures: textures, Key: key),
            input:      static (s, i) => Fin.Succ(One(i.Id, s.Ports, Constant(i.Pull(s.Parameters)))),
            // The DRIVER edge crosses beside the file slots: a texture whose sampler reads an upstream parameter
            // (a gradient driven by a field) exports that wire on the stdlib `texcoord` input, so a driven lookup
            // round-trips as the graph it is rather than as an unwired constant the far end resamples at (u,v).
            texture:    static (s, t) => Fin.Succ(One(t.Id, s.Ports,
                (FileSlots.TryGetValue(s.Ports[t.Id].Category, out ImmutableArray<string> slots)
                    ? toSeq(slots.Select(static slot => new MtlxInput(slot, MtlxPort.Filename, string.Empty, Option<string>.None)))
                    : s.Textures.Find(t.Id).Map(TextureInputs).IfNone(Seq<MtlxInput>()))
                + t.Parameter.Map(driver => Seq(Edge("texcoord", driver, s.Ports))).IfNone(Seq<MtlxInput>()))),
            // A unary row crosses on the stdlib `in` port and an n-ary ARITHMETIC row on the positional `in1..inN`
            // family, which IS MaterialX's own convention for add/subtract/multiply/divide/modulo/power/min/max/
            // atan2/dotproduct/crossproduct. The stdlib rows whose inputs are NAMED — smoothstep, remap, range,
            // contrast, the two conditionals, switch — read their roster off MtlxPorts: emitting `in3` where the
            // schema declares `outlow` produces a document that parses and shades wrong, which is worse than one
            // that refuses, and the far end has no way to recover the intended binding.
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

    // TextureInputs projects the procedural-node inputs: texture#TEXTURE_UV owns the case→(name, MaterialX-type, value) parameter rows
    // (TextureSource.MtlxParameters); this resolves each row's type string to the closed MtlxPort and never mints the case→parameter mechanics
    // locally (altitude: the MtlxCategory-owning page owns the parameter law too). An image-family node carries file slots in the texture arm
    // above, not parameters, so the two projections never overlap.
    static Seq<MtlxInput> TextureInputs(TextureSource source) =>
        source.MtlxParameters.Map(static row =>
            new MtlxInput(row.Name, MtlxPort.TryGet(row.Type, out MtlxPort? port) ? port! : MtlxPort.Float, row.Value, Option<string>.None));

    // Comp rows: fg = B (the SOURCE MixOp.Apply blends over the backdrop), bg = A (the backdrop the factor lerps FROM), mix = Factor — exactly
    // lerp(a, blend(b over a), t); an fg=A/bg=B spelling inverts every composite on the wire. Lowered rows: the inner factorless node at
    // node{id}b (math in1/in2, or the operand-SWAPPED overlay for HardLight), then the `mix` wrapper fg=node{id}b, bg=A, mix=Factor — the factor
    // lerp survives, downstream edges keep referencing node{id}, and the DAG topology stays recoverable. The row arrives
    // RESOLVED off the shape pass, so this fold owns no table lookup and no unreachable-miss branch.
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

    // Edge types an interior edge from its SOURCE (the resolved output of the node it connects from) and pins a surface-slot edge to its declared
    // type through the Option slot — one body, no arity twin.
    static MtlxInput Edge(string name, PortId source, HashMap<PortId, (NodeCategory Category, MtlxPort Port, Option<MixProjection> Mix)> ports, Option<MtlxPort> slot = default) =>
        new(name, slot.IfNone(() => ports[source].Port), string.Empty, Some($"node{source.Value}"));

    // ONE surface-document entry over an OPTIONAL baked set — never a bound/unbound pair, because the two documents
    // differ by which inputs carry a texture edge and by nothing else.
    public static Fin<MtlxDocument> ToOpenPbr(Wire.Material wire, Option<Wire.Set> planes, Op key) =>
        string.IsNullOrWhiteSpace(wire.Id)
            ? Fin.Fail<MtlxDocument>(new MaterialFault.Graph(key, "<mtlx-empty-material-id>"))
            : planes
                .Map(set => toSeq(set.Planes).Traverse(row => Bound(row, key)).As()
                    .Map(bound => Document(wire, bound.Bind(static entry => entry.Nodes).Add(Textured(wire, bound)))))
                .IfNone(() => Fin.Succ(Document(wire, Seq(SurfaceNode(wire)))));

    // The wire-sourced document carries an EMPTY lossy roster and that emptiness is a real claim: this projection
    // folds the OpenPBR port schema, which has no blend algebra to degrade — every input crosses exactly or the
    // table has no row for it at all.
    static MtlxDocument Document(Wire.Material wire, Seq<MtlxNode> nodes) =>
        new(MtlxDocument.Schema, nodes, "node0", wire.Id.Replace('.', '_'), Seq<string>());

    // The surface node is named node0 to key consistently with node{id}. The table spans the CONSTANT-VALUED inputs
    // alone: the four geometry inputs (geometry_normal, geometry_coat_normal, geometry_tangent,
    // geometry_coat_tangent) are genuine nodedef ports with NO wire column — GeometryPorts owns them and Textured
    // appends their edges only when a baked channel binds one.
    static readonly ImmutableArray<(string Port, MtlxPort Type, Func<Wire.OpenPbr, string> Text)> OpenPbrPorts = [
        ("base_weight", MtlxPort.Float, static g => Num(g.BaseWeight)),
        ("base_color", MtlxPort.Color3, static g => Rgb(g.BaseColor)),
        ("base_metalness", MtlxPort.Float, static g => Num(g.BaseMetalness)),
        ("base_diffuse_roughness", MtlxPort.Float, static g => Num(g.BaseDiffuseRoughness)),
        ("specular_weight", MtlxPort.Float, static g => Num(g.SpecularWeight)),
        // specular_color ships the wire column VERBATIM: OpenPbrSurface.Of already resolved the Disney tint into
        // it through Tinted(authored, baseHue, tint) at surface#OPENPBR_SLAB. Re-biasing here toward BaseColor
        // applied that tint a SECOND time under a different algebra, so an exported document and the Slab.Base its
        // own row shades disagreed on every tinted specular.
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
        // The frozen unit fork: thickness is NANOMETRES everywhere, and the open_pbr_surface nodedef's
        // thin_film_thickness input alone is MICROMETRES — the divide by 1000 lives at exactly this egress row and
        // nowhere else, so every other consumer reads nm and only the .mtlx text carries the µm lowering.
        ("thin_film_thickness", MtlxPort.Float, static g => Num(g.ThinFilmThickness / 1000.0)),
        ("thin_film_ior", MtlxPort.Float, static g => Num(g.ThinFilmIor)),
        ("emission_color", MtlxPort.Color3, static g => Rgb(g.EmissionColor)),
        ("emission_luminance", MtlxPort.Float, static g => Num(g.EmissionLuminance)),
        ("geometry_opacity", MtlxPort.Float, static g => Num(g.GeometryOpacity)),
        // The one geometry BOOLEAN with a wire column: MaterialX spells booleans lowercase true/false.
        ("geometry_thin_walled", MtlxPort.Boolean, static g => g.GeometryThinWalled ? "true" : "false"),
    ];

    static MtlxNode SurfaceNode(Wire.Material wire) =>
        new("node0", NodeCategory.OpenPbrSurface, MtlxPort.Surface,
            toSeq(OpenPbrPorts.Select(row => new MtlxInput(row.Port, row.Type, row.Text(wire.OpenPbr), Option<string>.None))));

    // The mm-band radius splits below — a zero-radius row scales (1,1,1) so the float radius alone zeroes the effect.
    static double RadiusMax(Wire.OpenPbr g) => Math.Max(g.SubsurfaceRadius.R, Math.Max(g.SubsurfaceRadius.G, g.SubsurfaceRadius.B));

    static string RadiusScale(Wire.OpenPbr g) =>
        RadiusMax(g) is > 0.0 and var max
            ? Triple(g.SubsurfaceRadius.R / max, g.SubsurfaceRadius.G / max, g.SubsurfaceRadius.B / max)
            : Triple(1.0, 1.0, 1.0);

    // Each bound channel becomes one `tiledimage` node whose `file` is
    // filename-typed and whose output feeds the open_pbr_surface input its OWN MtlxBinding row names: a canonical binding binds straight (the
    // OpenPBR port name IS the canonical channel name — that identity is why no translation column exists), Scaled inserts a `multiply` by its
    // factor (the thin-film nm-to-micrometre divide is a unit fork, not a rename), Split feeds the paired scale input, Lowered routes into the
    // named host input, and Absent emits NO node — a channel OpenPBR has no input for stays a wire column rather than becoming a phantom port a
    // validator rejects. Image node and multiply travel together on a Scaled row because a scaled channel whose multiply went missing feeds the
    // surface a value in the wrong unit, silently, since both slots are floats.
    static Fin<(TextureChannel Channel, Seq<MtlxNode> Nodes, string Source)> Bound(Wire.Plane row, Op key) =>
        WireVocabulary.Channel(row.Role).Case is TextureChannel channel
            ? BaseLeaf(row, key).Map(file => channel.Mtlx switch {
                MtlxBinding.Scaled scaled => (channel, Seq(
                        Image(channel, file),
                        new MtlxNode($"scale_{channel.Key}", NodeCategory.Multiply, Lane(channel), Seq(
                            new MtlxInput("in1", Lane(channel), string.Empty, Some($"tex_{channel.Key}")),
                            Value("in2", MtlxPort.Float, scaled.Factor)))),
                    $"scale_{channel.Key}"),
                // Absent EMITS NOTHING — the channel has no OpenPBR input, so an image node here would be the
                // dangling phantom port this arm's own law forecloses; Textured already skips the wiring, and the
                // node list must match it.
                MtlxBinding.Absent => (channel, Seq<MtlxNode>(), string.Empty),
                // A normal-row geometry channel routes tiledimage -> normalmap -> the surface's geometry input, so
                // the raw tangent-space texel never binds a shading-frame port direct; the normalmap output is the
                // source the surface edge references.
                _ when NormalWrapped.Contains(channel) => (channel, Seq(
                        Image(channel, file),
                        new MtlxNode($"nrm_{channel.Key}", NodeCategory.Normalmap, MtlxPort.Vector3,
                            Seq(new MtlxInput("in", MtlxPort.Vector3, string.Empty, Some($"tex_{channel.Key}"))))),
                    $"nrm_{channel.Key}"),
                _ => (channel, Seq(Image(channel, file)), $"tex_{channel.Key}"),
            })
            : Fin.Fail<(TextureChannel, Seq<MtlxNode>, string)>(new MaterialFault.Graph(key, $"<mtlx-unknown-channel:{row.Role}>"));

    // The `file` attribute names the BASE level alone: a mip chain resolves through the container (a self-pyramiding
    // KTX2 carries its own levels) or through the renderer's filtering, so the document binds level zero and the
    // remaining PlaneRef rows stay the object-store addresses a durability consumer walks. The leaf lives on the
    // LEVEL-ORDERED address list and nowhere else — the row carries no scalar file column, because a name beside a
    // level list is a second spelling of the same string that the frozen levels law already orders — so a row whose
    // list is empty names no file at all and refuses HERE rather than emitting an image node with a blank `file`
    // that a validator accepts and a renderer resolves to nothing.
    static Fin<string> BaseLeaf(Wire.Plane row, Op key) =>
        toSeq(row.Levels).Head
            .Map(static level => level.File)
            .ToFin(new MaterialFault.Graph(key, $"<mtlx-channel-unaddressed:{row.Role}>"));

    static MtlxNode Image(TextureChannel channel, string file) =>
        new($"tex_{channel.Key}", NodeCategory.TiledImage, Lane(channel),
            Seq(new MtlxInput("file", MtlxPort.Filename, file, Option<string>.None)));

    // Textured REPLACES every covered channel's constant input with its texture edge under the row's own MtlxBinding: an Absent binding
    // contributes nothing (the channel has no OpenPBR input at all) and a Lowered or Split binding names the input its own row carries, so the
    // port names come from the roster rather than a second list here that could disagree with the OpenPbrPorts table SurfaceNode already folds.
    // A GEOMETRY port has no constant row to replace — the surface table carries no input for it — so a bound
    // geometry channel APPENDS its edge; replace-or-append is one probe on the same fold, never a second emit path.
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

    // Lane drives a float input from a single-component channel and a color3 from a multi-component one; the count is the roster's SEMANTIC
    // column, so a vector channel stored in a four-component plane still declares three. A geometry channel is
    // vector data, never color — its image node and its surface edge both type vector3.
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

- Owner: `StageRequestRow`/`StageResultRow` with `StageInputRow`/`StageOutputRow`/`StageScoreRow` the photo-to-PBR inference crossing's hand rows; `StageCodec` the MessagePack leg; `StageRoster` the `(slot, wire)` roster DERIVED once from each row's own `[Key(n)]` attributes with its soundness proof; `StageRequestRow.Checksum` the roster digest the relaying root compares; the `AppearanceWireMap` partial generating the request transcription.
- Cases: the crossing is BRANCH-INTERIOR — `Rasm.Materials/Appearance/neural` ⇄ `Rasm.Compute/Model/inference` across the plugin firebreak, relayed by the app root as bytes. NO peer runtime decodes it, which is the wire-contract law's one discriminant admitting MessagePack: a proto family exists only where a peer decodes, so these rows are hand projections and take `Row`, never `Wire`. The card `STAGE_FAMILY` at `IDEAS.md` names the corpus family that retires this leg.
- Entry: `public static StageRequestRow StageRequestRow.Of(StageRequest request)` and `public static Fin<StageResult> StageResultRow.Admit(StageResultRow row, ModelCard card, Op key)` are the two halves — Materials writes the request and re-admits the returned result through the `neural#MODEL_REGISTRY` gate rather than trusting an executor's bytes; `StageCodec.Encode`/`Decode` carry both under the one options profile; `StageRequestRow.Checksum` folds the roster the relaying root compares against Compute's `StageCrossing.Checksum` before moving a byte.
- Law: the two ends agree by DIGEST, never by a boot probe over one end's own record. Each end folds its `(slot, wire)` roster through the kernel writer — `ContentHash.Of(roster, (r, w) => w.Sorted(r, row => row.Slot, Comparer<int>.Default, (row, x) => x.Ordinal(row.Slot).String(row.Wire)))` — and the relaying root compares `ContentHash.Hex` of both; this end's roster is DERIVED from the record's `[Key(n)]` attributes and the camelCase member name once at static init, where soundness proves unique slots and an arity equal to the record's constructor, so a column landing at one end alone moves the digest rather than a probe that compares one side to itself.
- Packages: MessagePack (`[MessagePackObject]`/`[Key]` positional modeling, `[GeneratedMessagePackResolver]`, `Lz4BlockArray` + `WithCompressionMinLength`, `MessagePackSecurity.UntrustedData`; `MessagePackAnalyzer` enforces `[Key]` coverage at compile time), Thinktecture.Runtime.Extensions.MessagePack (`ThinktectureMessageFormatterResolver`), Riok.Mapperly (the `AppearanceWireMap` partial under `RequiredMappingStrategy.Both`), `Rasm` (`ContentHash.Of`/`CanonicalWriter.Sorted`/`Ordinal`/`String` the checksum fold), `neural#MODEL_REGISTRY` (composed — `StageRequest`/`StageResult`/`StageInput`/`StageOutput`/`StageScore`/`StageProduct`/`PbrStage`/`ModelCard`/`ModelCardId`/`LicenseClass`/`InferenceProvider`/`TensorPrecision`/`InferenceTiling`), Rasm.Element (the seam `ContentAddress` and its ONE `ToValue` spelling), BCL inbox (`System.Reflection` for the ONE static-init roster read), LanguageExt.Core.
- Growth: a new stage column is one `StageRequestRow` slot with its trailing `[Key(n)]`, which the roster proof, the checksum, and the RMG diagnostics each force onto their owner in the same change — and the matching Compute slot, or the digests disagree at the relay. The family's growth past this page is the `STAGE_FAMILY` card: one `rasm.contracts.stage` proto retires every row here.
- Boundary: this section PROJECTS and never decides. `neural#MODEL_REGISTRY` owns the stage, licence, provider, and precision vocabularies; every one crosses as its own row's KEY STRING, so an unknown key REFUSES at `Admit` rather than defaulting. It mints NO `tests/contracts/manifest.json` entry — a corpus entry for a branch-interior hop is the fabricated contract the cross-`libs/` ruling forecloses.

```csharp signature
// (Continues the Rasm.Materials.Appearance.Interchange compilation unit — the [02] prelude is in scope.)

// --- [MODELS] ------------------------------------------------------------------------------
// One consumed product on a request. An empty `stage` names the intent's own source plane; otherwise the executor
// resolves the named producer's output from results it already holds, so a chained stage NEVER carries the source
// blob and its albedo estimator cannot read the raw photograph the delighting stage exists to replace.
[MessagePackObject]
public readonly record struct StageInputRow(
    [property: Key(0)] string Stage, [property: Key(1)] string Role, [property: Key(2)] string Key);

// One produced plane. `role` is a canonical channel key or a prior key, and StageProduct.Parse resolves the CHANNEL
// roster first, so a prior spelled as a channel is unreachable rather than ambiguous.
[MessagePackObject]
public readonly record struct StageOutputRow(
    [property: Key(0)] string Role, [property: Key(1)] string BlobKey, [property: Key(2)] uint Width,
    [property: Key(3)] uint Height, [property: Key(4)] string Transfer, [property: Key(5)] string Format);

// One produced GRADE. Fields and grades ride separate collections for the same reason they ride separate result
// collections: they are separate modalities, not one list with a small extent.
[MessagePackObject]
public readonly record struct StageScoreRow([property: Key(0)] string Role, [property: Key(1)] double Value);

// THE INFERENCE REQUEST. Extent THREADS: a stage tiles against the extent its input carries and publishes
// `inputWidth × scale`, so a chained stage never re-derives a grid. Eighteen slots — the producer's `Layout` is an
// interior tensor-layout note the executor re-derives off its model card and never a wire column.
[MessagePackObject]
public sealed record StageRequestRow(
    [property: Key(0)] string Stage,
    [property: Key(1)] string ModelCardId,
    [property: Key(2)] string LicenseClass,
    [property: Key(3)] StageInputRow[] Inputs,
    [property: Key(4)] uint InputWidth,
    [property: Key(5)] uint InputHeight,
    [property: Key(6)] uint OutputWidth,
    [property: Key(7)] uint OutputHeight,
    [property: Key(8)] int TileWidth,
    [property: Key(9)] int TileHeight,
    [property: Key(10)] int Overlap,
    [property: Key(11)] string PadMode,
    [property: Key(12)] string Bucket,
    [property: Key(13)] string Provider,
    [property: Key(14)] string Precision,
    [property: Key(15)] ulong Seed,
    [property: Key(16)] string Op,
    [property: Key(17)] string Artefact) {

    // neural#MODEL_REGISTRY StageRequest.Of already gated the request — a blocked licence has no request to project —
    // so Of transcribes alone through the generated AppearanceWireMap.ToWire row set.
    public static StageRequestRow Of(StageRequest request) => AppearanceWireMap.ToWire(request);

    // The roster digest the relaying root compares: slot-sorted, each slot framed beside its camelCase wire name
    // through the ONE kernel writer, so the producer and Compute fold byte-identical preimages from independently
    // derived rosters.
    public static UInt128 Checksum => ContentHash.Of(StageRoster.Request, static (roster, writer) =>
        writer.Sorted(roster, static row => row.Slot, Comparer<int>.Default, static (row, slot) => slot.Ordinal(row.Slot).String(row.Wire)));
}

// THE INFERENCE RESULT. Artefact is the digest of the weight bytes the session loaded, so two revisions of one card
// separate on the receipt rather than sharing an id a repository may re-publish.
[MessagePackObject]
public sealed record StageResultRow(
    [property: Key(0)] string Stage,
    [property: Key(1)] string ModelCardId,
    [property: Key(2)] StageOutputRow[] Outputs,
    [property: Key(3)] string ProviderUsed,
    [property: Key(4)] int PartitionCount,
    [property: Key(5)] double ElapsedMs,
    [property: Key(6)] double GoldenDelta,
    [property: Key(7)] int TilesEmitted,
    [property: Key(8)] string Op,
    [property: Key(9)] string Artefact,
    [property: Key(10)] bool ParityFresh,
    [property: Key(11)] float Coverage,
    [property: Key(12)] StageScoreRow[] Scores) {

    public static UInt128 Checksum => ContentHash.Of(StageRoster.Result, static (roster, writer) =>
        writer.Sorted(roster, static row => row.Slot, Comparer<int>.Default, static (row, slot) => slot.Ordinal(row.Slot).String(row.Wire)));

    // Ingestion re-admits through the OWNING gate rather than trusting the executor's bytes: every vocabulary key
    // lifts back onto its own closed roster (an unknown stage, product, provider, transfer, or format REFUSES here)
    // and the reconstructed result then crosses neural#MODEL_REGISTRY StageResult.Admit, so the physical-channel
    // prohibition, the partition bound, the residual ceiling, and the output completeness are all proved by their
    // owner. A decode that merely deserialized would let a peer publish a fabricated normal plane as measured.
    public static Fin<StageResult> Admit(StageResultRow row, ModelCard card, Op key) =>
        // The executor echoes the correlation key VERBATIM; a result whose echo names another request refuses
        // before any vocabulary lift — without this guard the op correlation is write-only.
        from _echo in guard(row.Op == key.ToString(), new MaterialFault.Parameter(key, $"<stage-op-echo:{row.Op}>"))
        from stage in PbrStage.TryGet(row.Stage, out PbrStage? stageRow) ? Fin.Succ(stageRow!) : Refused<PbrStage>(key, "stage", row.Stage)
        from provider in InferenceProvider.TryGet(row.ProviderUsed, out InferenceProvider? used) ? Fin.Succ(used!) : Refused<InferenceProvider>(key, "provider", row.ProviderUsed)
        from outputs in toSeq(row.Outputs).Traverse(output => Output(output, key)).As()
        from scores in toSeq(row.Scores).Traverse(score =>
            StageProduct.Parse(score.Role).ToFin(Refused(key, "score", score.Role))
                .Map(product => new StageScore(product, score.Value))).As()
        from echoed in key.AcceptValidated<ModelCardId>(ModelCardId.Validate(row.ModelCardId, null, out ModelCardId id), id)
        // The WIRE's own echo threads through, so the neural#STAGE_PLAN card-mismatch gate proves the result
        // answers the card it claims; the artefact admits through the seam's own X32 validator.
        from artefact in key.AcceptValidated<ContentAddress>(ContentAddress.Validate(row.Artefact, null, out ContentAddress? loaded), loaded!)
        from admitted in StageResult.Admit(
            new StageResult(stage, echoed, artefact, outputs, scores, provider, row.PartitionCount, row.ElapsedMs, row.GoldenDelta, row.ParityFresh, row.Coverage, row.TilesEmitted, key), card, key)
        select admitted;

    static Fin<StageOutput> Output(StageOutputRow output, Op key) =>
        from product in StageProduct.Parse(output.Role).ToFin(Refused(key, "product", output.Role))
        from transfer in PlaneTransfer.TryGet(output.Transfer, out PlaneTransfer? band) ? Fin.Succ(band!) : Refused<PlaneTransfer>(key, "transfer", output.Transfer)
        from format in PlaneFormat.TryGet(output.Format, out PlaneFormat? storage) ? Fin.Succ(storage!) : Refused<PlaneFormat>(key, "format", output.Format)
        from blob in key.AcceptValidated<ContentAddress>(ContentAddress.Validate(output.BlobKey, null, out ContentAddress? address), address!)
        select new StageOutput(product, blob.Value, Dimension.Create((int)output.Width), Dimension.Create((int)output.Height), transfer, format);

    static MaterialFault Refused(Op key, string axis, string value) => new MaterialFault.Graph(key, $"<stage-result-unknown-{axis}:{value}>");
    static Fin<T> Refused<T>(Op key, string axis, string value) => Fin.Fail<T>(Refused(key, axis, value));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The roster is DERIVED from the record's own `[Key(n)]` attributes — the ONE static-init reflection read on this
// page, legitimate for a PROOF and never for runtime identity — so no hand `(slot, wire)` list exists to drift from
// the record. Soundness proves at first touch: slots unique, and arity equal to the record's constructor, so a slot
// landed on one end alone cannot pass as a roster the other end agrees with.
internal static class StageRoster {
    public static readonly Seq<(int Slot, string Wire)> Request = Derive<StageRequestRow>();
    public static readonly Seq<(int Slot, string Wire)> Result = Derive<StageResultRow>();

    static Seq<(int Slot, string Wire)> Derive<TRow>() {
        Seq<(int Slot, string Wire)> rows = toSeq(typeof(TRow).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Choose(static property => Optional(property.GetCustomAttribute<KeyAttribute>())
                .Map(attribute => (Slot: attribute.IntKey, Wire: string.Concat(char.ToLowerInvariant(property.Name[0]), property.Name.AsSpan(1)))));
        int arity = typeof(TRow).GetConstructors().Max(static ctor => ctor.GetParameters().Length);
        return rows.Map(static row => row.Slot).Distinct().Count == rows.Count && rows.Count == arity
            ? rows
            : throw new InvalidOperationException($"<stage-roster-unsound:{typeof(TRow).Name}:{rows.Count}:{arity}>");
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// What lands here is the request transcription; the result re-admits by hand because every column lifts a key
// onto a closed roster on the rail, which a mapper cannot express.
public static partial class AppearanceWireMap {
    [MapProperty("Stage.Key", nameof(StageRequestRow.Stage))]
    [MapProperty("ModelCardId.Value", nameof(StageRequestRow.ModelCardId))]
    [MapProperty("LicenseClass.Key", nameof(StageRequestRow.LicenseClass))]
    [MapProperty("InputWidth.Value", nameof(StageRequestRow.InputWidth), Use = nameof(Unsigned))]
    [MapProperty("InputHeight.Value", nameof(StageRequestRow.InputHeight), Use = nameof(Unsigned))]
    [MapProperty("OutputWidth.Value", nameof(StageRequestRow.OutputWidth), Use = nameof(Unsigned))]
    [MapProperty("OutputHeight.Value", nameof(StageRequestRow.OutputHeight), Use = nameof(Unsigned))]
    [MapProperty("Provider.Key", nameof(StageRequestRow.Provider))]
    [MapProperty("Precision.Key", nameof(StageRequestRow.Precision))]
    [MapProperty(nameof(StageRequest.Op), nameof(StageRequestRow.Op), Use = nameof(OpKey))]
    [MapProperty(nameof(StageRequest.Artefact), nameof(StageRequestRow.Artefact), Use = nameof(AddressOrEmpty))]
    [MapProperty(nameof(StageRequest.Inputs), nameof(StageRequestRow.Inputs), Use = nameof(InputRows))]
    [MapperIgnoreSource(nameof(StageRequest.Layout))]  // interior tensor-layout note the executor re-derives, never a wire column
    public static partial StageRequestRow ToWire(StageRequest request);

    static string OpKey(Op op) => op.ToString();

    // Absence lowers to the empty string: a card whose weights the caller supplies has no registry digest, and
    // ContentAddress.Validate refuses "" so the absent case can never round-trip into a fabricated address.
    static string AddressOrEmpty(Option<ContentAddress> address) => address.Map(static digest => digest.ToValue()).IfNone(string.Empty);

    static StageInputRow InputRow(StageInput input) => new(input.Wire.Stage, input.Wire.Role, input.Wire.Key);

    static StageInputRow[] InputRows(Seq<StageInput> inputs) => [.. inputs.Map(InputRow)];
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// StageResolver resolves the [MessagePackObject] rows AOT source-generated — no IL-emit DynamicObjectResolver inside
// a plugin AssemblyLoadContext, and an unannotated record is a FormatterNotRegisteredException at first use.
[GeneratedMessagePackResolver]
public sealed partial class StageResolver;

// UntrustedData hardening caps depth, caps the decompression size, and takes collision-resistant maps, because the
// executor's bytes cross a process boundary. Resolver order: the source-generated rows, then any Thinktecture
// generated key, then the standard primitive fallback. The codec surface is the synchronous bounded-payload pair —
// framing and stream custody stay the relaying root's.
public static class StageCodec {
    public static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(StageResolver.Instance, ThinktectureMessageFormatterResolver.Instance, StandardResolver.Instance))
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithCompressionMinLength(512)
            .WithSecurity(MessagePackSecurity.UntrustedData);

    public static ReadOnlyMemory<byte> Encode<TRow>(TRow row) where TRow : class => MessagePackSerializer.Serialize(row, Options);

    // A null decode result is the documented return-contract refusal; thrown codec evidence stays exceptional.
    public static Fin<TRow> Decode<TRow>(ReadOnlyMemory<byte> payload, Op key) where TRow : class =>
        key.Catch(() => Fin.Succ(MessagePackSerializer.Deserialize<TRow>(payload, Options)))
            .Bind(row => row is { } decoded ? Fin.Succ(decoded) : new MaterialFault.Graph(key, $"<{typeof(TRow).Name}-messagepack-null>"));
}
```

## [05]-[RESEARCH]

(none)
