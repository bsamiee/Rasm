# [PRIVACY]

Covers macOS privacy grants (TCC) that library code checks and suites read the answers of, and the screen capture approvals `nx install` writes.

## [01]-[SERVICES]

`Grant` cases each name a `kTCCService` service with one check and one request call:

| [INDEX] | [SERVICE]       | [CHECK]                            | [REQUEST]                                          |
| :-----: | :-------------- | :--------------------------------- | :------------------------------------------------- |
|  [01]   | `Accessibility` | `AXIsProcessTrusted()`             | `AXIsProcessTrustedWithOptions`, prompt option set |
|  [02]   | `PostEvent`     | `CGPreflightPostEventAccess()`     | `CGRequestPostEventAccess()`                       |
|  [03]   | `ScreenCapture` | `CGPreflightScreenCaptureAccess()` | `CGRequestScreenCaptureAccess()`                   |

- Tests call checks alone, through the library members under test, as a prompt takes focus

## [02]-[CLIENTS]

tccd judges the responsible process, which is the caller itself unless another process launched it as its own:
- `responsibility_get_pid_responsible_for_pid` names that process, and `Grant.missing` reads it to throw `.noGrant` or `.notResponsible(grant, pid)`
- `xcodebuild` starts `<Xcode>/Contents/Developer/Platforms/MacOSX.platform/Developer/Library/Xcode/Agents/xctest` responsible for itself

## [03]-[TESTS]

Missing grants fail an act with an `ApplicationFailure` naming the grant:
- Accessibility reads map `AXError.apiDisabled` to `Grant.accessibility.missing` while `AXIsProcessTrusted()` is false
- `.noGrant(grant)` reads "<process> lacks the <grant> grant", `.notResponsible(grant, pid)` names the responsible process holding none
- Assertions read the reason with `#expect(throws: ApplicationFailure.self)` and `reason.is(\.noGrant)` where a test asserts the refusal itself

## [04]-[SCREEN_CAPTURE_ALERTS]

replayd alerts on screen captures that bypass the system picker, ScreenCaptureKit captures included, and the alert takes focus:
- Approvals sit in `~/Library/Group Containers/group.com.apple.replayd/ScreenCaptureApprovals.plist`, one dictionary per client
- Dictionaries hold `kScreenCaptureApprovalLastAlerted` and `kScreenCapturePrivacyHintDate`, and dates at `4000-01-01` hold off the alert
- `nx install` of an Xcode project archiving an app writes a row for each installed app's bundle id, then sends `HUP` to a running replayd
