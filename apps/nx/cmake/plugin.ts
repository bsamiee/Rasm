import { DependencyType, hashArray, type RawProjectGraphDependency } from '@nx/devkit';
import { getNamedInputs, hashObject } from '@nx/devkit/internal';
import { inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { type Array, Effect, Result, Schema } from 'effect';
import { CONFIG, memberInputs, memberTargets, released } from '#/targets';
import { MANIFEST, PRESETS, planted, resolved, type Tree } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = [PRESETS, MANIFEST, CONFIG] as const;
const TECHNOLOGIES = ['cmake'];

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    launcher: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    locks: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
}).annotate({
    description: 'launcher: argument list placed before every cmake, ctest, cpack, and clang tool invocation, entering the environment that holds the toolchain, absent runs each from PATH. locks: workspace files pinning that environment, read by every target as an input',
});

type Options = typeof Options.Type;

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Options,
    units: ({ files, options, path }: Request<Options>) =>
        Effect.succeed(files.filter((file) => path.basename(file) === PRESETS && files.includes(path.join(path.dirname(file), MANIFEST))).map((file): Unit => ({ key: file, file, salt: hashArray([hashObject(options), ...files.filter((other) => !path.relative(path.dirname(file), other).startsWith('..'))]) }))),
    read: (missing: Array.NonEmptyReadonlyArray<Unit>, { options, context: { workspaceRoot }, path, placed }: Request<Options>) =>
        Effect.forEach(missing, ({ file }) =>
            Effect.map(
                Effect.result(resolved(workspaceRoot, file, options.launcher, (preset) => planted(path, placed, path.dirname(file), 'cmake', preset))),
                Result.map(({ tree, partial }) => ({ report: tree, partial })),
            ),
        ),
    sources: ({ inputs }: Tree, { file }: Unit, { options }: Request<Options>) => ({ tracked: [file, ...inputs, ...options.locks] }),
    projects: (tree: Tree, _: Unit, { options, context, plugin, path, placed, files }: Request<Options>) =>
        tree.members
            .filter(({ targets, installs, tested }) => targets.length > 0 || installs || tested)
            .map(
                (member): Project => ({
                    file: path.join(tree.root, PRESETS),
                    root: member.root,
                    name: member.name,
                    technologies: TECHNOLOGIES,
                    clean: 'cmake',
                    members: memberTargets(plugin, path, tree, member, options, placed, new Set(files)),
                    ...released(tree, member),
                    configuration: { namedInputs: { default: memberInputs(path, tree, member, options, { default: ['{projectRoot}/**/*'], ...getNamedInputs(member.root, context) }.default) } },
                }),
            ),
    edges: (reports) => Effect.succeed(reports.flatMap(([, { members }]) => members.flatMap(({ root, links }) => links.map((link): RawProjectGraphDependency => ({ source: root, target: link.root, type: DependencyType.static, sourceFile: link.file }))))),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
