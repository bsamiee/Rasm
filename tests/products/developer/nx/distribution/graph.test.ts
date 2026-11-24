import { execFileSync } from 'node:child_process';
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { delimiter, dirname, join } from 'node:path';
import { env, execPath } from 'node:process';
import { fileURLToPath } from 'node:url';
import { workspaceRoot } from '@nx/devkit';
import { Option, Schema } from 'effect';
import { describe, expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = fileURLToPath(import.meta.resolve('nx/bin/nx'));
const OWNERS = { GRAPH_PLUGINS: [dirname(dirname(fileURLToPath(import.meta.resolve('@rasm/nx-distribution')))), dirname(fileURLToPath(import.meta.resolve('@rasm/nx/package.json')))].join(delimiter), GRAPH_NX: dirname(fileURLToPath(import.meta.resolve('nx/package.json'))) } as const;
const TRACER = `data:text/javascript,${encodeURIComponent(String.raw`
import childProcess from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { syncBuiltinESMExports } from 'node:module';
import workerThreads from 'node:worker_threads';
const traced = fs.openSync(process.env.GRAPH_TRACE, 'a');
const owners = process.env.GRAPH_PLUGINS.split(path.delimiter);
const log = (kind, what) => {
    const frames = (new Error().stack ?? '').split('\n');
    const at = frames.find((frame) => owners.some((owner) => frame.includes(owner)))?.trim() ?? '';
    fs.writeSync(traced, JSON.stringify({ kind, what: String(what), at, owned: at !== '' || !frames.some((frame) => frame.includes(process.env.GRAPH_NX)) }) + '\n');
};
const wrap = (owner, names) => names.forEach((name) => { const held = owner[name]; owner[name] = function (...args) { log(name, args[0]); return held.apply(this, args); }; });
wrap(childProcess, ['spawn', 'spawnSync', 'exec', 'execSync', 'execFile', 'execFileSync', 'fork']);
wrap(fs, ['writeFile', 'writeFileSync', 'appendFile', 'appendFileSync']);
wrap(fs.promises, ['writeFile', 'appendFile']);
const Held = workerThreads.Worker;
workerThreads.Worker = class extends Held { constructor(file, options) { log('Worker', file); super(file, options); } };
syncBuiltinESMExports();
`)}`;

// --- [MODELS] --------------------------------------------------------------------------

const Event = Schema.fromJsonString(Schema.Struct({ kind: Schema.String, what: Schema.String, at: Schema.String, owned: Schema.Boolean })).annotate({
    description: 'One spawn, worker thread, or file write a traced Nx process made, the plugin frame that made it, and whether plugin code owns it: a plugin frame on its stack, or no Nx frame at all',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('warm workspace graph', () => {
    it('starts no process or thread and writes no file from plugin code once warm', ({ onTestFinished }) => {
        const folder = mkdtempSync(join(tmpdir(), 'nx-graph-'));
        onTestFinished(() => rmSync(folder, { recursive: true, force: true }));
        const trace = join(folder, 'trace.jsonl');
        const graph = () => execFileSync(execPath, [NX, 'show', 'projects', '--json'], { cwd: workspaceRoot, env: { ...env, ...OWNERS, NX_DAEMON: 'false', NODE_OPTIONS: `--import=${TRACER}`, GRAPH_TRACE: trace }, stdio: ['ignore', 'pipe', 'pipe'] });
        graph();
        writeFileSync(trace, '');

        graph();

        const events = Option.getOrThrow(Schema.decodeOption(Schema.Array(Event))(readFileSync(trace, 'utf-8').trim().split('\n')));
        expect([events.some(({ what }) => what === 'git'), events.filter(({ owned }) => owned)]).toStrictEqual([true, []]);
    }, 180_000);
});
