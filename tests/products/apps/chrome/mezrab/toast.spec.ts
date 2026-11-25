import { expect } from '@playwright/test';
import messages from '@rasm/mezrab/_locales/en/messages.json' with { type: 'json' };
import { Function } from 'effect';
import { frozen, held, it, STUB, settled, sought } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFUSED = { 'permissions-policy': 'web-share=()' } as const;
const RATE = 20;
const SCHEMES = ['light', 'dark'] as const;
const FILL = { pseudo: null, properties: ['background-color', 'color'] } as const;
const RING = { pseudo: '::before', properties: ['outline-color'] } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const hexes = (element: Element, { pseudo, properties }: { readonly pseudo: string | null; readonly properties: readonly string[] }) =>
    properties.map((property) => {
        const context = new OffscreenCanvas(1, 1).getContext('2d');
        Object.assign(context ?? {}, { fillStyle: getComputedStyle(element, pseudo).getPropertyValue(property) });
        context?.fillRect(0, 0, 1, 1);
        return `#${Uint8Array.from(context?.getImageData(0, 0, 1, 1).data ?? []).toHex()}`;
    });

const channels = (hex: string) => Array.from(Uint8Array.fromHex(hex.slice(1)), (level) => level / 255);

const luminance = (levels: readonly number[]) => {
    const [red = Number.NaN, green = Number.NaN, blue = Number.NaN] = levels.map((level) => (level <= 0.04045 ? level / 12.92 : ((level + 0.055) / 1.055) ** 2.4));
    return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
};

const contrast = (top: string, fill: string) => {
    const [ground, [red = Number.NaN, green = Number.NaN, blue = Number.NaN, alpha = Number.NaN]] = [channels(fill), channels(top)];
    const seen = [red, green, blue].map((level, index) => level * alpha + (ground[index] ?? Number.NaN) * (1 - alpha));
    const [light = Number.NaN, dark = Number.NaN] = [luminance(seen), luminance(ground)].toSorted((left, right) => right - left);
    return (light + 0.05) / (dark + 0.05);
};

const onScreen = (element: Element) => [element.getBoundingClientRect().height, element.getBoundingClientRect().width].map((length) => Math.round(length * devicePixelRatio));

const drawn = (element: Element) => ({ font: getComputedStyle(element).font, shown: [getComputedStyle(element).visibility, getComputedStyle(element).opacity], box: [element.getBoundingClientRect().width, element.getBoundingClientRect().height] });

const framed = () => {
    const { promise, resolve } = Promise.withResolvers<number>();
    requestAnimationFrame(() => {
        requestAnimationFrame(resolve);
    });
    return promise;
};

const exiting = (element: Element) => element.getAnimations().filter((animation) => animation instanceof CSSTransition && animation.playState === 'running').length;

const themed = () => {
    const sheet = new CSSStyleSheet();
    sheet.replaceSync(document.body.textContent);
    return ['--color-toast-background-prominent', '--color-toast-foreground'].map((token) => [...sheet.cssRules].flatMap((rule) => (rule instanceof CSSStyleRule ? [rule.style.getPropertyValue(token)] : [])).find((tone) => tone !== ''));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('keeps its on-screen size when page zoom is 200% as the action runs and when zoom returns to 100% while it shows', async ({ page, open, extension, toast }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/zoomed', '<p>Zoomed</p>', REFUSED);
    const [tab] = await extension.worker.evaluate(() => browser.tabs.query({ active: true, lastFocusedWindow: true }));
    const zoom = (level: number) => extension.worker.evaluate(([id, at]) => browser.tabs.setZoomSettings(id, { scope: 'per-tab' }).then(() => browser.tabs.setZoom(id, at)), [tab?.id ?? -1, level] as const);
    await zoom(2);
    await expect.poll(() => page.evaluate('devicePixelRatio')).toBe(2);
    await extension.act(page);
    await expect(toast).toBeVisible();
    await frozen(toast);
    const zoomed = await toast.evaluate(onScreen);

    await zoom(1);

    await expect.poll(() => page.evaluate('devicePixelRatio')).toBe(1);
    await expect.poll(() => toast.evaluate(onScreen)).toEqual(zoomed);
});

it("draws the pill of a plain page on a page whose CSS unsets every element, sets every font to 5px, and hides and uppercases the root, under Chrome's 40px font size setting", async ({ page, context, open, extension, toast }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/plain', '<p>Plain</p>', REFUSED);
    await extension.act(page);
    await expect(toast).toBeVisible();
    const plain = await toast.evaluate(drawn);
    await open('/hostile', '<style>* { all: unset !important; font-size: 5px !important } :root { visibility: hidden !important; text-transform: uppercase !important }</style><p>Hostile</p>', REFUSED);
    const session = await context.newCDPSession(page);
    await session.send('Page.setFontSizes', { fontSizes: { standard: 40 } });

    await extension.act(page);

    await expect(toast).toHaveText(messages.copied.message, { useInnerText: true });
    expect(await toast.evaluate(drawn)).toEqual(plain);
});

it("raises its type to Chrome's 24px minimum font size and keeps its text inside the pill", async ({ page, open, extension, toast, setting }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await setting('webkit.webprefs.minimum_font_size', 24);
    await open('/minimum', '<p>Minimum</p>', REFUSED);

    await extension.act(page);

    await expect(toast).toBeVisible();
    const { size, overflow } = await toast.evaluate((element) => {
        const [pill, text] = [element.getBoundingClientRect(), document.createRange()];
        text.selectNodeContents(element.querySelector('span') ?? element);
        const words = text.getBoundingClientRect();
        return { size: getComputedStyle(element).fontSize, overflow: Math.max(pill.top - words.top, words.bottom - pill.bottom, pill.left - words.left, words.right - pill.right) };
    });
    expect(size).toBe('24px');
    expect(overflow, 'text length outside the pill').toBeLessThanOrEqual(0);
});

it('centres its pill in a viewport too narrow for it, with a whole share button of at least 24 by 24px inside the pill', async ({ page, open, armed, extension, toast, button }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.setViewportSize({ width: 120, height: 400 });
    await open('/narrow', '<p>Narrow</p>');
    await armed(STUB.offered);

    await extension.act(page);

    await expect(button).toBeVisible();
    const { clipped, start, end, cut, target } = await toast.evaluate((element) => {
        const [pill, action, text] = [element.getBoundingClientRect(), element.querySelector('button')?.getBoundingClientRect() ?? new DOMRect(Number.NaN, Number.NaN, Number.NaN, Number.NaN), element.querySelector('span')];
        return { clipped: (text?.scrollWidth ?? Number.NaN) - (text?.clientWidth ?? Number.NaN), start: pill.left, end: innerWidth - pill.right, cut: Math.max(pill.left - action.left, action.right - pill.right, pill.top - action.top, action.bottom - pill.bottom), target: [action.width, action.height] };
    });
    expect(clipped, 'text length the narrow pill cuts').toBeGreaterThan(0);
    expect(start, 'gap before the pill').toBeGreaterThan(0);
    expect(end, 'gap after the pill').toBe(start);
    expect(cut, 'share button length outside the pill').toBeLessThanOrEqual(0);
    expect(Math.min(...target), `share button target ${target.join(' by ')}`).toBeGreaterThanOrEqual(24);
});

it("paints the toast background and foreground of chrome://theme in the browser's own colour scheme", async ({ page, context, open, extension, toast }) => {
    await page.emulateMedia({ colorScheme: null, contrast: 'no-preference' });
    const theme = await context.newPage();
    await theme.goto('chrome://theme/colors.css?sets=ui');
    const tokens = await theme.evaluate(themed);
    await theme.close();
    await open('/themed', '<p>Themed</p>');

    await extension.act(page);

    await expect(toast).toBeVisible();
    await settled(toast);
    expect(await toast.evaluate(hexes, FILL)).toEqual(tokens);
});

SCHEMES.forEach((scheme) =>
    it(`keeps its text above 4.5:1 and the share button's keyboard focus ring above 3:1 against its fill in the ${scheme} colour scheme`, async ({ page, open, armed, extension, toast, button }) => {
        await page.emulateMedia({ colorScheme: scheme, reducedMotion: 'reduce' });
        await open(`/contrast-${scheme}`, '<input aria-label="Draft">');
        await armed(STUB.offered);
        await extension.act(page);
        await expect(button).toBeVisible();
        await page.getByRole('textbox', { name: 'Draft' }).focus();

        await page.keyboard.press('Tab');

        await expect(button).toBeFocused();
        const [fill = '', text = ''] = await toast.evaluate(hexes, FILL);
        const [ring = ''] = await button.evaluate(hexes, RING);
        expect(await button.evaluate((element) => getComputedStyle(element, '::before').outlineStyle)).not.toBe('none');
        expect(fill, 'an opaque fill').toMatch(/^#[\da-f]{6}ff$/u);
        expect(contrast(text, fill), `text ${text} on fill ${fill}`).toBeGreaterThanOrEqual(4.5);
        expect(contrast(ring, fill), `ring ${ring} on fill ${fill}`).toBeGreaterThanOrEqual(3);
    }),
);

it('paints in an Incognito window under a light colour scheme the fill and text it paints under a dark one', async ({ page, context, built, open, extension, toast }) => {
    await page.emulateMedia({ colorScheme: 'light' });
    await open('/schemes', '<p>Schemes</p>');
    await extension.act(page);
    await expect(toast).toBeVisible();
    await settled(toast);
    const light = await toast.evaluate(hexes, FILL);
    await page.emulateMedia({ colorScheme: 'dark' });
    await settled(toast);
    const dark = await toast.evaluate(hexes, FILL);
    expect(dark, 'dark tones that differ from the light ones').not.toEqual(light);
    const devtools = await context.browser()?.newBrowserCDPSession();
    const [reloaded, opened] = [context.waitForEvent('serviceworker'), context.waitForEvent('page')];
    await devtools?.send('Extensions.loadUnpacked', { path: built.folder, enableInIncognito: true });
    const worker = await reloaded;
    await context.route('https://page.test/private', (route) => route.fulfill({ contentType: 'text/html', body: '<!doctype html><title>Private</title><p>Private</p>' }));
    await worker.evaluate(() => browser.windows.create({ incognito: true, url: 'about:blank' }));
    const incognito = await opened;
    await incognito.emulateMedia({ colorScheme: 'light' });
    await incognito.goto('https://page.test/private');

    await extension.act(incognito);

    await expect(incognito.getByRole('status')).toBeVisible();
    await settled(incognito.getByRole('status'));
    expect(await incognito.getByRole('status').evaluate(hexes, FILL)).toEqual(dark);
});

it('paints the CanvasText system colour under Canvas text under prefers-contrast more', async ({ page, open, extension, toast }) => {
    await page.emulateMedia({ contrast: 'more' });
    await open('/contrast', '<p style="color: CanvasText; background-color: Canvas">System</p>');
    const system = await page.getByText('System').evaluate((element) => [getComputedStyle(element).color, getComputedStyle(element).backgroundColor] as const);

    await extension.act(page);

    await expect(toast).toHaveCSS('background-color', system[0]);
    await expect(toast).toHaveCSS('color', system[1]);
});

it('draws a border, its link icon in CanvasText, and its share glyph in ButtonText under forced colours, and no border without them', async ({ page, open, armed, extension, toast, button }) => {
    await open('/forced', '<p style="color: CanvasText">Link</p><p style="color: ButtonText">Share</p>');
    await armed(STUB.offered);
    await extension.act(page);
    await expect(button).toBeVisible();
    await expect(toast).toHaveCSS('border-block-start-style', 'none');

    await page.emulateMedia({ forcedColors: 'active' });

    const system = await page.locator('p').evaluateAll((marks) => marks.map((mark) => getComputedStyle(mark).color));
    await expect(toast).not.toHaveCSS('border-block-start-style', 'none');
    expect([await toast.evaluate((element) => getComputedStyle(element, '::before').backgroundColor), await button.evaluate((element) => getComputedStyle(element, '::after').backgroundColor)]).toEqual(system);
});

it('moves as it enters and again as it leaves', async ({ page, context, open, extension, toast }) => {
    await open('/motion', '<p>Motion</p>', REFUSED);
    const session = await context.newCDPSession(page);
    const moved: string[] = [];
    session.on('Animation.animationStarted', ({ animation: { type, name } }) => {
        moved.push(...(type === 'CSSTransition' ? [name] : []));
    });
    await session.send('Animation.enable');
    await extension.act(page);
    await toast.evaluate((element) => element.getAnimations().forEach((animation) => (animation instanceof CSSAnimation ? animation.pause() : undefined)));
    await settled(toast);
    const entered = moved.length;

    await toast.evaluate((element) => element.getAnimations().forEach((animation) => animation.finish()));

    await expect(page.locator('mezrab-toast')).toHaveCount(0);
    expect(entered, 'transitions as it enters').toBeGreaterThan(0);
    expect(moved.length - entered, 'transitions as it leaves').toBeGreaterThan(0);
});

it('runs no transition under prefers-reduced-motion and still leaves the page when its hold ends', async ({ page, context, open, extension, toast }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/still', '<p>Still</p>', REFUSED);
    const session = await context.newCDPSession(page);
    const moved: string[] = [];
    session.on('Animation.animationStarted', ({ animation: { type, name } }) => {
        moved.push(...(type === 'CSSTransition' ? [name] : []));
    });
    await session.send('Animation.enable');
    await extension.act(page);
    await expect(toast).toBeVisible();

    await sought(toast, 4000);

    await expect(page.locator('mezrab-toast')).toHaveCount(0);
    expect(moved).toEqual([]);
});

(
    [
        { offer: 'without the share button', stub: STUB.refused, buttons: 0, hold: 4000 },
        { offer: 'with the share button', stub: STUB.offered, buttons: 1, hold: 8000 },
    ] as const
).forEach(({ offer, stub, buttons, hold }) =>
    it(`stays open through ${hold - 1}ms of its hold and leaves the page at ${hold}ms ${offer}`, async ({ page, open, armed, extension, toast, button }) => {
        await open(`/held-${hold}`, '<p>Held</p>');
        await armed(stub);
        await extension.act(page);
        await expect(toast).toBeVisible();
        await expect(button).toHaveCount(buttons);
        await frozen(toast);

        await sought(toast, hold - 1);
        await page.evaluate(framed);
        const before = await toast.evaluate((element) => element.matches(':popover-open'));
        await sought(toast, hold);

        await expect(page.locator('mezrab-toast')).toHaveCount(0);
        expect(before).toBe(true);
    }),
);

it('leaves the page when Chrome cancels its exit motion', async ({ page, context, open, extension, toast }) => {
    await open('/cancelled', '<p>Cancelled</p>', REFUSED);
    await extension.act(page);
    await expect(toast).toBeVisible();
    await settled(toast);
    const session = await context.newCDPSession(page);
    await session.send('Animation.setPlaybackRate', { playbackRate: 1 / RATE });
    await sought(toast, 4000);
    await expect.poll(() => toast.evaluate((element) => element.matches(':popover-open'))).toBe(false);
    await expect.poll(() => toast.evaluate(exiting), { message: 'exit transitions running' }).toBeGreaterThan(0);

    await page.emulateMedia({ reducedMotion: 'reduce' });

    await expect(page.locator('mezrab-toast')).toHaveCount(0, { timeout: 1000 });
});

it('keeps one toast and restarts its hold when a second action finds it 2000ms into the hold', async ({ page, open, extension, toast }) => {
    await open('/twice', '<p>Twice</p>', REFUSED);
    await extension.act(page);
    await expect(toast).toBeVisible();
    await frozen(toast);
    await sought(toast, 2000);

    await extension.act(page);

    await expect.poll(() => held(toast)).toEqual([{ state: 'paused', at: 0 }]);
    await expect(page.locator('mezrab-toast')).toHaveCount(1);
});

it('announces Link copied on each action', async ({ page, open, extension, isolated }) => {
    await open('/announced', '<p>Announced</p>');
    await isolated('globalThis.notified = []; Document.prototype.ariaNotify = Element.prototype.ariaNotify = function (text) { globalThis.notified.push(text); };');
    const notified = () => extension.evaluate(page, 'globalThis.notified');

    await extension.act(page);
    await expect.poll(notified).toEqual([messages.copied.message]);
    await extension.act(page);

    await expect.poll(notified).toEqual([messages.copied.message, messages.copied.message]);
});

it('passes a click on its pill to the page beneath', async ({ page, open, extension, toast }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/through', '<button style="position: fixed; inset: 0 0 auto; block-size: 96px" onclick="this.dataset.hits = Number(this.dataset.hits ?? 0) + 1">Under</button>', REFUSED);
    await extension.act(page);
    await expect(toast).toBeVisible();
    const [x, y] = await toast.evaluate((element) => [element.getBoundingClientRect().left + element.getBoundingClientRect().width / 2, element.getBoundingClientRect().top + element.getBoundingClientRect().height / 2] as const);

    await page.mouse.click(x, y);

    await expect(page.getByRole('button', { name: 'Under' })).toHaveAttribute('data-hits', '1');
});

it('shows on a page that painted and still parses behind a script that has not loaded', async ({ page, context, extension, toast }) => {
    await context.route('https://page.test/held.js', Function.constVoid);
    await context.route('https://page.test/loading', (route) => route.fulfill({ contentType: 'text/html', body: `<!doctype html><title>Loading</title><p>${'Painted '.repeat(30)}</p><script src="/held.js"></script><p>Below</p>` }));
    await page.goto('https://page.test/loading', { waitUntil: 'commit' });
    await page.waitForFunction(() => performance.getEntriesByName('first-contentful-paint').length === 1);

    await extension.act(page);

    await expect(toast).toBeVisible();
    await expect(toast).toHaveText(messages.copied.message);
    expect(await page.evaluate(() => document.readyState)).toBe('loading');
});
