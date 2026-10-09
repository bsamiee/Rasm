// biome-ignore-all lint/correctness/noNodejsModules: Node owns cancellable writers, process streams, and filesystem paths

import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { stat } from 'node:fs/promises';
import { matchesGlob, relative, resolve } from 'node:path';
import { text } from 'node:stream/consumers';
import { quoteShellArg } from 'nx/src/utils/shell-quoting.js';
import { bind, fault, ok, type Result, rendered } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface WriterProject {
    readonly name: string;
    readonly writers: readonly { readonly target: string; readonly files: readonly string[]; readonly configuration?: string }[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const formatEdited = async (root: string, paths: readonly string[] | 'all', signal: AbortSignal, project: WriterProject): Promise<readonly string[]> => {
    const run = async (program: string, args: readonly string[]): Promise<Result<string>> => {
        const child = spawn(program, args, { cwd: root, signal, stdio: ['ignore', 'pipe', 'pipe'] });
        const exited = once(child, 'close').then(
            (): Result<number> => (child.exitCode === null ? fault({ kind: 'refused', subject: program, text: `terminated by ${child.signalCode}` }) : ok(child.exitCode)),
            (cause: unknown): Result<number> => fault({ kind: 'unstarted', subject: program, cause }),
        );
        const [status, stdout, stderr] = await Promise.all([exited, text(child.stdout), text(child.stderr)]);
        signal.throwIfAborted();
        return bind(status, (code): Result<string> => (code === 0 ? ok(stdout) : fault({ kind: 'exited', subject: program, code, stderr })));
    };
    const messages = (result: Result<unknown>): readonly string[] => (result.kind === 'ok' ? [] : result.faults.map((value) => (value.kind === 'exited' ? `${value.subject} exited ${value.code}.\n${value.stderr}` : rendered([value]))));
    if (paths.length === 0) {
        return [];
    }
    const present: Result<readonly string[] | 'all'> =
        paths === 'all'
            ? ok('all')
            : await bind(
                  await run('git', ['--literal-pathspecs', 'ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ...paths.map((path) => relative(root, resolve(root, path)))]),
                  async (printed): Promise<Result<readonly string[]>> =>
                      ok(
                          (
                              await Promise.all(
                                  printed
                                      .split('\0')
                                      .filter((path) => path.length > 0)
                                      .map(async (path) => ((await stat(resolve(root, path), { throwIfNoEntry: false }))?.isFile() ? [path] : [])),
                              )
                          ).flat(),
                      ),
              );
    if (present.kind === 'fault') {
        return messages(present);
    }
    if (present.value.length === 0) {
        return [];
    }
    const invocations = project.writers.flatMap((writer) => {
        const globs = writer.files;
        const files = present.value === 'all' ? [] : present.value.filter((path) => globs.some((glob) => !glob.startsWith('!') && matchesGlob(path, glob)) && globs.every((glob) => !(glob.startsWith('!') && matchesGlob(path, glob.slice(1)))));
        return present.value !== 'all' && files.length === 0
            ? []
            : [['run', `--project=${project.name}`, `--target=${writer.target}`, '--output-style=summary', ...(present.value === 'all' ? [] : [...(writer.configuration === undefined ? [] : [`--configuration=${writer.configuration}`]), `--args=${files.map((file) => quoteShellArg(resolve(root, file))).join(' ')}`])]];
    });
    return invocations.reduce<Promise<readonly string[]>>(async (before, invocation) => [...(await before), ...messages(await run('nx', invocation))], Promise.resolve([]));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { WriterProject };
export { formatEdited };
