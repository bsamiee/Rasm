import type { ConformanceRule, ConformanceViolation, WritableConformanceTree } from '@nx/conformance';
import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache, type ProjectNodesWithFiles } from '@nx/conformance/testing';
import { type ProjectConfiguration, type ProjectGraphExternalNode, writeJson } from '@nx/devkit';
import { Array, Effect, Order } from 'effect';
import { onTestFinished } from 'vitest';

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

const violations = Effect.fn('violations')(function* <Options>(rule: ConformanceRule<Options>, ruleOptions: Options, { projects = [], nxJson, files = {}, externalNodes = {} }: Workspace = {}) {
    const services = yield* Effect.context<never>();
    const written = (writable: WritableConformanceTree) =>
        Effect.runPromiseWith(services)(
            Effect.sync(() => {
                Object.entries(files).forEach(([file, content]) => writable.write(file, content));
                Array.fromNullishOr(nxJson).forEach((settings) => writeJson(writable, 'nx.json', settings));
            }),
        );
    const { tree, cleanup } = yield* Effect.promise(() => createReadOnlyTree(written));
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = yield* Effect.promise(() => createStubbedProjectGraphAndFileMapCache(tree, [...projects]));
    const { details } = yield* Effect.promise(() => rule.implementation({ tree, projectGraph: { ...projectGraph, externalNodes: { ...projectGraph.externalNodes, ...externalNodes } }, fileMapCache, ruleOptions }));
    return details.violations;
});

const reported = (found: readonly ConformanceViolation[], rows: readonly string[]) =>
    Array.sort(
        found.map(({ sourceProject, file, message }) => `${sourceProject ?? file ?? 'workspace'} | ${message}`).map((text) => rows.find((row) => text.startsWith(row)) ?? text),
        Order.mapInput(Order.Number, (text: string) => (rows.includes(text) ? rows.indexOf(text) : rows.length)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { project, reported, violations };
