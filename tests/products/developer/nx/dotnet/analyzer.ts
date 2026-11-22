import { mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { basename, extname, join } from 'node:path';
import { it as workspaced, pathed } from '@rasm/testing/nx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STUB = { report: '.stub/report.json', sdk: '.stub/sdk' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const mode = (configuration: string, stated: Readonly<Record<string, unknown>> = {}) => ({ configuration, outputs: [], published: [], reports: [], documents: [], packed: '', ...stated });

const node = (file: string, stated: Readonly<Record<string, unknown>> = {}) => ({
    file,
    name: basename(file, extname(file)),
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
    external: [],
    releases: null,
    ...stated,
});

const report = (nodes: readonly Readonly<Record<string, unknown>>[], stated: Readonly<Record<string, unknown>> = {}) => ({ [STUB.report]: JSON.stringify({ nodes, failures: [], evaluated: [], ...stated }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('analyzer', { auto: true }, ({ workspace }, { onCleanup }) => {
    const folder = mkdtempSync(join(tmpdir(), 'dotnet-stub-'));
    onCleanup(() => rmSync(folder, { recursive: true }));
    pathed(folder, {
        dotnet: [
            `printf '%s\\n' "$*" >> ${folder}/calls.log`,
            `[ "$1" = --version ] && exec /bin/cat ${STUB.sdk}`,
            `printf '%s|%s|%s\\n' "$DOTNET_ROLL_FORWARD_TO_PRERELEASE" "$DOTNET_DbgMiniDumpName" "$DOTNET_EnableCrashReport" >> ${folder}/env.log`,
            `/bin/cat > ${folder}/stdin.log`,
            `exec /bin/cat ${STUB.report}`,
        ].join('\n'),
    });
    workspace.write({ [STUB.sdk]: '11.0.100\n' });
    return { logged: (log: 'calls' | 'stdin' | 'env') => readFileSync(join(folder, `${log}.log`), 'utf-8') };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it, mode, node, report, STUB };
