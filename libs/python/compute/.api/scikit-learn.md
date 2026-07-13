# [PY_COMPUTE_API_SCIKIT_LEARN]

`scikit-learn` supplies the classical-ML estimator surface — preprocessing, supervised and unsupervised estimators, feature selection, decomposition, calibration, pipelines, and model selection — for the compute model-asset rail. The package owner composes a `Pipeline`/`ColumnTransformer` of transformers and a final estimator, tunes it through `model_selection`, scores it with `metrics`, fits it offline, and exports the fitted pipeline to ONNX (`.api/skl2onnx.md`) as a graduation candidate; it never re-implements an estimator, splitter, or metric the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-learn`
- package: `scikit-learn`
- import: `sklearn`; submodules `sklearn.base`, `sklearn.pipeline`, `sklearn.compose`, `sklearn.preprocessing`, `sklearn.model_selection`, `sklearn.linear_model`, `sklearn.ensemble`, `sklearn.cluster`, `sklearn.svm`, `sklearn.tree`, `sklearn.decomposition`, `sklearn.feature_selection`, `sklearn.calibration`, `sklearn.inspection`, `sklearn.metrics`; active gates import-time (`from sklearn.experimental import enable_halving_search_cv`)
- owner: `compute`
- rail: model
- capability: classical machine learning — estimators, transformers, pipelines, cross-validation, decomposition, feature selection, probability calibration, model inspection, metrics, and a uniform `fit`/`predict`/`transform`/`score` protocol with metadata routing and DataFrame/array output configuration; the fitted pipeline is the ONNX export source for the model-asset rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: estimator-protocol roots and composition
- rail: model

Every estimator derives from `BaseEstimator`; mixins add the task verbs. Composition roots chain steps and route columns; `set_config(transform_output='pandas'/'polars')` and per-estimator `set_output` make the whole pipeline emit DataFrames with feature-name provenance, and `set_*_request` enables metadata routing of `sample_weight`/`groups` through the pipeline.

| [INDEX] | [SYMBOL]                             | [PROTOCOL_ROLE]     | [PROTOCOL_METHODS]                                                     |
| :-----: | :----------------------------------- | :------------------ | :--------------------------------------------------------------------- |
|  [01]   | `base.BaseEstimator`                 | estimator root      | `get_params`, `set_params`, `set_output`, `set_*_request`              |
|  [02]   | `base.TransformerMixin`              | transformer mixin   | `fit_transform`, `get_feature_names_out`                               |
|  [03]   | `base.ClassifierMixin`               | classifier mixin    | `score` (accuracy)                                                     |
|  [04]   | `base.RegressorMixin`                | regressor mixin     | `score` (R^2)                                                          |
|  [05]   | `base.ClusterMixin`                  | clusterer mixin     | `fit_predict`                                                          |
|  [06]   | `base.clone`                         | estimator copy      | unfitted deep copy preserving hyperparameters                          |
|  [07]   | `pipeline.Pipeline`                  | composition root    | chains transformers + estimator (`steps`, `transform_input`, `memory`) |
|  [08]   | `pipeline.FeatureUnion`              | composition root    | concatenates transformer outputs                                       |
|  [09]   | `pipeline.FunctionTransformer`       | stateless transform | wraps a callable as a transformer for a pipeline step                  |
|  [10]   | `compose.ColumnTransformer`          | column router       | per-column-subset transformers with `remainder` passthrough            |
|  [11]   | `compose.TransformedTargetRegressor` | target wrap         | transforms `y` before fit, inverts on predict                          |

[PUBLIC_TYPE_SCOPE]: supervised estimators
- rail: model
- estimators import from `linear_model` (linear), `ensemble` (bagging/boosting/stacking/voting), `svm` (kernel), `tree` (tree), and `calibration`; a row lists the `Classifier`/`Regressor` pair where both exist.

| [INDEX] | [SYMBOL]                                                            | [ESTIMATOR_FAMILY] | [TASK]                                      |
| :-----: | :------------------------------------------------------------------ | :----------------- | :------------------------------------------ |
|  [01]   | `LinearRegression`                                                  | linear             | regression                                  |
|  [02]   | `LogisticRegression`                                                | linear             | classification (`predict_proba`)            |
|  [03]   | `Ridge` \| `Lasso` \| `ElasticNet`                                  | linear             | regularized regression                      |
|  [04]   | `SGDClassifier` \| `SGDRegressor`                                   | linear             | large-scale online learning                 |
|  [05]   | `RandomForestClassifier` \| `RandomForestRegressor`                 | bagging            | classification / regression                 |
|  [06]   | `HistGradientBoostingClassifier` \| `HistGradientBoostingRegressor` | boosting           | histogram GB (native categorical + missing) |
|  [07]   | `GradientBoostingRegressor`                                         | boosting           | regression                                  |
|  [08]   | `StackingClassifier` \| `StackingRegressor`                         | stacking           | meta-estimator over base learners           |
|  [09]   | `VotingClassifier` \| `VotingRegressor`                             | voting             | hard/soft ensemble vote                     |
|  [10]   | `SVC` \| `SVR`                                                      | kernel             | classification / regression                 |
|  [11]   | `DecisionTreeClassifier` \| `DecisionTreeRegressor`                 | tree               | single-tree classification / regression     |
|  [12]   | `CalibratedClassifierCV`                                            | probability calib  | sigmoid/isotonic probability calibration    |

[PUBLIC_TYPE_SCOPE]: transformers, decomposition, feature selection, and clusterers
- rail: model

| [INDEX] | [SYMBOL]                                                               | [SUBMODULE]         | [ROLE]                                  |
| :-----: | :--------------------------------------------------------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `StandardScaler` \| `MinMaxScaler` \| `RobustScaler` \| `MaxAbsScaler` | `preprocessing`     | center/range/quantile-robust scaling    |
|  [02]   | `OneHotEncoder` \| `OrdinalEncoder` \| `TargetEncoder`                 | `preprocessing`     | one-hot / ordinal / target encoding     |
|  [03]   | `PolynomialFeatures` \| `SplineTransformer`                            | `preprocessing`     | polynomial / B-spline feature expansion |
|  [04]   | `PowerTransformer` \| `QuantileTransformer` \| `KBinsDiscretizer`      | `preprocessing`     | Gaussian / quantile / binning transform |
|  [05]   | `SimpleImputer` \| `KNNImputer` \| `IterativeImputer`                  | `impute`            | missing-value imputation                |
|  [06]   | `PCA` \| `TruncatedSVD` \| `KernelPCA` \| `NMF`                        | `decomposition`     | linear/sparse/kernel/nonneg reduction   |
|  [07]   | `SelectKBest` \| `SelectFromModel` \| `RFE` \| `RFECV`                 | `feature_selection` | univariate/model-based/recursive        |
|  [08]   | `KMeans` \| `MiniBatchKMeans`                                          | `cluster`           | centroid clustering                     |
|  [09]   | `DBSCAN` \| `HDBSCAN`                                                  | `cluster`           | density/hierarchical-density clustering |
|  [10]   | `AgglomerativeClustering` \| `SpectralClustering`                      | `cluster`           | hierarchical/graph-spectral clustering  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: uniform estimator protocol
- rail: model

One protocol spans every estimator; the verb is discriminated by mixin (transformer `transform`, classifier `predict`/`predict_proba`/`decision_function`, regressor `predict`, clusterer `fit_predict`). `set_output(transform='pandas'/'polars')` makes any transformer emit a named-column frame.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]   | [RESULT]                                                |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------------------------ |
|  [01]   | `estimator.fit(X, y, **fit_params)`                    | train            | the fitted estimator (`self`)                           |
|  [02]   | `estimator.predict(X)`                                 | inference        | predicted labels or values                              |
|  [03]   | `estimator.predict_proba(X)` \| `decision_function(X)` | inference        | class probabilities / signed margins                    |
|  [04]   | `transformer.transform(X)` \| `fit_transform(X, y)`    | transform        | transformed feature array (frame if `set_output`)       |
|  [05]   | `estimator.score(X, y)`                                | evaluate         | default task score (accuracy / R^2)                     |
|  [06]   | `estimator.get_params()` \| `set_params(**p)`          | configure        | hyperparameter dict / reconfigured estimator            |
|  [07]   | `estimator.set_output(transform='pandas')`             | output config    | route transform output to a DataFrame backend           |
|  [08]   | `estimator.set_fit_request(sample_weight=True)`        | metadata routing | enable per-step metadata propagation through a pipeline |
|  [09]   | `transformer.get_feature_names_out()`                  | introspect       | output feature names for downstream provenance          |

[ENTRYPOINT_SCOPE]: pipeline composition and model selection
- rail: model

`Pipeline`/`ColumnTransformer` are the canonical composition roots; tuning discriminates exhaustive (`GridSearchCV`) vs sampled (`RandomizedSearchCV`) vs successive-halving (`HalvingGridSearchCV`/`HalvingRandomSearchCV`, gated by the active enable import). Rows drop their `pipeline`/`compose`/`model_selection` submodule prefix. CV entries take `(est, X, y, cv=, scoring=)` (`cross_validate` adds `return_estimator=`); searches take `param_grid`/`param_distributions` (+`n_iter` for randomized). The `model_selection` splitters passed as `cv=` are `StratifiedKFold`/`KFold`/`GroupKFold`/`StratifiedGroupKFold`/`TimeSeriesSplit`/`RepeatedStratifiedKFold`/`ShuffleSplit`/`LeaveOneOut`; each composition root has a `make_*` auto-naming factory (`make_pipeline`/`make_union`/`make_column_transformer`).

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RESULT]                                           |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Pipeline(steps, memory)`                                          | compose        | chained estimator (auto-named for `make_pipeline`) |
|  [02]   | `FeatureUnion(...)` \| `FunctionTransformer(func)`                 | compose        | concatenated / stateless transform steps           |
|  [03]   | `ColumnTransformer(transformers, remainder)`                       | compose        | column-routed transformer                          |
|  [04]   | `train_test_split(*arrays, stratify, test_size)`                   | split          | train/test array partition                         |
|  [05]   | `cross_val_score` \| `cross_validate`                              | evaluate       | CV score array / per-metric result dict            |
|  [06]   | `cross_val_predict(est, X, y, cv, method)`                         | evaluate       | out-of-fold predictions                            |
|  [07]   | `GridSearchCV` \| `RandomizedSearchCV`                             | tune           | refitted best-parameter estimator                  |
|  [08]   | `HalvingGridSearchCV(...)` \| `HalvingRandomSearchCV(...)`         | tune           | successive-halving tuned estimator (enable-gated)  |
|  [09]   | `learning_curve` \| `validation_curve` \| `permutation_test_score` | diagnose       | learning/validation curve + permutation test       |

[ENTRYPOINT_SCOPE]: scoring, calibration, and inspection
- rail: model

`make_scorer`/`get_scorer` produce the `scoring=` callable used by every CV entry; `permutation_importance` and `partial_dependence` explain a fitted pipeline. Each row's exact member roster is listed by matching index in `[FUNCTION_ROSTER]` below the table.

| [INDEX] | [ENTRY_FAMILY] | [RESULT]                                       |
| :-----: | :------------- | :--------------------------------------------- |
|  [01]   | classification | scalar classification scores                   |
|  [02]   | classification | probabilistic ranking / loss scores            |
|  [03]   | classification | curve arrays / matrix / per-class report       |
|  [04]   | regression     | regression error / determination scores        |
|  [05]   | clustering     | cluster cohesion/separation score              |
|  [06]   | scorer factory | `scoring=` callable for CV / search            |
|  [07]   | inspection     | feature-importance / partial-dependence result |

[FUNCTION_ROSTER]: exact member names per row
- [01]-[CLASS_SCORES]: `metrics.accuracy_score`/`balanced_accuracy_score`/`f1_score`/`matthews_corrcoef`
- [02]-[CLASS_RANKING]: `metrics.roc_auc_score`/`average_precision_score`/`log_loss`
- [03]-[CLASS_CURVES]: `metrics.roc_curve`/`precision_recall_curve`/`confusion_matrix`/`classification_report`
- [04]-[REGRESSION]: `metrics.mean_squared_error`/`root_mean_squared_error`/`mean_absolute_error`/`mean_absolute_percentage_error`/`r2_score`/`mean_pinball_loss`/`d2_absolute_error_score`
- [05]-[CLUSTERING]: `metrics.silhouette_score`
- [06]-[SCORER_FACTORY]: `metrics.make_scorer(score_func, greater_is_better)`/`get_scorer(name)`/`get_scorer_names()`
- [07]-[INSPECTION]: `inspection.permutation_importance(est, X, y)`/`partial_dependence(est, X, features)`

## [04]-[IMPLEMENTATION_LAW]

[ESTIMATOR_TOPOLOGY]:
- protocol: every estimator derives from `base.BaseEstimator` and exposes `fit`/`get_params`/`set_params`/`set_output`/`set_*_request`; transformers add `transform`/`fit_transform`/`get_feature_names_out`, classifiers add `predict`/`predict_proba`/`decision_function`, regressors add `predict`, clusterers add `fit_predict`. Discriminate the task on mixin, never proliferate per-task method names.
- composition: classical models compose as one `pipeline.Pipeline` of `preprocessing`/`impute`/`decomposition`/`feature_selection` transformers and a final estimator; heterogeneous columns route through `compose.ColumnTransformer`; the whole graph clones with `base.clone` and round-trips hyperparameters through `get_params`/`set_params`.
- output discipline: `set_config(transform_output='pandas'/'polars')` (or per-step `set_output`) carries feature names through the pipeline so the ONNX `initial_types` and the model-asset receipt name columns consistently; `set_*_request` routes `sample_weight`/`groups` to the steps that consume them.
- selection: hyperparameters tune through `GridSearchCV`/`RandomizedSearchCV` (or successive-halving variants under the active enable import); generalization estimates come from `cross_validate`/`cross_val_score` over a task-appropriate splitter (`StratifiedKFold`, `GroupKFold`, `TimeSeriesSplit`); the `scoring=` callable is one `make_scorer`/`get_scorer` value reused across train and tune.
- scoring/inspection: classification and regression metrics live in `metrics`; probability calibration uses `CalibratedClassifierCV`; a fitted pipeline is explained with `inspection.permutation_importance`/`partial_dependence` for the model-asset evidence.
- graduation: the fitted `Pipeline` is the ONNX export source — `skl2onnx.to_onnx(pipeline, X, target_opset=...)` (`.api/skl2onnx.md`) with `initial_types` taken from the trained feature schema, validated through an `onnxruntime` session, and graduated through the `onnx`/`onnxruntime` model-asset checks.

[LOCAL_ADMISSION]:
- import: submodule imports at boundary scope; the active `enable_halving_search_cv`/`enable_iterative_imputer` gate import precedes the gated symbol's use.
- stacking: scipy backs every estimator (sparse `csr_array` inputs to linear/SVM, `scipy.stats` distributions for `RandomizedSearchCV` `param_distributions`); numeric tolerances and CV scores route as study evidence through `.api/scipy.md`.
- evidence: the model-asset receipt captures the pipeline composition (`get_params`), the cross-validation score (`cross_validate`), the inspection importances, and the ONNX export + `onnxruntime`-session check result.
- boundary: scikit-learn fitting is offline; production model serving and benchmark authority stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scikit-learn`
- Owns: offline classical-ML fitting, pipeline/column composition, decomposition and feature selection, probability calibration, model selection and inspection, scoring, and ONNX export sourcing for the model-asset rail
- Accept: a fitted `Pipeline`/`ColumnTransformer` with named-column output, a captured cross-validation score over a task-appropriate splitter, inspection importances, and an ONNX export + `onnxruntime` validation receipt
- Reject: production model serving; hand-rolled estimators, splitters, or metrics sklearn owns; per-task method-name proliferation over the uniform `fit`/`predict`/`transform`/`score` protocol; wrapper-renames of the estimator protocol
