import { expect } from '@playwright/test';
import { Function } from 'effect';
import { ended, it, STUB, settled, styled } from '#/fixtures.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFUSED = { 'permissions-policy': 'web-share=()' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it('shows Link copied as a 48px pill centred 6px below the top, its 20px icon 14px from the start and 6px before 13px system text that ends 16px from the end', async ({ page, open, extension, toast, button }) => {
    await open('/article', '<p>Article</p>', REFUSED);

    await extension.act(page);

    await expect(toast).toHaveText('Link copied');
    await expect(button).toHaveCount(0);
    await settled(toast);
    const geometry = await toast.evaluate((element) => {
        const [box, text] = [element.getBoundingClientRect(), document.createRange()];
        text.selectNodeContents(element);
        const { left, right } = text.getBoundingClientRect();
        return { top: box.top, centre: box.left + box.width / 2 - innerWidth / 2, height: box.height, start: left - box.left, end: box.right - right };
    });
    expect(geometry).toEqual({ top: 6, centre: expect.closeTo(0, 0), height: 48, start: expect.closeTo(40, 0), end: expect.closeTo(16, 0) });
    expect(await styled(toast)).toMatchObject({ font: ['system-ui', '13px', '400', '24px'] });
    expect(await styled(toast, '::before')).toMatchObject({ width: '20px', height: '20px' });
});

(
    [
        { scheme: 'light', background: 'lab(20 0 0)', color: 'lab(95 0 0)' },
        { scheme: 'dark', background: 'lab(90 0 0)', color: 'lab(10 0 0)' },
    ] as const
).forEach(({ scheme, background, color }) =>
    it(`paints Chrome's neutral inverse surface ${background} under text ${color} in the ${scheme} colour scheme`, async ({ page, open, extension, toast }) => {
        await page.emulateMedia({ colorScheme: scheme });
        await open(`/${scheme}`, '<p>Scheme</p>');

        await extension.act(page);

        await expect(toast).toBeVisible();
        await settled(toast);
        expect(await styled(toast)).toMatchObject({ background, color, border: ['0px', 'none'] });
    }),
);

(
    [
        { scheme: 'light', background: 'rgb(0, 0, 0)', color: 'rgb(255, 255, 255)' },
        { scheme: 'dark', background: 'rgb(255, 255, 255)', color: 'rgb(18, 18, 18)' },
    ] as const
).forEach(({ scheme, background, color }) =>
    it(`paints CanvasText ${background} under Canvas ${color} text under prefers-contrast more in the ${scheme} colour scheme`, async ({ page, open, extension, toast }) => {
        await page.emulateMedia({ colorScheme: scheme, contrast: 'more' });
        await open(`/contrast-${scheme}`, '<p>Contrast</p>');

        await extension.act(page);

        await expect(toast).toBeVisible();
        await settled(toast);
        expect(await styled(toast)).toMatchObject({ background, color });
    }),
);

it('draws a 1px solid border under forced colours', async ({ page, open, extension, toast }) => {
    await page.emulateMedia({ forcedColors: 'active' });
    await open('/forced', '<p>Forced</p>');

    await extension.act(page);

    await expect(toast).toBeVisible();
    expect(await styled(toast)).toMatchObject({ border: ['1px', 'solid'] });
});

it("enters over 300ms and leaves over 150ms on Chrome's emphasized curves", async ({ page, open, extension, lifecycle }) => {
    await open('/motion', '<p>Motion</p>', REFUSED);
    const watching = await lifecycle();

    await extension.act(page);

    const { entry, exit } = await ended(watching);
    expect(entry).toEqual(
        expect.arrayContaining([
            { property: 'translate', duration: 300, delay: 0, easing: 'cubic-bezier(0.05, 0.7, 0.1, 1)' },
            { property: 'scale', duration: 300, delay: 0, easing: 'cubic-bezier(0.05, 0.7, 0.1, 1)' },
        ]),
    );
    expect(exit).toEqual(
        expect.arrayContaining([
            { property: 'translate', duration: 150, delay: 0, easing: 'cubic-bezier(0.3, 0, 0.8, 0.15)' },
            { property: 'scale', duration: 150, delay: 0, easing: 'cubic-bezier(0.3, 0, 0.8, 0.15)' },
        ]),
    );
});

(
    [
        { offer: 'without the share button', stub: STUB.refused, buttons: 0, hold: 4000 },
        { offer: 'with the share button', stub: STUB.offered, buttons: 1, hold: 8000 },
    ] as const
).forEach(({ offer, stub, buttons, hold }) =>
    it(`closes ${hold}ms after it opens ${offer}`, async ({ page, open, armed, extension, button, lifecycle }) => {
        await open('/held', '<p>Held</p>');
        await armed(stub);
        const watching = await lifecycle();

        await extension.act(page);

        await expect(button).toHaveCount(buttons);
        const { opened, closed } = await ended(watching);
        expect(closed - opened).toBeGreaterThanOrEqual(hold - 50);
        expect(closed - opened).toBeLessThanOrEqual(hold + 250);
    }),
);

it('runs no transition on entry or exit under prefers-reduced-motion', async ({ page, open, extension, lifecycle }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await open('/still', '<p>Still</p>', REFUSED);
    const watching = await lifecycle();

    await extension.act(page);

    const { entry, exit } = await ended(watching);
    expect({ entry, exit }).toEqual({ entry: [], exit: [] });
});

it('announces Link copied through ariaNotify on each action', async ({ page, open, extension, isolated }) => {
    await open('/announced', '<p>Announced</p>');
    await isolated('globalThis.notified = []; document.ariaNotify = (...given) => globalThis.notified.push(given);');

    await extension.act(page);

    await expect.poll(() => extension.evaluate(page, 'globalThis.notified')).toEqual([['Link copied']]);
});

it('keeps one toast and restarts its 4s hold when a second action 2s later finds it showing', async ({ page, open, extension, toast, lifecycle }) => {
    await open('/twice', '<p>Twice</p>', REFUSED);
    const watching = await lifecycle();
    await extension.act(page);
    await expect(toast).toBeVisible();
    await page.waitForTimeout(2000);

    await extension.act(page);

    await expect(page.locator('mezrab-toast')).toHaveCount(1);
    const { opened, closed } = await ended(watching);
    expect(closed - opened).toBeGreaterThanOrEqual(6000 - 50);
    expect(closed - opened).toBeLessThanOrEqual(6000 + 500);
});

it('cancels page zoom, keeping 48 device-independent pixels of height at 200% zoom', async ({ page, open, extension, toast }) => {
    await open('/zoomed', '<p>Zoomed</p>');
    await extension.worker.evaluate(() => chrome.tabs.query({ active: true, lastFocusedWindow: true }).then(([tab]) => chrome.tabs.setZoom(tab?.id ?? chrome.tabs.TAB_ID_NONE, 2)));
    await expect.poll(() => page.evaluate(() => devicePixelRatio)).toBe(2);

    await extension.act(page);

    await expect(toast).toBeVisible();
    await settled(toast);
    expect(await toast.evaluate((element) => [element.getBoundingClientRect().height * devicePixelRatio, element.getBoundingClientRect().top * devicePixelRatio])).toEqual([48, 6]);
});

it('stays visible in its own case under page CSS that unsets every element and hides and uppercases the root', async ({ page, open, extension, toast }) => {
    await open('/unset', '<style>* { all: unset !important } :root { visibility: hidden !important; text-transform: uppercase !important }</style><p>Unset</p>');

    await extension.act(page);

    await expect(toast).toBeVisible();
    await expect(toast).toHaveText('Link copied', { useInnerText: true });
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
