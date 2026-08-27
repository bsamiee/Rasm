# 1. Name the normal-derived scalar by what it measures

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:679`
```csharp
    public static readonly EncodingChannel Consistency = new("consistency", arity: 1, dtype: ChannelDtype.Float16, wire: "_CONSISTENCY", filter: MeshoptFilter.None, placement: ChannelPlacement.Invariant);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:679`
```csharp
    public static readonly EncodingChannel Verticality = new("verticality", arity: 1, dtype: ChannelDtype.Float16, wire: "_VERTICALITY", filter: MeshoptFilter.None, placement: ChannelPlacement.Invariant);
```

## Why
The lane stores `abs(normal.Z)`, which measures alignment with the vertical axis; “consistency” claims an unrelated statistical property.

## Change
Rename the channel, key, and application-specific wire semantic to `Verticality`.

## Delta
`LOC: +0; symbols: +0`

# 2. Carry the corrected channel through the point-cloud declaration

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:135`
```csharp
    public static readonly PackKind PointCloud = new("point-cloud", Seq(EncodingChannel.Position, EncodingChannel.Normal, EncodingChannel.ColorRgba, EncodingChannel.Consistency));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:135`
```csharp
    public static readonly PackKind PointCloud = new("point-cloud", Seq(EncodingChannel.Position, EncodingChannel.Normal, EncodingChannel.ColorRgba, EncodingChannel.Verticality));
```

## Why
The active set must use the same real quantity name as the channel row.

## Change
Replace the stale `Consistency` reference with `Verticality`.

## Delta
`LOC: +0; symbols: +0`

# 3. Compute point-cloud normals once and project both dependent lanes locally

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:285`
```csharp
        pointCloud: static (k, c) => HashMap(
            (EncodingChannel.Position,    () => Fin.Succ(Encode.Points(c.Source.Vertices))),
            (EncodingChannel.Normal,      () => Encode.Oriented(c.Source, c.Policy, k).Map(Encode.Vectors)),
            (EncodingChannel.ColorRgba,   () => Encode.Colors(c.Colors, c.Source.Vertices.Count)),
            (EncodingChannel.Consistency, () => Encode.Oriented(c.Source, c.Policy, k).Map(Encode.Consistency))),
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:285`
```csharp
        pointCloud: static (k, c) => {
            Fin<Vector3d[]> normals = VectorCloudMetric.OrientedNormals
                .Project<Seq<Vector3d>>(cloud: c.Source, policy: c.Policy.Cloud, key: k)
                .Map(static values => values.ToArray());
            return HashMap(
                (EncodingChannel.Position, () => Fin.Succ(Encode.Points(c.Source.Vertices))),
                (EncodingChannel.Normal, () => normals.Map(vectors => Encode.Interleave3(
                    vectors.Length, i => (vectors[i].X, vectors[i].Y, vectors[i].Z)))),
                (EncodingChannel.ColorRgba, () => c.Colors
                    .ToFin(new GeometryFault.MissingEncodingChannel(EncodingChannel.ColorRgba))
                    .Bind(rows => Encode.Block(rows, c.Source.Vertices.Count, EncodingChannel.ColorRgba))),
                (EncodingChannel.Verticality, () => normals.Map(static vectors =>
                    Array.ConvertAll(vectors, static vector => (float)Math.Abs(vector.Z)))));
        },
```

## Why
The existing map runs the same orientation operation twice and hides three one-use projections behind module members. `Option.ToFin` already owns missing-colour admission.

## Change
Share one settled normal result and keep each trivial projection at the lane it fills.

## Delta
`LOC: +9; symbols: +0`

# 4. Delete the absorbed point-cloud projections

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:496`
```csharp
    internal static Fin<float[]> Colors(Option<Arr<float>> block, int count) =>
        block.Match(
            Some: rows => Block(rows, count, EncodingChannel.ColorRgba),
            None: () => Fin.Fail<float[]>(new GeometryFault.MissingEncodingChannel(EncodingChannel.ColorRgba)));

    internal static float[] Vectors(Vector3d[] vectors) =>
        Interleave3(vectors.Length, i => (vectors[i].X, vectors[i].Y, vectors[i].Z));

    internal static float[] Consistency(Vector3d[] normals) {
        float[] values = new float[normals.Length];
        for (int i = 0; i < normals.Length; i++) values[i] = (float)Math.Abs(normals[i].Z);
        return values;
    }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:496`
```csharp
// Encode.Colors DELETED
// Encode.Vectors DELETED
// Encode.Consistency DELETED
```

## Why
Each member had one caller and no meaning independent of the point-cloud lane definition.

## Change
Delete all three projections after their bodies move to `PackOp.Lanes`.

## Delta
`LOC: -11; symbols: -3`

# 5. Delete the orientation forwarding wrapper

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:595`
```csharp
    internal static Fin<Vector3d[]> Oriented(VectorCloud.ClusterCase cloud, PackPolicy policy, Op key) =>
        VectorCloudMetric.OrientedNormals.Project<Seq<Vector3d>>(cloud: cloud, policy: policy.Cloud, key: key)
            .Map(static seq => seq.ToArray());
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:595`
```csharp
// Encode.Oriented DELETED
```

## Why
The wrapper only renames one existing spatial operation and obscures its duplicate execution.

## Change
Delete `Encode.Oriented`.

## Delta
`LOC: -3; symbols: -1`

# 6. Delete the unused policy projection

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:278`
```csharp
    internal PackPolicy Policy => Switch(
        pointCloud: static p => p.Policy, meshPatch: static m => m.Policy, voxelGrid: static v => v.Policy,
        brepPatch:  static b => b.Policy, field:     static f => f.Policy, toolpath:  static t => t.Policy,
        gaussianSplat: static g => g.Policy);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:278`
```csharp
// PackOp.Policy DELETED
```

## Why
Every lane arm reads its case policy directly, and no consumer uses this second total dispatch.

## Change
Delete `PackOp.Policy`.

## Delta
`LOC: -4; symbols: -1`

# 7. Construct the mesh-patch curvature field at its use

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:294`
```csharp
            (EncodingChannel.Curvature, () => Encode.Vertexwise(Encode.Curvature(m.Source, m.Policy), m.Source, m.Policy.Tolerance, k)),
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:294`
```csharp
            (EncodingChannel.Curvature, () => Encode.Vertexwise(
                new ScalarField.MeanCurvatureFlowCase(m.Source, m.Policy.CurvatureStep, m.Policy.CurvatureRounds),
                m.Source, m.Policy.Tolerance, k)),
```

## Why
The helper only forwards already-admitted values into one generated union case.

## Change
Construct `MeanCurvatureFlowCase` directly in the mesh-patch lane.

## Delta
`LOC: +2; symbols: +0`

# 8. Construct the Brep-patch curvature field at its use

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:305`
```csharp
            (EncodingChannel.Curvature, () => Encode.Vertexwise(Encode.Curvature(b.Source, b.Policy), b.Source, b.Policy.Tolerance, k))),
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:305`
```csharp
            (EncodingChannel.Curvature, () => Encode.Vertexwise(
                new ScalarField.MeanCurvatureFlowCase(b.Source, b.Policy.CurvatureStep, b.Policy.CurvatureRounds),
                b.Source, b.Policy.Tolerance, k))),
```

## Why
This is the forwarding helper's second real caller; omitting it makes the later deletion invalid.

## Change
Construct the field case directly in the Brep-patch lane.

## Delta
`LOC: +2; symbols: +0`

# 9. Delete the curvature forwarding helper

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:571`
```csharp
    internal static ScalarField Curvature(MeshSpace space, PackPolicy policy) =>
        new ScalarField.MeanCurvatureFlowCase(Space: space, TimeStep: policy.CurvatureStep, Iterations: policy.CurvatureRounds);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:571`
```csharp
// Encode.Curvature DELETED
```

## Why
Both callers now construct the generated case without the rename hop.

## Change
Delete `Encode.Curvature`.

## Delta
`LOC: -2; symbols: -1`

# 10. Keep only lane-local facts on the descriptor

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:147`
```csharp
public sealed record EncodingChannelDescriptor(
    EncodingChannel Channel, int Count, int ByteOffset, ChannelDtype Dtype, Option<EncodingChannel> Mask = default) {
    public int Floats => Count * Channel.Arity;
    public int Bytes => Floats * Dtype.Width;
    public SchemaNullability Nulls => Mask.IsSome ? SchemaNullability.Masked : SchemaNullability.Dense;
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:147`
```csharp
public sealed record EncodingChannelDescriptor(EncodingChannel Channel, int ByteOffset, Option<EncodingChannel> Mask = default);
// EncodingChannelDescriptor.Count DELETED
// EncodingChannelDescriptor.Dtype DELETED
// EncodingChannelDescriptor.Floats DELETED
// EncodingChannelDescriptor.Bytes DELETED
// EncodingChannelDescriptor.Nulls DELETED
```

## Why
`Count` belongs to the arena, `Dtype` belongs to the channel, and both extents derive from those authorities. Persisting all five values permits contradictory descriptors.

## Change
Reduce the descriptor to channel, lane-local byte offset, and optional validity-channel reference.

## Ripples
`libs/dotnet/Rasm.Element/.planning/Projection/projection.md:256` sizes from `Lanes.Count * d.Channel.Arity` and uses `d.Channel.Dtype`; `libs/dotnet/Rasm.Compute/.planning/Tensor/residency.md:514` uses `descriptor.Channel.Dtype`; `libs/dotnet/Rasm.Compute/.planning/Runtime/codecs.md:345` hashes `d.Channel.Dtype.Key` and `s.lanes.Count`; `Runtime/tiles.md:174` and `Runtime/payload.md:594` size from `arena.Count * found.Channel.Arity` and use `found.Channel.Dtype`; `libs/dotnet/Rasm.Bim/.planning/Exchange/export.md:453`, `Exchange/tessellation.md:255`, and `Exchange/import.md:480` size from `geometry.Lanes.Count * descriptor.Channel.Arity` and use `descriptor.Channel.Dtype`.

## Delta
`LOC: -5; symbols: -5`

# 11. Validate descriptors from their actual owners

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:169`
```csharp
        Descriptors.Map(static d => d.Channel).Distinct().Count == Descriptors.Count,
        ValidityClaim.CountExactly(count: Witness.ChannelError.Count, expected: Descriptors.Count),
        Descriptors.ForAll(static d => (long)d.Count * d.Channel.Arity * d.Dtype.Width is > 0 and <= int.MaxValue),
        Descriptors.Fold((Offset: 0L, Holds: true), static (acc, d) => {
            long bytes = (long)d.Count * d.Channel.Arity * d.Dtype.Width;
            return (acc.Offset + bytes, acc.Holds && d.ByteOffset == acc.Offset && d.Count == Count);
        }) is var tile && tile.Holds && tile.Offset == Payload.Length,
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:169`
```csharp
        Descriptors.Map(static d => d.Channel).Distinct().Count == Descriptors.Count,
        Descriptors.ForAll(d => d.Mask.Match(
            Some: mask => mask != d.Channel && mask.Arity == 1 && Descriptors.Exists(candidate => candidate.Channel == mask),
            None: static () => true)),
        ValidityClaim.CountExactly(count: Witness.ChannelError.Count, expected: Descriptors.Count),
        Descriptors.ForAll(d => (long)Count * d.Channel.Arity * d.Channel.Dtype.Width is > 0 and <= int.MaxValue),
        Descriptors.Fold((Offset: 0L, Holds: true), (acc, d) => {
            long bytes = (long)Count * d.Channel.Arity * d.Channel.Dtype.Width;
            return (acc.Offset + bytes, acc.Holds && d.ByteOffset == acc.Offset);
        }) is var tile && tile.Holds && tile.Offset == Payload.Length,
```

## Why
Arena count and channel dtype are canonical. The only independent reference left must name a distinct scalar lane in the same arena.

## Change
Derive byte tiling from owners and gate mask integrity once at arena admission.

## Delta
`LOC: +2; symbols: +0`

# 12. Derive raw and typed lane extents at the arena owner

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:181`
```csharp
    public Option<ReadOnlyMemory<byte>> Channel(EncodingChannel channel) =>
        Descriptors.Find(d => d.Channel == channel).Map(d => Payload.Slice(d.ByteOffset, d.Bytes));

    public Fin<EncodingChannelDescriptor> Lane<T>(EncodingChannel channel) where T : unmanaged =>
        Descriptors.Find(d => d.Channel == channel && Unsafe.SizeOf<T>() == d.Dtype.Width)
            .ToFin(new GeometryFault.ChannelWidthMismatch(channel, Unsafe.SizeOf<T>()));

    public ReadOnlyTensorSpan<T> View<T>(EncodingChannelDescriptor lane) where T : unmanaged {
        ReadOnlySpan<T> cast = MemoryMarshal.Cast<byte, T>(Payload.Span.Slice(lane.ByteOffset, lane.Bytes));
        return TensorMarshal.CreateReadOnlyTensorSpan(
            ref MemoryMarshal.GetReference(cast), cast.Length, lengths: [lane.Count, lane.Channel.Arity], strides: [], pinned: false);
    }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:181`
```csharp
    public Option<ReadOnlyMemory<byte>> Channel(EncodingChannel channel) =>
        Descriptors.Find(d => d.Channel == channel).Map(d => Payload.Slice(
            d.ByteOffset, checked(Count * d.Channel.Arity * d.Channel.Dtype.Width)));

    public Fin<EncodingChannelDescriptor> Lane<T>(EncodingChannel channel) where T : unmanaged =>
        Descriptors.Find(d => d.Channel == channel && Unsafe.SizeOf<T>() == d.Channel.Dtype.Width)
            .ToFin(new GeometryFault.ChannelWidthMismatch(channel, Unsafe.SizeOf<T>()));

    public ReadOnlyTensorSpan<T> View<T>(EncodingChannelDescriptor lane) where T : unmanaged {
        ReadOnlySpan<T> cast = MemoryMarshal.Cast<byte, T>(Payload.Span.Slice(
            lane.ByteOffset, checked(Count * lane.Channel.Arity * lane.Channel.Dtype.Width)));
        return TensorMarshal.CreateReadOnlyTensorSpan(
            ref MemoryMarshal.GetReference(cast), cast.Length, lengths: [Count, lane.Channel.Arity], strides: [], pinned: false);
    }
```

## Why
These reads already occur on `EncodedGeometry`, which owns count and reaches the dtype through the channel.

## Change
Calculate each slice from arena and channel authorities.

## Delta
`LOC: +2; symbols: +0`

# 13. Write reduced descriptors

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:384`
```csharp
        EncodingChannelDescriptor descriptor = new(channel, count, state.Offset, channel.Dtype, mask);
        channel.Dtype.Pack(raw, store.Payload.AsSpan(state.Offset, descriptor.Bytes));
        store.Descriptors[state.Slot] = descriptor;
        return (state.Slot + 1, state.Offset + descriptor.Bytes, state.Packed.Add(new PackedLane(descriptor, raw)));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:384`
```csharp
        int bytes = checked(count * channel.Arity * channel.Dtype.Width);
        EncodingChannelDescriptor descriptor = new(channel, state.Offset, mask);
        channel.Dtype.Pack(raw, store.Payload.AsSpan(state.Offset, bytes));
        store.Descriptors[state.Slot] = descriptor;
        return (state.Slot + 1, state.Offset + bytes, state.Packed.Add(new PackedLane(descriptor, raw)));
```

## Why
`Write` already receives the arena count and channel, so copying their projections to every descriptor is redundant.

## Change
Compute byte extent locally and construct the reduced descriptor.

## Delta
`LOC: +1; symbols: +0`

# 14. Grade packed lanes through the channel-owned dtype

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:395`
```csharp
        Seq<(EncodingChannel Channel, double Error)> errors = packed.Lanes.Map(lane => (
            lane.Descriptor.Channel,
            Error(lane.Raw, packed.Store.Payload.AsSpan(lane.Descriptor.ByteOffset, lane.Descriptor.Bytes), lane.Descriptor.Dtype)));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:395`
```csharp
        Seq<(EncodingChannel Channel, double Error)> errors = packed.Lanes.Map(lane => (
            lane.Descriptor.Channel,
            Error(lane.Raw, packed.Store.Payload.AsSpan(
                lane.Descriptor.ByteOffset, lane.Raw.Length * lane.Descriptor.Channel.Dtype.Width), lane.Descriptor.Channel.Dtype)));
```

## Why
Admitted raw length and channel dtype determine the packed slice exactly.

## Change
Derive witness input without descriptor mirrors.

## Delta
`LOC: +1; symbols: +0`

# 15. Use the generated meshopt key as the filter identity

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:650`
```csharp
    public static readonly MeshoptFilter None        = new("NONE",        ordinal: 0, admits: static (_, _) => true);
    public static readonly MeshoptFilter Octahedral  = new("OCTAHEDRAL",  ordinal: 1, admits: static (arity, width) => arity == 4 && width is 1 or 2);
    public static readonly MeshoptFilter Quaternion  = new("QUATERNION",  ordinal: 2, admits: static (arity, width) => arity == 4 && width == 2);
    public static readonly MeshoptFilter Exponential = new("EXPONENTIAL", ordinal: 3, admits: static (_, width) => width == 4);
    public static readonly MeshoptFilter Color       = new("COLOR",       ordinal: 4, admits: static (arity, width) => arity == 4 && width is 1 or 2);

    public int Ordinal { get; }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:650`
```csharp
    public static readonly MeshoptFilter None        = new("NONE",        admits: static (_, _) => true);
    public static readonly MeshoptFilter Octahedral  = new("OCTAHEDRAL",  admits: static (arity, width) => arity == 4 && width is 1 or 2);
    public static readonly MeshoptFilter Quaternion  = new("QUATERNION",  admits: static (arity, width) => arity == 4 && width == 2);
    public static readonly MeshoptFilter Exponential = new("EXPONENTIAL", admits: static (_, width) => width == 4);
    public static readonly MeshoptFilter Color       = new("COLOR",       admits: static (arity, width) => arity == 4 && width is 1 or 2);
// MeshoptFilter.Ordinal DELETED
```

## Why
Thinktecture already generates `Key` as stable wire token and equality identity; the parallel ordinal roster can drift.

## Change
Remove the ordinal column and property, then hash `Filter.Key` below.

## Delta
`LOC: -1; symbols: -1`

# 16. Keep only independent schema-field data

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:722`
```csharp
[SmartEnum<int>]
public sealed partial class SchemaNullability {
    public static readonly SchemaNullability Dense = new(0);
    public static readonly SchemaNullability Masked = new(1);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PackSchemaField(
    EncodingChannel Channel, int Arity, ChannelDtype Dtype, int ElementStride, SchemaNullability Nulls, string WireName, MeshoptFilter Filter) {

    public static PackSchemaField Of(EncodingChannel channel, ChannelDtype dtype, SchemaNullability nulls) =>
        new(Channel: channel, Arity: channel.Arity, Dtype: dtype, ElementStride: channel.Arity * dtype.Width,
            Nulls: nulls, WireName: channel.WireName, Filter: channel.Filter);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:722`
```csharp
// SchemaNullability DELETED
// SchemaNullability.Dense DELETED
// SchemaNullability.Masked DELETED
// --- [MODELS] --------------------------------------------------------------------------
public sealed record PackSchemaField(EncodingChannel Channel, Option<EncodingChannel> Mask);
// PackSchemaField.Arity DELETED
// PackSchemaField.Dtype DELETED
// PackSchemaField.ElementStride DELETED
// PackSchemaField.Nulls DELETED
// PackSchemaField.WireName DELETED
// PackSchemaField.Filter DELETED
// PackSchemaField.Of DELETED
```

## Why
Arity, dtype, stride, wire token, and filter derive from `Channel`; the nullability enum weakens `Mask` by discarding its companion identity.

## Change
Retain only channel and optional mask, deleting every mirrored column and factory.

## Delta
`LOC: -11; symbols: -9`

# 17. Validate and construct reduced schema rows from their owners

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:741`
```csharp
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Fields.Count, floor: 1),
        Fields.Map(static field => field.Channel).Distinct().Count == Fields.Count,
        Fields.ForAll(static field =>
            field.Arity > 0
            && field.ElementStride == field.Arity * field.Dtype.Width
            && field.Filter.Admits(field.Arity, field.Dtype.Width)),
        SchemaId == Of(kind: Kind, fields: Fields).SchemaId);

    public static PackSchema Of(PackKind kind) =>
        Of(kind: kind, fields: kind.Channels.Map(static channel => PackSchemaField.Of(channel, channel.Dtype, SchemaNullability.Dense)));

    public static PackSchema Of(EncodedGeometry geometry, PackKind kind) =>
        Of(kind: kind, fields: geometry.Descriptors.Map(static descriptor =>
            PackSchemaField.Of(descriptor.Channel, descriptor.Dtype, descriptor.Nulls)));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:741`
```csharp
    public bool IsValid => ValidityClaim.All(
        Fields.Map(static field => field.Channel) == Kind.Channels,
        Fields.ForAll(field =>
            field.Channel.Filter.Admits(field.Channel.Arity, field.Channel.Dtype.Width)
            && field.Mask.Match(
                Some: mask => mask != field.Channel && mask.Arity == 1 && Fields.Exists(row => row.Channel == mask),
                None: static () => true)),
        SchemaId == Of(kind: Kind, fields: Fields).SchemaId);

    public static PackSchema Of(PackKind kind) =>
        Of(kind: kind, fields: kind.Channels.Map(static channel => new PackSchemaField(channel, None)));

    public static PackSchema Of(EncodedGeometry geometry, PackKind kind) =>
        Of(kind: kind, fields: geometry.Descriptors.Map(static descriptor =>
            new PackSchemaField(descriptor.Channel, descriptor.Mask)));
```

## Why
Active-set equality subsumes separate non-empty and distinct checks; remaining laws read canonical channel facts and validate the one independent reference.

## Change
Compare directly to `Kind.Channels`, validate filter and mask integrity, and construct reduced rows directly.

## Delta
`LOC: -2; symbols: +0`

# 18. Hash schema identity from canonical owners and the full mask

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:771`
```csharp
                    .Rows(state.Fields, static (field, row) => row
                        .String(field.Channel.Key).Ordinal(field.Arity).Ordinal(field.Dtype.Key).Ordinal(field.Dtype.Width)
                        .Ordinal(field.Nulls.Key).String(field.WireName).Ordinal(field.Filter.Ordinal))),
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:771`
```csharp
                    .Rows(state.Fields, static (field, row) => row
                        .String(field.Channel.Key).Ordinal(field.Channel.Arity)
                        .Ordinal(field.Channel.Dtype.Key).Ordinal(field.Channel.Dtype.Width)
                        .String(field.Channel.WireName).String(field.Channel.Filter.Key)
                        .Optional(field.Mask, static (mask, framed) => framed.String(mask.Key)))),
```

## Why
`CanonicalWriter.Optional` frames presence before the companion key, while channel and Thinktecture keys remain the authorities for every derived fact.

## Change
Project facts from `EncodingChannel`, hash the generated filter key, and retain complete mask identity.

## Ripples
`libs/dotnet/Rasm.Compute/.planning/Runtime/codecs.md:379` retains its `PackSchema.Of(Kind)` call, but geometry lake generations keyed by the prior `SchemaId` must be rebuilt because the canonical preimage changes.

## Delta
`LOC: +2; symbols: +0`

# 19. Compose source identity into the shared witness operation

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:328`
```csharp
            .Bind(count => PackChannels(op, op.Kind, count, k)
                .Bind(packed => Witness(op, packed, k)
                    .Map(witness => new EncodedGeometry(packed.Store.Descriptors.ToSeq(), packed.Store.Payload, packed.Store.Count, witness))))
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:328`
```csharp
            .Bind(count => PackChannels(op, op.Kind, count, k)
                .Bind(packed => SourceDigest(op, k)
                    .Bind(digest => Witness(packed, digest, DigestRoot.Source))
                    .Map(witness => new EncodedGeometry(packed.Store.Descriptors.ToSeq(), packed.Store.Payload, packed.Store.Count, witness))))
```

## Why
Source-rooted packing differs only in how it obtains the digest before the common round-trip fold.

## Change
Bind `SourceDigest` in `Apply` and pass its result to `Witness`.

## Delta
`LOC: +1; symbols: +0`

# 20. Give payload-rooted packing the same witness operation

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:354`
```csharp
            .Bind(packed => Screened(packed, GeometryHash.Create(ContentHash.Of(packed.Store.Payload)), DigestRoot.Payload)
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:354`
```csharp
            .Bind(packed => Witness(packed, GeometryHash.Create(ContentHash.Of(packed.Store.Payload)), DigestRoot.Payload)
```

## Why
`Screened` is a coined name for the same witness operation, not a separate concept.

## Change
Call `Witness` from `Of`.

## Delta
`LOC: +0; symbols: +0`

# 21. Collapse the witness forwarding overload

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:391`
```csharp
    static Fin<RoundTripWitness> Witness(PackOp op, PackedChannels packed, Op key) =>
        SourceDigest(op, key).Bind(digest => Screened(packed, digest, DigestRoot.Source));

    static Fin<RoundTripWitness> Screened(PackedChannels packed, GeometryHash digest, DigestRoot root) {
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:391`
```csharp
// Encode.Witness(PackOp, PackedChannels, Op) DELETED
    static Fin<RoundTripWitness> Witness(PackedChannels packed, GeometryHash digest, DigestRoot root) {
```

## Why
The overload only forwards through `SourceDigest`; both callers now reach the real witness body directly.

## Change
Delete the forwarding overload and rename `Screened` to `Witness`.

## Delta
`LOC: -2; symbols: -1`

# 22. Build the channel-error map through LanguageExt admission

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:400`
```csharp
            None: () => Fin.Succ(RoundTripWitness.Of(digest, root, errors)));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:400`
```csharp
            None: () => Fin.Succ(new RoundTripWitness(digest, root, toHashMap(errors))));
```

## Why
`Prelude.toHashMap` already admits a sequence of typed key-value tuples.

## Change
Construct the witness directly with LanguageExt map admission.

## Delta
`LOC: +0; symbols: +0`

# 23. Delete the hand-rolled witness factory

## From
`libs/dotnet/Rasm/.planning/Drawing/pack.md:158`
```csharp
    public static RoundTripWitness Of(GeometryHash digest, DigestRoot root, Seq<(EncodingChannel Channel, double Error)> errors) =>
        new(digest, root, errors.Fold(HashMap<EncodingChannel, double>(), static (acc, e) => acc.Add(e.Channel, e.Error)));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/pack.md:158`
```csharp
// RoundTripWitness.Of DELETED
```

## Why
Its only caller now uses the package's tuple-map admission directly.

## Change
Delete `RoundTripWitness.Of`.

## Delta
`LOC: -2; symbols: -1`
