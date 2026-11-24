import type { ConformanceRule, ConformanceViolation, WritableConformanceTree } from '@nx/conformance';
import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache, type ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { ProjectConfiguration, ProjectGraphExternalNode } from '@nx/devkit';
import { Array, Effect, Order } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Workspace {
    readonly projects?: readonly ProjectNodesWithFiles[];
    readonly nxJson?: Readonly<Record<string, unknown>>;
    readonly files?: Readonly<Record<string, string>>;
    readonly externalNodes?: Readonly<Record<string, ProjectGraphExternalNode>>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (name: string, data: Omit<ProjectConfiguration, 'root'> & { readonly root?: string } = {}, files: readonly string[] = [], depends: readonly string[] = []): ProjectNodesWithFiles => ({
    projectNode: { name, type: 'lib', data: { root: `libs/${name}`, ...data } },
    projectFiles: files.map((file, at) => ({ projectRootRelativeFile: file, depsItCreates: at === 0 ? [...depends] : [] })),
});

const violations = <Options>(rule: ConformanceRule<Options>, ruleOptions: Options, { projects = [], nxJson, files = {}, externalNodes = {} }: Workspace = {}) =>
    Effect.gen(function* () {
        const stated = [...Object.entries(files), ...Array.fromNullishOr(nxJson).map((settings) => ['nx.json', JSON.stringify(settings)] as const)];
        const services = yield* Effect.context<never>();
        const written = (writable: WritableConformanceTree) => Effect.runPromiseWith(services)(Effect.sync(() => stated.forEach(([file, content]) => writable.write(file, content))));
        const { tree } = yield* Effect.acquireRelease(
            Effect.promise(() => createReadOnlyTree(written)),
            ({ cleanup }) => Effect.sync(cleanup),
        );
        const { projectGraph, fileMapCache } = yield* Effect.promise(() => createStubbedProjectGraphAndFileMapCache(tree, [...projects]));
        const { details } = yield* Effect.promise(() => rule.implementation({ tree, projectGraph: { ...projectGraph, externalNodes: { ...projectGraph.externalNodes, ...externalNodes } }, fileMapCache, ruleOptions }));
        return details.violations;
    }).pipe(Effect.scoped, Effect.runPromise);

const reported = (found: readonly ConformanceViolation[], rows: readonly string[]) =>
    Array.sort(
        found.map(({ sourceProject, file, message }) => `${sourceProject ?? file ?? 'workspace'} | ${message}`).map((text) => rows.find((row) => text.startsWith(row)) ?? text),
        Order.mapInput(Order.Number, (text: string) => (rows.includes(text) ? rows.indexOf(text) : rows.length)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { project, reported, violations };
