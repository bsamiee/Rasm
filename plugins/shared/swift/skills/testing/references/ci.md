# [CI]

Covers `xcodebuild test` runs of `Rasm-Package`, local and in job `macos` of `.github/workflows/ci.yml`, which runs `test--xcodebuild` on a GitHub-hosted macOS image (`runs-on: xcode-27`).

## [01]-[SELECTION]

`xcodebuild test` runs the tests its scheme, test plan, and selection options name (`xcodebuild -help`):
- Runs naming no `-testPlan` take the scheme's default plan alone, and `-testPlan <name>` takes another plan the scheme references
- `-only-testing` reaches no target outside the selected plan or marked skipped in the scheme, failing with "isn't a member of the specified test plan or scheme"
- `-only-testing` and `-skip-testing` also take `@<path>`, a file of newline-delimited identifiers composing with inline ones
- `-enumerate-tests` lists the tests the same command selects, as identifiers `-only-testing` takes, and runs none, `-test-enumeration-format json` for machine output
- Test plans select or skip tests by identifier (`selectedTests`, `skippedTests`) and by Swift Testing tag (`selectedTags`, `skippedTags`), keys of Xcode's `IDEFoundation`

## [02]-[IMAGE]

Hosted macOS images hold a logged-in session and a TCC database a job can write:
- Jobs start on a new VM with passwordless `sudo` (GitHub docs, GitHub-hosted runners)
- SIP is off on hosted macOS images, an architectural limit (`actions/runner-images` issue 8162), so `sudo sqlite3` writes TCC rows
- Image builds write rows for their own clients (`/bin/bash`, `/opt/hca/hosted-compute-agent`) the same way (`configure-tccdb-macos-27.sh`)
- Image scripts find the per-user database as the one `TCC.db` six levels under `ProtectedSystem`, or the one `lsof -c tccd` lists
- Images log a user in and turn on `DevToolsSecurity` and Automation Mode without authentication (`configure-autologin.sh`, `configure-machine.sh`)
- Images turn off App Nap, sleep, and notification center, and close Finder windows as they interfere with UI tests (`configure-system.sh`)
- Runner client rows never cover a test, as `xctest` runs responsible for itself
- Rows for `xctest` derive at job time from the selected Xcode, as each Xcode build is a new client

Use [privacy](privacy.md) for TCC services, client identity, tccd log lines, and grants on a Mac.

## [03]-[GRANT_STEP]

Job `macos` grants the selected Xcode's `xctest` before any test runs:

```yaml
- run: |-
      client="$(xcode-select -p)/Platforms/MacOSX.platform/Developer/Library/Xcode/Agents/xctest"
      requirement="$(codesign -d -r- "${client}" 2>/dev/null | sed -n 's/^# designated => //p' | csreq -r- -b /dev/stdout | xxd -p | tr -d '\n')"
      rows="$(printf "('%s','${client}',1,2,4,1,X'${requirement}',NULL,NULL,'UNUSED',NULL,0,unixepoch())," kTCCServiceAccessibility kTCCServicePostEvent kTCCServiceScreenCapture)"
      sudo sqlite3 -bail '/Library/Application Support/com.apple.TCC/TCC.db' '.timeout 5000' "INSERT OR REPLACE INTO access (service, client, client_type, auth_value, auth_reason, auth_version, csreq, policy_id, indirect_object_identifier_type, indirect_object_identifier, indirect_object_code_identity, flags, last_modified) VALUES ${rows%,}"
      defaults write "${HOME}/Library/Group Containers/group.com.apple.replayd/ScreenCaptureApprovals.plist" "${client}" -dict kScreenCaptureApprovalLastAlerted -date '4000-01-01 00:00:00 +0000' kScreenCapturePrivacyHintDate -date '4000-01-01 00:00:00 +0000'
      ! pgrep -qx replayd || killall -HUP replayd
```

Step sits directly after `maxim-lobanov/setup-xcode` and before `nx affected`, as the action runs `sudo xcode-select -s` (action source) and `xcode-select -p` then names the Xcode that runs the tests:

| [INDEX] | [LINE]           | [EFFECT]                                                                            |
| :-----: | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `client`         | Path of the selected `xctest`, which tccd judges by path                            |
|  [02]   | `requirement`    | Designated requirement compiled by `csreq -b`, in hex for an `X'…'` literal         |
|  [03]   | `rows`           | One row per service: path client `1`, allowed `2`, system set `4`, version `1`      |
|  [04]   | `sqlite3 -bail`  | `INSERT OR REPLACE` into system `access`, exiting at its first error                |
|  [05]   | `.timeout 5000`  | Waits up to 5000 ms on a lock tccd holds                                            |
|  [06]   | `defaults write` | Approval of `xctest` in replayd, both alert dates in year 4000                      |
|  [07]   | `killall -HUP`   | Signal to a running replayd to read approvals again, as `install--xcodebuild` sends |

- `run` steps execute under `bash --noprofile --norc -eo pipefail` (GitHub docs, workflow syntax), so an empty requirement fails the step
- Failed steps skip `nx affected` through the default `success()` check (GitHub docs, expressions), and the run is red at the grant step
- Derived requirement blobs equal the blob tccd stores for a grant set in System Settings (tried against a Mac's Accessibility row)
- `INSERT OR REPLACE` overwrites a row on key `(service, client, client_type, indirect_object_identifier)`, where `INSERT OR IGNORE` keeps it
- Unlisted columns (`pid`, `boot_uuid`, `last_reminded`, `reminder_count`) take schema defaults (tried on a copy of a macOS 27.2 database)
- Services are the `Grant` cases suites read, and the Accessibility row answers ListenEvent requests, so a listen-only test adds no row
- Automation rows sit in the per-user database with their target, outside the step
- Approval dicts follow the form macOS 27.2 writes for each client, where the image writes a bare date for its own agent

First run of the step settles what no local Mac can try:

| [INDEX] | [OPEN]                                               | [SETTLED_BY]                                                 |
| :-----: | :--------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Hosted macOS 27.0 judging `Agents/xctest` as on 27.2 | Grant-bound suites green, or tccd subject lines of a red run |
|  [02]   | Dict form and `HUP` taking effect in hosted replayd  | No capture alert, window suites inside their time limit      |

Item 01 reads tccd's `AUTHREQ_SUBJECT` lines from a step under `if: ${{ failure() }}` (GitHub docs, expressions), as each job takes a new VM and its log ends with it.

## [04]-[SKIPS]

Grant-bound tests lose their `.enabled(if:)` conditions, as a skip enters the Nx remote cache as a pass:
- `test--xcodebuild` caches (`cache: true`) under inputs of workspace files, `Package.resolved`, `xcodebuild -version`, and compiler and SDK builds
- Inputs hold no TCC row and no workflow file, so a skipped green result restores on every later run with those inputs, granted or not
- Adding, changing, or removing the grant step reruns no test task, which runs again only where its inputs change or a run passes the cache
- Result bundles count skips (`skippedTests` of `xcresulttool get test-results summary`), where no grant-bound test appears

## [05]-[RESULT_BUNDLES]

Each `xcodebuild test` run of `test--xcodebuild` writes a result bundle into the target's declared outputs:
- `-resultBundlePath` writes build logs, test results, coverage, attachments, and diagnostic logs, creating its parent folders (`man xcodebuild`)
- Bundles take one path per run, named by scheme, test plan, and configuration, under the plugin's `outputPath` (`.artifacts/apple/<project>`)
- `xcodebuild` exits with an error where the path exists (`man xcodebuild`), so each run first removes a bundle a checkout or cache restore left
- Target `outputs` name the bundles, so a cache hit restores the bundle of the run it replays
- Bundles hold window captures of failing tests alone, as `CapturesOnFailure` records nothing on a green run
- Job `macos` adds `.artifacts/apple/**/*.xcresult` to its `reports` upload, which runs under `always()` past a red test step
- `upload-artifact` uploads a matched folder with every descendant and skips names opening with `.` below its search root (`@actions/glob`)

`gh run download <run> -n macos-reports` fetches the reports artifact of job `macos`, and `xcrun xcresulttool` reads each bundle (`xcresulttool help`):

| [INDEX] | [SUBCOMMAND]                                                             | [PRINTS]                                |
| :-----: | :----------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `get test-results summary --path <bundle>`                               | Result, counts, skipped count, failures |
|  [02]   | `get test-results tests --path <bundle>`                                 | Each test with its result               |
|  [03]   | `export attachments --path <bundle> --output-path <dir> --only-failures` | Attachments of failing tests            |

## [06]-[FLAKES]

CI runs take the target's `xcodebuild test` words alone, with no repetition or retry option:
- Hunts reproduce a red CI test on a Mac: `nx test Rasm-Package -- -only-testing:<id> -test-iterations <n> -run-tests-until-failure`
- `-run-tests-until-failure` stops at the first failure, after 100 iterations where `-test-iterations` is absent (`xcodebuild -help`)
- `-test-repetition-relaunch-enabled YES` starts each repetition in a new process, so a failure it ends names process state one repetition leaves
- `-collect-test-diagnostics on-failure` adds sysdiagnoses and log archives to a hunt's bundle (`man xcodebuild`)
- `-retry-tests-on-failure` excludes `-run-tests-until-failure` (`man xcodebuild`) and stays out of the target, job `macos`, and hunts

## [07]-[PROOF]

Grant step proves itself once on a branch, red without it and green with it:
1. Branch from a commit holding the step and grant-bound tests with no condition, as a skipping test is green either way
2. Remove the step on the branch and push
3. Dispatch `gh workflow run ci.yml --ref <branch> -f uncached=macos`, as a cached green result otherwise replays
4. Read red: each grant-bound test fails with `noGrant`, `notResponsible`, or `AXError` `-25211` naming its grant
5. Red for another reason (fixture launch, time limit) proves nothing, and its cause is fixed before the next run
6. Restore the step and dispatch again: green, with `skippedTests` 0 in each bundle
7. Delete the branch
