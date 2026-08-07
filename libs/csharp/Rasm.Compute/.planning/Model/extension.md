# [COMPUTE_EXTENSION_OPS]

Rasm.Compute model extension-ops: one `CustomOps` owner folds extension and custom-op registration into the `Model/sessions#SESSION_CAPSULE` admission AND reads the non-tensor model boundary the custom-op lane produces — `string`-tensor outputs and the structured `ZipMap` sequence/map outputs the numeric tensor egress cannot carry. ONNX Runtime owns the custom-op library lifetime through `RegisterCustomOpLibrary(path)`, freed when the `SessionOptions` and every session built from them release, so registration tracks no caller handle; the `out`-handle `RegisterCustomOpLibraryV2(path, out nint)` whose discarded handle leaks the library is the rejected form.

Registration extends the `ModelSessions` boundary capsule and rides `Microsoft.ML.OnnxRuntime.Extensions`/`Microsoft.ML.OnnxRuntime`; `SessionPolicy` arrives settled from `Model/sessions#SESSION_CAPSULE`, native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt, and string INGRESS rides `Model/inference#INFERENCE_MODES` `RunInput.Strings`. Non-tensor `Egress` is the catalogued completion of that ingress: `RunInput.Strings` admits a `Tensor<string>` through the `Tensor/residency` `TensorBridge.Ingress` `OrtValue.CreateFromStringTensor` factory (the sole `OrtValue` C-data factory, never re-minted here), and the `OnnxType`-discriminated `Egress` reads the model's non-tensor outputs back — never a second string-input factory and never the interior `System.Numerics.Tensors` carrier, because a string tensor is a model-boundary `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` only.

## [01]-[INDEX]

- [02]-[EXTENSION_OPS]: extension/custom-op registration with asset evidence and ORT-managed library lifetime; the recursive non-tensor `Egress` reader closing coverage over every container and leaf the identity snapshot can carry; the guarded bound string-output allocator.

## [02]-[EXTENSION_OPS]

- Owner: `CustomOps` — the registration fold over the extensions bundle and the custom-op library rows (ORT-managed lifetime, no caller handle), the guarded bound string-output allocator `StringSlots`, and the recursive non-tensor `Egress` projecting an output `OrtValue` onto the `OpOutput` `[Union]` by `OnnxValueType`; string INGRESS rides `RunInput.Strings` on the inference owner, never a second string-input factory here.
- Cases: registration arms `RegisterOrtExtensions` (the bundle, gated on `SessionPolicy.OrtExtensions`) and `RegisterCustomOpLibrary` per `SessionPolicy.CustomOpLibraries` path; `OpOutput` egress cases `Strings` (an `ONNX_TYPE_TENSOR` of `String` → shaped `Tensor<string>`), `Numeric` (every other tensor leaf, carried as its own dtype, shape, and owned byte run), `Mapping` (one `ONNX_TYPE_MAP` typed-key→typed-value roster), recursive `Sequence` over `ONNX_TYPE_SEQUENCE` elements, and recursive `Optional` over zero-or-one value; nested `MapKey` cases retain `String` and `Int64` identity without text coercion, and nested `MapValue` cases `Real` and `Whole` keep the value domain the map declared.
- Law: coverage is CLOSED against the identity snapshot. `Model/identity#MODEL_IDENTITY` `SlotShape` admits sequence and map slots by NAME because a slot describes a shape while a value carries one, and THIS grammar is where that value proves — so every `SlotShape` case reachable from a snapshot has a disposition here: a tensor leaf reads through `Strings` or `Numeric`, a sequence recurses through `Sequence`, a map reads through `Mapping`, an optional recurses through `Optional`, and a sparse tensor NESTED in a container refuses by name. Anything short of that leaves a schema the admitter accepts with no reader at run, which is the asymmetry that made a legal `seq(tensor(float))` output an unexplained refusal.
- Law: the sparse carve is OUTER-LEVEL routing, not a gap. A sparse output at the top is the caller's own undisposed value and crosses whole to `Tensor/residency#ORT_BRIDGE`; a sparse child dies with the container walk, and a sparse value's three-buffer residency is that owner's shape rather than a byte run this page could copy out. Reopens only on the residency owner publishing an owned sparse snapshot.
- Entry: `public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy)` aborts with `ExtensionAssetMissing` naming every absent or replaced custom-op asset before registration, then converts boundary exceptions to the same typed fault. `public Fin<OpOutput> Egress()` traps native metadata reads and faults `ModelRejected` on any outer or nested contract outside the admitted grammar — shaped `String` tensors with exact cardinality, concrete-shaped numeric tensors, typed `String`/`Int64` map keys paired one-to-one with finite `Float`/`Double` or exact `Int64` values, and containers whose every element proves recursively.
- Receipt: native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt; the missing-path set (or the native fault message) is the `ExtensionAssetMissing` payload.
- Packages: Microsoft.ML.OnnxRuntime.Extensions, Microsoft.ML.OnnxRuntime, LanguageExt.Core, BCL inbox
- Growth: a new custom-op library is one path row on `SessionPolicy.CustomOpLibraries`; a new ONNX value kind is one `OpOutput` case with one `OnnxType` arm on `Egress`, landing beside the `SlotShape` case the identity snapshot grows in the same change; a new map-value domain is one `MapValue` case and one `Valued` arm. Container growth costs nothing: `Sequence` and `Optional` recurse through the one reader, so a sequence of sequences of maps reads today.
- Boundary: `CustomOps` extends `Model/sessions#SESSION_CAPSULE`; asset guards precede registration, bundle faults convert at the seam, and every child `OrtValue` is read inside `using`. `RegisterCustomOpLibrary(path)` transfers lifetime to ONNX Runtime through the owning `SessionOptions`; `RegisterCustomOpLibraryV2(path, out _)` is rejected because the discarded handle leaks. `Egress` bulk-reads a `String` tensor only after every extent fits `int` and the shape product equals the returned element count, and a `Numeric` leaf leaves as an OWNED byte copy under its own dtype and shape — the child's native buffer dies with the walk, and copying opaque bytes is what keeps every dtype interpretation at `Tensor/residency#ORT_BRIDGE` rather than minting a second extraction here. `Pairs` requires exactly two tensor children, concrete rank-one extents, a legal ONNX map-key type, a modelled value type, matching declared and materialized cardinalities, and unique keys; `Real` proves finiteness while `Whole` keeps `Int64` exact, because widening a 64-bit integral onto a double silently rounds past 2^53 and then reads back as a different value. `ONNX_TYPE_SEQUENCE` recurses each element through the same reader rather than proving it a map first. `ONNX_TYPE_OPTIONAL` admits zero or one child and recursively proves the child before wrapping it. `StringSlots` admits only fixed nonnegative extents whose product fits native allocation; dynamic output routes through `Egress`.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OpOutput {
    private OpOutput() { }

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record MapKey {
        private MapKey() { }
        public sealed record String(string Value) : MapKey;
        public sealed record Int64(long Value) : MapKey;
    }

    // `Real` carries `Float` and `Double` alike because a float widens onto a double EXACTLY; `Whole` keeps `Int64`
    // in its own case because that widening does not — a 64-bit label past 2^53 rounds and reads back as another.
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record MapValue {
        private MapValue() { }
        public sealed record Real(double Value) : MapValue;
        public sealed record Whole(long Value) : MapValue;
    }

    public sealed record Strings(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> Text) : OpOutput;

    public sealed record Numeric(TensorElementType Dtype, Seq<long> Shape, ReadOnlyMemory<byte> Raw) : OpOutput;

    public sealed record Mapping(Seq<(MapKey Key, MapValue Value)> Pairs) : OpOutput;

    // ONE recursive container case covers every sequence shape ONNX admits — of maps (the `ZipMap` classifier
    // output), of tensors, of sequences, of optionals — where a maps-only case refused a legal `seq(tensor(float))`
    // output as an unmodelled element and forced a second case per element kind.
    public sealed record Sequence(Seq<OpOutput> Elements) : OpOutput;

    public sealed record Optional(Option<OpOutput> Value) : OpOutput;
}

public static class CustomOps {
    public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy) =>
        policy.CustomOpLibraries.TraverseM(static library => library.Verify()).As().Bind(_ => RegisterAdmitted(options, policy));

    static Fin<SessionOptions> RegisterAdmitted(SessionOptions options, SessionPolicy policy) {
        try {
            if (policy.OrtExtensions) { options.RegisterOrtExtensions(); }
            policy.CustomOpLibraries.Iter(library => options.RegisterCustomOpLibrary(library.Path));
            return Fin.Succ(options);
        }
        catch (Exception error) when (error is OnnxRuntimeException or ArgumentException or InvalidOperationException or DllNotFoundException) {
            return Fin.Fail<SessionOptions>(new ComputeFault.ExtensionAssetMissing(error.Message));
        }
    }

    public static Fin<OrtValue> StringSlots(OrtAllocator allocator, long[] shape) {
        long elements = shape.Aggregate(1L, static (size, extent) =>
            size < 0L || extent < 0L || extent is not 0L && size > long.MaxValue / extent ? -1L : size * extent);
        return elements < 0L
            ? Fin.Fail<OrtValue>(new ComputeFault.ModelRejected($"string-slots-symbolic:{string.Join('x', shape)}"))
            : Try.lift(() => OrtValue.CreateTensorWithEmptyStrings(allocator, shape)).Run()
                .MapFail(error => new ComputeFault.ModelRejected($"string-slots:{error.Message}"));
    }

    extension(OrtValue value) {
        public Fin<OpOutput> Egress() =>
            Try.lift(() => EgressAdmitted(value)).Run()
                .MapFail(error => new ComputeFault.ModelRejected($"non-tensor-egress:{error.Message}"))
                .Bind(identity);
    }

    // Child values die with the walk, so every recursion re-enters `EgressAdmitted` rather than the bracketed
    // `Egress`: one outer `Try` already owns every native read beneath it, and a per-level bracket would classify
    // one native fault at whichever depth it happened to surface.
    static Fin<OpOutput> EgressAdmitted(OrtValue value) => value.OnnxType switch {
        OnnxValueType.ONNX_TYPE_TENSOR => Dense(value, value.GetTensorTypeAndShape()),
        OnnxValueType.ONNX_TYPE_MAP => Pairs(value).Map(static pairs => (OpOutput)new OpOutput.Mapping(pairs)),
        OnnxValueType.ONNX_TYPE_SEQUENCE => Elements(value),
        OnnxValueType.ONNX_TYPE_OPTIONAL => Optional(value),
        // Outer-level sparse is the caller's own undisposed value and crosses whole to the residency owner; a sparse
        // CHILD has no such life, and its three-buffer layout is not a byte run this page can copy out.
        OnnxValueType.ONNX_TYPE_SPARSETENSOR =>
            Fin.Fail<OpOutput>(new ComputeFault.ModelRejected("non-tensor-egress-sparse")),
        OnnxValueType unmodeled => Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"non-tensor-egress:{unmodeled}")),
    };

    static Fin<OpOutput> Dense(OrtValue value, OrtTensorTypeAndShapeInfo info) =>
        info.ElementDataType is TensorElementType.String ? Strings(value, info) : Numeric(value, info);

    // Payload leaves as an OWNED byte copy under its declared dtype and shape: the copy is forced by the child's
    // lifetime, and carrying the bytes uninterpreted is what keeps every dtype dispatch at the residency owner
    // instead of minting a second numeric extraction on a page that exists for the non-tensor boundary.
    static Fin<OpOutput> Numeric(OrtValue value, OrtTensorTypeAndShapeInfo info) =>
        Array.Exists(info.Shape, static extent => extent < 0)
            ? Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"numeric-shape:{string.Join('x', info.Shape)}"))
            : Fin.Succ<OpOutput>(new OpOutput.Numeric(
                info.ElementDataType, toSeq(info.Shape), value.GetTensorMutableRawData().ToArray()));

    static Fin<OpOutput> Elements(OrtValue value) =>
        toSeq(Enumerable.Range(0, value.GetValueCount()))
            .TraverseM(index => {
                using OrtValue element = value.GetValue(index, OrtAllocator.DefaultInstance);
                return EgressAdmitted(element);
            })
            .As()
            .Map(static elements => (OpOutput)new OpOutput.Sequence(elements));

    static Fin<OpOutput> Optional(OrtValue value) => value.GetValueCount() switch {
        0 => Fin.Succ<OpOutput>(new OpOutput.Optional(None)),
        1 => WithOptional(value),
        int count => Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"optional-cardinality:{count}")),
    };

    static Fin<OpOutput> WithOptional(OrtValue value) {
        using OrtValue element = value.GetValue(0, OrtAllocator.DefaultInstance);
        return EgressAdmitted(element).Map(static output => (OpOutput)new OpOutput.Optional(Some(output)));
    }

    static Fin<OpOutput> Strings(OrtValue value, OrtTensorTypeAndShapeInfo info) {
        if (Array.Exists(info.Shape, static extent => extent is < 0 or > int.MaxValue)) {
            return Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"string-shape:{string.Join('x', info.Shape)}"));
        }
        string[] text = value.GetStringTensorAsArray();
        long elements = info.Shape.Aggregate(1L, static (size, extent) =>
            size < 0 || extent is not 0 && size > long.MaxValue / extent ? -1L : size * extent);
        return elements == text.LongLength
            ? Fin.Succ<OpOutput>(new OpOutput.Strings(new DenseTensor<string>(text, Array.ConvertAll(info.Shape, static extent => (int)extent))))
            : Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"string-cardinality:{elements}!={text.LongLength}"));
    }

    // Key and value dtypes are INDEPENDENT axes of one map, so each admits through its own arm and the shape gate
    // runs once over both — a value dtype pinned to `Float` inside the shape predicate refused a legal
    // `map(int64, double)` output with a shape message naming nothing that was wrong with its shape.
    static Fin<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>> Pairs(OrtValue map) {
        if (map.GetValueCount() is not 2) {
            return Refused($"map-children:{map.GetValueCount()}");
        }
        using OrtValue keys = map.GetValue(0, OrtAllocator.DefaultInstance);
        using OrtValue values = map.GetValue(1, OrtAllocator.DefaultInstance);
        if (keys.OnnxType is not OnnxValueType.ONNX_TYPE_TENSOR || values.OnnxType is not OnnxValueType.ONNX_TYPE_TENSOR) {
            return Refused($"map-child-types:{keys.OnnxType}:{values.OnnxType}");
        }
        OrtTensorTypeAndShapeInfo keyInfo = keys.GetTensorTypeAndShape();
        OrtTensorTypeAndShapeInfo valueInfo = values.GetTensorTypeAndShape();
        if (keyInfo.Shape is not [>= 0] || valueInfo.Shape is not [>= 0]
            || keyInfo.Shape[0] != valueInfo.Shape[0] || keyInfo.Shape[0] > int.MaxValue) {
            return Refused($"map-shape:{string.Join('x', keyInfo.Shape)}|{string.Join('x', valueInfo.Shape)}");
        }
        int cardinality = (int)keyInfo.Shape[0];
        return Keyed(keys, keyInfo.ElementDataType)
            .Bind(admitted => Valued(values, valueInfo.ElementDataType)
                .Bind(carried => Zip(admitted, carried, cardinality)));
    }

    static Fin<Seq<OpOutput.MapKey>> Keyed(OrtValue keys, TensorElementType dtype) => dtype switch {
        TensorElementType.String => Fin.Succ(toSeq(keys.GetStringTensorAsArray())
            .Map(static key => (OpOutput.MapKey)new OpOutput.MapKey.String(key))),
        TensorElementType.Int64 => Fin.Succ(toSeq(keys.GetTensorDataAsSpan<long>().ToArray())
            .Map(static key => (OpOutput.MapKey)new OpOutput.MapKey.Int64(key))),
        TensorElementType unmodeled =>
            Fin.Fail<Seq<OpOutput.MapKey>>(new ComputeFault.ModelRejected($"map-key:{unmodeled}")),
    };

    static Fin<Seq<OpOutput.MapValue>> Valued(OrtValue values, TensorElementType dtype) => dtype switch {
        TensorElementType.Float => Real(toSeq(values.GetTensorDataAsSpan<float>().ToArray()).Map(static value => (double)value)),
        TensorElementType.Double => Real(toSeq(values.GetTensorDataAsSpan<double>().ToArray())),
        TensorElementType.Int64 => Fin.Succ(toSeq(values.GetTensorDataAsSpan<long>().ToArray())
            .Map(static value => (OpOutput.MapValue)new OpOutput.MapValue.Whole(value))),
        TensorElementType unmodeled =>
            Fin.Fail<Seq<OpOutput.MapValue>>(new ComputeFault.ModelRejected($"map-value:{unmodeled}")),
    };

    // Finiteness is a REAL-value law alone: an integral map value has no degenerate encoding to screen for, so
    // folding both through one guard would invent a check the integral domain cannot fail.
    static Fin<Seq<OpOutput.MapValue>> Real(Seq<double> values) =>
        values.ForAll(double.IsFinite)
            ? Fin.Succ(values.Map(static value => (OpOutput.MapValue)new OpOutput.MapValue.Real(value)))
            : Fin.Fail<Seq<OpOutput.MapValue>>(new ComputeFault.ModelRejected($"map-value-degenerate:{values.Count}"));

    static Fin<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>> Zip(
        Seq<OpOutput.MapKey> keys, Seq<OpOutput.MapValue> values, int cardinality) =>
        keys.Count == cardinality && values.Count == cardinality && keys.Distinct().Count == cardinality
            ? Fin.Succ(keys.Zip(values, static (key, value) => (key, value)))
            : Refused($"map-cardinality:{keys.Count}:{values.Count}:{cardinality}");

    static Fin<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>> Refused(string detail) =>
        Fin.Fail<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>>(new ComputeFault.ModelRejected(detail));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
