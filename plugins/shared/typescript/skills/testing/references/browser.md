# [BROWSER]

Suites in a browser: Vitest browser mode over components, Storybook stories as tests, Playwright over built products, extensions, and store media, and WebDriver over Firefox.

## [01]-[VITEST_BROWSER]

Root `vitest.config.ts` gives each browser project one `browser(name)` option set:

| [INDEX] | [OPTION]                | [VALUE]                                        | [EFFECT]                                            |
| :-----: | :---------------------- | :--------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `provider`              | `playwright()` of `@vitest/browser-playwright` | Chromium driven through Playwright                  |
|  [02]   | `headless`, `instances` | `true`, `[{ browser: 'chromium' }]`            | One headless Chromium per project                   |
|  [03]   | `locators.errorFormat`  | `'aria'`                                       | Missing-element errors print the accessibility tree |
|  [04]   | `screenshotDirectory`   | `.artifacts/vitest/<name>/screenshots`         | Screenshots of failing tests                        |
|  [05]   | `trace`                 | `{ mode: 'retain-on-failure', tracesDir }`     | Trace of each failing test                          |

Suites listing `vitest-browser-react` take plugin `react` of `@rasm/testing/storybook`, React Compiler at `panicThreshold: 'all_errors'`, so a component the compiler refuses fails its suite:

```tsx
it('confirms through an alert dialog titled by its label, focusing cancel first', async () => {
    const onConfirm = vi.fn<() => void>();
    const screen = await render(<ModalDialog.Confirm cancel="Keep" description="Deletion is permanent" label="Delete item" onConfirm={onConfirm} />);
    await screen.getByRole('button', { name: 'Delete item' }).click();
    const alert = screen.getByRole('alertdialog', { name: 'Delete item' });
    await expect.element(alert).toHaveAccessibleDescription('Deletion is permanent');
    await expect.element(alert.getByRole('button', { name: 'Keep' })).toHaveFocus();
});
```

- `render` of `vitest-browser-react` returns a promise of its result, awaited before the first locator
- Locators take a role and accessible name (`getByRole(role, { name })`) and chain inside a container (`alert.getByRole(...)`), as a role locator fails when a control loses its name
- Matchers read accessibility state: `toHaveAccessibleDescription`, `toHaveFocus`, `toBeDisabled`, `toHaveTextContent`
- `expect.element(locator).not.toBeInTheDocument()` retries until the element leaves, the form for a dialog that closes
- `userEvent` of `vitest/browser` sends input through Playwright: `tab()`, `keyboard('{ControlOrMeta>}a{/ControlOrMeta}')`, `dragAndDrop(source, target, { targetPosition })`
- `locator.element()` returns one match for synchronous reads (`getBoundingClientRect`), and `expect.poll` over `locator.elements()` asserts an order
- Callbacks are `vi.fn<Signature>()` spies asserted after the act (`toHaveBeenCalledOnce`)
- Controlled components render inside a test component holding `useState` (`Sheets` in `reorderable-list.test.tsx`), so a test reads what `onChange` produced

## [02]-[STORYBOOK]

Packages listing `@storybook/addon-vitest` keep stories beside their components (`libs/typescript/web/ui/*.stories.tsx`), and `storybookTest({ configDir })` runs each story as a test:
- `.storybook/main.ts` spreads `main` of `@rasm/testing/storybook` into `defineMain` with `framework` and `stories`, and `main` adds the vitest, a11y, and docs addons, turns telemetry off, and merges `react`
- `.storybook/preview.ts` spreads `preview` of `@rasm/testing/storybook-preview` into `definePreview` with `addons: [addonA11y(), addonDocs()]`, and extends `parameters` by spread (`layout: 'fullscreen'` in portfolio)
- `preview` sets `a11y.test: 'error'` over `runOnly` tags `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa`, `wcag22aa`, and `best-practice`, so an axe violation fails its story
- Violations are fixed in the component, as `a11y.test` at `'todo'` or `'off'` and `a11y.disable` turn a story green over a defect
- Stories are CSF factories: `preview.meta({ component, args })` and `meta.story({ args, render, play })`, with `preview` imported from `#/.storybook/preview`
- `play` receives `canvas`, `userEvent`, and `args`, and `storybook/test` supplies `expect`, `waitFor`, `within`, and `fn`

Each kind of case takes the form that observes it:

| [INDEX] | [CASE]                                       | [FORM]                                    |
| :-----: | :------------------------------------------- | :---------------------------------------- |
|  [01]   | State a reader sees (open, empty, signed in) | Story with `args`, scanned without `play` |
|  [02]   | Interaction inside one state                 | Story `play`                              |
|  [03]   | Row tables, callback spies, pointer geometry | `*.test.tsx` under `vitest-browser-react` |

## [03]-[PLAYWRIGHT_CONFIG]

`playwright.config.ts` exports `config(import.meta.dirname)` of `@rasm/testing/playwright`, which reads its suite's package name and the `@rasm/nx-playwright` `outputPath` row of `nx.json`:

| [INDEX] | [PROJECT] | [MATCHES]           | [USE]                                                                                      |
| :-----: | :-------- | :------------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `test`    | Default `testMatch` | Trace `retain-on-failure`, screenshot `only-on-failure`, html reporter unopened            |
|  [02]   | `media`   | `**/*.media.ts`     | `outputDir` `<outputPath>/media`, scale 2, light, `en-US`, `UTC`, trace and screenshot off |

- Suite settings merge through `defineConfig(config(import.meta.dirname), { use })` (`contextOptions.permissions: ['clipboard-read']` in mezrab)
- `test--playwright` runs projects with an `outputDir` outside `outputPath` under `--forbid-only` and `--output=<outputPath>/test-results`, after `^build--vite`, codegen, and each reused `webServer` task
- `test--playwright` writes html, blob, and perfetto reports under `outputPath` through `PLAYWRIGHT_<REPORTER>_OUTPUT_DIR`, over folders a config states
- `pack--playwright` runs projects with an `outputDir` under `outputPath` with `--forbid-only --no-deps --reporter=list` after `test--playwright`, and outputs each `outputDir`, which Playwright empties first, so `attach--gh` uploads no earlier file
- Mise `[deps.playwright]` installs Chromium for the locked Playwright before `mise exec`, for Vitest browser projects and Playwright suites alike
- Suites name their product as a `workspace:*` dev dependency or in `nx.implicitDependencies`, so `^build--vite` builds it before a run

## [04]-[FIXTURES]

Fixtures take the object form of `test.extend<TestFixtures, WorkerFixtures>({ ... })`, and Effect resources enter through `resource(scoped)`:

```ts
const site = test.extend<{ readonly owner: Page }, { readonly site: Site }>({
    site: [resource(Effect.provide(served, platform)), { scope: 'worker' }],
    baseURL: ({ site: { origin } }, use) => use(origin),
});

const blank = site.extend<{ readonly blank: void }>({ blank: [({ site: { reset } }, use) => reset().then(use), { auto: true }] });
```

`served` and `platform` are the scoped effect and layer of `tests/e2e/portfolio/served.ts`, and `Site` names the `{ origin, reset }` handle `served` returns:
- `resource` runs its effect in a scope, hands the value to `use`, and closes the scope at fixture teardown, releasing every acquisition there
- New worker fixtures take the tuple `[fn, { scope: 'worker' }]`, and `auto: true` runs a new fixture for every test or worker, whether or not one names it
- Overrides of built-ins (`baseURL`, `context`) are plain functions, as `Fixtures` refuses `auto` and `option` on a key Playwright declares
- `baseURL` derived from a worker fixture points `page.goto('/')` and `request` at the served product
- Resets are `auto` fixtures: `blank` empties state before each test, and `shown` publishes content once per worker
- `served` acquires a temporary folder, a Miniflare R2 bucket on its S3 route, a free port, and the product's process, racing readiness against its exit
- Suites sign in through an `ACCESS` source `Forwarded`, as the headers of a request state its user
- Fixture contexts acquire through `Effect.acquireDisposable(Effect.promise(() => browser.newContext(...)))`, and contexts a test opens take `await using`, as `BrowserContext` implements `Symbol.asyncDispose`
- `mergeTests(it, blank)` joins fixture sets of separate modules into one test function
- Variants extend one set per variant in place of a branch: `rendering.spec.ts` maps each viewer to a `shown.extend` binding `viewed` to `page` or `owner`

## [05]-[ACCESSIBILITY]

`it` of `@rasm/testing/playwright` adds fixture `axe`, an `AxeBuilder` over `page` at tags `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa`, and `wcag22aa`:

```ts
it('serves the published site with its landmarks and no WCAG A or AA violation', async ({ page, axe, site }) => {
    await site.publish(content);
    await page.goto('/');
    await expect(page.getByRole('main').getByRole('heading', { level: 1 })).toBeVisible();
    const { violations } = await axe.analyze();
    expect(violations.map(({ id, nodes }) => ({ id, targets: nodes.map(({ target }) => target) }))).toEqual([]);
});
```

- Scans follow a wait on rendered landmarks, as a scan of a page still loading passes over content it never saw
- Violations map to rule `id` and `targets` compared with `[]`, so a failure prints each rule and its selectors
- Playwright scans omit `best-practice`, which Storybook scans include

## [06]-[EXTENSIONS]

`loaded` of `@rasm/testing/playwright` loads a suite's built extension into a persistent Chromium context:

| [INDEX] | [FIXTURE]   | [SCOPE] | [HOLDS]                                                                                                  |
| :-----: | :---------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `built`     | Worker  | Folder and root of the one dependency whose build outputs hold a Manifest V3 `manifest.json`             |
|  [02]   | `launched`  | Worker  | `launchPersistentContext` at `channel: 'chromium'` with extension switches and project `use`             |
|  [03]   | `context`   | Test    | `Extensions.loadUnpacked` at acquisition, `Extensions.uninstall`, unrouting, and page closing at release |
|  [04]   | `extension` | Test    | `worker`, `act`, `toolbar`, `stop`, and `evaluate` once the service worker is `activated`                |

- `launched` drops Playwright's `--disable-extensions` through `ignoreDefaultArgs` and adds `--enable-unsafe-extension-debugging` to `launchOptions.args`
- `extension` waits for an `activated` service worker, as Chrome drops an action pressed before the worker registers listeners, and fails a test whose worker turns `redundant`
- `launched` passes `viewport`, `deviceScaleFactor`, `colorScheme`, `locale`, `timezoneId`, and `contextOptions` of project `use` to the launch, as `loaded` replaces built-in `context`
- Suites extend `loaded` in `fixtures.ts` with routed pages, stubs, and settings writes, each released in its fixture
- Use `chrome:extensions` for extension suites: CDP methods, switches, routed pages, stubs, and oracles

## [07]-[MEDIA]

`media(scenes)` in a `*.media.ts` registers one `loaded` test per scene and per promo tile, each writing a PNG to project `media`'s `outputDir` under `nx pack <suite>`:

| [INDEX] | [FIELD]       | [USE]                                                          |
| :-----: | :------------ | :------------------------------------------------------------- |
|  [01]   | `name`        | File `screenshot-<n>-<name>.png`                               |
|  [02]   | `url`, `body` | Page address, `body` routed there as HTML when given           |
|  [03]   | `colorScheme` | `page.emulateMedia` for one scene                              |
|  [04]   | `shows`       | Role the extension action reveals, pressed and awaited visible |
|  [05]   | `after`       | Step over `page`, `extension`, and `open` before capture       |

- Viewports take 1280 by 800 divided by `deviceScaleFactor`, and `screenshot({ scale: 'device' })` writes 1280 by 800 pixels
- Captures wait for transitions of the `shows` element to end, pause its animations, then wait for fonts and images
- Tiles `small-promo-tile` (440 by 280) and `marquee-promo-tile` (1400 by 560) draw the built manifest's name, resolving `__MSG_<key>__` through `default_locale`, over `config.brand` with `icons/mark.svg` of the product root
- `hosted(import.meta.resolve('<package>'))` serves a local file at `https://files.test/<path>`, the form scene bodies load fonts through
- `attach--gh` of a host package depends on `pack--*` of each workspace package depending on it and uploads the visible files they write

## [08]-[WEBDRIVER]

`selenium-webdriver` drives a stable Firefox, as Playwright and Vitest browser providers launch a Firefox build that differs from it in the CSS it supports:

```ts
const drawn = Effect.fnUntraced(function* ({ viewer, path, width, read, expected }: (typeof cases)[number]) {
    const [site, { driver, viewed }] = yield* Effect.all([Site, Session]);
    yield* Effect.promise(() => driver.get('about:blank'));
    yield* Effect.promise(() => viewed.setViewport(width, HEIGHT));
    yield* Effect.promise(() => driver.get(new URL(path, site[viewer]).href));
    const seen = yield* Effect.promise(() => driver.executeScript(read)).pipe(Effect.repeat({ until: (value) => isDeepStrictEqual(value, expected), schedule: Schedule.upTo(Schedule.spaced('50 millis'), { duration: '10 seconds' }) }));
    assert.deepStrictEqual(seen, expected);
});
```

`rendering.test.ts` of `tests/e2e/portfolio-webdriver` runs `drawn` per row, `Site` mapping each viewer to an origin and `Session` holding the driver and its BiDi browsing context:
- Suites are Vitest `node` projects stating tag `host:browser` in their manifest, with `test--vitest` depending on `build--vite` of the product they load
- `new Builder().disableEnvironmentOverrides()` over `firefox.Options` with `-headless` and `enableBidi()` starts one session per `layer` block, quit at its release
- Selenium Manager finds an installed Firefox and `geckodriver`, and downloads the stable release of each a machine lacks
- Rows are `cases` the Chromium suite exports from `tests/e2e/portfolio/rendering.ts` (viewer, path, width, in-page `read`, `expected`), filtered to rows holding `gecko: true`, which Gecko can render differently
- Viewports take BiDi `browsingContext.setViewport` at the case's width by 900, as a Firefox window stops at 500px wide
- Loads through `about:blank` start each case in a new document, and `executeScript(read)` repeats until it equals `expected`
- Owner rows load through a proxy on port 0 that sets the owner's `Forwarded` headers, as WebDriver sets no request header
