import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-playwright';
import manifest from '@rasm/nx-playwright/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPEC = "import { test } from '@playwright/test';\n\ntest('opens', async ({ page }) => {\n    await page.goto('/');\n});\n";
const config = (servers: string) => `import { defineConfig } from '@playwright/test';\n\nexport default defineConfig({ testDir: '.', reporter: [['html', { open: 'never' }]], ${servers} });\n`;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['@playwright/test'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["site", "reused", "owned", "split"] }\n',
        'site/package.json': '{ "name": "site", "nx": { "targets": { "serve": { "command": "node server.js", "continuous": true, "readyWhen": { "signal": true } }, "preview": { "command": "node server.js --built", "continuous": true, "readyWhen": { "signal": true } } } } }\n',
        'reused/package.json': '{ "name": "reused", "type": "module" }\n',
        'reused/playwright.config.ts': config("webServer: { command: 'npx nx run site:serve', url: 'http://localhost:4173', reuseExistingServer: true }"),
        'reused/site.spec.ts': SPEC,
        'owned/package.json': '{ "name": "owned", "type": "module" }\n',
        'owned/playwright.config.ts': config("webServer: { command: 'npx nx run site:serve', url: 'http://localhost:4173', reuseExistingServer: false }"),
        'owned/site.spec.ts': SPEC,
        'split/package.json': '{ "name": "split", "type": "module" }\n',
        'split/playwright.config.ts': config("projects: [{ name: 'chromium', webServer: { command: 'npx nx run site:preview', url: 'http://localhost:4174', reuseExistingServer: true } }]"),
        'split/site.spec.ts': SPEC,
    },
    broken: ['owned/playwright.config.ts', "import { defineConfig } from '@playwright/test';\n\nexport default defineConfig({ testDir:\n"],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('runs playwright test cached, writing its results and HTML report folders, and waits for the Nx task a reused webServer command runs, at the top level and per Playwright project, and for no server Playwright owns', () =>
        inferred(subject).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.map(({ project, cache, outputs, waits }) => ({ project, cache, outputs: outputs.toSorted(), waits }))),
            Effect.map((tested) =>
                assert.deepStrictEqual(tested, [
                    { project: 'owned', cache: true, outputs: ['owned/playwright-report', 'owned/test-results'], waits: [] },
                    { project: 'reused', cache: true, outputs: ['reused/playwright-report', 'reused/test-results'], waits: [{ projects: ['site'], target: 'serve', waitFor: 'ready' }] },
                    { project: 'split', cache: true, outputs: ['split/playwright-report', 'split/test-results'], waits: [{ projects: ['site'], target: 'preview', waitFor: 'ready' }] },
                ]),
            ),
        ),
    );
});
