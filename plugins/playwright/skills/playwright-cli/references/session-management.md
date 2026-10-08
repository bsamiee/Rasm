# [SESSION_MANAGEMENT]

Every session launches its own browser with its own cookies, storage, IndexedDB, cache, history, and tabs.

## [01]-[SESSIONS]

Commands without `-s` address session `default`, `PLAYWRIGHT_CLI_SESSION=<name>` renames it:
- Sessions register in `~/Library/Caches/ms-playwright/daemon/<workspace hash>/`, a workspace per directory holding `.playwright/`
- Directories outside a `.playwright/` folder share one workspace, every agent reaches every session
- `open` on an open session restarts it with the new options and drops its pages
- `--config=<file>` on `open` or a `.playwright/cli.config.json` of the current directory replaces `PLAYWRIGHT_MCP_CONFIG`
- `--idle-timeout=<ms>` closes a session after that long without a command, one hour for a headless session without the option, `0` keeps it open
- Commands after the idle shutdown fail with `The browser '<name>' is not open`, `open` starts the session again
- `list` prints each session's status, browser type, user data directory, and headed flag, and unregisters each closed session
- `list --all` adds every workspace's sessions, the browsers `attach <name>` reaches, and each Chrome and Edge channel with a profile
- `close-all` closes every session of the workspace with other agents' sessions, `--json close-all` names them
- `kill-all` kills every CLI daemon and dashboard process on the machine with their browsers, the MCP servers stay
- `<session>.err` in the workspace directory holds the daemon's stderr, a failed `open` prints it

```bash
playwright cli -s=auth open https://example.com/login
playwright cli -s=public open https://example.com
playwright cli -s=auth fill e1 "user@example.com"
playwright cli -s=public snapshot
playwright cli list
playwright cli -s=auth close
playwright cli -s=public close

PLAYWRIGHT_CLI_SESSION=auth playwright cli open https://example.com --idle-timeout=0
PLAYWRIGHT_CLI_SESSION=auth playwright cli close

playwright cli list --all
playwright cli --json close-all
playwright cli kill-all
```

## [02]-[PROFILES]

Sessions hold their profile in memory, `--persistent` keeps it at `~/Library/Caches/ms-playwright/daemon/<workspace hash>/ud-<session>-chromium` and restores it on the next `open --persistent`, `--profile=<dir>` keeps it in that directory. `delete-data` closes the session and removes a `--persistent` profile, a `--profile` directory stays:

```bash
playwright cli -s=auth open https://example.com --persistent
playwright cli -s=auth close
playwright cli -s=auth open https://example.com --persistent
playwright cli -s=auth delete-data
```

## [03]-[ATTACHING]

`attach` connects a session to a running browser in place of launching one and replies with its snapshot:
- `attach <name>` connects to a session or MCP browser `list --all` prints, both sessions drive the same pages
- `--cdp=<url>` connects to a Chrome DevTools Protocol endpoint, its context keeps its own permissions, HTTPS, and CSP settings
- `--cdp=<channel>` connects to a `chrome*` or `msedge*` channel `list --all` prints, remote debugging enabled at its `inspect` page
- `--extension` connects through the Playwright browser extension, `--extension=<channel>` names the browser
- `--session=<name>` or `-s=<name>` names the session, without one it takes the channel or browser name, and `default` for a URL
- `close` and `detach` end an attached session with the browser left running, `detach` refuses a session `open` started
- `--idle-timeout=<ms>` detaches after that long without a command, an attached session stays attached without the option

```bash
playwright cli attach auth --session=auth-view
playwright cli -s=auth-view snapshot
playwright cli -s=auth-view detach

playwright cli attach --cdp=http://localhost:9222 --session=remote
playwright cli -s=remote detach

playwright cli attach --cdp=msedge
playwright cli -s=msedge detach

playwright cli attach --extension
playwright cli detach
```
