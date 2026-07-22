# [RASM_APPHOST_API_SERILOG_SINKS]

`Serilog.Sinks.Console` and `Serilog.Sinks.File` own AppHost's two bootstrap log sinks — interactive terminal diagnostics and retained rolling runtime files — each registered through the `WriteTo`/`AuditTo` rail at the composition root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog.Sinks.Console`
- package: `Serilog.Sinks.Console`
- assembly: `Serilog.Sinks.Console`
- namespace: `Serilog`
- rail: telemetry sink

[PACKAGE_SURFACE]: `Serilog.Sinks.File`
- package: `Serilog.Sinks.File`
- assembly: `Serilog.Sinks.File`
- namespace: `Serilog`
- rail: telemetry sink

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: console sink

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]   | [CAPABILITY]                      |
| :-----: | :------------------------------------------ | :-------------- | :-------------------------------- |
|  [01]   | `ConsoleLoggerConfigurationExtensions`      | sink extension  | `WriteTo.Console` overload family |
|  [02]   | `ConsoleAuditLoggerConfigurationExtensions` | audit extension | `AuditTo.Console` overload family |
|  [03]   | `AnsiConsoleTheme`                          | theme           | ANSI terminal color palette       |
|  [04]   | `SystemConsoleTheme`                        | theme           | classic console color palette     |
|  [05]   | `ConsoleTheme`                              | theme base      | styled text emission contract     |
|  [06]   | `ConsoleThemeStyle`                         | style enum      | console token style keys          |
|  [07]   | `SystemConsoleThemeStyle`                   | style enum      | system-console token style keys   |
|  [08]   | `LevelOutputFormat`                         | level formatter | level token formatting            |

[PUBLIC_TYPE_SCOPE]: file sink

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]       | [CAPABILITY]                                                 |
| :-----: | :---------------------------------- | :------------------ | :----------------------------------------------------------- |
|  [01]   | `FileLoggerConfigurationExtensions` | sink extension      | `WriteTo.File` overload family                               |
|  [02]   | `RollingInterval`                   | enum                | `Infinite`/`Year`/`Month`/`Day`/`Hour`/`Minute` roll cadence |
|  [03]   | `FileLifecycleHooks`                | lifecycle hook base | mutate/delete hook seam                                      |
|  [04]   | `IFileSink`                         | sink contract       | event emission contract                                      |
|  [05]   | `IFlushableFileSink`                | sink contract       | flush-to-disk contract                                       |
|  [06]   | `FileSink`                          | sink impl           | exclusive file sink                                          |
|  [07]   | `SharedFileSink`                    | sink impl           | shared file sink                                             |
|  [08]   | `PeriodicFlushToDiskSink`           | wrapper sink        | interval-bound flush-to-disk                                 |
|  [09]   | `NullSink`                          | sink impl           | dropped-event sink                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: console sink registration

| [INDEX] | [SURFACE]                         | [SHAPE] | [CAPABILITY]                   |
| :-----: | :-------------------------------- | :------ | :----------------------------- |
|  [01]   | `WriteTo.Console`                 | static  | interactive themed text sink   |
|  [02]   | `WriteTo.Console(ITextFormatter)` | static  | formatter-owned console sink   |
|  [03]   | `AuditTo.Console`                 | static  | synchronous audit console sink |
|  [04]   | `AuditTo.Console(ITextFormatter)` | static  | formatter audit console sink   |

[ENTRYPOINT_SCOPE]: file sink registration

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------- | :------- | :------------------------------ |
|  [01]   | `WriteTo.File`                      | static   | rolling retained text-file sink |
|  [02]   | `WriteTo.File(ITextFormatter)`      | static   | formatter-owned file sink       |
|  [03]   | `AuditTo.File`                      | static   | synchronous audit file sink     |
|  [04]   | `AuditTo.File(ITextFormatter)`      | static   | formatter audit file sink       |
|  [05]   | `FileLifecycleHooks.OnFileOpened`   | instance | stream-open lifecycle seam      |
|  [06]   | `FileLifecycleHooks.OnFileDeleting` | instance | retention-deletion seam         |
|  [07]   | `FileLifecycleHooks.Then`           | instance | hook-chain composition          |
|  [08]   | `IFlushableFileSink.FlushToDisk`    | instance | durability boundary             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Console and file sinks extend `LoggerSinkConfiguration.WriteTo` and `LoggerAuditSinkConfiguration.AuditTo`; every registration folds through that one rail and the library emits `ILogger` alone.

[STACKING]:
- `Serilog`(`.api/api-serilog.md`): `WriteTo`/`AuditTo` resolve `LoggerSinkConfiguration`/`LoggerAuditSinkConfiguration`; these extensions are the terminal sink arms that rail admits.
- `SerilogProjectionPolicy.Shape`: folds console and file arms onto the frozen `LoggerConfiguration` at observability bootstrap, stacking `Fallible` failure-listener wrap and `RollingInterval`/`retainedFileCountLimit` retention.

[LOCAL_ADMISSION]:
- Console output carries bounded structured event rendering for interactive and supervisor diagnostics, never domain receipts as log text.
- File output writes only owner-declared runtime log paths under composition-declared rolling interval, retention count, and size limits.
- File lifecycle hooks serve retention and compliance composition and never mutate domain state.

[RAIL_LAW]:
- Packages: `Serilog.Sinks.Console`, `Serilog.Sinks.File`
- Own: local Serilog sink emission for interactive diagnostics and retained bounded log files
- Accept: `WriteTo.Console`, `WriteTo.File`, `AuditTo.Console`, `AuditTo.File` in host bootstrap composition
- Reject: sink configuration below AppHost composition; unbounded file growth; raw receipt serialization as log lines
