import { expect, type Locator, type Page, type Route } from '@playwright/test';
import { type Extension, loaded } from '@rasm/testing/playwright';
import { Effect } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STUB = {
    offered: 'globalThis.shares = []; globalThis.pending = []; navigator.canShare = () => true; navigator.share = (data) => new Promise((resolve, reject) => { globalThis.shares.push(data); globalThis.pending.push({ resolve, reject }); });',
    matched: 'globalThis.shares = []; navigator.canShare = (data) => data.url === location.href && data.title === document.title; navigator.share = (data) => new Promise(() => globalThis.shares.push(data));',
    refused: 'globalThis.shares = []; navigator.canShare = () => false; navigator.share = (data) => new Promise(() => globalThis.shares.push(data));',
    missing: 'delete Navigator.prototype.share; delete Navigator.prototype.canShare;',
    unshareable: 'navigator.canShare = () => true; delete Navigator.prototype.share;',
} as const;

const SETTLE = {
    resolved: 'globalThis.pending.at(-1).resolve()',
    rejected: "globalThis.pending.at(-1).reject(new DOMException('Share canceled', 'AbortError'))",
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = (body: string, headers: Record<string, string>) => (route: Route) => route.fulfill({ contentType: 'text/html', headers, body: `<!doctype html><title>Page</title>${body}` });

const held = (toast: Locator) => toast.evaluate((element) => element.getAnimations().flatMap((animation) => (animation instanceof CSSAnimation ? [{ state: animation.playState, at: Number(animation.currentTime) }] : [])));

const settled = (toast: Locator) => expect.poll(() => toast.evaluate((element) => element.getAnimations().filter((animation) => animation instanceof CSSTransition).length)).toBe(0);

const frozen = (toast: Locator) => toast.evaluate((element) => element.getAnimations().forEach((animation) => animation.pause()));

const sought = (toast: Locator, at: number) =>
    toast.evaluate((element, time) => {
        element
            .getAnimations()
            .filter((animation) => animation instanceof CSSAnimation)
            .forEach((animation) => {
                Object.assign(animation, { currentTime: time });
            });
    }, at);

const isolate = ({ page, extension }: { readonly page: Page; readonly extension: Extension }, expression: string) =>
    Effect.all([Effect.promise(() => extension.act(page)), Effect.promise(() => expect(page.getByRole('status')).toBeVisible()), Effect.promise(() => extension.evaluate(page, expression))], { discard: true });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = loaded.extend<{
    readonly open: (path: string, body: string, headers?: Record<string, string>) => Promise<void>;
    readonly copied: () => Promise<string>;
    readonly toast: Locator;
    readonly button: Locator;
    readonly isolated: (expression: string) => Promise<void>;
    readonly armed: (stub: string) => Promise<void>;
    readonly shares: () => Promise<unknown>;
    readonly settle: (outcome: string) => Promise<unknown>;
    readonly setting: (key: string, value: number) => Promise<unknown>;
}>({
    open: ({ context, page }, use) =>
        use((path, body, headers = {}) => {
            const { origin, href } = new URL(path, 'https://page.test/');
            return Effect.runPromise(
                Effect.andThen(
                    Effect.promise(() => context.route(`${origin}/**`, served(body, headers))),
                    Effect.asVoid(Effect.promise(() => page.goto(href))),
                ),
            );
        }),
    copied: ({ page }, use) => use(() => page.evaluate(() => navigator.clipboard.readText())),
    toast: ({ page }, use) => use(page.getByRole('status')),
    button: ({ toast }, use) => use(toast.getByRole('button')),
    isolated: ({ page, extension }, use) => use((expression) => Effect.runPromise(isolate({ page, extension }, expression))),
    armed: ({ page, isolated, toast }, use) =>
        use((stub) =>
            isolated(stub)
                .then(() => toast.evaluate((element) => (element instanceof HTMLElement ? element.hidePopover() : undefined)))
                .then(() => expect(page.locator('mezrab-toast')).toHaveCount(0)),
        ),
    shares: ({ page, extension }, use) => use(() => extension.evaluate(page, 'globalThis.shares')),
    settle: ({ page, extension }, use) =>
        use((outcome) =>
            expect
                .poll(() => extension.evaluate(page, 'globalThis.pending.length'))
                .toBe(1)
                .then(() => extension.evaluate(page, outcome)),
        ),
    setting: ({ context, page }, use) => {
        const before = new Map<string, unknown>();
        const settings = context.newPage().then((tab) => tab.goto('chrome://settings').then(() => tab));
        const write = (key: string, value: unknown) => settings.then((tab) => tab.evaluate(`new Promise((resolve) => chrome.settingsPrivate.getPref(${JSON.stringify(key)}, (earlier) => chrome.settingsPrivate.setPref(${JSON.stringify(key)}, ${JSON.stringify(value)}, '', () => resolve(earlier.value))))`));
        return use((key, value) =>
            write(key, value)
                .then((earlier) => before.getOrInsert(key, earlier))
                .then(() => page.bringToFront()),
        ).then(() => Effect.runPromise(Effect.forEach(before, ([key, earlier]) => Effect.promise(() => write(key, earlier)), { discard: true })));
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { frozen, held, isolate, it, SETTLE, STUB, settled, sought };
