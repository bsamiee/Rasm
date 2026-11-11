import type { RawProjectGraphDependency } from '@nx/devkit';
import { inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { Array, Effect, Option, Record, Result, Schema } from 'effect';
import { crateTargets, library, released, workspaceTargets } from '#/targets';
import { CONFIG, DENY, MANIFEST, workspaces } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly projects: readonly Project[];
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = [MANIFEST, 'rust-toolchain.toml', 'rust-toolchain', ...CONFIG, 'clippy.toml', '.clippy.toml', '.config/nextest.toml', ...Object.values(DENY).flat()] as const;
const TECHNOLOGIES = ['rust'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzed = Effect.fnUntraced(function* ({ files: matched, context, plugin, path, named }: Request<unknown>) {
    const files = new Set(matched);
    const manifests = matched.filter((file) => path.basename(file) === MANIFEST).toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right));
    const [resolved, failures] = Array.partition(yield* workspaces(context.workspaceRoot, manifests, files), ([file, result]) => Result.mapError(result, (error): readonly [string, Error] => [file, error]));
    const projects = resolved.flatMap((workspace) => {
        const crates = Record.fromIterableWith(workspace.members, ({ crate, root }) => {
            const { targets, release } = released(plugin, path, crate, workspace.root);
            const implicit = crate.metadata?.nx?.implicitDependencies;
            return [
                root,
                {
                    name: Option.match(Array.findFirst(crate.targets, library), { onNone: () => crate.name, onSome: ({ name }) => name }),
                    configuration: { ...(release !== undefined && { release }), ...(implicit !== undefined && { implicitDependencies: [...implicit] }) },
                    published: targets,
                    members: crateTargets(plugin, path, workspace, files, [named(root), `^${named(root)}`], crate, root),
                },
            ] as const;
        });
        const own = crates[workspace.root];
        const rooted = workspaceTargets(plugin, path, workspace);
        return Record.collect(
            { ...crates, [workspace.root]: { name: undefined, configuration: {}, published: {}, ...own, members: { ...own?.members, check: { ...own?.members.check, ...rooted.check }, upgrade: rooted.upgrade } } },
            (root, { name, configuration, published, members }): Project => ({
                file: path.join(workspace.root, MANIFEST),
                root,
                technologies: TECHNOLOGIES,
                clean: 'cargo',
                members,
                targets: published,
                configuration,
                ...(name !== undefined && { name }),
            }),
        );
    });
    return Result.succeed({ report: { projects, edges: resolved.flatMap((workspace) => workspace.edges) } satisfies Graph, failures });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Schema.Struct({}),
    units: ({ files }: Request<unknown>) => Effect.succeed(Array.take(files, 1).map((file): Unit => ({ key: MANIFEST, file }))),
    read: (missing: Array.NonEmptyReadonlyArray<Unit>, request: Request<unknown>) => Effect.map(analyzed(request), (reading) => missing.map(() => reading)),
    sources: () => ({ tracked: FILES.map((file) => `**/${file}`) }),
    projects: ({ projects }: Graph) => projects,
    edges: (reports) => Effect.succeed(reports.flatMap(([, { edges }]) => edges)),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
