# [PY_COMPUTE_API_SKL2ONNX]

`skl2onnx` converts scikit-learn estimators and pipelines to ONNX graphs for the compute model-asset export rail. The package owner calls `to_onnx` or `convert_sklearn` on a fitted `sklearn` estimator (`.api/scikit-learn.md`), declaring `initial_types` from the trained feature schema and a `target_opset` at or below `get_latest_tested_opset_version()`, validates the `onnx.ModelProto` through an `onnxruntime` session, and registers custom converters through `update_registered_converter`. It never hand-rolls an ONNX graph builder or converter the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `skl2onnx`
- package: `skl2onnx`
- import: `skl2onnx`; submodules `skl2onnx.algebra`, `skl2onnx.common`, `skl2onnx.common.data_types`, `skl2onnx.helpers`, `skl2onnx.operator_converters`, `skl2onnx.shape_calculators`, `skl2onnx.sklapi`
- owner: `compute`
- rail: model-asset
- installed: marker-gated `python_version<'3.15'` (no cp315 wheel; follows the scikit-learn cp315 floor); license Apache-2.0; pure-Python over `onnx`/`onnxconverter-common`, declares `scikit-learn` and `onnx` as runtime peers
- capability: scikit-learn-to-ONNX export — converts fitted estimators and `Pipeline`/`ColumnTransformer` objects to `onnx.ModelProto` with configurable target opset (int or `{domain: int}` dict), typed `initial_types`/`final_types`, `white_op`/`black_op` operator gating, custom converter/parser/shape-calculator registration, the `OnnxOperator`/`OnnxSubEstimator` operator algebra for custom nodes, and the `sklapi` cast transformers that stabilize float32-vs-float64 numeric drift at the pipeline front; latest tested ONNX opset is `22`

## [02]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: primary export functions
- rail: model-asset

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                   | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `to_onnx(model, X, name, initial_types, target_opset, options, white_op, black_op, final_types, dtype, naming, model_optim, verbose)`                                                                                                       | primary export | converts fitted estimator to `onnx.ModelProto` |
|  [02]   | `convert_sklearn(model, name, initial_types, doc_string, target_opset, custom_conversion_functions, custom_shape_calculators, custom_parsers, options, intermediate, white_op, black_op, final_types, dtype, naming, model_optim, verbose)` | full export    | lower-level export with explicit custom hooks  |

[PUBLIC_TYPE_SCOPE]: converter and parser registration
- rail: model-asset

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `update_registered_converter(model, alias, shape_fct, convert_fct, overwrite, parser, options)` | registration   | registers a custom estimator converter        |
|  [02]   | `update_registered_parser(model, parser_fct)`                                                   | registration   | registers a custom parser for an estimator    |
|  [03]   | `get_model_alias(model_type)`                                                                   | introspection  | returns the registered ONNX alias for a type  |
|  [04]   | `get_latest_tested_opset_version()`                                                             | opset query    | returns the highest tested ONNX opset version |
|  [05]   | `supported_converters(from_sklearn)`                                                            | introspection  | lists all registered converter aliases        |
|  [06]   | `wrap_as_onnx_mixin(model, target_opset)`                                                       | mixin factory  | wraps a fitted estimator as an ONNX mixin     |

[PUBLIC_TYPE_SCOPE]: algebra and operator building
- rail: model-asset

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `algebra.OnnxOperator`            | operator base    | base class for ONNX operator node builders (chains into a graph) |
|  [02]   | `algebra.onnx_operator.OnnxSubEstimator` | sub-graph | embeds a fitted sub-estimator as an ONNX sub-graph inside a custom converter |
|  [03]   | `algebra.onnx_ops`                | operator module  | versioned standard operator node classes (`OnnxAdd`, `OnnxMatMul`, `OnnxAbs_13`, ...) |
|  [04]   | `algebra.custom_ops` \| `algebra.complex_functions` | custom op | custom-domain op nodes and composite complex-function builders |
|  [05]   | `algebra.type_helper` \| `algebra.graph_state` | build state | type annotation/mapping helpers and graph-construction state carrier |

[PUBLIC_TYPE_SCOPE]: initial-type vocabulary and cast transformers
- rail: model-asset

`common.data_types` is the `initial_types`/`final_types` declaration vocabulary — each `*TensorType([rows, cols])` names an ONNX graph input/output tensor; `None` in a shape dimension is the dynamic batch axis. `sklapi` transformers slot at the pipeline front to pin the numeric dtype, neutralizing the float32 ONNX-runtime drift against scikit-learn's float64 fit.

| [INDEX] | [SYMBOL]                                                        | [PACKAGE_ROLE]   | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `common.data_types.FloatTensorType([None, n])` \| `DoubleTensorType` | input type | float32 / float64 tensor input declaration for `initial_types` |
|  [02]   | `common.data_types.Int64TensorType` \| `StringTensorType` \| `BooleanTensorType` | input type | integer / string / boolean tensor inputs (categorical pipelines) |
|  [03]   | `common.data_types.guess_data_type(X)` \| `guess_tensor_type(dtype)` \| `guess_numpy_type(t)` | type inference | infer `initial_types` from a sample array / dtype |
|  [04]   | `sklapi.CastTransformer(dtype=np.float64)`                      | pipeline step    | dtype-cast transformer that pins float64 at the pipeline front |
|  [05]   | `sklapi.ReplaceTransformer`                                     | pipeline step    | value-replacement transformer for ONNX-safe preprocessing |

## [03]-[LOCAL_ADMISSION]

[EXPORT_TOPOLOGY]:
- export: `to_onnx(model, X=...)` is the high-level route (infers `initial_types` from the sample `X`); `convert_sklearn(model, initial_types=...)` is the explicit route exposing `custom_conversion_functions`/`custom_shape_calculators`/`custom_parsers` hooks. `target_opset` is an int or a `{domain: opset}` dict; `white_op`/`black_op` gate which ONNX operators the converter may emit; `final_types` renames graph outputs.
- typing: `initial_types` are declared from `common.data_types` (`FloatTensorType([None, n_features])` for a dense numeric matrix; `None` is the dynamic batch axis) and aligned to the scikit-learn pipeline's trained feature schema and `get_feature_names_out()`; mismatched dtypes cause runtime drift, mitigated by a `sklapi.CastTransformer(np.float64)` at the pipeline front.
- registration: an estimator skl2onnx does not ship a converter for registers through `update_registered_converter(model, alias, shape_fct, convert_fct, parser=...)`; the converter body composes `algebra.OnnxOperator`/`OnnxSubEstimator` over the standard `onnx_ops`; `supported_converters(True)` lists registered aliases and `get_model_alias(type)` resolves one.
- validation: the exported `onnx.ModelProto` is validated by loading it into an `onnxruntime.InferenceSession` and comparing `session.run` outputs against the scikit-learn `predict`/`predict_proba` on the same `X`; `target_opset` stays at or below `get_latest_tested_opset_version()` (currently `22`) unless an explicit validation justifies a higher opset.
- stacking: the export source is the fitted scikit-learn `Pipeline`/`ColumnTransformer` (`.api/scikit-learn.md`); the validated `ModelProto` graduates through the `onnx`/`onnxruntime` model-asset checks; the model-asset receipt captures the target opset, the emitted operator list, and the `onnxruntime`-session parity check.

[RAIL_LAW]:
- Package: `skl2onnx`
- Owns: scikit-learn-to-ONNX conversion — fitted estimator/pipeline export to `onnx.ModelProto`, typed `initial_types`/`final_types`, operator gating, custom converter/parser/shape-calculator registration, and the `OnnxOperator`/`OnnxSubEstimator` algebra for novel operators
- Accept: a fitted `sklearn.pipeline.Pipeline` or estimator exported via `to_onnx(model, X, target_opset=<int>)` with `initial_types` from `common.data_types`, then validated through an `onnxruntime` session; captured receipt includes target opset, operator list, and the inference-parity check
- Reject: hand-rolled ONNX graph construction for estimators skl2onnx already supports; unfitted models passed to `to_onnx`; opset choices above `get_latest_tested_opset_version()` without explicit validation; untyped `initial_types` that drop the dynamic batch axis
