# [RUNNING_CODE]

`run-code` runs one function expression that receives `page`, for page and context operations the CLI commands lack.

Evaluation wraps the function in `(...)` inside a VM context, `import`, `export`, `require`, and `import()` fail, `console` output goes nowhere.

Beside `page` and the JavaScript built-ins, the function sees these globals alone, `process` and `performance` undefined:
- `setTimeout`, `clearTimeout`, `setInterval`, `clearInterval`, `setImmediate`, `clearImmediate`, `queueMicrotask`
- `URL`, `URLSearchParams`, `TextEncoder`, `TextDecoder`, `AbortController`, `AbortSignal`
- `fetch`, `Headers`, `Request`, `Response`, `FormData`, `Blob`, `Buffer`, `crypto`, `atob`, `btoa`, `structuredClone`

`page.getByRef('<ref>')` locates the element a ref of the last `snapshot` names.

Results, errors, and code files take these forms:
- Return values print as JSON under `### Result`, a string in quotes, `undefined` prints no result, `--raw` prints the JSON alone
- Thrown errors exit with code 1 and the Playwright error
- `--filename` loads the function from a file, a relative path resolves against the shell directory

```bash
playwright cli run-code "async page => {
    return { title: await page.title(), url: page.url() };
}"

playwright cli run-code "async page => {
    await page.getByRef('e5').click();
    return page.getByRef('e5').textContent();
}"

playwright cli run-code --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/script.js
```

## [01]-[PERMISSIONS]

Sessions open with every permission at `prompt`, and a page request behind one (geolocation, clipboard) fails as denied:
- `grantPermissions` grants the listed permissions, on `origin` alone when given
- `clearPermissions` returns every permission to `prompt`

```bash
playwright cli run-code "async page => {
    await page.context().grantPermissions(['geolocation']);
    await page.context().setGeolocation({ latitude: 51.5074, longitude: -0.1278 });
}"

playwright cli run-code "async page => {
    await page.context().grantPermissions(['clipboard-read', 'clipboard-write'], { origin: 'https://example.com' });
}"

playwright cli run-code "async page => {
    await page.context().clearPermissions();
}"
```

## [02]-[WAITS]

Action replies wait for the requests an action starts within the 500 ms `timeouts.settle` window and for a navigation's load:
- Later page states take a `run-code` wait

```bash
playwright cli run-code "async page => {
    await page.locator('.loading').waitFor({ state: 'hidden' });
}"

playwright cli run-code "async page => {
    await page.waitForFunction(() => window.appReady === true);
}"

playwright cli run-code "async page => {
    await page.locator('.result').waitFor({ timeout: 10000 });
}"

playwright cli run-code "async page => {
    await page.waitForURL('**/dashboard');
}"
```

## [03]-[FRAMES]

```bash
playwright cli run-code "async page => {
    return page.frames().map(f => f.url());
}"
```

## [04]-[API_REQUESTS]

`page.request` sends requests with the context's cookies and returns the response outside the page.

```bash
playwright cli run-code "async page => {
    const response = await page.request.get('https://example.com/api/users');
    return { status: response.status(), body: await response.json() };
}"
```

## [05]-[FLOWS]

```bash
playwright cli run-code "async page => {
    const results = [];
    for (let i = 1; i <= 3; i++) {
        await page.goto(\`https://example.com/page/\${i}\`);
        results.push(...await page.locator('.item').allTextContents());
    }
    return results;
}"
```
