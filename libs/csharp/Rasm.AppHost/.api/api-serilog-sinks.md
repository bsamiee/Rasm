# [RASM_APPHOST_API_SERILOG_SINKS]

`Serilog.Sinks.Console` and `Serilog.Sinks.File` provide the bootstrap sink surfaces AppHost uses for interactive diagnostics and retained local runtime logs. Both extend the `Serilog` `WriteTo` and `AuditTo` configuration rails; the core event model, enrichment, filtering, and host integration stay in `api-serilog.md` and `api-serilog-hosting.md`.

[APP_ROOT_RESERVED]: `[V15]` — `WriteTo.Console`/`WriteTo.File` sink projection is a composition-root concern; the lib emits `ILogger` only. The central pins are retained; the rows move out of the lib csproj.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog.Sinks.Console`

- package: `Serilog.Sinks.Console`
- assembly: `Serilog.Sinks.Console`
- namespace: `Serilog`
- extension owner: `Serilog.ConsoleLoggerConfigurationExtensions`
- rail: telemetry sink

[PACKAGE_SURFACE]: `Serilog.Sinks.File`

- package: `Serilog.Sinks.File`
- assembly: `Serilog.Sinks.File`
- namespace: `Serilog`
- extension owner: `Serilog.FileLoggerConfigurationExtensions`
- rail: telemetry sink

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: console sink

- rail: telemetry sink

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]       | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------- | :------------------ | :-------------------------------- |
|  [01]   | `ConsoleLoggerConfigurationExtensions`                 | sink extension      | `WriteTo.Console` overload family |
|  [02]   | `ConsoleAuditLoggerConfigurationExtensions`            | audit extension     | `AuditTo.Console` overload family |
|  [03]   | `AnsiConsoleTheme`                                     | theme               | ANSI terminal color palette       |
|  [04]   | `SystemConsoleTheme`                                   | theme               | classic console color palette     |
|  [05]   | `ConsoleTheme`                                         | theme base          | styled text emission contract     |
|  [06]   | `ConsoleThemeStyle` / `SystemConsoleThemeStyle`        | style keys          | per-token console formatting      |
|  [07]   | `Serilog.Sinks.SystemConsole.Output.LevelOutputFormat` | level output helper | level token formatting            |

[PUBLIC_TYPE_SCOPE]: file sink

- rail: telemetry sink

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]        | [CAPABILITY]                                                 |
| :-----: | :---------------------------------- | :------------------- | :----------------------------------------------------------- |
|  [01]   | `FileLoggerConfigurationExtensions` | sink extension       | `WriteTo.File` overload family                               |
|  [02]   | `RollingInterval`                   | enum                 | `Infinite`/`Year`/`Month`/`Day`/`Hour`/`Minute` roll cadence |
|  [03]   | `FileLifecycleHooks`                | lifecycle hook base  | mutate/delete hook seam                                      |
|  [04]   | `IFileSink` / `IFlushableFileSink`  | sink contracts       | event emission and flush                                     |
|  [05]   | `FileSink` / `SharedFileSink`       | sink implementations | exclusive/shared file sinks                                  |
|  [06]   | `PeriodicFlushToDiskSink`           | wrapper sink         | interval-bound flush-to-disk                                 |
|  [07]   | `NullSink`                          | sink implementation  | dropped-event sink                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: console sink registration

- rail: telemetry sink

[WRITE_TO_CONSOLE_OVERLOADS]:

- Shared parameters: `restrictedToMinimumLevel`, `levelSwitch`, `standardErrorFromLevel`, `syncRoot`
- Template parameters: `outputTemplate`, `formatProvider`, `theme`, `applyThemeToRedirectedOutput`
- Formatter parameter: `ITextFormatter formatter`

| [INDEX] | [SURFACE]         | [OVERLOAD] | [CAPABILITY]                 |
| :-----: | :---------------- | :--------- | :--------------------------- |
|  [01]   | `WriteTo.Console` | template   | interactive text sink        |
|  [02]   | `WriteTo.Console` | formatter  | formatter-owned console sink |
|  [03]   | `AuditTo.Console` | template   | audit console sink           |
|  [04]   | `AuditTo.Console` | formatter  | audit console sink           |

[ENTRYPOINT_SCOPE]: file sink registration

- rail: telemetry sink

[WRITE_TO_FILE_TEMPLATE]:

- Parameters: `path`, `restrictedToMinimumLevel`, `outputTemplate`, `formatProvider`, `fileSizeLimitBytes`, `levelSwitch`, `buffered`, `shared`, `flushToDiskInterval`, `rollingInterval`, `rollOnFileSizeLimit`, `retainedFileCountLimit`, `encoding`, `hooks`

[WRITE_TO_FILE_FORMATTER]:

- Parameters: `ITextFormatter formatter`, file path, and size, roll, retention, buffering, and sharing policy

| [INDEX] | [SURFACE]                           | [OVERLOAD] | [CAPABILITY]               |
| :-----: | :---------------------------------- | :--------- | :------------------------- |
|  [01]   | `WriteTo.File`                      | template   | retained text-file sink    |
|  [02]   | `WriteTo.File`                      | formatter  | formatter-owned file sink  |
|  [03]   | `AuditTo.File`                      | template   | retained audit file sink   |
|  [04]   | `AuditTo.File`                      | formatter  | retained audit file sink   |
|  [05]   | `FileLifecycleHooks.OnFileOpened`   | hook       | stream-open lifecycle seam |
|  [06]   | `FileLifecycleHooks.OnFileDeleting` | hook       | retention-deletion seam    |
|  [07]   | `FileLifecycleHooks.Then`           | hook       | lifecycle extension seam   |
|  [08]   | `IFlushableFileSink.FlushToDisk`    | flush      | durability boundary        |

## [04]-[IMPLEMENTATION_LAW]

[LOCAL_ADMISSION]:

- Console and file sinks are bootstrap/composition concerns only.
- Console output is the interactive and supervisor diagnostic sink; it carries bounded structured event rendering, never domain receipts as log text.
- File output writes only to owner-declared runtime log paths, with rolling interval, retention count, and size limits declared at composition.
- File lifecycle hooks belong to retention/compliance composition and never mutate domain state.

[RAIL_LAW]:

- Packages: `Serilog.Sinks.Console`, `Serilog.Sinks.File`
- Own: local Serilog sink emission for interactive diagnostics and retained bounded log files
- Accept: `WriteTo.Console`, `WriteTo.File`, `AuditTo.Console`, `AuditTo.File` in host bootstrap/composition
- Reject: sink configuration below AppHost composition; unbounded file growth; raw receipt serialization as log lines
