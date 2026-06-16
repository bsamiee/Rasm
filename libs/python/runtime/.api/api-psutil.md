# [PY_RUNTIME_API_PSUTIL]

`psutil` supplies cross-platform process and system introspection: per-process CPU/memory/file/connection/thread inspection and control, system-wide CPU/memory/disk/network counters, and process iteration. It is the runtime owner for local process and resource-budget observation feeding receipts and lane policy.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `psutil`
- package: `psutil`
- import: `psutil`
- version: `7.2.2`
- owner: `runtime`
- rail: observability
- namespaces: `psutil`
- capability: per-process inspection/control, system CPU/memory/disk/network counters, process iteration, load average

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `Process` | process | per-process inspection/control handle |
| [2] | `Popen` | process | psutil-aware subprocess wrapper |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `Error` | fault base | base psutil error |
| [2] | `NoSuchProcess` | fault | pid no longer exists |
| [3] | `AccessDenied` | fault | insufficient permission |
| [4] | `ZombieProcess` | fault | defunct process |
| [5] | `TimeoutExpired` | fault | wait timeout |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process operations
- rail: observability

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `Process` | build | inspection handle for a pid |
| [2] | `Process.oneshot` | batch | cached batched attribute read |
| [3] | `Process.cpu_percent` | read | process CPU share |
| [4] | `Process.memory_info` | read | RSS/VMS memory |
| [5] | `Process.memory_full_info` | read | USS/PSS memory |
| [6] | `Process.num_threads` | read | thread count |
| [7] | `Process.open_files` | read | open file handles |
| [8] | `Process.children` | read | child processes |
| [9] | `Process.terminate` / `Process.kill` | control | signal the process |
| [10] | `Process.wait` | control | await exit |
| [11] | `process_iter` | enumerate | iterate live processes |

[ENTRYPOINT_SCOPE]: system operations
- rail: observability

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `cpu_percent` | read | system CPU utilisation |
| [2] | `cpu_count` | read | logical/physical core count |
| [3] | `virtual_memory` | read | system memory stats |
| [4] | `swap_memory` | read | swap stats |
| [5] | `disk_usage` | read | filesystem usage |
| [6] | `disk_io_counters` | read | disk I/O counters |
| [7] | `net_io_counters` | read | network I/O counters |
| [8] | `getloadavg` | read | 1/5/15-minute load average |
| [9] | `boot_time` | read | system boot timestamp |

## [4]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- batch law: multi-attribute process reads use a single `Process.oneshot()` block so the kernel is queried once; scattered per-attribute calls in a hot path are deleted.
- budget law: lane policy reads `cpu_count`/`virtual_memory`/`getloadavg` once to size capacity limiters; the budget is a computed value threaded into `LanePolicy`, not re-polled per task.
- receipt law: process and system stats feed the receipt surface as resource-budget signals; psutil reads, the receipt owner records.
- fault law: `NoSuchProcess`/`AccessDenied` are expected and handled as `Result` outcomes, never crashes; a vanished pid is an `Error`, not an exception.

[LOCAL_ADMISSION]:
- The lane and receipt surfaces compose psutil for CPU/memory budget and process observation; the runtime owns no second system-stats reader.
- This is local introspection only — no global health state, no product telemetry pipeline (those stay at `Rasm.AppHost`).

[RAIL_LAW]:
- Package: `psutil`
- Owns: local process/system introspection feeding resource budgets and receipts
- Accept: `oneshot` batched reads, budget computation for lane policy, `Result`-handled process faults, receipt-fed stats
- Reject: scattered per-attribute hot-path reads, repeated budget polling, unhandled process faults, global health ownership
