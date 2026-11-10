import { basename, join, sep } from 'node:path';
import { env, platform } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { printed, ran, runtime, ToolError } from '@rasm/nx-devkit/process';
import { Effect, Record, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({ runs: Schema.Array(Schema.NonEmptyArray(Schema.String)), env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)), applications: Schema.optionalKey(Schema.Array(Schema.String)), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const commands = Effect.fnUntraced(function* ({ runs, env: stated = {}, applications = [], args = [] }: typeof Run.Type, root: string) {
    const cleared = Record.fromIterableWith([...Object.keys(env).filter((name) => name.startsWith('NX_')), 'LERNA_PACKAGE_NAME'], (name) => [name, undefined]);
    yield* Effect.forEach(
        runs,
        ([program, ...given]) =>
            Effect.map(ran(program, [...given, ...args], { cwd: root, env: { ...cleared, ...stated }, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code).pipe(
                Effect.catchReason('PlatformError', 'NotFound', () => ToolError.make({ subject: program, detail: `is absent on this ${platform} host; projects tagged os:darwin run on macOS with Xcode` })),
                Effect.filterOrFail(
                    (code) => code === 0,
                    (code) => ToolError.make({ subject: program, detail: `exited ${code}` }),
                ),
            ),
        { discard: true },
    );
    const running = applications.length === 0 ? [] : (yield* printed('ps', ['-Ao', 'comm='], { cwd: root })).split('\n');
    const stale = applications.filter((application) => running.some((executable) => executable.startsWith(join(application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Schema.decodeUnknownEffect(Run)(options).pipe(
        Effect.flatMap((run) => commands(run, context.root)),
        runtime.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, execute, Run };
