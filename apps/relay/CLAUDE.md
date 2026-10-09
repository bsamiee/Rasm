# [RELAY]

Relay shows Claude and OpenAI subscription usage in the menu bar, switches each provider's active account, and starts 5-hour sessions

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
- `ProviderClient` methods of `ClaudeClient` and `CodexClient` return `ProviderError` over `ClaudeFailure` or `CodexFailure`
- `requiresSignIn` failures mark an account signed out, every other failure keeps it connected with an issue on its card
- `readSelection` alone sets `isSelected`
- `isCancellation` failures set no issue, keep usage, and log nothing

## [02]-[STORAGE]

Stored state sits under `~/Library/Application Support/Relay`, `<id>` an account's UUID:

| [INDEX] | [PATH]                  | [CONTENT]                                                           |
| :-----: | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `accounts.json`         | Account order, identity, policy, sign-in state, usage, `retryAfter` |
|  [02]   | `claude-selection.json` | Claude switch in progress: incoming, outgoing                       |
|  [03]   | `Accounts/<id>/Claude`  | Private Claude config directory                                     |
|  [04]   | `Accounts/<id>/Codex`   | Private `CODEX_HOME`                                                |
|  [05]   | `Session/`              | Working directory of every `claude` and `codex` child               |

- Failed load of `accounts.json` disables adding accounts and starts no trigger
- `retryAfter` keeps the later date, clears on a successful usage read or sign-in, and loads only while in the future

## [03]-[ACCOUNTS]

Each provider's CLI and desktop app run as its active account, Relay keeps every other account's credential in a private store:
- Selected account reads and writes the provider's live store, every other account its private store
- Credential operations reserve affected stores through suspension and selection publication
- `AccountIdentity.isSameAccount` compares account id and organization id, every identity check goes through it
- Sign-in to an existing account that returns another identity signs that store out and fails
- New sign-in matching a connected or busy account is refused, one matching a signed-out account replaces that record
- Active account with an unknown identity joins as a connected account
- Launch deletes each account directory a non-empty `accounts.json` holds no record for, with its credentials

## [04]-[CLAUDE]

Claude stores are config directories with an account file and a login Keychain item, `ClaudeClient` resolves a shared store and one per account:
- Shared store directory is `CLAUDE_SECURESTORAGE_CONFIG_DIR`, else `CLAUDE_CONFIG_DIR`, else `~/.claude`
- Account file is `.config.json` in a config directory when present, else `.claude.json` in `CLAUDE_CONFIG_DIR` or at `~/.claude.json`
- Active Claude account is `oauthAccount` of the shared account file
- Store with a token and no `oauthAccount` stays connected, `/api/oauth/profile` supplies its `oauthAccount`
- Item service is `Claude Code-credentials`, suffixed `-<first 8 hex of SHA-256>` of the NFC config directory path when one is set
- `Keychain` reads, writes, and deletes every item through `/usr/bin/security`, the one application each item's ACL lists
- `security` exiting with `errSecItemNotFound` is a missing item, with `errSecInteractionNotAllowed` a locked keychain
- `waitid` reports the low 24 bits of an `OSStatus` a child exits with, `Keychain` masks each `errSec` code to 24 bits
- Item writes send hex on `security -i` stdin, never argv, a command over the 4095 bytes one line holds writes nothing and fails as too large
- Item writes replace `claudeAiOauth` alone and keep `mcpOAuth`, the user's MCP server logins
- Item with a blank `accessToken` or `refreshToken` reads as signed out, Claude Code's sign-out write blanks both with `expiresAt` 0
- Every `claude` child takes `processEnvironment`, which drops `excludedEnvironmentVariables` and points at the child's store
- Switch holds Claude Code's lock pair through `ClaudeLock` on every store it touches and runs no `claude` child or HTTP request inside
- Switch copies the shared credential into its account's private store, then moves the incoming credential and `oauthAccount` into the shared store
- Launch finishes a switch `claude-selection.json` still records, under the lock on every store it names
- Finishing deletes each private item holding the shared access token, after writing its `oauthAccount` into the shared account file
- After an outside login, `saveLastCredential` writes the credential the last selection read kept into the outgoing private store when newer

## [05]-[OAUTH]

Relay refreshes Claude tokens as a peer of Claude Code, under the same lock pair and compare-and-swap write:
- Refresh tokens are single use, a second use revokes the token family and signs out every holder
- Access token refreshes when `expiresAt` is within 300 s, a missing `expiresAt` or refresh-token expiry triggers none
- Refresh holds `ClaudeLock` on the store, re-reads it, and adopts a stored access token that differs from the one it replaces
- Refresh posts a JSON `refresh_token` grant with Claude Code's client id and the stored scopes joined by spaces
- Refresh writes the rotated pair when the stored `refreshToken` is empty or the posted one, else adopts the rotation another process wrote
- `refreshTokenExpiresAt` stays 30 days after the browser sign-in unless a token response names `refresh_token_expires_in`
- `invalid_grant` on 400 or 401 writes Claude Code's sign-out item while the store holds the posted token, then requires sign-in
- `account_on_hold` in `error`, `error.type`, or `error_description` on 400, 401, or 403 wins over `invalid_grant` and keeps the token
- On-hold cards link `error_uri`, else `https://claude.ai/restricted`
- Non-200 responses classify from the JSON body and name their endpoint, status, and server message when present
- 401 forces one refresh, drops the failed token's verified identity, and retries once
- `/api/oauth/profile` checks each new access token against a stored identity unless its token response named account and organization
- Every request sends `User-Agent: claude-cli/<version> (external, cli)`, the token endpoint returns 429 to other User-Agents except axios's
- `/api/oauth/usage` throttles other User-Agents harder and allows one read per account every 3 to 5 minutes
- Usage 429 `Retry-After` counts down to a fixed instant that repeated reads do not extend
- `Retry-After` is integer seconds or an HTTP-date, a missing or 0 value means 300 s, and waits cap at 24 h

## [06]-[CODEX]

`CodexClient` drives the desktop app's `codex app-server` over JSON lines on stdio:
- Server binary is the `entrypoint` of `Contents/Resources/codex-cli/codex-package.json` in the app with bundle id `com.openai.codex`
- One server runs per `CODEX_HOME`, the next request after an exit starts a new one
- Server environment drops `CODEX_*` and `excludedEnvironmentVariables`, then sets `CODEX_HOME` to an account's home
- Selected account's home is `CODEX_HOME`, else `~/.codex`
- Identity is `id_token` claims of `auth.json`, a usage response for another workspace fails as `identityChanged`
- Notifications a later wait claims belong in `retainedNotifications`
- Switch reads effective configuration through `config/read`, requiring `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset
- Switch stops affected servers before reading credential bytes with identity, and rejects outgoing identity changes
- Finished switch quits and reopens a running desktop app

## [07]-[SESSIONS]

Session start reads usage and sends one greeting while `AccountUsage.availability` is `ready`:
- Weekly window exhausted or rejected, or included usage denied, blocks an account until its weekly reset
- Session and model windows at 100% block nothing
- Claude greets on the first enabled Haiku model, OpenAI on `gpt-5.6-luna` at its lowest effort, both with tools, MCP, hooks, and memory off
- Claude greeting reads its access token from a pipe that `CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR` names
- Claude session reset comes from the greeting's `rate_limit_event`, set on a session window the next usage read reports without one
- Window with no reset keeps a known future reset of its kind
- Automatic policy starts once per ready window

## [08]-[TRIGGERS]

`AccountStore` reads selection and usage on events and on a schedule:
- Selection read runs at launch, wake, panel open, network return, each change of shared `.claude.json` or live `auth.json`, and refresh lock removal
- Usage read runs when the network path turns satisfied (launch included), on panel open, after a switch or selection change, and on schedule
- Select, sign-in, session start, automatic policy, and Codex `account/rateLimits/updated` force a full usage read
- Other usage reads skip accounts with a future `retryAfter` or recent usage
- Recent usage is `.current` and under 180 s old, a `.stale` snapshot loaded at launch or left by a failed read never counts
- Schedule ticks every 180 s while the panel is open, else every 5, 15, or 30 minutes by time since it opened
- Reset and `retryAfter` wakes respect the account's next permitted read, resets observed by a later usage snapshot schedule nothing
- Each refresh re-reads a signed-out inactive account's private store without network and reconnects it on a stored credential
- File observation delivers current state after registration and survives directory removal and recreation

## [09]-[PROCESSES]

`ProcessRun` starts every child through `Subprocess.run` in its own session, teardown sends SIGTERM to its group and SIGKILL 2 s later:
- Every child runs under a deadline except `codex app-server`, which puts one on each request
- Stream children are read and written inside the `run` body closure
- `$SHELL -lc` runs once at launch and overlays `LoginShell.variables` on the launch environment, a failed run keeps the launch environment
- `FileLocations` resolves once, after the login shell
- Each account runs one operation at a time
- Quit cancels every operation, waits 5 s at most, then saves accounts

## [10]-[INTERFACE]

Panel and Settings use system controls, semantic colors, and fonts:
- Panel opens Settings by dismissing itself, `openWindow`, then `Activation.bringToFront` through `NSWorkspace.openApplication`
- Sign-in sheet state and task belong to `AccountStore`, closing Settings mid-login cancels nothing
- Login item registers the running bundle through `SMAppService.mainApp`, its status reloads on panel open, pane appearance, and activation
- Provider failures log under subsystem `app.rasm.relay` with the account id public and the email private, failure cases hold no token
