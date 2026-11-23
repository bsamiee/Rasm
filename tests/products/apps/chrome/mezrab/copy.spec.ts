import process from 'node:process';
import { type BrowserContext, expect, type Page } from '@playwright/test';
import messages from '@rasm/mezrab/_locales/en/messages.json' with { type: 'json' };
import type { Extension } from '@rasm/testing/playwright';
import { it } from '#/fixtures';

// --- [TYPES] ---------------------------------------------------------------------------

interface Reached {
    readonly page: Page;
    readonly context: BrowserContext;
    readonly extension: Extension;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BEFORE = 'https://page.test/before';
const SHORTCUTS: Partial<Record<NodeJS.Platform, string>> = { darwin: '⇧⌘U', linux: 'Alt+Shift+U', win32: 'Ctrl+Shift+U' };
const HIDDEN = { tab: 'a chrome:// page, which activeTab never opens', reason: 'unreadable', badge: '✕', clipboard: BEFORE, reach: ({ page }: Reached) => page.goto('chrome://version/') } as const;
const REFUSING = {
    tab: 'a page whose clipboard refuses the link',
    reason: 'uncopied',
    badge: '✕',
    clipboard: BEFORE,
    reach: ({ page, extension }: Reached) =>
        page.reload().then(() =>
            extension.worker.evaluate(() => {
                navigator.clipboard.writeText = () => Promise.reject(new DOMException('Write refused', 'NotAllowedError'));
            }),
        ),
} as const;
const DRAWN = {
    tab: 'an SVG document, which takes no toast',
    reason: 'unconfirmed',
    badge: '✓',
    clipboard: 'https://image.test/mark.svg',
    reach: ({ page, context }: Reached) => context.route('https://image.test/mark.svg', (route) => route.fulfill({ contentType: 'image/svg+xml', body: '<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96"><circle cx="48" cy="48" r="40"/></svg>' })).then(() => page.goto('https://image.test/mark.svg')),
} as const;
const FAILURES = [HIDDEN, REFUSING, DRAWN] as const;

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

FAILURES.forEach(({ tab, reason, badge, clipboard, reach }) =>
    it(`leaves ${clipboard} on the clipboard and marks the toolbar button with ${badge} and the ${reason} reason for 4s on ${tab}`, async ({ page, context, open, copied, extension }) => {
        await open('/before', '<p>Before</p>');
        await extension.act(page);
        await expect.poll(copied).toBe(BEFORE);
        await reach({ page, context, extension });

        await extension.act(page);

        await expect.poll(() => extension.toolbar(page)).toEqual({ badge, title: messages[reason].message });
        await expect(page.locator('mezrab-toast')).toHaveCount(0);
        await page.waitForTimeout(3000);
        expect(await extension.toolbar(page)).toEqual({ badge, title: messages[reason].message });
        await expect.poll(() => extension.toolbar(page), { intervals: [100], timeout: 2500 }).toEqual({ badge: '', title: messages.action.message });
        await open('/reader', '<p>Reader</p>');
        expect(await copied()).toBe(clipboard);
    }),
);

it('keeps the copy and marks ✓ on an SVG document that is the first page its tab shows', async ({ page, context, extension }) => {
    await DRAWN.reach({ page, context, extension });

    await extension.act(page);

    await expect.poll(() => extension.toolbar(page)).toEqual({ badge: DRAWN.badge, title: messages[DRAWN.reason].message });
});

it('clears the ✕ badge 4s after the action though Chrome stopped the service worker meanwhile', async ({ page, context, extension }) => {
    await HIDDEN.reach({ page, context, extension });
    await extension.act(page);
    await expect.poll(() => extension.toolbar(page)).toMatchObject({ badge: HIDDEN.badge });

    await extension.stop(page);
    await page.waitForTimeout(4500);

    expect(await extension.toolbar(page)).toEqual({ badge: '', title: messages.action.message });
});

it('holds the ✕ badge 4s after a second failure that follows the first by 3s', async ({ page, context, extension }) => {
    await HIDDEN.reach({ page, context, extension });
    await extension.act(page);
    await expect.poll(() => extension.toolbar(page)).toMatchObject({ badge: HIDDEN.badge });
    await page.waitForTimeout(3000);

    await extension.act(page);
    await page.waitForTimeout(2000);

    expect(await extension.toolbar(page)).toMatchObject({ badge: HIDDEN.badge });
    await expect.poll(() => extension.toolbar(page), { intervals: [100], timeout: 4000 }).toMatchObject({ badge: '' });
});
