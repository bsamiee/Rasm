# [RELAY]

Relay shows Claude and OpenAI subscription usage in the menu bar, switches each provider's active account, and starts 5-hour sessions.

## [01]-[LAYOUT]

Views call `AccountStore`, the store calls one client actor per provider, and clients reach providers through children, files, and HTTP:

```text
apps/relay/
├── Accounts/   # Domain types, usage availability, accounts.json codec, error accumulation
├── App/        # Store, account model, file locations, watches, login shell, login item, entry point
├── Providers/  # Claude and Codex clients and protocols, Keychain, Claude Code locks, process runner
└── Views/      # Menu bar panel, Settings window, gauges, text formats
```

- `AccountStore` owns every trigger (launch, wake, network, file watch, schedule, user action) and every `AccountModel` state change
- `ClaudeClient` and `CodexClient` return `ClaudeFailure` and `CodexFailure`, `erased()` lifts both into `ProviderError` at the store
- `requiresSignIn` failures mark an account signed out and deselect it, every other failure keeps it connected with an issue on its card
- `isCancellation` failures set no issue, keep usage, and log nothing

## [02]-[STORAGE]

Stored state sits under `~/Library/Application Support/Relay`, `<id>` an account's UUID:

| [INDEX] | [PATH]                  | [CONTENT]                                             |
| :-----: | :---------------------- | :---------------------------------------------------- |
|  [01]   | `accounts.json`         | Account order, identity, policy, sign-in state, usage |
|  [02]   | `claude-selection.json` | Claude switch in progress: incoming, outgoing, phase  |
|  [03]   | `Accounts/<id>/Claude`  | Private Claude config directory                       |
|  [04]   | `Accounts/<id>/Codex`   | Private `CODEX_HOME`                                  |
|  [05]   | `Session/`              | Working directory of every `claude` and `codex` child |

- Failed load of `accounts.json` disables adding accounts and starts no trigger

## [03]-[ACCOUNTS]

Each provider's CLI and desktop app run as its active account, Relay keeps every other account's credential in a private store:
- Selected account reads and writes the provider's live store, every other account its private store
- `AccountIdentity.isSameAccount` compares account id and organization id, every identity check goes through it
- Sign-in to an existing account that returns another identity signs that store out and fails
- New sign-in matching a connected or busy account is refused, one matching a signed-out account replaces that record
- Active account with an unknown identity joins as a connected account
- Launch finishes a recorded Claude switch, then deletes each account directory with no record, with its Claude credential

## [04]-[CLAUDE]

Claude stores are config directories with an account file and a login Keychain item, `ClaudeClient` resolves a shared store and one per account:
- Shared store directory is `CLAUDE_SECURESTORAGE_CONFIG_DIR`, else `CLAUDE_CONFIG_DIR`, else `~/.claude`
- Account file is `.config.json` in a config directory when present, else `.claude.json` in `CLAUDE_CONFIG_DIR` or at `~/.claude.json`
- Active Claude account is `oauthAccount` of the shared account file
- Item service is `Claude Code-credentials-<hash>` over a store's NFC config directory path, `Claude Code-credentials` with no path
- `Keychain` reads, writes, and deletes every item through `/usr/bin/security`
- Every `claude` child takes its environment from `processEnvironment`, which drops `excludedEnvironmentVariables` and points at the child's store
- Switch holds Claude Code's lock pair through `ClaudeLock` on every store it touches and runs no `claude` child or HTTP request inside
- Switch saves the shared credential into the outgoing private store, then installs the incoming item and `oauthAccount` into the shared store
- `claude-selection.json` records each switch phase, launch finishes a recorded switch by deleting the private copy the shared store holds
- Access token refreshes within 10 minutes of `expiresAt`, refresh-token expiry triggers nothing
- Refresh is a `claude -p` child in an account's store with no token, Relay reads the rotated item after it exits
- Refresh runs on an unstructured task no cancel or quit reaches, `rotations` runs refreshes one at a time across stores
- 401 on a request refreshes once and retries, a second 401 is an account issue and keeps it connected
- `/api/oauth/profile` checks each new access token against a stored identity before any request uses it
- Each selection read keeps the active account's credential, `saveLastCredential` writes it into the outgoing private item after an outside login
- 429 blocks usage reads until `Retry-After`

## [05]-[CODEX]

`CodexClient` drives the desktop app's `codex app-server` over JSON lines on stdio:
- Server binary is `Contents/Resources/codex` in the app with bundle id `com.openai.codex`
- One server runs per `CODEX_HOME`, the next request after an exit starts a new one
- Server environment drops `CODEX_*` and `excludedEnvironmentVariables`, then sets `CODEX_HOME` to an account's home
- Selected account's home is launch `CODEX_HOME`, else `~/.codex`
- Identity is `id_token` claims of `auth.json`, a usage response for another workspace fails as `identityChanged`
- Notifications a later wait claims belong in `retainedNotifications`, `account/rateLimits/updated` updates usage without a request
- Switch requires top-level `config.toml` to leave `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset
- Switch stops every server it touches, moves `auth.json` files, then quits and reopens a running desktop app

## [06]-[SESSIONS]

Session start reads usage and sends one greeting while `AccountUsage.availability` is `ready`:
- Weekly window exhausted or rejected, or included usage denied, blocks an account until its weekly reset
- Session and model windows at 100% block nothing
- Claude greets on the first enabled Haiku model, OpenAI on `gpt-5.6-luna` at its lowest effort, both with tools, MCP, hooks, and memory off
- Claude greeting reads its access token from a pipe that `CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR` names
- Claude session reset comes from the greeting's `rate_limit_event`, set on a session window the next usage read reports without one
- Window with no reset keeps a known future reset of its kind
- Automatic policy starts once per ready window

## [07]-[TRIGGERS]

`AccountStore` reads selection and usage on events and on a schedule:
- Selection read runs at launch, wake, panel open, network return, each change of shared `.claude.json` or live `auth.json`, and refresh lock removal
- Usage refresh runs when the network path turns satisfied (launch included), on panel open, after a switch, and on schedule
- Schedule ticks every 60 s while the panel is open, else every 5, 15, or 30 minutes by time since it last opened, and past each known reset
- Refreshes and watch events skip while a switch runs

## [08]-[PROCESSES]

`ProcessRun` starts every child through `Subprocess.run` in its own session, teardown sends SIGTERM to its group and SIGKILL 2 s later:
- Every child runs under a deadline except `codex app-server`, which puts one on each request
- Stream children are read and written inside the `run` body closure
- `$SHELL -lc` runs once at launch and overlays `LoginShell.variables` on the launch environment, a failed run keeps the launch environment
- Each account runs one operation at a time, and a switch waits for the outgoing account's operation alone
- Quit cancels every operation, waits 5 s at most, then saves accounts

## [09]-[INTERFACE]

Panel and Settings use system controls, semantic colors, and fonts:
- Panel opens Settings by dismissing itself, `openWindow`, then `Activation.requestFront` through `NSWorkspace.openApplication`
- Sign-in sheet state and task belong to `AccountStore`, closing Settings mid-login cancels nothing
- Login item registers the running bundle through `SMAppService.mainApp`, its status reloads on panel open, pane appearance, and activation
- Provider failures log with `privacy: .public` under subsystem `app.rasm.relay`, failure cases hold no token

## [10]-[PROOF]

`nx run Relay:check` builds Debug and runs `swift-format lint --strict`:
- `nx run Relay:build` places Debug `Relay.app` under `.cache/xcode/apps/relay/Build/Products/Debug`
- `nx run Relay:install` places Release `Relay.app` under `/Applications`, the bundle a login item registers
- Hardened runtime is on, App Sandbox is off, and the target declares no entitlements
- Relay starts from its process environment, a launch through `open` gives it launchd's variables
- Scheme's Run loads `.lldbinit`, which starts LLDB's MCP protocol server
- `log stream --predicate 'subsystem == "app.rasm.relay"'` shows provider and storage errors
