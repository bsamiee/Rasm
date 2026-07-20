# [RASM_RHINO_IDEAS]

`Rasm.Rhino`'s forward pool holds higher-order host-boundary concepts: RhinoCommon document/display/command/exchange capture and native Eto UI composition over the `Rasm` kernel. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[HOST_HOOK_REGISTRY]-[ACTIVE]: Every detached fact stream in the boundary unifies under one typed hook-point registry addressed as `rasm.rhino.<domain>.<point>`.
- Capability: One registry rail over the folder's fact emissions — document events, pointer/gumball/widget facts, panel and page signals, render content events, command registry events, scene-change batches — each point a typed row carrying its modality (observe everywhere, veto only where the host callback admits refusal, replay from retained evidence), subscriber-fault isolation onto the existing fault rail, and telemetry-as-tap so observability subscribes to domain facts instead of emit calls living inside domain code.
- Shape: Registry spine lands on `libs/csharp/Rasm.Rhino/.planning/Document/events.md` beside `DocumentStream`; point rows for the sibling streams land on `libs/csharp/Rasm.Rhino/.planning/Display/interaction.md`, `libs/csharp/Rasm.Rhino/.planning/HostUi/panels.md`, and `libs/csharp/Rasm.Rhino/.planning/Render/registry.md` as adopters of the same row grammar.
- Unlocks: App roots and agents bind any host fact by point name without learning per-domain stream APIs, and the branch hook law lands uniformly across the boundary.
- Anchors: `DocumentStream.Observe` scoped attach and symmetric release; `PointerFact` bounded channels; `PanelHost` fact stream; `ContentStream` events; branch hook law naming `rasm.<pkg>.<domain>.<point>` with veto/observe/replay modalities.
- Tension: Host callbacks are largely post-hoc — veto exists only where the native seam is cancelable, so each row states its modality from the host contract rather than the registry granting veto uniformly.

[MULTI_PLUGIN_COEXISTENCE]-[ACTIVE]: Many Rasm-built plugins coexist in one Rhino process without fighting over process-global custody.
- Capability: Per-plugin scoping of every process-global surface — static host event subscriptions ruled single-subscription per process, the `ObjectsTelemetry` sink, `HostTap` exception and cloud-log mounts, named shell callbacks, panel and page registrations — through keyed arbitration: first mount wins the process seat, later plugins attach as keyed subscribers, every registry keys on plugin identity, and teardown returns the seat.
- Shape: Arbitration law and seat rows land on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md` (telemetry sink and `HostTap` seat), `libs/csharp/Rasm.Rhino/.planning/Document/events.md` (static event-subscription custody), and `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` (named callbacks and runtime rows).
- Unlocks: App-neutrality at the host — two independent Rasm plugins ship into one Rhino session with zero collision or shadowing across telemetry, events, callbacks, or panels.
- Anchors: Single-subscription-per-process host event law; `HostTap.Mount` idempotent-detacher precedent; collectible-ALC plugin loading; `ObjectsTelemetry.Configure` app-root registration.
- Tension: Seat lifetime versus ALC lifetime — a seat owner unloading mid-session must hand the seat to a surviving plugin or park it detached; the handoff protocol is part of this card, never an app-root afterthought.

[PLUGIN_DOMAIN]-[ACTIVE]: `Rhino.PlugIns` becomes the boundary's own domain folder — plug-in identity, lifecycle, licensing, and per-plugin document participation captured as typed rails.
- Capability: `PlugIn` base custody as staged phases (load, command creation, shutdown) with unload-flush obligations for every meter, log, and telemetry lifetime riding the collectible ALC; installed-plugin census with load protection and id/path resolution; the complete license rail — acquisition, checkout/checkin, CloudZoo lease facts, state-change events; per-plugin document serialization bridged onto archive framing; settings-saved change facts on the settings rail.
- Shape: New sub-folder `libs/csharp/Rasm.Rhino/.planning/Plugin/` holding `lifecycle.md` (staged `OnLoad`/`CreateCommands`/`OnShutdown` custody, ALC `Unloading` flush, diagnostics capture window), `census.md` (`PlugInInfo`, installed census, load protection, id/path resolution), `licensing.md` (`LicenseUtils` and CloudZoo lease rail, `LicenseStateChangedEventArgs`, `ZooClientParameters`), and `document.md` (`ReadDocument`/`WriteDocument`/`ShouldCallWriteDocument` on `ArchiveIo` framing, `GetPluginSettings`/`SettingsSaved` bridge).
- Unlocks: Rasm plugins gain typed lifecycle, entitlement gating, and per-plugin document persistence without touching `Rhino.PlugIns` raw statics; the profiling and metrics flush law for plugin processes gains its structural owner.
- Anchors: `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-plugins.md` lifecycle, census, and license rosters; `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md` licensing rows; `ArchiveIo` framing on `libs/csharp/Rasm.Rhino/.planning/Persistence/userdata.md`; `SkinPhase` load-progress precedent on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md`.
- Tension: File-dialog dispatch stays with `CodecImportPort`/`CodecExportPort` on `libs/csharp/Rasm.Rhino/.planning/Exchange/formats.md` and options/properties pages stay with `libs/csharp/Rasm.Rhino/.planning/HostUi/pages.md` — the new domain owns only the unowned remainder, never a second seat for realized surfaces.

[HOST_INSTRUMENT_PARTITION]-[QUEUED]: Rhino receipt families declare their instrument projection as data — a kind partition the app root merges into the branch instrument fan.
- Capability: Per-domain receipt-kind partition rows naming the instrument (`rasm.rhino.<domain>.<measure>`, UCUM units), the source receipt field, and attribution tags (document key, command name, op provenance, tenant) so cost attribution partitions by document and command; marshal-seam latency checkpoints ride `ILatencyContext` on `HostThread.Run`. Rows are declared in-boundary as data and executed at app root, preserving the zero-OTel altitude law.
- Shape: Partition rows land beside the structured-log egress on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md`; latency checkpoint names land on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` beside `HostThread`.
- Unlocks: Host-boundary receipts become dashboard-visible metrics at the composition root, closing the branch instrument fan's missing Rhino arm.
- Anchors: `ObjectsTelemetry` one-egress precedent; typed receipts on every measured surface; `libs/csharp/.api/api-extensions-telemetry.md` latency-measurement rows; branch instrument-fan arm merging at the app root.
- Tension: Partition rows are projection truth, never a second measurement — a row names an existing receipt field or the receipt gains the field first; instruments never sample the host directly.

[HOST_BENCH_HARVEST]-[QUEUED]: Measured host operations emit benchmark-grade evidence in-host, because the benchmark harness cannot cross the native boundary.
- Capability: One timing and allocation evidence band on the Modeling spine and the capture run rail, normalized to the benchmark-receipt shape the corpus gate ingests — operation identity, input scale, duration, allocation, host fingerprint — harvested from bridge-run sessions where P/Invoke-backed members actually execute.
- Shape: Evidence band lands on `libs/csharp/Rasm.Rhino/.planning/Modeling/solids.md` (the `Built` spine) and `libs/csharp/Rasm.Rhino/.planning/Viewport/capture.md` (run-rail frame timing).
- Unlocks: Regression-visible performance truth for meshing, booleans, capture, and exchange under the same corpus gate the rest of the branch uses.
- Anchors: `ModelGate` and `Built` spine receipts; capture run rail; static-spec native boundary (host P/Invoke members fail outside the host, so in-host harvest is the only honest bench path).
- Tension: Bench evidence is harvest-grade, never statistically controlled — receipts carry raw observations and the corpus gate owns aggregation and admission thresholds.

[DOCUMENT_ANALYTICS_CENSUS]-[QUEUED]: One typed document census projects the whole document as analytics-ready evidence.
- Capability: A single census receipt spanning object counts by kind, layer-tree shape, block-definition closure metrics, material and annotation usage, and archive size evidence — composed from existing snapshot and closure owners, detached, and shaped for the analytics egress an app root lands into the data plane.
- Shape: Census owner lands on `libs/csharp/Rasm.Rhino/.planning/Objects/state.md` composing closure evidence from `libs/csharp/Rasm.Rhino/.planning/Blocks/graph.md` and table counts from `libs/csharp/Rasm.Rhino/.planning/Document/tables.md`.
- Unlocks: Fleet-level document analytics — size, complexity, and usage trends across every document a Rasm app touches — without any consumer walking live tables.
- Anchors: `Objects.Ask` snapshot window; `BlockGraph.Ask` closure evidence; table vocabulary receipts; exact-byte archive identity evidence on `libs/csharp/Rasm.Rhino/.planning/Exchange/archive.md`.

[REDACTED_HOST_EGRESS]-[QUEUED]: Log egress payloads carry data classification, and redaction is a registered app-root policy.
- Capability: Every `ObjectsTelemetry` and `HostTap` payload member classified — file paths, machine and user identity, license and lease facts, document names — with redactor registration as an app-root contract, so cloud-bound host logs never leak private material and the boundary never hardcodes a scrubbing policy.
- Shape: Classification taxonomy and annotated payload rows land on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md` beside the egress.
- Unlocks: Compliance-grade host logging for cloud sinks with per-app redaction policy, zero boundary rewrites per app.
- Anchors: `libs/csharp/.api/api-redaction.md` classification and redactor rosters; `ObjectsTelemetry` generated-event surface; `HostStaticEnricher` app-root registration precedent.

[HOST_SCRIPT_ENGINE]-[QUEUED]: Host scripting engine capability becomes typed shell-runtime rows — compile, execute, and census without raw statics.
- Capability: Script-engine access as capability rows — compile source into a reusable compiled unit, execute with typed argument and result custody, engine census as host facts — under the same guarded window every shell runtime row uses.
- Shape: Rows land on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` inside `[06]-[RUNTIME]`.
- Unlocks: Agents and apps drive in-host scripting through the boundary — batch automation and generative workflows without a second scripting integration.
- Anchors: `PythonScript` and `PythonCompiledCode` rows on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md`; shell runtime capability-row precedent.
- Tension: Engine availability is a host fact, never assumed — rows probe engine presence and refuse typed when the host ships without the engine.

[RHINO_ACCOUNTS_TOKEN_RAIL]-[BLOCKED]: Rhino Accounts' OAuth2/OpenID surface becomes one secret-scoped token rail on the shell runtime stratum.
- Capability: Cloud-authenticated capability for any host feature needing a McNeel-account identity — token acquisition, refresh, and revocation as one typed rail whose secret custody is structurally confined to the host's protected-code window, with login progress and entitlement projected as detached facts.
- Shape: A `HostUi/shell.md` `[06]-[RUNTIME]` extension — a `TokenAsk` request union (acquire, acquire-scoped, try-cached, revoke, refresh) dispatched inside `RhinoAccountsManager.ExecuteProtectedCode`/`ExecuteProtectedCodeAsync` so the `SecretKey` never escapes the callback; token pairs (`IOpenIDConnectToken` + `IOAuth2Token`) detach into claim/expiry evidence records, login progress folds `RhinoAccoountsProgressInfo`/`ProgressState` onto the shell fact stream, and `CloudHostUtils.IsEntitled`/`DenyReason` lands as a `HostProbe` capability row.
- Unlocks: Cloud-licensed plugin features, per-user service authorization, and entitlement-gated capability rows without any consumer touching the accounts namespace.
- Anchors: `RhinoAccountsManager.GetAuthTokensAsync`/`TryGetAuthTokens`/`RevokeAuthTokenAsync`/`UpdateOpenIDConnectTokenAsync`, `SecretKey`, and `ProgressState`; `HostUi` shell `HostProbe`/`HostFact` capability census and guarded-notice precedent for assembly-restricted crossings.
- Tension: Zero current consumers and a live-network, interactive-login dependency no design page can exercise headless — blocked until a consuming feature (cloud licensing, compute authorization) names the demand; the client-id/secret custody policy also belongs to the estate secret doctrine, not this page.

[INPROCESS_HEADLESS_BOOT]-[BLOCKED]: `Rhino.Runtime.InProcess.RhinoCore` boots headless Rhino inside a foreign process as an app-stratum composition shell.
- Capability: Full RhinoCommon under a console, service, or test host — disposable boot (`RhinoCore(args, WindowStyle, hostWnd)`), idle/message pumping (`Run`/`DoIdle`/`DoEvents`/`RaiseIdle`), and host-context marshalling (`InvokeInHostContext`) — so batch geometry, exchange, and render pipelines run without the interactive application.
- Shape: An `apps/`-stratum composition root owning the `RhinoCore` lifetime as a token-gated session cell (boot once, `WindowStyle.NoWindow`, dispose deterministically), lowering every in-process call onto the same `DocumentSession` demand the interactive boundary uses; `Rasm.Rhino` pages stay boot-agnostic and gain zero new members.
- Unlocks: CI-grade geometry pipelines, headless exchange/render farms, and out-of-Rhino test harnesses driving the full boundary.
- Anchors: `RhinoCore` constructors/`Run`/`DoIdle`/`DoEvents`/`InvokeInHostContext`, `Interop.StartupInProcess`/`LaunchInProcess`/`RunMessageLoop`/`ShutdownInProcess`, `WindowStyle`, and `StartupOrigin`; `DocumentSession` as the capability floor a boot shell feeds.
- Tension: App-stratum by ruling — an `apps/` shell composes headless Rhino while this package owns only the boundary, so the card stays blocked until an `apps/` composition root exists; macOS WIP in-process hosting constraints (bridge-only launch custody) are unresolved boot-environment facts.

[COMPUTE_ENDPOINT_ROWS]-[BLOCKED]: `HostUtils` compute-endpoint registration becomes shell runtime rows beside the named-callback rail.
- Capability: Typed REST-endpoint exposure for a compute-hosted Rhino process — registration, endpoint census, and entitlement gating as shell runtime rows, so no consumer touches `HostUtils` statics.
- Shape: A `HostUi/shell.md` named-callback-cluster extension — `HostUtils.RegisterComputeEndpoint` admitted behind one typed row, `GetCustomComputeEndpoints` folded into the shell capability census, `CloudHostUtils.IsEntitled`/`DenyReason` riding the same `HostProbe` capability row the accounts card names.
- Unlocks: Rasm geometry, exchange, and render capability served over rhino.compute from the same boundary the interactive host composes.
- Anchors: `HostUtils.RegisterComputeEndpoint`/`GetCustomComputeEndpoints` and `CloudHostUtils` on `api-rhinocommon-runtime.md`; the shell named-callback rail precedent.
- Tension: Meaningful only under a compute-server boot — the same app-stratum dependency that blocks `INPROCESS_HEADLESS_BOOT`; blocked until that composition shell exists.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [HOST_TAP_EGRESS]-[COMPLETE]: host process-wide exception and cloud-log streams capture as `HostTap` through the `ObjectsTelemetry` egress on `Objects/authoring.md`.
