# [CI]

Covers `xcodebuild test` runs of `Rasm-Package`, local and in job `macos` of `.github/workflows/ci.yml`, which runs `test--xcodebuild` on a GitHub-hosted macOS image (`runs-on: xcode-27`).

## [01]-[SELECTION]

`xcodebuild test` runs the tests its scheme, test plan, and selection options name (`xcodebuild -help`):
- Runs naming no `-testPlan` take the scheme's default plan alone, and `-testPlan <name>` takes another plan the scheme references
- `-only-testing` reaches no target outside the selected plan or marked skipped in the scheme, failing with "isn't a member of the specified test plan or scheme"
- `-only-testing` and `-skip-testing` also take `@<path>`, a file of newline-delimited identifiers composing with inline ones
- `-enumerate-tests` lists the tests the same command selects, as identifiers `-only-testing` takes, and runs none, `-test-enumeration-format json` for machine output
- Test plans select or skip tests by identifier (`selectedTests`, `skippedTests`) and by Swift Testing tag (`selectedTags`, `skippedTags`), keys of Xcode's `IDEFoundation`

## [02]-[SKIPS]

Skipped tests enter the Nx remote cache as a pass:
- `test--xcodebuild` caches (`cache: true`) under inputs of workspace files, `Package.resolved`, `xcodebuild -version`, and compiler and SDK builds
- Inputs hold no workflow file and no machine state a condition reads, so a skipped green result restores on every later run with those inputs

## [03]-[RESULT_BUNDLES]

Each `xcodebuild test` run of `test--xcodebuild` writes a result bundle into the target's declared outputs:
- `-resultBundlePath` writes build logs, test results, coverage, attachments, and diagnostic logs, creating its parent folders (`man xcodebuild`)
- Bundles take one path per run, named by scheme, test plan, and configuration, under the plugin's `outputPath` (`.artifacts/apple/<project>`)
- `xcodebuild` exits with an error where the path exists (`man xcodebuild`), so each run first removes a bundle a checkout or cache restore left
- Target `outputs` name the bundles, so a cache hit restores the bundle of the run it replays
- Job `macos` adds `.artifacts/apple/**/*.xcresult` to its `reports` upload, which runs under `always()` past a red test step
- `upload-artifact` uploads a matched folder with every descendant and skips names opening with `.` below its search root (`@actions/glob`)

`gh run download <run> -n macos-reports` fetches the reports artifact of job `macos`, and `xcrun xcresulttool` reads each bundle (`xcresulttool help`):

| [INDEX] | [SUBCOMMAND]                                                             | [PRINTS]                                |
| :-----: | :----------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `get test-results summary --path <bundle>`                               | Result, counts, skipped count, failures |
|  [02]   | `get test-results tests --path <bundle>`                                 | Each test with its result               |
|  [03]   | `export attachments --path <bundle> --output-path <dir> --only-failures` | Attachments of failing tests            |

## [04]-[FLAKES]

CI runs take the target's `xcodebuild test` words alone, with no repetition or retry option:
- Hunts reproduce a red CI test on a Mac: `nx test Rasm-Package -- -only-testing:<id> -test-iterations <n> -run-tests-until-failure`
- `-run-tests-until-failure` stops at the first failure, after 100 iterations where `-test-iterations` is absent (`xcodebuild -help`)
- `-test-repetition-relaunch-enabled YES` starts each repetition in a new process, so a failure it ends names process state one repetition leaves
- `-collect-test-diagnostics on-failure` adds sysdiagnoses and log archives to a hunt's bundle (`man xcodebuild`)
- `-retry-tests-on-failure` excludes `-run-tests-until-failure` (`man xcodebuild`) and stays out of the target, job `macos`, and hunts
