import { hashArray, type RawProjectGraphDependency } from '@nx/devkit';
import { getNamedInputs, globWithWorkspaceContext } from '@nx/devkit/internal';
import { inferred, MANIFESTS, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { Array, Effect, Option, Record, Result, Schema } from 'effect';
import { crateInputs, crateTargets, library, released, workspaceTargets } from '#/targets';
import { CLIPPY, CONFIG, DENY, MANIFEST, workspaces } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly projects: readonly Project[];
    readonly edges: readonly RawProjectGraphDependency[];
}
interface Rooted extends Unit {
    readonly marked: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = [MANIFEST, 'rust-toolchain.toml', 'rust-toolchain', ...CONFIG, ...CLIPPY, '.config/nextest.toml', ...Object.values(DENY).flat()] as const;
const TECHNOLOGIES = ['rust'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzed = Effect.fnUntraced(function* ({ files: matched, context, plugin, path, placed }: Request<unknown>, marked: readonly string[]) {
    const files = new Set(matched);
    const manifests = matched.filter((file) => path.basename(file) === MANIFEST).toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right));
    const [resolved, failures] = Array.partition(yield* workspaces(context.workspaceRoot, manifests, files), ([file, result]) => Result.mapError(result, (error): readonly [string, Error] => [file, error]));
    const reported = Option.map(placed('llvm-cov'), (at) => path.join(at, 'lcov.info'));
    const projects = resolved.flatMap((workspace) => {
        const crates = Record.fromIterableWith(workspace.members, ({ crate, root }) => {
            const implicit = crate.metadata?.nx?.implicitDependencies;
            return [
                root,
                {
                    name: Option.match(Array.findFirst(crate.targets, library), { onNone: () => crate.name, onSome: ({ name }) => name }),
                    members: crateTargets(plugin, path, workspace, files, reported, crate, root),
                    ...released(path, plugin, crate, workspace.root),
                    configuration: {
                        namedInputs: {
                            default: crateInputs(path, workspace, files, root, { default: ['{projectRoot}/**/*'], ...getNamedInputs(root, context) }.default),
                        },
                        ...(implicit !== undefined && { implicitDependencies: [...implicit] }),
                    },
                },
            ] as const;
        });
        const own = crates[workspace.root];
        const rooted = workspaceTargets(plugin, path, workspace);
        const top = own === undefined && !marked.includes(workspace.root) ? {} : { [workspace.root]: { ...own, members: { ...own?.members, check: { ...own?.members.check, ...rooted.check }, upgrade: rooted.upgrade } } };
        return Record.collect({ ...crates, ...top }, (root, project): Project => ({ file: path.join(workspace.root, MANIFEST), root, technologies: TECHNOLOGIES, clean: 'cargo', ...project }));
    });
    return Result.succeed({ report: { projects, edges: resolved.flatMap((workspace) => workspace.edges) } satisfies Graph, failures });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context: { workspaceRoot }, path }: Request<unknown>) {
        const folders = files.filter((file) => path.basename(file) === MANIFEST).map((file) => path.dirname(file));
        const held = (pattern: string) =>
            Effect.promise(() =>
                globWithWorkspaceContext(
                    workspaceRoot,
                    folders.map((folder) => path.join(folder, pattern)),
                ),
            );
        const [sources, markers] = yield* Effect.all([held('**/*.rs'), held(MANIFESTS)]);
        return Array.take(files, 1).map((file): Rooted => ({ key: MANIFEST, file, salt: hashArray([...sources, ...markers]), marked: markers.map((at) => path.dirname(at)) }));
    }),
    read: (missing: Array.NonEmptyReadonlyArray<Rooted>, request: Request<unknown>) => Effect.map(analyzed(request, Array.headNonEmpty(missing).marked), (reading) => missing.map(() => reading)),
    sources: () => ({ tracked: FILES.map((file) => `**/${file}`) }),
    projects: ({ projects }: Graph) => projects,
    edges: (reports) => Effect.succeed(reports.flatMap(([, { edges }]) => edges)),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
