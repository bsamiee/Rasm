import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import type { StackRequest, Stacks } from '@rasm/nx-pulumi/stacks';
import { Array, Cause, Effect, FileSystem, type Layer, Match, Option, Record } from 'effect';
import { deployed } from '#/command';
import { hostingRows, hostingStack, hostingValues } from '#/hosting-stack';
import manifest from '#/package.json' with { type: 'json' };
import { platformStacks } from '#/platform-stack';
import { repositoryStack } from '#/repository-stack';

// --- [CONSTANTS] -----------------------------------------------------------------------

const project = manifest.name.replace('@', '').replace('/', '-');

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (context: ExecutorContext, production: boolean) {
    const [fs, rows] = yield* Effect.all([FileSystem.FileSystem, hostingRows]);
    const workDir = yield* fs.makeTempDirectoryScoped();
    const [candidates, listed] = yield* Effect.all([
        Effect.forEach(Record.keys(rows), (value) => hostingStack(context, rows, value, production)),
        Effect.flatMap(
            Effect.tryPromise(() => LocalWorkspace.create({ workDir, projectSettings: { name: project, runtime: 'nodejs' } })),
            (workspace) => Effect.tryPromise(() => workspace.listStacks()),
        ),
    ]);
    const names = new Set(listed.map(({ name }) => name));
    return yield* Array.match(
        candidates.filter(({ stackName }) => names.has(stackName)),
        { onEmpty: () => Effect.map(hostingStack(context, rows, 'local', production), Array.of), onNonEmpty: Effect.succeed },
    );
}, Effect.scoped);

const inline = <E>({ program, ...args }: { readonly stackName: string; readonly envVars?: Readonly<Record<string, string>>; readonly program: Effect.Effect<Readonly<Record<string, unknown>>, E, Layer.Success<typeof layer>> }): InlineProgramArgs => ({
    ...args,
    projectName: project,
    program: () => Effect.runPromise(Effect.provide(program, layer)),
});

const owned = Effect.fnUntraced(function* (request: StackRequest) {
    const named = Match.valueTags(request, { Install: () => Option.none<string>(), Deploy: ({ stack }) => stack, Release: () => Option.none<string>() });
    const envVars = yield* deployed;
    const repository = inline({ ...(yield* repositoryStack(request.context, envVars)), envVars });
    const platform = Effect.map(
        platformStacks(envVars),
        Array.map((stack) => inline({ ...stack, envVars })),
    );
    const declared = Option.contains(named, repository.stackName) ? [repository] : [repository, ...(yield* platform)];
    return yield* Option.match(named, {
        onNone: () => Effect.succeed(declared),
        onSome: (name) =>
            Effect.map(
                Effect.fromOption(
                    Array.findFirst(declared, ({ stackName }) => stackName === name),
                    () => new Cause.NoSuchElementError(`${manifest.name} holds no stack named ${name}`),
                ),
                Array.of,
            ),
    });
});

const hosted = Match.typeTags<StackRequest>()({
    Install: ({ context }) => Effect.map(hostingStack(context, hostingValues, 'local', false), Array.of),
    Deploy: ({ context, stack }) =>
        Option.match(stack, {
            onNone: () => held(context, false),
            onSome: (value) =>
                hostingRows.pipe(
                    Effect.flatMap((rows) => hostingStack(context, rows, value, false)),
                    Effect.map(Array.of),
                ),
        }),
    Release: ({ context }) => held(context, true),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const stacks: Stacks = (request) =>
    Effect.runPromise(
        Effect.provide(
            Effect.gen(function* () {
                return request.context.projectName === manifest.name ? yield* owned(request) : Array.map(yield* hosted(request), inline);
            }),
            layer,
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { stacks };
