import { realpathSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, join, parse, relative } from 'node:path';
import { playwright } from '@vitest/browser-playwright';
import { Config, Effect } from 'effect';
import { searchForWorkspaceRoot } from 'vite';
import { configDefaults, defineConfig } from 'vitest/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CI = Effect.runSync(Config.withDefault(Config.Boolean('CI'), false));

// --- [OPERATIONS] ----------------------------------------------------------------------

const defaults = (configFile: string) => {
    const { dir, name } = parse(configFile);
    const root = searchForWorkspaceRoot(dir);
    const project = relative(root, dir);
    const results = join(root, '.artifacts', 'typescript', 'vitest', project, name);
    const { devDependencies = {} }: { readonly devDependencies?: Readonly<Record<string, string>> } = createRequire(configFile)('./package.json');
    const tested =
        Object.keys(devDependencies)
            .map((dependency) => realpathSync(join(dir, 'node_modules', dependency)))
            .find((folder) => basename(folder) === basename(dir)) ?? dir;
    return defineConfig({
        test: {
            attachmentsDir: join(results, 'attachments'),
            browser: { provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure', tracesDir: join(results, 'traces') } },
            chaiConfig: { includeStack: true, truncateThreshold: 0 },
            coverage: {
                allowExternal: true,
                exclude: ['**/*.d.ts', '**/*.config.ts', '**/tests/**', '**/.storybook/**'],
                ...(dir === root ? {} : { include: [join(relative(dir, tested), '**/*.{ts,tsx}')] }),
                reporter: ['text', 'html'],
                reportOnFailure: true,
                reportsDirectory: join(results, 'coverage'),
                skipFull: true,
            },
            fsModuleCachePath: join(root, '.cache', 'vitest', project, 'modules'),
            hideSkippedTests: CI,
            maxWorkers: '50%',
            outputFile: { junit: join(results, 'junit.xml') },
            pool: 'threads',
            provide: { artifacts: results },
            reporters: [...configDefaults.reporters, 'junit'],
            sequence: { shuffle: CI },
            silent: 'passed-only',
            slowTestThreshold: 5000,
            testTimeout: 10_000,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaults };
