# [pytest] — the spec-run substrate every Python lane and gauge stands on

`pytest` is the collector, fixture, marker, config, and hook engine the whole `tests/python` estate composes on: the `_testkit` runtime plugin binds Hypothesis profiles and auto-markers through its hooks, the `spec.py` matrix folds report through its core `subtests` fixture, and every lane — unit, network, subprocess, benchmark, mutation — is a marker selection over one session. The parallel, ordering, and wall-clock lanes are sibling rails: `.api/pytest-xdist.md`, `.api/pytest-randomly.md`, `.api/pytest-timeout.md`.

## [01]-[PACKAGE_SURFACE]

- package: `pytest` · version `9.1.1` · license `MIT`
- namespace: `import pytest` — one flat barrel; internals live under `_pytest.*` but the public surface is re-exported on `pytest`.
- asset: dist `pytest`; console scripts `pytest` and `py.test` (`_pytest.config:_console_main`); the plugin protocol group is `pytest11`.
- rail: spec execution — the session every folder's specs and every gauge terminate in.

## [02]-[PUBLIC_TYPES]

The config, item, and hook objects the `_testkit` plugins and per-suite conftests implement against.

| [INDEX] | [SYMBOL]                                                | [KIND]                  | [CAPABILITY]                                                                                                |
| :-----: | :------------------------------------------------------ | :---------------------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `pytest.Config`                                         | config object           | resolved session config; `getoption`/`getini`/`pluginmanager`/`stash` read the run policy                   |
|  [02]   | `pytest.Parser`                                         | option parser           | `pytest_addoption` binds CLI options and `addini` config keys through it                                    |
|  [03]   | `pytest.Item` · `pytest.Function`                       | collected test          | one runnable node; `add_marker`/`get_closest_marker`/`fixturenames`/`nodeid` drive marker and fixture logic |
|  [04]   | `pytest.Session` · `pytest.Collector` · `pytest.Module` | collection tree         | the session root and its module/collector nodes walked at collection                                        |
|  [05]   | `pytest.Metafunc`                                       | parametrizer            | `pytest_generate_tests` calls `metafunc.parametrize` to expand a test                                       |
|  [06]   | `pytest.Stash` · `pytest.StashKey`                      | typed side-channel      | plugin-owned per-config/item state keyed without collision                                                  |
|  [07]   | `pytest.MonkeyPatch`                                    | patch context           | `setattr`/`setenv`/`setitem`/`delenv`/`chdir`/`context`/`undo` — auto-reverted seam owner                   |
|  [08]   | `pytest.CaptureFixture[AnyStr]`                         | capture handle          | `readouterr()` returns the captured `out`/`err` pair; `capsysbinary` yields the `bytes` variant             |
|  [09]   | `pytest.FixtureRequest` · `pytest.TempPathFactory`      | fixture context         | `request.node`/`getfixturevalue`; the factory backs `tmp_path`                                              |
|  [10]   | `pytest.Subtests`                                       | subtest scope           | the core `subtests` fixture type — `RowCarrier` satisfier for the matrix folds                              |
|  [11]   | `pytest.Mark` · `pytest.MarkDecorator`                  | marker value            | the applied-marker object and its decorator; `get_closest_marker` returns a `Mark`                          |
|  [12]   | `pytest.ExceptionInfo[E]` · `pytest.RaisesExc`          | raised-exception handle | `pytest.raises` yields it; `.value`/`.type`/`.match` interrogate the caught error                           |
|  [13]   | `pytest.PytestPluginManager` · `pytest.hookimpl`        | plugin protocol         | `config.pluginmanager` registers hooks; `hookimpl` marks a hook's ordering                                  |
|  [14]   | `pytest.LogCaptureFixture`                              | log capture             | the `caplog` handle over the stdlib logging channel                                                         |
|  [15]   | `pytest.ExitCode`                                       | session verdict         | the run's terminal status enum                                                                              |

```python contract
class Config:
    def getoption(self, name: str, default: object = ..., skip: bool = False) -> object: ...
    def getini(self, name: str) -> object: ...
    @property
    def pluginmanager(self) -> PytestPluginManager: ...
    @property
    def stash(self) -> Stash: ...
class Item:
    def add_marker(self, marker: str | MarkDecorator, append: bool = True) -> None: ...
    def get_closest_marker(self, name: str, default: Mark | None = None) -> Mark | None: ...
class Metafunc:
    def parametrize(self, argnames, argvalues, indirect=False, ids=None, scope=None) -> None: ...
class Subtests:
    def test(self, msg: str | None = None, **kwargs: object) -> AbstractContextManager[None]: ...
```

## [03]-[ENTRYPOINTS]

Collection, assertion, and parametrization surface — the API specs and folds call directly.

| [INDEX] | [SURFACE]                                                              | [KIND]              | [CAPABILITY]                                                                                                                                                 |
| :-----: | :--------------------------------------------------------------------- | :------------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `pytest.fixture(*, scope, params, autouse, ids, name)`                 | fixture decorator   | declares a fixture; `scope` spans `function`..`session`                                                                                                      |
|  [02]   | `pytest.mark.<name>`                                                   | marker factory      | attaches a closed marker (`benchmark`/`mutation`/`network`/`property`/`subprocess`) or builtin `skipif`/`xfail`/`parametrize`/`usefixtures`/`filterwarnings` |
|  [03]   | `pytest.param(*values, marks, id)`                                     | parametrization row | one case in a `parametrize` set carrying per-row marks and id                                                                                                |
|  [04]   | `pytest.raises(expected, *, match)`                                    | exception assertion | context manager yielding `ExceptionInfo`; `match` regex-checks the message                                                                                   |
|  [05]   | `pytest.warns` · `pytest.deprecated_call`                              | warning assertion   | assert a warning under `filterwarnings = ["error"]`                                                                                                          |
|  [06]   | `pytest.approx(expected, rel, abs)`                                    | tolerance compare   | numeric proximity assertion                                                                                                                                  |
|  [07]   | `pytest.skip` · `pytest.fail` · `pytest.xfail` · `pytest.importorskip` | flow verdict        | terminate a test/collection with a typed outcome                                                                                                             |
|  [08]   | `subtests.test(msg=None, **kwargs)`                                    | row scope           | core fixture; a failing row reports independently instead of stopping the fold                                                                               |

```python contract
def fixture(fixture_function=None, *, scope="function", params=None, autouse=False,
            ids=None, name=None): ...
def param(*values: object, marks: MarkDecorator | Collection[MarkDecorator | Mark] = (),
          id: str | None = None) -> ParameterSet: ...
def raises(expected_exception=None, *args, **kwargs) -> RaisesExc | ExceptionInfo: ...  # match= regex kwarg
def skip(reason: str = "", allow_module_level: bool = False) -> NoReturn: ...
def importorskip(modname: str, minversion: str | None = None, reason: str | None = None, *, exc_type: type[ImportError] | None = None) -> ModuleType: ...
```

The `_testkit` plugins implement this hook surface; each signature is the kit's declared subset of the full hookspec — pluggy passes a hook argument only when the implementation names it.

```python contract
def pytest_addoption(parser: Parser) -> None: ...                        # bind CLI options + addini config keys
def pytest_configure(config: Config) -> None: ...                        # register plugins once config resolves
def pytest_collection_modifyitems(items: list[Item]) -> None: ...        # auto-apply markers, consume COVERS
def pytest_runtest_setup(item: Item) -> None: ...                        # per-test seam isolation
def pytest_runtest_teardown(item: Item, nextitem: Item | None) -> None: ...
def pytest_sessionstart(session: Session) -> None: ...                   # optional CPU sampler start
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_TOPOLOGY]:
- `[tool.pytest]` is the native TOML table (pytest 9): `minversion = "9.0"`, `testpaths`, `pythonpath`, `python_files`, `python_functions`, `cache_dir = ".cache/pytest"`, `collect_imported_tests = false`, `empty_parameter_set_mark = "fail_at_collect"`.
- `addopts` composes the default lane: `-p tests.python._testkit.runtime` loads the runtime plugin before config, `-m "not benchmark"` deselects the benchmark lane, `--disable-socket`/`--allow-unix-socket` bind pytest-socket, `--benchmark-storage=file://.artifacts/python/benchmarks`/`--benchmark-autosave` pin the measurement series, `--import-mode=importlib`, `--hypothesis-profile=rasm`, `-x`, `--tb=short`.
- `filterwarnings = ["error"]` makes every unhandled warning a failure; the single allow row exempts pytest-benchmark's deselected-session autosave no-op.
- `required_plugins` guards the session: `anyio`, `hypothesis`, `inline-snapshot`, `pytest-benchmark`, `pytest-cov`, `pytest-randomly`, `pytest-socket`, `pytest-timeout`, `pytest-xdist` must all load or the session aborts.
- `collect_imported_tests = false` stops collecting tests imported into a module rather than defined there; `empty_parameter_set_mark = "fail_at_collect"` fails an empty `parametrize` set at collection instead of silently skipping.
- `--import-mode=importlib` collects without mutating `sys.path`, so `pythonpath = ["."]` is the only import anchor and suites import by fully-qualified package path.
- The closed marker set is declared in `markers`: `benchmark`, `mutation`, `network`, `property`, `subprocess`; `strict = true` rejects any undeclared marker.
- The runtime plugin implements `pytest_configure` (registers the bench regression hook when pytest-benchmark loads), `pytest_collection_modifyitems` (auto-applies `network` from `socket_enabled` membership and `property` from `is_hypothesis_test`, then consumes each module's `COVERS`), and `pytest_sessionstart` (optional CPU sampler); the assay conftest adds `pytest_runtest_setup`/`pytest_runtest_teardown` to isolate SUT `ContextVar`s per test.
- The assay `cli` fixture composes `capsysbinary` (the `bytes` `CaptureFixture` variant), `monkeypatch`, and `request` to run the CLI in-process and decode the stdout envelope; `tmp_path` (backed by `TempPathFactory`) roots the isolated harness so filesystem laws never touch the repo tree.
- `otel_spans` and `log_events` are the runtime plugin's observability fixtures — an `InMemorySpanExporter` cleared per test and a structlog event list — so a span or log assertion reads structured records, never scraped stdout.

[STACKING]:
- `runtime.py`(`../_testkit/runtime.py`): loaded via `-p tests.python._testkit.runtime`; registers the `rasm*` Hypothesis profile family (`.api/hypothesis.md` carries the roster) and provides `otel_spans`/`log_events` fixtures.
- `laws.py`(`../_testkit/laws.py`): `@spec` and `COVERS` fold into `MANIFEST`; `pytest_collection_modifyitems` calls `consume_covers` per collected module.
- `spec.py`(`../_testkit/spec.py`): `validity_matrix`/`projection_matrix`/`support_matrix` accept `subtests: RowCarrier | None`; the core `pytest.Subtests` fixture satisfies `RowCarrier`, so a breached row reports independently.
- `pytest-xdist`(`.api/pytest-xdist.md`): the parallel lane, excluded from default `addopts`.
- `pytest-randomly`(`.api/pytest-randomly.md`): the order/seed lane, active by default, waived under mutmut.
- `pytest-timeout`(`.api/pytest-timeout.md`): the wall-clock lane, `timeout = "30"`.

[LOCAL_ADMISSION]:
- `pytest` is admitted on the `tests/` dev plane in `[dependency-groups] dev`; no `libs/python` runtime graph imports it.
- Specs reach construction and projection through `@spec`-injected `resolve(subject)` strategies, never a raw `test` free of the law registration.
- Per-suite conftests compose fixtures and seams only; SUT registration lives in the root `conftest.py` via `register_tree`, and a tool suite registers in its own conftest via `register_sut`.
- A new capability extends the owning `_testkit` module, never a helper file; the matrix folds take `subtests` as an optional row carrier so a single spec drives the whole fault table.

[RAIL_LAW]:
- Package: `pytest`
- Owns: the session, collection, fixture graph, marker taxonomy, `Config`/`Item`/hook protocol, capture and monkeypatch seams, `pytest.raises`/`approx`/`warns` assertions, `pytest.param` parametrization, and the core `subtests` fixture.
- Accept: `[tool.pytest]` native TOML config; the closed marker set under `strict = true`; `required_plugins` as the load guard; `subtests` folds through the `RowCarrier` protocol; `MonkeyPatch` for auto-reverted seams.
- Reject: an undeclared marker (`strict` fails collection); a raw `test` bypassing `@spec`/`COVERS` registration; an unhandled warning under `filterwarnings = ["error"]`; parallel or timeout policy restated here — `.api/pytest-xdist.md` and `.api/pytest-timeout.md` own those laws.
