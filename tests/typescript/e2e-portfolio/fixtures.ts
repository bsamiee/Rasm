import AxeBuilder from '@axe-core/playwright';
import { expect as base, type Page } from '@playwright/test';
import { sessionPath } from '@rasm/hosting/authentication/sites.ts';
import { app } from '@rasm/test-support/app.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const test = app('architectural-portfolio').extend<{ readonly owner: Page }>({
    page: async ({ page }, use) => {
        await page.goto('/');
        await use(page);
    },
    owner: async ({ browser }, use) => {
        const context = await browser.newContext();
        const owner = await context.newPage();
        await owner.goto(sessionPath('signin', '/?edit=1'));
        await use(owner);
        await context.close();
    },
});
const expect = base.extend({
    async toPassAxe(page: Page, include: string) {
        const { violations } = await new AxeBuilder({ page }).include(include).analyze();
        const report = JSON.stringify(violations, null, 2);
        await test.info().attach('axe', { body: report, contentType: 'application/json' });
        return { name: 'toPassAxe', pass: violations.length === 0, message: () => `Axe found ${violations.length} violations in ${include}:\n${report}` };
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { expect, test };
