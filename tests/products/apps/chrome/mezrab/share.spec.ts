import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { expect, type Locator, type Page } from '@playwright/test';
import { Effect, Record } from 'effect';
import { ended, it, pixel, SETTLE, STUB, settled } from '#/fixtures.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Rgb = readonly number[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PAGE = {
    path: '/articles/ledger?sort=desc#totals',
    url: 'https://page.test/articles/ledger?sort=desc#totals',
    title: 'Page',
    fields: '<input aria-label="Other"><input aria-label="Draft">',
    under: '<button style="position: fixed; inset: 0 0 auto; block-size: 96px" onclick="this.dataset.hits = Number(this.dataset.hits ?? 0) + 1">Under</button>',
} as const;

const GLYPHS = Record.map(
    {
        ios_share: import.meta.resolve('@material-symbols/svg-400/outlined/ios_share.svg'),
        share_windows: import.meta.resolve('@material-symbols/svg-400/outlined/share_windows.svg'),
        share: import.meta.resolve('@material-symbols/svg-400/outlined/share.svg'),
    },
    (file) => `data:image/svg+xml;base64,${readFileSync(fileURLToPath(file)).toString('base64')}`,
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const clock = (page: Page) => page.evaluate(() => Number(document.timeline.currentTime));

const boxed = (locator: Locator) =>
    Effect.flatMap(
        Effect.promise(() => locator.boundingBox()),
        Effect.fromNullishOr,
    );

const srgb = (toast: Locator) =>
    toast.evaluate((element) => {
        const pen = new OffscreenCanvas(1, 1).getContext('2d');
        Object.assign(pen ?? {}, { fillStyle: getComputedStyle(element).color });
        pen?.fillRect(0, 0, 1, 1);
        return [...(pen?.getImageData(0, 0, 1, 1).data.subarray(0, 3) ?? [])];
    });

const mixed = (base: Rgb, content: Rgb, share: number) => base.map((channel, index) => channel + share * ((content[index] ?? channel) - channel));

const near = (got: Rgb, want: Rgb) =>
    expect(
        got.map((channel, index) => Math.abs(channel - (want[index] ?? Number.NaN))).every((gap) => gap <= 3),
        `got rgb(${got.join(' ')}), want rgb(${want.map(Math.round).join(' ')})`,
    ).toBe(true);

const circled = (toast: Locator) =>
    Effect.runPromise(
        Effect.map(boxed(toast), (box) => {
            const x = box.x + box.width - 24;
            const y = box.y + 24;
            const column = Math.floor(x);
            return {
                base: [column, Math.ceil(box.y + 2)],
                layer: [column, Math.ceil(y + 11)],
                corner: [Math.ceil(x + 12), Math.ceil(y + 12)],
                rightIn: [Math.ceil(x + 14), Math.floor(y)],
                rightOut: [Math.ceil(x + 16), Math.floor(y)],
                topIn: [column, Math.ceil(y - 16)],
                topOut: [column, Math.ceil(y - 16) - 1],
                ringIn: [column, Math.ceil(y + 15)],
                ringOut: [column, Math.ceil(y + 17)],
                center: { x, y },
            } as const;
        }),
    );

const sampled = (page: Page, [x, y]: readonly [number, number]) => pixel(page, x, y);

const shot = (page: Page, clip: { readonly x: number; readonly y: number; readonly width: number; readonly height: number }) => page.screenshot({ clip, scale: 'css' }).then((png) => png.toString('base64'));

const difference = async ([left, right]: readonly [string, string]) => {
    const bitmaps = await Array.fromAsync([left, right], (png) => createImageBitmap(new Blob([Uint8Array.fromBase64(png)], { type: 'image/png' })));
    const [a, b] = bitmaps.map((bitmap) => {
        const pen = new OffscreenCanvas(bitmap.width, bitmap.height).getContext('2d');
        pen?.drawImage(bitmap, 0, 0);
        return pen?.getImageData(0, 0, bitmap.width, bitmap.height).data ?? new Uint8ClampedArray();
    });
    const gaps = [...(a ?? [])].flatMap((value, index) => (index % 4 === 3 ? [] : [Math.abs(value - (b?.[index] ?? 0))]));
    return gaps.reduce((sum, gap) => sum + gap, 0) / Math.max(gaps.length, 1);
};

const reference = ({ x, y, glyph, color, background }: { readonly x: number; readonly y: number; readonly glyph: string; readonly color: string; readonly background: string }) => {
    const plate = Object.assign(document.createElement('div'), { id: 'reference' });
    const mark = document.createElement('div');
    document.getElementById('reference')?.remove();
    Object.assign(plate.style, { position: 'fixed', inset: '0', background });
    Object.assign(mark.style, { position: 'fixed', left: `${x - 10}px`, top: `${y - 10}px`, width: '20px', height: '20px', background: color, mask: `url("${glyph}") center / 20px no-repeat` });
    plate.append(mark);
    document.body.append(plate);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

(
    [
        { tab: 'an https tab where canShare is true', path: PAGE.path, stub: STUB.offered, buttons: 1 },
        { tab: 'an http tab where canShare is true', path: 'http://page.test/plain', stub: STUB.offered, buttons: 1 },
        { tab: 'an https tab where canShare is true only for the tab URL and title', path: PAGE.path, stub: STUB.matched, buttons: 1 },
        { tab: 'an https tab where canShare is false', path: PAGE.path, stub: STUB.refused, buttons: 0 },
        { tab: 'an https tab where navigator.share and canShare are missing', path: PAGE.path, stub: STUB.missing, buttons: 0 },
        { tab: 'an https tab where navigator.share alone is missing', path: PAGE.path, stub: STUB.unshareable, buttons: 0 },
    ] as const
).forEach(({ tab, path, stub, buttons }) =>
    it(`shows ${buttons} share buttons on ${tab}`, async ({ page, open, armed, extension, toast, button }) => {
        await open(path, PAGE.fields);
        await armed(stub);

        await extension.act(page);

        await expect(toast).toBeVisible();
        await expect(button).toHaveCount(buttons);
    }),
);

it('shows no share button on a file: tab even where canShare is true', async ({ page, context, extension, armed, toast, button }, info) => {
    const file = info.outputPath('ledger.html');
    writeFileSync(file, '<!doctype html><title>Ledger</title><input aria-label="Draft">');
    const settings = await context.newPage();
    await settings.goto('chrome://extensions');
    await settings.evaluate(`chrome.developerPrivate.updateExtensionConfiguration({ extensionId: '${new URL(extension.worker.url()).host}', fileAccess: true })`);
    await settings.close();
    await page.goto(pathToFileURL(file).href);
    await armed(STUB.offered);

    await extension.act(page);

    await expect(toast).toBeVisible();
    await expect(button).toHaveCount(0);
});

(
    [
        { press: 'a click', act: (button: Locator) => button.click() },
        { press: 'Enter', act: (button: Locator) => button.press('Enter') },
        { press: 'Space', act: (button: Locator) => button.press(' ') },
    ] as const
).forEach(({ press, act }) =>
    it(`shares the tab URL and title once on ${press} on the Share link button`, async ({ page, open, armed, extension, toast, shares }) => {
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await extension.act(page);

        await act(toast.getByRole('button', { name: 'Share link' }));

        await expect.poll(shares).toEqual([{ url: PAGE.url, title: PAGE.title }]);
    }),
);

it('shares the link alone from a page that sets no title', async ({ page, context, armed, extension, toast, shares }) => {
    await context.route('https://page.test/untitled', (route) => route.fulfill({ contentType: 'text/html', body: '<!doctype html><input aria-label="Draft">' }));
    await page.goto('https://page.test/untitled');
    await armed(STUB.offered);
    await extension.act(page);

    await toast.getByRole('button', { name: 'Share link' }).click();

    await expect.poll(shares).toEqual([{ url: 'https://page.test/untitled' }]);
});

it('keeps 4px from each side of a 120px viewport too narrow for the pill, cutting the text and keeping the whole 44px share button at its end', async ({ page, open, armed, extension, toast, button }) => {
    await page.setViewportSize({ width: 120, height: 400 });
    await open('/narrow', '<p>Narrow</p>');
    await armed(STUB.offered);

    await extension.act(page);

    await expect(button).toBeVisible();
    await settled(toast);
    const geometry = await toast.evaluate((element) => {
        const [pill, action] = [element.getBoundingClientRect(), element.querySelector('button')?.getBoundingClientRect()];
        return { start: pill.left, end: innerWidth - pill.right, width: action?.width, after: pill.right - (action?.right ?? 0) };
    });
    expect(geometry).toEqual({ start: 4, end: 4, width: 44, after: 0 });
});

(
    [
        { cause: 'hover', enter: (_page: Page, button: Locator) => button.hover(), leave: (page: Page) => page.mouse.move(0, 400) },
        { cause: 'keyboard focus', enter: (page: Page) => page.keyboard.press('Tab'), leave: (page: Page) => page.keyboard.press('Shift+Tab') },
    ] as const
).forEach(({ cause, enter, leave }) =>
    it(`pauses the 8000ms hold for as long as ${cause} rests on the share button`, async ({ page, open, armed, extension, button, lifecycle }) => {
        it.slow();
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await page.getByRole('textbox', { name: 'Draft' }).focus();
        const watching = await lifecycle();
        await extension.act(page);
        await expect(button).toBeVisible();
        await page.waitForTimeout(2000);

        await enter(page, button);
        const entered = await clock(page);
        await page.waitForTimeout(4000);
        await leave(page);
        const left = await clock(page);

        const { opened, closed } = await ended(watching);
        expect(left - entered).toBeGreaterThanOrEqual(3900);
        expect(closed - opened - (left - entered)).toBeGreaterThanOrEqual(8000 - 300);
        expect(closed - opened - (left - entered)).toBeLessThanOrEqual(8000 + 300);
    }),
);

it('holds past 8000ms while a share it started is pending, then closes once the share resolves', async ({ page, open, armed, extension, toast, button, shares, settle }) => {
    it.slow();
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);

    await button.click();
    await page.mouse.move(0, 400);
    await page.getByRole('textbox', { name: 'Other' }).focus();
    await expect.poll(shares).toHaveLength(1);
    await page.waitForTimeout(9000);

    await expect(toast).toBeVisible();
    await settle(SETTLE.resolved);
    await expect(toast).toBeHidden({ timeout: 1000 });
});

it('leaves the toast open and Escape to the page while focus rests outside the toast', async ({ page, open, armed, extension, toast, button }) => {
    await open(PAGE.path, `${PAGE.fields}<script>addEventListener('keydown', ({ key }) => { document.body.dataset.key = key; })</script>`);
    await armed(STUB.offered);
    await extension.act(page);
    await expect(button).toBeVisible();

    await page.getByRole('textbox', { name: 'Draft' }).press('Escape');

    await expect(page.locator('body')).toHaveAttribute('data-key', 'Escape');
    expect(await toast.evaluate((element) => element.matches(':popover-open'))).toBe(true);
});

(
    [
        { dismissal: 'Escape', dismiss: (page: Page) => page.keyboard.press('Escape') },
        { dismissal: 'Enter then a resolved share', dismiss: (page: Page, settle: (outcome: string) => Promise<unknown>) => page.keyboard.press('Enter').then(() => settle(SETTLE.resolved)) },
        { dismissal: 'Enter then a rejected share', dismiss: (page: Page, settle: (outcome: string) => Promise<unknown>) => page.keyboard.press('Enter').then(() => settle(SETTLE.rejected)) },
    ] as const
).forEach(({ dismissal, dismiss }) =>
    it(`closes and returns focus to the field focused before the share button on ${dismissal}`, async ({ page, open, armed, extension, toast, button, settle }) => {
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await extension.act(page);
        await page.getByRole('textbox', { name: 'Draft' }).focus();
        await page.keyboard.press('Tab');
        await expect(button).toBeFocused();

        await dismiss(page, settle);

        await expect(toast).toBeHidden();
        await expect(page.getByRole('textbox', { name: 'Draft' })).toBeFocused();
    }),
);

it('leaves focus on the page itself when focus reached the share button from outside the page and the toast closes on Escape', async ({ page, open, armed, extension, toast, button }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);
    await button.focus();

    await page.keyboard.press('Escape');

    await expect(toast).toBeHidden();
    expect(await page.evaluate(() => document.activeElement === document.body)).toBe(true);
});

it('leaves focus where it is when the toast closes with focus outside it', async ({ page, open, armed, extension, toast, button }) => {
    it.slow();
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);
    await page.getByRole('textbox', { name: 'Draft' }).focus();
    await page.keyboard.press('Tab');
    await expect(button).toBeFocused();

    await page.getByRole('textbox', { name: 'Other' }).click();

    await expect(toast).toBeHidden({ timeout: 12_000 });
    await expect(page.getByRole('textbox', { name: 'Other' })).toBeFocused();
});

it('passes clicks on the pill through to the page and takes clicks on the share button', async ({ page, open, armed, extension, toast, button, shares }) => {
    await open(PAGE.path, PAGE.under);
    await armed(STUB.offered);
    await extension.act(page);
    await expect(button).toBeVisible();
    await settled(toast);
    const [pill, action] = await Effect.runPromise(Effect.all([boxed(toast), boxed(button)]));

    await page.mouse.click(pill.x + (action.x - pill.x) / 2, pill.y + pill.height / 2);
    await expect(page.getByRole('button', { name: 'Under' })).toHaveAttribute('data-hits', '1');
    expect(await shares()).toEqual([]);
    await page.mouse.click(action.x + action.width / 2, action.y + action.height / 2);

    await expect.poll(shares).toHaveLength(1);
    await expect(page.getByRole('button', { name: 'Under' })).toHaveAttribute('data-hits', '1');
});

(
    [
        { state: 'at rest', layer: 0, enter: (page: Page) => page.mouse.move(0, 400) },
        { state: 'under hover', layer: 0.08, enter: (page: Page, center: { readonly x: number; readonly y: number }) => page.mouse.move(center.x, center.y) },
        { state: 'while the pointer presses it', layer: 0.1, enter: (page: Page, center: { readonly x: number; readonly y: number }) => page.mouse.move(center.x, center.y).then(() => page.mouse.down()) },
        { state: 'under keyboard focus', layer: 0.1, enter: (page: Page) => page.getByRole('textbox', { name: 'Draft' }).press('Tab') },
    ] as const
).forEach(({ state, layer, enter }) =>
    it(`draws a ${layer * 100}% state layer over the share glyph ${state}`, async ({ page, open, armed, extension, toast, button }) => {
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await extension.act(page);
        await expect(button).toBeVisible();
        await settled(toast);
        const at = await circled(toast);
        const [base, content] = [await sampled(page, at.base), await srgb(toast)];

        await enter(page, at.center);
        await settled(toast);

        near(await sampled(page, at.layer), mixed(base, content, layer));
    }),
);

it('bounds the hover layer by a 32px circle inset 8px from the end of the pill', async ({ page, open, armed, extension, toast, button }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);
    await expect(button).toBeVisible();
    await settled(toast);
    const at = await circled(toast);
    const [base, content] = [await sampled(page, at.base), await srgb(toast)];

    await page.mouse.move(at.center.x, at.center.y);
    await settled(toast);

    const layer = mixed(base, content, 0.08);
    near(await sampled(page, at.rightIn), layer);
    near(await sampled(page, at.topIn), layer);
    near(await sampled(page, at.rightOut), base);
    near(await sampled(page, at.topOut), base);
    near(await sampled(page, at.corner), base);
});

it('rings the circle edge with a 2px line under keyboard focus', async ({ page, open, armed, extension, toast, button }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);
    await expect(button).toBeVisible();
    await settled(toast);
    const at = await circled(toast);
    const [base, content] = [await sampled(page, at.base), await srgb(toast)];

    await page.getByRole('textbox', { name: 'Draft' }).press('Tab');
    await expect(button).toBeFocused();
    await settled(toast);

    near(await sampled(page, at.ringIn), content);
    near(await sampled(page, at.ringOut), base);
});

(
    [
        { os: 'mac', glyph: 'ios_share' },
        { os: 'win', glyph: 'share_windows' },
        { os: 'cros', glyph: 'share' },
        { os: 'linux', glyph: 'share' },
    ] as const
).forEach(({ os, glyph }) =>
    it(`draws the 20px ${glyph} glyph on ${os}`, async ({ page, open, armed, extension, toast, button }) => {
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await extension.worker.evaluate((platform) => {
            chrome.runtime.getPlatformInfo = () => Promise.resolve({ os: platform, arch: 'x86-64', nacl_arch: 'x86-64' });
        }, os);

        await extension.act(page);

        await expect(button).toBeVisible();
        await settled(toast);
        const { center } = await circled(toast);
        const clip = { x: Math.floor(center.x) - 12, y: Math.floor(center.y) - 12, width: 24, height: 24 };
        const [color, background] = await toast.evaluate((element) => [getComputedStyle(element).color, getComputedStyle(element).backgroundColor] as const);
        const shown = await shot(page, clip);
        await toast.evaluate((element) => (element instanceof HTMLElement ? element.hidePopover() : undefined));
        await expect(toast).toBeHidden();
        const gaps = await Array.fromAsync(Record.toEntries(GLYPHS), async ([name, drawn]) => {
            await page.evaluate(reference, { x: center.x, y: center.y, glyph: drawn, color, background });
            return [name, await page.evaluate(difference, [shown, await shot(page, clip)] as const)] as const;
        });
        const [closest, gap] = gaps.reduce((best, entry) => (entry[1] < best[1] ? entry : best));
        expect({ closest, below: gap < 4 }, `mean channel gap per glyph: ${JSON.stringify(gaps)}`).toEqual({ closest: glyph, below: true });
    }),
);
