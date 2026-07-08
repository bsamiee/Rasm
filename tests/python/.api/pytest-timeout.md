# [pytest-timeout] — the per-test wall-clock guard

`pytest-timeout` bounds each test's wall-clock duration and dumps every thread's stack on breach, converting a hung test or runaway loop into a deterministic failure with a diagnostic trace. It owns the real-time ceiling the Hypothesis lane deliberately cedes: the `rasm` profiles set `deadline=None`, so per-example timing is off and this plugin is the sole wall-clock authority over a whole property test.

## [01]-[PACKAGE_SURFACE]

- package: `pytest-timeout` · version `2.4.0` · license `MIT`
- namespace: `import pytest_timeout` — a single-module plugin.
- asset: dist `pytest-timeout`; `pytest11` entry point `timeout = pytest_timeout` (disable with `-p no:timeout`).
- rail: per-test wall-clock enforcement — the hang and runaway-loop guard.

## [02]-[PUBLIC_TYPES]

The resolved per-item settings and the diagnostic dump the plugin emits on breach.

| [INDEX] | [SYMBOL]                        | [KIND]          | [CAPABILITY]                                                                                         |
| :-----: | :------------------------------ | :-------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `pytest_timeout.Settings`       | settings tuple  | fields `timeout`, `method`, `func_only`, `disable_debugger_detection` — the resolved per-item policy |
|  [02]   | `pytest_timeout.DEFAULT_METHOD` | method default  | `signal` where SIGALRM exists, else `thread`                                                         |
|  [03]   | `pytest_timeout.dump_stacks(terminal)` | diagnostic dump | writes every other thread's traceback at timeout onto the terminal writer passed as `terminal` |
|  [04]   | `pytest_timeout.is_debugging()` | debugger probe  | suppresses the timer under a detected debugger unless detection is disabled                          |

```python contract
class Settings(NamedTuple):
    timeout: float | None
    method: str            # 'signal' | 'thread'
    func_only: bool
    disable_debugger_detection: bool
def dump_stacks(terminal: TerminalWriter) -> None: ...                              # traceback dump of every thread but the current one
def pytest_timeout_set_timer(item: Item, settings: Settings) -> bool | None: ...    # hook to override the timer
def pytest_timeout_cancel_timer(item: Item) -> bool | None: ...
```

## [03]-[ENTRYPOINTS]

The config key, marker, and CLI/env surface resolving a per-test ceiling.

| [INDEX] | [SURFACE]                                                                           | [KIND]            | [CAPABILITY]                                                                    |
| :-----: | :---------------------------------------------------------------------------------- | :---------------- | :------------------------------------------------------------------------------ |
|  [01]   | `timeout` (ini)                                                                     | config key        | session-wide seconds; `0` means no timeout — the estate sets the string `"30"`  |
|  [02]   | `@pytest.mark.timeout(seconds, *, method=None, func_only=None)`                     | per-test override | overrides the ini ceiling and mechanism for one test                            |
|  [03]   | `--timeout <seconds>` · `PYTEST_TIMEOUT` (env)                                      | CLI/env override  | run-time ceiling above the ini value                                            |
|  [04]   | `--timeout-method <signal\|thread>` · `timeout_method` (ini)                        | mechanism         | `signal` uses SIGALRM (main-thread only), `thread` a timer thread (any context) |
|  [05]   | `timeout_func_only` (ini) · `func_only` marker arg                                  | scope             | times only the test body, excluding fixture setup/teardown                      |
|  [06]   | `--session-timeout <seconds>` · `session_timeout` (ini)                             | session ceiling   | caps total session time, checked between tests                                  |
|  [07]   | `--timeout-disable-debugger-detection` · `timeout_disable_debugger_detection` (ini) | debugger policy   | keeps the timer armed under a debugger                                          |

```python contract
# ini keys (addini): timeout, timeout_method, timeout_func_only,
#                    timeout_disable_debugger_detection, session_timeout
# marker: @pytest.mark.timeout(30, method="thread", func_only=True)
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_TIMEOUT_TOPOLOGY]:
- `[tool.pytest] timeout = "30"` is string-typed on purpose: pytest-timeout registers `timeout` as a string ini option, and pytest 9's native TOML mode rejects a bare int for a string option, so the quoted `"30"` is the only valid spelling.
- The `signal` method fires SIGALRM on the main thread and cannot bound work off it; the `thread` method arms a timer thread and bounds any context — the DEFAULT is `signal` where SIGALRM exists.
- On breach the plugin runs `dump_stacks` for every thread, then fails the item; a detected debugger suppresses the timer unless `timeout_disable_debugger_detection` is set.

[STACKING]:
- `pytest`(`.api/pytest.md`): `required_plugins` lists `pytest-timeout`, so the guard fails the session if it is absent; `@pytest.mark.timeout` composes with the closed marker set.
- `runtime.py`(`../_testkit/runtime.py`): the `rasm` profiles set `deadline=None`, ceding per-example timing; pytest-timeout owns the whole-test wall clock instead, so a long property run is bounded once, not per example.
- `pytest-xdist`(`.api/pytest-xdist.md`): the ceiling applies inside each worker; a hung worker test fails locally rather than stalling the controller.

[LOCAL_ADMISSION]:
- Admitted on the dev plane in `[dependency-groups] dev`; no runtime graph imports `pytest_timeout`.
- A test needing a longer bound raises it through `@pytest.mark.timeout`, never by weakening the session-wide `"30"`.

[RAIL_LAW]:
- Package: `pytest-timeout`
- Owns: per-test wall-clock enforcement, the `signal`/`thread` mechanism choice, the all-thread stack dump on breach, and the session-total ceiling.
- Accept: the string `timeout = "30"` ini row; `@pytest.mark.timeout` for a per-test override; `--timeout-method thread` where work runs off the main thread; `func_only` to exclude fixture time.
- Reject: a native-int `timeout` in the TOML table (string-only option); reliance on a Hypothesis `deadline` for whole-test bounding (`deadline=None` cedes it); the `signal` method for off-main-thread work.
