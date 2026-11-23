import { expect, type Locator, type Page } from '@playwright/test';
import { Function } from 'effect';
import { frozen, held, it, STUB, settled, sought } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFUSED = { 'permissions-policy': 'web-share=()' } as const;
const RATE = 20;
const TONES = [
    { scheme: 'light', background: '#062e6f', color: '#f2f2f2', hovered: '#244780', pressed: '#486594', ring: '#a8c7fa' },
    { scheme: 'dark', background: '#ecf3fe', color: '#1f1f1f', hovered: '#d2d8e1', pressed: '#b2b8bf', ring: '#0b57d0' },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const painted = (element: Element) =>
    [getComputedStyle(element).backgroundColor, getComputedStyle(element).color].map((tone) => {
        const context = new OffscreenCanvas(1, 1).getContext('2d');
        Object.assign(context ?? {}, { fillStyle: tone });
        context?.fillRect(0, 0, 1, 1);
        return `#${new Uint8Array(context?.getImageData(0, 0, 1, 1).data.buffer ?? new ArrayBuffer(3), 0, 3).toHex()}`;
    });

const circled = (button: Locator) =>
    button.evaluate((element) => {
        const [target, circle] = [element.getBoundingClientRect(), getComputedStyle(element, '::before')];
        return { start: Math.round(target.right - Number.parseFloat(circle.insetInlineEnd) - Number.parseFloat(circle.inlineSize)), row: Math.floor(target.top + target.height / 2) };
    });

const moving = (element: Element) => element.getAnimations({ subtree: true }).length;

const themed = () => {
    const sheet = new CSSStyleSheet();
    sheet.replaceSync(document.body.textContent);
    return ['--color-toast-background-prominent', '--color-toast-foreground'].map((token) => [...sheet.cssRules].flatMap((rule) => (rule instanceof CSSStyleRule ? [rule.style.getPropertyValue(token)] : [])).find((tone) => tone !== ''));
};

const decoded = (png: string) =>
    createImageBitmap(new Blob([Uint8Array.fromBase64(png)], { type: 'image/png' })).then((bitmap) => {
        const context = new OffscreenCanvas(1, 1).getContext('2d');
        context?.drawImage(bitmap, 0, 0);
        return `#${new Uint8Array(context?.getImageData(0, 0, 1, 1).data.buffer ?? new ArrayBuffer(3), 0, 3).toHex()}`;
    });

const sampled = (page: Page, x: number, y: number) => page.screenshot({ clip: { x, y, width: 1, height: 1 } }).then((shot) => page.evaluate(decoded, shot.toString('base64')));

const near = (drawn: string, tone: string, layers: number) => {
    const [screen, source] = [drawn, tone].map((hex) => Uint8Array.fromHex(hex.slice(1)));
    expect(Math.max(...Array.from(screen ?? [], (level, channel) => Math.abs(level - (source?.[channel] ?? Number.NaN)))), `screen draws ${drawn}, within one level per translucent layer (${layers}) of ${tone} in each channel`).toBeLessThanOrEqual(layers);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

[1, 2].forEach((zoom) =>
    it(`shows Link copied as Chrome's 48px pill, centred, its text 40px from the start and 16px from the end in 13px/24px type, at ${zoom * 100}% page zoom`, async ({ page, open, extension, toast, button }) => {
        await page.emulateMedia({ reducedMotion: 'reduce' });
        await open(`/size-${zoom}`, '<p>Size</p>', REFUSED);
        const [tab] = await extension.worker.evaluate(() => browser.tabs.query({ active: true, lastFocusedWindow: true }));
        await extension.worker.evaluate((id) => browser.tabs.setZoomSettings(id, { scope: 'per-tab' }), tab?.id ?? -1);
        await extension.worker.evaluate(([id, level]) => browser.tabs.setZoom(id, level), [tab?.id ?? -1, zoom] as const);
        await expect.poll(() => page.evaluate('devicePixelRatio')).toBe(zoom);

        await extension.act(page);

        await expect(toast).toHaveText('Link copied');
        expect(await button.count()).toBe(0);
        const drawn = await toast.evaluate((element) => {
            const [pill, text, style] = [element.getBoundingClientRect(), document.createRange(), getComputedStyle(element)];
            text.selectNodeContents(element);
            const words = text.getBoundingClientRect();
            return { box: [pill.height, pill.left + pill.width / 2 - innerWidth / 2, words.left - pill.left, pill.right - words.right], type: [style.fontSize, style.lineHeight] };
        });
        expect({ ...drawn, box: drawn.box.map((length) => Math.round(length * zoom) + 0) }).toEqual({ box: [48, 0, 40, 16], type: ['13px', '24px'] });
    }),
);

[20, 40].forEach((setting) =>
    it(`keeps Chrome's 13px type on a 24px line at a Chrome font size setting of ${setting}px on a page whose CSS sets every font size to 5px`, async ({ page, context, open, extension, toast }) => {
        await open(`/type-${setting}`, '<style>:root, * { font-size: 5px !important }</style><p>Type</p>', REFUSED);
        const session = await context.newCDPSession(page);
        await session.send('Page.setFontSizes', { fontSizes: { standard: setting } });

        await extension.act(page);

        await expect(toast).toBeVisible();
        expect(await toast.evaluate((element) => [getComputedStyle(element).fontSize, getComputedStyle(element).lineHeight])).toEqual(['13px', '24px']);
    }),
);

[18, 24].forEach((minimum) =>
    it(`raises its type to ${minimum}px on the same 24px line inside the 48px pill at a Chrome minimum font size of ${minimum}px`, async ({ page, open, extension, toast, setting }) => {
        await page.emulateMedia({ reducedMotion: 'reduce' });
        await setting('webkit.webprefs.minimum_font_size', minimum);
        await open(`/minimum-${minimum}`, '<p>Minimum</p>', REFUSED);

        await extension.act(page);

        await expect(toast).toBeVisible();
        expect(await toast.evaluate((element) => [getComputedStyle(element).fontSize, getComputedStyle(element).lineHeight, element.getBoundingClientRect().height])).toEqual([`${minimum}px`, '24px', 48]);
    }),
);

it('keeps its 48px pill when page zoom changes from 100% to 200% while it shows', async ({ page, open, extension, toast }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/rezoomed', '<p>Rezoomed</p>', REFUSED);
    const [tab] = await extension.worker.evaluate(() => browser.tabs.query({ active: true, lastFocusedWindow: true }));
    await extension.worker.evaluate((id) => browser.tabs.setZoomSettings(id, { scope: 'per-tab' }), tab?.id ?? -1);
    await extension.act(page);
    await expect(toast).toBeVisible();
    await frozen(toast);

    await extension.worker.evaluate((id) => browser.tabs.setZoom(id, 2), tab?.id ?? -1);

    await expect.poll(() => page.evaluate('devicePixelRatio')).toBe(2);
    await expect.poll(() => toast.evaluate((element) => Math.round(element.getBoundingClientRect().height * devicePixelRatio))).toBe(48);
});

it("keeps Chrome's 4px from each side of a 120px viewport too narrow for the pill, with the whole share button inside it", async ({ page, open, armed, extension, toast, button }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.setViewportSize({ width: 120, height: 400 });
    await open('/narrow', '<p>Narrow</p>');
    await armed(STUB.offered);

    await extension.act(page);

    await expect(button).toBeVisible();
    const drawn = await toast.evaluate((element) => {
        const [pill, action] = [element.getBoundingClientRect(), element.querySelector('button')?.getBoundingClientRect()];
        return { start: pill.left, end: innerWidth - pill.right, cut: Math.max(0, (action?.right ?? Number.NaN) - pill.right) };
    });
    expect(drawn).toEqual({ start: 4, end: 4, cut: 0 });
});

it("draws the share button as the 28px circle of Chrome's toast menu button, 12px after the toast text and 10px from the pill's end, in a 50px by 48px target", async ({ page, open, armed, extension, toast, button }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/button', '<p>Button</p>');
    await armed(STUB.offered);

    await extension.act(page);

    await expect(button).toBeVisible();
    const drawn = await toast.evaluate((element) => {
        const [pill, text, action] = [element.getBoundingClientRect(), element.querySelector('span')?.getBoundingClientRect(), element.querySelector('button')];
        const circle = action === null ? undefined : getComputedStyle(action, '::before');
        const [side, inset] = [Number.parseFloat(circle?.inlineSize ?? 'NaN'), Number.parseFloat(circle?.insetInlineEnd ?? 'NaN')];
        const [end, target] = [pill.right - (action?.getBoundingClientRect().right ?? Number.NaN) + inset, action?.getBoundingClientRect()];
        return { side: [circle?.inlineSize, circle?.blockSize], end, gap: pill.right - end - side - (text?.right ?? Number.NaN), target: [target?.width, target?.height] };
    });
    expect(drawn).toEqual({ side: ['28px', '28px'], end: 10, gap: 12, target: [50, 48] });
});

TONES.forEach(({ scheme, background, color, hovered, pressed, ring }) => {
    it(`paints the ${background} fill of Chrome's toast at its default theme under text ${color} in the ${scheme} colour scheme`, async ({ page, open, extension, toast }) => {
        await page.emulateMedia({ colorScheme: scheme });
        await open(`/${scheme}`, '<p>Scheme</p>');

        await extension.act(page);

        await expect(toast).toBeVisible();
        await settled(toast);
        expect(await toast.evaluate(painted)).toEqual([background, color]);
    });

    it(`lays ${color} at 12.8% over the share button's circle under a hovering pointer, ${hovered} on screen, fading over 250ms in the ${scheme} colour scheme`, async ({ page, context, open, armed, extension, toast, button }) => {
        await page.emulateMedia({ colorScheme: scheme });
        await open(`/hovered-${scheme}`, '<p>Hovered</p>');
        await armed(STUB.offered);
        const session = await context.newCDPSession(page);
        const faded: string[] = [];
        session.on('Animation.animationStarted', ({ animation: { type, name, source } }) => {
            faded.push(...(type === 'CSSTransition' && name === 'background-color' ? [`${source?.duration} ${source?.easing}`] : []));
        });
        await session.send('Animation.enable');
        await extension.act(page);
        await expect(button).toBeVisible();
        await settled(toast);
        const { start, row } = await circled(button);

        await button.hover();

        await expect.poll(() => faded.length).toBe(1);
        await expect.poll(() => button.evaluate(moving)).toBe(0);
        near(await sampled(page, start + 2, row), hovered, 1);
        expect(faded).toEqual(['250 ease-in-out']);
    });

    it(`adds ${color} at 17.5% over the hover layer of the share button's circle while pressed, ${pressed} on screen in the ${scheme} colour scheme`, async ({ page, open, armed, extension, toast, button }) => {
        await page.emulateMedia({ colorScheme: scheme, reducedMotion: 'reduce' });
        await open(`/pressed-${scheme}`, '<p>Pressed</p>');
        await armed(STUB.offered);
        await extension.act(page);
        await expect(button).toBeVisible();
        await settled(toast);
        const { start, row } = await circled(button);
        await button.hover();

        await page.mouse.down();

        near(await sampled(page, start + 2, row), pressed, 2);
        await page.mouse.move(0, 400);
        await page.mouse.up();
    });

    it(`draws a 2px ${ring} ring around the unfilled circle of the share button under keyboard focus in the ${scheme} colour scheme`, async ({ page, open, armed, extension, toast, button }) => {
        await page.emulateMedia({ colorScheme: scheme, reducedMotion: 'reduce' });
        await open(`/ringed-${scheme}`, '<input aria-label="Draft">');
        await armed(STUB.offered);
        await extension.act(page);
        await expect(button).toBeVisible();
        await settled(toast);
        const { start, row } = await circled(button);
        await page.getByRole('textbox', { name: 'Draft' }).focus();

        await page.keyboard.press('Tab');

        await expect(button).toBeFocused();
        expect(await button.evaluate((element) => getComputedStyle(element, '::before').outlineWidth)).toBe('2px');
        expect([await sampled(page, start - 4, row), await sampled(page, start + 2, row)]).toEqual([background, background]);
        expect([await sampled(page, start - 2, row), await sampled(page, start - 1, row)]).toContain(ring);
    });
});

it("paints the dark fill #ecf3fe under text #1f1f1f of Chrome's toast in an Incognito window under a light colour scheme", async ({ context, built, extension }) => {
    const devtools = await context.browser()?.newBrowserCDPSession();
    const [reloaded, opened] = [context.waitForEvent('serviceworker'), context.waitForEvent('page')];
    await devtools?.send('Extensions.loadUnpacked', { path: built.folder, enableInIncognito: true });
    const worker = await reloaded;
    await context.route('https://page.test/private', (route) => route.fulfill({ contentType: 'text/html', body: '<!doctype html><title>Private</title><p>Private</p>' }));
    await worker.evaluate(() => browser.windows.create({ incognito: true, url: 'about:blank' }));
    const page = await opened;
    await page.emulateMedia({ colorScheme: 'light' });
    await page.goto('https://page.test/private');

    await extension.act(page);

    await expect(page.getByRole('status')).toBeVisible();
    await settled(page.getByRole('status'));
    expect(await page.getByRole('status').evaluate(painted)).toEqual(['#ecf3fe', '#1f1f1f']);
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
    const drawn = await toast.evaluate(painted);
    expect(drawn.map((tone) => `${tone}ff`)).toEqual(tokens);
});

it('paints the CanvasText system colour under Canvas text under prefers-contrast more', async ({ page, open, extension, toast }) => {
    await page.emulateMedia({ contrast: 'more' });
    await open('/contrast', '<p style="color: CanvasText; background-color: Canvas">System</p>');
    const system = await page.getByText('System').evaluate((element) => [getComputedStyle(element).color, getComputedStyle(element).backgroundColor] as const);

    await extension.act(page);

    await expect(toast).toHaveCSS('background-color', system[0]);
    await expect(toast).toHaveCSS('color', system[1]);
});

it('draws a solid border under forced colours and none without them', async ({ page, open, extension, toast }) => {
    await open('/forced', '<p>Forced</p>');
    await extension.act(page);
    await expect(toast).toHaveCSS('border-block-start-style', 'none');

    await page.emulateMedia({ forcedColors: 'active' });

    await expect(toast).toHaveCSS('border-block-start-style', 'solid');
});

it('draws its link icon in CanvasText and its share glyph in ButtonText under forced colours', async ({ page, open, armed, extension, toast, button }) => {
    await open('/icons', '<p style="color: CanvasText">Link</p><p style="color: ButtonText">Share</p>');
    await armed(STUB.offered);
    await page.emulateMedia({ forcedColors: 'active' });
    const system = await page.locator('p').evaluateAll((marks) => marks.map((mark) => getComputedStyle(mark).color));

    await extension.act(page);

    await expect(button).toBeVisible();
    expect([await toast.evaluate((element) => getComputedStyle(element, '::before').backgroundColor), await button.evaluate((element) => getComputedStyle(element, '::after').backgroundColor)]).toEqual(system);
});

it("enters over 300ms and leaves over 150ms on Chrome's toast curves", async ({ page, context, open, extension }) => {
    await open('/motion', '<p>Motion</p>', REFUSED);
    const session = await context.newCDPSession(page);
    const moved: string[] = [];
    session.on('Animation.animationStarted', ({ animation: { type, name, source } }) => {
        moved.push(...(type === 'CSSTransition' && (name === 'translate' || name === 'scale') ? [`${name} ${source?.duration} ${source?.easing}`] : []));
    });
    await session.send('Animation.enable');
    await session.send('Animation.setPlaybackRate', { playbackRate: RATE });

    await extension.act(page);

    await expect(page.locator('mezrab-toast')).toHaveCount(1);
    await expect(page.locator('mezrab-toast')).toHaveCount(0);
    expect(moved.toSorted()).toEqual(['scale 150 cubic-bezier(0.3, 0, 0.8, 0.15)', 'scale 300 cubic-bezier(0.05, 0.7, 0.1, 1)', 'translate 150 cubic-bezier(0.3, 0, 0.8, 0.15)', 'translate 300 cubic-bezier(0.05, 0.7, 0.1, 1)']);
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
        { offer: 'without the share button', stub: STUB.refused, hold: 4000 },
        { offer: 'with the share button', stub: STUB.offered, hold: 8000 },
    ] as const
).forEach(({ offer, stub, hold }) =>
    it(`stays open through ${hold - 1}ms of its hold and leaves the page at ${hold}ms ${offer}`, async ({ page, open, armed, extension, toast }) => {
        await open(`/held-${hold}`, '<p>Held</p>');
        await armed(stub);
        await extension.act(page);
        await expect(toast).toBeVisible();
        await frozen(toast);

        await sought(toast, hold - 1);
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
    await expect(page.locator('mezrab-toast')).toHaveCount(1);

    await page.emulateMedia({ reducedMotion: 'reduce' });

    await expect(page.locator('mezrab-toast')).toHaveCount(0, { timeout: 1000 });
});

it('keeps one toast and restarts its hold when a second action finds it 2000ms into the hold', async ({ page, open, extension, toast }) => {
    await open('/twice', '<p>Twice</p>', REFUSED);
    await extension.act(page);
    await expect(toast).toBeVisible();
    await sought(toast, 2000);

    await extension.act(page);

    await expect.poll(() => held(toast).then(([hold]) => (hold?.at ?? Number.NaN) < 1000)).toBe(true);
    await expect(page.locator('mezrab-toast')).toHaveCount(1);
});

it('announces Link copied from the toast on each action', async ({ page, open, extension, isolated }) => {
    await open('/announced', '<p>Announced</p>');
    await isolated("globalThis.notified = []; Document.prototype.ariaNotify = Element.prototype.ariaNotify = function (text) { globalThis.notified.push([this instanceof Element ? this.role ?? this.localName : 'document', text]); };");

    await extension.act(page);

    await expect.poll(() => extension.evaluate(page, 'globalThis.notified')).toEqual([['output', 'Link copied']]);
});

it('stays visible in its own case under page CSS that unsets every element and hides and uppercases the root', async ({ page, open, extension, toast }) => {
    await open('/unset', '<style>* { all: unset !important } :root { visibility: hidden !important; text-transform: uppercase !important }</style><p>Unset</p>');

    await extension.act(page);

    await expect(toast).toBeVisible();
    await expect(toast).toHaveText('Link copied', { useInnerText: true });
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

    await expect(toast).toHaveText('Link copied');
    expect(await page.evaluate(() => document.readyState)).toBe('loading');
});
