import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import type { StackRequest, Stacks } from '@rasm/nx-pulumi/stacks';
import { Array, Effect, FileSystem, Match, Option, type Path, Record, Schema } from 'effect';
import { hostingStack, hostingValues } from '#/hosting-stack';
import manifest from '#/package.json' with { type: 'json' };
import { repositoryStack } from '#/repository-stack';

// --- [MODELS] --------------------------------------------------------------------------

const Value = Schema.Literals(Record.keys(hostingValues));

// --- [OPERATIONS] ----------------------------------------------------------------------

const subject = ({ context }: StackRequest) => Effect.flatMap(Effect.fromNullishOr(context.projectName), (project) => Effect.map(Effect.fromNullishOr(context.projectsConfigurations.projects[project]), ({ root }) => ({ project, directory: join(context.root, root) })));

const held = Effect.fnUntraced(function* (directory: string, production: boolean) {
    const fs = yield* FileSystem.FileSystem;
    const workDir = yield* fs.makeTempDirectoryScoped();
    const [candidates, listed] = yield* Effect.all([
        Effect.forEach(Record.keys(hostingValues), (value) => hostingStack(directory, value, production)),
        Effect.flatMap(
            Effect.tryPromise(() => LocalWorkspace.create({ workDir, projectSettings: { name: 'rasm-hosting', runtime: 'nodejs' } })),
            (workspace) => Effect.tryPromise(() => workspace.listStacks()),
        ),
    ]);
    const names = new Set(listed.map(({ name }) => name));
    return yield* Array.match(
        candidates.filter(({ stackName }) => names.has(stackName)),
        { onEmpty: () => Effect.map(hostingStack(directory, 'local', production), Array.of), onNonEmpty: Effect.succeed },
    );
}, Effect.scoped);

const inline = <A extends Record<string, unknown>, E>({ program, ...args }: { readonly projectName: string; readonly stackName: string; readonly program: Effect.Effect<A, E, FileSystem.FileSystem | Path.Path> }): InlineProgramArgs => ({
    ...args,
    program: () => Effect.runPromise(Effect.provide(program, layer)),
});

const hosted = (request: StackRequest, directory: string) =>
    Match.valueTags(request, {
        Install: () => Effect.map(hostingStack(directory, 'local', false), Array.of),
        Deploy: ({ stack }) =>
            Option.match(stack, {
                onNone: () => held(directory, false),
                onSome: (value) => Effect.flatMap(Schema.decodeUnknownEffect(Value)(value), (decoded) => Effect.map(hostingStack(directory, decoded, false), Array.of)),
            }),
        Release: () => held(directory, true),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const stacks: Stacks = (request) => Effect.runPromise(Effect.flatMap(subject(request), ({ project, directory }) => (project === manifest.name ? Effect.succeed([inline(repositoryStack(request.context))]) : Effect.map(hosted(request, directory), Array.map(inline)))).pipe(Effect.provide(layer)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { stacks };
