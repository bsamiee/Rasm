# [RELAY]

Relay shows Claude and OpenAI weekly subscription usage in the menu bar and switches each provider's active account.

## [01]-[STATE]

One store owns every trigger and account state change, views call it and provider clients return results to it:
- Views hold presentation state alone, sign-in and login item state outlive the Settings window
- Credential reads and writes reserve each live and private store they touch for the whole operation
- Selection follows each provider's live store, actions re-read selection in place of setting it
- Account identity is account id with organization id, every identity check compares both
- Failures needing a new sign-in mark the account for sign-in, every other failure logs and leaves its card connected
- Accounts marked for sign-in read no usage until their own store holds a matching credential that refreshes or has not expired
- Logs mark account ids public and emails private, failures hold no token
- Pending and failed `accounts.json` loads start no trigger, a failed load leaves the file unwritten

## [02]-[CLAUDE]

Claude stores are Claude Code config directories, each with an account file and a login Keychain item:
- Selected account uses the shared store Claude Code resolves, every other account a private config directory under `Accounts/<id>/Claude`
- Keychain items trust `/usr/bin/security` alone, reads and writes go through the `security` CLI
- Writes to an item or account file replace Relay's fields and keep the rest (`mcpOAuth` MCP server logins)
- Claude Code signs out by revoking the refresh token and deleting the item, a rejected refresh blanks `accessToken` and `refreshToken` with `expiresAt` 0, a blanked item reads as signed out
- `claude` children point at one store and drop variables that redirect their credential or API
- Refresh tokens are single use, a reused one revokes its token family and signs out every holder
- Claude Code refreshes under `.oauth_refresh.lock` and `<realpath>.lock` and writes credentials under `.storage-write.lock` in its store
- Claude Code writes `.credentials.json` when a Keychain write fails and reads it only while the item is absent, while it exists running sessions detect outside writes by its mtime alone, Relay's Keychain writes delete it
- Refresh holds the three locks on its store, re-reads it, and adopts any token another process wrote
- Switch holds the three locks on every store it touches and runs no `claude` child or HTTP request inside
- Switch records its target after reading every store it moves, a failed move and launch finish a recorded switch
- Outside sign-ins replace the shared credential, Relay saves the credential its last selection read into the outgoing private store
- Token endpoint returns 429 to User-Agents other than Claude Code's and axios's, requests send the installed `claude` version's
- `/api/oauth/usage` allows one read per account every 3 to 5 minutes, each account holds its next permitted read time

## [03]-[CODEX]

Relay drives the Codex desktop app's bundled `codex app-server` over JSON lines on stdio, one server per `CODEX_HOME`:
- Selected account uses the live `CODEX_HOME`, every other account a private home under `Accounts/<id>/Codex`
- Switch requires `config/read` to report `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset
- Switch refuses to run while the desktop app runs, otherwise stops each affected server and moves `auth.json`
- Notifications that can arrive before their wait starts are retained until a wait reads them
- Usage reads map rate-limit windows of a week or longer, Relay models no shorter window

## [04]-[PROCESSES]

Relay spawns one login shell at launch, `claude`, `codex app-server`, and `security` alone:
- Every child except `codex app-server` runs under a timeout, the server puts one on each request
- Later children and file locations take the login shell's exports in place of the launch environment
- Failed login shell reads stop launch and show in the panel and Settings

## [05]-[INTERFACE]

Panel and Settings use system controls, semantic colors, and fonts.
