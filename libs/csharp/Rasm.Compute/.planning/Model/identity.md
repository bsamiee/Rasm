# [COMPUTE_IDENTITY]

Rasm.Compute model identity: ONNX model identity and provenance — the checksum-derived `ModelIdentity` record carrying the nested `Slot` schema rows, the `Provenance` producer/domain/graph triple, and the `CustomMetadata`/`Initializers` self-description channels; the `ModelSource` five-case acquisition union whose `Acquire` fold collapses every source to one byte admission and whose `Origin` projects the receipt source string; and the one `ModelFingerprint` ordinal-keyvalue projection this owner homes and the execution-provider axis composes. The page owns the `ModelSource`/`ModelIdentity`/`ModelFingerprint` vocabulary, the `Snapshot` schema admission, the `Acquire`/`Origin` source fold, the `Accepts`/`Initializer` admission gates, and the `ModelLoad` receipt mint; identity derives from the model bytes through the kernel seed-zero `XxHash128` content-hash entry (`Rasm.Domain.ContentHash.Of`, the workspace's ONE hasher composed — never a second hasher; the `ModelFingerprint` options projection rides `System.IO.Hashing` `XxHash3`), the slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata, the receipt rides the `ComputeReceipt` rail, and `NodaTime` `Instant`, the AppHost receipt spine, and the Persistence `ArtifactIndexRow` arrive settled. The `ModelIdentity`/`ModelFingerprint`/`Slot` shapes cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/inference#INFERENCE_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, and the `Checksum` is the deterministic cache and result-key seed `Model/inference#RESULT_CACHE` consumes.

## [01]-[INDEX]

- [01]-[MODEL_IDENTITY]: checksum identity; five-case acquisition union with the byte-resolution fold; schema snapshot with provenance; admission over input slots and overridable initializers; custom-metadata self-description; shared ordinal-keyvalue fingerprint; `ModelLoad` receipt mint.

## [02]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` schema rows, the `Provenance` producer/domain/graph triple, and the `CustomMetadata`/`Initializers` channels; `ModelSource` `[Union]` five acquisition cases whose `Acquire` fold resolves each to bytes through the injected `SourceResolver` ports and whose `Origin` projects the receipt source string; `ModelFingerprint` the one ordinal-keyvalue projection homed here and composed by `ExecutionProvider.ResultKey`.
- Cases: `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`, `Buffer`.
- Entry: `public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — pure value; identity derives from the bytes, never from the caller.
- Auto: `Snapshot` stamps the kernel `ContentHash.Of` seed-zero identity checksum (the ONE federation hasher composed, never a per-call-site `System.IO.Hashing` invocation), the graph version, the input/output/overridable-initializer slot rows, the `ModelMetadata` provenance triple, and the `CustomMetadataMap` self-description in one pass; `Acquire` folds the five source cases to one `Fin<ReadOnlyMemory<byte>>` admission — `LocalFile`/`EmbeddedResource`/`Buffer` self-resolve through the file, manifest-resource, and in-memory channels while `PersistenceBlob`/`RemoteFetch` defer to the `SourceResolver` blob and remote ports — so a new acquisition route is one case, never a parallel loader; `Accepts` runs once at load and rejects any input binding whose name, dtype, or fixed dimensions miss the slot it targets, naming every rejected binding with its offered shape in the fault; `Initializer` admits a deployment-constant `OrtValue` against its overridable-initializer slot only when dtype AND element count conform, faulting with the dtype/count evidence; `ModelFingerprint.Of` is the single ordinal-keyvalue `XxHash3` projection — homed here and composed by `ExecutionProvider.ResultKey` through `OptionsFor(precision)`, so the keyvalue body is declared once, while the model's `CustomMetadata` is captured as self-description without minting a second identity token; per-call re-validation is the deleted form because admission settles the contract once.
- Receipt: the `ModelLoad` receipt — the `Runtime/receipts#RECEIPT_UNION` `ModelLoad(checksum, source, ep, version)` shape — is minted by `LoadReceipt` from this owner's `Key`, `Source.Origin`, and snapshotted `GraphVersion` plus the loader's `ExecutionProvider`, correlation, and elapsed; emission rides the sink port at the composition edge.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new acquisition route is one `ModelSource` case plus its `Acquire` arm and `Origin` projection; a new deployment-constant binding is one `Initializer` admission against its overridable-initializer slot; a new self-description axis is one `Provenance` field read from `ModelMetadata`; zero new surface.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum` — path-keyed or filename-keyed model identity is the deleted form, and the `Checksum` composes the kernel seed-zero `XxHash128` content-hash entry (`ContentHash.Of(bytes)`) — the workspace's ONE hasher, shared with the geometry `GeometryHash`, the seam `ContentAddress`, and the `Rasm.Persistence` `ArtifactIndexRow`/`ModelResultIndex` spine — so the model digest is content-addressed through the one federation entry, never a per-call-site `System.IO.Hashing.XxHash128.HashToUInt128(bytes)` call (the named second-hasher defect the seam, the kernel naming/reconciliation owners, and the Persistence index all reject). The `Acquire` fold is the single byte-admission path: it carries the boundary-capsule statement forms for the file and manifest-resource reads (the `boundaries.md` CAPSULE_OWNER exemption), brackets every I/O fault into `ComputeFault.ModelRejected`, and faults a missing file or absent resource explicitly — a source case that cannot produce its bytes is the rejected inert-label form, which is why each of the five cases owns an `Acquire` arm. The `SourceResolver` blob and remote ports are injected so this owner embeds neither a blob store nor an `HttpClient` (parameterized ingress), and `SourceResolver.Local` is the local-only default whose blob/remote arms fault; the `PersistenceBlob` case resolves through the Persistence blob lane by its `ArtifactIndexRow`, never by re-reading a file path. `Slot.FreeDims` rows drive the free-dimension overrides at session build, with symbolic-dim values arriving from the geometry-encoding rows as settled vocabulary, and `Accepts` treats a slot dimension of `-1` as a wildcard so a free axis admits any extent while a fixed axis rejects a mismatched one. The `CustomMetadata` channel is the artifact's self-description read once at snapshot; it rides the identity for downstream inspection but mints NO second identity token — two models with identical bytes are one identity, and differing metadata already changes the bytes and thus the 128-bit checksum, so a metadata-only fingerprint would discriminate nothing. The one ordinal-keyvalue projection is the static `ModelFingerprint.Of`, homed here and composed by the `ExecutionProvider.ResultKey` `ModelFingerprint.Of(OptionsFor(precision))` fold so the keyvalue body is declared exactly once — a re-derived `XxHash3.HashToUInt64(UTF8(string.Join(';', ...)))` body in either owner is the named defect. The `Initializers` slot table carries the `OverridableInitializerMetadata` deployment constants admitted through `AddInitializer(string, OrtValue)` at session build — never per-run inputs — and schema admission happens exactly once at load. The `ModelLoad` receipt is minted here and emitted at the edge: it carries the model's `GraphVersion` (the `ModelMetadata.Version` snapshotted once at load), because the ONNX operator-set version is not exposed by the managed `ModelMetadata`/`InferenceSession` surface and a managed `Google.Protobuf` field scan to recover it is the form the protobuf rail rejects — so the load fact rides the derivable graph version, never an underivable opset threaded as a dead loader parameter.

```csharp signature
public readonly record struct SourceResolver(
    Func<ArtifactIndexRow, Fin<ReadOnlyMemory<byte>>> Blob,
    Func<string, Fin<ReadOnlyMemory<byte>>> Remote) {
    public static readonly SourceResolver Local = new(
        static _ => Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected("<no-blob-resolver>")),
        static _ => Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected("<no-remote-resolver>")));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModelSource {
    private ModelSource() { }

    public sealed record LocalFile(string Path) : ModelSource;

    public sealed record EmbeddedResource(Assembly Assembly, string Name) : ModelSource;

    public sealed record PersistenceBlob(ArtifactIndexRow Row) : ModelSource;

    public sealed record RemoteFetch(string ArtifactId) : ModelSource;

    public sealed record Buffer(ReadOnlyMemory<byte> Bytes) : ModelSource;

    public string Origin => Switch(
        localFile:        static f => $"file:{f.Path}",
        embeddedResource: static e => $"resource:{e.Assembly.GetName().Name}/{e.Name}",
        persistenceBlob:  static _ => "blob",
        remoteFetch:      static r => $"remote:{r.ArtifactId}",
        buffer:           static b => $"buffer:{b.Bytes.Length}");

    public Fin<ReadOnlyMemory<byte>> Acquire(SourceResolver resolver) {
        try {
            return Switch(
                localFile: static f => File.Exists(f.Path)
                    ? Fin.Succ((ReadOnlyMemory<byte>)File.ReadAllBytes(f.Path))
                    : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected($"<model-source-missing:file:{f.Path}>")),
                embeddedResource: static e => e.Assembly.GetManifestResourceStream(e.Name) is Stream stream
                    ? Read(stream)
                    : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected($"<model-source-missing:resource:{e.Name}>")),
                persistenceBlob: b => resolver.Blob(b.Row),
                remoteFetch:     r => resolver.Remote(r.ArtifactId),
                buffer:          static b => Fin.Succ(b.Bytes));
        }
        catch (Exception error) {
            return Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected($"<model-source-error:{Origin}:{error.Message}>"));
        }
    }

    static Fin<ReadOnlyMemory<byte>> Read(Stream stream) {
        using (stream) {
            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes);
            return Fin.Succ((ReadOnlyMemory<byte>)bytes);
        }
    }
}

public static class ModelFingerprint {
    public static ulong Of(IEnumerable<KeyValuePair<string, string>> rows) =>
        XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(string.Join(';',
            rows.OrderBy(static row => row.Key, StringComparer.Ordinal).Select(static row => $"{row.Key}={row.Value}"))));
}

public sealed record ModelIdentity(
    UInt128 Checksum,
    long GraphVersion,
    Seq<ModelIdentity.Slot> Inputs,
    Seq<ModelIdentity.Slot> Outputs,
    Seq<ModelIdentity.Slot> Initializers,
    FrozenDictionary<string, string> CustomMetadata,
    ModelIdentity.Provenance Provenance,
    ModelSource Source,
    Instant AcquiredAt) {
    public sealed record Slot(string Name, TensorElementType Dtype, Seq<int> Dims, Seq<string> FreeDims);

    public sealed record Provenance(string Producer, string Domain, string GraphName);

    public string Key => $"{Checksum:x32}";

    public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at) {
        var meta = session.ModelMetadata;
        return new(
            ContentHash.Of(bytes),
            meta.Version,
            Slots(session.InputMetadata),
            Slots(session.OutputMetadata),
            Slots(session.OverridableInitializerMetadata),
            meta.CustomMetadataMap.ToFrozenDictionary(StringComparer.Ordinal),
            new Provenance(meta.ProducerName, meta.Domain, meta.GraphName),
            source,
            at);
    }

    public Fin<Unit> Accepts(Seq<(string Name, TensorElementType Dtype, Seq<long> Shape)> binding) {
        var rejected = binding.Filter(b => !Inputs.Exists(slot => Conforms(slot, b)));
        return rejected.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected(
                $"{Key}:reject:{string.Join(',', rejected.Map(static b => $"{b.Name}[{string.Join('x', b.Shape)}]"))}"));
    }

    public Fin<(string Name, OrtValue Value)> Initializer(string name, OrtValue value) {
        var info = value.GetTensorTypeAndShape();
        return Initializers.Find(slot => StringComparer.Ordinal.Equals(slot.Name, name)).Case is Slot slot
            && slot.Dtype == info.ElementDataType
            && Sized(slot, info.ElementCount)
                ? Fin.Succ((name, value))
                : Fin.Fail<(string, OrtValue)>(new ComputeFault.ModelRejected(
                    $"{Key}:initializer:{name}:dtype={info.ElementDataType}:n={info.ElementCount}"));
    }

    public ComputeReceipt.ModelLoad LoadReceipt(ExecutionProvider ep, CorrelationId correlation, Duration elapsed) =>
        new(Key, Source.Origin, ep, GraphVersion) {
            Correlation = correlation,
            Lane = WorkLane.Background,
            Substrate = Substrate.Onnx,
            AllocationClass = AllocationClass.NativeOrt,
            Elapsed = elapsed,
        };

    static bool Conforms(Slot slot, (string Name, TensorElementType Dtype, Seq<long> Shape) binding) =>
        StringComparer.Ordinal.Equals(slot.Name, binding.Name)
        && slot.Dtype == binding.Dtype
        && slot.Dims.Count == binding.Shape.Count
        && slot.Dims.Zip(binding.Shape).ForAll(static pair => pair.Item1 < 0 || pair.Item1 == pair.Item2);

    static bool Sized(Slot slot, long elementCount) =>
        slot.Dims.Exists(static dim => dim < 0) || slot.Dims.Fold(1L, static (acc, dim) => acc * dim) == elementCount;

    static Seq<Slot> Slots(IReadOnlyDictionary<string, NodeMetadata> nodes) =>
        toSeq(nodes).Map(static pair => new Slot(
            pair.Key,
            pair.Value.ElementDataType,
            toSeq(pair.Value.Dimensions),
            toSeq(pair.Value.SymbolicDimensions)));
}
```
