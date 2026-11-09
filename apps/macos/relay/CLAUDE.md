# [RELAY]

Relay shows Claude and OpenAI weekly subscription usage in the menu bar and switches each provider's active account.

## [01]-[STATE]

One store owns every trigger and account state change, views call it and provider clients return results to it:
- Views hold presentation state alone, sign-in and login item state outlive the Settings window
- Credential reads and writes reserve each live and private store they touch for the whole operation
- Selection follows each provider's live store, actions re-read selection in place of setting it
- Account identity is account id with organization id, every identity check compares both
- Failures needing a new sign-in mark the account for sign-in and its reading stale, every other failure logs and leaves its card connected
- Accounts marked for sign-in read no usage until their own store holds a matching login that has not expired
- Selected accounts read usage at most every 3 minutes, parked accounts every 30 minutes, and every account at its next window reset
- Logs mark account ids public and emails private, failures hold no token
- Pending and failed `accounts.json` loads start no trigger, a failed load leaves the file unwritten

## [02]-[CLAUDE]

Claude stores are Claude Code config directories, each with an account file and a login Keychain item:
- Selected account uses the shared store Claude Code resolves, every other account a private config directory under `Accounts/<id>/Claude`
- Relay sends no request to Anthropic: `claude auth status --json` reports a store's login, `claude -p "/usage" --output-format stream-json --verbose` its usage, stream JSON in print mode requiring `--verbose`
- Claude Code alone refreshes tokens, since refresh tokens are single use and a second refresher revokes the token family
- `claude auth status` exits 1 for a store with no login and prints email and organization alone, the account file holds the account id
- Print-mode `/usage` starts no Remote Control bridge, so a read creates no claude.ai session
- Usage reads run one `claude` at a time and pass `disableClaudeAiConnectors`, skipping the claude.ai connector fetch
- `/usage` rows are the usage endpoint reply or Claude Code's snapshot under 60 seconds old, and null while the fetch fails
- Print-mode runs fill a missing account entry from Claude Code's profile fetch, `claude auth status` does not, so a switch first reads usage on a store missing one
- Keychain items trust `/usr/bin/security` alone, reads and writes go through the `security` CLI, never Security framework calls
- Security framework writes stamp the item's partition list with Relay's signature, and every later `claude` and Claude Desktop read prompts
- Writes pass `security -i` lines up to 4032 bytes, Claude Code's limit, and longer items as `add-generic-password -X` arguments
- Writes to an item or account file replace Relay's fields and keep the rest (`mcpOAuth` MCP server logins)
- Claude Code signs out by revoking the refresh token and deleting the item, a blanked item reads as signed out
- `claude` children point at one store and drop variables that redirect their credential or API
- Claude Code refreshes under `.oauth_refresh.lock` and `<realpath>.lock` and writes credentials under `.storage-write.lock` in its store, its release re-reads selection
- Claude Code writes `.credentials.json` when a Keychain write fails and reads it only while the item is absent
- While `.credentials.json` exists, running sessions detect outside writes by its mtime alone, Relay's Keychain writes delete it
- Switch holds Claude Code's locks on every store it touches and runs no `claude` child inside
- Switch records its target after reading every store it moves, a failed move and launch finish a recorded switch
- Outside sign-ins replace the shared credential, Relay saves the credential its last selection read into the outgoing private store

## [03]-[CODEX]

Relay drives `codex app-server` over JSON lines on stdio, one server per `CODEX_HOME`:
- `codex` resolves from the login shell's `PATH`, then from the Codex desktop app's bundled `codex-cli`
- Selected account uses the live `CODEX_HOME`, every other account a private home under `Accounts/<id>/Codex`
- Switch requires `config/read` to report `cli_auth_credentials_store` unset or `file` and `forced_chatgpt_workspace_id` unset
- Switch refuses to run while the desktop app runs, otherwise stops each affected server and moves `auth.json`
- Sign-in subscribes to `account/login/completed` before `account/login/start`, so the notification cannot arrive unheard
- Usage reads map rate-limit windows of a week or longer, Relay models no shorter window
- Lines read with no length limit, since a `config/read` reply holds the whole effective configuration and outgrows the 128 KiB Subprocess default

## [04]-[PROCESSES]

Relay spawns one login shell at launch, `claude`, `codex app-server`, and `security` alone:
- Every child except `codex app-server` runs under a timeout, the server puts one on each request
- Later children and file locations take the login shell's exports in place of the launch environment
- Failed login shell reads stop launch and show in the panel and Settings

## [05]-[INTERFACE]

Panel and Settings use system controls, semantic colors, and fonts:
- Windows past their reset show no reading and block nothing until the next read
- Past sign-in expiry shows as the card note, a future one as the card date
