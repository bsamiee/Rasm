import { Hermetic } from '@rasm/ts-testkit/e2e';
import { type BrowserContext, type Page, test as base, expect } from '@playwright/test';
import { Option } from 'effect';

// --- [TYPES] -----------------------------------------------------------------------------

type Kit = {
    readonly hermetic: { readonly open: (route: string) => Promise<void> };
    readonly pausedClock: Page['clock'];
    readonly duo: (route: string) => Promise<readonly [Page, Page]>;
    readonly webauthn: { readonly id: string; readonly remove: () => Promise<void> };
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const _EPOCH = new Date('2026-01-01T00:00:00.000Z');

// --- [OPERATIONS] ------------------------------------------------------------------------

// Hermetic serving: every context route on the kit origin fulfills from the page corpus — a 404 for
// a phantom path is the falsifiable miss, never a hang.
const _serve = (context: BrowserContext): Promise<void> =>
    context.route(`${Hermetic.origin}/**`, (route) =>
        Option.match(Hermetic.page(new URL(route.request().url()).pathname), {
            onNone: () => route.fulfill({ body: '', status: 404 }),
            onSome: (document) => route.fulfill({ body: document, contentType: 'text/html' }),
        }),
    );

// The one fixture tower: every platform capability is a row here, composed from kit data — never a
// helper beside a spec.
const test = base.extend<Kit>({
    duo: async ({ browser }, use) => {
        const opened: Array<BrowserContext> = [];
        await use(async (route) => {
            const open = async (): Promise<Page> => {
                const context = await browser.newContext();
                opened.push(context);
                await _serve(context);
                const page = await context.newPage();
                await page.goto(`${Hermetic.origin}${route}`);
                return page;
            };
            return [await open(), await open()] as const;
        });
        await Promise.all(opened.map((context) => context.close()));
    },
    hermetic: async ({ context, page }, use) => {
        await _serve(context);
        await use({
            open: async (route) => {
                await page.goto(`${Hermetic.origin}${route}`);
            },
        });
    },
    pausedClock: async ({ page }, use) => {
        await page.clock.install({ time: _EPOCH });
        await use(page.clock);
    },
    webauthn: async ({ browserName, page }, use) => {
        test.skip(browserName !== 'chromium', 'CDP WebAuthn is chromium-only');
        const cdp = await page.context().newCDPSession(page);
        await cdp.send('WebAuthn.enable');
        const { authenticatorId } = await cdp.send('WebAuthn.addVirtualAuthenticator', {
            options: {
                automaticPresenceSimulation: true,
                hasResidentKey: true,
                hasUserVerification: true,
                isUserVerified: true,
                protocol: 'ctap2',
                transport: 'internal',
            },
        });
        await use({
            id: authenticatorId,
            remove: async () => {
                await cdp.send('WebAuthn.removeVirtualAuthenticator', { authenticatorId });
            },
        });
    },
});

// --- [EXPORTS] ---------------------------------------------------------------------------

export { expect, test };
