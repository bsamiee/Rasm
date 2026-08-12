import { AxeBuilder } from '@axe-core/playwright';
import { type BrowserContext, test as base, expect, type Page } from '@playwright/test';
import { Hermetic } from '@rasm/ts-testkit/e2e';
import { Option } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Held = Awaited<ReturnType<BrowserContext['credentials']['get']>>;
type Violations = Awaited<ReturnType<AxeBuilder['analyze']>>['violations'];
type Kit = {
    readonly a11y: (scope?: string) => Promise<Violations>;
    readonly clock: Page['clock'];
    readonly cohort: (route: string, count: number) => Promise<ReadonlyArray<Page>>;
    readonly target: { readonly origin: string; readonly open: (route: string) => Promise<void> };
    readonly webauthn: { readonly held: (rpId: string) => Promise<Held> };
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const _EPOCH = new Date('2026-01-01T00:00:00.000Z');
const _WCAG = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'] as const;  // The axe rule surface: the WCAG 2.x A/AA tags — the conformance floor every served page clears.

// --- [OPERATIONS] ----------------------------------------------------------------------

// Hermetic serving: every context route on the kit origin fulfills from the page corpus — a 404 for a phantom path is the falsifiable miss, never a hang.
const _serve = async (context: BrowserContext): Promise<void> => {
    await context.route(`${Hermetic.origin}/**`, (route) =>
        Option.match(Hermetic.page(new URL(route.request().url()).pathname), {
            onNone: () => route.fulfill({ body: '', status: 404 }),
            onSome: (document) => route.fulfill({ body: document, contentType: 'text/html' }),
        }),
    );
};

// The target discriminant is baseURL presence: a served product project carries its origin as baseURL and rides the real server; an unset baseURL is the hermetic row, armed in-context.
const _arm = async (context: BrowserContext, baseURL: string | undefined): Promise<string> => {
    if (baseURL === undefined) {
        await _serve(context);
        return Hermetic.origin;
    }
    return baseURL;
};

// The one fixture tower: every platform capability is a row here, composed from kit data — never a helper beside a spec.
// Multi-client choreography is one parameterized cohort; a fixed-arity twin is the rejected form. Every row resolves its target through _arm, so a spec is target-agnostic.
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
    // Installed, not paused: install() anchors the epoch and auto-advances; pauseAt/setFixedTime are the caller's explicit stops, so the fixture name claims only what installation delivers.
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
    // install() IS the capability: it overrides navigator.credentials across every page of the context in every engine, so requesting this row arms the ceremony and omitting it is the refutation seed.
    // `held` reads the authenticator's own registry back, giving a mint assertion an oracle outside the page.
    webauthn: async ({ context }, use) => {
        await context.credentials.install();
        await use({ held: (rpId) => context.credentials.get({ rpId }) });
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { expect, test };
