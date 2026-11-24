---
name: testing
description: "Use when writing, running, or debugging tests of code that calls RhinoCommon, Grasshopper, or Grasshopper 2, covering in-process hosts, macOS setup, CI, and test design"
---

# [RHINO_TESTING]

Covers tests that load an installed Rhino into the test process and drive documents, plug-ins, and Grasshopper solutions through RhinoCommon. Every test touching a RhinoCommon type needs an installed, licensed Rhino, as RhinoCommon geometry calls native Rhino libraries.

[REFERENCES]:
- [01]-[GRASSHOPPER](references/grasshopper.md): Grasshopper 1 and 2 documents, components, inputs, solutions, and file fixtures under test
- [02]-[CI](references/ci.md): Runner choice, licensing, Rhino installation, step order, and artefacts for Rhino test jobs

[TEMPLATES]:
- [01]-[TESTS_CSPROJ](templates/Tests.csproj.template): Test project saved as `Tests/Tests.csproj`, with Rhino references, install gate, runtime, and dumps
- [02]-[RHINOTESTHOST](templates/RhinoTestHost.cs): Shared host, mode, boot request, and per-test Rhino checks
- [03]-[DARWIN](templates/Darwin.cs): Entry point for macOS, licensed scratch profile, and in-process boot
- [04]-[WINDOWS](templates/Windows.cs): Windows entry point booting `RhinoCore` through Rhino.Inside
- [05]-[RHINOTESTS](templates/RhinoTests.cs): Grid property with its pinned fact, document round trip, and a test of mode `Interface` alone
- [06]-[GRASSHOPPERTESTS](templates/GrasshopperTests.cs): Grasshopper 2 component solved headless with outputs and messages asserted
- [07]-[BACKGROUND](templates/Background.m): Injected library keeping Rhino unfocused, refusing modal prompts, and redirecting preferences

Templates test a placeholder project `Library` (`Slabs.Extrude`, `Slabs.Place`, `SlabVolumeComponent`), rename them to the code under test.

## [01]-[RUNNING]

### [01.1]-[HOSTS]

Pick host by platform and by what the code under test touches:

| [INDEX] | [HOST]             | [PLATFORMS]    | [FRAMEWORK] | [USE]                                                     |
| :-----: | :----------------- | :------------- | :---------- | :-------------------------------------------------------- |
|  [01]   | Template host      | Windows, macOS | `xunit.v3`  | Documents, plug-ins, Grasshopper 2, per-test Rhino checks |
|  [02]   | Rhino.Testing      | Windows, Linux | `NUnit`     | McNeel's harness, Grasshopper 1 `.gh` assertion files     |
|  [03]   | Rhino-free project | Any            | Any         | Logic with no RhinoCommon type in its signatures or body  |
|  [04]   | Running Rhino      | Windows, macOS | None        | Commands, views, dialogs, and anything needing user input |

Logic with no Rhino type belongs in a Rhino-free project tested everywhere, and Rhino-bound tests stay few and deep. Running Rhino (rhinocode, an MCP server, a launched app) suits interactive checks, never a test suite: rhinocode reports success on queuing a script, before it finishes.

Template host boots Rhino once on the process main thread and runs tests on a worker: macOS loads Rhino's frameworks into the test process (`Darwin.cs`, `[02.2]-[BOOT]`), Windows starts `RhinoCore` through Rhino.Inside (`Windows.cs`, `[02.5]-[WINDOWS]`), and both end the process once results are written.

### [01.2]-[COMMANDS]

`global.json` selects Microsoft.Testing.Platform for `dotnet test`:

```json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

```bash
dotnet test --project Tests/Tests.csproj
dotnet test --project Tests/Tests.csproj -- --filter-class 'Tests.SlabTests' --filter-method '*Undo*'
dotnet test --project Tests/Tests.csproj --configuration Release --no-build --results-directory <dir>
dotnet test --project Tests/Tests.csproj '-p:TestingPlatformCommandLineArguments=--report-trx --hangdump --hangdump-timeout 90s'
```

- `TestingPlatformCommandLineArguments` precede every run's arguments, an option given twice fails with exit code 5, and `-p:` replaces the project value
- `--filter-trait`, `--filter-namespace`, `--filter-not-*`, and `--filter-query` select tests, `--list-tests` lists them without running
- Runs print one summary line per assembly, failed tests with their captured command window lines, and the TRX path

Exit codes decide what to read next:

| [INDEX] | [CODE] | [MEANING]                     | [READ]                                                  |
| :-----: | :----: | :---------------------------- | :------------------------------------------------------ |
|  [01]   |  `0`   | Every test passed             | Nothing                                                 |
|  [02]   |  `2`   | One or more tests failed      | Failure messages, `Standard output` of each failed test |
|  [03]   |  `5`   | Invalid command line          | Option named in the message                             |
|  [04]   |  `7`   | Test host exited unexpectedly | Crash sequence log, crash report, macOS `.ips` report   |
|  [05]   |  `8`   | Zero tests ran                | Filter or test discovery                                |

### [01.3]-[RHINO_TESTING]

Rhino.Testing is McNeel's NUnit harness (`9.0.8-beta` for Rhino 9, `net48`, `net8.0`, `net9.0`, and their `-windows` forms). Its macOS support sits in an unmerged pull request, and macOS tests use the template host:
- `Rhino.Testing.Configs.xml` beside the test assembly (`CopyToOutputDirectory`) holds `<Settings>` with `RhinoSystemDirectory`, `LoadGrasshopper`, `LoadGrasshopper2`, `LoadEto`, `LoadRDK`, `CreateRhinoDoc`, `CreateRhinoView`, and `<LoadPlugins><Plugin Location="..."/></LoadPlugins>`
- `RhinoSystemDirectory` empty or missing lets Rhino.Inside find the install, an absolute path pins one, and `9.0.8-beta` no longer resolves a relative path against the file
- Elements carry no platform condition, one file holds one value per element for every machine reading it
- `[SetUpFixture]` subclass of `RhinoSetupFixture` boots Rhino once per run, `LoadGHA` loads `.gha` files, and `[RhinoTestFixture]` boots Rhino at discovery for test cases whose data names RhinoCommon types
- `CreateRhinoDoc` sets `RhinoDoc.ActiveDoc` to one shared headless document, and tests writing a document create their own
- `RhinoTestFixture.TestGrasshopper` and `RunGrasshopper` solve a `.gh` file and return a `GHReport` of component messages
- Tests run on the thread that booted Rhino, which serves as Rhino's main thread, and NUnit parallelism stays off

### [01.4]-[CHECKS]

Rhino catches exceptions inside its callbacks and writes command output only to its command window, tests see neither without these members:

| [INDEX] | [CHECK]             | [MEMBER]                                        | [REPORTS]                                                     |
| :-----: | :------------------ | :---------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | Swallowed exception | `HostUtils.OnExceptionReport`                   | Exceptions in `InvokeOnUiThread`, events, commands, plug-ins  |
|  [02]   | Command output      | `RhinoApp.CommandWindowCaptureEnabled`          | Lines `CapturedCommandWindowStrings(true)` drains on main     |
|  [03]   | Debug strings       | `HostUtils.SendDebugToCommandLine`              | `HostUtils.DebugString` lines in the captured output          |
|  [04]   | Object validity     | `IsValidWithLog(out string log)`                | First openNURBS defect, log goes in the assertion message     |
|  [05]   | Brep validity       | `Brep.IsValidTopology`, then `IsValidGeometry`  | Topology defect, then geometry defect, each with its log      |
|  [06]   | Mesh defects        | `Mesh.Check(TextLog, ref MeshCheckParameters)`  | Degenerate faces, naked and non-manifold edges, intersections |
|  [07]   | Document integrity  | `RhinoDoc.Audit(TextLog, attemptRepair: false)` | Table and object defects of the whole document                |
|  [08]   | Undo grouping       | `RhinoDoc.GetUndoRecords()`                     | Undo records newest first, with descriptions                  |
|  [09]   | Added object        | `ObjectTable.Add*` return value                 | `Guid.Empty` on refusal                                       |

- Brep checks run topology, then geometry, then `IsValidTolerancesAndFlags`, each assuming the one before passed
- `RhinoApp.SendWriteToConsole` output appears nowhere inside the test process
- `RhinoApp.CheckNewObject` adds invalid objects without a report, `File3dm.IsValid` and `File3dm.Audit` are obsolete and always pass
- Template host fails each test that raised an `OnExceptionReport` event and writes captured lines to the test's output

### [01.5]-[DEBUGGING]

With `--hangdump` or `--crashdump` on, Microsoft.Testing.Platform runs tests in a child process carrying `--internal-testhostcontroller-pid`, the process debuggers attach to:

| [INDEX] | [ARTEFACT]                           | [WRITER]                               | [HOLDS]                                                 |
| :-----: | :----------------------------------- | :------------------------------------- | :------------------------------------------------------ |
|  [01]   | `<assembly>_<id>_crash.sequence.log` | `--crashdump`                          | Tests started and ended, last line without end is cause |
|  [02]   | `<assembly>_<pid>_crash.dmp`         | `--crashdump --crashdump-type Mini`    | Managed and native state at the crash                   |
|  [03]   | `<dump>.crashreport.json`            | `--crash-report-if-supported`          | Threads and frames of the crash as JSON                 |
|  [04]   | Hang dump                            | `--hangdump --hangdump-timeout <time>` | Process state when a test outlived the timeout          |
|  [05]   | `dotnet-<time>.ips`                  | Crash reporter of macOS                | Exception type, signal, and native frames of the crash  |
|  [06]   | TRX file                             | `--report-trx`                         | Results, recovered after a crash                        |

```bash
tail -n +2 ~/Library/Logs/DiagnosticReports/dotnet-<time>.ips | jq -r '.exception.type, .exception.signal, (.faultingThread as $t | .threads[$t].frames[:12][] | .symbol // "?")'
pgrep -fl 'Tests.dll'
dotnet-stack report -p <pid>
lldb -p <pid> --batch -o 'thread backtrace all' -o detach
```

- `dotnet-stack report` prints managed stacks of a live test host through the diagnostics socket, the fastest read of a hang
- `lldb` attaches to Rhino's bundled .NET host, which carries `get-task-allow`, and `dotnet-sos install` adds `clrstack` and `clrthreads` to it
- Crash and hang dumps of Rhino reach gigabytes at type `Full`, type `Mini` keeps them near 200 MB
- `DOTNET_DbgEnableMiniDump=1` with `DOTNET_DbgMiniDumpName` and `DOTNET_EnableCrashReport=1` in the `EnvironmentVariables` of one `Exec` task dumps that process alone outside Microsoft.Testing.Platform, a global value makes self-contained .NET tools print createdump warnings

## [02]-[HOST]

### [02.1]-[INSTALL]

- Rhino for Mac installs as `Rhino <major>.app` in `/Applications` or `~/Applications`, and the host takes the major from the RhinoCommon pin
- Rhino for Mac licenses through the Rhino account signed in to the app once by hand, with no token or scripted license key
- `RHINO_TOKEN` licenses Windows Server and Linux alone, and a single-computer key binds one machine for good
- Rhino ships its .NET host at `Rhino <major>.app/Contents/Frameworks/RhCore.framework/Resources/dotnet/<arch>/dotnet`, the runtime Rhino itself runs on

### [02.2]-[BOOT]

Rhino for Mac boots inside the test process, with the process main thread running Rhino's run loop:
- Rhino.Inside 9 on macOS boots headless alone: its `RhinoCore` ignores arguments and `WindowStyle`, and its `Run`, `DoEvents`, and `DoIdle` throw `EntryPointNotFoundException`
- `Main` adds a CoreFoundation run loop source, starts the test framework on a worker, and runs `CFRunLoopRun()`
- Assembly fixtures signal that source, and Rhino boots on the main thread
- Boot hands CoreCLR to Rhino (`GetCLRRuntimeHost` into `coreclr_handle` of `libxamarin-dotnet-coreclr.dylib`) and opens Rhino's frameworks with `RTLD_GLOBAL`
- `RhSetDotNetStartProc` then has Rhino load `dotnetstart.<runtime major>.dll` and call `Start`, `"headless"` for mode `Headless`
- Mode `Interface`, the default, calls `RhinoMacLibraryStart(0)` under `MRApplication` and `AppController`, with views and documents from `NSDocumentController`
- Configuration `Headless` (`dotnet test -c Headless`) calls `RhinoMacLibraryStart(1)` without views, the mode CI runs
- Module initializers resolve Rhino and plug-in assemblies from `RhCore.framework/Resources` and each `ManagedPlugIns/*.rhp` folder, Grasshopper 2 included
- Booted main threads run `NSApplication.Run()`, and `RhinoApp.InvokeOnUiThread`, Eto `InvokeAsync`, and `SwitchToMainThreadAsync` reach them
- `InvokeOnUiThread` returns at once whether or not the main thread is busy, `InvokeAndWait` blocks and shares static state across callers
- End with `_exit` after results are written: teardown crashes in static destructors, and Cycles and RhinoCode threads hang a normal return

Headless Rhino for Mac behaves in ways tests meet:
- Every document is headless, `RhinoDoc.Create` included, and headless documents raise no `RhinoDoc` events
- Commands prompting in a headless document crash the process or block the main thread, tests call the code a command calls
- `PlugIn.LoadPlugIn(<path>)` registers the plug-in in the user's Rhino profile for later sessions, `PlugIn.LoadPlugIn(<guid>)` loads an installed one
- Startup prints `Could not find <assembly>` and `3DconnexionNavlib` lines that change nothing

### [02.3]-[PROFILE]

Each boot writes Rhino's profile under `HOME`: cloned `settings-*.xml`, RhinoCycles, RhinoCode, and Grasshopper 2 logs, and on first boot template files, Grasshopper 2 resources, and plug-in install scans. The host keeps every run off the owner's profile:
- `Main` copies `McNeel/RegInfo.json`, `License Manager`, and `keypairs` of the owner's `Rhinoceros/<major>.0` into a temporary `HOME`, sets `HOME` and `CFFIXED_USER_HOME`, and deletes the folder after the run
- `keypairs` decrypts the cached Cloud Zoo lease, and a profile lacking it deadlocks boot in `GetCloudZooLease` on the main thread
- `cfprefsd` ignores both, and the background library moves the host process's domain and `com.mcneel.rhinoceros` domains into the scratch profile
- Background library (`Background.m`, injected through `DYLD_INSERT_LIBRARIES`) keeps windows unordered and activation prohibited, and Rhino never takes focus
- Unordered windows never paint: `DisplayPipeline` draw events stay silent, a view redraw leaves nothing a test reads, and `CaptureToBitmap` renders on its own
- It answers `NSAlert.runModal` and `runModalForWindow:` with abort and logs their text to `RHINO_TEST_ALERTS`, and the host fails the test that raised one
- One Rhino runs per machine, as Rhino processes sharing one Cloud Zoo license raise a license prompt in each, failing the test
- Hosts wait for other test processes on Rhino's .NET host before booting, and a Rhino app open beside them raises the prompt
- Boot runs from the assembly fixture with a two-minute limit, out of reach of `--timeout` and hang dumps, and a boot stuck in licensing fails every test

### [02.4]-[MSBUILD]

Template project holds every setting below:

| [INDEX] | [SETTING]                                       | [VALUE]                       | [EFFECT]                                                     |
| :-----: | :---------------------------------------------- | :---------------------------- | :----------------------------------------------------------- |
|  [01]   | `RhinoCommon`, `Grasshopper`, `Grasshopper2`    | `ExcludeAssets="runtime"`     | Compile against packages, load installed copies              |
|  [02]   | `RhinoVersion` property                         | RhinoCommon pin               | Package rows, Rhino path, runtime, license seed              |
|  [03]   | `TargetFrameworks`, `EnableWindowsTargeting`    | `net10.0;net10.0-windows`     | Runtime Rhino 9 hosts, both frameworks built on every OS     |
|  [04]   | `Compile Remove` per framework                  | `Windows.cs`, `Darwin.cs`     | One boot file and its references compile per framework       |
|  [05]   | `IsTestProject`, `IsTestingPlatformApplication` | `false` off framework's OS    | `dotnet test` runs `net10.0` on macOS, `-windows` on Windows |
|  [06]   | `XunitAutoGeneratedEntryPoint`                  | `false`                       | Host's own `Main`                                            |
|  [07]   | `GenerateSelfRegisteredExtensions`              | `true`                        | `SelfRegisteredExtensions.AddSelfRegisteredExtensions`       |
|  [08]   | `AllowUnsafeBlocks`                             | `true`                        | `LibraryImport` generator, function pointers                 |
|  [09]   | `Configurations`, `RhinoMode` option            | `Interface;Headless`          | `Interface` default, `Headless` in CI                        |
|  [10]   | `RuntimeEnvironmentVariable`                    | `DYLD_INSERT_LIBRARIES`       | Background library loaded into every macOS test run          |
|  [11]   | `TestingPlatformCommandLineArguments`           | TRX per run, hang, crash dump | Options every run starts with                                |
|  [12]   | `CsCheck`, `CsCheck_Threads`                    | `1`                           | Property samples on one thread                               |
|  [13]   | `RunCommand`, `RunArguments`                    | Rhino's host, `exec <dll>`    | Tests run on the runtime Rhino uses                          |

- `RhinoApplication` names `Rhino <major>.app` on macOS and `CoreDllPath` under `HKLM\SOFTWARE\McNeel\Rhinoceros\<major>.0\Install` on Windows
- Target `ResolveRhinoApplication` runs in every build and before `ComputeRunArguments`, fails a build or run missing Rhino or its runtime, and drops copy-local assemblies Rhino ships, so build output depends on the install
- Target `BuildBackground` compiles `Background.m` with `xcrun clang -dynamiclib` beside the test assembly on macOS
- Both frameworks build on every OS into one lock file, and locked restore passes on each OS
- `ExcludeAssets="runtime"` keeps every Rhino assembly out of build output, a copied RhinoCommon loads beside Rhino's and breaks type identity

### [02.5]-[WINDOWS]

- `Resolver.Initialize()` of Rhino.Inside finds Rhino by its own major through `HKLM\SOFTWARE\McNeel\Rhinoceros\<major>.0\Install`
- Rhino.Inside resolves from `System`, `System\netcore`, and `Plug-ins\Grasshopper` alone, and Grasshopper 2 sits in `Plug-ins\Grasshopper2\net8.0`
- `Main` resolves the rest from each .NET folder under `Plug-ins` (`net*.*`, never `net48`), as xUnit discovery loads exported types before boot
- `Main` starts the test framework on a worker, and on the boot request constructs `RhinoCore` on the main thread (`NoWindow` by default, `Hidden` for mode `Interface`)
- Main threads then pump Rhino with `RhinoCore.DoEvents()` and Eto with `Application.Instance.RunIteration()` until tests finish, and end with `TerminateProcess`, as native teardown calls into a stopped runtime
- `DoEvents` dispatches messages only while a Rhino window is active, none under `NoWindow`, so Eto `InvokeAsync` and `SwitchToMainThreadAsync` wait forever without `RunIteration`
- Tests run on the system .NET host, its folder read from `InstallLocation` under `HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\<arch>` (32-bit view) for the OS architecture
- Windows Server licenses Rhino through `RHINO_TOKEN` alone, and Windows desktops through the signed-in account

## [03]-[TEST_DESIGN]

### [03.1]-[ASSERTIONS]

Tests prove the code under test decides correctly, RhinoCommon's own behavior is McNeel's to test:
- Assert contract properties of each result (validity with its log, solidity, counts, topology) and measures against an analytic or independent value
- State each law the code owns (a second reconcile plans nothing) as a CsCheck property over generated inputs
- Name each branch the code owns (invalid input, degenerate size, boundary values) in theory data, properties cover the space between
- Check interpolations at an interior law (`Interpolate(I, R(θ), t) ≈ R(tθ)`), end points alone pass a constant or nearest-end implementation
- Assert absence and refusal as values (`null`, `Guid.Empty`, an error message) when the code reports failure that way
- Break the subject once (drop a cap, scale an output) and watch each test fail, tests passing on a fixed or skipped result prove nothing
- Read written state back (object table, layer table, saved file) in place of trusting a `true` from the write
- View tests compare captures taken before and after the change, as new views draw wireframe and one pixel can land on background

### [03.2]-[TOLERANCES]

- Tolerances come from the operation: `ModelAbsoluteTolerance` and `ModelAngleToleranceRadians` for documents, the subject's argument for geometry
- Assertion tolerance scales with the quantity (area times length tolerance for a volume), a fixed `1e-9` fails large models and hides small defects
- `RhinoMath.ZeroTolerance` and `RhinoMath.SqrtEpsilon` bound floating error of exact arithmetic, never modeling tolerance
- Generators place values on the model tolerance grid inside a model's range (integers times `0.001`), arbitrary doubles reach denormals no model holds
- Angle comparisons stay above `Vector3d.VectorAngle` noise (about `1e-8` rad for near-equal unit vectors)
- `Transform.GetQuaternion` returns true with a NaN quaternion for turns at or past about 177.4°, rotations interpolate through Silk.NET.Maths `Quaternion<double>`
- `Mesh.SolidOrientation` reads a cached flag that `Flip` leaves and `DuplicateMesh` copies, a flipped result is a new `Mesh` filled by `Append` and then flipped, and tests read orientation from face normals
- Units stay explicit: set `ModelUnitSystem` on documents a test creates and convert with `RhinoMath.UnitScale`

### [03.3]-[DOCUMENTS]

- Each test creates its document with `RhinoDoc.CreateHeadless(null)` or `RhinoDoc.OpenHeadless(<file>)` inside `using`, and names it by reference, never through `RhinoDoc.ActiveDoc`
- Document, plug-in, and command work runs on Rhino's main thread, collect observations there as a tuple and assert on the test thread
- Geometry construction and queries run on any thread
- Undo needs `BeginUndoRecord` and `EndUndoRecord` around a change, `GetUndoRecords` proves grouping and `Undo` proves reversal
- `ObjectCount(new ObjectEnumeratorSettings())` counts active objects, `Objects.Count` counts objects an undo removed
- `Audit` after each mutation sequence catches table defects no single object reports
- Files round trip through `Write3dmFile` with `SuppressDialogBoxes` and `File3dm.Read`, into a temporary folder the test deletes
- Failed tests attach their document, written as `.3dm`, to the test result through `TestContext.Current.AddAttachment`

### [03.4]-[ISOLATION]

- Rhino boots once per process, a second boot throws, and the host boots it from an assembly fixture
- Tests run serially: command window, active document, plug-in registry, and exception reports are process-wide
- Class or collection fixtures load what a group of tests needs (Grasshopper 2, a plug-in), unrelated tests skip the cost
- Test data (`.3dm`, `.gh`, `.ghz`) sits beside the tests with `CopyToOutputDirectory` and loads from `AppContext.BaseDirectory`
- Properties sample on one thread (`RuntimeEnvironmentVariable` `CsCheck_Threads` `1`), as RhinoCommon and documents are single-threaded
- Properties draw new inputs each run, a failure is a defect the code always held: rerun its printed seed, pin the shrunk input as a `[Fact]`, fix the cause
- Properties that mutate a document open a headless document per sample through `Documents.With`, a shared document carries state into later samples and shrinks
- Mode-specific tests skip through `[Fact(Skip = "<reason>", SkipUnless = nameof(<property>))]`, before any setup runs
- Formatting uses invariant culture, and limits count iterations in place of time
- Timeouts guard waits (Grasshopper 2 solutions, hang dumps), never pacing

### [03.5]-[GROUPING]

- One test class per unit under test, sections per contract (geometry, document, outputs) under `// --- [GROUP]` dividers
- Test names state the contract (`PlacedSlabsShareOneUndoRecordAndSurviveSave`), the failure message names the broken part
- Rhino-bound and Rhino-free tests sit in separate projects, the Rhino-bound project behind the install gate
