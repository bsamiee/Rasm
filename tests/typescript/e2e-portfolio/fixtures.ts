import AxeBuilder from '@axe-core/playwright';
import { expect as base, type Page } from '@playwright/test';
import { sessionPath } from '@rasm/hosting/authentication';
import { app } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

const test = app('@rasm/portfolio').extend<{ readonly owner: Page }>({
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
        return { name: 'toPassAxe', pass: violations.length === 0, message: () => `${violations.length} Axe violations in ${include}\n${report}` };
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { expect, test };
