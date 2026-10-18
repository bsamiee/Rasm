# [TASKS]

Each task runs in its own named session from repository root and ends with `close`.

## [01]-[EXPLORATION]

Depth-limited snapshots map a page's regions, `find` and subtree snapshots read one region, refs drive the next action:

```bash
playwright cli -s=<session> open https://playwright.dev
playwright cli -s=<session> snapshot --depth=3
playwright cli -s=<session> snapshot e49 --depth=2
playwright cli -s=<session> find "Get started"
playwright cli -s=<session> click e44
playwright cli -s=<session> tab-list
playwright cli -s=<session> close
```

## [02]-[ACCESSIBILITY]

Snapshots hold the accessibility tree with roles, names, levels, and states:
- `find --regex` lists headings, landmarks, or unnamed controls across the tree
- `press Tab` with an `eval` of `document.activeElement` walks focus order
- `set-forced-colors`, `set-contrast`, and `set-reduced-motion` with a `screenshot` show each media feature's rendering
- `run-code` runs axe-core from `@axe-core/playwright`'s dependency through an init script and a reload, the reload drops page state
- `realpath` resolves the axe source path through pnpm's sibling layout

```bash
playwright cli -s=<session> find --regex "heading .*\[level=[12]\]" --max-results=10
playwright cli -s=<session> press Tab
playwright cli -s=<session> --raw eval "(({ tagName, textContent }) => ({ tagName, text: textContent.trim() }))(document.activeElement)"
playwright cli -s=<session> set-forced-colors active
playwright cli -s=<session> screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>-forced-colors.png
playwright cli -s=<session> clear-forced-colors
realpath node_modules/@axe-core/playwright/../../axe-core/axe.min.js
playwright cli -s=<session> --raw run-code "async page => {
    await page.addInitScript({ path: '<axe>' });
    await page.reload();
    const { violations, passes } = await page.evaluate(() => axe.run());
    return { passes: passes.length, violations: violations.map(v => ({ id: v.id, impact: v.impact, targets: v.nodes.map(n => n.target.join(' ')) })) };
}"
```

## [03]-[PERFORMANCE]

`eval` reads navigation and paint timing from the page's performance timeline, a trace's `requests` lists each request's start, duration, and size:
- Largest contentful paint reaches the page through a buffered `PerformanceObserver` alone, the timer resolves `null` for a page with no entry
- `run-code` reads Chromium's `Performance.getMetrics` (heap, layout, script, task durations, DOM nodes) through a CDP session

```bash
playwright cli -s=<session> --raw eval "(() => { const [n] = performance.getEntriesByType('navigation'); return { ttfb: n.responseStart - n.startTime, domContentLoaded: n.domContentLoadedEventEnd - n.startTime, load: n.loadEventEnd - n.startTime, paints: performance.getEntriesByType('paint').map(p => [p.name, p.startTime]) }; })()"
playwright cli -s=<session> --raw eval "new Promise(resolve => { new PerformanceObserver(list => resolve(list.getEntries().at(-1).startTime)).observe({ type: 'largest-contentful-paint', buffered: true }); setTimeout(() => resolve(null), 1000); })"
playwright cli -s=<session> --raw run-code "async page => {
    const cdp = await page.context().newCDPSession(page);
    await cdp.send('Performance.enable');
    const { metrics } = await cdp.send('Performance.getMetrics');
    await cdp.detach();
    return Object.fromEntries(metrics.map(m => [m.name, m.value]));
}"
```

## [04]-[SCRAPING]

`eval --filename` writes a page value as JSON, `run-code` loops pages, `response-body` reads an API response the page fetched:

```bash
playwright cli -s=<session> eval "[...document.links].map(a => ({ text: a.textContent.trim(), href: a.href }))" --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/links.json
playwright cli -s=<session> requests --filter="/api/"
playwright cli -s=<session> response-body <n> --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>.json
```

## [05]-[VERIFICATION]

Checks read the page through `--raw` values, `find` matches, console errors, and the requests an action sent:

```bash
playwright cli -s=<session> --raw eval "document.title"
playwright cli -s=<session> find --regex "/sign (in|up)/i" --max-results=1
playwright cli -s=<session> console error
playwright cli -s=<session> requests --filter="/api/"
playwright cli -s=<session> screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>.png --full-page
```
