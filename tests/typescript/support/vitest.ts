import { realpathSync } from 'node:fs';
import { createRequire } from 'node:module';
import { join, parse, relative } from 'node:path';
import { playwright } from '@vitest/browser-playwright';
import { Array, Config, Effect, Record, String } from 'effect';
import { searchForWorkspaceRoot } from 'vite';
import { configDefaults, defineConfig, defineProject, mergeConfig } from 'vitest/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CI = Effect.runSync(Config.withDefault(Config.Boolean('CI'), false));

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (configFile: string) => {
    const { dir, name } = parse(configFile);
    const root = searchForWorkspaceRoot(dir);
    const project = relative(root, dir);
    return { dir, root, project, results: join(root, '.artifacts', 'typescript', 'vitest', project, name) };
};
const rootDefaults = (configFile: string) => {
    const { dir, root, results } = located(configFile);
    const { devDependencies = {} }: { readonly devDependencies?: Record.ReadonlyRecord<string, string> } = createRequire(configFile)('./package.json');
    const subjects = Record.keys(Record.filter(devDependencies, String.startsWith('workspace:')))
        .map((dependency) => realpathSync(join(dir, 'node_modules', dependency)))
        .filter((folder) => folder !== import.meta.dirname);
    return defineConfig({
        test: {
            attachmentsDir: join(results, 'attachments'),
            coverage: {
                allowExternal: true,
                exclude: ['**/*.d.ts', '**/*.config.ts', '**/tests/**', '**/.storybook/**'],
                ...(dir === root ? {} : { include: (Array.isArrayNonEmpty(subjects) ? subjects : [dir]).map((folder) => join(relative(dir, folder), '**/*.{ts,tsx}')) }),
                reporter: ['text', 'html'],
                reportOnFailure: true,
                reportsDirectory: join(results, 'coverage'),
                skipFull: true,
            },
            outputFile: { junit: join(results, 'junit.xml') },
            reporters: [...configDefaults.reporters, 'junit', ['html', { outputDir: join(results, 'html') }]],
            silent: 'passed-only',
            slowTestThreshold: 5000,
        },
    });
};
const defaults = (configFile: string) => {
    const { root, project, results } = located(configFile);
    return mergeConfig(
        rootDefaults(configFile),
        defineProject({
            test: {
                browser: { provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure', tracesDir: join(results, 'traces') } },
                chaiConfig: { includeStack: true, truncateThreshold: 0 },
                fsModuleCachePath: join(root, '.cache', 'vitest', project, 'modules'),
                hideSkippedTests: CI,
                maxWorkers: '50%',
                pool: 'threads',
                provide: { artifacts: results },
                sequence: { shuffle: CI },
                testTimeout: 10_000,
            },
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaults, rootDefaults };
