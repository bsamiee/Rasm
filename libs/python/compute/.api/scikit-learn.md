# [PY_COMPUTE_API_SCIKIT_LEARN]

`scikit-learn` supplies the classical-ML estimator surface — preprocessing, supervised and unsupervised estimators, pipelines, and model selection — for the compute model-asset rail. The package owner composes a `Pipeline` of transformers and a final estimator, selects it through `model_selection`, fits it offline, and exports it to ONNX as a graduation candidate; it never re-implements an estimator the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-learn`
- package: `scikit-learn`
- import: `sklearn`; submodules `sklearn.base`, `sklearn.pipeline`, `sklearn.compose`, `sklearn.preprocessing`, `sklearn.model_selection`, `sklearn.linear_model`, `sklearn.ensemble`, `sklearn.cluster`, `sklearn.svm`, `sklearn.tree`, `sklearn.metrics`
- owner: `compute`
- rail: model
- manifest: pinned `scikit-learn>=1.9.0; python_version<'3.15'`
- capability: classical machine learning — estimators, transformers, pipelines, cross-validation, metrics, and a uniform `fit`/`predict`/`transform` protocol; `sklearn`-to-ONNX export feeds the model-asset rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: estimator-protocol roots and composition
- rail: model

| [INDEX] | [SYMBOL]                    | [PROTOCOL_ROLE]   | [PROTOCOL_METHODS]                |
| :-----: | :-------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `base.BaseEstimator`        | estimator root    | `get_params`, `set_params`        |
|  [02]   | `base.TransformerMixin`     | transformer mixin | `fit_transform`                   |
|  [03]   | `base.ClassifierMixin`      | classifier mixin  | `score`                           |
|  [04]   | `base.RegressorMixin`       | regressor mixin   | `score`                           |
|  [05]   | `base.ClusterMixin`         | clusterer mixin   | `fit_predict`                     |
|  [06]   | `pipeline.Pipeline`         | composition root  | chains transformers and estimator |
|  [07]   | `pipeline.FeatureUnion`     | composition root  | concatenates transformer outputs  |
|  [08]   | `compose.ColumnTransformer` | column router     | per-column-subset transformers    |

[PUBLIC_TYPE_SCOPE]: supervised estimators
- rail: model

| [INDEX] | [SYMBOL]                                        | [ESTIMATOR_FAMILY] | [TASK]                     |
| :-----: | :---------------------------------------------- | :----------------- | :------------------------- |
|  [01]   | `linear_model.LinearRegression`                 | linear             | regression                 |
|  [02]   | `linear_model.LogisticRegression`               | linear             | classification             |
|  [03]   | `linear_model.Ridge` \| `Lasso` \| `ElasticNet` | linear             | regularized regression     |
|  [04]   | `linear_model.SGDClassifier`                    | linear             | large-scale classification |
|  [05]   | `ensemble.RandomForestClassifier`               | bagging            | classification             |
|  [06]   | `ensemble.RandomForestRegressor`                | bagging            | regression                 |
|  [07]   | `ensemble.HistGradientBoostingClassifier`       | boosting           | classification             |
|  [08]   | `ensemble.GradientBoostingRegressor`            | boosting           | regression                 |
|  [09]   | `ensemble.StackingClassifier`                   | stacking           | meta-classification        |
|  [10]   | `ensemble.VotingClassifier`                     | voting             | ensemble classification    |
|  [11]   | `svm.SVC` \| `svm.SVR`                          | kernel             | classification/regression  |
|  [12]   | `tree.DecisionTreeClassifier`                   | tree               | classification             |

[PUBLIC_TYPE_SCOPE]: transformers and clusterers
- rail: model

| [INDEX] | [SYMBOL]                           | [SUBMODULE]     | [ROLE]                          |
| :-----: | :--------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `preprocessing.StandardScaler`     | `preprocessing` | zero-mean unit-variance scale   |
|  [02]   | `preprocessing.MinMaxScaler`       | `preprocessing` | range scaling                   |
|  [03]   | `preprocessing.RobustScaler`       | `preprocessing` | quantile-robust scaling         |
|  [04]   | `preprocessing.OneHotEncoder`      | `preprocessing` | categorical one-hot encoding    |
|  [05]   | `preprocessing.OrdinalEncoder`     | `preprocessing` | categorical ordinal encoding    |
|  [06]   | `preprocessing.PolynomialFeatures` | `preprocessing` | polynomial feature expansion    |
|  [07]   | `preprocessing.PowerTransformer`   | `preprocessing` | Gaussian-like transform         |
|  [08]   | `cluster.KMeans`                   | `cluster`       | centroid clustering             |
|  [09]   | `cluster.DBSCAN`                   | `cluster`       | density clustering              |
|  [10]   | `cluster.AgglomerativeClustering`  | `cluster`       | hierarchical clustering         |
|  [11]   | `cluster.HDBSCAN`                  | `cluster`       | hierarchical density clustering |
|  [12]   | `cluster.SpectralClustering`       | `cluster`       | graph-spectral clustering       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: uniform estimator protocol
- rail: model

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RESULT]                       |
| :-----: | :-------------------------------- | :------------- | :----------------------------- |
|  [01]   | `estimator.fit(X, y)`             | train          | the fitted estimator (`self`)  |
|  [02]   | `estimator.predict(X)`            | inference      | predicted labels or values     |
|  [03]   | `estimator.predict_proba(X)`      | inference      | class probabilities            |
|  [04]   | `transformer.transform(X)`        | transform      | transformed feature array      |
|  [05]   | `transformer.fit_transform(X, y)` | transform      | fit then transform in one call |
|  [06]   | `estimator.score(X, y)`           | evaluate       | default task score             |
|  [07]   | `estimator.get_params()`          | introspect     | hyperparameter dict            |
|  [08]   | `estimator.set_params(**params)`  | configure      | the reconfigured estimator     |

[ENTRYPOINT_SCOPE]: pipeline composition and model selection
- rail: model

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [RESULT]                           |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `pipeline.Pipeline(steps, memory)`                                           | compose        | a chained estimator                |
|  [02]   | `pipeline.make_pipeline(*steps)`                                             | compose        | a `Pipeline` with auto-named steps |
|  [03]   | `compose.ColumnTransformer(transformers)`                                    | compose        | a column-routed transformer        |
|  [04]   | `compose.make_column_transformer(*transformers)`                             | compose        | auto-named `ColumnTransformer`     |
|  [05]   | `model_selection.train_test_split(*arrays)`                                  | split          | train/test array partition         |
|  [06]   | `model_selection.cross_val_score(estimator, X, y, cv)`                       | evaluate       | cross-validation score array       |
|  [07]   | `model_selection.cross_validate(estimator, X, y, scoring)`                   | evaluate       | per-metric CV result dict          |
|  [08]   | `model_selection.GridSearchCV(estimator, param_grid, cv)`                    | tune           | refitted best-parameter estimator  |
|  [09]   | `model_selection.RandomizedSearchCV(estimator, param_distributions, n_iter)` | tune           | sampled-search estimator           |
|  [10]   | `model_selection.StratifiedKFold(n_splits, shuffle)`                         | split          | stratified CV splitter             |

[ENTRYPOINT_SCOPE]: scoring metrics
- rail: model

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RESULT]                        |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `metrics.accuracy_score(y_true, y_pred)`        | classification | fraction correct                |
|  [02]   | `metrics.f1_score(y_true, y_pred)`              | classification | harmonic precision/recall       |
|  [03]   | `metrics.roc_auc_score(y_true, y_score)`        | classification | area under ROC curve            |
|  [04]   | `metrics.confusion_matrix(y_true, y_pred)`      | classification | confusion matrix array          |
|  [05]   | `metrics.classification_report(y_true, y_pred)` | classification | per-class precision/recall text |
|  [06]   | `metrics.mean_squared_error(y_true, y_pred)`    | regression     | mean squared error              |
|  [07]   | `metrics.r2_score(y_true, y_pred)`              | regression     | coefficient of determination    |

## [04]-[IMPLEMENTATION_LAW]

[ESTIMATOR_TOPOLOGY]:
- protocol: every estimator derives from `base.BaseEstimator` and exposes `fit`/`get_params`/`set_params`; transformers add `transform`/`fit_transform`, classifiers and regressors add `predict`/`score`, clusterers add `fit_predict`.
- composition: classical models compose as a `pipeline.Pipeline` of `preprocessing` transformers and a final estimator; heterogeneous columns route through `compose.ColumnTransformer`.
- selection: hyperparameters tune through `model_selection.GridSearchCV` or `RandomizedSearchCV`; generalization estimates come from `cross_val_score`/`cross_validate` over a `StratifiedKFold` or `KFold` splitter.
- scoring: classification metrics (`accuracy_score`, `f1_score`, `roc_auc_score`) and regression metrics (`mean_squared_error`, `r2_score`) live in `metrics`.
- graduation: a fitted pipeline exports to ONNX (`skl2onnx` route) and graduates through the `onnx`/`onnxruntime` model-asset checks.

[LOCAL_ADMISSION]:
- fitting: classical models compose as a `Pipeline` of `preprocessing` transformers and a final estimator, selected through `model_selection`.
- evidence: the model-asset receipt captures the pipeline composition, the cross-validation score, and the ONNX export result.
- boundary: scikit-learn fitting is offline; production model serving and benchmark authority stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scikit-learn`
- Owns: offline classical-ML fitting, pipeline composition, model selection, scoring, and ONNX export for the model-asset rail
- Accept: a fitted `Pipeline` with a captured cross-validation score and ONNX export receipt
- Reject: production model serving; hand-rolled estimators sklearn owns; wrapper-renames of the `fit`/`predict`/`transform` protocol
