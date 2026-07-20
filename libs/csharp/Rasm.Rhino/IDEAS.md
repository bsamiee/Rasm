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
