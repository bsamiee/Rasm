# [RELAY]

- Relay shows Claude and OpenAI subscription usage in the menu bar, switches the active account, and opens a 5-hour window on any account.
- `Relay:build` places the Debug `Relay.app` under `.cache/xcode/apps/relay/Build/Products`, `Relay:install` the Release one at `/Applications`.

## [01]-[ACCOUNTS]

Settings' `+` menu adds an account, a panel provider symbol or Use Account selects it, provider CLI and app run as the selected account:
- Sign In… must return the account's identity, a different identity is signed out and refused
- Re-sign-in that ends signed out reads "Sign in to the account you selected", a duplicate add reads "<email> is already connected"
- Active sign-in with an unknown identity adds an account row
- Launch deletes account directories with no record

[CLAUDE]:
- `claude` resolves in user and system install directories, then launchd's `PATH`, a miss reads "Install Claude Code to connect an account"
- Sign-in runs `claude auth login --claudeai` in the account's private store
- Shared store is the directory `CLAUDE_SECURESTORAGE_CONFIG_DIR` names, else `CLAUDE_CONFIG_DIR`, else `~/.claude`
- Shared item holds the one live credential, every other account's credential sits in its private item
- Active account is the shared store's `oauthAccount`
- `.claude.json` sits inside `CLAUDE_CONFIG_DIR` when set, else at `~/.claude.json`, a `.config.json` in the store directory is read first
- Switch copies shared credential into outgoing private item, then moves incoming private item into shared item
- Switch writes incoming `oauthAccount` into `.claude.json`, runs no child process, and makes no network request inside its locks
- Switch holds Claude Code's `.oauth_refresh.lock` pair on every store it touches, 10 attempts at 100 ms to 1 s apart
- Item writes hold `.storage-write.lock` of their store
- Crash mid-switch leaves its record in `claude-selection.json`, the next launch deletes the private copy of the credential the shared item holds
- Running CLI keeps its token until its next refresh reads the shared item, new invocations read it at once
- Login outside Relay saves the outgoing account's last-read credential into its private item
- Outgoing credential that rotated since its last read reads "Sign in to this Claude account again"
- `claude -p` in the account's store is the one credential refresh, run when the access token or refresh token nears expiry
- Item with an empty `refreshToken` reads as signed out
- Denied Keychain request reads "macOS refused Relay’s Keychain request for this Claude sign-in"
- Locked login keychain reads "Unlock the login keychain to read this Claude sign-in" on the card, the account stays connected
- 401 on a usage read refreshes once through the CLI, a second 401 reads "Sign in to this Claude account again"
- 429 waits `Retry-After`, a missing or zero value waits the usage cache duration
- Children inherit no Claude Code credential, endpoint, model, cloud, or remote-session variable
- Sign-out runs `claude auth logout` in the account's store, the active account's sign-out signs the CLI out
- Remove signs out, then deletes the Keychain item and the account directory

[OPENAI]:
- `codex app-server` from `ChatGPT.app`, bundle `com.openai.codex`, runs once per `CODEX_HOME`, the next request after an exit restarts it
- Children inherit no `CODEX_*` or OpenAI API variable, `CODEX_HOME` names the account's home
- Active account is `auth.json` under `CODEX_HOME`, else `~/.codex`, identity is its `id_token` claims
- Switch copies the live file into the outgoing account's directory, installs the incoming copy live, then removes the copy
- Switch writes each `auth.json` at mode 0600 through `FileManager.replaceItemAt`
- Switch requires `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset in live `config.toml`, refusal names the key
- Running `codex` keeps its account, a switch quits and reopens a running desktop app
- Sign-in opens the `account/login/start` page in the browser, a login without ChatGPT reads "Connect this account with a ChatGPT subscription"
- `account/rateLimits/updated` pushes from a running server update the card without a request
- Sign-out runs `account/logout` on the account's server
- Remove deletes the account's `Codex` directory

## [02]-[SESSIONS]

Sessions are 5-hour usage windows, each started by one greeting on the least costly model from the account's own store, never by selecting it:
- Session start reads usage first and sends the greeting only while the account is ready, else returns the reading
- Claude greeting is `claude -p` in stream-json, tools, hooks, and memory off, one turn, then `hi`
- Claude greeting reads its access token from an inherited descriptor named by `CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR`
- Claude model is the first enabled Haiku row of `list_models`, applied through `set_model`, stdin closes once the greeting is written
- OpenAI greeting is an ephemeral `thread/start` with every tool, feature, MCP server, and web search disabled, then `turn/start` with `hi`
- OpenAI model is `gpt-5.6-luna` unhidden in `model/list` at its lowest supported effort, the turn runs at the default service tier
- Exhausted or rejected weekly window, or included usage denied, blocks the account until the weekly reset
- Model window at 100% blocks nothing
- Session reset ahead is running, else the account is ready
- Greeting failure records the account's issue, the next report clears it
- Manual is the default policy, Automatic sends the greeting when a report shows the account ready, once per ready window
- Automatic attempt resets when a report shows the window running or the policy changes
- Launch, wake, panel open, and each change of `.claude.json` or the live `auth.json` reread the active accounts, then refresh usage
- Refresh ticks every 60 s while the panel is open, every 5, 15, or 30 minutes by how recently it was open while closed, and at each known reset
- Claude usage is cached 15 minutes, a session start reads fresh after the greeting

## [03]-[INTERFACE]

Panel and Settings use system controls, semantic colors, fonts, and appearance, tooltips sit on provider symbols, the session start, and gauges:
- Panel is a 384pt `MenuBarExtra` window sized to its content, one card per account, issues under the cards
- Gear menu holds `Settings…` (⌘,) and `Quit Relay` (⌘Q), `Add Account…` stands alone while no account exists
- Glyph controls (gear, provider switch, session start) are accessory-bar buttons, system symbols at the body size
- Provider symbol in accent marks the active account, tooltip "Active account"
- Provider symbol in secondary is the button switching to the account, a spinner over it while switching
- Gauges Session, Weekly, and one per model window sit beside the provider symbol, label leading, `percent · reset` trailing
- Gauge track is a 6pt capsule progress style in quaternary, filled in accent, in secondary with dimmed labels while stale, digits monospaced
- Gauge reads to accessibility as one progress indicator, `<title>, <reading>` with the fraction as value, the session start its own element
- Reading shows `<1%` under 1% and no percent at zero, weekly and model windows show the reset weekday and time
- Session reading is `2h 15m` while running, the start symbol while ready with a current reading, `Blocked` blocked, `Awaiting update` stale
- Session start is `arrow.trianglehead.clockwise` at the reading's trailing end, a spinner over it and Cancel while starting
- Session start stays shown through a refresh, every other operation hides it
- Elapsed time beside the session start and in the sign-in sheet is a live `Text` over `durationOffset`
- Note under the gauges is the latest failure, else the running sign-out or removal, else "Sign in required" signed out, else "Blocked until <reset>"
- Card context menu holds Move Up, Move Down, Remove Account…
- Menu bar and provider symbols draw at their intrinsic 16pt with stroke and bounds matching SF Symbols at the 13pt menu bar configuration
- Settings is a `Window` scene with id `settings` in the unified toolbar style
- Settings window is 680×460 by default and 600×380 at minimum, titled by the email with the provider as subtitle, General for the pane
- Sidebar is General, then Accounts rows with symbol, email, and checkmark, drag to reorder, the card's context menu
- `+` as a provider menu and `−` on the selection sit in a control group at the sidebar's bottom
- Account detail is a grouped form: Active account (Use Account or Active), Session start picker, Sign Out or Sign In…, issue as footer
- Active account section of an OpenAI account shows the switch precondition refusal as its footer
- General holds the Launch at login switch and Open Login Items… while approval is pending, status is polled every second while the pane is open
- Sign-in sheet is 360pt wide, names the browser wait, Cancel deletes a new account's store
- Cancel stays enabled and reads "Canceling…" while the login child stops
- Closing Settings mid-login cancels nothing, sheet state belongs to the store
- Remove alert keeps Cancel as default and names the CLI sign-out for the active Claude account

## [04]-[STORAGE]

Stored state sits under `~/Library/Application Support/Relay`, `<id>` the account's UUID:

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
- Reads, writes, and deletes run `security` (`find-generic-password -w`, `add-generic-password -U -X <hex>` over `-i`, `delete-generic-password`)
- `claude auth login` creates each private item, `security` alone sits in every item's ACL and partition list
- `-w` prints non-printable data as hex on macOS 26, all-hex output is decoded as bytes
- `security` exits with its `OSStatus`, `waitid` reports the low 24 bits and a shell's `$?` the low byte
- `errSecItemNotFound` is not found, `errSecInteractionNotAllowed` is a locked keychain, every other nonzero status is denied
- Claude Code's lock directories are managed through `FileManager`, inode and modification date decide a compromised lock

## [05]-[PERMISSIONS]

Bundle declares no entitlement and links no Keychain, Automation, Accessibility, or notification API:
- Hardened runtime is on with the App Sandbox off, children spawn and inherit the greeting descriptor under it
- Keychain raises no prompt while `/usr/bin/security` alone reads and writes every Claude Code item
- In-process Keychain writes reset an item's partition list to the app, every later CLI write then makes in-process reads prompt
- Launch at login registers the running bundle through `SMAppService.mainApp`, the `/Applications` copy is the one to register
- Login shell profile that reads Desktop, Documents, or Downloads can raise a Files and Folders prompt
- Network is `URLSession` to `api.anthropic.com` for Claude usage and profile at a 15 s request timeout, OpenAI usage goes through the app-server

## [06]-[PROCESSES]

Every child runs under `Subprocess.run` from `swiftlang/swift-subprocess` in its own session, cancel is SIGTERM to the group, SIGKILL 2 s later:
- Every invocation races a deadline (login 5 min, greeting 2 min, refresh, usage, and logout 60 s, `security` 15 s, login shell 5 s)
- `codex app-server` alone runs without a deadline, its requests carry their own
- Login shell is `SHELL` as a path, an unqualified name resolves on the launch `PATH`, a missing or unresolved name keeps launchd's variables
- `CLAUDE_CONFIG_DIR`, `CLAUDE_SECURESTORAGE_CONFIG_DIR`, and `CODEX_HOME` come from `$SHELL -lc` once after launch, every other variable is launchd's
- Stream children (`claude -p`, `codex app-server`) are read and written inside the `run` body closure
- Cancel of any operation returns once the child is reaped, quit cancels every operation and waits 5 s at most
- Per-account operations run on the account's one task, a switch waits for every other account's task before it starts
- Click refused while the account's task runs records an issue on the card

## [07]-[PROJECT]

```text
apps/relay/
├── App/        # Entry point, store, and host services
├── Accounts/   # Domain types and their storage
├── Providers/  # One client per provider with process and keychain tools
└── Views/      # Panel and Settings
```

- MainActor is the default isolation under approachable concurrency, `nonisolated` marks every type off the main actor
- Generated Info.plist keeps Relay out of the Dock and files it under Utilities
- Provider and menu bar symbols are single-SVG imagesets with template rendering and preserved vector representation
