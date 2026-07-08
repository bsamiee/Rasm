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

| [INDEX] | [SYMBOL] | [PROTOCOL_ROLE] | [PROTOCOL_METHODS] |
| --- | --- | --- | --- |
| [01] | `base.BaseEstimator` | estimator root | `get_params`, `set_params`, `set_output`, `set_*_request` |
| [02] | `base.TransformerMixin` | transformer mixin | `fit_transform`, `get_feature_names_out` |
| [03] | `base.ClassifierMixin` | classifier mixin | `score` (accuracy) |
| [04] | `base.RegressorMixin` | regressor mixin | `score` (R^2) |
| [05] | `base.ClusterMixin` | clusterer mixin | `fit_predict` |
| [06] | `base.clone` | estimator copy | unfitted deep copy preserving hyperparameters |
| [07] | `pipeline.Pipeline` | composition root | chains transformers + final estimator (`steps`, `transform_input`, `memory`) |
| [08] | `pipeline.FeatureUnion` | composition root | concatenates transformer outputs |
| [09] | `pipeline.FunctionTransformer` | stateless transform | wraps a callable as a transformer for a pipeline step |
| [10] | `compose.ColumnTransformer` | column router | per-column-subset transformers with `remainder` passthrough |
| [11] | `compose.TransformedTargetRegressor` | target wrap | transforms `y` before fit, inverts on predict |

[PUBLIC_TYPE_SCOPE]: supervised estimators
- rail: model

| [INDEX] | [SYMBOL] | [ESTIMATOR_FAMILY] | [TASK] |
| --- | --- | --- | --- |
| [01] | `linear_model.LinearRegression` | linear | regression |
| [02] | `linear_model.LogisticRegression` | linear | classification (`predict_proba`) |
| [03] | `linear_model.Ridge` \| `Lasso` \| `ElasticNet` | linear | regularized regression |
| [04] | `linear_model.SGDClassifier` \| `SGDRegressor` | linear | large-scale online learning |
| [05] | `ensemble.RandomForestClassifier` \| `RandomForestRegressor` | bagging | classification / regression |
| [06] | `ensemble.HistGradientBoostingClassifier` \| `HistGradientBoostingRegressor` | boosting | fast histogram gradient boosting (native categorical + missing) |
| [07] | `ensemble.GradientBoostingRegressor` | boosting | regression |
| [08] | `ensemble.StackingClassifier` \| `StackingRegressor` | stacking | meta-estimator over base learners |
| [09] | `ensemble.VotingClassifier` \| `VotingRegressor` | voting | hard/soft ensemble vote |
| [10] | `svm.SVC` \| `svm.SVR` | kernel | classification / regression |
| [11] | `tree.DecisionTreeClassifier` \| `DecisionTreeRegressor` | tree | single-tree classification / regression |
| [12] | `calibration.CalibratedClassifierCV` | probability calib | sigmoid/isotonic calibration of a base classifier's probabilities |

[PUBLIC_TYPE_SCOPE]: transformers, decomposition, feature selection, and clusterers
- rail: model

| [INDEX] | [SYMBOL] | [SUBMODULE] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `preprocessing.StandardScaler` \| `MinMaxScaler` \| `RobustScaler` \| `MaxAbsScaler` | `preprocessing` | center/range/quantile-robust scaling |
| [02] | `preprocessing.OneHotEncoder` \| `OrdinalEncoder` \| `TargetEncoder` | `preprocessing` | categorical one-hot / ordinal / target encoding |
| [03] | `preprocessing.PolynomialFeatures` \| `SplineTransformer` | `preprocessing` | polynomial / B-spline feature expansion |
| [04] | `preprocessing.PowerTransformer` \| `QuantileTransformer` \| `KBinsDiscretizer` | `preprocessing` | Gaussian-like / quantile-uniform transform / binning |
| [05] | `impute.SimpleImputer` \| `KNNImputer` \| `IterativeImputer` | `impute` | missing-value imputation |
| [06] | `decomposition.PCA` \| `TruncatedSVD` \| `KernelPCA` \| `NMF` | `decomposition` | linear / sparse / kernel / nonnegative dimensionality reduction |
| [07] | `feature_selection.SelectKBest` \| `SelectFromModel` \| `RFE` \| `RFECV` | `feature_selection` | univariate / model-based / recursive feature elimination |
| [08] | `cluster.KMeans` \| `MiniBatchKMeans` | `cluster` | centroid clustering |
| [09] | `cluster.DBSCAN` \| `HDBSCAN` | `cluster` | density / hierarchical-density clustering |
| [10] | `cluster.AgglomerativeClustering` \| `SpectralClustering` | `cluster` | hierarchical / graph-spectral clustering |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: uniform estimator protocol
- rail: model

One protocol spans every estimator; the verb is discriminated by mixin (transformer `transform`, classifier `predict`/`predict_proba`/`decision_function`, regressor `predict`, clusterer `fit_predict`). `set_output(transform='pandas'/'polars')` makes any transformer emit a named-column frame.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RESULT] |
| --- | --- | --- | --- |
| [01] | `estimator.fit(X, y, **fit_params)` | train | the fitted estimator (`self`) |
| [02] | `estimator.predict(X)` | inference | predicted labels or values |
| [03] | `estimator.predict_proba(X)` \| `decision_function(X)` | inference | class probabilities / signed margins |
| [04] | `transformer.transform(X)` \| `fit_transform(X, y)` | transform | transformed feature array (frame if `set_output`) |
| [05] | `estimator.score(X, y)` | evaluate | default task score (accuracy / R^2) |
| [06] | `estimator.get_params()` \| `set_params(**p)` | configure | hyperparameter dict / reconfigured estimator |
| [07] | `estimator.set_output(transform='pandas')` | output config | route transform output to a DataFrame backend |
| [08] | `estimator.set_fit_request(sample_weight=True)` | metadata routing | enable per-step metadata propagation through a pipeline |
| [09] | `transformer.get_feature_names_out()` | introspect | output feature names for downstream provenance |

[ENTRYPOINT_SCOPE]: pipeline composition and model selection
- rail: model

`Pipeline`/`ColumnTransformer` are the canonical composition roots; tuning discriminates exhaustive (`GridSearchCV`) vs sampled (`RandomizedSearchCV`) vs successive-halving (`HalvingGridSearchCV`/`HalvingRandomSearchCV`, gated by the active enable import). Splitters are passed as `cv=`.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RESULT] |
| --- | --- | --- | --- |
| [01] | `pipeline.Pipeline(steps, memory)` \| `make_pipeline(*steps)` | compose | chained estimator (auto-named for `make_pipeline`) |
| [02] | `pipeline.FeatureUnion(...)` \| `make_union(*t)` \| `FunctionTransformer(func)` | compose | concatenated / stateless transform steps |
| [03] | `compose.ColumnTransformer(transformers, remainder)` \| `make_column_transformer(*t)` | compose | column-routed transformer |
| [04] | `model_selection.train_test_split(*arrays, stratify, test_size)` | split | train/test array partition |
| [05] | `model_selection.cross_val_score(est, X, y, cv, scoring)` \| `cross_validate(est, X, y, scoring, return_estimator)` | evaluate | CV score array / per-metric result dict |
| [06] | `model_selection.cross_val_predict(est, X, y, cv, method)` | evaluate | out-of-fold predictions |
| [07] | `model_selection.GridSearchCV(est, param_grid, cv, scoring, refit)` \| `RandomizedSearchCV(est, param_distributions, n_iter)` | tune | refitted best-parameter estimator |
| [08] | `model_selection.HalvingGridSearchCV(...)` \| `HalvingRandomSearchCV(...)` | tune | successive-halving tuned estimator (experimental-gated) |
| [09] | `model_selection.StratifiedKFold(n_splits, shuffle)` \| `KFold` \| `GroupKFold` \| `StratifiedGroupKFold` \| `TimeSeriesSplit` \| `RepeatedStratifiedKFold` \| `ShuffleSplit` \| `LeaveOneOut` | split | CV splitter passed as `cv=` |
| [10] | `model_selection.learning_curve(...)` \| `validation_curve(...)` \| `permutation_test_score(...)` | diagnose | learning/validation curve, label-permutation significance |

[ENTRYPOINT_SCOPE]: scoring, calibration, and inspection
- rail: model

`make_scorer`/`get_scorer` produce the `scoring=` callable used by every CV entry; `permutation_importance` and `partial_dependence` explain a fitted pipeline.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RESULT] |
| --- | --- | --- | --- |
| [01] | `metrics.accuracy_score` \| `balanced_accuracy_score` \| `f1_score` \| `matthews_corrcoef` | classification | scalar classification scores |
| [02] | `metrics.roc_auc_score` \| `average_precision_score` \| `log_loss` | classification | probabilistic ranking / loss scores |
| [03] | `metrics.roc_curve` \| `precision_recall_curve` \| `confusion_matrix` \| `classification_report` | classification | curve arrays / matrix / per-class report |
| [04] | `metrics.mean_squared_error` \| `root_mean_squared_error` \| `mean_absolute_error` \| `mean_absolute_percentage_error` \| `r2_score` \| `mean_pinball_loss` \| `d2_absolute_error_score` | regression | regression error / determination scores |
| [05] | `metrics.silhouette_score` | clustering | cluster cohesion/separation score |
| [06] | `metrics.make_scorer(score_func, greater_is_better)` \| `get_scorer(name)` \| `get_scorer_names()` | scorer factory | `scoring=` callable for CV / search |
| [07] | `inspection.permutation_importance(est, X, y)` \| `partial_dependence(est, X, features)` | inspection | feature-importance / partial-dependence result |

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
