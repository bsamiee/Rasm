# [PY_COMPUTE_API_SKL2ONNX]

`skl2onnx` converts scikit-learn estimators and pipelines to ONNX graphs for the compute model-asset export rail. The package owner calls `to_onnx` or `convert_sklearn` on a fitted `sklearn` estimator, specifying initial types and a target opset; custom operator converters register through `update_registered_converter`. It never re-implements an ONNX graph builder or converter the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `skl2onnx`
- package: `skl2onnx`
- import: `skl2onnx`; submodules `skl2onnx.algebra`, `skl2onnx.common`, `skl2onnx.helpers`, `skl2onnx.operator_converters`, `skl2onnx.shape_calculators`
- owner: `compute`
- rail: model-asset
- capability: scikit-learn-to-ONNX export — converts fitted estimators and `Pipeline` objects to `onnx.ModelProto` with configurable target opset, type initial types, custom converter registration, and ONNX operator algebra for custom operators via `skl2onnx.algebra.OnnxOperator`

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: primary export functions
- rail: model-asset

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                   | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------- |
|   [1]   | `to_onnx(model, X, name, initial_types, target_opset, options, white_op, black_op, final_types, dtype, naming, model_optim, verbose)`                                                                                                       | primary export | converts fitted estimator to `onnx.ModelProto` |
|   [2]   | `convert_sklearn(model, name, initial_types, doc_string, target_opset, custom_conversion_functions, custom_shape_calculators, custom_parsers, options, intermediate, white_op, black_op, final_types, dtype, naming, model_optim, verbose)` | full export    | lower-level export with explicit custom hooks  |

[PUBLIC_TYPE_SCOPE]: converter and parser registration
- rail: model-asset

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|   [1]   | `update_registered_converter(model, alias, shape_fct, convert_fct, overwrite, parser, options)` | registration   | registers a custom estimator converter        |
|   [2]   | `update_registered_parser(model, parser_fct)`                                                   | registration   | registers a custom parser for an estimator    |
|   [3]   | `get_model_alias(model_type)`                                                                   | introspection  | returns the registered ONNX alias for a type  |
|   [4]   | `get_latest_tested_opset_version()`                                                             | opset query    | returns the highest tested ONNX opset version |
|   [5]   | `supported_converters(from_sklearn)`                                                            | introspection  | lists all registered converter aliases        |
|   [6]   | `wrap_as_onnx_mixin(model, target_opset)`                                                       | mixin factory  | wraps a fitted estimator as an ONNX mixin     |

[PUBLIC_TYPE_SCOPE]: algebra and operator building
- rail: model-asset

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]   | [CAPABILITY]                                  |
| :-----: | :--------------------- | :--------------- | :-------------------------------------------- |
|   [1]   | `algebra.OnnxOperator` | operator base    | base class for ONNX operator node builders    |
|   [2]   | `algebra.onnx_ops`     | operator module  | pre-built ONNX standard operator node classes |
|   [3]   | `algebra.custom_ops`   | custom op module | support for custom ONNX op nodes              |
|   [4]   | `algebra.type_helper`  | type utilities   | helpers for ONNX type annotation and mapping  |
|   [5]   | `algebra.graph_state`  | graph state      | state carrier during ONNX graph construction  |

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `skl2onnx`
- Owns: scikit-learn-to-ONNX conversion — fitted estimator export to `onnx.ModelProto`, custom converter registration, and ONNX operator algebra for novel operators
- Accept: a fitted `sklearn.pipeline.Pipeline` or estimator exported via `to_onnx(model, X, target_opset=<int>)`, then validated through `onnxruntime`; captured receipt includes target opset, operator list, and inference session check
- Reject: hand-rolled ONNX graph construction for estimators skl2onnx already supports; unfitted models passed to `to_onnx`; opset choices above `get_latest_tested_opset_version()` without explicit validation
