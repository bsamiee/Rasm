import { createRequire } from 'node:module';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import type { InlineProgramArgs, LocalWorkspaceOptions } from '@pulumi/pulumi/automation/index.js';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Data, Effect, Exit, FileSystem, flow, Path, type Scope } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type StackRequest = Data.TaggedEnum<{
    Install: { readonly context: ExecutorContext };
    Deploy: { readonly context: ExecutorContext; readonly production: boolean };
}>;
type Stated = InlineProgramArgs & Pick<LocalWorkspaceOptions, 'envVars'>;
type Stacks = (request: StackRequest) => Effect.Effect<readonly (readonly Stated[])[], { readonly _tag: string }, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>;

// --- [MODELS] --------------------------------------------------------------------------

const StackRequest = Data.taggedEnum<StackRequest>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const applied = Effect.fnUntraced(function* (programs: readonly string[], request: StackRequest, project: string, { stack: named, destroy = false }: { readonly stack?: string | undefined; readonly destroy?: boolean | undefined } = {}) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { root } = yield* Effect.fromNullishOr(request.context.projectsConfigurations.projects[project]);
    const entry = path.join(request.context.root, root, 'package.json');
    yield* Effect.forEach(
        programs,
        Effect.fnUntraced(function* (program) {
            const resolved = yield* Effect.try({ try: () => createRequire(entry).resolve(program), catch: ToolError.of(program, `unresolvable from ${entry}`) });
            const main = path.dirname(resolved);
            const [{ stacks }, automation] = yield* Effect.all([
                Effect.tryPromise({ try: (): Promise<{ readonly stacks: Stacks }> => import(pathToFileURL(resolved).href), catch: ToolError.of(program, `failed to load ${resolved}`) }),
                Effect.try({ try: (): typeof import('@pulumi/pulumi/automation/index.js') => createRequire(resolved)('@pulumi/pulumi/automation/index.js'), catch: ToolError.of(program, 'declares no @pulumi/pulumi') }),
            ]);
            const groups = yield* Effect.mapError(stacks(request), ToolError.of(program, 'stacks failed'));
            const selected = named === undefined ? groups : groups.map(Array.filter(({ stackName }) => stackName === named));
            yield* selected.flat().length > 0
                ? Effect.void
                : ToolError.make({
                      subject: program,
                      detail: `states no stack ${named} for ${project}, its stacks are ${groups
                          .flat()
                          .map(({ stackName }) => stackName)
                          .join(', ')}`,
                  });
            const one = Effect.fnUntraced(function* ({ envVars = {}, ...stated }: Stated) {
                const workDir = yield* fs.makeTempDirectoryScoped();
                const printed = (text: string) => process.stderr.write(text.replaceAll(/^(?=.)/gmu, `${stated.stackName} | `));
                const failed = ToolError.of(stated.stackName, destroy ? 'was not destroyed' : 'was not applied');
                const held = yield* Effect.tryPromise({ try: () => automation.LocalWorkspace.createOrSelectStack(stated, { workDir, envVars, projectSettings: { name: stated.projectName, runtime: 'nodejs', main } }), catch: failed });
                yield* Effect.tryPromise({
                    try: (signal) => {
                        const handlers = { signal, onOutput: printed, onError: printed, color: 'never', refresh: true, runProgram: true } as const;
                        return destroy ? held.destroy({ ...handlers, remove: true }) : held.up(handlers);
                    },
                    catch: failed,
                });
            }, Effect.scoped);
            yield* Effect.forEach(selected, (group) => Effect.flatMap(Effect.forEach(group, flow(one, Effect.exit), { concurrency: 'unbounded' }), Exit.asVoidAll), { discard: true });
        }),
        { discard: true },
    );
    return true;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applied, StackRequest, type Stacks, type Stated };
