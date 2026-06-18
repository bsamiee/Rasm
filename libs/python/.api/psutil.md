# [PY_BRANCH_API_PSUTIL]

`psutil` supplies cross-platform system and process metrics: per-process CPU, memory, file descriptors, connections, and status via `Process`; system-wide CPU, memory, disk, network, and battery counters via module-level functions; and process iteration and waiting via `process_iter` and `wait_procs`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `psutil`
- package: `psutil`
- module: `psutil`
- asset: runtime library
- rail: observability

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process classes
- rail: observability

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :-------- | :------------ | :-------------------------------- |
|   [1]   | `Process` | process class | per-process metrics and control   |
|   [2]   | `Popen`   | process class | subprocess.Popen with Process API |

[PUBLIC_TYPE_SCOPE]: exception types
- rail: observability

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [RAIL]                     |
| :-----: | :--------------- | :------------- | :------------------------- |
|   [1]   | `Error`          | base exception | all psutil exceptions root |
|   [2]   | `NoSuchProcess`  | process error  | pid no longer exists       |
|   [3]   | `ZombieProcess`  | process error  | process is a zombie        |
|   [4]   | `AccessDenied`   | access error   | insufficient privileges    |
|   [5]   | `TimeoutExpired` | timeout error  | wait_procs timeout         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process iteration and control
- rail: observability

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `Process(pid=None)`                    | constructor    | attach to process by pid            |
|   [2]   | `Popen(*args, **kwargs)`               | constructor    | launch subprocess with Process API  |
|   [3]   | `process_iter(attrs, ad_value)`        | iterator       | iterate all live processes          |
|   [4]   | `pids()`                               | query          | list of all live PIDs               |
|   [5]   | `pid_exists(pid)`                      | query          | true if pid is alive                |
|   [6]   | `wait_procs(procs, timeout, callback)` | wait           | wait for multiple processes to exit |

[ENTRYPOINT_SCOPE]: CPU metrics
- rail: observability

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------ | :------------- | :------------------------------- |
|   [1]   | `cpu_percent(interval, percpu)`       | metric         | CPU utilization as float percent |
|   [2]   | `cpu_times(percpu)`                   | metric         | named CPU time fields            |
|   [3]   | `cpu_times_percent(interval, percpu)` | metric         | CPU time percentages             |
|   [4]   | `cpu_count(logical)`                  | metric         | logical or physical CPU count    |
|   [5]   | `cpu_freq(percpu)`                    | metric         | CPU frequency current/min/max    |
|   [6]   | `cpu_stats()`                         | metric         | CPU statistics namedtuple        |

[ENTRYPOINT_SCOPE]: memory and swap
- rail: observability

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :----------------- | :------------- | :------------------------ |
|   [1]   | `virtual_memory()` | metric         | virtual memory namedtuple |
|   [2]   | `swap_memory()`    | metric         | swap memory namedtuple    |

[ENTRYPOINT_SCOPE]: disk metrics
- rail: observability

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------- | :------------- | :------------------------------ |
|   [1]   | `disk_usage(path)`                  | metric         | disk usage for path             |
|   [2]   | `disk_partitions(all)`              | metric         | list of mounted disk partitions |
|   [3]   | `disk_io_counters(perdisk, nowrap)` | metric         | disk I/O counters               |

[ENTRYPOINT_SCOPE]: network and system metrics
- rail: observability

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------- | :------------- | :----------------------------- |
|   [1]   | `net_io_counters(pernic, nowrap)` | metric         | network interface I/O counters |
|   [2]   | `net_if_addrs()`                  | metric         | network interface addresses    |
|   [3]   | `net_if_stats()`                  | metric         | network interface statistics   |
|   [4]   | `net_connections(kind)`           | metric         | open network connections       |
|   [5]   | `sensors_battery()`               | metric         | battery status or None         |
|   [6]   | `boot_time()`                     | metric         | system boot time as float      |
|   [7]   | `users()`                         | metric         | currently logged-in users list |

## [4]-[IMPLEMENTATION_LAW]

[PSUTIL_TOPOLOGY]:
- single namespace: `psutil`; 32 public types across one module
- `Process(pid)` wraps a PID; if `pid=None`, uses `os.getpid()`; raises `NoSuchProcess` when the process is gone
- `process_iter(attrs)` pre-fetches named attributes in one call per process; avoids per-attribute `AccessDenied` mid-iteration by using `ad_value` as a fallback
- `cpu_percent(interval=None)` on first call always returns 0.0; pass a positive interval or call twice to get a meaningful value
- metric return types are namedtuples (`svmem`, `scpufreq`, `sdiskusage`, etc.) accessible by field name or index

[LOCAL_ADMISSION]:
- Wrap `Process` method calls in try/except catching `NoSuchProcess`, `ZombieProcess`, and `AccessDenied`; race conditions are unavoidable between listing and reading.
- Use `process_iter(attrs=[...])` with an explicit attribute list to amortize per-process syscall cost.
- Use `cpu_percent(interval=1.0)` in blocking contexts; use `interval=None` in async/polling loops where the caller owns the timing.

[RAIL_LAW]:
- Package: `psutil`
- Owns: process metrics, system CPU/memory/disk/network metrics, process iteration and waiting
- Accept: `Process`, `process_iter`, `cpu_percent`, `virtual_memory`, `disk_usage`, `net_io_counters`
- Reject: direct `/proc` parsing or platform-specific syscall wrappers when psutil owns the metric
