# [RELAY]

- Relay shows Claude and OpenAI subscription usage in the menu bar, switches the active account, and starts a 5-hour session on any account
- `Relay:build` places the Debug `Relay.app` under `.cache/xcode/apps/relay/Build/Products`, `Relay:install` the Release one under `/Applications`
- Views reuse a status line or prompt the provider CLI prints verbatim, with no trailing ellipsis

## [01]-[ACCOUNTS]

Provider CLI and desktop app run as the selected account, Relay stores every other account's credential:
- Sign-in on an existing account must return its identity, a different identity is signed out and refused
- Added account matching a connected account is refused, one matching a signed-out account replaces its record
- Active sign-in with an unknown identity adds an account
- Launch deletes account directories with no record

[CLAUDE]:
- `claude` resolves in user and system install directories, then launchd's `PATH`
- `claude auth login --claudeai` in the account's private store signs in through the browser, its exit status decides the outcome
- Code pasted at `Paste code here if prompted` goes to the login child's stdin
- Shared store is the directory `CLAUDE_SECURESTORAGE_CONFIG_DIR` names, else `CLAUDE_CONFIG_DIR`, else `~/.claude`
- `.claude.json` sits inside `CLAUDE_CONFIG_DIR` when set, else at `~/.claude.json`
- Active account is the shared store's `oauthAccount`
- Shared Keychain item holds the one live credential, every other account's credential sits in its private item
- Switch copies the shared credential into the outgoing private item, then moves the incoming item and `oauthAccount` into the shared store
- Switch runs no child process or network request inside its locks
- Switch holds Claude Code's `.oauth_refresh.lock` pair on every store it touches, 10 attempts 100 ms to 1 s apart
- Crash mid-switch leaves its record in `claude-selection.json`, the next launch deletes the private copy of the credential the shared item holds
- Running CLI keeps its token until its next refresh reads the shared item, new invocations read it at once
- Every selection read (launch, wake, panel open, `.claude.json` change, after each operation) re-reads the shared item so the last-read credential is at most one read old
- The CLI holds `.oauth_refresh.lock` in the store while it rotates a token, Relay re-reads the shared item when that directory vanishes
- Refresh tokens are single use and the grant ends 30 days after the sign-in, no refresh extends it, the card shows the date as "Oct 12" at the trailing end of the account row with the full date on hover, "Login expired" as the note past it
- Codex stores no login expiry, `auth.json` carries `last_refresh` and JWT `exp` alone and the app-server reports auth mode, email, and plan, so a Codex card shows no date
- Access token is refreshed 10 minutes before expiry, wider than the CLI's own 5-minute margin so a greeting child never rotates the token itself, a refresh-token expiry triggers nothing
- Refresh tokens are single use, so the refresh child runs on its own task that no cancel or quit reaches, a rotation the server completed is persisted by the child or lost for good
- Refresh children run one at a time across stores, tokens refreshed in one batch expire in one batch and usage reads stay concurrent
- Login outside Relay saves the outgoing account's last-read credential into its private item
- Saved credential that rotated since its last read requires a new sign-in
- `claude -p` in the account's store is the one credential refresh, run when the access token or refresh token nears expiry
- 401 on a usage read refreshes once, a second 401 is reported and the account stays connected, sign-in required comes from the store alone
- CLI that gets `invalid_grant` on a refresh blanks the item's access and refresh tokens and keeps its metadata, the blank item reads as signed out
- Item with an empty `refreshToken` reads as signed out
- Locked login keychain keeps the account connected
- 429 waits `Retry-After`, a missing or zero value leaves the next scheduled read in place
- Children inherit no Claude Code credential, endpoint, model, cloud, or remote-session variable
- Sign-out runs `claude auth logout` in the account's store
- Remove signs out, then deletes the Keychain item and the account directory

[OPENAI]:
- `codex app-server` from `ChatGPT.app`, bundle `com.openai.codex`, runs once per `CODEX_HOME`, the next request after an exit restarts it
- Server version comes from `initialize`'s `userAgent`, a rejected request reads "Codex <version> rejected <method>, <message>" so a ChatGPT update that changes the protocol names itself
- Children inherit no `CODEX_*` or OpenAI API variable, `CODEX_HOME` names the account's home
- Active account is `auth.json` under `CODEX_HOME`, else `~/.codex`, identity is its `id_token` claims
- Switch copies the live `auth.json` into the outgoing account's directory, installs the incoming copy live, then removes the copy
- Switch writes each `auth.json` at mode 0600 through `FileManager.replaceItemAt`
- Switch requires `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset in live `config.toml`
- Running `codex` keeps its account, a switch quits and reopens a running desktop app
- Sign-in opens the `account/login/start` page in the browser, sign-out runs `account/logout`
- `account/rateLimits/updated` pushes from a running server update usage without a request

## [02]-[SESSIONS]

Sessions are 5-hour usage windows, each started by one greeting on the least costly model from the account's own store, selection starts none:
- Session start reads usage first and sends the greeting only while the account is ready
- Claude greeting is `claude -p` in stream-json with tools, hooks, and memory off, one turn, then `hi`
- Claude greeting reads its access token from an inherited descriptor named by `CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR`
- Claude model is the first enabled Haiku entry of `list_models`, applied through `set_model`
- OpenAI greeting is an ephemeral `thread/start` with every tool, feature, MCP server, and web search disabled, then `turn/start` with `hi`
- OpenAI model is `gpt-5.6-luna` unhidden in `model/list` at its lowest supported effort, the turn runs at the default service tier
- Exhausted or rejected weekly window, or included usage denied, blocks the account until the weekly reset
- Model window at 100% blocks nothing
- Limit reporting no window of 12 h or less has no session, its card shows no session gauge and no start, a Pro Codex account reports the weekly window alone while the Spark limit carries 5 h and weekly
- Manual is the default policy, Automatic sends the greeting once per ready window
- Launch, wake, panel open, and each change of `.claude.json` or the live `auth.json` reread the active accounts
- Usage refreshes when `NWPathMonitor` reports the path satisfied, at launch and once the network returns after a wake, and on panel open, a dark wake with no network refreshes nothing
- Refresh ticks every 60 s while the panel is open, every 5, 15, or 30 minutes by how recently it was open while closed, and at each known reset
- Every refresh is a request, the schedule alone decides how often Claude usage is read
- Claude session reset is `rate_limit_info.unifiedWindows.five_hour.resetsAt` of the `rate_limit_event` the stream emits after the greeting turn, the top-level `rateLimitType` names the limiting window alone and reads `seven_day` once weekly usage passes session usage, the usage endpoint reports the window later and a reading without a reset keeps the one already known
- Session start reads usage again after the greeting and carries the event's reset into that reading
- Session start control stays in place under a spinner overlay with its symbol hidden while the greeting runs, no elapsed counter

## [03]-[INTERFACE]

- Panel and Settings use system controls, semantic colors, fonts, and appearance
- Settings is a `Window` scene, the `Settings` scene disables minimize and zoom and stacks its toolbar under the title
- Panel opens Settings through `openWindow`, then `NSWorkspace.openApplication` on the running bundle
- Cooperative activation honors `NSWorkspace.openApplication`, `NSApplication.activate` from the non-activating panel is refused
- Panel window sets `allowsToolTipsWhenApplicationIsInactive`, a window of an inactive app shows no `.help` text by default
- Settings sidebar removes the `NavigationSplitView` sidebar toggle
- `SMAppService.Status.notFound` is the never-registered state on macOS 26, the switch reads off and stays enabled
- Thrown registration error is the General pane footer
- Login item status is read when the pane appears and each time Relay becomes active, `SMAppService` posts no status notification
- Controls keep their label while their operation runs, the status line names the state
- Sheet state belongs to the store, closing Settings mid-login cancels nothing
- Error text in the unified log is public, provider failures name no token

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
- Claude Code's locks are directories created through `FileManager`, inode and modification date decide a compromised lock

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
- Stream children (`claude auth login`, `claude -p`, `codex app-server`) are read and written inside the `run` body closure
- Cancel of any operation returns once the child is reaped, quit cancels every operation and waits 5 s at most
- Per-account operations run on the account's one task, a switch waits for the outgoing account's task alone since every other account works in its private store
- Session start clicked during a usage refresh starts once the refresh finishes, a cancelled operation keeps the usage it found

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
