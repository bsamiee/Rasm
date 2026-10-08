# [STORAGE_STATE]

Storage commands act on the context of the current tab, reads print plain lines and writes print the Playwright code they ran. State files hold cookies and per-origin storage between sessions.

## [01]-[STATE]

`state-save` writes cookies and each origin's localStorage as JSON, `state-load` replaces cookies and every origin's localStorage, IndexedDB, and OPFS of the open session with a file's contents:
- `state-save` without a path writes `$PLAYWRIGHT_MCP_OUTPUT_DIR/storage-state-<timestamp>.json`, a relative path resolves against the shell directory
- `state-load` needs an open session, the page keeps its URL and sessionStorage and reads restored values without a reload
- `PLAYWRIGHT_MCP_STORAGE_STATE=<file>` on `open` starts an in-memory session from a state file
- `run-code` with `storageState({ indexedDB: true, opfs: true, credentials: true })` adds IndexedDB, OPFS files, and virtual WebAuthn passkeys

```bash
playwright cli open https://example.com/login
playwright cli snapshot
playwright cli fill e1 "user@example.com"
playwright cli fill e2 "password123"
playwright cli click e3
playwright cli state-save $PLAYWRIGHT_MCP_OUTPUT_DIR/auth.json
playwright cli run-code "async page => {
    await page.context().storageState({ path: '$PLAYWRIGHT_MCP_OUTPUT_DIR/auth-full.json', indexedDB: true, opfs: true, credentials: true });
}"
playwright cli close

PLAYWRIGHT_MCP_STORAGE_STATE=$PLAYWRIGHT_MCP_OUTPUT_DIR/auth.json playwright cli open https://example.com/dashboard
playwright cli state-load $PLAYWRIGHT_MCP_OUTPUT_DIR/auth-full.json
playwright cli close
```

Saved file, a session cookie with `expires: -1`, and `indexedDB`, `opfs`, and `credentials` from the `run-code` form:

```json
{
    "cookies": [
        {
            "name": "session",
            "value": "abc123",
            "domain": "example.com",
            "path": "/",
            "expires": -1,
            "httpOnly": true,
            "secure": true,
            "sameSite": "Lax"
        }
    ],
    "origins": [
        {
            "origin": "https://example.com",
            "localStorage": [{ "name": "theme", "value": "dark" }],
            "indexedDB": [
                {
                    "name": "shop",
                    "version": 1,
                    "stores": [
                        {
                            "name": "items",
                            "autoIncrement": false,
                            "keyPath": "id",
                            "records": [{ "value": { "id": 1, "name": "cart" } }],
                            "indexes": []
                        }
                    ]
                }
            ],
            "opfs": [{ "path": "draft.txt", "type": "file", "base64": "aGVsbG8=" }]
        }
    ],
    "credentials": []
}
```

## [02]-[COOKIES]

`cookie-list` and `cookie-get` print `name=value` lines with domain and path, `cookie-set` takes a name and value:
- `--domain` defaults to the current page's host and `--path` to `/`, a blank page needs `--domain`
- `--expires` takes Unix seconds, Chromium caps it at 400 days ahead, a cookie without it ends with the session
- `--httpOnly` and `--secure` set their flags, `--sameSite` takes `Strict`, `Lax`, or `None`, and `Lax` applies without it
- `cookie-list --domain` keeps domains containing the text, `--path` paths starting with it
- `cookie-get` prints the first cookie of that name with its flags, `cookie-delete` removes the name from every domain
- `run-code` with `clearCookies({ domain })` clears one domain

```bash
playwright cli cookie-set session abc123
playwright cli cookie-set session abc123 --domain=example.com --path=/ --expires=1893456000 --httpOnly --secure --sameSite=Strict
playwright cli cookie-list
playwright cli cookie-list --domain=example.com --path=/api
playwright cli cookie-get session
playwright cli cookie-delete session
playwright cli cookie-clear

playwright cli run-code "async page => page.context().clearCookies({ domain: 'tracker.example.com' })"
```

## [03]-[WEB_STORAGE]

`localstorage-*` and `sessionstorage-*` commands act on the current tab's origin, a blank page denies them access:
- `localstorage-set` and `sessionstorage-set` store a string, a JSON value goes in single quotes
- `-list` prints `name=value` lines, `-get` one line, a missing key prints `<storage> key '<key>' not found`
- `-delete` removes one key, `-clear` every key of the origin

```bash
playwright cli localstorage-set settings '{"theme":"dark","language":"en"}'
playwright cli localstorage-list
playwright cli localstorage-get settings
playwright cli localstorage-delete settings
playwright cli localstorage-clear

playwright cli sessionstorage-set step 3
playwright cli sessionstorage-list
playwright cli sessionstorage-get step
playwright cli sessionstorage-delete step
playwright cli sessionstorage-clear
```

## [04]-[INDEXED_DB]

`run-code` reads every origin's IndexedDB stores and records through `storageState`, `eval` lists the current origin's databases and deletes one. A database the page holds open rejects the delete as blocked, and the delete completes once the page navigates away:

```bash
playwright cli --raw run-code "async page => {
    const { origins } = await page.context().storageState({ indexedDB: true });
    return origins.map(({ origin, indexedDB }) => ({ origin, indexedDB }));
}"

playwright cli --raw eval "indexedDB.databases()"

playwright cli --raw eval "new Promise((resolve, reject) => {
    const request = indexedDB.deleteDatabase('<database>');
    request.onsuccess = () => resolve('<database>');
    request.onerror = () => reject(request.error);
    request.onblocked = () => reject(new Error('<database> is open in a page'));
})"
```
