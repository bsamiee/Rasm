import { createRequire } from 'node:module';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import type { InlineProgramArgs, LocalWorkspaceOptions } from '@pulumi/pulumi/automation/index.js';
import { Array, Cause, Data, Effect, Exit, FileSystem, flow, Path, Schema, type Scope, Struct } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type StackRequest = Data.TaggedEnum<{
    Install: { readonly context: ExecutorContext };
    Deploy: { readonly context: ExecutorContext; readonly production: boolean };
}>;
type Stated = InlineProgramArgs & Pick<LocalWorkspaceOptions, 'envVars'>;
type Stacks = (request: StackRequest) => Effect.Effect<readonly (readonly Stated[])[], { readonly _tag: string }, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPERATIONS = { up: 'was not applied', preview: 'was not previewed', destroy: 'was not destroyed' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const StackRequest = Data.taggedEnum<StackRequest>();
const Operation = Schema.Literals(Struct.keys(OPERATIONS)).annotate({ description: 'What runs on each stack: up applies it, preview prints the changes an apply makes and makes none, destroy destroys and removes it', default: 'up' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const applied = Effect.fnUntraced(function* (programs: readonly string[], request: StackRequest, { stack: named, operation }: { readonly stack?: string | undefined; readonly operation: keyof typeof OPERATIONS }) {
    const [fs, path, project] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.fromNullishOr(request.context.projectName)]);
    const { root } = yield* Effect.fromNullishOr(request.context.projectsConfigurations.projects[project]);
    const entry = path.join(request.context.root, root, 'package.json');
    yield* Effect.forEach(
        programs,
        Effect.fnUntraced(function* (program) {
            const resolved = yield* Effect.try({ try: () => createRequire(entry).resolve(program), catch: refused(program, `unresolvable from ${entry}`) });
            const main = path.dirname(resolved);
            const [{ stacks }, automation] = yield* Effect.all([
                Effect.tryPromise({ try: (): Promise<{ readonly stacks: Stacks }> => import(pathToFileURL(resolved).href), catch: refused(program, `failed to load ${resolved}`) }),
                Effect.try({ try: (): typeof import('@pulumi/pulumi/automation/index.js') => createRequire(resolved)('@pulumi/pulumi/automation/index.js'), catch: refused(program, 'declares no @pulumi/pulumi') }),
            ]);
            const groups = yield* Effect.mapError(stacks(request), refused(program, 'stacks failed'));
            const selected = named === undefined ? groups : groups.map(Array.filter(({ stackName }) => stackName === named));
            yield* selected.flat().length > 0
                ? Effect.void
                : Effect.fail(
                      refused(
                          program,
                          `states no stack ${named} for ${project}, its stacks are ${groups
                              .flat()
                              .map(({ stackName }) => stackName)
                              .join(', ')}`,
                      )(),
                  );
            const one = Effect.fnUntraced(function* ({ envVars = {}, ...stated }: Stated) {
                const workDir = yield* fs.makeTempDirectoryScoped();
                const printed = (text: string) => process.stderr.write(text.replaceAll(/^(?=.)/gmu, `${stated.stackName} | `));
                const failed = refused(stated.stackName, OPERATIONS[operation]);
                const held = yield* Effect.tryPromise({ try: () => automation.LocalWorkspace.createOrSelectStack(stated, { workDir, envVars, projectSettings: { name: stated.projectName, runtime: 'nodejs', main } }), catch: failed });
                yield* Effect.tryPromise({
                    try: (signal): Promise<unknown> => {
                        const handlers = { signal, onOutput: printed, onError: printed, color: 'never', refresh: true, runProgram: true } as const;
                        return { up: () => held.up(handlers), preview: () => held.preview(handlers), destroy: () => held.destroy({ ...handlers, remove: true }) }[operation]();
                    },
                    catch: failed,
                });
            }, Effect.scoped);
            yield* Effect.forEach(selected, (group) => Effect.flatMap(Effect.forEach(group, flow(one, Effect.exit), { concurrency: 'unbounded' }), Exit.asVoidAll), { discard: true });
        }),
        { discard: true },
    );
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => Effect.runPromise(Effect.provide(Effect.scoped(effect), layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export { applied, Operation, run, StackRequest, type Stacks, type Stated };
