import process from 'node:process';
import { type BrowserContext, expect, type Page } from '@playwright/test';
import messages from '@rasm/mezrab/_locales/en/messages.json' with { type: 'json' };
import type { Extension } from '@rasm/testing/playwright';
import { it } from '#/fixtures';

// --- [TYPES] ---------------------------------------------------------------------------

interface Reached {
    readonly tab: Page;
    readonly context: BrowserContext;
    readonly extension: Extension;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BEFORE = 'https://page.test/before';
const HOLD = 4000;
const SHORTCUTS: Partial<Record<NodeJS.Platform, string>> = { darwin: '⇧⌘U', linux: 'Alt+Shift+U', win32: 'Ctrl+Shift+U' };
const HIDDEN = { tab: 'a chrome:// page, which activeTab never opens', reason: 'unreadable', badge: '✕', clipboard: BEFORE, reach: ({ tab }: Reached) => tab.goto('chrome://version/') } as const;
const REFUSING = {
    tab: 'a page whose clipboard refuses the link',
    reason: 'uncopied',
    badge: '✕',
    clipboard: BEFORE,
    reach: ({ tab, extension }: Reached) =>
        tab.goto('https://page.test/refused').then(() =>
            extension.worker.evaluate(() => {
                navigator.clipboard.writeText = () => Promise.reject(new DOMException('Write refused', 'NotAllowedError'));
            }),
        ),
} as const;
const DRAWN = {
    tab: 'an SVG document that is the first page its tab shows, which takes no toast',
    reason: 'unconfirmed',
    badge: '✓',
    clipboard: 'https://image.test/mark.svg',
    reach: ({ tab, context }: Reached) => context.route('https://image.test/mark.svg', (route) => route.fulfill({ contentType: 'image/svg+xml', body: '<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96"><circle cx="48" cy="48" r="40"/></svg>' })).then(() => tab.goto('https://image.test/mark.svg')),
} as const;
const FAILURES = [HIDDEN, REFUSING, DRAWN] as const;
const CLEARED = { badge: '', title: messages.action.message } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const now = (extension: Extension) => extension.worker.evaluate(() => performance.timeOrigin + performance.now());

const due = (extension: Extension) => extension.worker.evaluate(() => browser.alarms.getAll().then((alarms) => alarms.map(({ scheduledTime }) => scheduledTime)));

const rung = (extension: Extension, minutes: number) => extension.worker.evaluate((delayInMinutes) => browser.alarms.getAll().then((alarms) => Promise.all(alarms.map(({ name }) => browser.alarms.create(name, { delayInMinutes })))), minutes);

// --- [COMPOSITION] ---------------------------------------------------------------------

it('copies the URL of the tab with its query, fragment, and non-ASCII characters as the URL standard serializes them', async ({ page, open, copied, extension }) => {
    await open('/café/naïve?q=ü&r=a b#ß', '<p>Article</p>');

    await extension.act(page);

    await expect.poll(copied).toBe('https://page.test/caf%C3%A9/na%C3%AFve?q=%C3%BC&r=a%20b#%C3%9F');
});

it('copies on the first action after its service worker stopped', async ({ page, open, copied, extension }) => {
    await open('/after-idle', '<p>Idle</p>');
    await extension.stop(page);

    await extension.act(page);

    await expect.poll(copied).toBe('https://page.test/after-idle');
});

it(`binds the action to ${SHORTCUTS[process.platform]} on ${process.platform}`, async ({ extension }) => {
    const commands = await extension.worker.evaluate(() => browser.commands.getAll());

    expect(commands.map(({ name, shortcut }) => ({ name, shortcut }))).toEqual([{ name: '_execute_action', shortcut: SHORTCUTS[process.platform] }]);
});

FAILURES.forEach(({ tab: shows, reason, badge, clipboard, reach }) =>
    it(`leaves ${clipboard} on the clipboard and marks the toolbar button with ${badge} and the ${reason} reason until an alarm ${HOLD}ms after the action on ${shows}`, async ({ page, context, open, copied, extension }) => {
        await open('/before', '<p>Before</p>');
        await extension.act(page);
        await expect.poll(copied).toBe(BEFORE);
        const tab = await context.newPage();
        await reach({ tab, context, extension });
        const acted = await now(extension);

        await extension.act(tab);

        await expect.poll(() => extension.toolbar(tab)).toEqual({ badge, title: messages[reason].message });
        await expect.poll(() => due(extension)).toHaveLength(1);
        const [[scheduled = Number.NaN], marked] = [await due(extension), await now(extension)];
        expect(scheduled).toBeGreaterThanOrEqual(acted + HOLD);
        expect(scheduled).toBeLessThanOrEqual(marked + HOLD);
        await expect(tab.locator('mezrab-toast')).toHaveCount(0);
        await rung(extension, 0);
        await expect.poll(() => extension.toolbar(tab)).toEqual(CLEARED);
        await page.bringToFront();
        expect(await copied()).toBe(clipboard);
    }),
);

it('clears the ✕ badge when its alarm wakes the service worker Chrome stopped', async ({ page, context, extension }) => {
    await HIDDEN.reach({ tab: page, context, extension });
    await extension.act(page);
    await expect.poll(() => extension.toolbar(page)).toMatchObject({ badge: HIDDEN.badge });
    await expect.poll(() => due(extension)).toHaveLength(1);
    const devtools = await context.browser()?.newBrowserCDPSession();
    const running = () => devtools?.send('Target.getTargets', { filter: [{ type: 'service_worker' }] }).then(({ targetInfos }) => targetInfos.filter(({ url }) => url === extension.worker.url()).length);
    await rung(extension, 1 / 60);

    await extension.stop(page);

    await expect.poll(running, { message: 'a service worker the alarm woke' }).toBe(1);
    await expect.poll(() => extension.toolbar(page)).toEqual(CLEARED);
});

it(`moves the ✕ badge alarm to ${HOLD}ms after a second failure that finds the first badge showing`, async ({ page, context, extension }) => {
    await HIDDEN.reach({ tab: page, context, extension });
    await extension.act(page);
    await expect.poll(() => extension.toolbar(page)).toMatchObject({ badge: HIDDEN.badge });
    await expect.poll(() => due(extension)).toHaveLength(1);
    const acted = await now(extension);

    await extension.act(page);

    await expect.poll(() => due(extension).then(([scheduled = Number.NaN]) => scheduled)).toBeGreaterThanOrEqual(acted + HOLD);
    const [[scheduled = Number.NaN], marked] = [await due(extension), await now(extension)];
    expect(scheduled).toBeLessThanOrEqual(marked + HOLD);
    expect(await extension.toolbar(page)).toMatchObject({ badge: HIDDEN.badge });
});
