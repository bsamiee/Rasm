---
name: use-atelier
description: "Use when driving an Adobe app or reading its host state, covering the per-app walk from launch to a document, dialogs, scripts, and application and document settings"
---

# [USE_ATELIER]

Covers Adobe apps driven in the background: `watch-app` opens, reads, captures, answers, and quits them, and the `atelier` MCP server runs scripts inside them. Use `uxp:coding` for code a plug-in holds.

## [01]-[SEQUENCE]

One Adobe app runs at a time, its Beta build first:

```bash
watch-app processes "<app>"                                # crash reports and leftovers before a launch
watch-app launch "<app>"                                   # no activation, no document, prints the pid
watch-app read <pid> | jq -c '.[][] | .running | {status, asking}'
watch-app capture <pid> --window <number> --longest-side 1100
watch-app read <pid> --window <number>                     # ids of an asking row's answers
watch-app perform <id> AXPress                             # the row's accept id
watch-app quit <pid>
```

1. Run `processes` first, and read each report under `reports` before launching
2. `launch`, then wait for `status` `responding` with `asking` empty
3. `capture` each presented window and read the image: Home screen, What's New, survey, sign-in, update, or crash-recovery panels
4. Answer each asking row with `perform` on its `accept` or `cancel` id, a `blocking` row by `capture` then `key`
5. Read host state through `execute_script` on the app, every read of one question in one call
6. `quit` once the app reads `responding`, and `processes` again to confirm no instance or helper stays

- After Effects ignores a quit sent while `launching`, so wait for `responding` first
- Crash dialogs ("quit unexpectedly", Adobe Crash Reporter "Error Report") belong to the crash reporter processes, read with `watch-app read <reporter pid>` as `kind` `system` with nothing asking
- Dialogs and crash windows the owner did not ask an agent to answer stay for the owner

## [02]-[SCRIPTS]

Scripts never raise a host dialog:
- InDesign runs `execute_script` bodies through `do script` with language `uxpscript`, where an uncaught throw raises a modal "UXPScript Error" alert, so Atelier catches every throw and returned promise and sets `app.scriptPreferences.userInteractionLevel` to `UserInteractionLevels.NEVER_INTERACT` for the call
- `execute_script` returns at once with the state when a dialog needs an answer or the application exits, and leaves the dialog for its owner
- Illustrator starts at `DISPLAYALERTS`, so its scripts set `app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS` first
- Photoshop sets `displayDialogs` to `dontDisplay`, and a script reading `activeDocument` with no document open raises a modal alert that activates Photoshop
- Read `app.documents.length` before `app.activeDocument` in every app
- Large WebAssembly modules crash InDesign's UXP runtime: ICU's 20 MB `icu_capi.wasm` raised SIGSEGV in `libdynamic-napi`, HarfBuzz loaded

## [03]-[SETTINGS]

Settings belong to the application or to one document, and each app splits them its own way:

| [INDEX] | [APP]         | [APPLICATION]                                           | [DOCUMENT]                             |
| :-----: | :------------ | :------------------------------------------------------ | :------------------------------------- |
|  [01]   | InDesign      | Defaults every new document copies, and its own objects | Values of one file, copied at creation |
|  [02]   | Illustrator   | Every preference                                        | One color space, swatches, styles      |
|  [03]   | Photoshop     | Every preference and color settings                     | One mode, bit depth, and profile       |
|  [04]   | After Effects | Every preference                                        | Project working space and bit depth    |

InDesign holds most settings twice:
- Members on `app` and on a document (`textDefaults`, `documentPreferences`, `viewPreferences`, `marginPreferences`, `swatches`, styles) are defaults on `app` and one file's values on the document
- Writes with no document open change the defaults of every later document, and writes to a document leave `app` unchanged
- `colorSettings`, `generalPreferences`, `scriptPreferences`, and the find, change, export, and import preferences exist on `app` alone
- `printPreferences`, `metadataPreferences`, `rgbProfile`, and `cmykProfile` exist on a document alone
- Illustrator, Photoshop, and After Effects hold no such defaults, a write with no document open changing the application alone

## [04]-[DOCUMENTS]

| [INDEX] | [APP]         | [NO_DOCUMENT]                             | [CREATE]                                 |
| :-----: | :------------ | :---------------------------------------- | :--------------------------------------- |
|  [01]   | InDesign      | `app.activeDocument` throws               | `app.documents.add(false)`, no window    |
|  [02]   | Illustrator   | `app.activeDocument` throws               | `app.documents.add(space, w, h)`         |
|  [03]   | Photoshop     | `app.activeDocument` raises a modal alert | `app.createDocument` in `executeAsModal` |
|  [04]   | After Effects | One project always open                   | `app.project.items.addComp`              |

- InDesign documents made with no window stay out of `app.activeDocument` and selection, so scripts keep the reference `add` returns
- InDesign applies a close after its script returns, `app.documents.length` in the same script still counting the document
- Scripts close every document they create: InDesign `doc.close(SaveOptions.NO)`, Illustrator `doc.close(SaveOptions.DONOTSAVECHANGES)`
- Photoshop closes with `doc.closeWithoutSaving()` in `executeAsModal`, After Effects with `app.project.close(CloseOptions.DO_NOT_SAVE_CHANGES)`
