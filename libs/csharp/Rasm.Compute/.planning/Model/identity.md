# [COMPUTE_IDENTITY]

Rasm.Compute model identity owns ONNX provenance and the content address every downstream cache key, receipt, and claim derives from. `ModelIdentity` carries the checksum, the nested `Slot` schema rows, the `Provenance` producer/domain/graph triple, and the `CustomMetadata`/`Initializers` self-description channels; `ModelSource` folds five acquisition cases to one byte admission through `Acquire` and projects the receipt source string through `Origin`; `ModelFingerprint` is the one ordinal-keyvalue projection homed here and composed by the execution-provider axis. Admission settles the input and initializer contract once — per-call re-validation is the rejected form.

Identity derives from the model bytes through the kernel seed-zero `XxHash128` entry `Rasm.Domain.ContentHash.Of`, the workspace's one hasher — shared with the geometry `GeometryHash`, the seam `ContentAddress`, and the Persistence `ArtifactIndexRow`/`ModelResultIndex` spine — while `ModelFingerprint` rides `System.IO.Hashing` `XxHash3`; slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata; `ModelLoad` rides the `ComputeReceipt` rail. `NodaTime` `Instant`, the AppHost receipt spine, and the Persistence `ArtifactIndexRow` arrive settled. `ModelIdentity`/`ModelFingerprint`/`Slot` cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/inference#INFERENCE_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, and `Checksum` is the deterministic cache and result-key seed `Model/inference#RESULT_CACHE` consumes.

## [01]-[INDEX]

- [01]-[MODEL_IDENTITY]: checksum identity; five-case acquisition union with the byte-resolution fold; schema snapshot with provenance; admission over input slots and overridable initializers; custom-metadata self-description; shared ordinal-keyvalue fingerprint; `ModelLoad` receipt mint.

## [02]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` schema rows, the `Provenance` producer/domain/graph triple, and the `CustomMetadata`/`Initializers` channels; `ModelSource` `[Union]` five acquisition cases whose `Acquire` fold resolves each to bytes through the injected `SourceResolver` ports and whose `Origin` projects the receipt source string; `ModelFingerprint` the one ordinal-keyvalue projection homed here and composed by `ExecutionProvider.ResultKey`.
- Cases: `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`, `Buffer`.
- Entry: `public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — pure value; identity derives from the bytes, never from the caller.
- Auto: `Snapshot` stamps checksum, graph version, the input/output/overridable-initializer slot rows, the provenance triple, and the `CustomMetadataMap` self-description in one pass; `Acquire` folds five source cases to one `Fin<ReadOnlyMemory<byte>>` — `LocalFile`/`EmbeddedResource`/`Buffer` self-resolve through the file, manifest-resource, and in-memory channels, `PersistenceBlob`/`RemoteFetch` defer to the injected `SourceResolver` ports — so a new route is one case, never a parallel loader. `Accepts` runs once at load and rejects any binding whose name, dtype, or fixed dims miss its target slot, naming each with its offered shape; `Initializer` admits a deployment-constant `OrtValue` against its slot only when dtype AND element count conform. `ModelFingerprint.Of` is the single ordinal-keyvalue `XxHash3` projection declared once here and composed by `ExecutionProvider.ResultKey` through `OptionsFor(precision)`, so a re-derived keyvalue body in either owner is the rejected form.
- Receipt: `ModelLoad` — the `Runtime/receipts#RECEIPT_UNION` `ModelLoad(checksum, source, ep, version)` shape — is minted by `LoadReceipt` from this owner's `Key`, `Source.Origin`, and snapshotted `GraphVersion` plus the loader's `ExecutionProvider`, correlation, and elapsed; emission rides the sink port at the composition edge.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new acquisition route is one `ModelSource` case plus its `Acquire` arm and `Origin` projection; a new deployment-constant binding is one `Initializer` admission against its overridable-initializer slot; a new self-description axis is one `Provenance` field read from `ModelMetadata`; zero new surface.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum` — path-keyed or filename-keyed identity is the deleted form — and `Checksum` composes the kernel seed-zero `XxHash128` entry `ContentHash.Of(bytes)`, the workspace's one hasher shared with the geometry `GeometryHash`, the seam `ContentAddress`, and the Persistence `ArtifactIndexRow`/`ModelResultIndex` spine, so a per-call-site `System.IO.Hashing.XxHash128.HashToUInt128(bytes)` is the named second-hasher defect the seam, kernel, and Persistence owners all reject. `Acquire` is the single byte-admission path carrying the `boundaries.md` CAPSULE_OWNER statement forms for the file and manifest-resource reads, bracketing every I/O fault into `ComputeFault.ModelRejected` and faulting a missing file or absent resource explicitly — a source case that cannot produce its bytes is the rejected inert-label form, so each of the five cases owns an `Acquire` arm. `SourceResolver` blob and remote ports are injected so this owner embeds neither a blob store nor an `HttpClient`, `SourceResolver.Local` faults its blob/remote arms, and `PersistenceBlob` resolves through the Persistence blob lane by `ArtifactIndexRow`, never by re-reading a path. `Slot.FreeDims` drives the free-dimension overrides at session build with symbolic-dim values arriving from the geometry-encoding rows, and `Accepts` treats a slot dim of `-1` as a wildcard so a free axis admits any extent while a fixed axis rejects a mismatch. `CustomMetadata` is read once at snapshot and mints NO second identity token — two models with identical bytes are one identity, and differing metadata already changes the bytes and thus the checksum, so a metadata-only fingerprint discriminates nothing. `Initializers` carry the `OverridableInitializerMetadata` deployment constants admitted through `AddInitializer(string, OrtValue)` at session build, never per-run inputs, and schema admission happens once at load. `ModelLoad` carries `GraphVersion` (the `ModelMetadata.Version` snapshotted at load) because the ONNX opset version is not exposed by the managed `ModelMetadata`/`InferenceSession` surface and a `Google.Protobuf` field scan to recover it is the form the protobuf rail rejects — so the load fact rides the derivable graph version, never an underivable opset threaded as a dead loader parameter.

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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
