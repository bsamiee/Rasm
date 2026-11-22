import { expect, type BrowserContext, type Page } from '@playwright/test';
import messages from '@rasm/mezrab/_locales/en/messages.json' with { type: 'json' };
import type { Extension } from '@rasm/testing/playwright';
import { it } from '#/fixtures.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BEFORE = 'https://page.test/before';

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

(
    [
        { tab: 'a chrome:// page, which activeTab never opens', reason: 'unreadable', badge: '✕', clipboard: BEFORE, reach: ({ page }: { readonly page: Page }) => page.goto('chrome://version/') },
        {
            tab: 'a page whose clipboard refuses the link',
            reason: 'uncopied',
            badge: '✕',
            clipboard: BEFORE,
            reach: ({ page, extension }: { readonly page: Page; readonly extension: Extension }) =>
                page.reload().then(() =>
                    extension.worker.evaluate(() => {
                        navigator.clipboard.writeText = () => Promise.reject(new DOMException('Write refused', 'NotAllowedError'));
                    }),
                ),
        },
        {
            tab: 'an SVG document, which takes no toast',
            reason: 'unconfirmed',
            badge: '✓',
            clipboard: 'https://image.test/mark.svg',
            reach: ({ page, context }: { readonly page: Page; readonly context: BrowserContext }) =>
                context.route('https://image.test/mark.svg', (route) => route.fulfill({ contentType: 'image/svg+xml', body: '<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96"><circle cx="48" cy="48" r="40"/></svg>' })).then(() => page.goto('https://image.test/mark.svg')),
        },
    ] as const
).forEach(({ tab, reason, badge, clipboard, reach }) =>
    it(`leaves ${clipboard} on the clipboard and marks the toolbar button with ${badge} and the ${reason} reason for 4s on ${tab}`, async ({ page, context, open, copied, extension }) => {
        await open('/before', '<p>Before</p>');
        await extension.act(page);
        await expect.poll(copied).toBe(BEFORE);
        await reach({ page, context, extension });

        await extension.act(page);

        await expect.poll(() => extension.toolbar(page)).toEqual({ badge, title: messages[reason].message });
        await expect(page.locator('mezrab-toast')).toHaveCount(0);
        await expect.poll(() => extension.toolbar(page), { timeout: 8000 }).toEqual({ badge: '', title: messages.action.message });
        await open('/reader', '<p>Reader</p>');
        expect(await copied()).toBe(clipboard);
    }),
);
