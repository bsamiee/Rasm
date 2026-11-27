import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { execPath, kill } from 'node:process';
import { fileURLToPath } from 'node:url';
import { environment, it } from '@rasm/testing/nx';
import { Cause, Effect } from 'effect';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = fileURLToPath(import.meta.resolve('nx/bin/nx'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('readiness of storybook dev', () => {
    it('starts a task waiting on serve--storybook ready only after Storybook announces its address, with no port named and stdout a pipe', async ({ workspace }) => {
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
        const nx = spawn(execPath, [NX, 'run', 'site:after', '--outputStyle=stream'], { cwd: workspace.root, env: environment(workspace.root), stdio: ['ignore', 'pipe', 'ignore'], detached: true });
        const closed = Promise.all([nx.stdout.toArray(), once(nx, 'close')]);
        const ran = Effect.promise(() => closed).pipe(
            Effect.timeoutOrElse({ duration: '3 minutes', orElse: () => Effect.fail(new Cause.UnknownError(undefined, 'nx run site:after never ended after storybook dev started')) }),
            Effect.ensuring(Effect.ignore(Effect.try({ try: () => kill(-(nx.pid ?? 0), 'SIGTERM'), catch: (cause) => new Cause.UnknownError(cause, 'the nx process group ended before its release') }))),
        );

        const [chunks] = await Effect.runPromise(ran);

        const lines = chunks.join('').split('\n');
        const [address, started] = [lines.findIndex((line) => /https?:\/\/[^\s/]+:\d+\//u.test(line)), lines.findIndex((line) => line.endsWith('dependent-started'))];
        expect({ announced: address >= 0, started: started >= 0, after: started > address }).toStrictEqual({ announced: true, started: true, after: true });
    });
});
