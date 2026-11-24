import { join } from 'node:path';
import { it as workspaced, pathed } from '@rasm/testing/nx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STUB = { report: '.stub/report.json' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const mode = (configuration: string, stated: Readonly<Record<string, unknown>> = {}) => ({ configuration, outputs: [], published: [], reports: [], documents: [], packed: '', ...stated });

const node = (file: string, stated: Readonly<Record<string, unknown>> = {}) => ({
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
    bases: [],
    packages: [],
    referenced: [],
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

const report = (nodes: readonly Readonly<Record<string, unknown>>[]) => ({ [STUB.report]: JSON.stringify({ nodes, failures: [], evaluated: [] }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('analyzer', { auto: true }, ({ workspace }) => {
    const folder = join(workspace.root, '.stub', 'dotnet');
    pathed(folder, { dotnet: `[ "$1" = --version ] && exec /bin/cat ${folder}/version\nexec /bin/cat ${STUB.report}` });
    return { folder };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it, mode, node, report };
