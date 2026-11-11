import { basename, join, sep } from 'node:path';
import { env, platform } from 'node:process';
import { executor } from '@rasm/nx-devkit/executor';
import { printed, ran, ToolError } from '@rasm/nx-devkit/process';
import { Effect, Record, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({
    runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables every invocation runs with, over the caller's" }),
    applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Application bundles the invocations install, a running one named in the last line as needing a relaunch' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to every invocation' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
}).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed, stopping at the first failure" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const commands = Effect.fnUntraced(function* ({ runs, env: stated = {}, applications = [], args = [] }: typeof Run.Type, root: string) {
    const cleared = Record.fromIterableWith([...Object.keys(env).filter((name) => name.startsWith('NX_')), 'LERNA_PACKAGE_NAME'], (name) => [name, undefined]);
    yield* Effect.forEach(
        runs,
        ([program, ...given]) =>
            Effect.map(ran(program, [...given, ...args], { cwd: root, env: { ...cleared, ...stated }, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code).pipe(
                Effect.catchReason('PlatformError', 'NotFound', () => ToolError.make({ subject: program, detail: `is absent on this ${platform} host, the target runs on macOS with Xcode` })),
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
    return true;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Run, (run, context) => commands(run, context.root));

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, execute, Run };
