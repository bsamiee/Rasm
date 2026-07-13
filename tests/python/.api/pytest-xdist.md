# [pytest-xdist] — the parallel and distributed execution lane

`pytest-xdist` shards one pytest session across worker subprocesses, each a full interpreter running an assigned slice of the collected tests. It drives the estate's parallel lane deliberately out of the default run: `-n` never enters `addopts` because pytest-benchmark measurement and parallel execution stay in separate sessions, so a parallel run is an explicit opt-in that carries its own scheduling and worker-restart policy.

## [01]-[PACKAGE_SURFACE]

- package: `pytest-xdist` · version `3.8.0` · license `MIT`
- namespace: `import xdist` — public helpers on `xdist`; the plugin body is `xdist.plugin`.
- asset: dist `pytest-xdist`; `pytest11` entry point `xdist = xdist.plugin` (disable with `-p no:xdist`).
- rail: parallel / distributed spec execution — the opt-in multi-worker session.

## [02]-[PUBLIC_TYPES]

Worker-identity helpers a fixture or conftest reads to branch on execution role.

| [INDEX] | [SYMBOL]                    | [KIND]     | [CAPABILITY]                                                                           |
| :-----: | :-------------------------- | :--------- | :------------------------------------------------------------------------------------- |
|  [01]   | `xdist.get_xdist_worker_id` | worker id  | returns `gw0`/`gw1`/… on a worker, `master` on the controller or a non-distributed run |
|  [02]   | `xdist.is_xdist_worker`     | role probe | true inside a worker subprocess                                                        |
|  [03]   | `xdist.is_xdist_controller` | role probe | true on the controlling process orchestrating workers                                  |
|  [04]   | `xdist.is_xdist_master`     | role probe | legacy alias of `xdist.is_xdist_controller`                                            |

```python signature
def get_xdist_worker_id(request_or_session: FixtureRequest | Session) -> str: ...  # 'gw0' | 'master'
def is_xdist_worker(request_or_session: FixtureRequest | Session) -> bool: ...
def is_xdist_controller(request_or_session: FixtureRequest | Session) -> bool: ...
```

## [03]-[ENTRYPOINTS]

CLI surface controlling worker count, scheduling mode, and restart tolerance.

| [INDEX] | [SURFACE]                        | [KIND]       | [CAPABILITY]                                                               |
| :-----: | :------------------------------- | :----------- | :------------------------------------------------------------------------- |
|  [01]   | `-n <n>` · `--numprocesses <n>`  | worker count | `auto` = CPUs, `logical` = logical cores, int fixes count; `0` disables    |
|  [02]   | `--maxprocesses <n>`             | worker cap   | ceiling on workers when `-n auto` derives the count                        |
|  [03]   | `--dist <mode>`                  | scheduler    | `load`/`loadscope`/`loadfile`/`loadgroup`/`worksteal`/`each`/`no`          |
|  [04]   | `--max-worker-restart <n>`       | crash budget | max worker restarts before the session aborts                              |
|  [05]   | `--maxschedchunk <n>`            | chunk cap    | caps later `load` dispatch batches; the initial round seeds two per worker |
|  [06]   | `--tx <spec>` · `--px <spec>`    | transport    | `--tx` adds an execnet gateway; `--px` a proxy gateway routed by `via=`    |
|  [07]   | `@pytest.mark.xdist_group(name)` | group pin    | under `--dist loadgroup`; routes same-group tests to one worker            |

```python signature
# --dist scheduler vocabulary (choices enforced by the option):
#   load       — round-robin dynamic dispatch, no affinity
#   loadscope  — group by module::class, one scope per worker
#   loadfile   — group by test file
#   loadgroup  — group by @pytest.mark.xdist_group(name)
#   worksteal  — dynamic dispatch with idle workers stealing queued tests
#   each       — every worker runs the full suite (env-matrix runs)
#   no         — the default: run in-process, no distribution
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_XDIST_TOPOLOGY]:
- Each worker is a separate interpreter; `PYTEST_XDIST_WORKER` (`gw0`/`gw1`/…), `PYTEST_XDIST_WORKER_COUNT`, and `PYTEST_XDIST_TESTRUNUID` identify it in the environment, and `PYTEST_XDIST_AUTO_NUM_WORKERS` overrides the `-n auto` derivation.
- `-n` is absent from `[tool.pytest] addopts` by design: a parallel run and a pytest-benchmark measurement session never share a process, so parallelism is always an explicit CLI opt-in.
- Session-scoped fixtures build once per worker, not once per run; a shared external resource keyed by `get_xdist_worker_id` avoids cross-worker collision.

[STACKING]:
- `pytest`(`.api/pytest.md`): the session xdist shards; `required_plugins` lists `pytest-xdist` so the guard holds even when `-n` is unset.
- `pytest-randomly`(`.api/pytest-randomly.md`): reseeds `random` per test inside every worker; `--randomly-seed` propagates so a parallel run stays reproducible per worker.
- `pytest-benchmark`(`.api/pytest-benchmark.md`): excluded from any `-n` session — measurement demands a single unshared process, the reason `-n` never enters `addopts`.

[LOCAL_ADMISSION]:
- Admitted on the dev plane in `[dependency-groups] dev`; no runtime graph imports `xdist`.
- Worker-role branching goes through `xdist.get_xdist_worker_id`, never a raw `PYTEST_XDIST_WORKER` read scattered across suites.

[RAIL_LAW]:
- Package: `pytest-xdist`
- Owns: worker-subprocess orchestration, the `--dist` scheduling vocabulary, worker-restart tolerance, and worker-identity helpers.
- Accept: `-n auto`/`-n <int>` as an explicit CLI opt-in; `--dist loadgroup` with `@pytest.mark.xdist_group` for resource affinity; `get_xdist_worker_id` for per-worker resource keying.
- Reject: `-n` in default `addopts`; a benchmark session under any `-n`; a session-scoped fixture assumed once-per-run when it is once-per-worker; a bare `PYTEST_XDIST_WORKER` read where the helper exists.
