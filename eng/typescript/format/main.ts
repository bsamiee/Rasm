// biome-ignore-all lint/correctness/noNodejsModules: Arguments and extensions of the named files come from Node

import { execFileSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { extname, relative, resolve } from 'node:path';
import process from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { normalizePath, readJsonFile, workspaceRoot } from '@nx/devkit';
import { Array, Data, Effect, Option } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITERS: readonly { readonly target: string; readonly extensions: Option.Option<readonly string[]> }[] = [
    { target: 'format:biome', extensions: Option.none() },
    { target: 'format:ruff-format', extensions: Option.some(['.py', '.pyi', '.pyw', '.ipynb']) },
    { target: 'format:yamlfmt', extensions: Option.some(['.yaml', '.yml']) },
    { target: 'format:google-java-format', extensions: Option.some(['.java']) },
    { target: 'format:swift-format', extensions: Option.some(['.swift']) },
];

// --- [ERRORS] --------------------------------------------------------------------------

class WriterExited extends Data.TaggedError('WriterExited')<{ readonly target: string; readonly code: number }> {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const argued = process.argv.slice(2).map((path) => normalizePath(relative(workspaceRoot, resolve(path))));
const files = execFileSync('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z'], { cwd: workspaceRoot, encoding: 'utf8' }).split('\0').slice(0, -1);
const paths = Array.intersection(argued, files).filter((path) => existsSync(resolve(workspaceRoot, path)));
const { name } = readJsonFile<{ readonly name: string }>(`${workspaceRoot}/package.json`);
Effect.validate(
    WRITERS,
    ({ target, extensions }) => {
        const named = Option.match(extensions, { onNone: () => paths, onSome: (listed) => paths.filter((path) => listed.includes(extname(path))) });
        return named.length === 0
            ? Effect.void
            : Effect.scoped(Effect.flatMap(ChildProcess.make('nx', ['run', `${name}:${target}`, '--configuration=files', '--', ...named], { stdout: 'inherit', stderr: 'inherit' }), (handle) => Effect.flatMap(handle.exitCode, (code) => (code === 0 ? Effect.void : Effect.fail(new WriterExited({ target, code }))))));
    },
    { concurrency: 'unbounded', discard: true },
).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
