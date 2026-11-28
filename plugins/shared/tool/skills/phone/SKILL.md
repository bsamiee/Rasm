---
name: phone
description: "Use when work reads or drives the owner's paired iPhone from this Mac (screenshots, taps, typing, apps, URLs, pasteboard, settings, location, logs, files, metrics), covering devicectl, Device Hub's mirror driven through Rasad for taps and typing, pymobiledevice3 input when Device Hub cannot act, lock and owner steps, and Xcode's simulator-only DeviceInteraction tools"
---

# [TOOL_PHONE]

Mac reads the phone over CoreDevice on the same Wi-Fi or a cable, with nothing installed on the phone. `devicectl` runs every job it has a command for, `pymobiledevice3` adds element captions, Media files, live logs, and metrics, and Rasad drives taps and typing through the phone's mirror in Xcode's Device Hub. Phone settings already on: Developer Mode, Developer > Enable UI Automation, Safari > Advanced > Web Inspector and Remote Automation.

## [01]-[TARGET]

Every task opens with one listing and one lock read, and later calls name the phone by the UDID it prints:

```bash
devicectl list devices --filter "Reality = 'physical'" -q -j - | jq -c '.result.devices[] | {name: .deviceProperties.name, udid: .hardwareProperties.udid, tunnel: .connectionProperties.tunnelState}'
devicectl device info lockState -d <udid> -q -j - | jq -c .result
devicectl device info displays -d <udid> -q -j - | jq -c '.result.displays[0] | {bounds, pointScale}'
```

- `-q -j -` prints the versioned JSON result alone on stdout, `-j <f>` writes it to a file, human output is unstable across releases
- `passcodeRequired: true` means locked: taps, typing, app launches, and URL opens wait for the owner to unlock, asked once with the reason
- Auto-Lock locks the phone again after 5 idle minutes
- Locked phones still answer state reads, screenshots, lock screen captions, pasteboard, files, logs, and metrics
- `-d` takes the UDID, CoreDevice id, ECID, serial, or name, and never a tailnet DNS name, as CoreDevice discovery stays on the local network
- `pymobiledevice3` picks the one paired phone itself, as `PYMOBILEDEVICE3_NATIVE=1` in every shell routes it through Apple's `remoted` tunnel
- Face ID, passkeys, Apple Account passwords, camera, and unlock stay the owner's

## [02]-[JOBS]

Taps, typing, and buttons run through Device Hub's mirror in [03], and this table holds every other job:

| [INDEX] | [JOB]           | [COMMAND]                                                                                                             |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Screenshot      | `devicectl device capture screenshot -d <udid> --destination <f>.png`                                                 |
|  [02]   | Captions        | `pymobiledevice3 developer accessibility list-items`                                                                  |
|  [03]   | Apps            | `devicectl device info apps -d <udid> --include-all-apps -s <name> -q -j -`                                           |
|  [04]   | Install         | `devicectl device install app -d <udid> <app>.app`, then `process launch --console` streams its output                |
|  [05]   | Launch, quit    | `devicectl device process launch -d <udid> --terminate-existing <bundle>`, `... terminate --pid <pid>`                |
|  [06]   | Processes       | `devicectl device info processes -d <udid> -s <name> -q -j -`                                                         |
|  [07]   | Open a URL      | `devicectl device process openURL -d <udid> '<url>'`                                                                  |
|  [08]   | Pasteboard      | `printf '%s' '<text>' \| devicectl device pasteboard copy -d <udid>`, `... paste`, `... info`                         |
|  [09]   | Appearance      | `devicectl device info appearance -d <udid>`, `... settings appearance -d <udid> --mode dark`                         |
|  [10]   | Orientation     | `devicectl device orientation get\|set\|rotate -d <udid>`                                                             |
|  [11]   | Location        | `devicectl device simulate location coordinate -d <udid> --latitude <lat> --longitude <lon>`, `... clear`             |
|  [12]   | Media files     | `pymobiledevice3 afc ls\|pull\|push\|rm` under `/` (DCIM, Downloads, Books)                                           |
|  [13]   | Crash logs      | `devicectl device info files -d <udid> --domain-type systemCrashLogs`, `... copy from --source <p> --destination <f>` |
|  [14]   | Live log        | `timeout <s> pymobiledevice3 syslog live -pn <process> -m <text>`                                                     |
|  [15]   | Log archive     | `sudo log collect --device-udid <udid> --last <n>m --output <f>.logarchive`, then `log show <f>.logarchive`           |
|  [16]   | Metrics         | `xcrun xctrace record --device <udid> --template 'Activity Monitor' --all-processes --time-limit <n>s`                |
|  [17]   | Battery         | `pymobiledevice3 diagnostics battery single`                                                                          |
|  [18]   | Network shaping | `pymobiledevice3 developer dvt condition list`, `... set <profile>` held open, `... clear`                            |
|  [19]   | Safari pages    | `pymobiledevice3 webinspector opened-tabs`, `... js-shell`, `... cdp`                                                 |

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

Taps, typing, and buttons run through Device Hub's mirror, driven by Rasad (`DeviceHub` in `applications`), by the owner's ruling, its focus change for the length of each act included. A press on the mirror lands on the phone at the same point, and Device Hub passes input only while its window is in front and the Mac's screen is unlocked.

[STEPS]:
1. Open: `applications` open `DeviceHub` (it opens behind), then press `View Screen` when the window shows no mirror
2. Read: `view` of the window (`of` its reference) shows the live mirror from its backing store with Device Hub behind, and its pixels are the points `act` takes
3. Locate: take the target's center in that image, add `list-items` captions to tell lookalike controls apart
4. Front: `applications` lists the front application, then open `DeviceHub` with `activate: true`
5. Act: one `act` call holding every step, pointer presses with `hold: 0.15`, Device Hub's `Home` and `Rotate Left` buttons
6. Type: in the same call, `Capture Keyboard` to on, key steps, then `Capture Keyboard` to off, as Device Hub sends every Mac key to the phone while it is on
7. Return: open the noted front application with `activate: true` in the next call, then open `DeviceHub` without `activate`, as each open retargets the handle
8. Verify: `view` the mirror again before the next act, as presses land on what moved during an animation

Mirror cases:
- Characters outside the Mac keyboard layout (emoji) -> `pasteboard copy` the text, press the field with `hold: 1`, and press Paste
- Key steps type accented letters the layout holds and a wrong letter for emoji
- Mirror shows "Error displaying screen contents" -> press its `Close`, then `View Screen`
- Rasad drops its connection mid-call -> open `DeviceHub` again, set `Capture Keyboard` to off, then return focus, before any other step
- Holding Escape for 1.2 s also ends keyboard capture

### [03.1]-[HID_FALLBACK]

HID input runs only when Device Hub cannot act: the Mac's screen is locked, or the mirror keeps erroring after `Close` and `View Screen`. `HID` stands for `pymobiledevice3 developer core-device universal-hid-service`, and coordinates run 0 to 65535, `x = round(px * 65535 / W)` and `y = round(py * 65535 / H)`, where `W` and `H` are the size of the image `px` and `py` were read from:

| [INDEX] | [JOB]         | [COMMAND]                                                                                        |
| :-----: | :------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | Tap, scroll   | `HID tap -- <x> <y>`, `HID drag -- <x1> <y1> <x2> <y2>`, `HID multi-tap`, `HID multi-drag`       |
|  [02]   | Gesture batch | `printf 'tap <x> <y>\nsleep 0.5\ndrag <x1> <y1> <x2> <y2>\n' \| HID session`                     |
|  [03]   | Type, keys    | `HID type '<ascii>'`, `HID key enter\|backspace\|escape\|tab\|left\|right\|up\|down --count <n>` |
|  [04]   | Buttons       | `pymobiledevice3 developer core-device hid button home\|volume-up\|volume-down\|mute\|siri`      |

- `HID drag` scrolls, `HID swipe` moves a pointer with no contact and scrolls nothing
- `HID session` opens one input stream for a whole batch, where back-to-back single calls each open their own
- `HID type` sends printable ASCII alone, turns a newline into Return, and drops or doubles a character in some composers (ChatGPT)

## [04]-[CHANGES]

Writes to the owner's phone restore what they changed:
- Settings, orientation, location, and pasteboard -> read the current value first (`info appearance`, `orientation get`, `pasteboard info` and `paste`), write, then restore and read back
- `pasteboard copy` replaces the whole item and `--file` makes one item per file, so an item `info` lists with more than one flavor comes back as text alone
- `hid button lock` locks the phone and hands the next step to the owner
- `settings reset`, `reboot`, `uninstall app`, `rename`, `settings biometrics --disable`, `profile install\|remove`, and `manage unpair` run on the owner's word alone

## [05]-[FAILURES]

`devicectl` errors carry a `CoreDeviceError` code in the JSON `error` object:
- `4000` "disconnected immediately after connecting", `21005` -> rerun once, the first call over Wi-Fi wakes the tunnel
- `1001` "capability ... is not supported by this device" -> the phone lacks it, `.result.devices[].capabilities[].featureIdentifier` of `list devices` names every capability it has
- `1000` device not found -> phone off the home Wi-Fi or asleep with no tunnel, `tunnelState` reads other than `connected`
- Unsupported on this phone: `capture screen-record`, `simulate statusBar`, `info audio`, `settings audio`, `settings standby`
- Screen video has no CoreDevice route, and a `capture screenshot` loop records state changes alone, as each screenshot takes seconds over Wi-Fi
- `pymobiledevice3 developer core-device get-lockstate` answers "not implemented", `devicectl device info lockState` reads it
- `bonjour` commands find nothing from a WezTerm shell (`No route to host`)

## [06]-[XCODE]

Xcode 27.2's MCP server (`xcrun mcpbridge`) lists DeviceInteraction tools with element frames, hit points, and Unicode typing, and serves simulators alone:
- `DeviceInteractionStartSession` on the phone answers "None of the available devices are supported for Device Interaction"
- Device pool adds physical-device rules only under an internal Xcode state flag (`DVTToolsInfo.runtimeInternalState`)
- No simulator runtime is installed and Rasm builds no iOS target, so no `.mcp.json` row carries `mcpbridge`
- Each new Xcode build reopens the question: one `DeviceInteractionStartSession` on the phone over a stdio `xcrun mcpbridge` after `sudo xcrun mcp-server enable --unsafe-always-allow-all-agents`, then `sudo xcrun mcp-server disable`
- `xcrun mcpbridge run-agent` launches Xcode's own agent download and leaves a hidden Xcode running, never the session's harness
