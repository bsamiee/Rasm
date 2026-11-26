# [PRIVACY]

Covers macOS privacy grants (TCC) that tests reading another process need, from the service a call reads to the row tccd answers it with on a Mac. Use [ci.md](ci.md) for grants on the hosted runner.

## [01]-[SERVICES]

Services carry the `kTCCService` prefix, and each System Settings pane sits under Privacy & Security:

| [INDEX] | [SERVICE]       | [PANE]                          | [DATABASE] | [READ_BY]                                                           |
| :-----: | :-------------- | :------------------------------ | :--------- | :------------------------------------------------------------------ |
|  [01]   | `Accessibility` | Device Control and Data Access  | System     | Each outside read of `Fixture`                                      |
|  [02]   | `PostEvent`     | Device Control and Data Access  | System     | `KeyboardTests`, `MenuTests`, `RouteTests`                          |
|  [03]   | `ScreenCapture` | Screen & System Audio Recording | System     | `ScreenshotTests`, `RecordingTests`, `MenuTests`, `recognizeText`   |
|  [04]   | `ListenEvent`   | Input Monitoring                | System     | None, a listen-only tap (`.listenOnly`) joins                       |
|  [05]   | `AppleEvents`   | Automation                      | Per-user   | None, `AppleEventTests` sends to its own process, which needs no consent |

- Accessibility rows answer PostEvent and ListenEvent requests too: tccd allows both for a client holding an Accessibility row alone
- `TextRecognition` captures a window before reading it, so `Watch.recognizeText(in:)` reads ScreenCapture while an image read needs no grant

Each service has one check and one request call, and `Grant` wraps Accessibility, PostEvent, and ScreenCapture:

| [INDEX] | [SERVICE]       | [CHECK]                                                       | [REQUEST]                                          |
| :-----: | :-------------- | :------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `Accessibility` | `AXIsProcessTrusted()`                                        | `AXIsProcessTrustedWithOptions`, prompt option set |
|  [02]   | `PostEvent`     | `CGPreflightPostEventAccess()`                                | `CGRequestPostEventAccess()`                       |
|  [03]   | `ScreenCapture` | `CGPreflightScreenCaptureAccess()`                            | `CGRequestScreenCaptureAccess()`                   |
|  [04]   | `ListenEvent`   | `CGPreflightListenEventAccess()`                              | `CGRequestListenEventAccess()`                     |
|  [05]   | `AppleEvents`   | `AEDeterminePermissionToAutomateTarget`, `askUserIfNeeded` no | Same call, `askUserIfNeeded` yes, target running   |

- `CGRequestScreenCaptureAccess()` prompts an undecided client alone, and a denied client goes to the pane (`CGWindow.h`)
- `Program.require()` requests each missing grant once per designated requirement, stored under `grants.requested.<grant>`, then checks them all
- Tests call checks alone, through the library members under test, as a prompt takes focus from the fixture

## [02]-[DATABASES]

System and per-user files hold rows on macOS 27, and `lsof` lists both as tccd holds them open:

```bash
sudo lsof -F n -c tccd | rg 'TCC.db$'
sudo sqlite3 -readonly '/Library/Application Support/com.apple.TCC/TCC.db' "select service, client, client_type, auth_value, auth_reason, hex(csreq) from access where client like '%/xctest'"
```

- System rows read under `sudo sqlite3 -readonly` from a terminal holding Full Disk Access, and tests and agents never write either file
- Columns read as `client_type` 0 for a bundle id and 1 for a path, `auth_value` 0 for denied and 2 for allowed, `csreq` the stored requirement
- Per-user rows moved to `/private/var/containers/Data/ProtectedSystem/<UUID>/Data/Library/Application Support/com.apple.TCC/TCC.db`
- `sqlite3` answers "authorization denied" on that file even under `sudo`, so the Automation pane is its only reader
- `~/Library/Application Support/com.apple.TCC/` no longer exists, and a tool reading it reads nothing

## [03]-[CLIENTS]

tccd judges the responsible process, which is the caller itself unless another process launched it as its own:
- Bundles are keyed by bundle id (`client_type` 0), bare executables by path (`client_type` 1), so each path of an executable is a new client
- `responsibility_get_pid_responsible_for_pid` names that process, and `Grant.missing` reads it to throw `.noGrant` or `.notResponsible(grant, pid)`
- `xcodebuild` starts `<Xcode>/Contents/Developer/Platforms/MacOSX.platform/Developer/Library/Xcode/Agents/xctest` responsible for itself
- `Fixture` started by a test through `Subprocess` is charged to `xctest`, so it needs no grant of its own
- Commands run from a shell are charged to the terminal app, so a tool passing from WezTerm can fail when launched any other way

tccd logs each request as lines sharing one `msgID`: `AUTHREQ_CTX` names the service, `AUTHREQ_SUBJECT` the judged client, `AUTHREQ_RESULT` its `authValue`:

```bash
sudo log show --last 1h --predicate 'process == "tccd" AND eventMessage CONTAINS "AUTHREQ"' --style compact
```

## [04]-[REQUIREMENTS]

Rows store the client's designated requirement in `csreq`, and tccd allows a process whose signature satisfies it. `codesign -d -r-` prints the requirement and `csreq -r- -b` compiles it into the blob a row holds:

```bash
client="$(xcode-select -p)/Platforms/MacOSX.platform/Developer/Library/Xcode/Agents/xctest"
codesign -d -r- "${client}" 2>/dev/null | sed -n 's/^# designated => //p' | csreq -r- -b /dev/stdout | xxd -p | tr -d '\n'
```

- Output equal to `hex(csreq)` of the client's row, compared case-blind, proves the row still covers the installed binary
- Apple signs `xctest` ad hoc, so its requirement names its cdhashes (`cdhash H"<hash>" or cdhash H"<hash>"`) and each Xcode build is a new client
- Bundles signed with a certificate keep their grant across rebuilds while identifier and certificate hold, as their requirement names both

## [05]-[TESTS]

Grant-bound tests run their act with `try`, and a missing grant fails them with an `ApplicationFailure` naming it:
- `Instance.capture` and `Instance.recording` call `Grant.screenCapture.check()` before any capture
- Accessibility reads map `AXError.apiDisabled` to `Grant.accessibility.missing` while `AXIsProcessTrusted()` is false
- `.noGrant(grant)` reads "<process> lacks the <grant> grant", `.notResponsible(grant, pid)` names the responsible process holding none
- `CGEvent.post(tap:)` and `postToPid` return no status, so posting code calls `Grant.postEvent.check()` first or loses the cause
- Assertions read the reason with `#expect(throws: ApplicationFailure.self)` and `reason.is(\.noGrant)` where a test asserts the refusal itself

## [06]-[SCREEN_CAPTURE_ALERTS]

replayd alerts on screen captures that bypass the system picker, ScreenCaptureKit captures included, and the alert takes focus from the fixture:
- Approvals sit in `~/Library/Group Containers/group.com.apple.replayd/ScreenCaptureApprovals.plist`, one dictionary per client
- Dictionaries hold `kScreenCaptureApprovalLastAlerted` and `kScreenCapturePrivacyHintDate`, and dates at `4000-01-01` hold off the alert
- `nx install` of an Xcode project archiving an app writes a row for each installed app's bundle id, then sends `HUP` to a running replayd
- Bare executables take their path as key, the form the hosted image writes for its own client

## [07]-[AUTOMATION]

Automation consent covers one client and one target, stored per-user with the target as indirect object (bundle id, or path for a bare executable):
- Core events (`quit`, `open`) pass without consent and prove nothing, and a gated event (a scripting command, a Finder query) proves consent
- Gated sends raise the prompt themselves, and the unified log line "Prompting for access to indirect object" shows it
- Denial answers `-1743` (`errAEEventNotPermitted`), which `AppleEventFailure.undelivered` turns into "Automation permission denied for <target>"
- `AEDeterminePermissionToAutomateTarget` answers `-1744` for an undecided pair and `procNotFound` for a target not running
- Apps sending events state `INFOPLIST_KEY_NSAppleEventsUsageDescription`, and hardened apps hold `com.apple.security.automation.apple-events`
- `osascript` hides the Apple event status, so a Swift `NSAppleEventDescriptor` sender or Atelier reads it
- `tccutil reset AppleEvents <bundle id>` clears a client's pairs, and its next gated send prompts again

## [08]-[GRANTS_ON_A_MAC]

Grants on a Mac go through each pane's add button, as no command line adds a row:
- Add `xctest` by its path through the open panel's Go to Folder (Command-Shift-G), as the panel hides package contents
- Agents drive System Settings through Rasad's `inspect` and `act`
- `tccutil reset <service> <bundle id>` takes installed bundle ids alone, so a path row leaves through the pane's minus button
- Each Xcode build needs `xctest` added again to every pane its tests read, and failing tests naming the grant signal it

MDM grants nothing a test needs on macOS 27:
- PPPC profiles granting Accessibility are deprecated, and the grant raises a "Device Control and Data Access Added" notice the user can revoke
- Screen Recording and Input Monitoring payloads deny alone
- Declaration `app.settings` raises a consent prompt for AppKit apps on a supervised Mac
- `profiles` installs nothing from the command line, its help naming the Profiles pane as the route
