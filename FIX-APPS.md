# [FIX_APPS]

Faults found in Atelier, `watch-apps`, and `function-hooks` on November 10 and 11 while session `rasm-21` drove InDesign 2027 (Beta), each with its evidence, owner, required change, and check.

## [01]-[STATE]

Rows marked reported were fixed by session `rasm-98` and not rerun by `rasm-21`. Evidence files are in `.artifacts/fix-apps/`, which Git ignores.

| [INDEX] | [FAULT]                                                   | [OWNER]                                                | [STATE]         |
| :-----: | :-------------------------------------------------------- | :----------------------------------------------------- | :-------------- |
|  [01]   | Plugin uninstall deadlocks a running InDesign             | `libs/typescript/creative-cloud/uxp/plugin.ts`         | Open            |
|  [02]   | Plugin throws on document close                           | `apps/creative-cloud/atelier/plugins/indesign/`        | Open            |
|  [03]   | Watchers keep the `watch-apps` binary they started with   | `plugins/function-hooks/hooks/register.ts`             | Open            |
|  [04]   | Install starts a half-copied server helper                | `apps/creative-cloud/atelier/macos/Atelier.swift`      | Reported fixed  |
|  [05]   | Atelier reads off-screen export windows as `blocking`     | Atelier build of `libs/swift/macos/Applications`       | Reported fixed  |
|  [06]   | Idle app under load reads `unresponsive`                  | `libs/swift/macos/Applications/Workspace/Beat.swift`   | Reported fixed  |
|  [07]   | Hook sends each `unresponsive` line to agents as a hang   | `plugins/function-hooks/instances.ts`                  | Fixed in tree   |
|  [08]   | Off-screen splash window reads `blocking` in `watch-apps` | `libs/swift/macos/Applications/Workspace/Beat.swift`   | Fixed in tree   |
|  [09]   | Hook passes a relative bundle path to `launch` as a name  | `plugins/function-hooks/policies/apps.ts`              | Fixed in tree   |
|  [10]   | `launch` matched part of a name and opened Twinmotion     | `libs/swift/macos/Applications/Workspace/Search.swift` | Fixed, verified |

InDesign pid 90265 stays deadlocked from fault `[01]` until a person force quits it. `A PERSIAN GARDEN - WORKING.indd` was saved at 00:01:39 on November 11 with no later script call.

## [02]-[UNINSTALL_DEADLOCK]

`nx install Atelier` uninstalls `app.rasm.atelier.indesign` from a running InDesign, and InDesign froze at 00:22:23 on November 11:
- `xcodebuild` for Atelier ran at 00:20:33, and UPI sent `"action":"uninstall"` at 00:22:23 (`indesign-uxp.log` line 1635)
- InDesign logged `Plugin request failure ... for action uninstall` at 00:22:34 (line 1637)
- Later installs and uninstalls at 00:22:34, 00:23:21, 00:24:15, and 00:29:10 log success, and no unload ran
- One earlier uninstall in this InDesign run, at 00:01:03, finished in one second (line 1456), so the fault is a race
- InDesign recovery files were last written at 00:22:24, and the main thread logged nothing after 00:02:22 (`indesign-mainthread-log.txt`)
- `watch-apps read 90265` prints `unresponsive` with `process` `sleeping` and every row `undescribed`

`sample 90265 2` shows the main thread and the plugin's thread waiting on each other in all 1266 samples (`indesign-hang-sample.txt`):

| [INDEX] | [THREAD]                 | [STACK]                                                                                               |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | Main, line 52            | `dvauxphost::UnloadExtensionById`, `torq::pm::PluginRuntime::destroy`, `std::__assoc_sub_state::wait` |
|  [02]   | `dvascripting::Engine3P` | `PluginRuntime::forEachPanel`, `napi_call_function`, `Support for JavaScript`, `__do_timed_wait`      |

Thread `[02]` is the plugin's JavaScript thread inside a panel callback that calls the InDesign scripting DOM, which the main thread serves. Plugin code reaching the DOM at teardown:
- `Plugin.serve` gives UXP `destroy: () => runtime.dispose()` and each panel a `hide` that closes its scope (`libs/typescript/creative-cloud/uxp/plugin.ts`)
- Panel `typesetting` releases `app.removeEventListener(event, changed)` for 3 events when its scope closes (`plugins/indesign/main.ts`)

Required change:
- Panel `hide` and plugin `destroy` return without a host DOM call while UXP unloads the plugin
- Installs over a running host leave the loaded plugin in place, or close the plugin's link and panels and wait before they uninstall

Check: with a document open and panel `typesetting` shown, run `nx install Atelier` 20 times, then `watch-apps read com.adobe.InDesign` prints `responding` each time.

## [03]-[CLOSE_EXCEPTIONS]

InDesign's UXP log holds 128 lines `Uncaught JS Exception:` at 00:01:47 on thread `0x16ff17000`, with empty messages (`indesign-uxp.log`, 145 in the run):
- At 00:01:47 an ExtendScript `do script` call closed `Persian-Garden-tmp.indd` with `doc.close(SaveOptions.NO)`
- UPI reinstalled the plugin 43 seconds earlier, at 00:01:04
- Whether these exceptions lead to fault `[01]` is unknown, 20 minutes separate them

Required change: handlers of `afterSelectionChanged`, `afterSelectionAttributeChanged`, and `afterContextChanged` finish without a throw when their document closed.

Check: open and close a document from a script with panel `typesetting` shown, the UXP log gains no `Uncaught JS Exception` line.

## [04]-[STALE_WATCHERS]

`register.ts` starts `watch-apps watch <names>` once per session (`$.process.spawn({ argv: ['watch-apps', 'watch', ...names] })`), and a running watcher keeps its old binary:
- On November 10 `watch-apps watch sh.voicebox.app` (pid 219, started 17:41:21) and `watch com.adobe.InDesign` (pid 217) reported `unresponsive` after fixed builds were installed
- Nine `watch-apps read 84325` runs on the current build printed `responding` in 0.25 s each over the same period
- At 02:20 on November 11 watchers 62032 and 51295 of session `rasm-98` predated the installed binary of 22:35

Required change: the hook restarts its watchers when the installed `watch-apps` binary changes.

Check: start a session, run `nx install watch-apps`, and `ps -axo lstart,command` lists that session's watcher with a start time after the install.

## [05]-[SERVER_HELPER]

`nx install Atelier` at 20:49:42 on November 10 left port 7490 without a listener:
- Process launchd started the helper while the copy ran, and `amfid` refused it: `Contents/Helpers/atelier not valid ... Code=-423`
- Process launchd logged `OS_REASON_CODESIGNING | Launch Constraint Violation` for `app.rasm.atelier.server` and removed the service
- Crash report `atelier-2026-11-10-204942.ips` holds `SIGKILL (Code Signature Invalid)`, namespace `CODESIGNING`
- `rasm-98` found the background-task record kept the old helper hash, and `agent.register()` did nothing for a registered agent

Reported change: `rasm-98` removed the stale job, relaunched, and installed a build that registers again after an install.

Check: run `nx install Atelier` twice, then `lsof -nP -iTCP:7490 -sTCP:LISTEN` prints the server within one minute each time.

## [06]-[EXPORT_WINDOWS]

At 20:12 on November 10 `execute_script` running `doc.exportFile(ExportFormat.PDF_TYPE, ...)` on pid 90265 returned the instance state as an error:
- Windows 14084 (766 by 516, untitled) and 14088 (`Adobe InDesign`, 444 by 175) read `kind` `blocking`, `asking` true, `onScreen` false, layer 8
- InDesign wrote the PDF whole, and `watch-apps read 90265` seconds later printed `responding` with no asking row
- `watch-apps` already skipped off-screen windows at the modal-panel level, Atelier's build of that library did not

Reported change: `rasm-98` rebuilt Atelier with the current state library.

Check: export a 9-page document to PDF through `execute_script`, the call returns its result.

## [07]-[UNRESPONSIVE]

`watch-apps watch com.adobe.InDesign` (pid 217) switched pid 90265 between `responding` and `unresponsive` nine times in 51 seconds from 23:52:39Z on November 10:
- Five one-shot `read 90265` runs printed `unresponsive` in the same period (`indesign-90265-states.tsv`)
- One sample by `rasm-98` held the main thread in `mach_msg` for 1059 of 1094 samples
- With one reader, six timed reads took 229 to 718 ms (`read-timing.json`), and 25 script calls produced no `unresponsive` line
- `rasm-21` attributed the fault to readers queued on one main thread, and `rasm-98` disproved that reading with concurrent whole reads
- Cause found by `rasm-98`: an idle background app under App Nap runs its main thread at priority 4, so Accessibility messages took 5.7 to 11.5 s under load

Reported change: a silent heartbeat samples the main thread before and after its timeout, and `unresponsive` holds `process` `running`, `sleeping`, `stopped`, or `runnable`. A watcher with an earlier answer keeps its status while `process` is `runnable`.

Check: load every core, then `watch-apps watch com.adobe.InDesign` over an idle InDesign prints `responding` once and nothing for 60 seconds.

## [08]-[HOOK_NOTES]

`plugins/function-hooks/instances.ts` maps `process` to a note: `stopped` to `run kill -CONT <pid>`, `sleeping` to `is blocked, run watch-apps capture <pid>`, `running` to `is busy`, and `runnable` to no note.

Check: `rg -n "process: '(stopped|sleeping|running)'" plugins/function-hooks/instances.ts` prints one `Match.when` row per value.

## [09]-[SPLASH_WINDOW]

On November 10 `watch-apps` read InDesign's splash window as `blocking`: a window at the modal-panel level with `onScreen` false. `Beat.swift` now tags a modal-level window only `where window.layer == modal && window.onScreen`.

Check: `rg -n 'window.layer == modal && window.onScreen' libs/swift/macos/Applications/Workspace/Beat.swift` prints one line.

## [10]-[RELATIVE_BUNDLE]

Fix `launched` in `policies/apps.ts` rewrote `open build/<name>.app` to `watch-apps launch build/<name>.app`, which exact-name resolution refused as `unmatched`:
- `launched` now resolves a `.app` word with `/`, or without a flag, through `host.place` against the command's folder, and passes that path
- `open ./NoSuchBundle.app` became `watch-apps launch <folder>/NoSuchBundle.app` and failed with reason `host`, a missing file
- `nx run @rasm/function-hooks:check` passed after the edit

Check: in a folder holding `<name>.app`, `open <name>.app` through the hook launches that bundle.

## [11]-[PARTIAL_NAME]

Command `watch-apps launch Editor`, run by an agent, matched `TwinmotionCookedEditor` by containment and opened Twinmotion. Names now resolve whole: a `file:` URL, a path, a pid, a bundle id, a file name, then a display name.

Check, run at 02:25 on November 11: `watch-apps find Editor` prints a `fail` line with `candidates` and starts nothing.

## [12]-[EVIDENCE]

| [INDEX] | [FILE]                          | [HOLDS]                                                              |
| :-----: | :------------------------------ | :------------------------------------------------------------------- |
|  [01]   | `indesign-hang-sample.txt`      | `sample 90265 2` of the deadlock, main thread at line 52             |
|  [02]   | `indesign-uxp.log`              | InDesign's UXP log of the run, uninstalls at lines 1635 to 1667      |
|  [03]   | `indesign-mainthread-log.txt`   | System log lines of main thread `65f065a` from 00:00 on November 11  |
|  [04]   | `indesign-90265-states.tsv`     | States `watch-apps` recorded for pid 90265: UTC time, run pid, state |
|  [05]   | `read-timing.json`              | `hyperfine -r 6 'watch-apps read 90265'`                             |
|  [06]   | `atelier-2026-11-10-204942.ips` | Crash report of the refused server helper                            |

Sources outside `.artifacts/fix-apps/`: `~/Library/Logs/Adobe/Adobe InDesign/UXPLogs_2026-11-10_15-57-25_694231.log`, `~/Library/Logs/atelier/`, and `.artifacts/logs/watch-apps/`.
