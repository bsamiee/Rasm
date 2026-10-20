# [RELAY]

Relay shows Claude and OpenAI subscription usage in the menu bar, switches each provider's active account, and starts 5-hour sessions

## [01]-[STATE]

`AccountStore` owns every trigger and account state change, views call it and provider clients return results to it:
- Views hold presentation state alone, sign-in and login item state belong to `AccountStore` and outlive the Settings window
- Credential reads and writes reserve each live and private store they touch through `withStores`
- `isSelected` follows each provider's live store through `readSelection`, actions re-read selection in place of setting it
- `AccountIdentity.isSameAccount` compares account id and organization id, every identity check goes through it
- Failure cases set `requiresSignIn` when an account must sign in again, every other failure leaves an issue on a connected card
- Logs mark account ids public and emails private, failure cases hold no token
- Failed `accounts.json` loads leave the file unwritten and start no trigger

## [02]-[CLAUDE]

Claude stores are Claude Code config directories, each with an account file and a login Keychain item:
- Selected account uses the shared store Claude Code resolves, every other account a private config directory under `Accounts/<id>/Claude`
- Keychain items trust `/usr/bin/security` alone, Relay reads and writes them through the `security` CLI
- Writes to an item or account file replace Relay's fields and keep the rest (`mcpOAuth` MCP server logins)
- Claude Code signs out by blanking `accessToken` and `refreshToken` with `expiresAt` 0, Relay reads and writes the blanked item as signed out
- `processEnvironment` points every `claude` child at one store and drops variables that redirect its credential or API
- Refresh tokens are single use, a reused one revokes its token family and signs out every holder
- Refresh holds Claude Code's lock pair on its store, re-reads it, and adopts any token another process wrote
- Switch holds the lock pair on every store it touches and runs no `claude` child or HTTP request inside
- Switch records `claude-selection.json` before moving credentials, launch finishes a switch the file still records
- Outside sign-ins replace the shared credential, Relay saves the credential its last selection read into the outgoing private store
- Token endpoint returns 429 to User-Agents other than Claude Code's and axios's, Relay sends the installed `claude` version's
- `/api/oauth/usage` allows one read per account every 3 to 5 minutes, `usageReadAfter` holds each account's next permitted read

## [03]-[CODEX]

Relay drives the Codex desktop app's bundled `codex app-server` over JSON lines on stdio, one server per `CODEX_HOME`:
- Selected account uses the live `CODEX_HOME`, every other account a private home under `Accounts/<id>/Codex`
- Switch requires `config/read` to report `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset
- Switch stops each affected server, moves `auth.json`, then quits and reopens a running desktop app
- Notifications that can arrive before their wait starts join `retainedNotifications`

## [04]-[SESSIONS]

Session start sends one greeting when `AccountUsage.availability` is `ready`:
- Greetings run with tools, MCP servers, hooks, and memory off
- Claude greetings read access tokens from a pipe `CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR` names, outside argv and environment
- Claude session resets come from greeting's `rate_limit_event` when the next usage read reports none

## [05]-[PROCESSES]

Relay children are one login shell at launch, `claude`, `codex app-server`, and `security`:
- Every child except `codex app-server` runs under a deadline, the server puts one on each request
- Later children and `FileLocations` take login shell exports, `open` from a shell changes the launch environment

## [06]-[INTERFACE]

Panel and Settings use system controls, semantic colors, and fonts.

## [07]-[INSTALL]

`nx run Relay:install` replaces `/Applications/Relay.app`, a running Relay keeps its old build until it quits.
