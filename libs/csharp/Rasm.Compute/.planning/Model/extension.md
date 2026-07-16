# [COMPUTE_EXTENSION_OPS]

Rasm.Compute model extension-ops: one `CustomOps` owner folds extension and custom-op registration into the `Model/sessions#SESSION_CAPSULE` admission AND reads the non-tensor model boundary the custom-op lane produces — `string`-tensor outputs and the structured `ZipMap` sequence/map outputs the numeric tensor egress cannot carry. ONNX Runtime owns the custom-op library lifetime through `RegisterCustomOpLibrary(path)`, freed when the `SessionOptions` and every session built from them release, so registration tracks no caller handle; the `out`-handle `RegisterCustomOpLibraryV2(path, out nint)` whose discarded handle leaks the library is the rejected form.

Registration extends the `ModelSessions` boundary capsule and rides `Microsoft.ML.OnnxRuntime.Extensions`/`Microsoft.ML.OnnxRuntime`; `SessionPolicy` arrives settled from `Model/sessions#SESSION_CAPSULE`, native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt, and string INGRESS rides `Model/inference#INFERENCE_MODES` `RunInput.Strings`. Non-tensor `Egress` is the catalogued completion of that ingress: `RunInput.Strings` admits a `Tensor<string>` through the `Tensor/residency` `TensorBridge.Ingress` `OrtValue.CreateFromStringTensor` factory (the sole `OrtValue` C-data factory, never re-minted here), and the `OnnxType`-discriminated `Egress` reads the model's non-tensor outputs back — never a second string-input factory and never the interior `System.Numerics.Tensors` carrier, because a string tensor is a model-boundary `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` only.

## [01]-[INDEX]

- [01]-[EXTENSION_OPS]: extension/custom-op registration with asset evidence and ORT-managed library lifetime; the polymorphic non-tensor `Egress` reader over `string`-tensor and `ZipMap` sequence/map outputs; the guarded bound string-output allocator.

## [02]-[EXTENSION_OPS]

- Owner: `CustomOps` — the registration fold over the extensions bundle and the custom-op library rows (ORT-managed lifetime, no caller handle), the guarded bound string-output allocator `StringSlots`, and the polymorphic non-tensor `Egress` projecting an output `OrtValue` onto the `OpOutput` `[Union]` by `OnnxValueType`; string INGRESS rides `RunInput.Strings` on the inference owner, never a second string-input factory here.
- Cases: registration arms `RegisterOrtExtensions` (the bundle, gated on `SessionPolicy.OrtExtensions`) and `RegisterCustomOpLibrary` per `SessionPolicy.CustomOpLibraries` path; `OpOutput` egress cases `Strings` (an `ONNX_TYPE_TENSOR` of `String` → shaped `Tensor<string>`), `Mapping` (one `ONNX_TYPE_MAP` typed-key→score), `Batched` (an `ONNX_TYPE_SEQUENCE` of maps — the `ZipMap` classifier output), and recursive `Optional` over zero-or-one non-tensor value; nested `MapKey` cases retain `String` and `Int64` identity without text coercion.
- Entry: `public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy)` aborts with `ExtensionAssetMissing` naming every absent or replaced custom-op asset before registration, then converts boundary exceptions to the same typed fault. `public Fin<OpOutput> Egress()` traps native metadata reads and faults `ModelRejected` on any outer or nested contract outside the admitted grammar — shaped `String` tensors with exact cardinality, typed `String`/`Int64` map keys paired one-to-one with finite rank-one `Float` scores, and map-shaped sequence elements.
- Receipt: native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt; the missing-path set (or the native fault message) is the `ExtensionAssetMissing` payload.
- Packages: Microsoft.ML.OnnxRuntime.Extensions, Microsoft.ML.OnnxRuntime, LanguageExt.Core, BCL inbox
- Growth: a new custom-op library is one path row on `SessionPolicy.CustomOpLibraries`; a new non-tensor output kind is one `OpOutput` case plus one `OnnxType` arm on `Egress`. `ONNX_TYPE_OPTIONAL` reuses `Egress` recursively and preserves absence as `Option<OpOutput>`; sparse numeric output remains on the inference/tensor egress owner because this page never duplicates numeric extraction.
- Boundary: `CustomOps` extends `Model/sessions#SESSION_CAPSULE`; asset guards precede registration, bundle faults convert at the seam, and every child `OrtValue` is read inside `using`. `RegisterCustomOpLibrary(path)` transfers lifetime to ONNX Runtime through the owning `SessionOptions`; `RegisterCustomOpLibraryV2(path, out _)` is rejected because the discarded handle leaks. `Egress` bulk-reads a `String` tensor only after every extent fits `int` and the shape product equals the returned element count. `Pairs` requires exactly two tensor children, concrete rank-one extents, `Float` scores, a legal ONNX map-key type, matching declared and materialized cardinalities, unique keys, and finite scores; `Zip` never truncates malformed output or erases key type. `ONNX_TYPE_SEQUENCE` proves each element is a map before traversal. `ONNX_TYPE_OPTIONAL` admits zero or one child and recursively proves the child before wrapping it. Numeric and sparse tensors remain on the inference/tensor egress owner; nested contracts outside this grammar return `ModelRejected`. `StringSlots` admits only fixed nonnegative extents whose product fits native allocation; dynamic output routes through `Egress`.

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

    public sealed record Strings(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> Text) : OpOutput;

    public sealed record Mapping(Seq<(MapKey Key, float Score)> Pairs) : OpOutput;

    public sealed record Batched(Seq<Seq<(MapKey Key, float Score)>> Rows) : OpOutput;

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

    static Fin<OpOutput> EgressAdmitted(OrtValue value) => value.OnnxType switch {
        OnnxValueType.ONNX_TYPE_TENSOR when value.GetTensorTypeAndShape() is { ElementDataType: TensorElementType.String } info =>
            Strings(value, info),
        OnnxValueType.ONNX_TYPE_MAP =>
            Pairs(value).Map(static pairs => (OpOutput)new OpOutput.Mapping(pairs)),
        OnnxValueType.ONNX_TYPE_SEQUENCE =>
            toSeq(Enumerable.Range(0, value.GetValueCount()))
                .TraverseM(index => {
                    using OrtValue element = value.GetValue(index, OrtAllocator.DefaultInstance);
                    return element.OnnxType is OnnxValueType.ONNX_TYPE_MAP
                        ? Pairs(element)
                        : Fin.Fail<Seq<(OpOutput.MapKey Key, float Score)>>(new ComputeFault.ModelRejected($"sequence-element:{element.OnnxType}"));
                })
                .As()
                .Map(static rows => (OpOutput)new OpOutput.Batched(rows)),
        OnnxValueType.ONNX_TYPE_OPTIONAL => Optional(value),
        _ => Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"non-tensor-egress:{value.OnnxType}")),
    };

    static Fin<OpOutput> Optional(OrtValue value) => value.GetValueCount() switch {
        0 => Fin.Succ<OpOutput>(new OpOutput.Optional(None)),
        1 => WithOptional(value),
        int count => Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"optional-cardinality:{count}")),
    };

    static Fin<OpOutput> WithOptional(OrtValue value) {
        using OrtValue element = value.GetValue(0, OrtAllocator.DefaultInstance);
        return element.Egress().Map(static output => (OpOutput)new OpOutput.Optional(Some(output)));
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

    static Fin<Seq<(OpOutput.MapKey Key, float Score)>> Pairs(OrtValue map) {
        if (map.GetValueCount() is not 2) {
            return Fin.Fail<Seq<(OpOutput.MapKey Key, float Score)>>(new ComputeFault.ModelRejected($"map-children:{map.GetValueCount()}"));
        }
        using OrtValue keys = map.GetValue(0, OrtAllocator.DefaultInstance);
        using OrtValue values = map.GetValue(1, OrtAllocator.DefaultInstance);
        if (keys.OnnxType is not OnnxValueType.ONNX_TYPE_TENSOR || values.OnnxType is not OnnxValueType.ONNX_TYPE_TENSOR) {
            return Fin.Fail<Seq<(OpOutput.MapKey Key, float Score)>>(new ComputeFault.ModelRejected($"map-child-types:{keys.OnnxType}:{values.OnnxType}"));
        }
        OrtTensorTypeAndShapeInfo keyInfo = keys.GetTensorTypeAndShape();
        OrtTensorTypeAndShapeInfo valueInfo = values.GetTensorTypeAndShape();
        if (keyInfo.Shape is not [>= 0] || valueInfo.ElementDataType is not TensorElementType.Float || valueInfo.Shape is not [>= 0]
            || keyInfo.Shape[0] != valueInfo.Shape[0] || keyInfo.Shape[0] > int.MaxValue) {
            return Fin.Fail<Seq<(OpOutput.MapKey Key, float Score)>>(new ComputeFault.ModelRejected($"map-shape:{keyInfo.ElementDataType}:{string.Join('x', keyInfo.Shape)}|{valueInfo.ElementDataType}:{string.Join('x', valueInfo.Shape)}"));
        }
        int cardinality = (int)keyInfo.Shape[0];
        return keyInfo.ElementDataType switch {
            TensorElementType.String => Zip(toSeq(keys.GetStringTensorAsArray()).Map(static key => (OpOutput.MapKey)new OpOutput.MapKey.String(key)), values, cardinality),
            TensorElementType.Int64 => Zip(toSeq(keys.GetTensorDataAsSpan<long>().ToArray()).Map(static key => (OpOutput.MapKey)new OpOutput.MapKey.Int64(key)), values, cardinality),
            TensorElementType unmodeled => Fin.Fail<Seq<(OpOutput.MapKey Key, float Score)>>(new ComputeFault.ModelRejected($"map-key:{unmodeled}")),
        };
    }

    static Fin<Seq<(OpOutput.MapKey Key, float Score)>> Zip(Seq<OpOutput.MapKey> keys, OrtValue values, int cardinality) {
        Seq<float> scores = toSeq(values.GetTensorDataAsSpan<float>().ToArray());
        return keys.Count == cardinality && scores.Count == cardinality && keys.Distinct().Count == cardinality && scores.ForAll(float.IsFinite)
            ? Fin.Succ(keys.Zip(scores, static (key, score) => (key, score)))
            : Fin.Fail<Seq<(OpOutput.MapKey Key, float Score)>>(new ComputeFault.ModelRejected($"map-cardinality:{keys.Count}:{scores.Count}:{cardinality}"));
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
