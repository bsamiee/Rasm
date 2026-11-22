import type { ConformanceRule, WritableConformanceTree } from '@nx/conformance';
import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache, type ProjectNodesWithFiles } from '@nx/conformance/testing';
import { type ProjectConfiguration, type ProjectGraphExternalNode, writeJson } from '@nx/devkit';
import { Effect } from 'effect';
import { onTestFinished } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Workspace {
    readonly projects?: readonly ProjectNodesWithFiles[];
    readonly nxJson?: Readonly<Record<string, unknown>>;
    readonly externalNodes?: Readonly<Record<string, ProjectGraphExternalNode>>;
    readonly nonProjectFiles?: readonly string[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (name: string, data: Omit<ProjectConfiguration, 'root'> & { readonly root?: string } = {}, files: readonly string[] = [], depends: readonly string[] = []): ProjectNodesWithFiles => ({
    projectNode: { name, type: 'lib', data: { root: `libs/${name}`, ...data } },
    projectFiles: files.map((file, at) => ({ projectRootRelativeFile: file, depsItCreates: at === 0 ? [...depends] : [] })),
});

const violations = Effect.fn('violations')(function* <Options>(rule: ConformanceRule<Options>, ruleOptions: Options, { projects = [], nxJson, externalNodes = {}, nonProjectFiles = [] }: Workspace = {}) {
    const services = yield* Effect.context<never>();
    const written = (writable: WritableConformanceTree) => Effect.runPromiseWith(services)(Effect.sync(() => (nxJson === undefined ? undefined : writeJson(writable, 'nx.json', nxJson))));
    const { tree, cleanup } = yield* Effect.promise(() => createReadOnlyTree(written));
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = yield* Effect.promise(() => createStubbedProjectGraphAndFileMapCache(tree, [...projects]));
    const { details } = yield* Effect.promise(() =>
        rule.implementation({
            tree,
            projectGraph: { ...projectGraph, externalNodes: { ...projectGraph.externalNodes, ...externalNodes } },
            fileMapCache: { ...fileMapCache, fileMap: { ...fileMapCache.fileMap, nonProjectFiles: [...fileMapCache.fileMap.nonProjectFiles, ...nonProjectFiles.map((file) => ({ file, hash: file }))] } },
            ruleOptions,
        }),
    );
    return details.violations;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { project, violations };
