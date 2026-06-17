# [PY_COMPUTE_API_SCIKIT_LEARN]

`scikit-learn` supplies the classical-ML estimator surface — preprocessing, supervised and unsupervised estimators, pipelines, and model selection — for the compute model-asset rail. The package owner fits classical models offline and exports them to ONNX as graduation candidates; it never re-implements an estimator the package owns. The distribution is marker-gated `python_version<'3.15'` and is absent from the cp315 lock; member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-learn`
- package: `scikit-learn`
- import: `sklearn`; submodules `sklearn.pipeline`, `sklearn.preprocessing`, `sklearn.model_selection`, `sklearn.linear_model`, `sklearn.ensemble`, `sklearn.cluster`
- owner: `compute`
- rail: model
- installed: ABSENT on cp315 (manifest pin `scikit-learn>=1.9.0; python_version<'3.15'`; no cp315 wheel — manifest gaps 1+2)
- capability: classical machine learning — estimators, transformers, pipelines, cross-validation, and a uniform fit/predict/transform protocol; `sklearn`-to-ONNX export feeds the model-asset rail

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: estimator-protocol owners
- rail: model

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE]        | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------- | :-------------------- | :----------------------------------------- |
|   [1]   | `sklearn.base.BaseEstimator`                                | estimator root        | base of every estimator                    |
|   [2]   | `sklearn.pipeline.Pipeline`                                 | composition root      | chains transformers and a final estimator  |
|   [3]   | `sklearn.preprocessing`                                     | transformer submodule | scaling, encoding, normalization           |
|   [4]   | `sklearn.model_selection`                                   | selection submodule   | cross-validation and hyperparameter search |
|   [5]   | `sklearn.linear_model` `sklearn.ensemble` `sklearn.cluster` | estimator submodules  | supervised and unsupervised estimators     |

[ENTRYPOINTS]:
- UN_REFLECTED: exact estimator-protocol method spellings (`fit`, `predict`, `transform`, `fit_transform`, `score`) and verified constructor signatures require a reflectable install to capture; the submodule/type names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- fitting: classical models compose as a `Pipeline` of `sklearn.preprocessing` transformers and a final estimator, selected through `sklearn.model_selection`.
- graduation: a fitted pipeline exports to ONNX (`skl2onnx` route) and graduates through the `onnx`/`onnxruntime` model-asset checks.
- evidence: the model-asset receipt captures the pipeline composition, the cross-validation score, and the ONNX export result.
- boundary: scikit-learn fitting is offline; production model serving and benchmark authority stay in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `scikit-learn`
- Owns: offline classical-ML fitting, pipeline composition, model selection, and ONNX export for the model-asset rail
- Accept: a fitted `Pipeline` with a captured cross-validation score and ONNX export receipt
- Reject: production model serving; hand-rolled estimators sklearn owns; wrapper-renames of the fit/predict protocol
