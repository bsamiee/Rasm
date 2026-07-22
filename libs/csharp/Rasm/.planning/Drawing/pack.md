# [RASM_ENCODING_PACK]

`Rasm.Drawing` encoding folds one `PackOp` through `Encode.Apply` into a dtype-strided `EncodedGeometry`, every active channel proven against its quantization tolerance or routed as the typed fault. `ToolpathPath` retains line and circular spans, so arc centre and sense survive packing, posting, and reconciliation as content, never collapsing to sampled chords.

Compute wraps `EncodedGeometry.Payload` and its descriptors as an `EncodedTensor` residency view, and AppHost marshals the descriptor set under `EncodingKind` rows locked one-to-one onto `PackKind`.

## [01]-[INDEX]

- [02]-[ENCODING]: `PackOp` fold over its channel, dtype, and kind vocabulary into the descriptor-tiled `EncodedGeometry` with round-trip witness.
- [03]-[SCHEMA_AND_EVIDENCE]: `PackSchema` columnar schema identity and `EvidenceWire` exact-hi/lo evidence block.

## [02]-[ENCODING]

- Owner: `PackKind` binds each representation to its active `EncodingChannel` set, each channel composing its live kernel reader as the sole owner of its curvature, geodesic, or normal field through a `[UseDelegateFromConstructor]` `Read` column no channel can omit. `ChannelDtype` owns the quantization seam — width, tolerance, and the bulk pack/unpack arms — and every channel writes into one descriptor-tiled byte arena carrying its round-trip proof.
- Cases: `ToolpathSpan` splits `Line` and `Arc`, the arc retaining its analytic centre and sense; every `PackKind` shares one `Apply`, one `Read` column, and one witness fold.
- Entry: `Encode.Apply(PackOp, Op?)` is the ONE encoding entrypoint, discriminating by `PackOp` case on the `Fin` rail and gating `EncodedGeometry` at `key.AcceptValue`. `PackPolicy.Tolerance` sets the voxel SDF iso-band and the field-sampling floor, never a domain-local epsilon. `EncodingFault` 2444 routes a reader bind failure, an extent-versus-arity disagreement, or an unpack breaching `Dtype.Tolerance`; `DegenerateInput` 2400 routes an empty or sub-floor source; a non-digest reconcile answer routes the `Op` admission channel.
- Auto: `SourceDigest` projects a `ToolpathPath` through a canonical vertex stream, so reconciliation observes every analytic distinction rather than sampled chords.
- Receipt: `EncodedGeometry` is the `IValidityEvidence` carrier; its claim set rejects any descriptor set that gaps, overlaps, or carries a non-finite witness error, so a hand-assembled carrier fails the acceptance oracle. `View<T>` dispatches on the `Dtype` row, answering the empty view for an absent channel or a width-mismatched `T`.
- Packages: `Rasm.Meshing`, `Rasm.Spatial`, `Rasm.Processing`, `Rasm.Numerics`, `Rasm.Domain`, RhinoCommon, `System.Numerics.Tensors`, `CommunityToolkit.HighPerformance`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`, and BCL inbox.
- Growth: a new modality is one `PackKind` row and one `PackOp` case; a new feature is one `EncodingChannel` row with its `Read` column; a new quantization is one `ChannelDtype` row over the SAME witness; a per-instance block descriptor is one column on `EncodingChannelDescriptor`. Zero new surface.
- Law: `EncodingLaws` is the tier-2 law matrix — descriptor tiling, per-channel recovery within `Dtype.Tolerance`, active-set equality against `PackKind.Channels`, and schema-id agreement between kind declaration and packed instance.
- Boundary: one `PackOp` `[Union]` folds through `Apply` with no per-kind encoder class; reconciliation owns the content digest, so the page binds `(form, digest)` pairs and cloud, mesh, and parametric byte layouts share one digest owner rather than crossing as raw bytes; raw `float`/`byte` stay inside the pack loop, and the only public residency seam is the `Payload`/descriptor pair.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Drawing;

// --- [TYPES] ------------------------------------------------------------------------------
// Width is the residency fact (bytes per scalar); the span arms are the ONE quantization seam.
// A generated Switch cannot carry ref-struct operands, so the three-row if-chain IS the dispatch.
[SmartEnum<int>]
public sealed partial class ChannelDtype {
    public static readonly ChannelDtype Float32 = new(key: 0, width: 4, tolerance: 0.0);
    public static readonly ChannelDtype Float16 = new(key: 1, width: 2, tolerance: 9.77e-4);
    public static readonly ChannelDtype Unorm8  = new(key: 2, width: 1, tolerance: 1.0 / 255.0);

    public int Width { get; }
    public double Tolerance { get; }

    // A dtype row extending neither arm packs nothing and the witness routes 2444 — no silent fall-through.
    public void Pack(ReadOnlySpan<float> raw, Span<byte> stored) {
        if (this == Float32) { MemoryMarshal.AsBytes(raw).CopyTo(stored); return; }
        if (this == Float16) { TensorPrimitives.ConvertToHalf(raw, MemoryMarshal.Cast<byte, Half>(stored)); return; }
        if (this == Unorm8) { for (int i = 0; i < raw.Length; i++) stored[i] = (byte)MathF.Round(Math.Clamp(raw[i], 0f, 1f) * 255f); }
    }

    public void Unpack(ReadOnlySpan<byte> stored, Span<float> restored) {
        if (this == Float32) { MemoryMarshal.Cast<byte, float>(stored).CopyTo(restored); return; }
        if (this == Float16) { TensorPrimitives.ConvertToSingle(MemoryMarshal.Cast<byte, Half>(stored), restored); return; }
        if (this == Unorm8) { for (int i = 0; i < stored.Length; i++) restored[i] = stored[i] / 255f; }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EncodingChannel {
    public static readonly EncodingChannel Position  = new("position",   arity: 3, dtype: ChannelDtype.Float32, read: static (op, _) => Encode.ReadPosition(op));
    public static readonly EncodingChannel Normal    = new("normal",     arity: 3, dtype: ChannelDtype.Float32, read: Encode.ReadNormal);
    public static readonly EncodingChannel ColorRgba = new("color-rgba", arity: 4, dtype: ChannelDtype.Unorm8,  read: static (op, _) => Encode.ReadColor(op));
    public static readonly EncodingChannel Curvature = new("curvature",  arity: 1, dtype: ChannelDtype.Float16, read: Encode.ReadCurvature);
    public static readonly EncodingChannel Geodesic  = new("geodesic",   arity: 1, dtype: ChannelDtype.Float16, read: Encode.ReadGeodesic);
    public static readonly EncodingChannel Intensity = new("intensity",  arity: 1, dtype: ChannelDtype.Float16, read: Encode.ReadIntensity);
    public static readonly EncodingChannel Occupancy = new("occupancy",  arity: 1, dtype: ChannelDtype.Float16, read: Encode.ReadOccupancy);
    public static readonly EncodingChannel Weight    = new("weight",     arity: 1, dtype: ChannelDtype.Float16, read: static (op, _) => Encode.ReadWeight(op));
    public static readonly EncodingChannel ArcCenter = new("arc-center", arity: 3, dtype: ChannelDtype.Float32, read: static (op, _) => Encode.ReadArcCenter(op));
    public static readonly EncodingChannel ArcSense  = new("arc-sense",  arity: 1, dtype: ChannelDtype.Float32, read: static (op, _) => Encode.ReadArcSense(op));

    public int Arity { get; }
    public ChannelDtype Dtype { get; }

    [UseDelegateFromConstructor] internal partial Fin<float[]> Read(PackOp op, Op key);
}

// Channels IS the active set; the field row rides the mesh the content digest binds (no position dup).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PackKind {
    public static readonly PackKind PointCloud = new("point-cloud", Seq(EncodingChannel.Position, EncodingChannel.Normal, EncodingChannel.ColorRgba, EncodingChannel.Intensity));
    public static readonly PackKind MeshPatch  = new("mesh-patch",  Seq(EncodingChannel.Position, EncodingChannel.Normal, EncodingChannel.Curvature, EncodingChannel.Geodesic, EncodingChannel.Weight));
    public static readonly PackKind VoxelGrid  = new("voxel-grid",  Seq(EncodingChannel.Position, EncodingChannel.Occupancy, EncodingChannel.Weight));
    public static readonly PackKind BrepPatch  = new("brep-patch",  Seq(EncodingChannel.Position, EncodingChannel.Normal, EncodingChannel.Curvature));
    public static readonly PackKind Field      = new("field",       Seq(EncodingChannel.Geodesic, EncodingChannel.Weight));
    public static readonly PackKind Toolpath   = new("toolpath",    Seq(EncodingChannel.Position, EncodingChannel.ArcCenter, EncodingChannel.ArcSense, EncodingChannel.Weight));

    public Seq<EncodingChannel> Channels { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
public sealed record PackGrid(int Nx, int Ny, int Nz, BoundingBox Bounds) {
    public int CellCount => Nx * Ny * Nz;

    public Point3d CellCenter(int linear) {
        int k = linear / (Nx * Ny);
        int r = linear - k * (Nx * Ny);
        int j = r / Nx;
        int i = r - j * Nx;
        double sx = Bounds.Diagonal.X / Nx, sy = Bounds.Diagonal.Y / Ny, sz = Bounds.Diagonal.Z / Nz;
        return new Point3d(Bounds.Min.X + (i + 0.5) * sx, Bounds.Min.Y + (j + 0.5) * sy, Bounds.Min.Z + (k + 0.5) * sz);
    }
}

// Cloud defaults through its own AdmitOrDefault on None.
public sealed record PackPolicy(
    Seq<int> GeodesicSources, double CurvatureTimeStep, int CurvatureIterations,
    SdfMeshPolicy Sdf, Option<CloudMetricPolicy> Cloud, Context Tolerance) {
    public static Fin<PackPolicy> Of(
        Context tolerance, SdfMeshPolicy sdf, Seq<int> geodesicSources = default,
        Option<CloudMetricPolicy> cloud = default, double curvatureTimeStep = 1e-3, int curvatureIterations = 1, Op? key = null) =>
        guard(curvatureTimeStep > 0.0 && curvatureIterations > 0, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new PackPolicy(geodesicSources, curvatureTimeStep, curvatureIterations, sdf, cloud, tolerance));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record EncodingChannelDescriptor(EncodingChannel Channel, int Count, int ByteOffset, ChannelDtype Dtype) {
    public int Floats => Count * Channel.Arity;
    public int Bytes => Floats * Dtype.Width;
}

public sealed record RoundTripWitness(GeometryHash ContentHash, HashMap<string, double> ChannelError, bool Lossless) {
    public static RoundTripWitness Of(GeometryHash digest, Seq<(EncodingChannel Channel, double Error)> errors) =>
        new(digest,
            errors.Fold(HashMap<string, double>(), static (acc, e) => acc.Add(e.Channel.Key, e.Error)),
            errors.ForAll(static e => e.Error <= e.Channel.Dtype.Tolerance));
}

public sealed record EncodedGeometry(
    Seq<EncodingChannelDescriptor> Descriptors, ReadOnlyMemory<byte> Payload, int Count, RoundTripWitness Witness) : IValidityEvidence {

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Count, floor: 1),
        ValidityClaim.Of(Witness.Lossless),
        ValidityClaim.Of(Descriptors.Map(static d => d.Channel.Key).Distinct().Count == Descriptors.Count),
        ValidityClaim.CountExactly(count: Witness.ChannelError.Count, expected: Descriptors.Count),
        ValidityClaim.Of(Descriptors.ForAll(static d =>
            (long)d.Count * d.Channel.Arity * d.Dtype.Width is > 0 and <= int.MaxValue)),
        ValidityClaim.Of(Descriptors.Fold((Offset: 0L, Holds: true), static (acc, d) => {
            long bytes = (long)d.Count * d.Channel.Arity * d.Dtype.Width;
            return (acc.Offset + bytes,
                acc.Holds && d.ByteOffset == acc.Offset && d.Count == Count && d.Dtype == d.Channel.Dtype);
        }) is var tile
            && tile.Holds && tile.Offset == Payload.Length),
        ValidityClaim.Of(Witness.ChannelError.Values.AsIterable().ForAll(static error => double.IsFinite(error) && error >= 0.0)),
        ValidityClaim.Of(Descriptors.ForAll(d => Witness.ChannelError.Find(d.Channel.Key).Match(
            Some: error => double.IsFinite(error) && error >= 0.0 && error <= d.Channel.Dtype.Tolerance,
            None: static () => false))));

    public ReadOnlyMemory<byte> Channel(EncodingChannel channel) =>
        Descriptors.Find(d => d.Channel == channel)
            .Match(Some: d => Payload.Slice(d.ByteOffset, d.Bytes), None: static () => ReadOnlyMemory<byte>.Empty);

    // Dtype row names the one legal T: float32→float · float16→Half · unorm8→byte, over [Count × Arity].
    public ReadOnlyTensorSpan<T> View<T>(EncodingChannel channel) where T : unmanaged {
        if (Descriptors.Find(d => d.Channel == channel).Case is not EncodingChannelDescriptor found || Unsafe.SizeOf<T>() != found.Dtype.Width)
            return default;
        ReadOnlySpan<T> cast = MemoryMarshal.Cast<byte, T>(Payload.Span.Slice(found.ByteOffset, found.Bytes));
        return TensorMarshal.CreateReadOnlyTensorSpan(
            ref MemoryMarshal.GetReference(cast), cast.Length, lengths: [found.Count, found.Channel.Arity], strides: [], pinned: false);
    }
}

// Reserve sums count·arity·width per active channel — the residency arithmetic lives on the descriptor row.
public sealed record EncodedStore(int Count, byte[] Payload, EncodingChannelDescriptor[] Descriptors) {
    public static EncodedStore Reserve(int count, Seq<EncodingChannel> channels) =>
        new(count, new byte[channels.Fold(0, (acc, c) => acc + (count * c.Arity * c.Dtype.Width))], new EncodingChannelDescriptor[channels.Count]);
}

public sealed record PackedChannels(EncodedStore Store, (EncodingChannel Channel, float[] Raw)[] Raws);

[SmartEnum<int>]
public sealed partial class ToolpathArcSense {
    public static readonly ToolpathArcSense Clockwise = new(-1);
    public static readonly ToolpathArcSense Counterclockwise = new(1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToolpathSpan {
    private ToolpathSpan() { }

    public sealed record Line(Point3d Target) : ToolpathSpan;
    public sealed record Arc(Point3d Target, Point3d Center, ToolpathArcSense Sense) : ToolpathSpan;

    public Point3d Target => Switch(
        line: static span => span.Target,
        arc: static span => span.Target);
}

public sealed record ToolpathPath(Point3d Start, Seq<ToolpathSpan> Spans) {
    public Seq<Point3d> Vertices => Start.Cons(Spans.Map(static span => span.Target));
    public Seq<Point3d> CanonicalVertices => Spans.Fold(Seq(Start), static (stream, span) => span.Switch(
        state: stream,
        line: static (state, row) => state.Add(row.Target).Add(row.Target).Add(Point3d.Origin),
        arc: static (state, row) => state.Add(row.Target).Add(row.Center).Add(new Point3d(row.Sense.Key, 0.0, 0.0))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PackOp {
    private PackOp() { }

    public sealed record PointCloud(VectorCloud.ClusterCase Source, PackPolicy Policy) : PackOp;
    public sealed record MeshPatch(MeshSpace Source, PackPolicy Policy) : PackOp;
    public sealed record VoxelGrid(MeshSpace Source, PackGrid Grid, PackPolicy Policy) : PackOp;
    public sealed record BrepPatch(MeshSpace Source, PackPolicy Policy) : PackOp;
    public sealed record Field(MeshSpace Source, ScalarField Values, PackPolicy Policy) : PackOp;
    public sealed record Toolpath(ToolpathPath Source, PackPolicy Policy) : PackOp;

    public PackKind Kind =>
        Switch(
            pointCloud: static _ => PackKind.PointCloud,
            meshPatch:  static _ => PackKind.MeshPatch,
            voxelGrid:  static _ => PackKind.VoxelGrid,
            brepPatch:  static _ => PackKind.BrepPatch,
            field:      static _ => PackKind.Field,
            toolpath:   static _ => PackKind.Toolpath);

    internal PackPolicy Policy =>
        Switch(
            pointCloud: static p => p.Policy, meshPatch: static m => m.Policy, voxelGrid: static v => v.Policy,
            brepPatch:  static b => b.Policy, field:     static f => f.Policy, toolpath:  static t => t.Policy);
}

public static class Encode {
    public static Fin<EncodedGeometry> Apply(PackOp op, Op? key = null) {
        Op k = key.OrDefault();
        return ElementCount(op)
            .Bind(count => PackChannels(op, op.Kind, count, k)
                .Bind(packed => Witness(op, packed, k)
                    .Map(witness => new EncodedGeometry(packed.Store.Descriptors.ToSeq(), packed.Store.Payload, packed.Store.Count, witness))))
            .Bind(geometry => k.AcceptValue(geometry));
    }

    // --- [PACK]
    static Fin<PackedChannels> PackChannels(PackOp op, PackKind kind, int count, Op key) {
        long bytes = kind.Channels.Fold(0L, static (extent, channel) =>
            extent + ((long)count * channel.Arity * channel.Dtype.Width));
        if (bytes > Array.MaxLength) {
            EncodingChannel channel = kind.Channels[0];
            return Fin.Fail<PackedChannels>(new GeometryFault.EncodingFault(
                channel, channel.Dtype, $"payload extent {bytes} exceeds {Array.MaxLength}").ToError());
        }
        EncodedStore store = EncodedStore.Reserve(count, kind.Channels);
        List<(EncodingChannel Channel, float[] Raw)> raws = new(kind.Channels.Count);
        return kind.Channels.Fold(Fin.Succ((slot: 0, offset: 0)), (state, channel) =>
                state.Bind(s => channel.Read(op, key).Bind(raw =>
                    raw.Length == count * channel.Arity
                        ? Fin.Succ(WriteChannel(store, s.slot, s.offset, channel, count, raw, raws))
                        : Fin.Fail<(int, int)>(new GeometryFault.EncodingFault(
                            channel, channel.Dtype, $"arity {raw.Length} != {count * channel.Arity}").ToError()))))
            .Map(_ => new PackedChannels(store, raws.ToArray()));
    }

    static (int Slot, int Offset) WriteChannel(EncodedStore store, int slot, int offset, EncodingChannel channel, int count, float[] raw, List<(EncodingChannel, float[])> raws) {
        EncodingChannelDescriptor descriptor = new(channel, count, offset, channel.Dtype);
        channel.Dtype.Pack(raw, store.Payload.AsSpan(offset, descriptor.Bytes));
        store.Descriptors[slot] = descriptor;
        raws.Add((channel, raw));
        return (slot + 1, offset + descriptor.Bytes);
    }

    // --- [WITNESS]
    // ONE digest chain: EncodeForm.Of(source) → Reconciliation.Apply(Encode) → ReconcileAnswer.Digest;
    // reconciliation solely owns the canonical byte layouts. Error reduce = Subtract·Abs·MaxMagnitude, scale-relative.
    static Fin<RoundTripWitness> Witness(PackOp op, PackedChannels packed, Op key) =>
        SourceDigest(op, key).Bind(digest => {
            Seq<(EncodingChannel Channel, double Error)> errors = toSeq(packed.Raws).Map(row => {
                EncodingChannelDescriptor descriptor = System.Array.Find(packed.Store.Descriptors, d => d.Channel == row.Channel)!;
                return (row.Channel, ChannelError(row.Raw, packed.Store.Payload.AsSpan(descriptor.ByteOffset, descriptor.Bytes), row.Channel.Dtype));
            });
            return errors.Find(e => e.Error > e.Channel.Dtype.Tolerance).Match(
                Some: breach => Fin.Fail<RoundTripWitness>(new GeometryFault.EncodingFault(
                    breach.Channel, breach.Channel.Dtype, $"round-trip {breach.Error:e3} > {breach.Channel.Dtype.Tolerance:e3}").ToError()),
                None: () => Fin.Succ(RoundTripWitness.Of(digest, errors)));
        });

    // Dtype tolerances are RELATIVE precision facts, so the max delta divides by max(1, ‖raw‖∞) — an
    // absolute bound would fault every real-scale channel above magnitude one; an infinite delta stays loud.
    static double ChannelError(float[] raw, ReadOnlySpan<byte> stored, ChannelDtype dtype) {
        using SpanOwner<float> staging = SpanOwner<float>.Allocate(raw.Length);
        Span<float> restored = staging.Span;
        dtype.Unpack(stored, restored);
        TensorPrimitives.Subtract<float>(restored, raw, restored);
        TensorPrimitives.Abs<float>(restored, restored);
        return TensorPrimitives.MaxMagnitude<float>(restored) / Math.Max(1f, TensorPrimitives.MaxMagnitude<float>(raw));
    }

    static Fin<GeometryHash> SourceDigest(PackOp op, Op key) => op switch {
        PackOp.PointCloud source => Digest(EncodeForm.Of(source.Source), key),
        PackOp.MeshPatch source  => Digest(EncodeForm.Of(source.Source), key),
        PackOp.VoxelGrid source  => Digest(EncodeForm.Of(source.Source), key),
        PackOp.BrepPatch source  => Digest(EncodeForm.Of(source.Source), key),
        PackOp.Field source      => Digest(EncodeForm.Of(source.Source), key),
        PackOp.Toolpath source   => VectorCloud.Polyline(source.Source.CanonicalVertices, source.Policy.Tolerance, key)
            .Bind(cloud => Digest(EncodeForm.Of(cloud), key)),
        _ => Fin.Fail<GeometryHash>(key.InvalidResult()),
    };

    static Fin<GeometryHash> Digest(EncodeForm form, Op key) =>
        Reconciliation.Apply(new ReconcileOp.Encode(form), key)
            .Bind(answer => answer is ReconcileAnswer.Digest digest
                ? Fin.Succ(digest.Value)
                : Fin.Fail<GeometryHash>(key.InvalidResult()));

    // --- [READERS]
    static Fin<int> ElementCount(PackOp op) => op.Switch(
        pointCloud: static c => Elements(c.Source.Vertices.Count, 1, Kind.PointCloud),
        meshPatch:  static m => MeshVertexCount(m.Source),
        voxelGrid:  static v => Elements(v.Grid.CellCount, 1, Kind.BoundingBox),
        brepPatch:  static b => MeshVertexCount(b.Source),
        field:      static f => MeshVertexCount(f.Source),
        toolpath:   static t => Elements(t.Source.Vertices.Count, 2, Kind.Polyline));

    static Fin<int> Elements(int count, int floor, Kind kind) =>
        count >= floor
            ? Fin.Succ(count)
            : Fin.Fail<int>(new GeometryFault.DegenerateInput(kind, -1, $"under {floor} elements").ToError());

    static Fin<int> MeshVertexCount(MeshSpace space) => Elements(space.Native.Vertices.Count, 1, Kind.Mesh);

    internal static Fin<float[]> ReadPosition(PackOp op) =>
        op switch {
            PackOp.PointCloud c => Fin.Succ(PackPoints(c.Source.Vertices)),
            PackOp.Toolpath t   => Fin.Succ(PackPoints(t.Source.Vertices)),
            PackOp.VoxelGrid v  => Fin.Succ(PackCells(v.Grid)),
            PackOp.MeshPatch m  => Fin.Succ(PackVertices(m.Source)),
            PackOp.BrepPatch b  => Fin.Succ(PackVertices(b.Source)),
            _                   => NoReader(EncodingChannel.Position, op),
        };

    internal static Fin<float[]> ReadNormal(PackOp op, Op key) =>
        op switch {
            PackOp.PointCloud c => OrientedNormals(c.Source, c.Policy, key).Map(PackVectors),
            PackOp.MeshPatch m  => Fin.Succ(PackNormals(m.Source)),
            PackOp.BrepPatch b  => Fin.Succ(PackNormals(b.Source)),
            _                   => NoReader(EncodingChannel.Normal, op),
        };

    internal static Fin<float[]> ReadColor(PackOp op) =>
        op is PackOp.PointCloud c ? Fin.Succ(PackColors(c.Source)) : NoReader(EncodingChannel.ColorRgba, op);

    internal static Fin<float[]> ReadCurvature(PackOp op, Op key) =>
        op switch {
            PackOp.MeshPatch m => MeshScalarField(ScalarField.MeanCurvatureFlow(m.Source, m.Policy.CurvatureTimeStep, m.Policy.CurvatureIterations, key), m.Source, m.Policy.Tolerance, key),
            PackOp.BrepPatch b => MeshScalarField(ScalarField.MeanCurvatureFlow(b.Source, b.Policy.CurvatureTimeStep, b.Policy.CurvatureIterations, key), b.Source, b.Policy.Tolerance, key),
            _                  => NoReader(EncodingChannel.Curvature, op),
        };

    // Geodesic is the per-vertex scalar LANE: mesh patch binds the heat-geodesic field, field pack its ScalarField.
    internal static Fin<float[]> ReadGeodesic(PackOp op, Op key) =>
        op switch {
            PackOp.MeshPatch m => MeshScalarField(ScalarField.Geodesic(m.Source, m.Policy.GeodesicSources, key), m.Source, m.Policy.Tolerance, key),
            PackOp.Field f     => MeshScalarField(Fin.Succ(f.Values), f.Source, f.Policy.Tolerance, key),
            _                  => NoReader(EncodingChannel.Geodesic, op),
        };

    internal static Fin<float[]> ReadIntensity(PackOp op, Op key) =>
        op is PackOp.PointCloud c
            ? OrientedNormals(c.Source, c.Policy, key).Map(NormalConsistency)
            : NoReader(EncodingChannel.Intensity, op);

    // SignedDistanceFromMeshCase admits only vetted payloads by direct case construction; raw-ingress
    // siblings (MeanCurvatureFlow) go through their Fin factory.
    internal static Fin<float[]> ReadOccupancy(PackOp op, Op key) =>
        op is PackOp.VoxelGrid v
            ? GridOccupancy(new ScalarField.SignedDistanceFromMeshCase(Space: v.Source, Policy: v.Policy.Sdf), v.Grid, v.Policy.Tolerance, key)
            : NoReader(EncodingChannel.Occupancy, op);

    internal static Fin<float[]> ReadWeight(PackOp op) =>
        op switch {
            PackOp.MeshPatch m => Fin.Succ(VertexAreaWeight(m.Source)),
            PackOp.Field f     => Fin.Succ(VertexAreaWeight(f.Source)),
            PackOp.VoxelGrid v => Fin.Succ(Fill(v.Grid.CellCount, 1f)),
            PackOp.Toolpath t  => Fin.Succ(ChordWeight(t.Source.Vertices)),
            _                  => NoReader(EncodingChannel.Weight, op),
        };

    internal static Fin<float[]> ReadArcCenter(PackOp op) => op is PackOp.Toolpath toolpath
        ? Fin.Succ(PackPoints(toolpath.Source.Start.Cons(toolpath.Source.Spans.Map(static span => span.Switch(
            line: static row => row.Target,
            arc: static row => row.Center)))))
        : NoReader(EncodingChannel.ArcCenter, op);

    internal static Fin<float[]> ReadArcSense(PackOp op) => op is PackOp.Toolpath toolpath
        ? Fin.Succ(0f.Cons(toolpath.Source.Spans.Map(static span => span.Switch(
                line: static _ => 0f,
                arc: static row => (float)row.Sense.Key))).ToArray())
        : NoReader(EncodingChannel.ArcSense, op);

    static Fin<float[]> NoReader(EncodingChannel channel, PackOp op) =>
        Fin.Fail<float[]>(new GeometryFault.EncodingFault(channel, channel.Dtype, $"no reader for {op.Kind.Key}").ToError());

    // --- [PROJECTIONS]
    static Fin<Vector3d[]> OrientedNormals(VectorCloud.ClusterCase cloud, PackPolicy policy, Op key) =>
        VectorIntent.Cloud(cloud, VectorCloudMetric.OrientedNormals, policy.Cloud, key)
            .Bind(intent => intent.Project<Seq<Vector3d>>(policy.Tolerance, key))
            .Map(static seq => seq.ToArray());

    static Fin<float[]> MeshScalarField(Fin<ScalarField> built, MeshSpace space, Context tolerance, Op key) =>
        built.Bind(field => {
            Mesh native = space.Native;
            float[] values = new float[native.Vertices.Count];
            for (int i = 0; i < values.Length; i++) {
                Fin<FieldSample> sample = field.SampleDetailed(native.Vertices.Point3dAt(i), tolerance, key);
                if (sample.IsFail) return sample.Map(static _ => System.Array.Empty<float>());
                values[i] = (float)sample.IfFail(static _ => default).Value;
            }
            return Fin.Succ(values);
        });

    static Fin<float[]> GridOccupancy(ScalarField field, PackGrid grid, Context tolerance, Op key) {
        float[] values = new float[grid.CellCount];
        for (int i = 0; i < values.Length; i++) {
            Fin<SdfSample> sample = field.SampleSdfDetailed(grid.CellCenter(i), tolerance, key);
            if (sample.IsFail) return sample.Map(static _ => System.Array.Empty<float>());
            values[i] = sample.IfFail(static _ => default).Value <= 0.0 ? 1f : 0f;
        }
        return Fin.Succ(values);
    }

    static float[] NormalConsistency(Vector3d[] normals) {
        float[] values = new float[normals.Length];
        for (int i = 0; i < normals.Length; i++) values[i] = (float)Math.Abs(normals[i].Z);
        return values;
    }

    static float[] VertexAreaWeight(MeshSpace space) {
        Mesh native = space.Native;
        float[] weight = new float[native.Vertices.Count];
        for (int face = 0; face < native.Faces.Count; face++) {
            MeshFace mf = native.Faces[face];
            Point3d a = native.Vertices.Point3dAt(mf.A), b = native.Vertices.Point3dAt(mf.B), c = native.Vertices.Point3dAt(mf.C);
            float abc = (float)(0.5 * Vector3d.CrossProduct(b - a, c - a).Length / 3.0);
            weight[mf.A] += abc; weight[mf.B] += abc; weight[mf.C] += abc;
            if (mf.IsQuad) {
                Point3d d = native.Vertices.Point3dAt(mf.D);
                float acd = (float)(0.5 * Vector3d.CrossProduct(c - a, d - a).Length / 3.0);
                weight[mf.A] += acd; weight[mf.C] += acd; weight[mf.D] += acd;
            }
        }
        return Normalize(weight);
    }

    static float[] ChordWeight(Seq<Point3d> chain) {
        float[] weight = new float[chain.Count];
        for (int i = 0; i + 1 < chain.Count; i++) {
            float half = (float)(0.5 * chain[i].DistanceTo(chain[i + 1]));
            weight[i] += half; weight[i + 1] += half;
        }
        return Normalize(weight);
    }

    // No Normalize operator exists on the lattice — MaxMagnitude + Divide IS the spelling.
    static float[] Normalize(float[] values) {
        float max = TensorPrimitives.MaxMagnitude<float>(values);
        if (!(max > 0f)) return values;
        float[] scaled = new float[values.Length];
        TensorPrimitives.Divide<float>(values, max, scaled);
        return scaled;
    }

    // SoA interleave writers: AoS→SoA transposition has no TensorPrimitives form — span kernels.
    static float[] PackPoints(Seq<Point3d> points) {
        float[] buffer = new float[points.Count * 3];
        int i = 0;
        foreach (Point3d p in points) { buffer[i++] = (float)p.X; buffer[i++] = (float)p.Y; buffer[i++] = (float)p.Z; }
        return buffer;
    }

    // Read-only channel reads ride space.Native; ONLY PackNormals duplicates because ComputeNormals mutates.
    static float[] PackVertices(MeshSpace space) {
        Mesh native = space.Native;
        float[] buffer = new float[native.Vertices.Count * 3];
        for (int i = 0; i < native.Vertices.Count; i++) {
            Point3f v = native.Vertices[i];
            (buffer[3 * i], buffer[3 * i + 1], buffer[3 * i + 2]) = (v.X, v.Y, v.Z);
        }
        return buffer;
    }

    static float[] PackNormals(MeshSpace space) {
        Mesh native = space.DuplicateNative();
        if (native.Normals.Count != native.Vertices.Count) native.Normals.ComputeNormals();
        float[] buffer = new float[native.Vertices.Count * 3];
        for (int i = 0; i < native.Normals.Count; i++) {
            Vector3f n = native.Normals[i];
            (buffer[3 * i], buffer[3 * i + 1], buffer[3 * i + 2]) = (n.X, n.Y, n.Z);
        }
        return buffer;
    }

    static float[] PackCells(PackGrid grid) {
        float[] buffer = new float[grid.CellCount * 3];
        for (int i = 0; i < grid.CellCount; i++) {
            Point3d c = grid.CellCenter(i);
            (buffer[3 * i], buffer[3 * i + 1], buffer[3 * i + 2]) = ((float)c.X, (float)c.Y, (float)c.Z);
        }
        return buffer;
    }

    static float[] PackColors(VectorCloud.ClusterCase cloud) {
        float[] buffer = new float[cloud.Vertices.Count * 4];
        System.Array.Fill(buffer, 1f);
        return buffer;
    }

    static float[] PackVectors(Vector3d[] vectors) {
        float[] buffer = new float[vectors.Length * 3];
        for (int i = 0; i < vectors.Length; i++) {
            (buffer[3 * i], buffer[3 * i + 1], buffer[3 * i + 2]) = ((float)vectors[i].X, (float)vectors[i].Y, (float)vectors[i].Z);
        }
        return buffer;
    }

    static float[] Fill(int count, float value) {
        float[] buffer = new float[count];
        System.Array.Fill(buffer, value);
        return buffer;
    }
}
```

```mermaid
accTitle: Encoding channel flow
accDescr: Pack operations select channel readers, write one typed byte arena, and bind round-trip evidence to the reconciliation digest.
flowchart LR
    PackOp["PackOp (PointCloud / MeshPatch / VoxelGrid / BrepPatch / Field / Toolpath)"] -->|PackKind.Channels active set| PackChannels
    PackChannels -->|position / normal / curvature| Kernel["Rasm.Meshing MeshSpace / Rasm.Spatial VectorCloudMetric.OrientedNormals"]
    PackChannels -->|geodesic scalar lane| Fields["ScalarField.SampleDetailed"]
    PackChannels -->|occupancy SDF sign| Sdf["ScalarField.SignedDistanceFromMeshCase / SampleSdfDetailed"]
    PackChannels -->|"Dtype.Pack — ConvertToHalf / raw bits / unorm8"| Payload["dtype-strided byte[] arena"]
    Payload -->|"Dtype.Unpack → Subtract·Abs·MaxMagnitude"| Witness["per-channel round-trip error"]
    PackOp -->|"EncodeForm.Of(source)"| Reconcile["Reconciliation.Apply(ReconcileOp.Encode)"]
    Reconcile -->|"ReconcileAnswer.Digest → GeometryHash"| Witness
    Witness -->|Lossless verdict + descriptors| EncodedGeometry
    EncodedGeometry -->|"View&lt;float&gt; / View&lt;Half&gt; on the Dtype row"| Compute["Rasm.Compute EncodedTensor"]
    EncodedGeometry -->|EncodingKind.Field / .Toolpath locked rows| AppHost["Rasm.AppHost GeometryPacking capsule"]
    EncodedGeometry -->|"PackSchema.Of — ContentHash schema id"| Schema["PackSchema — columnar field rows"]
    Schema -->|zero-copy column mapping| Lake["Persistence storage plane"]
    PackOp -.->|"DegenerateInput 2400 / EncodingFault 2444"| GeometryFault
```

## [03]-[SCHEMA_AND_EVIDENCE]

- Owner: `PackSchema` is the columnar schema identity every kernel wire carries beside its payload — a `ContentHash`-derived `SchemaId` over the owning `PackKind` and one `PackSchemaField` per active channel. `SchemaNullability` is the null-semantics vocabulary. `EvidenceWire` is the lossless 106-bit count-prefixed binary block over `DoubleDoubleIOExpand`.
- Entry: `PackSchema.Of` is ONE polymorphic derivation discriminating on input shape — the `PackKind` projects the declaration truth, an `EncodedGeometry` projects the packed instance — and `Describes` validates both carriers before comparing ids on the `Fin` rail; `EvidenceWire.WriteBlock`/`ReadBlock` are the binary arms.
- Law: the schema id derives through `ContentHash.Of` over the kind key then one invariant-culture line per field in active-set order, so two kinds sharing an active set still key distinct and any field, arity, dtype, width, or nullability drift re-keys.
- Boundary: `SchemaId` is `UInt128` identity currency, its hex, two-lane `ulong`, and byte-order encodings consuming-seam projections; schema identity binds the representation vocabulary declared here, so a consumer-side roster re-declaring field rows diverges. Each derived-stride column stays contiguous at its descriptor offset, so a consumer maps every field zero-copy while the kernel never touches a storage client — the Persistence storage plane owns the Arrow, Parquet, and Flight adapters reading this schema.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Globalization;
using System.IO;
using System.Text;
using DoubleDouble;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Drawing;

// --- [TYPES] ------------------------------------------------------------------------------
// Dense carries no null sentinel — absence is COLUMN absence; a mask is one Masked row, never a magic value.
[SmartEnum<int>]
public sealed partial class SchemaNullability {
    public static readonly SchemaNullability Dense = new(0);
    public static readonly SchemaNullability Masked = new(1);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PackSchemaField(string Name, int Arity, ChannelDtype Dtype, int ElementStride, SchemaNullability Nulls) {
    public static PackSchemaField Of(EncodingChannel channel) =>
        new(Name: channel.Key, Arity: channel.Arity, Dtype: channel.Dtype, ElementStride: channel.Arity * channel.Dtype.Width, Nulls: SchemaNullability.Dense);
}

public sealed record PackSchema(UInt128 SchemaId, PackKind Kind, Seq<PackSchemaField> Fields) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Fields.Count, floor: 1),
        ValidityClaim.Of(holds: Fields.Map(static field => field.Name).Distinct().Count == Fields.Count),
        ValidityClaim.Of(Fields.ForAll(static field =>
            field.Arity > 0
            && field.ElementStride == field.Arity * field.Dtype.Width
            && (field.Nulls == SchemaNullability.Dense || field.Nulls == SchemaNullability.Masked))),
        ValidityClaim.Of(SchemaId == Of(kind: Kind, fields: Fields).SchemaId),
        ValidityClaim.Of(SchemaId == Of(kind: Kind).SchemaId));

    public static PackSchema Of(PackKind kind) => Of(kind: kind, fields: kind.Channels.Map(PackSchemaField.Of));
    public static PackSchema Of(EncodedGeometry geometry, PackKind kind) =>
        Of(kind: kind, fields: geometry.Descriptors.Map(static descriptor =>
            new PackSchemaField(Name: descriptor.Channel.Key, Arity: descriptor.Channel.Arity, Dtype: descriptor.Dtype,
                ElementStride: descriptor.Channel.Arity * descriptor.Dtype.Width, Nulls: SchemaNullability.Dense)));

    public Fin<Unit> Describes(EncodedGeometry geometry, Op? key = null) {
        PackSchema instance = Of(geometry: geometry, kind: Kind);
        return IsValid && geometry.IsValid && instance.IsValid && instance.SchemaId == SchemaId
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(key.OrDefault().InvalidResult(detail: $"descriptor set diverges from schema {Tag}"));
    }

    public string Tag => SchemaId.ToString(format: "x32", provider: CultureInfo.InvariantCulture);

    private static PackSchema Of(PackKind kind, Seq<PackSchemaField> fields) =>
        new(SchemaId: ContentHash.Of(canonicalBytes: CanonicalBytes(kind: kind, fields: fields)), Kind: kind, Fields: fields);

    // Canonical projection is THIS owner's obligation; ContentHash owns only the digest.
    private static byte[] CanonicalBytes(PackKind kind, Seq<PackSchemaField> fields) =>
        Encoding.UTF8.GetBytes(fields.Fold(
            string.Create(CultureInfo.InvariantCulture, $"{kind.Key}\n"),
            static (acc, field) => acc + string.Create(CultureInfo.InvariantCulture,
                $"{field.Name}|{field.Arity}|{field.Dtype.Key}|{field.Dtype.Width}|{field.Nulls.Key}\n")));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class EvidenceWire {
    public static Unit WriteBlock(BinaryWriter writer, ReadOnlySpan<ddouble> evidence) {
        writer.Write(evidence.Length);
        foreach (ddouble value in evidence) { writer.Write(value); }   // exact hi/lo pair per value
        return unit;
    }

    public static Fin<ddouble[]> ReadBlock(BinaryReader reader, int ceiling, Op? key = null) {
        Op k = key.OrDefault();
        return k.Catch(() => {
            int count = reader.ReadInt32();
            if (count < 0 || count > ceiling) { return Fin.Fail<ddouble[]>(k.InvalidResult(detail: $"evidence block count {count} outside [0, {ceiling}]")); }
            ddouble[] values = new ddouble[count];
            for (int i = 0; i < count; i++) { values[i] = reader.ReadDDouble(); }
            return Fin.Succ(values);
        });
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [JSON_OPTIONS_FREEZE_CATALOG]-[BLOCKED]: which BCL declaration seals the `DDoubleJsonConverter` options identity; catalog `JsonSerializerOptions.MakeReadOnly()` in `libs/csharp/.api/api-system-text-json.md`, then bind one static `EvidenceWire.Json` identity.
- [TENSOR_RESIDENCY_SEAM]-[OPEN]: which consumer coordinates bind this byte-strided wire; align the Compute and AppHost owners with `EncodedGeometry.Payload`, `ByteOffset`, `Bytes`, `Dtype`, and the `PackKind.Toolpath` channel row.
