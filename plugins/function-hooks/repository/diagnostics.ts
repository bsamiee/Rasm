// biome-ignore-all lint/correctness/noNodejsModules: Node owns cancellable child processes, streams, and repository file metadata

import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { stat } from 'node:fs/promises';
import { resolve } from 'node:path';
import { text } from 'node:stream/consumers';
import { quoteShellArg } from 'nx/src/utils/shell-quoting.js';
import { bind, fault, ok, type Result } from '../composition.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const diagnostics = async (root: string, paths: readonly string[], signal?: AbortSignal): Promise<Result<readonly string[]>> => {
    const present = await Promise.all(paths.map((path) => resolve(root, path)).map(async (path) => ((await stat(path, { throwIfNoEntry: false }))?.isFile() === true ? [path] : []))).then(
        (files) => ok(files.flat()),
        (cause: unknown) => fault<readonly string[]>({ kind: 'unread', subject: root, cause }),
    );
    return bind(present, (files): Promise<Result<readonly string[]>> => {
        if (files.length === 0) {
            return Promise.resolve(ok([]));
        }
        const target = 'lint:ast-grep';
        const subject = `nx ${target}`;
        const args = ['--report-style=short', '--color=never', ...files].map(quoteShellArg).join(' ');
        const ran = spawn('nx', ['run', '--project=rasm', `--target=${target}`, '--output-style=summary', `--args=${args}`], { cwd: root, signal, stdio: ['ignore', 'pipe', 'pipe'] });
        return Promise.all([once(ran, 'close'), text(ran.stdout), text(ran.stderr)]).then(
            ([, , stderr]): Result<readonly string[]> => {
                signal?.throwIfAborted();
                if (ran.exitCode === null) {
                    return fault({ kind: 'unstarted', subject, cause: ran.signalCode });
                }
                return ran.exitCode === 0 ? ok([]) : ok([stderr]);
            },
            (cause: unknown) => {
                signal?.throwIfAborted();
                return fault({ kind: 'unstarted', subject, cause });
            },
        );
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { diagnostics };
