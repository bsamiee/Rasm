import AxeBuilder from '@axe-core/playwright';
import { defineConfig, test } from '@playwright/test';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config = defineConfig({ reporter: [['junit'], ['html', { open: 'never' }]], use: { trace: 'retain-on-failure', screenshot: 'only-on-failure' } });
const it = test.extend<{ readonly axe: AxeBuilder }>({ axe: ({ page }, use) => use(new AxeBuilder({ page })) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { config, it };
