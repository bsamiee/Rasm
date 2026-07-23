# [PY_BRANCH_API_PSUTIL]

`psutil` owns cross-platform process and system telemetry: per-process metrics and lifecycle control through `Process`, system-wide CPU, memory, disk, network, and sensor counters through module functions, and live-process iteration through `process_iter`/`wait_procs`. Every reading is a named tuple read by field name; `Process.oneshot()` batches the shared syscalls so a cluster of reads on one process costs one collection. It is the observability rail's metric source — the SDK edge owns temporality, aggregation, and export.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `psutil`
- package: `psutil` (`BSD-3-Clause`)
- module: `psutil`
- abi: native C extension over the import-selected `_ps{osx,linux,windows,bsd}` platform layer
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process classes

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :-------- | :------------ | :------------------------------------------ |
|  [01]   | `Process` | process class | per-process metrics, control, and `oneshot` |
|  [02]   | `Popen`   | process class | `subprocess.Popen` fused with `Process` API |

[PUBLIC_TYPE_SCOPE]: exception types

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                     |
| :-----: | :--------------- | :------------- | :------------------------------- |
|  [01]   | `Error`          | base exception | root of all psutil exceptions    |
|  [02]   | `NoSuchProcess`  | process error  | pid no longer exists             |
|  [03]   | `ZombieProcess`  | process error  | process is a zombie (subclass)   |
|  [04]   | `AccessDenied`   | access error   | insufficient privileges          |
|  [05]   | `TimeoutExpired` | timeout error  | `wait`/`wait_procs` deadline hit |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process iteration and lookup

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `Process(pid=None)`                              | ctor    | attach to pid (defaults to `os.getpid()`) |
|  [02]   | `Popen(*args, **kwargs)`                         | ctor    | launch subprocess with `Process` API      |
|  [03]   | `process_iter(attrs=None, ad_value=None)`        | static  | iterate live processes, pre-fetch `attrs` |
|  [04]   | `pids() -> list[int]`                            | static  | all live PIDs                             |
|  [05]   | `pid_exists(pid) -> bool`                        | static  | true if pid is alive                      |
|  [06]   | `wait_procs(procs, timeout=None, callback=None)` | static  | wait for multiple processes to exit       |

[ENTRYPOINT_SCOPE]: Process batched-read and identity

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `Process.oneshot()`                                           | instance | batch internal syscalls; cache info per block |
|  [02]   | `Process.as_dict(attrs=None, ad_value=None)`                  | instance | dict of named attributes (skips inaccessible) |
|  [03]   | `Process.pid` / `name()` / `exe()` / `cmdline()`              | instance | pid, executable name/path, argv               |
|  [04]   | `Process.ppid()` / `parent()` / `parents()`                   | instance | parent pid / `Process` / ancestor chain       |
|  [05]   | `Process.children(recursive=False)`                           | instance | child `Process` list                          |
|  [06]   | `Process.create_time()` / `username()` / `cwd()` / `status()` | instance | start epoch, owner, cwd, status string        |
|  [07]   | `Process.is_running() -> bool`                                | instance | true if still alive (pid not reused)          |

[ENTRYPOINT_SCOPE]: Process resource metrics

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Process.memory_info() -> pmem`              | instance | RSS/VMS named tuple (fields platform-dependent)        |
|  [02]   | `Process.memory_full_info() -> pfullmem`     | instance | adds `uss` (and `pss`/`swap` on Linux)                 |
|  [03]   | `Process.memory_percent(memtype="rss")`      | instance | memory as percent of system total                      |
|  [04]   | `Process.cpu_percent(interval=None)`         | instance | CPU utilization float (`None` = since-last-call delta) |
|  [05]   | `Process.cpu_times() -> pcputimes`           | instance | user/system/children CPU seconds                       |
|  [06]   | `Process.num_threads()`                      | instance | live thread count                                      |
|  [07]   | `Process.num_ctx_switches() -> pctxsw`       | instance | voluntary/involuntary context switches                 |
|  [08]   | `Process.num_fds()` (POSIX)                  | instance | open file-descriptor count                             |
|  [09]   | `Process.io_counters() -> pio` (gated)       | instance | read/write counts + bytes                              |
|  [10]   | `Process.open_files() -> list[popenfile]`    | instance | open regular files                                     |
|  [11]   | `Process.net_connections(kind='inet')`       | instance | open sockets for this process                          |
|  [12]   | `Process.threads() -> list[pthread]` (gated) | instance | per-thread CPU times                                   |
|  [13]   | `Process.environ()` (gated)                  | instance | process environment dict                               |

[ENTRYPOINT_SCOPE]: Process control and scheduling

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Process.nice(value=None)`                            | instance | get/set process priority           |
|  [02]   | `Process.cpu_affinity(cpus=None)` (gated)             | instance | get/set CPU affinity mask          |
|  [03]   | `Process.cpu_num()` (gated)                           | instance | CPU the process last ran on        |
|  [04]   | `Process.ionice(ioclass=None, value=None)` (gated)    | instance | get/set I/O priority               |
|  [05]   | `Process.rlimit(resource, limits=None)` (gated)       | instance | get/set resource limits            |
|  [06]   | `Process.send_signal(sig)` / `suspend()` / `resume()` | instance | signal / SIGSTOP / SIGCONT         |
|  [07]   | `Process.terminate()` / `kill()`                      | instance | SIGTERM / SIGKILL                  |
|  [08]   | `Process.wait(timeout=None)`                          | instance | block until exit; return exit code |

[ENTRYPOINT_SCOPE]: CPU metrics

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                            |
| :-----: | :----------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `cpu_percent(interval=None, percpu=False)`       | static  | system CPU utilization float            |
|  [02]   | `cpu_times(percpu=False) -> scputimes`           | static  | system CPU time fields                  |
|  [03]   | `cpu_times_percent(interval=None, percpu=False)` | static  | CPU time percentages                    |
|  [04]   | `cpu_count(logical=True)`                        | static  | logical or physical CPU count           |
|  [05]   | `cpu_stats() -> scpustats`                       | static  | ctx-switches/interrupts/soft-interrupts |
|  [06]   | `cpu_freq(percpu=False) -> scpufreq` (gated)     | static  | current/min/max MHz (not on macOS)      |
|  [07]   | `getloadavg() -> (f, f, f)` (gated)              | static  | 1/5/15-min load average                 |

[ENTRYPOINT_SCOPE]: memory, disk, network, and system metrics

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :------ | :---------------------------------------------- |
|  [01]   | `virtual_memory() -> svmem`                                         | static  | system memory named tuple                       |
|  [02]   | `swap_memory() -> sswap`                                            | static  | swap memory named tuple                         |
|  [03]   | `disk_usage(path) -> sdiskusage`                                    | static  | total/used/free/percent for a path              |
|  [04]   | `disk_partitions(all=False)`                                        | static  | mounted partitions                              |
|  [05]   | `disk_io_counters(perdisk=False, nowrap=True)`                      | static  | disk I/O counters (`sdiskio`)                   |
|  [06]   | `net_io_counters(pernic=False, nowrap=True)`                        | static  | network I/O counters (`snetio`)                 |
|  [07]   | `net_connections(kind='inet')`                                      | static  | system-wide open sockets                        |
|  [08]   | `net_if_addrs()` / `net_if_stats()`                                 | static  | interface addresses / link stats                |
|  [09]   | `boot_time()` / `users()`                                           | static  | boot epoch / logged-in users                    |
|  [10]   | `sensors_battery() -> sbattery` (gated)                             | static  | battery percent/secsleft/plugged (not on macOS) |
|  [11]   | `sensors_temperatures(fahrenheit=False)` / `sensors_fans()` (gated) | static  | hardware sensors (Linux-mostly)                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `with proc.oneshot():` runs the internal collector once and caches its multi-valued result; fold every multi-attribute read of one process into one block. `process_iter(attrs=[...])` is the system-wide analogue, pre-fetching listed attributes once per process and supplying `ad_value` for `AccessDenied` fields.
- `cpu_percent(interval=None)` returns 0.0 on the first call; a positive `interval` blocks and samples, two `None` calls yield a non-blocking delta — same contract for module-level `cpu_percent`/`cpu_times_percent`.
- named-tuple returns read by field name, never positional index — `pmem`/`pfullmem`/`scputimes`/`svmem` field sets are OS-specific: macOS `pmem(rss, vms, pfaults, pageins)` with `pfullmem` adding `uss`, Linux `pfullmem` adding `uss, pss, swap`, Windows `pmem` carrying `peak_wset`/`pagefile`.
- a `(gated)` row binds only where the platform layer defines it: a gated module function is absent from `__all__`, an unbound `Process` method raises `AttributeError` — guard every gated use with `hasattr`. macOS (`_psosx`) omits `cpu_freq` and the `sensors_*` family; `getloadavg` resolves through `os.getloadavg`.

[STACKING]:
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): a `oneshot` reading feeds OTel observable gauges/counters — `proc.memory_info().rss` and `proc.cpu_percent()` register through the API `Meter`, shape through an SDK `View`, and ship via the OTLP exporter; read inside one `oneshot` so the gauge callback costs one collection. psutil is the source, the SDK owns temporality and aggregation.
- `loky`(`.api/loky.md`): `cpu_count(only_physical_cores=True)` reads the physical-core count through psutil to size the pool; `Supervisor` reads pool worker RSS via `psutil.Process().oneshot()` over `children(recursive=True)`/`memory_info().rss` against the `SupervisionPolicy` ceiling, scoped through `WorkerPool.pids()`, rolling the cooperative arm on a breach.
- `pebble`(`.api/pebble.md`): `Supervisor._probe` reads worker RSS via `psutil.Process().oneshot()` over `children(recursive=True)` and `memory_info().rss` against the `SupervisionPolicy.rss_ceiling`, rolling the terminal (pebble) arm on a `DEGRADED` breach and retiring-then-respawning on `DEAD`; psutil is the live-ceiling arm, `max_tasks` recycling the fixed-cadence arm.
- within-lib: the named-tuple reading is the boundary value carrier — map field names to canonical metric attribute keys at the edge, never thread a raw tuple through domain code.

[LOCAL_ADMISSION]:
- wrap every `Process` read in try/except for `NoSuchProcess`, `ZombieProcess`, and `AccessDenied` — the listing-to-reading race is unavoidable; `is_running()` is advisory (pid reuse).
- amortize syscalls with explicit attribute lists — `process_iter(attrs=[...])`/`as_dict(attrs=[...])` system-wide, `oneshot()` for repeated reads of one process.
- `cpu_percent(interval=1.0)` in blocking contexts, `interval=None` in async/polling loops where the caller owns timing.

[RAIL_LAW]:
- Package: `psutil`
- Owns: process metrics and lifecycle control, system CPU/memory/disk/network/sensor metrics, process iteration and waiting, the `oneshot` batch path
- Accept: `Process` + `oneshot`/`as_dict`, `process_iter(attrs=...)`, `cpu_percent`, `virtual_memory`, `disk_usage`, `net_io_counters`, `hasattr`-guarded platform-gated functions
- Reject: direct `/proc` parsing or platform syscall wrappers where psutil owns the metric, per-attribute reads outside `oneshot`, positional indexing of OS-specific named tuples, unguarded gated calls on macOS
