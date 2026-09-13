# [RELAY]

Relay shows Claude and OpenAI subscription usage in the menu bar, switches Claude Code's login, and opens the Codex desktop app per account.

`pnpm nx run Relay:build` builds the app under `.cache/xcode/apps/relay` and the `Relay` scheme launches it from Xcode.

## [01]-[ACCOUNTS]

Add an account from Settings' `+` menu and select it by the panel's email row or Use Account, its host signs in with the selected account.

Sign In… must return the same identity, else "Sign in to the account you selected", a duplicate add reads "This account is already connected":

[CLAUDE]:
- `claude` resolves on the login shell's `PATH`, else at `~/.local/bin/claude`, missing reads "Install Claude Code to connect an account."
- Sign-in runs `claude auth login --claudeai` into the account's private store, the browser login waits in a sheet with Cancel alone
- Selection parks the outgoing grant in its private store and imports the incoming refresh token into Claude Code's shared store
- Import is `claude auth login` with `CLAUDE_CODE_OAUTH_REFRESH_TOKEN`, `CLAUDE_CODE_OAUTH_SCOPES`, and `CLAUDE_CODE_OAUTH_CLIENT_ID` set
- Shared store is the directory `CLAUDE_SECURESTORAGE_CONFIG_DIR` names, else `CLAUDE_CONFIG_DIR`, else `~/.claude`
- `.claude.json` sits inside `CLAUDE_CONFIG_DIR` when set, else at `~/.claude.json`, a `.config.json` in the store directory is read first
- Relay holds Claude Code's `.oauth_refresh.lock` on both stores through a switch and its `.storage-write.lock` around each write
- Switch during a native refresh stops with "Claude is updating this sign-in. Try again when it finishes.", nothing is written
- Running CLI adopts the new login through its own credential poll, requests under way finish on their bearer, nothing must be quit
- App environment is the login shell's, `$SHELL -ilc` prints `env -0` once at launch, a `CLAUDE_CONFIG_DIR` or `PATH` a profile exports is honored
- Login shell failure is logged under `app.rasm.relay` and Relay runs on launchd's environment without profile exports
- `CLAUDE_CONFIG_DIR` set in a terminal alone names a store Relay never writes, that terminal keeps its login after a switch
- Children never inherit `ANTHROPIC_` key, token, URL, and model overrides, `CLAUDE_CODE_OAUTH_*`, or the Bedrock, Vertex, and Foundry switches
- Working account is whichever the shared store holds, read at launch, after a sign-in, and after a failed switch
- Login the CLI made outside Relay is kept, selecting another account parks it as a new account
- Switch a crash interrupted is settled at the next launch, the login child's pid and start time decide whether it runs
- Login child alive at launch reads "Finish recovering the previous Claude account switch." until it exits
- Incoming account with an import dead mid-exchange reads "Sign in to this Claude account again." and shows Sign In…
- 401 on a usage read is a rotated grant, Relay re-reads the store under the lock and refreshes once, a second 401 reads the same message
- Sign-out parks the shared grant in the private store and runs `claude auth logout` there, the working account's sign-out signs the CLI out
- Remove signs out, then deletes the private store and its Keychain item

[OPENAI]:
- Every operation runs its own `codex app-server` from `ChatGPT.app`, bundle `com.openai.codex`, over the account's `CODEX_HOME`
- Children never inherit `CODEX_*`, `OPENAI_API_KEY`, `OPENAI_BASE_URL`, `OPENAI_ORG_ID`, or `OPENAI_PROJECT_ID`
- Sign-in opens the `account/login/start` page in the browser, a login without ChatGPT reads "Connect this account with a ChatGPT subscription."
- Selection verifies the account on its app-server, then opens or focuses a desktop instance, one per account, other instances stay open
- Instance launches with `CODEX_HOME`, `CODEX_ELECTRON_USER_DATA_PATH`, and `--user-data-dir` at the account's directories
- Desktop and app-server share `auth.json`, a desktop login as another user reads "OpenAI is signed in to a different user or workspace."
- Frontmost tracked instance reads as the working account at launch, else the saved one while its instance runs, a quit instance clears it
- Terminal `codex` reads its own `CODEX_HOME`, default `~/.codex`, and follows an account only with `CODEX_HOME` exported to its `Codex` directory
- Quit the account's desktop instance before Sign Out, Remove, or Sign In…, each refuses a running instance
- Refusal reads "Quit this account’s Codex app before signing out.", "…before removing it.", or "…before signing in again."
- Sign-out runs `account/logout` on the app-server, remove deletes the account's `Codex` and `Desktop` directories

## [02]-[SESSIONS]

Sessions are 5-hour provider usage windows, each started by one greeting on the least costly model and never by selecting an account:
- Provider mark click starts a session while the window is idle, expands the reset line while it runs, and is disabled during another operation
- Start reads usage first and returns while the window runs
- Claude refuses an unknown session state with "Refresh Claude usage before starting a session."
- OpenAI refuses an account with `ordinaryUsageAllowed` false with "OpenAI reports that this account’s included usage is unavailable."
- Claude greeting is `claude -p` in stream-json on the account's access token, tools, hooks, and memory off, one turn, then `hi`
- Claude model is the first enabled Haiku row of `list_models`, applied through `set_model`
- OpenAI greeting is an ephemeral `thread/start` with every tool, feature, MCP server, and web search disabled, then `turn/start` with `hi`
- OpenAI model is `gpt-5.6-luna` unhidden in `model/list` at its lowest supported effort, the turn runs at the default service tier
- Session state from a report: a reset ahead is running, a reset passed is idle, no reset at zero use is idle, no reset with use is unknown
- Pending mark is saved before the greeting, a failure inside the greeting keeps it with "Waiting for the provider’s reset time"
- Next successful report settles the pending mark, across relaunch the monitor reads pending accounts with the panel closed
- Manual is the default policy, Automatic sends the greeting when a report shows the window idle, once per idle window
- Automatic attempt resets when a report shows the window running or the policy changes, a start failing before the greeting spends it
- Refresh reads every connected account at launch, wake, and panel open, then every 60 s while the panel is open
- Panel closed, the 60 s tick reads Automatic and pending accounts alone

## [03]-[INTERFACE]

Panel and Settings are system controls in semantic colors and fonts, appearance follows the system, panel rows, marks, and gauges carry tooltips:
- Panel is a 384pt `MenuBarExtra` window capped to the screen height, one card per account, issues under the cards
- Gear menu holds `Settings…` (⌘,) and `Quit Relay` (⌘Q), `Add Account…` stands alone while no account exists
- Email row selects the account, an accessory-bar button with a checkmark on the working account and the system's hover and press highlight
- Provider mark is a circular bordered button at `.large`, prominent while the session runs, a spinner while starting
- Gauges Session, Weekly, and Fable (Claude alone) sit beside the mark, label leading, `percent · reset` trailing
- Gauge track is a 6pt capsule in quaternary, filled in accent, in secondary with dimmed labels while stale, digits monospaced
- Reading shows `<1%` under 1% and no percent at zero, weekly and Fable show the reset weekday and time
- Session reading is `2h 15m` while running, `Ready` idle, `Awaiting update` stale, `Unavailable` unknown
- Note under the gauges is the account's latest failure, else "Sign in required" for a signed-out account
- Settings is a `Window` scene, id `settings`, opened by ⌘, and the gear menu, a `Settings` scene draws no unified toolbar
- Settings window is 680×460 by default and 600×380 at minimum, titled by the email with the provider as subtitle, General for the pane
- Sidebar is General, then Accounts rows with mark, email, and checkmark, drag to reorder, context menu Move Up, Move Down, Remove Account…
- `+` as a provider menu and `−` on the selection sit in a control group at the sidebar's bottom
- Account detail is a grouped form: Working account (Use Account or Current), Session start picker, Sign Out or Sign In…, issue as footer
- General holds the Launch at login switch and Open Login Items… while approval is pending
- Sign-in sheet is 360pt wide with Cancel alone, Cancel ends the login and deletes a new account's store
- Remove alert keeps Cancel as default and names the CLI sign-out for the working Claude account
- Menu bar symbol and provider marks draw at their intrinsic 16pt, symbol stroke and bounds match SF Symbols at the 13pt menu bar configuration

## [04]-[STORAGE]

Everything Relay owns sits under `~/Library/Application Support/Relay`, `<id>` the account's UUID:

| [INDEX] | [PATH]                  | [CONTENT]                                                                         |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `accounts.json`         | Accounts with identity, policy, sign-in state, last usage, selection per provider |
|  [02]   | `claude-selection.json` | Claude journal: selected id, unavailable ids, preserved accounts, pending switch  |
|  [03]   | `codex-desktop.json`    | Desktop instances by pid and launch date, selected id                             |
|  [04]   | `Accounts/<id>/Claude`  | Private Claude store, `.claude.json`, `.credentials.json` when no Keychain item   |
|  [05]   | `Accounts/<id>/Codex`   | `CODEX_HOME` with `auth.json`                                                     |
|  [06]   | `Accounts/<id>/Desktop` | Codex desktop user data                                                           |
|  [07]   | `Session/`              | Working directory of the `claude` and `codex app-server` children                 |

- Claude grants sit in the login Keychain, `Claude Code-credentials` for the shared store, `Claude Code-credentials-<hash>` per private store
- `<hash>` is the first 8 hex of SHA-256 over the `CLAUDE_CONFIG_DIR` string, the item's account is `$USER`
- Relay reads an item with `SecItemCopyMatching`, updates it in place, and deletes it on remove
- Missing private item is created through `security add-generic-password -T /usr/bin/security -T Relay`, Claude Code's reads never prompt

## [05]-[PERMISSIONS]

Relay has no sandbox, entitlement, Automation, Accessibility, or notification API, prompts are Keychain, login items, and a profile's folder reads:
- Build signs automatically with the Apple Development identity of team `BBXB9R367P` that Xcode holds for the signed-in Apple ID
- Designated requirement names the bundle id, Apple's anchor, the identity as leaf, and the WWDR intermediate, not the team
- Keychain grant survives every rebuild under the same identity
- CI signs ad-hoc through `CODE_SIGN_STYLE=Manual CODE_SIGN_IDENTITY=- DEVELOPMENT_TEAM=` forwarded to the build target
- Answer "Always Allow" with the login password at the first read of each item, Claude Code's `apple-tool:` partition asks for the password
- Items are the shared one and one private item per account, CLI logout deletes the private item and its prompt returns after the next sign-in
- Launch at login registers `SMAppService.mainApp`, macOS posts its login-items notice once
- Login shell and provider children run as Relay, a profile reading Desktop, Documents, or Downloads can raise a Files and Folders prompt
- Network is `URLSession` to `api.anthropic.com` for Claude usage and profile at a 15 s request timeout, OpenAI usage goes through the app-server

## [06]-[PROJECT]

`Relay.xcodeproj` is Xcode 27's format, `objectVersion = 110`, its synchronized root folder puts every file under `apps/relay` in the target:
- Sources sit under `App/` (store, paths, login shell, login item), `Accounts/` (domain, repository), `Providers/` (one client each), and `Views/`
- Membership exceptions leave CLAUDE.md, LICENSE, and the project out, bundle resources are `Assets.car` and `AppIcon.icns`
- Bundle `app.rasm.relay` on macOS 26.0, Swift 6 with approachable concurrency, MainActor default isolation, warnings as errors
- Generated Info.plist, `INFOPLIST_KEY_LSUIElement` its one added key, keeps Relay out of the Dock
- `AppIcon.icon` is an Icon Composer document, fill `#FAFAFA` light and `#242424` dark with the glyph `Relay.svg` inverted
- `actool` renders the icon stack for Aqua, DarkAqua, and Tintable, the 16 to 1024 renditions, and `AppIcon.icns`, no PNG is checked in
- `Claude`, `OpenAI`, and `RelaySymbol` are single-SVG imagesets with template rendering and preserved vector representation
- `Relay:lint` and `Relay:format` run `swift-format`, `Relay:check` depends on `build` and `lint`
- CI runs the affected `check` of macOS projects on the `xcode-27` runner

## [07]-[ATTRIBUTION]

- Claude mark is Claude's from claude.com, OpenAI mark is Simple Icons' under CC0 1.0, Relay's symbol and icon are original
- Relay is adapted from Franciskid's LLMCodeBar, LICENSE holds its MIT notice
