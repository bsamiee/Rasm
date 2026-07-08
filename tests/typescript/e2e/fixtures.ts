import { AxeBuilder } from '@axe-core/playwright';
import { type BrowserContext, test as base, expect, type Page } from '@playwright/test';
import { Hermetic } from '@rasm/ts-testkit/e2e';
import { Option } from 'effect';

// --- [TYPES] -----------------------------------------------------------------------------

type Violations = Awaited<ReturnType<AxeBuilder['analyze']>>['violations'];

type Kit = {
    readonly a11y: (scope?: string) => Promise<Violations>;
    readonly clock: Page['clock'];
    readonly cohort: (route: string, count: number) => Promise<ReadonlyArray<Page>>;
    readonly target: { readonly origin: string; readonly open: (route: string) => Promise<void> };
    readonly webauthn: { readonly id: string; readonly remove: () => Promise<void> };
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const _EPOCH = new Date('2026-01-01T00:00:00.000Z');

// The axe rule surface: the WCAG 2.x A/AA tags — the conformance floor every served page clears.
const _WCAG = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'] as const;

// --- [OPERATIONS] ------------------------------------------------------------------------

// Hermetic serving: every context route on the kit origin fulfills from the page corpus — a 404 for
// a phantom path is the falsifiable miss, never a hang.
const _serve = async (context: BrowserContext): Promise<void> => {
    await context.route(`${Hermetic.origin}/**`, (route) =>
        Option.match(Hermetic.page(new URL(route.request().url()).pathname), {
            onNone: () => route.fulfill({ body: '', status: 404 }),
            onSome: (document) => route.fulfill({ body: document, contentType: 'text/html' }),
        }),
    );
};

// The target discriminant is baseURL presence: a served product project carries its origin as
// baseURL and rides the real server; an unset baseURL is the hermetic row, armed in-context.
const _arm = async (context: BrowserContext, baseURL: string | undefined): Promise<string> => {
    if (baseURL === undefined) {
        await _serve(context);
        return Hermetic.origin;
    }
    return baseURL;
};

// The one fixture tower: every platform capability is a row here, composed from kit data — never a
// helper beside a spec. Multi-client choreography is one parameterized cohort; a fixed-arity twin is
// the rejected form. Every row resolves its target through _arm, so a spec is target-agnostic.
const test = base.extend<Kit>({
    a11y: async ({ page }, use) => {
        await use((scope) => {
            const audit = new AxeBuilder({ page }).withTags([..._WCAG]);
            return (scope === undefined ? audit : audit.include(scope)).analyze().then((report) => report.violations);
        });
    },
    cohort: async ({ baseURL, browser }, use) => {
        const opened: Array<BrowserContext> = [];
        await use(async (route, count) => {
            const open = async (): Promise<Page> => {
                const context = await browser.newContext();
                opened.push(context);
                const origin = await _arm(context, baseURL);
                const page = await context.newPage();
                await page.goto(`${origin}${route}`);
                return page;
            };
            return Promise.all(Array.from({ length: count }, open));
        });
        await Promise.all(opened.map((context) => context.close()));
    },
    // Installed, not paused: install() anchors the epoch and auto-advances; pauseAt/setFixedTime are
    // the caller's explicit stops, so the fixture name claims only what installation delivers.
    clock: async ({ page }, use) => {
        await page.clock.install({ time: _EPOCH });
        await use(page.clock);
    },
    target: async ({ baseURL, context, page }, use) => {
        const origin = await _arm(context, baseURL);
        await use({
            open: async (route) => {
                await page.goto(`${origin}${route}`);
            },
            origin,
        });
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
