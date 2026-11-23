import { existsSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { AxeBuilder } from '@axe-core/playwright';
import { layer } from '@effect/platform-node/NodeServices';
import { getOutputsForTargetAndConfiguration, readCachedProjectGraph, readJsonFile, workspaceRoot } from '@nx/devkit';
import { interpolate, isCI, readNxJsonFromDisk } from '@nx/devkit/internal';
import { type BrowserContext, chromium, defineConfig, expect, type Page, test, type TestInfo, type Worker } from '@playwright/test';
import { Brand } from '@rasm/vite/chrome';
import { Array, Cause, Effect, FileSystem, Function, Option, Predicate, Record, Schema, type Scope, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Extension {
    readonly worker: Worker;
    readonly act: (page: Page) => Promise<void>;
    readonly toolbar: (page: Page) => Promise<{ readonly badge: string; readonly title: string }>;
    readonly stop: (page: Page) => Promise<void>;
    readonly evaluate: (page: Page, expression: string) => Promise<unknown>;
}

interface Scene {
    readonly name: string;
    readonly url: string;
    readonly body?: string;
    readonly colorScheme?: 'light' | 'dark';
    readonly shows?: Parameters<Page['getByRole']>[0];
    readonly after?: (stage: { readonly page: Page; readonly extension: Extension; readonly open: (url: string, body: string) => Promise<void> }) => Promise<void>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = 'https://files.test';
const SCREENSHOT = { width: 1280, height: 800 } as const;
const TILES = [
    { name: 'small-promo-tile', width: 440, height: 280, axis: 'column', gap: 24, mark: 120, type: 72 },
    { name: 'marquee-promo-tile', width: 1400, height: 560, axis: 'row', gap: 56, mark: 280, type: 168 },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Routed = Schema.Struct({ plugin: Schema.String, options: Schema.Struct({ outputPath: Schema.String }) }).annotate({
    description: 'An nx.json plugin row whose outputPath names the folder each project of the plugin writes to, {projectName} naming the project',
});
const Named = Schema.Struct({ name: Schema.String }).annotate({ description: 'The name of a package.json, which Nx takes as its project name' });
const Unpacked = Schema.fromJsonString(Schema.declare((input: unknown): input is chrome.runtime.ManifestV3 => Predicate.hasProperty(input, 'manifest_version') && input.manifest_version === 3)).annotate({
    description: 'The manifest.json at the root of an unpacked Manifest V3 extension, the folder Chrome loads through --load-extension',
});
const Messages = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ message: Schema.String }))).annotate({ description: 'The _locales/<locale>/messages.json an extension names its __MSG_<key>__ strings from' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const routed = (plugin: string, projectName: string) =>
    Option.map(
        Array.findFirst(readNxJsonFromDisk().plugins ?? [], (row) => Option.filter(Schema.decodeUnknownOption(Routed)(row), (decoded) => decoded.plugin === plugin)),
        ({ options }) => join(workspaceRoot, interpolate(options.outputPath, { projectName })),
    );

const focused = (windowId: number) => browser.tabs.query({ active: true, windowId }).then(([tab]) => tab?.id ?? browser.tabs.TAB_ID_NONE);

const badged = (tabId: number) => browser.action.getBadgeText({ tabId });

const titled = (tabId: number) => browser.action.getTitle({ tabId });

const attached = Effect.fnUntraced(function* (context: BrowserContext) {
    const worker = yield* Option.match(Array.head(context.serviceWorkers()), { onSome: Effect.succeed, onNone: () => Effect.promise(() => context.waitForEvent('serviceworker')) });
    yield* Effect.promise(() =>
        worker.evaluate(() => {
            const scope: object = globalThis;
            const { promise, resolve, reject } = Promise.withResolvers<void>();
            if ('serviceWorker' in scope && scope.serviceWorker instanceof ServiceWorker) {
                const held = scope.serviceWorker;
                const settle = () => {
                    if (held.state === 'activated') {
                        resolve();
                    } else if (held.state === 'redundant') {
                        reject(new TypeError('The service worker is redundant and never activates'));
                    }
                };
                held.addEventListener('statechange', settle);
                settle();
            } else {
                reject(new TypeError('No service worker runs this script'));
            }
            return promise;
        }),
    );
    const id = new URL(worker.url()).host;
    const browser = yield* Effect.flatMap(Effect.fromNullishOr(context.browser()), (held) => Effect.promise(() => held.newBrowserCDPSession()));
    const targets = (type: 'tab' | 'service_worker') =>
        Effect.map(
            Effect.promise(() => browser.send('Target.getTargets', { filter: [{ type }] })),
            ({ targetInfos }) => targetInfos,
        );
    const inspected = (page: Page) =>
        Effect.tap(
            Effect.promise(() => context.newCDPSession(page)),
            (session) => Effect.promise(() => session.send('ServiceWorker.enable')),
        );
    const tabbed = Effect.fnUntraced(function* (page: Page) {
        const session = yield* inspected(page);
        const { targetInfo } = yield* Effect.promise(() => session.send('Target.getTargetInfo'));
        const tabs = Array.filter(yield* targets('tab'), ({ url }) => url === targetInfo.url);
        return yield* Effect.fromOption(
            Option.filter(Array.head(tabs), () => tabs.length === 1),
            () => new Cause.UnknownError(tabs, `${targetInfo.url} names ${tabs.length} tabs, and the extension reads and acts on the one tab a page URL names`),
        );
    });
    const act = Effect.fnUntraced(function* (page: Page) {
        const { targetId } = yield* tabbed(page);
        yield* Effect.promise(() => browser.send('Extensions.triggerAction', { id, targetId }));
    });
    const toolbar = Effect.fnUntraced(function* (page: Page) {
        const { targetId } = yield* tabbed(page);
        const { windowId } = yield* Effect.promise(() => browser.send('Browser.getWindowForTarget', { targetId }));
        const session = yield* inspected(page);
        yield* Effect.promise(() => session.send('ServiceWorker.startWorker', { scopeURL: `chrome-extension://${id}/` }));
        const tabId = yield* Effect.promise(() => worker.evaluate(focused, windowId));
        return yield* Effect.all({ badge: Effect.promise(() => worker.evaluate(badged, tabId)), title: Effect.promise(() => worker.evaluate(titled, tabId)) });
    });
    const stop = Effect.fnUntraced(function* (page: Page) {
        const session = yield* inspected(page);
        yield* Effect.promise(() => session.send('ServiceWorker.stopAllWorkers'));
        const running = Array.filter(yield* targets('service_worker'), ({ url }) => url === worker.url());
        yield* Array.isReadonlyArrayNonEmpty(running) ? Effect.fail(new Cause.UnknownError(running, `The service worker of ${id} still runs after stopAllWorkers`)) : Effect.void;
    });
    const evaluate = Effect.fnUntraced(function* (page: Page, expression: string) {
        const session = yield* Effect.promise(() => context.newCDPSession(page));
        const worlds: { readonly contextId: number; readonly origin: string }[] = [];
        session.on('Runtime.executionContextCreated', ({ context: { id: contextId, origin } }) => {
            worlds.push({ contextId, origin });
        });
        yield* Effect.promise(() => session.send('Runtime.enable'));
        const { contextId } = yield* Effect.fromOption(
            Array.findLast(worlds, ({ origin }) => origin === `chrome-extension://${id}`),
            () => new Cause.UnknownError(worlds, `${page.url()} holds no isolated world of ${id}, which the extension's first script injection into the page creates`),
        );
        const { result, exceptionDetails } = yield* Effect.promise(() => session.send('Runtime.evaluate', { contextId, expression, returnByValue: true, awaitPromise: true }));
        return yield* UndefinedOr.match(exceptionDetails, { onUndefined: () => Effect.succeed<unknown>(result.value), onDefined: (thrown) => Effect.fail(new Cause.UnknownError(thrown, `${expression} throws in the isolated world of ${id}: ${thrown.text}`)) });
    });
    const run = Effect.runPromiseWith(yield* Effect.context<never>());
    return { worker, act: (page: Page) => run(act(page)), toolbar: (page: Page) => run(toolbar(page)), stop: (page: Page) => run(stop(page)), evaluate: (page: Page, expression: string) => run(evaluate(page, expression)) } satisfies Extension;
});

const closed = (page: Page) => Effect.promise(() => page.close());

const hosted = (file: string) => `${FILES}${new URL(file).pathname}`;

const files = (context: BrowserContext) => context.route(`${FILES}/**`, (route) => route.fulfill({ path: fileURLToPath(new URL(new URL(route.request().url()).pathname, 'file:///')), headers: { 'access-control-allow-origin': '*' } }));

const branded = Effect.fnUntraced(function* ({ folder, root }: { readonly folder: string; readonly root: string }) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = yield* Effect.flatMap(fs.readFileString(join(folder, 'manifest.json')), Schema.decodeUnknownEffect(Unpacked));
    const messages = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(manifest.default_locale), (locale) => Effect.flatMap(fs.readFileString(join(folder, '_locales', locale, 'messages.json')), Schema.decodeUnknownEffect(Messages))));
    const named = Option.flatMap(Option.fromNullishOr(/^__MSG_(?<key>\w+)__$/u.exec(manifest.name)?.groups?.['key']), (key) => Option.flatMap(messages, Record.get(key)));
    const { config } = yield* Effect.flatMap(fs.readFileString(join(root, 'package.json')), Schema.decodeUnknownEffect(Brand));
    return { name: Option.match(named, { onNone: () => manifest.name, onSome: ({ message }) => message }), mark: hosted(pathToFileURL(join(root, 'icons', 'mark.svg')).href), color: config.brand };
});

const moving = (element: Element) => element.getAnimations({ subtree: true }).filter((animation) => animation instanceof CSSTransition).length;

const paused = (element: Element) => element.getAnimations({ subtree: true }).forEach((animation) => animation.pause());

const still = (page: Page) => (role: Parameters<Page['getByRole']>[0]) =>
    Effect.andThen(
        Effect.promise(() => expect.poll(() => page.getByRole(role).evaluate(moving)).toBe(0)),
        Effect.promise(() => page.getByRole(role).evaluate(paused)),
    );

const themed = (mark: string) => {
    const image = Object.assign(new Image(), { src: mark });
    return image.decode().then(() => document.documentElement.style.setProperty('--ratio', `${image.naturalWidth} / ${image.naturalHeight}`));
};

const rendered = () => document.fonts.ready.then(() => [...document.images].every((image) => image.complete && image.naturalWidth > 0));

const aligned = (heading: HTMLElement) => {
    const { x, y } = heading.getBoundingClientRect();
    const [across, down] = [x, y].map((at) => (Math.round(at * devicePixelRatio) - at * devicePixelRatio) / devicePixelRatio);
    heading.style.setProperty('translate', `${String(across)}px ${String(down)}px`);
};

const scene = Effect.fnUntraced(function* ({ name, url, body, colorScheme, shows, after }: Scene, index: number, { page, context, extension, deviceScaleFactor }: { readonly page: Page; readonly context: BrowserContext; readonly extension: Extension; readonly deviceScaleFactor: number }, info: TestInfo) {
    const open = (at: string, html: string) =>
        context
            .route(at, (route) => route.fulfill({ contentType: 'text/html', body: html }))
            .then(() => page.goto(at))
            .then(Function.constVoid);
    yield* Effect.promise(() => page.setViewportSize({ width: SCREENSHOT.width / deviceScaleFactor, height: SCREENSHOT.height / deviceScaleFactor }));
    yield* Effect.promise(() => page.emulateMedia(colorScheme === undefined ? {} : { colorScheme }));
    yield* Effect.promise(() => (body === undefined ? page.goto(url).then(Function.constVoid) : open(url, body)));
    yield* UndefinedOr.match(shows, {
        onUndefined: () => Effect.void,
        onDefined: (role) =>
            Effect.andThen(
                Effect.promise(() => extension.act(page)),
                Effect.promise(() => expect(page.getByRole(role)).toBeVisible()),
            ),
    });
    yield* UndefinedOr.match(after, { onUndefined: () => Effect.void, onDefined: (step) => Effect.promise(() => step({ page, extension, open })) });
    yield* UndefinedOr.match(shows, { onUndefined: () => Effect.void, onDefined: still(page) });
    yield* Effect.promise(() => page.waitForFunction(rendered));
    yield* Effect.promise(() => page.screenshot({ path: join(info.project.outputDir, `screenshot-${index + 1}-${name}.png`), scale: 'device' }));
});

const tile = Effect.fnUntraced(function* ({ name, width, height, axis, gap, mark, type }: (typeof TILES)[number], { context, built, deviceScaleFactor }: { readonly context: BrowserContext; readonly built: { readonly folder: string; readonly root: string }; readonly deviceScaleFactor: number }, info: TestInfo) {
    const brand = yield* branded(built);
    const page = yield* Effect.acquireRelease(
        Effect.promise(() => context.newPage()),
        (held) => Effect.promise(() => held.close()),
    );
    yield* Effect.promise(() => page.setViewportSize({ width: width / deviceScaleFactor, height: height / deviceScaleFactor }));
    yield* Effect.promise(() =>
        page.setContent(
            `<!doctype html><html lang="en"><link rel="stylesheet" href="${hosted(import.meta.resolve('@fontsource-variable/google-sans-flex'))}"><style>:root { color-scheme: light; background: ${brand.color}; color: white; font: 650 ${type / deviceScaleFactor}px / 1.1 'Google Sans Flex Variable' } body { display: flex; flex-direction: ${axis}; gap: ${gap / deviceScaleFactor}px; align-items: center; justify-content: center; block-size: 100vh; margin: 0 } svg { display: block; block-size: ${mark / deviceScaleFactor}px; aspect-ratio: var(--ratio); background: white; mask: url("${brand.mark}") center / contain no-repeat } h1 { margin: 0; font: inherit; letter-spacing: -0.02em }</style><svg aria-hidden="true"></svg><h1></h1>`,
        ),
    );
    yield* Effect.promise(() => page.evaluate(themed, brand.mark));
    yield* Effect.promise(() => page.locator('h1').evaluate((heading, text) => heading.replaceChildren(text), brand.name));
    yield* Effect.promise(() => page.waitForFunction(rendered));
    yield* Effect.promise(() => page.locator('h1').evaluate(aligned));
    yield* Effect.promise(() => page.screenshot({ path: join(info.project.outputDir, `${name}.png`), scale: 'device' }));
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const config = (home: string) => {
    const folder = Option.flatMap(Schema.decodeUnknownOption(Named)(readJsonFile(join(home, 'package.json'))), ({ name }) => routed('@rasm/nx-playwright', name));
    return defineConfig({
        forbidOnly: Boolean(isCI()),
        reporter: [['html', { open: 'never' }]],
        use: { trace: 'retain-on-failure', screenshot: 'only-on-failure' },
        ...Option.getOrUndefined(
            Option.map(folder, (at) => ({
                projects: [
                    { name: 'test' },
                    {
                        name: 'media',
                        testMatch: '**/*.media.ts',
                        outputDir: join(at, 'media'),
                        use: { deviceScaleFactor: 2, colorScheme: 'light', locale: 'en-US', timezoneId: 'UTC', trace: 'off', screenshot: 'off' },
                    },
                ],
            })),
        ),
    });
};

const resource =
    <A, E>(scoped: Effect.Effect<A, E, Scope.Scope>) =>
    ({}, use: (held: A) => Promise<void>) =>
        Effect.runPromise(Effect.scoped(Effect.flatMap(scoped, (held) => Effect.promise(() => use(held)))));

const it = test.extend<{ readonly axe: AxeBuilder }>({ axe: ({ page }, use) => use(new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'])) });

const loaded = test.extend<{ readonly extension: Extension }, { readonly built: { readonly folder: string; readonly root: string }; readonly launched: BrowserContext }>({
    built: [
        ({}, use, { config: { configFile } }) =>
            Effect.gen(function* () {
                const { name } = yield* Effect.flatMap(Effect.fromNullishOr(configFile), (file) => Schema.decodeUnknownEffect(Named)(readJsonFile(join(dirname(file), 'package.json'))));
                const graph = readCachedProjectGraph();
                const nodes = Array.getSomes((graph.dependencies[name] ?? []).map(({ target }) => Option.fromNullishOr(graph.nodes[target])));
                const targets = nodes.flatMap((node) => Array.map(Record.keys(node.data.targets ?? {}), (target) => [node, target] as const));
                const outputs = targets.flatMap(([node, target]) => getOutputsForTargetAndConfiguration({ project: node.name, target }, {}, node).map((at) => ({ folder: join(workspaceRoot, at), root: join(workspaceRoot, node.data.root) })));
                const builds = Array.dedupeWith(outputs, (one, other) => one.folder === other.folder).filter(({ folder }) => Option.isSome(Option.flatMap(Option.liftPredicate(join(folder, 'manifest.json'), existsSync), (file) => Schema.decodeOption(Unpacked)(readFileSync(file, 'utf-8')))));
                const built = yield* Effect.fromOption(
                    Option.filter(Array.head(builds), () => builds.length === 1),
                    () => new Cause.UnknownError(builds, `${name} depends on ${builds.length} projects whose builds write an unpacked Manifest V3 extension, and a suite loads exactly one`),
                );
                yield* Effect.promise(() => use(built));
            }).pipe(Effect.runPromise),
        { scope: 'worker' },
    ],
    launched: [
        ({ headless, launchOptions }, use, { project }) =>
            Effect.gen(function* () {
                const { viewport, deviceScaleFactor, colorScheme, locale, timezoneId, contextOptions } = project.use;
                const context = yield* Effect.acquireRelease(
                    Effect.promise(() =>
                        chromium.launchPersistentContext('', {
                            ...launchOptions,
                            ...Record.filter({ viewport, deviceScaleFactor, colorScheme, locale, timezoneId }, Predicate.isNotUndefined),
                            ...contextOptions,
                            headless,
                            channel: 'chromium',
                            ignoreDefaultArgs: ['--disable-extensions'],
                            args: [...(launchOptions.args ?? []), '--enable-unsafe-extension-debugging'],
                        }),
                    ),
                    (held) => Effect.promise(() => held.close()),
                );
                yield* Effect.promise(() => use(context));
            }).pipe(Effect.scoped, Effect.runPromise),
        { scope: 'worker' },
    ],
    context: ({ launched, built: { folder } }, use) =>
        Effect.gen(function* () {
            const open = new Set(launched.pages());
            const browser = yield* Effect.acquireRelease(
                Effect.flatMap(Effect.fromNullishOr(launched.browser()), (held) => Effect.promise(() => held.newBrowserCDPSession())),
                (session) => Effect.promise(() => session.detach()),
            );
            yield* Effect.acquireRelease(
                Effect.promise(() => browser.send('Extensions.loadUnpacked', { path: folder })),
                ({ id }) => Effect.promise(() => browser.send('Extensions.uninstall', { id })),
            );
            yield* Effect.addFinalizer(() => Effect.promise(() => launched.unrouteAll({ behavior: 'ignoreErrors' })));
            yield* Effect.addFinalizer(() => Effect.forEach(new Set(launched.pages()).difference(open), closed, { discard: true }));
            yield* Effect.promise(() => use(launched));
        }).pipe(Effect.scoped, Effect.runPromise),
    extension: ({ context }, use) => Effect.runPromise(Effect.flatMap(attached(context), (extension) => Effect.promise(() => use(extension)))),
});

const media = (scenes: Array.NonEmptyReadonlyArray<Scene>) => {
    scenes.forEach((row, index) =>
        loaded(`captures screenshot ${index + 1}, ${row.name}, at ${row.url}`, ({ page, context, extension, deviceScaleFactor }, info) =>
            Effect.runPromise(
                Effect.andThen(
                    Effect.promise(() => files(context)),
                    scene(row, index, { page, context, extension, deviceScaleFactor: deviceScaleFactor ?? 1 }, info),
                ),
            ),
        ),
    );
    TILES.forEach((row) =>
        loaded(`draws the ${row.width}x${row.height} ${row.name} from the icon and name of the built manifest`, ({ context, built, deviceScaleFactor }, info) =>
            Effect.runPromise(
                Effect.provide(
                    Effect.andThen(
                        Effect.promise(() => files(context)),
                        tile(row, { context, built, deviceScaleFactor: deviceScaleFactor ?? 1 }, info),
                    ),
                    layer,
                ),
            ),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { config, type Extension, hosted, it, loaded, media, resource, routed, type Scene };
