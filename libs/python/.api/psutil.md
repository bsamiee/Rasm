# [PY_BRANCH_API_PSUTIL]

`psutil` supplies cross-platform system and process metrics: per-process CPU, memory, file descriptors, threads, connections, status, and full lifecycle control via `Process`; system-wide CPU, memory, disk, network, and sensor counters via module-level functions; and process iteration/waiting via `process_iter` and `wait_procs`. The `Process.oneshot()` context manager is the performance spine — it batches the underlying syscalls so a cluster of reads on one process costs one collection. Every metric returns a named tuple, so the dense design folds a single `oneshot` block into one typed reading rather than firing one syscall per attribute.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `psutil`
- package: `psutil`
- version: `7.2.2`
- license: `BSD-3-Clause`
- wheel: `cp36-abi3-macosx_11_0_arm64` — NATIVE abi3 wheel, `Root-Is-Purelib: false`; ships `psutil/_psutil_osx.abi3.so` (per-OS C extension). ABI3 (`cp36-abi3`) covers CPython >=3.6 including cp315. Platform-specific wheels exist per OS/arch.
- module: `psutil`
- asset: runtime library (native extension)
- rail: observability
- platform note: the C extension and the pure-Python `_ps{osx,linux,windows,bsd,...}` layer are selected at import; a subset of the surface and several named-tuple fields are platform-gated (see `[PLATFORM_GATING]`).

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process classes
- rail: observability

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [RAIL]                                      |
| :-----: | :-------- | :------------ | :------------------------------------------ |
|  [01]   | `Process` | process class | per-process metrics, control, and `oneshot` |
|  [02]   | `Popen`   | process class | `subprocess.Popen` fused with `Process` API |

[PUBLIC_TYPE_SCOPE]: exception types
- rail: observability

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [RAIL]                           |
| :-----: | :--------------- | :------------- | :------------------------------- |
|  [01]   | `Error`          | base exception | root of all psutil exceptions    |
|  [02]   | `NoSuchProcess`  | process error  | pid no longer exists             |
|  [03]   | `ZombieProcess`  | process error  | process is a zombie (subclass)   |
|  [04]   | `AccessDenied`   | access error   | insufficient privileges          |
|  [05]   | `TimeoutExpired` | timeout error  | `wait`/`wait_procs` deadline hit |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process iteration and lookup
- rail: observability

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :----------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `Process(pid=None)`                              | constructor    | attach to pid (defaults to `os.getpid()`) |
|  [02]   | `Popen(*args, **kwargs)`                         | constructor    | launch subprocess with `Process` API      |
|  [03]   | `process_iter(attrs=None, ad_value=None)`        | iterator       | iterate live processes, pre-fetch `attrs` |
|  [04]   | `pids() -> list[int]`                            | query          | all live PIDs                             |
|  [05]   | `pid_exists(pid) -> bool`                        | query          | true if pid is alive                      |
|  [06]   | `wait_procs(procs, timeout=None, callback=None)` | wait           | wait for multiple processes to exit       |

[ENTRYPOINT_SCOPE]: Process batched-read and identity
- rail: observability
- read off a `Process(pid)` handle; raise `NoSuchProcess`/`ZombieProcess`/`AccessDenied` on a dead/inaccessible target. Wrap a cluster of reads in `oneshot()` to collapse syscalls.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :------------------------------------------------------------ | :-------------- | :-------------------------------------------- |
|  [01]   | `Process.oneshot()`                                           | context manager | batch internal syscalls; cache info per block |
|  [02]   | `Process.as_dict(attrs=None, ad_value=None)`                  | bulk read       | dict of named attributes (skips inaccessible) |
|  [03]   | `Process.pid` / `name()` / `exe()` / `cmdline()`              | identity        | pid, executable name/path, argv               |
|  [04]   | `Process.ppid()` / `parent()` / `parents()`                   | lineage         | parent pid / `Process` / ancestor chain       |
|  [05]   | `Process.children(recursive=False)`                           | lineage         | child `Process` list                          |
|  [06]   | `Process.create_time()` / `username()` / `cwd()` / `status()` | identity        | start epoch, owner, cwd, status string        |
|  [07]   | `Process.is_running() -> bool`                                | liveness        | true if still alive (pid not reused)          |

[ENTRYPOINT_SCOPE]: Process resource metrics
- rail: observability

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Process.memory_info() -> pmem`              | metric         | RSS/VMS named tuple (fields platform-dependent)        |
|  [02]   | `Process.memory_full_info() -> pfullmem`     | metric         | adds `uss` (and `pss`/`swap` on Linux)                 |
|  [03]   | `Process.memory_percent(memtype="rss")`      | metric         | memory as percent of system total                      |
|  [04]   | `Process.cpu_percent(interval=None)`         | metric         | CPU utilization float (`None` = since-last-call delta) |
|  [05]   | `Process.cpu_times() -> pcputimes`           | metric         | user/system/children CPU seconds                       |
|  [06]   | `Process.num_threads()`                      | metric         | live thread count                                      |
|  [07]   | `Process.num_ctx_switches() -> pctxsw`       | metric         | voluntary/involuntary context switches                 |
|  [08]   | `Process.num_fds()` (POSIX)                  | metric         | open file-descriptor count                             |
|  [09]   | `Process.io_counters() -> pio` (gated)       | metric         | read/write counts + bytes                              |
|  [10]   | `Process.open_files() -> list[popenfile]`    | metric         | open regular files                                     |
|  [11]   | `Process.net_connections(kind='inet')`       | metric         | open sockets for this process                          |
|  [12]   | `Process.threads() -> list[pthread]` (gated) | metric         | per-thread CPU times                                   |
|  [13]   | `Process.environ()` (gated)                  | metric         | process environment dict                               |

[ENTRYPOINT_SCOPE]: Process control and scheduling
- rail: observability

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :---------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `Process.nice(value=None)`                            | scheduling     | get/set process priority           |
|  [02]   | `Process.cpu_affinity(cpus=None)` (gated)             | scheduling     | get/set CPU affinity mask          |
|  [03]   | `Process.cpu_num()` (gated)                           | scheduling     | CPU the process last ran on        |
|  [04]   | `Process.ionice(ioclass=None, value=None)` (gated)    | scheduling     | get/set I/O priority               |
|  [05]   | `Process.rlimit(resource, limits=None)` (gated)       | scheduling     | get/set resource limits            |
|  [06]   | `Process.send_signal(sig)` / `suspend()` / `resume()` | lifecycle      | signal / SIGSTOP / SIGCONT         |
|  [07]   | `Process.terminate()` / `kill()`                      | lifecycle      | SIGTERM / SIGKILL                  |
|  [08]   | `Process.wait(timeout=None)`                          | lifecycle      | block until exit; return exit code |

[ENTRYPOINT_SCOPE]: CPU metrics
- rail: observability

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :----------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `cpu_percent(interval=None, percpu=False)`       | metric         | system CPU utilization float            |
|  [02]   | `cpu_times(percpu=False) -> scputimes`           | metric         | system CPU time fields                  |
|  [03]   | `cpu_times_percent(interval=None, percpu=False)` | metric         | CPU time percentages                    |
|  [04]   | `cpu_count(logical=True)`                        | metric         | logical or physical CPU count           |
|  [05]   | `cpu_stats() -> scpustats`                       | metric         | ctx-switches/interrupts/soft-interrupts |
|  [06]   | `cpu_freq(percpu=False) -> scpufreq` (gated)     | metric         | current/min/max MHz (not on macOS)      |
|  [07]   | `getloadavg() -> (f, f, f)` (gated)              | metric         | 1/5/15-min load average                 |

[ENTRYPOINT_SCOPE]: memory, disk, network, and system metrics
- rail: observability

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `virtual_memory() -> svmem`                                         | metric         | system memory named tuple                       |
|  [02]   | `swap_memory() -> sswap`                                            | metric         | swap memory named tuple                         |
|  [03]   | `disk_usage(path) -> sdiskusage`                                    | metric         | total/used/free/percent for a path              |
|  [04]   | `disk_partitions(all=False)`                                        | metric         | mounted partitions                              |
|  [05]   | `disk_io_counters(perdisk=False, nowrap=True)`                      | metric         | disk I/O counters (`sdiskio`)                   |
|  [06]   | `net_io_counters(pernic=False, nowrap=True)`                        | metric         | network I/O counters (`snetio`)                 |
|  [07]   | `net_connections(kind='inet')`                                      | metric         | system-wide open sockets                        |
|  [08]   | `net_if_addrs()` / `net_if_stats()`                                 | metric         | interface addresses / link stats                |
|  [09]   | `boot_time()` / `users()`                                           | metric         | boot epoch / logged-in users                    |
|  [10]   | `sensors_battery() -> sbattery` (gated)                             | metric         | battery percent/secsleft/plugged (not on macOS) |
|  [11]   | `sensors_temperatures(fahrenheit=False)` / `sensors_fans()` (gated) | metric         | hardware sensors (Linux-mostly)                 |

## [04]-[IMPLEMENTATION_LAW]

[NAMEDTUPLE_FIELDS]:
- returns are named tuples accessible by field or index. Cross-platform fields: `svmem(total, available, percent, used, free, ...)`, `sswap(total, used, free, percent, sin, sout)`, `sdiskusage(total, used, free, percent)`, `pio(read_count, write_count, read_bytes, write_bytes)`, `pctxsw(voluntary, involuntary)`, `pthread(id, user_time, system_time)`, `popenfile(path, fd)`, `sbattery(percent, secsleft, power_plugged)`.
- `pmem`/`pfullmem`/`scputimes`/`svmem` field SETS differ per OS (e.g. macOS `pmem(rss, vms, pfaults, pageins)` + `pfullmem += uss`; Linux `pfullmem += uss, pss, swap`; Windows `pmem` carries `peak_wset`/`pagefile`/...). Read by field name, never by fixed index, when the field is OS-specific.

[PLATFORM_GATING]:
- `cpu_freq`, `getloadavg`, `sensors_battery`, `sensors_temperatures`, `sensors_fans` are appended to `__all__` only when the platform layer defines them. On macOS (`_psosx`) `cpu_freq`/`sensors_battery`/`sensors_temperatures`/`sensors_fans` are NOT present; `getloadavg` resolves via `os.getloadavg`. Guard any use with `hasattr(psutil, name)`.
- `Process` methods `io_counters`, `ionice`, `rlimit`, `cpu_affinity`, `cpu_num`, `environ`, `threads`, `num_handles` (Windows), `num_fds` (POSIX), `memory_maps` are conditionally bound on the platform `Process`; calling an unbound method raises `AttributeError`.

[ONESHOT_TOPOLOGY]:
- `with proc.oneshot():` makes the internal collector run once and cache the multi-valued result; subsequent reads in the block return cached values. This is the canonical batch path — fold every multi-attribute read of one process into a single `oneshot` block.
- `process_iter(attrs=[...])` pre-fetches the listed attributes once per process and supplies `ad_value` for fields that raise `AccessDenied`, avoiding mid-iteration faults; it is the system-wide analogue of `oneshot`.
- `cpu_percent(interval=None)` returns 0.0 on first call (no prior sample); pass a positive `interval` to block-and-sample, or call twice with `None` for a non-blocking delta. Same contract for the module-level `cpu_percent`/`cpu_times_percent`.

[INTEGRATION_LAW]:
- Stack with `opentelemetry-sdk`: feed a `oneshot` reading into OTel observable gauges/counters — `proc.memory_info().rss` and `proc.cpu_percent()` register through the API `Meter`, are shaped by an SDK `View`, and ship via the OTLP exporter. psutil is the source, the SDK owns temporality/aggregation. Read inside one `oneshot` so the gauge callback costs one collection.
- The named-tuple readings are the boundary value carriers; map field names to canonical metric attribute keys at the edge, never thread raw tuples through domain code.

[LOCAL_ADMISSION]:
- Wrap `Process` reads in try/except catching `NoSuchProcess`, `ZombieProcess`, and `AccessDenied`; the listing-to-reading race is unavoidable. `is_running()` is advisory only (pid reuse).
- Use `process_iter(attrs=[...])`/`as_dict(attrs=[...])` with explicit attribute lists to amortize syscall cost; use `oneshot()` for repeated reads of one process.
- Use `cpu_percent(interval=1.0)` in blocking contexts; `interval=None` in async/polling loops where the caller owns timing.

[RAIL_LAW]:
- Package: `psutil`
- Owns: process metrics + lifecycle control, system CPU/memory/disk/network/sensor metrics, process iteration and waiting, the `oneshot` batch path
- Accept: `Process` + `oneshot`/`as_dict`, `process_iter(attrs=...)`, `cpu_percent`, `virtual_memory`, `disk_usage`, `net_io_counters`, `hasattr`-guarded platform-gated functions
- Reject: direct `/proc` parsing or platform-specific syscall wrappers when psutil owns the metric, per-attribute reads outside `oneshot`, indexing OS-specific named-tuple fields by position, unguarded use of `cpu_freq`/`sensors_battery` on macOS
