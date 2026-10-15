import { test as base } from '@playwright/test';

// --- [COMPOSITION] ---------------------------------------------------------------------

const test = base.extend({
    page: async ({ page }, use) => {
        await page.goto('/');
        await use(page);
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { test };
