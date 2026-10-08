# [REQUEST_MOCKING]

Routes mock, modify, and fail requests of the session, `requests` and its part commands read what each tab sent and received.

## [01]-[ROUTES]

Routes apply to every context of the session, `--isolated-context` tabs included, and hold across navigations until `unroute`:
- `route <pattern> --status=<code> --body=<text>` fulfills matching requests without the server, `--content-type` types the mock
- Mocks answer 200 without `--status` and send no type without `--content-type`
- `route <pattern> --header="Name: value" --remove-header=<name>,<name>` sends matching requests on with changed headers, `--header` repeats
- `--header` and `--remove-header` apply when neither `--status` nor `--body` is given
- Headers `cookie`, `host`, and `content-length` keep their browser value
- Newest matching route handles a request, a `**/*` header route registered after a mock sends the mocked URL to the server
- `route-list` numbers active routes with their options, `unroute [pattern]` removes the routes of one pattern or every route
- Globs match the whole URL with its query string

```bash
playwright cli route "**/*.jpg" --status=404
playwright cli route "**/api/users" --body='[{"id":1,"name":"Alice"}]' --content-type=application/json
playwright cli route "**/api/**" --header="X-Debug: 1" --remove-header=authorization
playwright cli route-list
playwright cli unroute "**/*.jpg"
playwright cli unroute
```

| [INDEX] | [PATTERN]             | [MATCHES]        |
| :-----: | :-------------------- | :--------------- |
|  [01]   | `**/api/users`        | Exact path       |
|  [02]   | `**/api/*/details`    | One path segment |
|  [03]   | `**/*.{png,jpg,jpeg}` | File extensions  |
|  [04]   | `**/search?q=*`       | Query parameters |

## [02]-[NETWORK_STATE]

`network-state-set offline` disconnects the current tab's context alone, `online` reconnects it:
- `goto` exits 1 with `net::ERR_INTERNET_DISCONNECTED` and leaves the tab on `chrome-error://chromewebdata/`
- Fetches list in `requests` as `[FAILED] net::ERR_INTERNET_DISCONNECTED`
- Tabs of another context stay online, `tab-select` then `network-state-set` disconnects that context

```bash
playwright cli network-state-set offline
playwright cli network-state-set online
```

## [03]-[READING]

`requests` numbers the current tab's requests since its last `goto` and hides successful ones other than fetch and XHR:
- `--static` shows them, `--filter=<regex>` keeps matching URLs, `--clear` empties the list
- Numbers stay fixed across `--static` and `--filter`, `reload` and link navigations append, `goto` and `--clear` restart them at 1
- `--filter` matches after static requests are hidden, a static URL needs `--static` beside it
- Lines without `=> [<status>]` are pending, actions wait up to 5 seconds for the requests they start
- `request <n>` prints status, duration, type, MIME type, and both header sets, `--filename` saves it
- `request-headers`, `request-body`, `response-headers`, and `response-body` print one part as its value alone, `--filename` saves it
- Part commands print nothing for an absent part (a GET request body, a 204 response body, a body from before the last reload or navigation)
- `request-headers` lists headers the page set after route changes, adds `cookie` while any route is active, and omits `host`, `accept-encoding`, and `sec-fetch-*`
- `response-body` inlines text, JSON, XML, JavaScript, and SVG and saves other types as `response-<timestamp>.<subtype>`, `jpg` for JPEG and `bin` without a type, printing the path

```bash
playwright cli requests
playwright cli requests --static --filter="/api/.*users"
playwright cli request 3
playwright cli request-headers 3
playwright cli request-body 3
playwright cli response-headers 3
playwright cli response-body 3 | jq '.[0].name'
playwright cli response-body 3 --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/users.json
playwright cli requests --clear
```

## [04]-[HANDLERS]

`run-code` registers a page route for a conditional response, a modified real response, a failure, or a delay:
- Page routes outrank `route` commands, stay out of `route-list` and `unroute`, and hold across navigations until `page.unrouteAll()`
- `route.abort(<code>)` takes a lowercase Chromium net error name, `requests` prints `internetdisconnected` as `[FAILED] net::ERR_INTERNET_DISCONNECTED`

```bash
playwright cli run-code "async page => {
    await page.route('**/api/login', route => {
        const body = route.request().postDataJSON();
        return body.username === 'admin'
            ? route.fulfill({ json: { token: 'mock-token' } })
            : route.fulfill({ status: 401, json: { error: 'Invalid' } });
    });
}"

playwright cli run-code "async page => {
    await page.route('**/api/user', async route => {
        const response = await route.fetch();
        const json = await response.json();
        json.isPremium = true;
        await route.fulfill({ response, json });
    });
}"

playwright cli run-code "async page => {
    await page.route('**/api/offline', route => route.abort('internetdisconnected'));
}"

playwright cli run-code "async page => {
    await page.route('**/api/slow', async route => {
        await new Promise(r => setTimeout(r, 3000));
        await route.fulfill({ json: { data: 'loaded' } });
    });
}"

playwright cli run-code "async page => page.unrouteAll()"
```
