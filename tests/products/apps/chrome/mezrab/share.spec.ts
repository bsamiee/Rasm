import { readFileSync, writeFileSync } from 'node:fs';
import { basename } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { expect, type Locator, type Page } from '@playwright/test';
import { held, it, SETTLE, STUB, sought } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PAGE = {
    path: '/articles/ledger?sort=desc#totals',
    url: 'https://page.test/articles/ledger?sort=desc#totals',
    title: 'Page',
    fields: '<input aria-label="Other"><input aria-label="Draft">',
} as const;
const OUTLINE = /\sd=["'](?<outline>[^"']+)["']/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const outlined = (svg: string) => OUTLINE.exec(svg)?.groups?.['outline'];

// --- [COMPOSITION] ---------------------------------------------------------------------

(
    [
        { tab: 'an http tab where canShare is true', path: 'http://page.test/plain', stub: STUB.offered, buttons: 1 },
        { tab: 'an https tab where canShare is true only for the tab URL and title', path: PAGE.path, stub: STUB.matched, buttons: 1 },
        { tab: 'an https tab where canShare is false', path: PAGE.path, stub: STUB.refused, buttons: 0 },
        { tab: 'an https tab where navigator.share and canShare are missing, as on Linux', path: PAGE.path, stub: STUB.missing, buttons: 0 },
        { tab: 'an https tab where navigator.share alone is missing', path: PAGE.path, stub: STUB.unshareable, buttons: 0 },
    ] as const
).forEach(({ tab, path, stub, buttons }) =>
    it(`shows ${buttons} share buttons on ${tab}`, async ({ page, open, armed, extension, toast, button }) => {
        await open(path, PAGE.fields);
        await armed(stub);

        await extension.act(page);

        await expect(toast).toBeVisible();
        expect(await button.count()).toBe(buttons);
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
    expect(await button.count()).toBe(0);
});

(
    [
        { os: 'mac', drawing: import.meta.resolve('@material-symbols/svg-400/outlined/ios_share.svg') },
        { os: 'win', drawing: import.meta.resolve('@material-symbols/svg-400/outlined/share_windows.svg') },
        { os: 'linux', drawing: import.meta.resolve('@material-symbols/svg-400/outlined/share.svg') },
    ] as const
).forEach(({ os, drawing }) =>
    it(`draws the ${basename(drawing, '.svg')} glyph on the Share link button on ${os}`, async ({ page, open, armed, extension, button }) => {
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await extension.worker.evaluate((platform) => {
            browser.runtime.getPlatformInfo = () => Promise.resolve({ os: platform, arch: 'x86-64', nacl_arch: 'x86-64' });
        }, os);

        await extension.act(page);

        await expect(button).toBeVisible();
        const glyphed = await button.evaluate((element) => getComputedStyle(element).getPropertyValue('--glyph'));
        expect(outlined(decodeURIComponent(glyphed))).toBe(outlined(readFileSync(fileURLToPath(drawing), 'utf-8')));
    }),
);

it('shares the tab URL and title once on a click on the Share link button', async ({ page, open, armed, extension, toast, shares }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);

    await toast.getByRole('button', { name: 'Share link' }).click();

    await expect.poll(shares).toEqual([{ url: PAGE.url, title: PAGE.title }]);
});

it('shares the link alone from a page that sets no title', async ({ page, context, armed, extension, toast, shares }) => {
    await context.route('https://page.test/untitled', (route) => route.fulfill({ contentType: 'text/html', body: '<!doctype html><input aria-label="Draft">' }));
    await page.goto('https://page.test/untitled');
    await armed(STUB.offered);
    await extension.act(page);

    await toast.getByRole('button', { name: 'Share link' }).click();

    await expect.poll(shares).toEqual([{ url: 'https://page.test/untitled' }]);
});

(
    [
        { cause: 'hover', enter: (_page: Page, button: Locator) => button.hover(), leave: (page: Page) => page.mouse.move(0, 400) },
        { cause: 'keyboard focus', enter: (page: Page) => page.keyboard.press('Tab'), leave: (page: Page) => page.keyboard.press('Shift+Tab') },
    ] as const
).forEach(({ cause, enter, leave }) =>
    it(`pauses its hold while ${cause} rests on the share button and runs it again after`, async ({ page, open, armed, extension, toast, button }) => {
        await open(PAGE.path, PAGE.fields);
        await armed(STUB.offered);
        await page.getByRole('textbox', { name: 'Draft' }).focus();
        await extension.act(page);
        await expect(button).toBeVisible();
        await expect.poll(() => held(toast)).toMatchObject([{ state: 'running' }]);

        await enter(page, button);
        await expect.poll(() => held(toast)).toMatchObject([{ state: 'paused' }]);
        await leave(page);

        await expect.poll(() => held(toast)).toMatchObject([{ state: 'running' }]);
    }),
);

it('runs its hold on under the hover a tap leaves on a screen with no hovering pointer', async ({ page, context, open, armed, extension, toast, button }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    const session = await context.newCDPSession(page);
    await session.send('Emulation.setTouchEmulationEnabled', { enabled: true });
    await extension.act(page);
    await expect(button).toBeVisible();

    await button.hover();

    expect(await toast.evaluate((element) => [matchMedia('(hover: none)').matches, element.matches(':hover')])).toEqual([true, true]);
    await expect.poll(() => held(toast)).toMatchObject([{ state: 'running' }]);
});

it('stays open with its hold paused while a share it started is pending, then closes once the share resolves', async ({ page, open, armed, extension, toast, button, shares, settle }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);

    await button.click();
    await page.mouse.move(0, 400);
    await page.getByRole('textbox', { name: 'Other' }).focus();
    await expect.poll(shares).toHaveLength(1);

    await expect.poll(() => held(toast)).toMatchObject([{ state: 'paused' }]);
    await settle(SETTLE.resolved);
    await expect(toast).toBeHidden();
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

it('leaves focus where it is when its hold ends with focus outside the toast', async ({ page, open, armed, extension, toast, button }) => {
    await open(PAGE.path, PAGE.fields);
    await armed(STUB.offered);
    await extension.act(page);
    await page.getByRole('textbox', { name: 'Draft' }).focus();
    await page.keyboard.press('Tab');
    await expect(button).toBeFocused();
    await page.getByRole('textbox', { name: 'Other' }).click();

    await sought(toast, 8000);

    await expect(toast).toBeHidden();
    await expect(page.getByRole('textbox', { name: 'Other' })).toBeFocused();
});
