# [PERFORMANCE_BASELINE]

Establish comparable evidence before you change the build. Record the measurement controls for every capture.

## [01]-[MEASUREMENT_CONTROLS]

Change only the input or setting that the measurement selects. Keep the build command, properties, parallelism, node reuse, restore state, binary-log settings, and MSBuild server state unchanged across compared captures.

- Keep binary logging enabled for every compared capture, because it adds overhead to the build it measures.
- Record the status, duration, project count, error count, and warning count of each capture.
- Do not apply universal timing or percentage thresholds. Compare the build against its own scenarios and history.
- Use `binlog_compare` for configuration drift, never for timing.

## [02]-[MSBUILD_SERVER_STATE]

`dotnet build` can use persistent MSBuild and compiler servers. A warm server can reduce process startup and compiler startup work.

Choose one server state and keep it unchanged across compared captures:

```bash
dotnet build-server shutdown                           # stop persistent build servers before the capture
dotnet build --disable-build-servers -bl:no-server-{}  # disable persistent build servers for this capture
```

Record the selected state with the capture.

## [03]-[CLEAN_OUTPUT_CAPTURE]

Use this capture to measure a full build after build outputs are clean.

```bash
dotnet clean -bl:clean-{}
dotnet build -bl:clean-output-{}
```

Do not include the clean operation in the build duration.

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

Analyze the second binlog. Make sure that the first build succeeded.
