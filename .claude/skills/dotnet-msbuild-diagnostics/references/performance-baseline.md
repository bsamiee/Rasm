# [PERFORMANCE_BASELINE]

Establish comparable evidence before you change the build. Record the measurement controls for every capture.

## [01]-[MEASUREMENT_CONTROLS]

Change only the input or setting that the measurement selects. Keep these other values unchanged:

- Machine and power state
- Source revision
- .NET SDK and MSBuild versions
- Build command, targets, properties, and environment
- Restore and package-cache state
- Parallelism and node-reuse settings
- Binary-log settings
- MSBuild server state

Capture the measured build with `-bl:{}`. Keep its command, properties, and parallelism unchanged.

- Binary logging can add measurable overhead to large builds. Keep it enabled for every compared capture.
- Run `binlog_overview` for each capture. Record the status, duration, project count, error count, and warning count.
- Do not apply universal timing or percentage thresholds. Compare the build against its own scenarios and history.
- `binlog_compare` compares properties and packages between two binlogs. Use it for configuration drift, never for timing.

## [02]-[MSBUILD_SERVER_STATE]

`dotnet build` can use persistent MSBuild and compiler servers. A warm server can reduce process startup and compiler startup work.

Choose one server state and keep it unchanged across compared captures:

```bash
dotnet build-server shutdown                         # stop persistent build servers before the capture
dotnet build --disable-build-servers -bl:no-server-{} # disable persistent build servers for this capture
```

Record the selected state with the capture. Do not compare a warm-server build with a fresh-server build.

## [03]-[CLEAN_OUTPUT_CAPTURE]

Use this capture to measure a full build after build outputs are clean.

```bash
dotnet clean -bl:clean-{}
dotnet build -bl:clean-output-{}
```

Run `binlog_overview` on the build binlog. Do not include the clean operation in the build duration.

## [04]-[CHANGED_INPUT_CAPTURE]

Use this capture to measure a representative edit after a successful build.

1. Capture the successful build that establishes the current outputs.
2. Change one representative input.
3. Capture the next build with the same command and properties.
4. Run `binlog_overview` on the changed-input binlog.

The changed input must represent the work that you want to measure. Record the input with the capture.

## [05]-[NO_CHANGE_CAPTURE]

Use this capture to measure work that runs when no input changed.

```bash
dotnet build -bl:prime-no-change-{}
dotnet build -bl:no-change-{}
```

Analyze the second binlog. Run `binlog_overview` on both binlogs to make sure that the first build succeeded.

## [06]-[EVIDENCE_ROUTE]

Follow the reference that matches the measured evidence:

- For executed work, graph constraints, or task cost, follow `execution-performance.md`.
- For evaluation cost or unexpected repeated work, follow `evaluation-and-incrementality.md`.

If both classes contribute, complete both workflows. Keep each conclusion tied to its binlog evidence.
