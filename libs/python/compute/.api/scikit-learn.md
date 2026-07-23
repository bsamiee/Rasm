# [PY_COMPUTE_API_SCIKIT_LEARN]

`scikit-learn` owns classical-ML fitting for the compute model-asset rail: a `Pipeline`/`ColumnTransformer` of transformers and a final estimator, tuned through `model_selection`, scored through `metrics`, fit offline, and exported to ONNX (`.api/skl2onnx.md`) as the graduation candidate. Every estimator obeys one `fit`/`predict`/`transform`/`score` protocol the mixin discriminates, so a new task is a mixin, never a per-task method family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-learn`
- package: `scikit-learn`
- import: `sklearn`; submodules `sklearn.base`, `sklearn.pipeline`, `sklearn.compose`, `sklearn.preprocessing`, `sklearn.impute`, `sklearn.model_selection`, `sklearn.linear_model`, `sklearn.ensemble`, `sklearn.svm`, `sklearn.tree`, `sklearn.decomposition`, `sklearn.feature_selection`, `sklearn.calibration`, `sklearn.inspection`, `sklearn.metrics`; the `from sklearn.experimental import enable_halving_search_cv`/`enable_iterative_imputer` gate import precedes the gated symbol
- owner: `compute`
- rail: model
- capability: one `fit`/`predict`/`transform`/`score` estimator protocol with metadata routing and DataFrame/array output configuration; the fitted pipeline is the ONNX export source for the model-asset rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: estimator-protocol roots and composition

Every estimator derives from `BaseEstimator`; a mixin adds the task verbs, and a composition root chains or routes steps.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]       | [CAPABILITY]                               |
| :-----: | :----------------------------------- | :------------------ | :----------------------------------------- |
|  [01]   | `base.BaseEstimator`                 | estimator root      | param, output, and metadata-request config |
|  [02]   | `base.TransformerMixin`              | transformer mixin   | adds `transform`/`get_feature_names_out`   |
|  [03]   | `base.ClassifierMixin`               | classifier mixin    | adds `predict`; `score` is accuracy        |
|  [04]   | `base.RegressorMixin`                | regressor mixin     | adds `predict`; `score` is R^2             |
|  [05]   | `base.ClusterMixin`                  | clusterer mixin     | adds `fit_predict`                         |
|  [06]   | `base.clone`                         | estimator copy      | unfitted deep copy of hyperparameters      |
|  [07]   | `pipeline.Pipeline`                  | composition root    | chains transformers and a final estimator  |
|  [08]   | `pipeline.FeatureUnion`              | composition root    | concatenates transformer outputs           |
|  [09]   | `pipeline.FunctionTransformer`       | stateless transform | wraps a callable as a step                 |
|  [10]   | `compose.ColumnTransformer`          | column router       | per-subset transformers with `remainder`   |
|  [11]   | `compose.TransformedTargetRegressor` | target wrap         | transforms `y`, inverts on predict         |

[PUBLIC_TYPE_SCOPE]: supervised estimators

Each family pairs a `Classifier`/`Regressor` where both exist, importing from `linear_model`/`ensemble`/`svm`/`tree`/`calibration`.

[LINEAR]: `LinearRegression` `LogisticRegression`(`predict_proba`) `Ridge` `Lasso` `ElasticNet` `SGDClassifier` `SGDRegressor`
[BAGGING]: `RandomForestClassifier` `RandomForestRegressor`
[BOOSTING]: `HistGradientBoostingClassifier` `HistGradientBoostingRegressor`(native categorical + missing) `GradientBoostingRegressor`
[ENSEMBLE_META]: `StackingClassifier` `StackingRegressor` `VotingClassifier` `VotingRegressor`
[KERNEL]: `SVC` `SVR`
[TREE]: `DecisionTreeClassifier` `DecisionTreeRegressor`
[CALIBRATION]: `CalibratedClassifierCV`(sigmoid/isotonic probability calibration)

[PUBLIC_TYPE_SCOPE]: transformers, decomposition, feature selection, and clusterers

[SCALING] (`preprocessing`): `StandardScaler` `MinMaxScaler` `RobustScaler` `MaxAbsScaler`
[ENCODING] (`preprocessing`): `OneHotEncoder` `OrdinalEncoder` `TargetEncoder`
[EXPANSION] (`preprocessing`): `PolynomialFeatures` `SplineTransformer`
[DISTRIBUTION] (`preprocessing`): `PowerTransformer` `QuantileTransformer` `KBinsDiscretizer`
[IMPUTATION] (`impute`): `SimpleImputer` `KNNImputer` `IterativeImputer`
[DECOMPOSITION] (`decomposition`): `PCA` `TruncatedSVD` `KernelPCA` `NMF`
[FEATURE_SELECTION] (`feature_selection`): `SelectKBest` `SelectFromModel` `RFE` `RFECV`
[CLUSTER_CENTROID] (`cluster`): `KMeans` `MiniBatchKMeans`
[CLUSTER_DENSITY] (`cluster`): `DBSCAN` `HDBSCAN`
[CLUSTER_HIERARCHICAL] (`cluster`): `AgglomerativeClustering` `SpectralClustering`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: uniform estimator protocol

One protocol spans every estimator; the mixin selects the verb, and `set_output` makes any transformer emit a named-column frame.

| [INDEX] | [SURFACE]                                    | [SHAPE]    | [CAPABILITY]                                     |
| :-----: | :------------------------------------------- | :--------- | :----------------------------------------------- |
|  [01]   | `estimator.fit(X, y, **params)`              | train      | fitted estimator (`self`)                        |
|  [02]   | `estimator.predict(X)`                       | inference  | predicted labels or values                       |
|  [03]   | `estimator.predict_proba(X)`                 | inference  | class probabilities                              |
|  [04]   | `estimator.decision_function(X)`             | inference  | signed class margins                             |
|  [05]   | `transformer.transform(X)`                   | transform  | transformed array (frame if `set_output`)        |
|  [06]   | `transformer.fit_transform(X, y)`            | transform  | fit then transform                               |
|  [07]   | `estimator.score(X, y)`                      | evaluate   | default task score (accuracy / R^2)              |
|  [08]   | `estimator.get_params()` / `set_params(**p)` | configure  | hyperparameter dict / reconfigured estimator     |
|  [09]   | `estimator.set_output(transform='pandas')`   | output     | route transform output to a DataFrame backend    |
|  [10]   | `estimator.set_fit_request(sample_weight=…)` | metadata   | per-step metadata propagation through a pipeline |
|  [11]   | `transformer.get_feature_names_out()`        | introspect | output feature names for downstream provenance   |

[ENTRYPOINT_SCOPE]: pipeline composition and model selection

`Pipeline`/`ColumnTransformer` compose; CV entries take `(est, X, y, cv=, scoring=)` and searches take `param_grid`/`param_distributions`.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `Pipeline(steps, memory)`                        | ctor     | chained estimator                            |
|  [02]   | `FeatureUnion(...)`                              | ctor     | concatenated transformer outputs             |
|  [03]   | `FunctionTransformer(func)`                      | ctor     | stateless callable step                      |
|  [04]   | `ColumnTransformer(transformers, remainder)`     | ctor     | column-routed transformer                    |
|  [05]   | `train_test_split(*arrays, stratify, test_size)` | function | train/test array partition                   |
|  [06]   | `cross_val_score(est, X, y, cv, scoring)`        | function | CV score array                               |
|  [07]   | `cross_validate(...)`                            | function | per-metric result dict (`return_estimator=`) |
|  [08]   | `cross_val_predict(est, X, y, cv, method)`       | function | out-of-fold predictions                      |

[TUNE]: `GridSearchCV`(exhaustive) `RandomizedSearchCV`(sampled, `n_iter`) `HalvingGridSearchCV` `HalvingRandomSearchCV`(successive-halving, enable-gated) — refit the best-parameter estimator over `param_grid`/`param_distributions`
[DIAGNOSE]: `learning_curve` `validation_curve` `permutation_test_score`
[SPLITTERS] (as `cv=`): `StratifiedKFold` `KFold` `GroupKFold` `StratifiedGroupKFold` `TimeSeriesSplit` `RepeatedStratifiedKFold` `ShuffleSplit` `LeaveOneOut`
[FACTORIES] (auto-naming): `make_pipeline` `make_union` `make_column_transformer`

[ENTRYPOINT_SCOPE]: scoring, calibration, and inspection

`make_scorer`/`get_scorer` produce the `scoring=` callable every CV entry reuses; `permutation_importance`/`partial_dependence` explain a fitted pipeline. Rosters live in `metrics` unless noted.

[CLASS_SCORES]: `accuracy_score` `balanced_accuracy_score` `f1_score` `matthews_corrcoef`
[CLASS_RANKING]: `roc_auc_score` `average_precision_score` `log_loss`
[CLASS_CURVES]: `roc_curve` `precision_recall_curve` `confusion_matrix` `classification_report`
[REGRESSION]: `mean_squared_error` `root_mean_squared_error` `mean_absolute_error` `mean_absolute_percentage_error` `r2_score` `mean_pinball_loss` `d2_absolute_error_score`
[CLUSTERING]: `silhouette_score`
[SCORER_FACTORY]: `make_scorer(score_func, *, greater_is_better)` `get_scorer(name)` `get_scorer_names()`
[INSPECTION] (`inspection`): `permutation_importance(est, X, y)` `partial_dependence(est, X, features)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- protocol: every estimator derives from `base.BaseEstimator` exposing `fit`/`get_params`/`set_params`/`set_output`/`set_*_request`; transformers add `transform`/`fit_transform`/`get_feature_names_out`, classifiers add `predict`/`predict_proba`/`decision_function`, regressors add `predict`, clusterers add `fit_predict` — the mixin discriminates the task.
- composition: a model composes as one `Pipeline` of `preprocessing`/`impute`/`decomposition`/`feature_selection` steps and a final estimator; heterogeneous columns route through `compose.ColumnTransformer`; the graph clones with `base.clone` and round-trips through `get_params`/`set_params`.
- output: `set_config(transform_output='pandas'|'polars')` or per-step `set_output` carries feature names through the pipeline so the ONNX `initial_types` and the receipt name columns consistently; `set_*_request` routes `sample_weight`/`groups` to the consuming steps.
- selection: hyperparameters tune through `GridSearchCV`/`RandomizedSearchCV` (or halving variants under the enable import) over a task splitter (`StratifiedKFold`, `GroupKFold`, `TimeSeriesSplit`); one `make_scorer`/`get_scorer` value is the `scoring=` reused across train and tune.

[STACKING]:
- `skl2onnx`(`.api/skl2onnx.md`): the fitted `Pipeline` is the export source — `to_onnx(pipeline, X, target_opset=...)` with `initial_types` from the trained `get_feature_names_out()` schema, gated at or below `get_latest_tested_opset_version()`.
- `onnx`(`.api/onnx.md`) / `onnxruntime`(`.api/onnxruntime.md`): the exported `ModelProto` graduates through the structural `checker` and an `InferenceSession` parity `run` against `predict`/`predict_proba`.
- `scipy`(`.api/scipy.md`): sparse `scipy.sparse.csr_array` feeds linear and SVM estimators, and `scipy.stats` distributions supply `RandomizedSearchCV` `param_distributions`.
- `ModelAsset`: folds one fitted pipeline — composition (`get_params`), the `cross_validate` score, inspection importances, and the ONNX export with its `onnxruntime`-session check — into the compute model-asset receipt.

[LOCAL_ADMISSION]:
- submodule imports at boundary scope; the `enable_halving_search_cv`/`enable_iterative_imputer` gate import precedes the gated symbol's use.
- classical fitting is offline; production model serving and benchmark authority stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scikit-learn`
- Owns: offline classical-ML fitting, pipeline and column composition, decomposition and feature selection, probability calibration, model selection and inspection, scoring, and ONNX export sourcing for the model-asset rail
- Accept: a fitted `Pipeline`/`ColumnTransformer` with named-column output, a `cross_validate` score over a task-appropriate splitter, inspection importances, and an ONNX export plus `onnxruntime` validation receipt
- Reject: production model serving; hand-rolled estimators, splitters, or metrics sklearn owns; per-task method-name proliferation over the uniform `fit`/`predict`/`transform`/`score` protocol
