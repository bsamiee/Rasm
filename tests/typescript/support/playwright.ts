import { join } from 'node:path';
import AxeBuilder from '@axe-core/playwright';
import { getPackageManagerCommand, getReadyUrl, readJsonFile } from '@nx/devkit';
import { isCI, type PackageJson } from '@nx/devkit/internal';
import { defineConfig, test } from '@playwright/test';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config = (directory: string, output: string) => {
    const folder = join(output, readJsonFile<PackageJson>(join(directory, 'package.json')).name);
    return defineConfig({
        forbidOnly: Boolean(isCI()),
        outputDir: join(folder, 'test-results'),
        reporter: [
            ['junit', { outputFile: join(folder, 'junit.xml') }],
            ['html', { open: 'never', outputFolder: join(folder, 'playwright-report') }],
        ],
        use: { trace: 'retain-on-failure', screenshot: 'only-on-failure' },
    });
};

const served = (task: string, address: string) => {
    const url = getReadyUrl(address);
    return defineConfig({ use: { baseURL: url }, webServer: { command: `${getPackageManagerCommand().exec} nx run ${task}`, url, reuseExistingServer: true } });
};

const it = test.extend<{ readonly axe: AxeBuilder }>({ axe: ({ page }, use) => use(new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'])) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { config, it, served };
