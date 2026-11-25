import { execFileSync } from 'node:child_process';
import { env, execPath } from 'node:process';
import { fileURLToPath } from 'node:url';
import { workspaceRoot } from '@nx/devkit';
import { Array, Record } from 'effect';
import { describe, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = fileURLToPath(import.meta.resolve('nx/bin/nx'));
const LEAVES = ['@rasm/telemetry:build--tsc', 'rasm_dependency_graph:check--clippy', 'Rasm.Telemetry.Api:build--dotnet', 'rasm-telemetry:check--mypy', 'watch-app:build--xcodebuild'] as const;
const MODES = { cached: [], uncached: ['--skip-nx-cache'] } as const;
const RUNS = Array.cartesianWith(LEAVES, Record.toEntries(MODES), (task, [mode, flags]) => ({ task, mode, flags }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const nx = (...args: readonly string[]) => execFileSync(execPath, [NX, ...args], { cwd: workspaceRoot, env: { ...env, NX_DAEMON: 'false' }, stdio: ['ignore', 'pipe', 'pipe'] });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('workspace graph', () => {
    it('times nx show projects over a warm plugin cache with the daemon off', ({ bench }) => bench('nx show projects, warm', () => nx('show', 'projects', '--json')).run({ iterations: 10, warmupIterations: 2 }));
});

describe.for(RUNS)('workspace task $task, $mode', ({ task, mode, flags }) => {
    it('times a cache restore every run pays or a run past the cache over the state its tool keeps', ({ bench }) => bench(`nx run ${task}, ${mode}`, () => nx('run', task, ...flags)).run({ iterations: 5, warmupIterations: 1 }));
});
