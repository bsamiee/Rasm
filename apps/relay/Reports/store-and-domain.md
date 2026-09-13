# Store and domain
Agent task: raise the Relay application layer (App/RelayStore, RelayPaths, LoginItem; Accounts/Account, Usage, AccountRepository, Issues) to the repository's implementation standards. Date: 2026-09-13.

Findings (verdict):
1. Five parallel `[UUID: X]` maps: fixed. One `AccountState` per account (account, authentication, usage, sessionStart, activity, issue, automaticStartAttempted) in one ordered array; `Activity` (.idle / .signingIn / .job(operation, task)) makes an operation without its job unrepresentable; every `?? .signInRequired` / `?? .unavailable` default is gone.
2. `guard let record(id)` duplication: fixed. `index(of:)` plus `update(_:_:)`; a guard remains only where a precondition (connected, idle activity) decides the operation.
3. `switch` ladders: fixed where a dependency exists (applyUsage, report, activateProvider, reconcile). The per-operation `switch account.provider` stays: it is the one place the provider is chosen, no `map` expresses it.
4. `ClientFailure` enum: not a defect. `any ProviderFailure` cannot conform to `Error` (verified with swiftc), so a protocol cannot be a `Result` failure type; the enum is the closed sum of the two provider failures. Comment added.
5. `Task { [weak self] in guard let self }` sprawl: fixed. One job runner `begin` owns the slot, persists after the work, clears the slot, and considers an automatic session; captures are strong because `stop()` awaits every task and the store lives for the process.
6. `jobs` / `persistenceJob` generation UUIDs: fixed, removed. A job is the only writer of its own slot; write order is the repository actor's (synchronous `save`, no chain).
7. `stop()` versus wake-observer `refresh()`: not a defect. `refresh`, `begin`, and `startMonitoring` all guard `stopping`, and the observer is removed before the awaits.
8. `applyUsage` writing `.connected`: fixed, removed. Refresh runs for connected accounts only and the job slot excludes a concurrent sign-out.
9. `considerAutomaticSession` racing the pending mark: fixed. The guard requires `.idle` activity before marking the attempt, so an attempt is never consumed without a job starting.
10. `moveAccounts` arithmetic: was correct ([A,B,C,D], offsets {0}, destination 3: insertion 3-1=2 gives [B,C,A,D]); replaced with `Array.move(fromOffsets:toOffset:)`.
11. `persist()` result discarded: not a defect, narrowed. `beginSession` binds on it (the pending mark must be durable before the greeting); the runner persists once after each job, so call sites fell from nine to four.
12. `saveConfiguration()` chaining on `previous.value`: fixed. `Task { await persist() }`; the actor orders writes.
13. Monitor tick seconds after a panel-open refresh: fixed. `refresh()` (launch, wake, panel open) restarts the monitor, so the next tick is one full interval later.
14. `expandedSessions` is view state: needs the Views owner. Kept (RelayMenu reads it) and toggled with `formSymmetricDifference`; RelayMenu should hold it as `@State` and call `activateProvider` only when the session is not running, then the set and the toggle leave the store.
15. Repository transport: fixed. One validating method per type (`savedAccounts`, `savedAccount`, `usageSnapshot`, `quotaWindow`); `AccountStorageIssue` drops the `record:` index (the id identifies a record; no reader used the index).
16. `try? record.identity.get()`: fixed. Identities now come from the already-checked results by pattern match; the failure was reported by `traverse` anyway, but every identity was validated twice.
17. `resetsAt == nil && fraction == 0 -> idle`: not a defect. Both parsers pass a null reset through with utilization 0 for a window that has not started (ClaudeQuota `resets_at`, CodexProtocol `resetsAt`), and `ClaudeClient.startSession` refuses `.unknown`, so `idle` is what makes a manual start possible. UsageGauge duplicates the rule in `resetLabel` and should read `sessionState(at:)`: needs the Views owner.
18. `LoginItem`: not a defect. The ServiceManagement boundary translates its one thrown error into UI state once; `requiresApproval` drives the Open Login Items button.
19. `RelayPaths`: not a defect. Derived URLs come from one stored root and per-account paths are functions of the id; both clients call them.
20. Duplicate sign-in then Cancel discarded the connection twice (authenticate, then cancelAuthentication; Claude's second discard fails on the missing directory): fixed, discarded once at cancel.
21. Standard library: `Array.move`, `Set.formSymmetricDifference`, `Task.sleep(for: Duration)`, `for case .job`, `async let` kept for the two independent selection reads.

Line counts (before -> after): RelayStore 635 -> 619, AccountRepository 263 -> 254, RelayPaths 28, LoginItem 48, Account 130, Usage 68, Issues 60 unchanged.
Members changed for other owners: `RelayAccount.identity` and `.sessionPolicy` are `var` (memberwise init unchanged, providers compile as is); `RelayStore.refresh()` is private (no external caller); no public name or shape changed, nothing added. Storage failures now log `String(describing:)` at the boundary.
Checks: xcodebuild exit 0, zero warnings (one build failed mid-way on the Providers agent's in-progress `CodexClient.swift`, passed once their file stabilized); swift-format format exit 0; swift-format lint --strict exit 0; full-module `swiftc -typecheck` exit 0.
