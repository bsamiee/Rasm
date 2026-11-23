import { execFileSync } from 'node:child_process';
import { env, execPath } from 'node:process';
import { fileURLToPath } from 'node:url';
import { workspaceRoot } from '@nx/devkit';
import { describe, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = fileURLToPath(import.meta.resolve('nx/bin/nx'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('workspace graph', () => {
    it('times nx show projects over a warm plugin cache with the daemon off', ({ bench }) =>
        bench('nx show projects, warm', () => execFileSync(execPath, [NX, 'show', 'projects', '--json'], { cwd: workspaceRoot, env: { ...env, NX_DAEMON: 'false' }, stdio: ['ignore', 'pipe', 'pipe'] })).run({ iterations: 10, warmupIterations: 2 }));
});
