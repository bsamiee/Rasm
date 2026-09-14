# [RELAY]

Relay shows Claude and OpenAI subscription usage in the menu bar, switches the machine's active account, and opens a 5-hour window on any account without switching to it.

`pnpm nx run Relay:build` places `Relay.app` under `.cache/xcode/apps/relay/Build/Products`, the `Relay` scheme and Xcode's MCP server place theirs in Xcode's derived data.

## [01]-[ACCOUNTS]

Add an account from Settings' `+` menu and select it by the panel's email row or Use Account, the provider's CLI and app then run as that account.

Sign In… must return the same identity, else "Sign in to the account you selected", a duplicate add reads "<email> is already connected":

[CLAUDE]:
- `claude` resolves at `~/.local/bin`, `~/.bun/bin`, `~/.npm-global/bin`, `~/.volta/bin`, `/opt/homebrew/bin`, `/usr/local/bin`, then launchd's `PATH`
- Missing `claude` reads "Install Claude Code to connect an account"
- Sign-in runs `claude auth login --claudeai` into the account's private store
- Shared store is the directory `CLAUDE_SECURESTORAGE_CONFIG_DIR` names, else `CLAUDE_CONFIG_DIR`, else `~/.claude`
- Shared item holds the one live credential, every other account's credential sits in its private item
- Switch copies shared credential into outgoing private item, then moves incoming private item into shared item
- Switch writes incoming `oauthAccount` into `.claude.json`, runs no child process, and makes no network request inside its locks
- Switch holds Claude Code's `.oauth_refresh.lock` pair on every store it touches, 10 attempts at 100 ms to 1 s apart
- Item writes hold `.storage-write.lock` of their store
- Crash mid-switch leaves its record in `claude-selection.json`, the next launch deletes the private copy of the credential the shared item holds
- Running CLI keeps the token it holds until its own refresh is due, then reads the shared item, new invocations read it at once
- Active account is the shared store's `oauthAccount`, read at launch, wake, panel open, every refresh, and each kqueue change of `.claude.json`
- Login the CLI made outside Relay to an unknown identity adds an account row
- Login outside Relay saves the outgoing account's last-read credential into its private item
- Outgoing credential that rotated since its last read reads "Sign in to this Claude account again"
- `claude -p` in the account's store refreshes its credential when the access token or refresh token nears expiry
- Relay posts nothing to the token endpoint
- Item with an empty `refreshToken` reads as signed out
- Denied Keychain read reads "macOS refused Relay’s Keychain request for this Claude sign-in"
- Locked login keychain reads "Unlock the login keychain to read this Claude sign-in" on the card, the account stays connected
- 401 on a usage read refreshes once through the CLI, a second 401 reads "Sign in to this Claude account again"
- 429 waits `Retry-After`, a missing or zero value waits the usage cache duration
- `.claude.json` sits inside `CLAUDE_CONFIG_DIR` when set, else at `~/.claude.json`, a `.config.json` in the store directory is read first
- `CLAUDE_CONFIG_DIR`, `CLAUDE_SECURESTORAGE_CONFIG_DIR`, and `CODEX_HOME` come from `$SHELL -lc`, no `-i`, once after launch
- Every other variable is launchd's
- Children inherit no `ANTHROPIC_` key, token, URL, or model override, no `CLAUDE_CODE_OAUTH_*`, and no Bedrock, Vertex, or Foundry switch
- Sign-out runs `claude auth logout` in the account's store, the active account's sign-out signs the CLI out
- Remove signs out, then deletes the private store, its Keychain item, and the account directory
- Launch deletes account directories with no record

[OPENAI]:
- `codex app-server` from `ChatGPT.app`, bundle `com.openai.codex`, runs once per `CODEX_HOME`, the next request after an exit restarts it
- Children inherit no `CODEX_*`, `OPENAI_API_KEY`, `OPENAI_BASE_URL`, `OPENAI_ORG_ID`, or `OPENAI_PROJECT_ID`
- `~/.codex/auth.json` is the one live sign-in, `Accounts/<id>/Codex/auth.json` the saved copies, identity is the file's `id_token` claims
- Switch stages each file beside its destination at mode 0600 and replaces it through `FileManager.replaceItemAt`
- Switch copies the live file into the outgoing account's directory, installs the incoming copy live, then removes the copy
- Switch requires `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset in `~/.codex/config.toml`
- Precondition refusal names its key
- Running `codex` keeps its account
- Desktop app rereads no `auth.json`, a switch quits and reopens a running one
- Sign-in opens the `account/login/start` page in the browser, a login without ChatGPT reads "Connect this account with a ChatGPT subscription"
- Active account is the live `auth.json`, read at launch, wake, panel open, every refresh, and on each change through a kqueue watch
- Live `auth.json` with an unknown identity adds an account row
- `account/rateLimits/updated` pushes from a running server update the card without a request
- Sign-out runs `account/logout` on the account's server
- Remove deletes the account's `Codex` directory

## [02]-[SESSIONS]

Sessions are 5-hour provider usage windows, each started by one greeting on the least costly model and never by selecting an account:
- Start Session reads usage first and sends the greeting only while the account is ready, else returns the reading
- Claude greeting is `claude -p` in stream-json against the account's own store, tools, hooks, and memory off, one turn, then `hi`
- Claude greeting reads its access token from an inherited descriptor named by `CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR`
- Claude model is the first enabled Haiku row of `list_models`, applied through `set_model`, stdin closes once the greeting is written
- OpenAI greeting is an ephemeral `thread/start` with every tool, feature, MCP server, and web search disabled, then `turn/start` with `hi`
- OpenAI model is `gpt-5.6-luna` unhidden in `model/list` at its lowest supported effort, the turn runs at the default service tier
- Exhausted or rejected weekly window, or included usage denied, blocks the account until the weekly reset
- Model window at 100% blocks nothing
- Session reset ahead is running, else the account is ready
- Greeting failure records the account's issue, the next report clears the card, nothing pending is written to disk
- Manual is the default policy, Automatic sends the greeting when a report shows the account ready, once per ready window
- Automatic attempt resets when a report shows the window running or the policy changes
- Refresh reads every connected account at launch, wake, panel open, and on each watch event, then every 60 s while the panel is open
- Panel closed, the tick runs every 5, 15, or 30 minutes by how recently the panel was open, and at each known reset boundary
- Claude usage is cached 15 minutes, a session start reads fresh after the greeting

## [03]-[INTERFACE]

Panel and Settings are system controls in semantic colors and fonts, appearance follows the system, rows, session controls, and gauges have tooltips:
- Panel is a 384pt `MenuBarExtra` window sized to its content, one card per account, issues under the cards
- Gear menu holds `Settings…` (⌘,) and `Quit Relay` (⌘Q), `Add Account…` stands alone while no account exists
- Email row selects the account, an accessory-bar button reading "In use" with a checkmark on the active account, a spinner over it while switching
- Gauges Session, Weekly, and one per model window sit beside the static provider symbol, label leading, `percent · reset` trailing
- One reset line sits under the weekly group
- Gauge track is a 6pt capsule progress style in quaternary, filled in accent, in secondary with dimmed labels while stale
- Gauge digits are monospaced and the gauge takes no focus
- Reading shows `<1%` under 1% and no percent at zero, weekly and model windows show the reset weekday and time
- Session reading is `2h 15m` while running, `Ready` idle, `Blocked` blocked, `Awaiting update` stale
- Session control reads `Start Session`, the countdown, `Blocked`, or `Cancel`, every other operation names itself beside it
- Elapsed time beside a running operation and in the sign-in sheet is a live `Text` over `durationOffset`
- Note under the gauges is the account's latest failure, else "Sign in required" for a signed-out account, else "Blocked until <reset>"
- Card context menu holds Move Up, Move Down, Remove Account…
- Settings is a `Window` scene with id `settings`, opened by ⌘, and the gear menu
- `Settings` scene draws no unified toolbar
- Settings window is 680×460 by default and 600×380 at minimum, titled by the email with the provider as subtitle, General for the pane
- Sidebar is General, then Accounts rows with symbol, email, and checkmark, drag to reorder, the card's context menu
- `+` as a provider menu and `−` on the selection sit in a control group at the sidebar's bottom
- Account detail is a grouped form: Active account (Use Account or In use), Session start picker, Sign Out or Sign In…, issue as footer
- Active account section of an OpenAI account shows the switch precondition refusal as its footer
- General holds the Launch at login switch and Open Login Items… while approval is pending, status is polled every second while the pane is open
- Sign-in sheet is 360pt wide, names the browser wait, Cancel deletes a new account's store
- Cancel stays enabled and reads "Canceling…" while the login child stops
- Sheet state belongs to the store, closing Settings mid-login cancels nothing
- Remove alert keeps Cancel as default and names the CLI sign-out for the active Claude account
- Menu bar symbol and provider symbols draw at their intrinsic 16pt, symbol stroke and bounds match SF Symbols at the 13pt menu bar configuration

## [04]-[STORAGE]

Everything Relay owns sits under `~/Library/Application Support/Relay`, `<id>` the account's UUID:

| [INDEX] | [PATH]                  | [CONTENT]                                                 |
| :-----: | :---------------------- | :-------------------------------------------------------- |
|  [01]   | `accounts.json`         | Accounts with identity, policy, sign-in state, last usage |
|  [02]   | `claude-selection.json` | Pending Claude switch: incoming, outgoing, phase          |
|  [03]   | `Accounts/<id>/Claude`  | Private Claude store, `.claude.json`                      |
|  [04]   | `Accounts/<id>/Codex`   | `CODEX_HOME` with the saved `auth.json`                   |
|  [05]   | `Session/`              | Working directory of the `claude` and `codex` children    |

- Claude credentials sit in the login Keychain, `Claude Code-credentials` for the shared store, `Claude Code-credentials-<hash>` per private store
- `<hash>` is the first 8 hex of SHA-256 over the `CLAUDE_CONFIG_DIR` string
- Item account is `$USER`
- Item reads, writes, and deletes are `/usr/bin/security` children
- Commands are `find-generic-password -w`, `add-generic-password -U -X <hex>` over `security -i`, and `delete-generic-password`
- Relay links no Keychain API
- `claude auth login` creates each private item, `security` is the only program in every item's ACL and partition list, `-T` is never passed
- `-w` output that is all hex is decoded as bytes, the macOS 26 form for non-printable data
- `security` exits with its `OSStatus` and `waitid` reports that value's low 24 bits, a shell's `$?` reports its low byte alone
- `errSecItemNotFound` is not found, `errSecInteractionNotAllowed` is a locked keychain, every other nonzero status is denied
- Claude Code's lock directories are created, touched, and removed through `FileManager`, inode and modification date decide a compromised lock

## [05]-[PERMISSIONS]

Relay holds no entitlement and calls no Automation, Accessibility, or notification API, its one prompt is the login-items notice:
- Build signs automatically with the Apple Development identity of team `BBXB9R367P` that Xcode holds for the signed-in Apple ID
- Designated requirement names the bundle id, Apple's anchor, the identity as leaf, and the WWDR intermediate, not the team
- CI signs ad-hoc through `CODE_SIGN_STYLE=Manual CODE_SIGN_IDENTITY=- DEVELOPMENT_TEAM=` forwarded to the build target
- Write through the legacy Keychain API resets an item's partition list to the writer, an in-process read then prompts after every CLI write
- Keychain raises no prompt while `/usr/bin/security` alone writes and reads every Claude Code item
- Launch at login registers `SMAppService.mainApp`, macOS posts its login-items notice once
- Profile reading Desktop, Documents, or Downloads under a login shell can raise a Files and Folders prompt
- Network is `URLSession` to `api.anthropic.com` for Claude usage and profile at a 15 s request timeout, OpenAI usage goes through the app-server

## [06]-[PROCESSES]

Every child runs under `Subprocess.run` from `swiftlang/swift-subprocess` in its own session, cancel is SIGTERM to the group, SIGKILL 2 s later:
- Every invocation races a deadline in a task group: login 5 min, greeting 2 min, refresh, usage, and logout 60 s, `security` 15 s, login shell 5 s
- `codex app-server` alone runs without a deadline, its requests carry their own
- Login shell is `SHELL` as a path, an unqualified name resolves on the launch `PATH`, a missing or unresolved name keeps launchd's variables
- Stream children (`claude -p`, `codex app-server`) are read and written inside the run closure, `Execution` stays inside it
- Cancel of any operation returns once the child is reaped, quit cancels every operation and waits 5 s at most
- Per-account operations run on the account's one task, a switch waits for every other account's task before it starts
- Click refused while the account's task runs records an issue on the card

## [07]-[PROJECT]

`Relay.xcodeproj` is Xcode 27's format, `objectVersion = 110`, its synchronized root folder puts every file under `apps/relay` in the target:
- Sources sit under `App/` (store, model, paths, watch, login shell, login item), `Accounts/` (domain, repository), `Providers/`, and `Views/`
- `Providers/` holds one client per provider, process runner, and keychain tool
- Membership exceptions leave `.lldbinit`, AGENTS.md, CLAUDE.md, LICENSE, and `Relay.xcodeproj` out
- Bundle resources are `Assets.car` and `AppIcon.icns`
- `Relay.xcodeproj` outside the exceptions makes Xcode add a `projectReferences` entry to itself at each save
- `Relay.xcscheme` names `$(SRCROOT)/.lldbinit` as the run action's LLDB Init File
- `.lldbinit` runs `protocol-server start MCP`, `xcrun lldb-mcp` reaches every debug session
- `swift-subprocess` is the one package, `Package.resolved` under the workspace pins it
- `Relay:build` passes `-disableAutomaticPackageResolution`, a stale `Package.resolved` fails the build instead of moving the pin
- Derived data holds the whole build tree, `-derivedDataPath` moves `Relay:build`'s under `.cache/xcode`
- Project-level `CONFIGURATION_BUILD_DIR` or `BUILD_DIR` moves the app product alone and leaves the package products in the tree
- Project-level `SYMROOT` turns on legacy build locations, legacy build locations refuse packages
- Bundle `app.rasm.relay` on macOS 26.0, Swift 6 with approachable concurrency and MainActor default isolation, warnings as errors
- Upcoming features are existential `any`, internal imports by default, and member import visibility
- Hardened runtime is on and the App Sandbox off, children spawn and inherit the greeting descriptor under it
- Generated Info.plist, `INFOPLIST_KEY_LSUIElement` its one added key, keeps Relay out of the Dock
- `AppIcon.icon` is an Icon Composer document, fill `#FAFAFA` light and `#242424` dark with the glyph `Relay.svg` inverted
- `actool` renders the icon stack for Aqua, DarkAqua, and Tintable, the 16 to 1024 renditions, and `AppIcon.icns`, no PNG is checked in
- `Claude`, `OpenAI`, and `RelaySymbol` are single-SVG imagesets with template rendering and preserved vector representation
- `Relay:lint` and `Relay:format` run `swift-format`, `Relay:check` depends on `build` and `lint`
- CI runs the affected `check` of macOS projects on the `xcode-27` runner
