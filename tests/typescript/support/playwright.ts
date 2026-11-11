import AxeBuilder from '@axe-core/playwright';
import { getPackageManagerCommand, getReadyUrl } from '@nx/devkit';
import { defineConfig, test } from '@playwright/test';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config = defineConfig({ reporter: [['junit'], ['html', { open: 'never' }]], use: { trace: 'retain-on-failure', screenshot: 'only-on-failure' } });

const served = (task: string, address: string) => {
    const url = getReadyUrl(address);
    return defineConfig({ use: { baseURL: url }, webServer: { command: `${getPackageManagerCommand().exec} nx run ${task}`, url, reuseExistingServer: true } });
};

const it = test.extend<{ readonly axe: AxeBuilder }>({ axe: ({ page }, use) => use(new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'])) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { config, it, served };
