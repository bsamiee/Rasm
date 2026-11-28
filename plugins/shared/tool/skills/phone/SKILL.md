---
name: phone
description: "Use when work reads or drives the owner's paired iPhone from this Mac (screenshots, taps, typing, apps, URLs, pasteboard, settings, location, logs, files, metrics), covering devicectl, pymobiledevice3 touch and typing, lock and owner steps, and Xcode's simulator-only DeviceInteraction tools"
---

# [TOOL_PHONE]

Mac reads and drives the phone over CoreDevice on the same Wi-Fi or a cable, with nothing installed on the phone and no window on the Mac. `devicectl` runs every job it has a command for, `pymobiledevice3` adds touch, typing, keys, element captions, Media files, live logs, and metrics. Phone settings already on: Developer Mode, Developer > Enable UI Automation, Safari > Advanced > Web Inspector and Remote Automation.

## [01]-[TARGET]

Every task opens with one listing and one lock read, and later calls name the phone by the UDID it prints:

```bash
xcrun devicectl list devices --filter "Reality = 'physical'" -q -j - | jq -c '.result.devices[] | {name: .deviceProperties.name, udid: .hardwareProperties.udid, tunnel: .connectionProperties.tunnelState}'
xcrun devicectl device info lockState -d <udid> -q -j - | jq -c .result
xcrun devicectl device info displays -d <udid> -q -j - | jq -c '.result.displays[0] | {bounds, pointScale}'
```

- `-q -j -` prints the versioned JSON result alone on stdout, `-j <f>` writes it to a file, human output is unstable across releases
- `passcodeRequired: true` means locked: taps, typing, element reads, app launches, and URL opens wait for the owner to unlock, an owner step asked once with the reason
- Locked phones still answer state reads, screenshots, pasteboard, files, logs, and metrics
- `-d` takes the UDID, CoreDevice id, ECID, serial, or name, and never a tailnet DNS name, as CoreDevice discovery stays on the local network
- `pymobiledevice3` picks the one paired phone itself, as `PYMOBILEDEVICE3_NATIVE=1` in every shell routes it through Apple's `remoted` tunnel
- Face ID, passkeys, Apple Account passwords, camera, and unlock stay the owner's

## [02]-[JOBS]

`HID` stands for `pymobiledevice3 developer core-device universal-hid-service`:

| [INDEX] | [JOB]           | [COMMAND]                                                                                                       |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | Screenshot      | `xcrun devicectl device capture screenshot -d <udid> --destination <f>.png`                                     |
|  [02]   | Tap, scroll     | `HID tap -- <x> <y>`, `HID drag -- <x1> <y1> <x2> <y2>`, `HID multi-tap`, `HID multi-drag`                      |
|  [03]   | Gesture batch   | `printf 'tap <x> <y>\nsleep 0.5\ndrag <x1> <y1> <x2> <y2>\n' \| HID session`                                    |
|  [04]   | Type, keys      | `HID type '<ascii>'`, `HID key enter\|backspace\|escape\|tab\|left\|right\|up\|down --count <n>`                |
|  [05]   | Buttons         | `pymobiledevice3 developer core-device hid button home\|volume-up\|volume-down\|mute\|siri`                     |
|  [06]   | Captions        | `pymobiledevice3 developer accessibility list-items`                                                            |
|  [07]   | Apps            | `xcrun devicectl device info apps -d <udid> --include-all-apps -s <name> -q -j -`                               |
|  [08]   | Launch, quit    | `xcrun devicectl device process launch -d <udid> --terminate-existing <bundle>`, `... terminate --pid <pid>`    |
|  [09]   | Processes       | `xcrun devicectl device info processes -d <udid> -s <name> -q -j -`                                             |
|  [10]   | Open a URL      | `xcrun devicectl device process openURL -d <udid> '<url>'`                                                      |
|  [11]   | Pasteboard      | `printf '%s' '<text>' \| xcrun devicectl device pasteboard copy -d <udid>`, `... paste`, `... info`             |
|  [12]   | Appearance      | `xcrun devicectl device info appearance`, `... settings appearance -d <udid> --mode dark --text-size <s>`       |
|  [13]   | Orientation     | `xcrun devicectl device orientation get\|set\|rotate -d <udid>`                                                 |
|  [14]   | Location        | `xcrun devicectl device simulate location coordinate -d <udid> --latitude <lat> --longitude <lon>`, `... clear` |
|  [15]   | Media files     | `pymobiledevice3 afc ls\|pull\|push\|rm` under `/` (DCIM, Downloads, Books)                                     |
|  [16]   | Crash logs      | `xcrun devicectl device info files -d <udid> --domain-type systemCrashLogs`, `... copy from --source <p>`       |
|  [17]   | Live log        | `timeout <s> pymobiledevice3 syslog live -pn <process> -m <text>`                                               |
|  [18]   | Log archive     | `sudo log collect --device-udid <udid> --last <n>m --output <f>.logarchive`, then `log show <f>.logarchive`     |
|  [19]   | Metrics         | `xcrun xctrace record --device <udid> --template 'Activity Monitor' --all-processes --time-limit <n>s`          |
|  [20]   | Battery         | `pymobiledevice3 diagnostics battery single`                                                                    |
|  [21]   | Network shaping | `pymobiledevice3 developer dvt condition list`, `... set <profile>` held open, `... clear`                      |
|  [22]   | Safari pages    | `pymobiledevice3 webinspector opened-tabs`, `... js-shell`, `... cdp`                                           |

- `HID drag` scrolls, `HID swipe` moves a pointer with no contact and scrolls nothing
- `HID session` opens one input stream for a whole batch, where back-to-back single calls each open their own
- `HID type` sends printable ASCII alone, a newline presses Return, and some composers (ChatGPT) drop or double a character
- `info apps` lists developer apps alone without `--include-all-apps`, Settings is `com.apple.Preferences`
- `list-items` reads captions of the foreground app with no frames, and fails "no focused element" on Settings and Claude
- `simulate location` holds until `clear`, `dvt condition set` holds while its process runs
- `xctrace record --output <f>.trace` names the trace, `xctrace export --input <f>.trace --toc` lists its tables
- `log collect` needs root and copies whole persisted chunks, hundreds of MB for a one-minute window
- `webinspector launch` holds a Safari automation window until the command exits
- `appDataContainer` domains open development-signed apps alone, App Store apps answer `ContainerLookupErrorDomain` 3
- `copy to --domain-type temporary --domain-identifier <name>` writes a scratch space `devicectl` cannot delete from
- `App-prefs:` URLs open Settings at Apps, where the app's name goes into search

## [03]-[TOUCH]

HID coordinates run 0 to 65535 across the screen, `x = round(px * 65535 / W)` and `y = round(py * 65535 / H)`, where `W` and `H` are the size of the image `px` and `py` were read from. Full screenshots match the `displays` bounds, and a downscaled copy keeps the same ratios.

[STEPS]:
1. Read: screenshot, then `sips -Z 1434 -s format jpeg <f>.png --out <f>.jpg` and Read the JPEG, as the full PNG is 16-bit at the display's native size
2. Locate: take the target's center in that image, add `list-items` captions to tell lookalike controls apart
3. Act: one `HID` call, or one `HID session` batch for a known sequence
4. Verify: screenshot again before the next act, as taps land on what moved during an animation
5. Unicode text: `pasteboard copy` the text, long-press the field, and tap Paste in the edit menu, in place of `HID type`

## [04]-[CHANGES]

Writes to the owner's phone restore what they changed:
- Settings, orientation, location, and pasteboard -> read the current value first (`info appearance`, `orientation get`, `pasteboard info` and `paste`), write, then restore and read back
- `pasteboard copy` replaces every flavor of the current item, so `paste --type <uti> > <f>` saves each flavor `info` lists before a write
- `hid button lock` locks the phone and hands the next step to the owner
- `settings reset`, `reboot`, `uninstall app`, `rename`, `settings biometrics --disable`, `profile install\|remove`, and `manage unpair` run on the owner's word alone

## [05]-[FAILURES]

`devicectl` errors carry a `CoreDeviceError` code in the JSON `error` object:
- `4000` "disconnected immediately after connecting", `21005` -> rerun once, the first call over Wi-Fi wakes the tunnel
- `1001` "capability ... is not supported by this device" -> the phone lacks it, `.result.devices[].capabilities[].featureIdentifier` of `list devices` names every capability it has
- `1000` device not found -> phone off the home Wi-Fi or asleep with no tunnel, `tunnelState` reads other than `connected`
- Unsupported on this phone: `capture screen-record`, `simulate statusBar`, `info audio`, `settings audio`, `settings standby`
- `pymobiledevice3 developer core-device get-lockstate` answers "not implemented", `devicectl device info lockState` reads it
- `bonjour` commands find nothing from a WezTerm shell (`No route to host`)

## [06]-[XCODE]

Xcode 27.2's MCP server (`xcrun mcpbridge`) lists DeviceInteraction tools with element frames, hit points, and Unicode typing, and serves simulators alone:
- `DeviceInteractionStartSession` on the phone answers "None of the available devices are supported for Device Interaction"
- Device pool adds physical-device rules only under an internal Xcode state flag (`DVTToolsInfo.runtimeInternalState`)
- No simulator runtime is installed and Rasm builds no iOS target, so no `.mcp.json` row carries `mcpbridge`
- Each new Xcode build reopens the question: one `DeviceInteractionStartSession` on the phone over a stdio `xcrun mcpbridge` after `sudo xcrun mcp-server enable --unsafe-always-allow-all-agents`, then `sudo xcrun mcp-server disable`
- `xcrun mcpbridge run-agent` launches Xcode's own agent download and leaves a hidden Xcode running, never the session's harness
