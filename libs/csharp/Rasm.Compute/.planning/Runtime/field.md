# [COMPUTE_FIELD]

Rasm.Compute owns the result-specific chunked layout a simulation field lands as bytes: its own 64-byte-header residence-bearing container, the HDF5 interop container the `Runtime/archive#HDF_ARCHIVE` capsule carries, and the scientific-ingest surface dispatching a foreign corpus onto whichever reader owns it. Every invariant this page admits rides a generated admission-bearing owner — the header, the codec policy, the waveform window, and the admitted field are `[ComplexValueObject]`s whose factories ACCUMULATE, so a corpus with three malformed fields reports three.

`FieldCodec` owns the encode/decode surface, `InterchangeIo` the ingest dispatch, and the `FieldResidence`/`Compression`/`FieldElement` row vocabulary the wire codes and container element gates read. The lane composes `Runtime/archive#HDF_ARCHIVE` for every container mechanic, `Runtime/codecs#GEOMETRY_DELTA`'s `Quantization` for the shared bit-budget law, `Runtime/codecs#CONTENT_ADDRESSING` for the emitted artifact's identity, the `Solver/field#DISCRETE_FIELD` `FieldSpace` shape, the model-lane `Model/run#RUN_MODES` ONNX session, and the Persistence `Ingest/pointcloud#SCAN_SOURCE` reader.

## [01]-[INDEX]

- [02]-[FIELD_RESULT_CODEC]: chunked simulation-field layout; error-bounded lossy, learned-residual-predicted, and exact lossless residence; the native header and the HDF5 interop container; zero-copy.
- [03]-[SCIENTIFIC_INGEST]: the container-dispatch surface — chunked field, point-scan, and waveform-corpus arms, each composing the reader that owns its decode.

## [02]-[FIELD_RESULT_CODEC]

- Owner: `FieldResidence` the closed exact/quantized/predicted residence `[Union]` whose per-case bits and bound ARE the arm's law — a lossless case selecting a lossy transform is unrepresentable — carrying the wire `Code` and its inverse `Of` on ONE owner so the forward projection and the header reconstruction cannot fork; `Compression` the `[SmartEnum<byte>]` byte-transform vocabulary whose rows carry their own pack and unpack arms, so the header's compression byte is a total row read rather than a bool the encode re-derives; `FieldCodecPolicy` the `[ComplexValueObject]` chunked-layout policy carrying the residence case, the compression row, and the pinned-or-inherited chunk shape as an `Option`; `FieldHeader` the `[ComplexValueObject]` self-describing 64-byte-prefix admission owner whose accumulating factory replaces the abort ladder a truncated payload used to walk one field at a time; `ResidualPredictor` the content-keyed model-lane chunk predictor; `FieldArtifact` the chunked simulation-field carrier over CGNS/EnSight/VTK/Zarr; `AdmittedField` the encode-side admission evidence, minted only by its own accumulating factory so the interior can never be handed an unadmitted pair; `FieldWindow` the Compute-owned station-slab declaration a partial read carries, lowered to a `HyperslabSelection` at the container edge so no host selection type reaches a Compute signature; `FieldCodec` the static encode/decode surface projecting a `FieldSpace`-shaped result into a Zarr/VTK-class chunked layout with a zero-copy solver↔store↔viz handoff. Two containers carry a chunked field and both are this codec's: its OWN 64-byte-header layout is the residence-bearing encode target, and HDF5 over the `Runtime/archive#HDF_ARCHIVE` capsule is both the ingest target every h5py and netCDF-4 corpus already writes and the interop egress those toolchains read back.
- Entry: `public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, Option<ResidualPredictor> predictor = default)` admits the header WHOLE, decodes exact and quantized bodies directly, and reconstructs predicted bodies through their required model; `public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default)` admits the (field, policy) pair into an `AdmittedField` and emits the chunked layout under the residence case's own law; `public static Fin<FieldArtifact> Hdf5Decode(string formatKey, HdfHandle handle, string dataset, Instant at, Option<FieldWindow> window = default)` reads the same station×component chunk model out of an HDF5 container, the optional `window` the station slab a frustum or station read declares; `public static Fin<ComputeArtifact> Hdf5Encode(FieldArtifact field, FieldCodecPolicy policy, HdfArchivePolicy archive, Stream sink, Instant at)` emits it as an HDF5 1.10 container over the archive's cursor-guarded writer, `Predicted` refusing typed; `public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field)` the multi-segment zero-flatten view the `Runtime/channels#ARTIFACT_FRAMES` frame law drains; the chunk-grid derivation is `Runtime/archive#CHUNK_CURSOR` `ChunkGrid.Derive`, composed and never re-spelled here.
- Auto: the chunk blob exposes two views — `ChunkSequence`, a multi-segment `ReadOnlySequence<byte>` (one segment per chunk) streamed with no flatten, and `FieldArtifact.Chunk(ordinal)`, the per-ordinal random-access slice a frustum cull reads through the artifact's own `ChunkGrid.Slice` — both addressing chunks by grid position, not byte offset; the quantized residence codes each chunk to its case's bit budget through the shared `Runtime/codecs#GEOMETRY_DELTA` `Quantization` kernel (`TensorPrimitives.MaxMagnitude` scale and the bulk `Divide`/`Round`/`Multiply` fold, never a per-element `Select` over a whole field) and gates its own bound; the predicted residence walks chunks CAUSALLY — the stencil gathers axis-aligned face neighbours by grid coordinate from the RECONSTRUCTED buffer (`GatherNeighbours`, the true spatial stencil, never a 1-D window crossing grid faces and never source values the decoder cannot hold), predicts through the `ResidualPredictor` ONNX field model, quantizes only the prediction residual, and re-codes an over-bound chunk's residual exactly (step 0) so the case bound holds by construction and `Reconstruct` inverts the walk from stored residuals alone; lossless compression rides the `Compression.Brotli` row's own pack arm over the `System.IO.Compression` span codec sized by `GetMaxCompressedLength`, no intermediate stream; the `ByteString` wrap fanning one chunk buffer to store blob and viz upload is the `Runtime/channels#ARTIFACT_FRAMES` frame law, composed.
- Receipt: the `StreamSegment` receipt carries the field artifact id, the chunk count, and the emitted bytes; a lossy or residual-predicted encode stamps the achieved max-residual against the bound on the `Cache` receipt so an error-bounded compression is auditable — `FieldArtifact.MaxResidual` is the value that stamp reads, carried on the artifact and written as the container's own `max-residual` attribute at the interop egress.
- Packages: PureHDF (`NativeDataset.Read<T>` chunk-slab reads under `PureHDF.Selections.HyperslabSelection` file selections, `IH5DataType`/`H5DataTypeClass` element gating, `IH5DataLayout.Chunks` grid seating, `H5Dataset<T>` deferred chunked writes — every open, cache, filter, and writer mechanic the `Runtime/archive#HDF_ARCHIVE` capsule owns), System.IO.Hashing, System.Numerics.Tensors (`TensorPrimitives.MaxMagnitude`/`ConvertToSingle`/`Divide`/`Round`/`Multiply`), Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox (`System.IO.Compression` Brotli span codec, `System.Buffers` sequence segments)
- Growth: a new residence law is one `FieldResidence` case whose arm the `FieldEncode` `Switch` demands at compile time and whose wire code the one `Code`/`Of` pair carries; a new byte transform is one `Compression` row carrying its pack and unpack arms, and the header byte reads it with zero codec edits; a new layout gate is one clause on the `FieldHeader` factory, accumulated beside every sibling; a learned predictor is one `ResidualPredictor` content-keyed ONNX session reused across chunks; zero new surface — a `ResidualCoder`/`NeuralFieldCompressor` sibling is collapsed onto the `FieldResidence.Predicted` case and the one `ResidualEncode`/`Reconstruct` pair.
- Boundary: the header is ADMITTED, never sliced-then-guarded — `FieldHeader.Read` reads a span into a factory whose validation accumulates, so a payload with a bad rank AND an impossible chunk count reports both and every refusal wears this codec's own slug rather than a BCL slice-range message dressed as a verdict; the fixed `HeaderBytes` prefix gates before the first slice and the `Pack` writer sizes off the same const so one number states the prefix. Field codec is the result-specific layout the generic blob/snapshot codecs never owned — a scalar/vector/tensor field rides the `Solver/field#DISCRETE_FIELD` `FieldSpace` shape, chunked by station and component, never a generic byte blob; HDF5 is that same chunk model under another container, which is why it is ADMITTED rather than adapted — one station×component chunk IS one HDF5 chunk and the shuffle-plus-deflate pipeline IS this codec's compression leg, so an h5py or netCDF-4 corpus reads directly with no format bridge; the ingest seats the dataspace as the field extent with the trailing axis the COMPONENT axis of ONE dataset — one dataset per component is the refuted sibling layout, forking the chunk address a consumer computes — the container's own chunk grid seating onto one `ChunkGrid` so `Chunk(ordinal)` and the residual stencil survive ingest, an unwindowed read walking the corpus chunk-by-chunk and a declared `FieldWindow` reading exactly the station slab a frustum or station read asks for, so a re-chunk on import and an unqualified whole-dataset call are both the deleted form; the reader is pure managed with no native asset and decodes little-endian alone, so a big-endian source corpus refuses TYPED at the archive open rather than transposing bytes behind the caller; HDF5 writes are create-only and chunk-aligned, so an in-place edit of an ingested container is unrepresentable; the native layout stays the residence-bearing format and `Hdf5Encode` is its interop egress — `Exact` and `Quantized` land with bits and bound as attributes (evidence a foreign reader may ignore), `Predicted` refuses because no container slot carries a residence law a reader must enforce; the layout composes the suite `XxHash128` chunk identity content-addressed on the Persistence blob lane, so an identical chunk dedups and a re-read warms, a second field store the rejected form; the error bound is per-residence-case data the receipt records, never silently exceeded — the quantized arm faults an unmeetable bound and the predicted arm holds its bound by per-chunk exact fallback; the zero-copy edge is the remote frame law's `GetReadOnlySequence`/`UnsafeWrap` path, so a chunk crosses solver→store→viz with no managed copy, a `ToArray` flatten the named defect; the learned-compression terminal `ResidualPredictor` is one model-lane `Model/run#RUN_MODES` ONNX session content-keyed by the parametric-family digest and shared across chunks, composing the model lane rather than minting a second inference path, its grid-coordinate chunk index preserved (content-defined byte chunking destroys the grid locality the predictor needs — the FastCDC `Runtime/codecs#GEOMETRY_DELTA` chunker is the rejected rewrite), only the bounded residual stored, an over-bound chunk re-coded exact so the bound holds structurally, and the causal reconstructed-stencil walk making `Reconstruct` the codec's true inverse, the ONNX weights one content-addressed artifact the Python offline-science companion fits over the same offline-training seam the optimizer surrogate uses (never an in-proc fit), the achieved residual auditable on the `Cache` receipt.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Residence is a CLOSED case family, never a bool triple whose combinations the encoder re-derives: each case
// carries exactly the law its arm enforces, so a "lossless" policy selecting a lossy transform is unrepresentable.
// `Code` and `Of` are the FORWARD and INVERSE of one wire correspondence and seat on ONE owner — the hand switch
// this collapse deletes spelled the projection at the writer and its reconstruction at the reader, so a new case
// could land at one end and pass every census at the other.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldResidence {
    private FieldResidence() { }

    public sealed record Exact : FieldResidence;
    public sealed record Quantized(int Bits, double Bound) : FieldResidence;
    public sealed record Predicted(int Bits, double Bound) : FieldResidence;

    public int Code => Switch(
        exact: static _ => 0,
        quantized: static _ => 1,
        predicted: static _ => 2);

    public int QuantizationBits => Switch(
        exact: static _ => 0,
        quantized: static q => q.Bits,
        predicted: static p => p.Bits);

    public double ErrorBound => Switch(
        exact: static _ => 0.0,
        quantized: static q => q.Bound,
        predicted: static p => p.Bound);

    // The bit budget the quantizer can actually meet — an arm outside it emits a step no float lane resolves.
    public const int MinBits = 1;
    public const int MaxBits = 24;

    public static Validation<Error, FieldResidence> Of(int code, int bits, double bound) =>
        (code, bits, bound) switch {
            (0, 0, 0d) => Success<Error, FieldResidence>(new Exact()),
            (1, >= MinBits and <= MaxBits, _) when double.IsFinite(bound) && bound > 0d => Success<Error, FieldResidence>(new Quantized(bits, bound)),
            (2, >= MinBits and <= MaxBits, _) when double.IsFinite(bound) && bound > 0d => Success<Error, FieldResidence>(new Predicted(bits, bound)),
            _ => Fail<Error, FieldResidence>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Rostered, new ContractEvidence.Scalars(code, bits, bound)))),
        };
}

// The header's compression byte is a two-value CODEC AXIS, not a bool: each row carries its own pack and unpack
// arms, so the byte reads as a total row and a `_ => throw` arm over an open integer disappears. The `Compress`
// bool this replaces forced the encode to re-derive its transform from a flag at every site.
[SmartEnum<byte>]
public sealed partial class Compression {
    public static readonly Compression None = new(0,
        pack: static data => Fin.Succ(data),
        unpack: static (data, raw) => data.Length == raw
            ? Fin.Succ(data)
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.Count(data.Length, raw))));

    // CONSTRAINT: `TryCompress` cannot answer false here — the destination is sized by `GetMaxCompressedLength`,
    // which is the encoder's own worst-case bound — so the fail arm states an outcome the API contract forecloses
    // and exists only because the BCL entry is `bool`-shaped.
    public static readonly Compression Brotli = new(1,
        pack: static data => {
            byte[] destination = new byte[BrotliEncoder.GetMaxCompressedLength(data.Length)];
            return BrotliEncoder.TryCompress(data.Span, destination, out int written)
                ? Fin.Succ((ReadOnlyMemory<byte>)destination.AsMemory(0, written))
                : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.None())));
        },
        unpack: static (data, raw) => {
            byte[] destination = new byte[raw];
            return BrotliDecoder.TryDecompress(data.Span, destination, out int written) && written == raw
                ? Fin.Succ((ReadOnlyMemory<byte>)destination)
                : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.Count(written, raw))));
        });

    [UseDelegateFromConstructor]
    public partial Fin<ReadOnlyMemory<byte>> Pack(ReadOnlyMemory<byte> data);

    [UseDelegateFromConstructor]
    public partial Fin<ReadOnlyMemory<byte>> Unpack(ReadOnlyMemory<byte> data, int rawLength);
}

// ONE element gate for every HDF5 float ingest on the branch, keyed on the byte SIZE the class already narrowed
// to. The gate — the `(H5DataTypeClass, Size)` correspondence and its typed refusal — was spelled byte-for-byte at
// the field decode and the waveform decode; the width branch stays at each site because that is where the read
// generic instantiates, and the library never width-converts (a float32 destination over a float64 dataset and
// the reverse both refuse `Unable to decode values types of different type size.`).
[SmartEnum<int>]
public sealed partial class FieldElement {
    public static readonly FieldElement Single = new(4);
    public static readonly FieldElement Double = new(8);

    public static Validation<Error, FieldElement> Of(IH5DataType type) =>
        type.Class == H5DataTypeClass.FloatingPoint && TryGet(type.Size, out FieldElement? row) && row is not null
            ? Success<Error, FieldElement>(row)
            : Fail<Error, FieldElement>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Count((long)type.Class, checked((long)type.Size)))));
}

// --- [MODELS] -----------------------------------------------------------------------------
// The Compute-owned station-slab declaration: PureHDF's `Selection` crossed two public entries as a nullable
// foreign type, so a caller had to know the container's coordinate order to ask for one station. The window
// states WHAT it wants and lowers to the hyperslab at the container edge alone.
[ComplexValueObject]
public sealed partial class FieldWindow {
    public int Station { get; }
    public int Stations { get; }

    public HyperslabSelection Lower(ReadOnlySpan<ulong> extent) =>
        new(rank: extent.Length,
            starts: [(ulong)Station, .. extent[1..].ToArray().Select(static _ => 0UL)],
            blocks: [(ulong)Stations, .. extent[1..].ToArray()]);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int station, ref int stations) =>
        validationError = station >= 0 && stations >= 1
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { $"<field-window:{station}:{stations}>" }));
}

// ChunkShape is a GATE, never an inherited decoration, and the gate is now a CASE rather than a length probe: a
// pinned shape admits only an artifact laid out exactly so (re-chunking is not this codec's operation), `None`
// inherits the artifact layout, and disagreement is the one typed disposition.
[ComplexValueObject]
public sealed partial class FieldCodecPolicy {
    public static readonly FieldCodecPolicy Lossless = Create(None, new FieldResidence.Exact(), Compression.Brotli);
    public static readonly FieldCodecPolicy Bounded = Create(None, new FieldResidence.Quantized(Bits: 12, Bound: 1e-3), Compression.Brotli);
    public static readonly FieldCodecPolicy Residual = Create(None, new FieldResidence.Predicted(Bits: 12, Bound: 1e-3), Compression.Brotli);

    public Option<Seq<int>> ChunkShape { get; }
    public FieldResidence Residence { get; }
    public Compression Compression { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<Seq<int>> chunkShape, ref FieldResidence residence, ref Compression compression) =>
        validationError = chunkShape.Match(
            None: () => null,
            Some: shape => shape.Count > 0 && shape.ForAll(static extent => extent > 0)
                ? null
                : (ValidationError)new ValidationError($"<field-policy-chunk-shape:[{string.Join(',', shape)}]>"));
}

// The self-describing header, ADMITTED WHOLE. The abort ladder this replaces walked nine sequential guards over
// one 64-byte prefix, so a caller holding three malformed fields learned exactly one and re-submitted twice to
// learn the rest. Every clause below accumulates, and `Read` is a pure span reader that feeds the factory.
[ComplexValueObject]
public sealed partial class FieldHeader {
    public string Station { get; }
    // Rank rides the landed `Solver/field#DISCRETE_FIELD` `FieldRank` row, and `Components` is the row's OWN
    // derivation over the spatial dimension the grid declares — so a vector field carrying five components is
    // refused at admission rather than written into a container a reader then trusts. The WIRE keeps its two
    // int32 slots verbatim (`tests/contracts` `[02.27]` freezes that layout): the rank slot carries the row's
    // `Order` and the component slot carries the derived count, which is evidence a foreign reader may check.
    public FieldRank Rank { get; }
    public int Components { get; }
    public long Count { get; }
    public int RawBytes { get; }
    public FieldResidence Residence { get; }
    public Compression Compression { get; }
    public Seq<int> GridChunks { get; }
    public Seq<int> ChunkShape { get; }

    // Fixed prefix ahead of the variable grid and chunk extents. `Read` gates on it BEFORE the first slice: a
    // payload shorter than the prefix would otherwise publish a BCL slice-range message as this codec's typed
    // refusal — unslugged, unmatched by any recovery, and naming an argument instead of a truncated artifact.
    public const int PrefixBytes = 64;

    public int HeaderLength => PrefixBytes + (GridChunks.Count * sizeof(int)) + (ChunkShape.Count * sizeof(int));
    public long ChunkElements => ChunkShape.Fold(1L, static (product, extent) => product * extent) * Components;
    public long GridCount => GridChunks.Fold(1L, static (product, extent) => product * extent);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string station, ref FieldRank rank, ref int components, ref long count, ref int rawBytes,
        ref FieldResidence residence, ref Compression compression, ref Seq<int> gridChunks, ref Seq<int> chunkShape) {
        long elements = chunkShape.Fold(1L, static (product, extent) => extent > 0 && product <= int.MaxValue / extent ? product * extent : long.MaxValue);
        elements = components > 0 && elements <= int.MaxValue / components ? elements * components : long.MaxValue;
        int dim = gridChunks.Count;
        validationError = Seq(
                components == rank.Components(dim) ? None : Some($"<field-rank-components:{rank.Key}:{dim}:{components}>"),
                count >= 0L ? None : Some($"<field-count:{count}>"),
                rawBytes >= 0 ? None : Some($"<field-raw-bytes:{rawBytes}>"),
                gridChunks.Count is >= 1 and <= 32 ? None : Some($"<field-grid-rank:{gridChunks.Count}>"),
                chunkShape.Count == gridChunks.Count ? None : Some($"<field-chunk-rank:{chunkShape.Count}:{gridChunks.Count}>"),
                gridChunks.ForAll(static extent => extent > 0) ? None : Some($"<field-grid-extent:[{string.Join(',', gridChunks)}]>"),
                chunkShape.ForAll(static extent => extent > 0) ? None : Some($"<field-chunk-extent:[{string.Join(',', chunkShape)}]>"),
                elements is > 0L and <= int.MaxValue / sizeof(float) ? None : Some($"<field-shape:{components}:{elements}>"))
            .Somes()
            .Match(Empty: () => null, More: (head, tail) => new ValidationError(string.Join(';', head.Cons(tail))));
    }

    // Pure span reader: every field lands in the factory, so the ONLY refusal path is the accumulated one above.
    // A payload shorter than the fixed prefix, or shorter than the variable extents it declares, is the one gate
    // that must fire before a read rather than inside the factory.
    public static Validation<Error, FieldHeader> Read(ReadOnlySpan<byte> span) {
        if (span.Length < PrefixBytes) {
            return Fail<Error, FieldHeader>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(span.Length, PrefixBytes))));
        }
        int gridRank = BinaryPrimitives.ReadInt32LittleEndian(span[56..]);
        if (gridRank is < 1 or > 32 || PrefixBytes + (gridRank * 4) + sizeof(int) > span.Length) {
            return Fail<Error, FieldHeader>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(gridRank, span.Length))));
        }
        int gridEnd = PrefixBytes + (gridRank * 4);
        int chunkRank = BinaryPrimitives.ReadInt32LittleEndian(span[(gridEnd - 4)..]);
        if (chunkRank is < 1 or > 32 || gridEnd + (chunkRank * 4) > span.Length) {
            return Fail<Error, FieldHeader>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(chunkRank, gridRank, span.Length))));
        }
        Seq<int> grid = toSeq(Range(0, gridRank).Select(axis => BinaryPrimitives.ReadInt32LittleEndian(span[(60 + (axis * 4))..])));
        Seq<int> chunk = toSeq(Range(0, chunkRank).Select(axis => BinaryPrimitives.ReadInt32LittleEndian(span[(gridEnd + (axis * 4))..])));
        return (FieldResidence.Of(
                    BinaryPrimitives.ReadInt32LittleEndian(span[36..]),
                    BinaryPrimitives.ReadInt32LittleEndian(span[40..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(span[44..])),
                Compression.Validate(span[52], null, out Compression? codec) is null && codec is not null
                    ? Success<Error, Compression>(codec)
                    : Fail<Error, Compression>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Rostered, new ContractEvidence.Count(span[52], Compression.Items.Count)))),
                Ranked(BinaryPrimitives.ReadInt32LittleEndian(span[16..])))
            .Apply((residence, compression, rank) => Create(
                Encoding.ASCII.GetString(span[..16]).TrimEnd('\0'),
                rank,
                BinaryPrimitives.ReadInt32LittleEndian(span[20..]),
                BinaryPrimitives.ReadInt64LittleEndian(span[24..]),
                BinaryPrimitives.ReadInt32LittleEndian(span[32..]),
                residence, compression, grid, chunk))
            .As();
    }

    // The wire rank slot carries the row's ORDER, so the int32 the frozen layout declares resolves back to the one
    // `Solver/field#DISCRETE_FIELD` vocabulary rather than standing as a bare integer this codec re-interprets.
    static Validation<Error, FieldRank> Ranked(int order) =>
        FieldRank.Items.Find(row => row.Order == order)
            .ToValidation<Error>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Rank(order, FieldRank.Items.Count))));

    // The writer sizes off the same const the reader gates on, so ONE number states the prefix at both ends.
    public byte[] Write() {
        byte[] header = new byte[HeaderLength];
        Encoding.ASCII.GetBytes(Station.PadRight(16, '\0')[..16]).CopyTo(header, 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(16), Rank.Order);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(20), Components);
        BinaryPrimitives.WriteInt64LittleEndian(header.AsSpan(24), Count);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(32), RawBytes);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(36), Residence.Code);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(40), Residence.QuantizationBits);
        BinaryPrimitives.WriteDoubleLittleEndian(header.AsSpan(44), Residence.ErrorBound);
        header[52] = Compression.Key;
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(56), GridChunks.Count);
        GridChunks.Iter((axis, extent) => BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(60 + (axis * 4)), extent));
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(60 + (GridChunks.Count * 4)), ChunkShape.Count);
        ChunkShape.Iter((axis, extent) => BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(PrefixBytes + (GridChunks.Count * 4) + (axis * 4)), extent));
        return header;
    }
}

public sealed record ResidualPredictor(
    UInt128 FamilyDigest,
    ModelIdentity Model,
    string InputName,
    string OutputName,
    int NeighbourStencil,
    InferenceSession Session,
    RunOptions Options,
    CancelScope Scope) {
    public Fin<float[]> Predict(float[] stencil, int chunkElements) =>
        RunOps.Bind(new RunInput.Managed<float>(InputName, stencil, [1, stencil.Length])).Bind(inputs =>
            Session.Infer(Options, Scope, inputs, Seq(OutputName),
                results => {
                    ReadOnlySpan<float> predicted = results.First().GetTensorDataAsSpan<float>();
                    return predicted.Length >= chunkElements ? Fin.Succ(predicted[..chunkElements].ToArray()) : Fin.Fail<float[]>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(predicted.Length, chunkElements))));
                }));
}

// The chunked carrier. `Grid` is the ONE `Runtime/archive#CHUNK_CURSOR` owner, so the ordinal↔slab
// correspondence a frustum cull reads, the container writer walks, and the residual stencil decomposes are one
// value rather than three agreeing spellings.
public sealed record FieldArtifact(
    string FormatKey,
    string Station,
    FieldRank Rank,
    int Components,
    long Count,
    ChunkGrid Grid,
    ReadOnlyMemory<byte> Chunks,
    double MaxResidual,
    Instant At) {
    public int ChunkElements => Grid.ChunkElements;
    public int ChunkCount => Grid.Count;

    // Random-access read of one chunk's float bytes by its grid ordinal — the frustum-cull seam: a viewport
    // maps its frustum onto grid coordinates, those onto ordinals, and reads only the intersected slices.
    public ReadOnlyMemory<byte> Chunk(int ordinal) => Chunks[Grid.Slice(ordinal, sizeof(float), Chunks.Length)];
}

// Encode-side admission EVIDENCE, and the only way to hold an admitted pair: the factory accumulates every
// disagreement between artifact and policy, so a caller whose residence bits AND chunk shape both refuse learns
// both, and no interior arm can be handed a raw (field, policy) tuple that skipped the gate.
[ComplexValueObject]
public sealed partial class AdmittedField {
    public FieldArtifact Field { get; }
    public FieldCodecPolicy Policy { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FieldArtifact field, ref FieldCodecPolicy policy) {
        long chunkBytes = (long)field.ChunkElements * sizeof(float);
        validationError = Seq(
                field.Components > 0 ? None : Some($"<field-codec-components:{field.Components}>"),
                field.Count >= 0L ? None : Some($"<field-codec-count:{field.Count}>"),
                field.Chunks.Length % sizeof(float) == 0 ? None : Some($"<field-codec-alignment:{field.Chunks.Length}>"),
                chunkBytes > 0L && field.Chunks.Length <= chunkBytes * field.ChunkCount ? None : Some($"<field-codec-extent:{field.Chunks.Length}:{chunkBytes}:{field.ChunkCount}>"),
                // Pinned policy shape GOVERNS: artifact layout equals it exactly, while `None` inherits.
                policy.ChunkShape.Match(None: () => true, Some: shape => shape.ToArray().AsSpan().SequenceEqual(field.Grid.Chunk.Span.ToArray().Select(static axis => (int)axis).ToArray()))
                    ? None
                    : Some($"<field-codec-chunk-shape:{policy.ChunkShape.Map(static shape => string.Join(',', shape)).IfNone(string.Empty)}>"))
            .Somes()
            .Match(Empty: () => null, More: (head, tail) => new ValidationError(string.Join(';', head.Cons(tail))));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FieldCodec {
    // Default chunk budget for derived grids; a slab above it splits its largest interior axis until it fits.
    public const int ChunkElementTarget = 1 << 18;

    public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, Option<ResidualPredictor> predictor = default) =>
        Decode(formatKey, bytes, at).ToFin()
            .Bind(decoded => decoded.Header.Residence.Switch(
                state: (Decoded: decoded.Artifact, Predictor: predictor),
                exact: static (state, _) => Fin.Succ(state.Decoded),
                quantized: static (state, _) => Fin.Succ(state.Decoded),
                predicted: static (state, _) => state.Predictor
                    .ToFin(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Resource)))
                    .Bind(net => Reconstruct(state.Decoded, net))));

    // The whole decode is an ACCUMULATING admission: header fields, grid seating, and body unpack each report on
    // the same rail, so a corrupt artifact answers with every fact it violated rather than the first one read.
    static Validation<Error, (FieldArtifact Artifact, FieldHeader Header)> Decode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at) =>
        FieldHeader.Read(bytes.Span).Bind(header =>
            ChunkGrid.Seat(
                    [.. header.GridChunks.Zip(header.ChunkShape, static (grid, chunk) => (ulong)((long)grid * chunk))],
                    [.. header.ChunkShape.Map(static axis => (uint)axis)])
                .Bind(grid => header.Compression.Unpack(bytes[header.HeaderLength..], header.RawBytes).ToValidation<Error>()
                    .Bind(payload => header.GridCount == (payload.Length + ((long)grid.ChunkElements * sizeof(float)) - 1) / ((long)grid.ChunkElements * sizeof(float))
                        ? Success<Error, (FieldArtifact, FieldHeader)>((
                            new FieldArtifact(formatKey, header.Station, header.Rank, header.Components, header.Count, grid, payload, 0.0, at), header))
                        : Fail<Error, (FieldArtifact, FieldHeader)>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.Count(payload.Length, header.GridCount)))))));

    public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default) =>
        AdmittedField.Validate(field, policy, out AdmittedField? admitted) is { } error
            ? Fin.Fail<ComputeArtifact>((ComputeFault)error)
            : admitted!.Policy.Residence.Switch(
                state: (admitted.Field, Predictor: predictor),
                exact: static (s, _) => Fin.Succ(s.Field with { MaxResidual = 0.0 }),
                quantized: static (s, q) => Fin.Succ(Quantize(s.Field, q.Bits)).Bind(coded =>
                    coded.MaxResidual <= q.Bound
                        ? Fin.Succ(coded)
                        : Fin.Fail<FieldArtifact>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(coded.MaxResidual, 0d, q.Bound)))),
                predicted: static (s, p) => s.Predictor
                    .ToFin(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Resource)))
                    .Bind(net => ResidualEncode(s.Field, p, net)))
            .Bind(encoded => Pack(encoded, admitted!.Policy)
                .Map(packed => ComputeArtifact.Of(formatKey, packed, at, [admitted.Policy.Residence.QuantizationBits, admitted.Policy.Residence.ErrorBound])));

    // Self-describing layout: the header states the prefix, the uncompressed payload length, the grid-coordinate
    // index, and the chunk extent, so Decode rebuilds the chunk grid and residual stencil from bytes alone —
    // never an out-of-band policy agreement that mis-counts chunks.
    static Fin<ReadOnlyMemory<byte>> Pack(FieldArtifact field, FieldCodecPolicy policy) =>
        FieldHeader.Validate(
                field.Station, field.Rank, field.Components, field.Count, field.Chunks.Length,
                policy.Residence, policy.Compression,
                toSeq(field.Grid.Grid.Span.ToArray()),
                toSeq(field.Grid.Chunk.Span.ToArray().Select(static axis => (int)axis)),
                out FieldHeader? header) is { } error
            ? Fin.Fail<ReadOnlyMemory<byte>>((ComputeFault)error)
            : policy.Compression.Pack(field.Chunks).Map(body => (ReadOnlyMemory<byte>)(byte[])[.. header!.Write(), .. body.Span]);

    // HDF5 ingest arm over the [HDF_ARCHIVE] handle. The dataspace IS the field extent with the trailing axis
    // the component axis — the same station×component layout the native header spells — and the container's OWN
    // chunk grid seats through `ChunkGrid.Seat`, so `Chunk(ordinal)`, `ChunkSequence`, and the residual stencil
    // stay live for an ingested corpus instead of collapsing onto one giant chunk. Contiguous corpora
    // (`Layout.Chunks` empty) land as one station-outermost derived grid. The predicted residence reaches only
    // the native arm because only the native header names a residence at all — an HDF5 corpus already applied
    // whatever transform its producer chose, so an ingested field lands `Exact` rather than publishing a bound
    // this codec never enforced.
    public static Fin<FieldArtifact> Hdf5Decode(string formatKey, HdfHandle handle, string dataset, Instant at, Option<FieldWindow> window = default) =>
        handle.Dataset(dataset).ToValidation<Error>().Bind(source => {
            ulong[] extent = source.Space.Dimensions;
            return (extent.Length >= 2 && extent.All(static axis => axis is not 0UL and <= int.MaxValue)
                        ? Success<Error, ulong[]>(extent)
                        : Fail<Error, ulong[]>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Extent([.. extent.Select(static axis => checked((long)axis))])))),
                    FieldElement.Of(source.Type))
                .Apply(static (dims, element) => (Dims: dims, Element: element))
                .Bind(shaped => (source.Layout.Chunks is { Length: > 0 } chunked
                        ? ChunkGrid.Seat(shaped.Dims, chunked)
                        : ChunkGrid.Derive([.. shaped.Dims[..^1].Select(static axis => (int)axis)], (int)shaped.Dims[^1], ChunkElementTarget))
                    .Bind(grid => {
                        int components = (int)shaped.Dims[^1];
                        return (Ranked(shaped.Dims.Length - 1, components), Read(source, handle.Access, window, shaped.Dims, shaped.Element))
                            .Apply((rank, values) => new FieldArtifact(
                                formatKey, dataset, rank, components,
                                values.LongLength / components, grid,
                                MemoryMarshal.AsBytes(values.AsSpan()).ToArray(), 0.0, at))
                            .As();
                    }));
        }).ToFin();

    // One chunk-aligned slab read per station in index order under the handle's bounded per-read cache — never
    // one unqualified whole-dataset call; a declared window reads exactly its lowered selection. Size 8 reads
    // double and narrows ONCE through the vectorized converter, since the library never width-converts.
    static Validation<Error, float[]> Read(NativeDataset source, H5DatasetAccess access, Option<FieldWindow> window, ulong[] extent, FieldElement element) =>
        Op.Of(name: "hdf5.field-read").Catch(() => Fin.Succ(element == FieldElement.Double
                ? Narrowed(Slabbed<double>(source, access, window, extent))
                : Slabbed<float>(source, access, window, extent)))
            .ToValidation<Error>();

    static T[] Slabbed<T>(NativeDataset source, H5DatasetAccess access, Option<FieldWindow> window, ulong[] extent) where T : unmanaged {
        if (window.Case is FieldWindow declared) {
            HyperslabSelection selection = declared.Lower(extent);
            T[] narrow = new T[checked((int)selection.TotalElementCount)];
            source.Read<T>(access, narrow.AsSpan(), fileSelection: selection);
            return narrow;
        }
        long total = extent.Aggregate(1L, static (acc, axis) => acc * (long)axis);
        T[] whole = new T[checked((int)total)];
        int stations = (int)extent[0];
        long slab = total / stations;
        for (int station = 0; station < stations; station++) {          // Exemption: a per-station slab walk over a caller-owned destination span; no rail combinator carries a `Span` destination
            source.Read<T>(access, whole.AsSpan(checked((int)(station * slab)), checked((int)slab)),
                fileSelection: FieldWindow.Create(station, 1).Lower(extent));
        }
        return whole;
    }

    // An HDF5 container declares its component count and its spatial dimension and carries NO rank slot, so the
    // row INFERS from the pair the dataspace already fixes. A component count no row derives at that dimension
    // refuses here rather than landing a field whose rank a consumer then guesses — the inference is total
    // because `FieldRank` closes, and it is unambiguous because the three derivations diverge above `dim = 1`.
    static Validation<Error, FieldRank> Ranked(int dim, int components) =>
        FieldRank.Items.Find(row => row.Components(dim) == components)
            .ToValidation<Error>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(dim, components))));

    // Vectorized narrow: the per-element widen loop this replaces ran once per value over a whole corpus.
    internal static float[] Narrowed(double[] wide) {
        float[] narrow = new float[wide.Length];
        TensorPrimitives.ConvertToSingle(wide.AsSpan(), narrow.AsSpan());
        return narrow;
    }

    // HDF5 egress arm: the interop counterpart of `Hdf5Decode`, emitting the SAME station×component chunk model
    // as an HDF5 1.10 container h5py, ParaView, and HDFView open with no bridge (Shuffle rank 0 ahead of Deflate
    // rank 1 reads back as compression='gzip', shuffle=True). The native layout stays the residence-bearing
    // format: `Predicted` refuses typed because no container slot carries a residence law, and a Quantized
    // encode lands its ALREADY-CODED values with bits and bound as attributes — evidence, never a law a foreign
    // reader must enforce. Chunk writes walk the cursor, so out-of-order is unrepresentable rather than refused.
    public static Fin<ComputeArtifact> Hdf5Encode(FieldArtifact field, FieldCodecPolicy policy, HdfArchivePolicy archive, Stream sink, Instant at) =>
        policy.Residence.Switch(
            state: (field, policy, archive, sink, at),
            exact: static (s, _) => Emit(s.field, s.policy, s.archive, s.sink, s.at),
            quantized: static (s, _) => Emit(s.field, s.policy, s.archive, s.sink, s.at),
            predicted: static (s, _) => Fin.Fail<ComputeArtifact>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.None()))));

    // Declaration is a VALUE handed to the one `Runtime/archive#CHUNK_CURSOR` capsule: the slot, its attributes,
    // the writer open, and the release are that owner's, so this producer states only what its container carries.
    // The five attributes are the roster `tests/contracts` `[02.27]` freezes and the python `FieldContainer`
    // reader consumes, each crossing through the closed `ArchiveAttribute` vocabulary rather than an `object` bag.
    static Fin<ComputeArtifact> Emit(FieldArtifact field, FieldCodecPolicy policy, HdfArchivePolicy archive, Stream sink, Instant at) {
        ArchiveSlot<float> slot = new(field.Station.Length > 0 ? field.Station : "field", field.Grid);
        return ArchiveSession.Write(sink, archive, Seq<IArchiveSlot>(slot), Seq(
                ("format-key", (ArchiveAttribute)new ArchiveAttribute.Text(field.FormatKey)),
                ("residence", new ArchiveAttribute.Text(policy.Residence.Code == 0 ? "exact" : "quantized")),
                ("bits", new ArchiveAttribute.Whole(policy.Residence.QuantizationBits)),
                ("bound", new ArchiveAttribute.Real(policy.Residence.ErrorBound)),
                ("max-residual", new ArchiveAttribute.Real(field.MaxResidual))),
            session => IO.pure(session.Cursor(slot).Bind(cursor =>
                Range(0, field.ChunkCount).Fold(Fin.Succ(unit), (rail, ordinal) =>
                    rail.Bind(_ => cursor.Write(MemoryMarshal.Cast<byte, float>(field.Chunk(ordinal).Span).ToArray()))))))
        .Run()
        // The sink is the composition's pooled stream (the ARTIFACT_FRAMES grant); the emitted container re-reads
        // off it as one contiguous view for the content-key mint, never a second buffer.
        .Map(_ => {
            sink.Position = 0;
            byte[] emitted = new byte[checked((int)sink.Length)];
            sink.ReadExactly(emitted);
            return ComputeArtifact.Of("hdf5", emitted, at, [policy.Residence.QuantizationBits, policy.Residence.ErrorBound]);
        });
    }

    // --- [RESIDUAL_WALK]

    static Fin<FieldArtifact> ResidualEncode(FieldArtifact field, FieldResidence.Predicted residence, ResidualPredictor net) {
        float[] source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span).ToArray();
        int chunkElements = field.ChunkElements;
        int[] grid = field.Grid.Grid.Span.ToArray();
        (float scale, float step) = Quantization.Steps<float>(source, residence.Bits);
        Fin<(float[] Residual, float[] Reconstructed, double Worst)> initial = Fin.Succ((new float[source.Length], new float[source.Length], 0d));
        return Range(0, field.ChunkCount)
            .Fold(initial, (rail, chunk) => rail.Bind(state => EncodeChunk(source, state, grid, chunk, chunkElements, net, residence, scale, step)))
            .Map(state => field with { Chunks = MemoryMarshal.AsBytes(state.Residual.AsSpan()).ToArray(), MaxResidual = state.Worst });
    }

    static Fin<(float[] Residual, float[] Reconstructed, double Worst)> EncodeChunk(
        float[] source,
        (float[] Residual, float[] Reconstructed, double Worst) state,
        int[] grid, int chunk, int chunkElements,
        ResidualPredictor net, FieldResidence.Predicted residence, float scale, float step) {
        int start = chunk * chunkElements;
        int length = Math.Min(chunkElements, source.Length - start);
        return length <= 0
            ? Fin.Succ(state)
            : net.Predict(GatherNeighbours(state.Reconstructed, grid, chunk, chunkElements, net.NeighbourStencil), length)
                .Map(prediction => {
                    double bounded = CodeChunk(source, prediction, state.Residual, state.Reconstructed, start, length, step, scale);
                    double achieved = bounded > residence.Bound
                        ? CodeChunk(source, prediction, state.Residual, state.Reconstructed, start, length, 0f, scale)
                        : bounded;
                    return (state.Residual, state.Reconstructed, Math.Max(state.Worst, achieved));
                });
    }

    static double CodeChunk(float[] source, float[] prediction, float[] residual, float[] reconstructed, int start, int length, float step, float scale) =>
        Range(0, length).Fold(0d, (worst, index) => {
            float predicted = index < prediction.Length ? prediction[index] : 0f;
            float coded = Quantization.Code(source[start + index] - predicted, step);
            residual[start + index] = coded;
            reconstructed[start + index] = predicted + coded;
            return Math.Max(worst, Quantization.Residual(source[start + index], predicted + coded, scale));
        });

    public static Fin<FieldArtifact> Reconstruct(FieldArtifact residualField, ResidualPredictor net) {
        float[] stored = MemoryMarshal.Cast<byte, float>(residualField.Chunks.Span).ToArray();
        int chunkElements = residualField.ChunkElements;
        int[] grid = residualField.Grid.Grid.Span.ToArray();
        return Range(0, residualField.ChunkCount)
            .Fold(Fin.Succ(new float[stored.Length]), (rail, chunk) => rail.Bind(values => ReconstructChunk(stored, values, grid, chunk, chunkElements, net)))
            .Map(values => residualField with { Chunks = MemoryMarshal.AsBytes(values.AsSpan()).ToArray() });
    }

    static Fin<float[]> ReconstructChunk(float[] stored, float[] reconstructed, int[] grid, int chunk, int chunkElements, ResidualPredictor net) {
        int start = chunk * chunkElements;
        int length = Math.Min(chunkElements, stored.Length - start);
        return length <= 0
            ? Fin.Succ(reconstructed)
            : net.Predict(GatherNeighbours(reconstructed, grid, chunk, chunkElements, net.NeighbourStencil), length)
                .Map(prediction => Range(0, length).Fold(reconstructed, (values, index) => {
                    values[start + index] = (index < prediction.Length ? prediction[index] : 0f) + stored[start + index];
                    return values;
                }));
    }

    // Grid-coordinate face neighbours prevent 1-D boundary leakage. CONSTRAINT: a missing neighbour contributes a
    // ZERO chunk the predictor cannot distinguish from measured zero field — the stencil carries no presence mask,
    // so a corpus whose boundary values are genuinely near zero and one whose boundary is absent predict alike.
    static float[] GatherNeighbours(ReadOnlySpan<float> source, int[] grid, int ordinal, int chunkElements, int radius) {
        int rank = grid.Length;
        Span<int> coord = stackalloc int[rank];
        int remainder = ordinal;
        for (int axis = rank - 1; axis >= 0; axis--) { coord[axis] = remainder % grid[axis]; remainder /= grid[axis]; }
        float[] stencil = new float[(1 + (2 * rank)) * chunkElements];
        CopyChunk(source, ordinal, chunkElements, stencil, 0);
        int slot = 1;
        for (int axis = 0; axis < rank; axis++) {                       // Exemption: a measured gather over pre-sized arenas addressed by grid coordinate; the rail resumes at Predict
            int minus = coord[axis] - radius, plus = coord[axis] + radius;
            if (minus >= 0) { CopyChunk(source, OrdinalAt(grid, coord, axis, minus), chunkElements, stencil, slot * chunkElements); }
            slot++;
            if (plus < grid[axis]) { CopyChunk(source, OrdinalAt(grid, coord, axis, plus), chunkElements, stencil, slot * chunkElements); }
            slot++;
        }
        return stencil;
    }

    static int OrdinalAt(int[] grid, ReadOnlySpan<int> coord, int axis, int value) {
        int ordinal = 0;
        for (int a = 0; a < grid.Length; a++) { ordinal = (ordinal * grid[a]) + (a == axis ? value : coord[a]); }
        return ordinal;
    }

    static void CopyChunk(ReadOnlySpan<float> source, int ordinal, int chunkElements, float[] destination, int offset) {
        int start = ordinal * chunkElements;
        if ((uint)start >= (uint)source.Length) { return; }
        int length = Math.Min(chunkElements, source.Length - start);
        source.Slice(start, length).CopyTo(destination.AsSpan(offset, length));
    }

    static FieldArtifact Quantize(FieldArtifact field, int bits) {
        float[] source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span).ToArray();
        (float scale, float step) = Quantization.Steps<float>(source, bits);
        float[] quantized = new float[source.Length];
        Quantization.Code<float>(source, quantized, step);
        double worst = Quantization.Worst<float>(source, quantized, scale);
        return field with { Chunks = MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray(), MaxResidual = worst };
    }

    // --- [ZERO_COPY_VIEW]
    // The multi-segment view the `Runtime/channels#ARTIFACT_FRAMES` frame law drains and the pooled content-key
    // mint folds incrementally: one segment per chunk, no flatten, so a chunk crosses solver→store→viz with no
    // managed copy and a `ToArray` at the seam is the named defect.
    public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field) {
        int chunkBytes = field.ChunkElements * sizeof(float);
        if (chunkBytes <= 0 || field.ChunkCount <= 1) { return new(field.Chunks); }
        ChunkSegment? head = null, tail = null;
        for (int chunk = 0; chunk < field.ChunkCount; chunk++) {        // Exemption: a linked-segment build whose tail is the accumulator; `Cons`-then-reverse cannot express a forward RunningIndex
            ReadOnlyMemory<byte> slice = field.Chunk(chunk);
            if (slice.IsEmpty) { break; }
            tail = tail is null ? head = new ChunkSegment(slice, 0) : tail.Append(slice);
        }
        return head is null ? new(field.Chunks) : new ReadOnlySequence<byte>(head, 0, tail!, tail!.Memory.Length);
    }

    // The one legitimate shell on this page: `ReadOnlySequenceSegment<byte>` is abstract, so the subclass exists
    // solely to reach its protected setters.
    sealed class ChunkSegment : ReadOnlySequenceSegment<byte> {
        public ChunkSegment(ReadOnlyMemory<byte> memory, long runningIndex) {
            Memory = memory;
            RunningIndex = runningIndex;
        }

        public ChunkSegment Append(ReadOnlyMemory<byte> memory) {
            ChunkSegment next = new(memory, RunningIndex + Memory.Length);
            Next = next;
            return next;
        }
    }
}
```

## [03]-[SCIENTIFIC_INGEST]

- Owner: `InterchangeIo` the scientific-data ingest surface dispatching the chunked field decode, the point-scan ingest, and the waveform-corpus read onto the reader that owns each — the geometry and IFC import arms owned by `Rasm.Bim`; `FieldContainerKind` the `[SmartEnum<string>]` container vocabulary whose rows carry their own reader arm, so a new container is a ROW and the dispatch body never grows a literal; `PointScan` the point-cloud carrier over E57/LAS/LAZ/PTS; `WaveformWindow`/`WaveformCorpus` the frame/hop-declared multi-channel waveform interchange carrier over long SHM records and fitted reference banks — the `Stats/signal` corpus seam, which stores nothing.
- Cases: `FieldContainerKind.Native` (`field-chunk`, this codec's own residence-bearing 64-byte-header layout) and `FieldContainerKind.Hdf5` (`hdf5`, the container the archive capsule opens); `WaveformWindow` carries `Strided` (hop at or above frame — ONE strided hyperslab) and `Overlapped` (hop below frame — one slab per frame) as a derived case the read arm reads instead of comparing two ints at the site.
- Entry: `public static Fin<FieldArtifact> ImportField(FieldContainerKind container, string formatKey, ReadOnlyMemory<byte> bytes, IClock clock, Option<ResidualPredictor> predictor = default, string dataset = "/field", Option<FieldWindow> window = default)` reads and reconstructs a self-describing chunked field through the container row's own reader; `public static IO<Fin<PointScan>> ImportPoints(string formatKey, string codecKey, ReadOnlySequence<byte> bytes, ScanSpec spec, ProjectionContext frame, Func<ScanFact, IO<Unit>> sink)` reads a point-cloud scan by composing the Persistence `Ingest/pointcloud#SCAN_SOURCE` owner — one `ScanOp.Ingest` landing the capture bytes, one `ScanOp.Window` over the regions that ingest yielded — and folds the batches into `PointScan`, `pts` alone keeping its unadmitted-reader refusal; `public static Fin<WaveformCorpus> ImportWaveforms(string formatKey, HdfHandle handle, string dataset, WaveformWindow window, Instant at)` reads a `[samples, channels]` waveform corpus over a `Runtime/archive#HDF_ARCHIVE` handle under the declared frame/hop selection, the `sample-rate` attribute mandatory.
- Auto: the container key picks a READER ROW, never a format test — the codec's own layout and HDF5 are two carriers of ONE chunk model, so the `FieldArtifact` that lands is the same shape either way, and the HDF5 arm brackets its handle through `HdfArchive.Session` so the container closes on the fault arm exactly as on the success arm. The waveform arm ACCUMULATES its three container constraints — a rank-2 dataspace, a frame that fits the record, and a present `sample-rate` — so a corpus violating two reports two.
- Receipt: the `StreamSegment` receipt carries the imported artifact id and the decoded bytes; a point-scan ingest rides the Persistence reader's own scan receipts, never a second Compute row.
- Packages: PureHDF, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm.Persistence (project — `Ingest/pointcloud#SCAN_SOURCE` `ScanSource.Run`/`ScanSpec`/`ScanOp`/`ScanYield`/`ScanBatch`/`ScanHeader`/`ScanRegion`/`ScanFact`, `Element/graph#STORE_RAIL` `ProjectionContext`), BCL inbox
- Growth: a new chunked-field container is one `FieldContainerKind` row carrying its reader arm — the `field-chunk` and container vocabulary is THIS page's, the Bim format axis carrying its own exchange formats and never these (its `[ROW_PROMOTION]` carve names chunked-field codecs Compute-owned, consumed at the seam); a new point-scan format is one `ScanFormat` row at the Persistence reader and one `point-cloud` codec row on the Bim format axis, never a decode arm here; a new waveform-corpus source is one `FormatKey` value on `WaveformCorpus` and a new windowing posture one `WaveformWindow` column, never a second carrier or a signal-local reader; zero new surface.
- Boundary: `PointScan` carries the `point-cloud` codec discriminant the Bim format axis names and COMPOSES the admitted E57/LAS/LAZ reader — `Rasm.Persistence` `Ingest/pointcloud#SCAN_SOURCE` owns that decode and the durable residence beneath it, so this arm mints no codec and narrows the double-position batches into the float carrier at one boundary, ASCII `pts` alone faulting `point-catalogue-pending` for want of a reader; the geometry mesh decode and IFC semantic ingest are the `Rasm.Bim` import rail, never re-derived — an `ImportGeometry`/`ImportIfc` arm here the deleted form; signal corpora cross ONLY through `ImportWaveforms` — `Stats/signal` composes `WaveformCorpus` and stores nothing, the recorded estimator (Arrow) and monitor (receipt-stream) negatives scoping the arm to genuine corpora, and a signal-local `H5File` open is the second surface the one-owner ruling rejects; frame and hop are ADMITTED on the window rather than guarded at the read, so `Frame <= 0` and `Hop <= 0` are values no caller can construct and only the cross-shape census against the container (frame past the record) survives as an admission row.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The container vocabulary as ROWS with their own reader arms: the literal `switch` this replaces called a new
// container "one dispatch row" in its Growth line while spelling a bare string, so a container the corpus never
// named elsewhere could only be found by reading the body.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class FieldContainerKind {
    public static readonly FieldContainerKind Native = new("field-chunk", InterchangeIo.ReadNative);
    public static readonly FieldContainerKind Hdf5 = new("hdf5", InterchangeIo.ReadHdf5);

    [UseDelegateFromConstructor]
    internal partial Fin<FieldArtifact> Read(
        string formatKey, ReadOnlyMemory<byte> bytes, Instant at,
        Option<ResidualPredictor> predictor, string dataset, Option<FieldWindow> window);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Frame/hop IS the hyperslab law, admitted rather than guarded: the frame length maps onto the block extent and
// the hop onto the stride, so a non-overlapping walk reads as ONE strided selection and an overlapping walk reads
// per-frame slabs, because an HDF5 hyperslab admits no stride below its block. Both forms are DECLARED
// selections, so the unqualified whole-record read of a screening-scale SHM corpus stays unspellable — a
// whole-record consumer (a fitted filter bank, a reference spectrum) declares frame == samples.
[ComplexValueObject]
public sealed partial class WaveformWindow {
    public int Frame { get; }
    public int Hop { get; }

    public bool Strided => Hop >= Frame;

    public int Frames(long samples) => (int)((samples - Frame) / Hop) + 1;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int frame, ref int hop) =>
        validationError = Seq(
                frame > 0 ? None : Some($"<waveform-frame:{frame}>"),
                hop > 0 ? None : Some($"<waveform-hop:{hop}>"))
            .Somes()
            .Match(Empty: () => null, More: (head, tail) => new ValidationError(string.Join(';', head.Cons(tail))));
}

public sealed record PointScan(
    string FormatKey,
    ReadOnlyMemory<float> Positions,
    Option<ReadOnlyMemory<float>> Colors,
    Option<ReadOnlyMemory<float>> Intensity,
    long PointCount,
    Instant At);

// Multi-channel waveform interchange carrier: long SHM records and python-fitted reference banks cross as ONE
// container class on the interchange surface — `Stats/signal` composes it and stores nothing, closing the
// estimator (Arrow) and monitor (receipt-stream) storage declines without re-opening them. Frames are frame-major
// `[FrameCount, Frame, Channels]` row-major float, the exact walk a Welch/PSD or filter-bank fold consumes.
public sealed record WaveformCorpus(
    string FormatKey,
    string Station,
    int Channels,
    long Samples,
    double SampleRate,
    WaveformWindow Window,
    int FrameCount,
    ReadOnlyMemory<float> Frames,
    Instant At);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class InterchangeIo {
    public static Fin<FieldArtifact> ImportField(
        FieldContainerKind container, string formatKey, ReadOnlyMemory<byte> bytes, IClock clock,
        Option<ResidualPredictor> predictor = default, string dataset = "/field", Option<FieldWindow> window = default) =>
        container.Read(formatKey, bytes, clock.GetCurrentInstant(), predictor, dataset, window);

    internal static Fin<FieldArtifact> ReadNative(
        string formatKey, ReadOnlyMemory<byte> bytes, Instant at,
        Option<ResidualPredictor> predictor, string dataset, Option<FieldWindow> window) =>
        FieldCodec.FieldDecode(formatKey, bytes, at, predictor);

    // The payload arm keeps the bytes-in seam a companion or store lane hands over; a parallel or store-resident
    // corpus opens its own `HdfSource.Path`/`Mapped` handle through the archive capsule and calls
    // `FieldCodec.Hdf5Decode` directly. Acquisition, read, and release ride the capsule's own bracket, so the
    // container closes on the fault arm exactly as on the success arm.
    internal static Fin<FieldArtifact> ReadHdf5(
        string formatKey, ReadOnlyMemory<byte> bytes, Instant at,
        Option<ResidualPredictor> predictor, string dataset, Option<FieldWindow> window) =>
        HdfArchive.Session(new HdfSource.Payload(bytes), HdfArchivePolicy.Interchange,
            handle => IO.pure(FieldCodec.Hdf5Decode(formatKey, handle, dataset, at, window))).Run();

    // Point-scan decode COMPOSES the admitted reader rather than re-deriving one: `Rasm.Persistence`
    // `Ingest/pointcloud#SCAN_SOURCE` owns the E57/LAS/LAZ decode and the durable residence, so capture bytes
    // land once under `ArtifactKind.Scan` and the windowed read streams batches back out of that blob — the
    // ingest yield's own region cells ARE the window, so a full read costs no second census. Only ASCII `pts`
    // has no admitted reader and keeps its refusal.
    public static IO<Fin<PointScan>> ImportPoints(
        string formatKey, string codecKey, ReadOnlySequence<byte> bytes, ScanSpec spec,
        ProjectionContext frame, Func<ScanFact, IO<Unit>> sink) =>
        (codecKey, formatKey) switch {
            (not "point-cloud", _) => IO.pure(Fin.Fail<PointScan>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Key(formatKey))))),
            (_, "pts") => IO.pure(Fin.Fail<PointScan>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Key(formatKey))))),
            _ => Scanned(spec, bytes, frame, sink),
        };

    // Waveform-corpus ingest arm over the [HDF_ARCHIVE] handle — the signal lane's one storage seam: no
    // signal-local `H5File` open exists, the handle arrives from the one archive owner, one `NativeFile` per job.
    // The dataset is `[samples, channels]` little-endian float with a `sample-rate` attribute; the element gate is
    // the shared `FieldElement` row. The three container constraints ACCUMULATE — a corpus with a rank-3
    // dataspace AND no sample rate reports both, where the abort ladder reported the first and hid the second.
    public static Fin<WaveformCorpus> ImportWaveforms(string formatKey, HdfHandle handle, string dataset, WaveformWindow window, Instant at) =>
        handle.Dataset(dataset).ToValidation<Error>().Bind(source => {
            ulong[] extent = source.Space.Dimensions;
            long samples = extent.Length == 2 ? (long)extent[0] : 0L;
            return (extent.Length == 2 && extent.All(static axis => axis is not 0UL and <= int.MaxValue)
                        ? Success<Error, long>(samples)
                        : Fail<Error, long>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Extent([.. extent.Select(static axis => checked((long)axis))])))),
                    window.Frame <= samples
                        ? Success<Error, Unit>(unit)
                        : Fail<Error, Unit>(new ComputeFault.PayloadOverBounds($"<hdf5-waveform-window:{window.Frame}:{samples}>")),
                    source.AttributeExists("sample-rate")
                        ? Success<Error, double>(source.Attribute("sample-rate").Read<double>())
                        : Fail<Error, double>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(dataset)))),
                    FieldElement.Of(source.Type))
                .Apply((count, _, rate, element) => (Samples: count, Rate: rate, Element: element))
                .Bind(admitted => {
                    int channels = (int)extent[1];
                    int frames = window.Frames(admitted.Samples);
                    return Op.Of(name: "hdf5.waveform-read").Catch(() => Fin.Succ(admitted.Element == FieldElement.Double
                            ? FieldCodec.Narrowed(Windowed<double>(source, handle.Access, window, frames, channels))
                            : Windowed<float>(source, handle.Access, window, frames, channels)))
                        .ToValidation<Error>()
                        .Map(values => new WaveformCorpus(formatKey, dataset, channels, admitted.Samples, admitted.Rate, window, frames, values, at));
                });
        }).ToFin();

    // The window's own `Strided` case decides the read: hop at or above frame is ONE strided hyperslab, hop below
    // frame walks per-frame slabs — a stride below its block is the selection form HDF5 refuses, so the overlap
    // case pays one read per frame by construction.
    static T[] Windowed<T>(NativeDataset source, H5DatasetAccess access, WaveformWindow window, int frames, int channels) where T : unmanaged {
        long frameValues = (long)window.Frame * channels;
        T[] destination = new T[checked((int)(frames * frameValues))];
        if (window.Strided) {
            source.Read<T>(access, destination.AsSpan(), fileSelection: new HyperslabSelection(rank: 2,
                starts: [0UL, 0UL],
                strides: [(ulong)window.Hop, (ulong)channels],
                counts: [(ulong)frames, 1UL],
                blocks: [(ulong)window.Frame, (ulong)channels]));
            return destination;
        }
        for (int frame = 0; frame < frames; frame++) {                  // Exemption: one read per overlapping frame into a caller-owned destination span; no rail combinator carries a `Span` destination
            source.Read<T>(access, destination.AsSpan(checked((int)(frame * frameValues)), checked((int)frameValues)),
                fileSelection: new HyperslabSelection(rank: 2,
                    starts: [(ulong)((long)frame * window.Hop), 0UL],
                    blocks: [(ulong)window.Frame, (ulong)channels]));
        }
        return destination;
    }

    static IO<Fin<PointScan>> Scanned(ScanSpec spec, ReadOnlySequence<byte> bytes, ProjectionContext frame, Func<ScanFact, IO<Unit>> sink) =>
        from landed in ScanSource.Run(new ScanOp.Ingest(spec, bytes), frame, sink)
        from read in landed.Match(
            Succ: y => y is ScanYield.Landed done
                ? ScanSource.Run(new ScanOp.Window(spec, done.Scan, done.Regions.Map(static region => region.Cell)), frame, sink)
                    .Map(points => Folded(spec, done.Header, points))
                : IO.pure(Fin.Fail<PointScan>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Payload)))),
            Fail: static faults => IO.pure(Fin.Fail<PointScan>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(faults.Count, 0L))))))
        select read;

    static Fin<PointScan> Folded(ScanSpec spec, ScanHeader header, Validation<Error, ScanYield> points) => points.Match(
        Succ: y => y is ScanYield.Points batches
            ? Fin<PointScan>.Succ(Carried(spec, header, batches.Batches))
            : Fin<PointScan>.Fail(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Payload))),
        Fail: static faults => Fin<PointScan>.Fail(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(faults.Count, 0L)))));

    // Batch lanes carry double positions with an optional 16-bit colour triple; `PointScan` is the float compute
    // carrier, so the fold narrows ONCE at this boundary through the vectorized converter. Intensity stays absent
    // because the residence currency carries no intensity lane, and a zero-filled one would read as measured.
    static PointScan Carried(ScanSpec spec, ScanHeader header, Seq<ScanBatch> batches) {
        int points = batches.Sum(static batch => batch.Count);
        bool coloured = batches.ForAll(static batch => batch.Colors.IsSome);
        float[] positions = new float[points * 3];
        float[] colors = coloured ? new float[points * 3] : [];
        int cursor = 0;
        foreach (ScanBatch batch in batches) {                          // Exemption: a cursor-advancing gather across batches into two pre-sized planes; the rail resumes at the returned carrier
            int lanes = batch.Count * 3;
            TensorPrimitives.ConvertToSingle(batch.Positions.Span[..lanes], positions.AsSpan(cursor, lanes));
            // 16-bit channels normalize to the unit float lane the compute carrier reads; an unnormalized
            // widen would put colour on a different scale than every other float channel here.
            batch.Colors.Iter(rgb => {
                ReadOnlySpan<ushort> channels = rgb.Span;
                for (int lane = 0; lane < lanes; lane++) { colors[cursor + lane] = channels[lane] / (float)ushort.MaxValue; }
            });
            cursor += lanes;
        }
        return new PointScan(spec.Format.Key, positions, coloured ? Some((ReadOnlyMemory<float>)colors) : None, None, header.Points, header.At);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
