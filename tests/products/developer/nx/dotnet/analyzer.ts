import { join } from 'node:path';
import { it as workspaced, pathed } from '@rasm/testing/nx';
import { Record } from 'effect';
import { vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STUB = { report: '.stub/report.json' } as const;
const SERVERLESS = { DOTNET_CLI_USE_MSBUILD_SERVER: '0', MSBUILDDISABLENODEREUSE: '1', UseSharedCompilation: 'false' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const mode = (configuration: string, stated: Record.ReadonlyRecord<string, unknown> = {}) => ({ configuration, outputs: [], published: [], reports: [], documents: [], packed: '', ...stated });

const node = (file: string, stated: Record.ReadonlyRecord<string, unknown> = {}) => ({
    file,
    name: file.slice(file.lastIndexOf('/') + 1, file.lastIndexOf('.')),
    language: 'C#',
    platform: 'AnyCPU',
    test: false,
    packable: false,
    publishable: false,
    pushed: false,
    versioned: false,
    evaluated: mode('Debug'),
    declared: [],
    references: [],
    inputs: [],
    loaded: [],
    catalogs: [],
    bases: [],
    packages: [],
    locks: [],
    restored: [],
    results: [],
    platforms: [],
    implicit: [],
    tags: [],
    environment: [],
    external: [],
    releases: null,
    ...stated,
});

const report = (nodes: readonly Record.ReadonlyRecord<string, unknown>[]) => ({ [STUB.report]: JSON.stringify({ nodes, failures: [], evaluated: [], probed: [], variables: [] }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('serverless', { auto: true }, ({}, { onCleanup }) => {
    Record.toEntries(SERVERLESS).forEach(([name, value]) => {
        vi.stubEnv(name, value);
    });
    onCleanup(() => {
        vi.unstubAllEnvs();
    });
    return SERVERLESS;
});

const reported = it.extend('analyzer', { auto: true }, ({ workspace }) => {
    const folder = join(workspace.root, '.stub', 'dotnet');
    pathed(folder, { dotnet: `[ "$1" = --version ] && exec /bin/cat ${folder}/version\nexec /bin/cat ${STUB.report}` });
    return { folder };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it, mode, node, report, reported };
