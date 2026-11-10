import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import type { StackRequest, Stacks } from '@rasm/nx-pulumi/stacks';
import { Array, Cause, Effect, FileSystem, flow, type Layer, Match, Option, Record, Schema } from 'effect';
import { run } from '#/command';
import { hostingStack, hostingValues } from '#/hosting-stack';
import manifest from '#/package.json' with { type: 'json' };
import { platformStack } from '#/platform-stack';
import { repositoryStack } from '#/repository-stack';

// --- [CONSTANTS] -----------------------------------------------------------------------

const project = manifest.name.replace('@', '').replace('/', '-');

// --- [MODELS] --------------------------------------------------------------------------

const Value = Schema.Literals(Record.keys(hostingValues));
const Deployed = Schema.fromJsonString(Schema.Record(Schema.String, Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (directory: string, production: boolean) {
    const fs = yield* FileSystem.FileSystem;
    const workDir = yield* fs.makeTempDirectoryScoped();
    const [candidates, listed] = yield* Effect.all([
        Effect.forEach(Record.keys(hostingValues), (value) => hostingStack(directory, value, production)),
        Effect.flatMap(
            Effect.tryPromise(() => LocalWorkspace.create({ workDir, projectSettings: { name: project, runtime: 'nodejs' } })),
            (workspace) => Effect.tryPromise(() => workspace.listStacks()),
        ),
    ]);
    const names = new Set(listed.map(({ name }) => name));
    return yield* Array.match(
        candidates.filter(({ stackName }) => names.has(stackName)),
        { onEmpty: () => Effect.map(hostingStack(directory, 'local', production), Array.of), onNonEmpty: Effect.succeed },
    );
}, Effect.scoped);

const inline = <A extends Record<string, unknown>, E>({ program, ...args }: { readonly stackName: string; readonly envVars?: Readonly<Record<string, string>>; readonly program: Effect.Effect<A, E, Layer.Success<typeof layer>> }): InlineProgramArgs => ({
    ...args,
    projectName: project,
    program: () => Effect.runPromise(Effect.provide(program, layer)),
});

const owned = Effect.fnUntraced(function* (request: StackRequest) {
    const named = Match.valueTags(request, { Install: () => Option.none<string>(), Deploy: ({ stack }) => stack, Release: () => Option.none<string>() });
    const repository = inline(yield* repositoryStack(request.context));
    const platform = run('doppler', ['secrets', 'download', '--no-file', '--format', 'json', '--config', 'deploy']).pipe(Effect.flatMap(Schema.decodeEffect(Deployed)), Effect.flatMap(platformStack), Effect.map(inline));
    const declared = Option.contains(named, repository.stackName) ? [repository] : [repository, yield* platform];
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

const hosted = (request: StackRequest, directory: string) =>
    Match.valueTags(request, {
        Install: () => Effect.map(hostingStack(directory, 'local', false), Array.of),
        Deploy: ({ stack }) =>
            Option.match(stack, {
                onNone: () => held(directory, false),
                onSome: (value) =>
                    Effect.map(
                        Effect.flatMap(Schema.decodeUnknownEffect(Value)(value), (decoded) => hostingStack(directory, decoded, false)),
                        Array.of,
                    ),
            }),
        Release: () => held(directory, true),
    });

const selected = Effect.fnUntraced(function* (request: StackRequest) {
    const { root, projectName, projectsConfigurations } = request.context;
    const selection = yield* Effect.fromNullishOr(projectName);
    const directory = join(root, (yield* Effect.fromNullishOr(projectsConfigurations.projects[selection])).root);
    return selection === manifest.name ? yield* owned(request) : Array.map(yield* hosted(request, directory), inline);
}, Effect.provide(layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const stacks: Stacks = flow(selected, Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { stacks };
