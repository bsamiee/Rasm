# Settings window
Agent task: slop hunt and redesign of `apps/relay/Views/RelaySettings.swift`, 2026-09-13

Slop found and fixed (all in `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/relay/Views/RelaySettings.swift`):
1. Two add-account affordances (sidebar header `+` menu and the `.none` detail `Menu("Add Account…")` with the same items): now one `+` menu.
2. Selection kept in four places (`onAppear`, `onChange(isLoading)`, `onChange(accounts)`, a guarded `Binding` setter) plus `.disabled(isLoading)`: now one derived `selectedAccount` lookup; a stale or absent saved id resolves to General. The `.none`/loading detail state is gone with it. The single remaining `onChange` selects a newly added account after its sheet closes (see facts below).
3. `operation != .idle && operation != .awaitingWindow` written four times: now `AccountPresentation.isBusy`, a fileprivate extension in my file.
4. `Sign In…` disabled on `store.authentication != nil`: dead, the sheet is modal and `store.signIn` guards it.
5. Sheet's `pending.id == id` check: dead, the store holds one authentication; the sheet reads `store.authentication?.issue` and no longer takes an id. Cancelling now shows the spinner and disables Cancel until the store clears it; `.interactiveDismissDisabled()` plus the binding setter calling `cancelAuthentication` remain the only exits.
6. Alert buttons set `removing = nil` by hand while the `isPresented` setter already does; removed. `isPresented` binding inlined.
7. `?` popover explaining the session picker: now the section footer in the grouped form. Its `@State`, button, popover, and 280pt frame are gone.
8. Provider images sized with `.resizable().scaledToFit().frame(16×16)` in three places: the assets are 16pt template vectors, so plain `Image`/`Label(_:image:)` render at that size.
9. `registration` switch with three `EmptyView` arms, `.pickerStyle(.menu)`, `.toggleStyle(.switch).controlSize(.small)` (grouped Form defaults), `Group`-less accessibility label/value strings on the row (a `Label` with the asset icon and a labelled checkmark carries them under `.combine`), a `VStack` wrapper with 20pt paddings and `.padding(.vertical, 3)` off the grid.

Design changes and basis: `+`/`−` sit in a `ControlGroup` at the bottom-left of the sidebar list (System Settings Users & Groups, Mail Accounts, Xcode Accounts); `+` is a menu of providers as in Xcode's Accounts pane; `−` acts on the selection, and the context menu keeps `Remove Account…` as the second, selection-free path (two places, not three). The detail pane sets `.navigationTitle(email)` and `.navigationSubtitle(provider)`, and General sets "General" (System Settings names the window after the pane), replacing the custom header. Working account, Session start with footer, Sign Out/Sign In…, issue as a footer, and the remove alert with Cancel as default remain. Spacing is 8/16/24; sheet width 360 stays fixed as alert-style sheets are fixed width (HIG Alerts). `.formStyle(.grouped)` and `.navigationSplitViewColumnWidth` retained.

Lines: 416 before, 278 after.

Facts the store should expose: (a) `AccountPresentation.isBusy` (operation is neither `.idle` nor `.awaitingWindow`) and `canSelect` (`isBusy` and `authentication != .connected`), duplicated in `RelayMenu.swift:134-137` and `:234`; (b) `addAccount(_:)` returns nothing, so selecting the new account reads `authentication?.id` through `onChange` instead of a returned id; (c) `LoginItemState.issue` is nil for `.unavailable`, so the view invents "Launch at login is unavailable"; (d) no cancelling state during `cancelAuthentication`, so the sheet keeps its own `isCancelling`; (e) every settable value is a store method behind `private(set)`, so `@Bindable` cannot replace the `Binding(get:set:)` calls.

Checks: `xcrun swift-format format` exit 0, `lint --strict` exit 0; `xcodebuild` exit 0, zero compiler diagnostics (first runs failed at `RelayMenu.swift:151` on another agent's in-flight `UsageGauge` change, since settled; a scratch type-check of the module with warnings as errors also passed).
