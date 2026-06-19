# [PY_RUNTIME_API_WATCHFILES]

`watchfiles` supplies Rust-backed filesystem change notification: sync and async watch generators, change-event classification, configurable filters, and process-restart drivers for reload workflows. It is the runtime owner for filesystem watches feeding the automation lanes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `watchfiles`
- package: `watchfiles`
- import: `watchfiles`
- owner: `runtime`
- rail: automation
- namespaces: `watchfiles`, `watchfiles.filters`, `watchfiles.run`
- capability: sync/async filesystem watches, change classification, path filters, process-restart drivers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: change and filter family
- rail: automation

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                |
| :-----: | :-------------- | :------------ | :------------------------------------ |
|  [01]   | `Change`        | enum          | added/modified/deleted classification |
|  [02]   | `BaseFilter`    | filter base   | change-filter contract                |
|  [03]   | `DefaultFilter` | filter        | dotfile/ignore default filter         |
|  [04]   | `PythonFilter`  | filter        | Python-source change filter           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: watch operations
- rail: automation

| [INDEX] | [SURFACE]      | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------- | :------------- | :------------------------------ |
|  [01]   | `watch`        | watch          | blocking change generator       |
|  [02]   | `awatch`       | watch          | async change generator          |
|  [03]   | `run_process`  | reload         | run+restart a process on change |
|  [04]   | `arun_process` | reload         | async run+restart driver        |

## [04]-[IMPLEMENTATION_LAW]

[AUTOMATION_TOPOLOGY]:
- watch law: filesystem watching is one `awatch` async generator under the anyio lane, cancelled through the lane's cancel scope; no polling loop with `stat` comparison.
- classification law: change handling matches on the `Change` enum (`added`/`modified`/`deleted`), never string comparison of event kinds.
- filter law: ignore rules are a `DefaultFilter`/`PythonFilter` or a `BaseFilter` subclass passed once; per-event path-string filtering in the consumer is deleted.
- reload law: process-restart workflows use `arun_process`; the runtime owns no second process supervisor.
- receipt law: change batches feed the receipt surface as a watch signal, never ad hoc logging.

[LOCAL_ADMISSION]:
- The automation lane composes `awatch` for change-driven work; the lane policy owns the cancel scope and capacity, watchfiles owns the change stream.
- This is a local filesystem watch, not a job framework or scheduler service.

[RAIL_LAW]:
- Package: `watchfiles`
- Owns: filesystem change watching, change classification, path filters, and process-restart drivers
- Accept: `awatch` under the lane, `Change`-enum matching, declared filters, `arun_process` reloads
- Reject: `stat` polling loops, string-kind comparison, per-event consumer filtering, a second process supervisor
