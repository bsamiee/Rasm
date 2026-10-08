import { spawnSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { access, mkdir, readFile, realpath, stat } from 'node:fs/promises';
import { dirname } from 'node:path';
import process from 'node:process';
import { all, bind, decoded, fault, fromUndefined, map, none, type Option, ok, type Result, some } from '../../plugins/function-hooks/composition.ts';
import { SCAN } from '../../plugins/function-hooks/policies/command.ts';
import type { Invocation } from '../../plugins/function-hooks/policies/invocation.ts';
import type { Host } from '../../plugins/function-hooks/policies/policies.ts';
import type { Formatter } from '../../plugins/function-hooks/repository/format.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const exec = ([program, ...args]: Invocation, cwd: string, stdin: Option<string>, exits: readonly number[]): Result<string> => {
    const ran = spawnSync(program, args, { cwd, encoding: 'utf8', input: stdin.kind === 'some' ? stdin.value : undefined, timeout: 120_000 });
    if (ran.error !== undefined) {
        return fault({ kind: 'unstarted', subject: program, cause: ran.error });
    }
    if (ran.status === null) {
        return fault({ kind: 'unstarted', subject: program, cause: `killed by ${ran.signal}` });
    }
    return exits.includes(ran.status) ? ok(ran.stdout) : fault({ kind: 'exited', subject: program, code: ran.status, stderr: ran.stderr.trim() });
};

const read = (path: string): Promise<Result<string>> => readFile(path, 'utf8').then(ok, (cause: unknown) => fault<string>({ kind: 'unread', subject: path, cause }));

const payload = <T>(holds: (value: unknown) => value is T): Result<T> => {
    const subject = 'hook input';
    return bind(decoded<unknown>(subject, ok(readFileSync(0, 'utf8'))), (value): Result<T> => (holds(value) ? ok(value) : fault({ kind: 'invalid', subject, cause: value })));
};

const host = (cwd: string): Host => ({
    scan: (text) => Promise.resolve(exec(SCAN, cwd, some(text), [0])),
    read,
    repo: () => Promise.resolve(map(exec(['git', 'rev-parse', '--path-format=absolute', '--git-common-dir'], cwd, none, [0]), (printed) => dirname(printed.trim()))).then((found) => (found.kind === 'ok' ? some(found.value) : none)),
    make: async (folders) => all(await Promise.all(folders.map((folder) => mkdir(folder, { recursive: true }).then(ok, (cause: unknown) => fault<unknown>({ kind: 'unwritten', subject: folder, cause }))))),
    exists: (path) =>
        access(path).then(
            () => true,
            () => false,
        ),
    real: (path) => realpath(path).then(some, () => none),
    home: () => Promise.resolve(fromUndefined(process.env.HOME)),
    workspaceData: () => Promise.resolve(fromUndefined(process.env.NX_WORKSPACE_DATA_DIRECTORY)),
});

const formatter = (root: string): Formatter => ({
    exec: (invocation, exits) => Promise.resolve(exec(invocation, root, none, exits)),
    read,
    mtime: (path) =>
        stat(path).then(
            ({ mtimeMs }) => some(mtimeMs),
            () => none,
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { exec, formatter, host, payload, read };
