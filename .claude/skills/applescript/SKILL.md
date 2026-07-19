---
name: applescript
description: >-
    Builds and hardens AppleScript, JXA, and Open Scripting Architecture automation — osascript/osacompile
    runners, Apple Events and object-specifier dispatch, TCC consent preflight and hardened-runtime
    entitlements, OSAKit/NSAppleScript embedding, ScriptingBridge and Cocoa Scripting, sdef and scriptable-app
    dictionaries, notarized script-app distribution. Use when writing or reviewing AppleScript/JXA code,
    sending Apple Events, wiring ScriptingBridge or OSAKit, authoring or reading an sdef, or packaging a signed
    macOS automation artifact. Not plain shell scripting, browser page automation, or Shortcuts-only flows.
---

# [APPLESCRIPT]

AppleScript is an object-specifier compiler over the Apple Event ABI: a production artifact treats the language as a descriptor DSL, keeps process invocation and TCC policy outside script bodies, and returns typed receipts. `scripts/validate_bundle.py` compile-checks, round-trips, and lints every OSA artifact.

## [01]-[ROUTING]

[REFERENCES]:
- [01]-[LANGUAGE](references/language.md): AppleScript source composition, from script-object algebra to `NSAppleEventDescriptor` surgery.
- [02]-[RUNTIME](references/runtime.md): `osascript` execution, compilation, and packaging, with AppleScript and JXA as language rows.
- [03]-[EMBEDDING](references/embedding.md): `OSAKit` embedded in a Cocoa host, bound directly to the Apple Event ABI.
- [04]-[EVENTS](references/events.md): Apple Event wire ABI and the entitlement axes gating every send.
- [05]-[HOSTS](references/hosts.md): host dispatch matrix binding each surface to its handler contract and entitlements.
- [06]-[DISTRIBUTION](references/distribution.md): packaging, notarization, and the observation rails that carry an artifact into production.

[TEMPLATES]:
- [01]-[HARDENED_RUNNER](templates/hardened-osascript-runner.sh): `osascript` dispatch shell gated on a silent consent preflight.
- [02]-[OSA_TOOL](templates/osa-tool.js): agent-tool skeleton across the OSA host set, argv in and one typed JSON envelope out.
- [03]-[COMPILED_LIBRARY](templates/applescript-library.applescript): reusable script object compiled to a `.scpt`/`.scptd` handler library.
- [04]-[NOTARIZED_APPLET](templates/notarized-applet.sh): script-app build driving `osacompile` output to a stapled notarized bundle.
- [05]-[LAUNCHD_AGENT](templates/launchd-osa-agent.plist): unattended launchd user agent driving one OSA artifact on one trigger.
- [06]-[OSAKIT_HOST](templates/osakit-host.swift): OSAKit host skeleton — one language instance owning every script behind a marshal boundary.
- [07]-[SCRIPTABLE_SDEF](templates/scriptable-app.sdef): dictionary spine of a scriptable app, one suite's terminology bound to cocoa keys.

[EXAMPLES]:
- [01]-[ATTRIBUTION_CORRELATION](examples/attribution-correlation.sh): audit-token correlation naming the binary charged for a failing send.
- [02]-[CHEVRON_DISPATCH](examples/chevron-dispatch.applescript): chevron-literal dispatch rail electing raw send or dictionary term per verb.
- [03]-[COCOA_SCRIPTING_SERVER](examples/cocoa-scripting-server.swift): server-side specifier resolution served from the receiver's own index.
- [04]-[OBJC_FFI_BRIDGE](examples/objc-ffi-bridge.js): JXA-to-C FFI boundary at composed scale over one declared ABI table.
- [05]-[OSAKIT_REENTRANCY](examples/osakit-reentrancy.swift): suspend-rebind-resume path for an event that re-enters its own host.
- [06]-[SDEF_ROUTING](examples/sdef-routing.js): verb-set routing against the installed dictionary before any send.

## [02]-[AUTOMATION_LAW]

- Automation consent binds one signed sender to one receiver, so every probe and every send runs from the binary that ships.
- A production sender preflights consent and routes an undetermined verdict to a user-initiated lane.
- User values reach a script as escaped literals inside closed templates, never as concatenated source.
- Terminology resolves against the installed target dictionary at run time, never from recall.
