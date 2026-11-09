import { realpathSync } from 'node:fs';
import { createRequire } from 'node:module';
import { dirname, join, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { playwright } from '@vitest/browser-playwright';
import { Array, Config, Effect, Option, Record } from 'effect';
import { searchForWorkspaceRoot } from 'vite';
import { configDefaults, defineConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

const defaults = (configFile: string) => {
    const root = dirname(configFile);
    const workspace = searchForWorkspaceRoot(root);
    const ci = Effect.runSync(Config.withDefault(Config.Boolean('CI'), false));
    const { name, dependencies = {}, devDependencies = {} }: { readonly name: string; readonly dependencies?: Record.ReadonlyRecord<string, string>; readonly devDependencies?: Record.ReadonlyRecord<string, string> } = createRequire(configFile)('./package.json');
    const results = join(workspace, '.artifacts', 'vitest', name);
    const subjects = Array.getSomes(Record.keys({ ...dependencies, ...devDependencies }).map((dependency) => Option.liftThrowable((at: string) => realpathSync(at, 'utf8'))(join(root, 'node_modules', dependency)))).filter((folder) => !folder.split(sep).includes('node_modules') && folder !== import.meta.dirname);
    return defineConfig({
        test: {
            attachmentsDir: join(results, 'attachments'),
            browser: { provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure', tracesDir: join(results, 'traces') } },
            chaiConfig: { includeStack: true, truncateThreshold: 0 },
            coverage: {
                allowExternal: true,
                exclude: ['**/*.d.ts', '**/*.config.ts', '**/.storybook/**'],
                include: (Array.isArrayNonEmpty(subjects) ? subjects : [root]).map((folder) => join(relative(root, folder), '**/*.{ts,tsx}')),
                reporter: ['text', ['html', { subdir: 'coverage' }]],
                reportOnFailure: true,
                reportsDirectory: results,
                skipFull: true,
            },
            fsModuleCachePath: join(workspace, '.cache', 'vitest', name, 'modules'),
            hideSkippedTests: ci,
            maxWorkers: '50%',
            outputFile: { junit: join(results, 'junit.xml') },
            pool: 'threads',
            provide: { artifacts: results },
            reporters: [...configDefaults.reporters, 'junit', [fileURLToPath(import.meta.resolve('@vitest/ui/reporter')), { outputDir: join(results, 'html') }]],
            sequence: { shuffle: ci },
            silent: 'passed-only',
            slowTestThreshold: 5000,
            testTimeout: 10_000,
        },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaults };
