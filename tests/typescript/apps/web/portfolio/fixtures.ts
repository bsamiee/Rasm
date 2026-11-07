import AxeBuilder from '@axe-core/playwright';
import playwright from '@playwright/test';

// --- [COMPOSITION] ---------------------------------------------------------------------

const test = playwright.test.extend<{ readonly makeAxeBuilder: () => AxeBuilder }>({ makeAxeBuilder: ({ page }, use) => use(() => new AxeBuilder({ page })) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { test };
