import { expect, type JSHandle, type Locator, type Page, type Route } from '@playwright/test';
import { type Extension, loaded } from '@rasm/testing/playwright';
import { Effect } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Timed {
    readonly property: string;
    readonly duration: unknown;
    readonly delay: unknown;
    readonly easing: unknown;
}

interface Lifecycle {
    readonly entry: readonly Timed[];
    readonly exit: readonly Timed[];
    readonly opened: number;
    readonly closed: number;
    readonly removed: boolean;
}

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

const watched = () => {
    const marks: Lifecycle = { entry: [], exit: [], opened: 0, closed: 0, removed: false };
    const timed = (popover: Element, phase: 'entry' | 'exit') =>
        Object.assign(marks, {
            [phase]: popover.getAnimations().flatMap((animation) => (animation instanceof CSSTransition ? [{ property: animation.transitionProperty, duration: animation.effect?.getTiming().duration, delay: animation.effect?.getTiming().delay, easing: animation.effect?.getTiming().easing }] : [])),
        });
    new MutationObserver((records, observer) => {
        const nodes = records.flatMap(({ addedNodes, removedNodes }) => [...addedNodes, ...removedNodes]);
        nodes
            .flatMap((node) => (node instanceof Element && node.isConnected ? [node.shadowRoot?.querySelector('[popover]')].filter((found) => found instanceof Element) : []))
            .forEach((popover) => {
                timed(popover, 'entry');
                popover.addEventListener(
                    'toggle',
                    ({ timeStamp }) => {
                        Object.assign(marks, { opened: timeStamp });
                    },
                    { once: true },
                );
                popover.addEventListener('animationend', ({ timeStamp }) => {
                    Object.assign(marks, { closed: timeStamp });
                    timed(popover, 'exit');
                });
            });
        nodes
            .filter((node) => node instanceof Element && !node.isConnected && node.shadowRoot !== null)
            .forEach(() => {
                observer.disconnect();
                Object.assign(marks, { removed: true });
            });
    }).observe(document.documentElement, { childList: true });
    return marks;
};

const moving = (element: Element) => element.getAnimations({ subtree: true }).filter((animation) => animation instanceof CSSTransition).length;

const settled = (toast: Locator) => expect.poll(() => toast.evaluate(moving)).toBe(0);

const ended = (handle: JSHandle<Lifecycle>) =>
    expect
        .poll(() => handle.evaluate(({ removed }) => removed), { timeout: 20_000 })
        .toBe(true)
        .then(() => handle.jsonValue());

const styled = (toast: Locator, pseudo?: string) =>
    toast.evaluate((element, at) => {
        const style = getComputedStyle(element, at);
        return { width: style.width, height: style.height, font: [style.fontFamily, style.fontSize, style.fontWeight, style.lineHeight], background: style.backgroundColor, color: style.color, border: [style.borderTopWidth, style.borderTopStyle] };
    }, pseudo ?? null);

const drawn = (base64: string) =>
    createImageBitmap(new Blob([Uint8Array.fromBase64(base64)], { type: 'image/png' })).then((bitmap) => {
        const pen = new OffscreenCanvas(1, 1).getContext('2d');
        pen?.drawImage(bitmap, 0, 0);
        return [...(pen?.getImageData(0, 0, 1, 1).data.slice(0, 3) ?? [])];
    });

const pixel = (page: Page, x: number, y: number) =>
    Effect.runPromise(
        Effect.flatMap(
            Effect.promise(() => page.screenshot({ clip: { x, y, width: 1, height: 1 }, scale: 'css' })),
            (png) => Effect.promise(() => page.evaluate(drawn, png.toString('base64'))),
        ),
    );

const isolate = ({ page, extension }: { readonly page: Page; readonly extension: Extension }, expression: string) =>
    Effect.all([Effect.promise(() => extension.act(page)), Effect.promise(() => expect(page.getByRole('status')).toBeVisible()), Effect.promise(() => extension.evaluate(page, expression))], { discard: true });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = loaded.extend<{
    readonly open: (path: string, body: string, headers?: Record<string, string>) => Promise<void>;
    readonly copied: () => Promise<string>;
    readonly toast: Locator;
    readonly button: Locator;
    readonly lifecycle: () => Promise<JSHandle<Lifecycle>>;
    readonly isolated: (expression: string) => Promise<void>;
    readonly armed: (stub: string) => Promise<void>;
    readonly shares: () => Promise<unknown>;
    readonly settle: (outcome: string) => Promise<unknown>;
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
    lifecycle: ({ page }, use) => use(() => page.evaluateHandle(watched)),
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
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ended, isolate, it, pixel, SETTLE, STUB, settled, styled };
