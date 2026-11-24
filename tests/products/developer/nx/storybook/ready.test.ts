import { execFileSync } from 'node:child_process';
import { execPath } from 'node:process';
import { fileURLToPath } from 'node:url';
import { environment, it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = fileURLToPath(import.meta.resolve('nx/bin/nx'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('readiness of storybook dev', { timeout: 120_000 }, () => {
    it('starts a task waiting on serve--storybook ready only after Storybook announces its address, with no port named and stdout a pipe', ({ workspace }) => {
        workspace
            .install(
                import.meta.resolve('nx/package.json'),
                import.meta.resolve('@rasm/nx-storybook/package.json'),
                import.meta.resolve('storybook/package.json'),
                import.meta.resolve('@storybook/react-vite/package.json'),
                import.meta.resolve('react/package.json'),
                import.meta.resolve('react-dom/package.json'),
                import.meta.resolve('vite/package.json'),
            )
            .write({
                'nx.json': '{"plugins":["@rasm/nx-storybook"]}',
                'package.json': '{"name":"repo","workspaces":["apps/*"]}',
                'package-lock.json': '{"name":"repo","lockfileVersion":3,"packages":{"":{"name":"repo","workspaces":["apps/*"]},"node_modules/storybook":{"version":"0.0.0"},"node_modules/@storybook/react-vite":{"version":"0.0.0"}}}',
                'apps/site/package.json': '{"name":"site","nx":{"targets":{"after":{"command":"echo dependent-started","dependsOn":[{"target":"serve--storybook","waitFor":"ready"}]}}}}',
                'apps/site/.storybook/main.ts': "export default { framework: '@storybook/react-vite', stories: ['../src/**/*.stories.tsx'] };\n",
            });

        const stdout = execFileSync(execPath, [NX, 'run', 'site:after', '--outputStyle=stream'], { cwd: workspace.root, env: environment(workspace.root), encoding: 'utf-8', stdio: ['ignore', 'pipe', 'pipe'] });

        const lines = stdout.split('\n');
        const [address, started] = [lines.findIndex((line) => /https?:\/\/[^\s/]+:\d+\//u.test(line)), lines.findIndex((line) => line.endsWith('dependent-started'))];
        expect({ announced: address >= 0, started: started >= 0, after: started > address }).toStrictEqual({ announced: true, started: true, after: true });
    });
});
