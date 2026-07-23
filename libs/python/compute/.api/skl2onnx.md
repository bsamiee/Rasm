# [PY_COMPUTE_API_SKL2ONNX]

`skl2onnx` converts fitted scikit-learn estimators and `Pipeline`/`ColumnTransformer` objects to `onnx.ModelProto` for the compute model-asset export rail. It owns the conversion algebra — typed `initial_types`/`final_types`, operator gating, and custom converter/parser registration — and hands its output to the `onnx` structural gate ahead of `onnxruntime` inference.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `skl2onnx`
- package: `skl2onnx` (Apache-2.0)
- module: `skl2onnx`
- namespaces: `skl2onnx.algebra`, `skl2onnx.common`, `skl2onnx.common.data_types`, `skl2onnx.helpers`, `skl2onnx.operator_converters`, `skl2onnx.shape_calculators`, `skl2onnx.sklapi`
- rail: model-asset

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: operator algebra for custom converter nodes

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :--------------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `algebra.OnnxOperator`                   | class         | base for ONNX operator node builders; chains into a graph                |
|  [02]   | `algebra.onnx_operator.OnnxSubEstimator` | class         | embeds a fitted sub-estimator as an ONNX sub-graph in a custom converter |
|  [03]   | `algebra.onnx_ops`                       | module        | versioned standard operator nodes (`OnnxAdd`, `OnnxAbs_13`, ...)         |
|  [04]   | `algebra.custom_ops`                     | module        | custom-domain op node classes                                            |
|  [05]   | `algebra.complex_functions`              | module        | composite complex-function builders                                      |
|  [06]   | `algebra.type_helper`                    | module        | type annotation and mapping helpers                                      |
|  [07]   | `algebra.graph_state`                    | module        | graph-construction state carrier                                         |

[PUBLIC_TYPE_SCOPE]: initial-type vocabulary and cast transformers — each `*TensorType` names an ONNX graph input/output tensor, `None` in a shape dimension the dynamic batch axis; `sklapi` transformers slot at the pipeline front to pin numeric dtype against scikit-learn's float64 fit.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `common.data_types.FloatTensorType([None, n])` | class         | float32 tensor input for `initial_types`                     |
|  [02]   | `common.data_types.DoubleTensorType`           | class         | float64 tensor input                                         |
|  [03]   | `common.data_types.Int64TensorType`            | class         | integer tensor input (categorical pipelines)                 |
|  [04]   | `common.data_types.StringTensorType`           | class         | string tensor input (categorical pipelines)                  |
|  [05]   | `common.data_types.BooleanTensorType`          | class         | boolean tensor input (categorical pipelines)                 |
|  [06]   | `sklapi.CastTransformer(dtype)`                | class         | dtype-cast transformer pinning float64 at the pipeline front |
|  [07]   | `sklapi.ReplaceTransformer`                    | class         | value-replacement transformer for ONNX-safe preprocessing    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fitted-estimator export
- `to_onnx`/`convert_sklearn` share carry: `initial_types`, `target_opset`, `options`, `white_op`, `black_op`, `final_types`, `dtype`, `naming`, `model_optim`, `verbose`

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :---------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `to_onnx(model, X, name, ...)`      | static  | high-level export; infers `initial_types` from `X` |
|  [02]   | `convert_sklearn(model, name, ...)` | static  | lower-level export exposing explicit custom hooks  |

- `convert_sklearn` adds: `custom_conversion_functions`, `custom_shape_calculators`, `custom_parsers`, `doc_string`, `intermediate`

[ENTRYPOINT_SCOPE]: registration, introspection, and initial-type inference (`guess_*` under `common.data_types`)

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `update_registered_converter(model, alias, shape_fct, convert_fct, ...)` | static  | register a custom estimator converter        |
|  [02]   | `update_registered_parser(model, parser_fct)`                            | static  | register a custom parser for an estimator    |
|  [03]   | `get_model_alias(model_type)`                                            | static  | resolve the registered ONNX alias for a type |
|  [04]   | `supported_converters(from_sklearn)`                                     | static  | list registered converter aliases            |
|  [05]   | `get_latest_tested_opset_version()`                                      | static  | highest tested ONNX opset                    |
|  [06]   | `wrap_as_onnx_mixin(model, target_opset)`                                | static  | wrap a fitted estimator as an ONNX mixin     |
|  [07]   | `guess_data_type(type_, shape)`                                          | static  | infer `initial_types` from a sample array    |
|  [08]   | `guess_tensor_type(data_type)`                                           | static  | infer the tensor type from a dtype           |
|  [09]   | `guess_numpy_type(data_type)`                                            | static  | infer the numpy type from a type object      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Export folds through one route pair: `to_onnx(model, X=...)` infers `initial_types` from the sample; `convert_sklearn(model, initial_types=...)` declares them explicitly and exposes the custom-hook parameters. `target_opset` is an int or a `{domain: opset}` dict, `white_op`/`black_op` gate emitted operators, `final_types` renames graph outputs.
- `initial_types` declare from `common.data_types` (`FloatTensorType([None, n_features])` for a dense numeric matrix, `None` the dynamic batch axis) aligned to the trained feature schema and `get_feature_names_out()`; a dtype mismatch drives runtime drift, pinned by `sklapi.CastTransformer(np.float64)` at the pipeline front.
- An estimator with no shipped converter registers through `update_registered_converter(model, alias, shape_fct, convert_fct, parser=...)`; its converter body composes `algebra.OnnxOperator`/`OnnxSubEstimator` over the standard `onnx_ops`, and `supported_converters(True)`/`get_model_alias(type)` resolve the registry.

[STACKING]:
- `scikit-learn`(`.api/scikit-learn.md`): the export source is a fitted `Pipeline`/`ColumnTransformer`; `to_onnx(fitted, X, target_opset=...)` consumes its trained schema and `get_feature_names_out()`.
- `onnx`(`.api/onnx.md`): the emitted `onnx.ModelProto` graduates through `checker.check_model(full_check=True)` and `shape_inference.infer_shapes` — the structural gate ahead of the runtime.
- `onnxruntime`(`.api/onnxruntime.md`): the gated `ModelProto` loads into `InferenceSession(...).run(...)`, whose outputs compare against the scikit-learn `predict`/`predict_proba` on the same `X` for parity.
- within-lib: the model-asset receipt captures the target opset, the emitted operator list, and the `onnxruntime`-session parity check; `target_opset` stays at or below `get_latest_tested_opset_version()` unless explicit validation justifies a higher opset.

[LOCAL_ADMISSION]:
- A fitted `Pipeline`/estimator exports via `to_onnx(model, X, target_opset=<int>)` with `initial_types` from `common.data_types`, validated through an `onnxruntime` session; the receipt records target opset, operator list, and the inference-parity check.

[RAIL_LAW]:
- Package: `skl2onnx`
- Owns: scikit-learn-to-ONNX conversion — fitted estimator/pipeline export to `onnx.ModelProto`, typed `initial_types`/`final_types`, operator gating, custom converter/parser/shape-calculator registration, and the `OnnxOperator`/`OnnxSubEstimator` algebra for novel operators
- Accept: a fitted `sklearn` estimator exported via `to_onnx` with `initial_types` from `common.data_types`, validated through an `onnxruntime` session with a captured opset/operator/parity receipt
- Reject: hand-rolled ONNX graph construction for supported estimators; unfitted models passed to `to_onnx`; opsets above `get_latest_tested_opset_version()` without validation; untyped `initial_types` dropping the dynamic batch axis
