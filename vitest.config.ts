import { globSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { readJsonFile } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, type PackageJson } from '@nx/devkit/internal';
import { react } from '@rasm/testing/storybook';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import { playwright } from '@vitest/browser-playwright';
import { Duration, Match } from 'effect';
import { defaultExclude, defineConfig, type TestProjectInlineConfiguration } from 'vitest/config';
import type { BrowserConfigOptions } from 'vitest/node';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKERS = '**/*.workers.test.ts';
const ARTIFACTS = join(import.meta.dirname, '.artifacts/vitest');
const LIMIT = Duration.toMillis(Duration.minutes(15));

// --- [OPERATIONS] ----------------------------------------------------------------------

const browser = (name: string): BrowserConfigOptions => ({
    enabled: true,
    provider: playwright(),
    headless: true,
    instances: [{ browser: 'chromium' }],
    locators: { errorFormat: 'aria' },
    screenshotDirectory: join(ARTIFACTS, name, 'screenshots'),
    trace: { mode: 'retain-on-failure', tracesDir: join(ARTIFACTS, name, 'traces') },
});

const suites = (root: string, { name, devDependencies = {}, peerDependencies = {} }: PackageJson): TestProjectInlineConfiguration[] => {
    const test = { name, testTimeout: LIMIT, hookTimeout: LIMIT, execArgv: ['--disable-warning=ExperimentalWarning'], env: { CI: 'false', ...(('nx' in devDependencies || '@nx/devkit' in devDependencies) && { NX_DAEMON: 'false', NX_WORKSPACE_DATA_DIRECTORY: '.nx/workspace-data' }) } };
    return 'vitest' in peerDependencies
        ? []
        : Match.value(devDependencies).pipe(
              Match.when({ '@storybook/addon-vitest': Match.string }, () => [
                  { plugins: [storybookTest({ configDir: join(import.meta.dirname, root, '.storybook') }), { name: 'dependency-cache', config: () => ({ cacheDir: join(import.meta.dirname, 'node_modules/.vite', name) }) }], test: { ...test, api: { allowWrite: true }, browser: browser(name) } },
              ]),
              Match.when({ 'vitest-browser-react': Match.string }, () => [{ root, plugins: [react], test: { ...test, browser: browser(name) } }]),
              Match.when({ '@cloudflare/vitest-plugin': Match.string }, () => [
                  { root, test: { ...test, name: `${name} node`, exclude: [...defaultExclude, WORKERS] } },
                  { root, plugins: [cloudflareTest({ config: true })], test: { ...test, name: `${name} workers`, include: [WORKERS] } },
              ]),
              Match.when({ vitest: Match.string }, () => [{ root, test }]),
              Match.orElse(() => []),
          );
};

const patterns = buildPackageJsonPatterns(import.meta.dirname, (file: string) => readJsonFile<PackageJson>(join(import.meta.dirname, file)));

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    test: {
        chaiConfig: { includeStack: true, truncateThreshold: 0 },
        attachmentsDir: join(ARTIFACTS, 'attachments'),
        silent: 'passed-only',
        projects: globSync(patterns.positive, { cwd: import.meta.dirname, exclude: ['**/node_modules/**', '**/.*/**'] })
            .filter((file) => file !== 'package.json' && buildPackageJsonWorkspacesMatcher(patterns)(file))
            .flatMap((file) => suites(dirname(file), readJsonFile<PackageJson>(join(import.meta.dirname, file)))),
    },
});
