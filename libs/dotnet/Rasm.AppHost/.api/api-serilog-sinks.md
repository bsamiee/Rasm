# [RASM_APPHOST_API_SERILOG_SINKS]

`Serilog.Sinks.Console` and `Serilog.Sinks.File` own AppHost's two bootstrap log sinks — interactive terminal diagnostics and retained rolling runtime files — each registered through the `WriteTo`/`AuditTo` rail at the composition root.

## [01]-[PUBLIC_TYPES]

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
|  [07]   | `SharedFileSink`                    | sink impl           | `[Obsolete]`; `WriteTo.File(shared: true)` supersedes it     |
|  [08]   | `PeriodicFlushToDiskSink`           | wrapper sink        | interval-bound flush-to-disk                                 |
|  [09]   | `NullSink`                          | sink impl           | dropped-event sink                                           |

## [02]-[ENTRYPOINTS]

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Console and file sinks extend `LoggerSinkConfiguration.WriteTo` and `LoggerAuditSinkConfiguration.AuditTo`; every registration folds through that one rail and the library emits `ILogger` alone.
- Cross-process append is the `shared: true` argument on the `WriteTo.File` arrow, never a constructed sink: `SharedFileSink` carries `[Obsolete]` naming that arrow as its replacement. The arrow returns a `LoggerConfiguration` and no sink handle, so `IFlushableFileSink.FlushToDisk` is reachable only from a caller-constructed `FileSink`, which is exclusive — a shared file and a caller-held flush handle are mutually exclusive, and `flushToDiskInterval` is the shared leg's own durability seat.

[STACKING]:
- `Serilog`(`.api/api-serilog.md`): `WriteTo`/`AuditTo` resolve `LoggerSinkConfiguration`/`LoggerAuditSinkConfiguration`; these extensions are the terminal sink arms that rail admits.
- `Rasm.AppHost` `Observability/telemetry#LOG_PROJECTION`: `SerilogSinks.For` mints every leg as a rail ARROW behind the `LogPipeline` arbitration — `WriteTo.Console(ITextFormatter)` on the hot tier, `AuditTo.Console(ITextFormatter)` on the audit leg, and the host-keyed `WriteTo.File(ITextFormatter, path, shared: true, flushToDiskInterval, rollingInterval)` on the `Fallible`/`FallbackChain` rescue leg — so `SerilogProjectionPolicy.Shape` folds arrows alone and the record holds no sink handle to dispose.

[LOCAL_ADMISSION]:
- Console output carries bounded structured event rendering for interactive and supervisor diagnostics, never domain outcomes as log text.
- File output writes only owner-declared runtime log paths under composition-declared rolling interval, retention count, and size limits.
- File lifecycle hooks serve retention and compliance composition and never mutate domain state.
