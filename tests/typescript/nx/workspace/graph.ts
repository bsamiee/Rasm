import { createProjectGraphAsync, type ProjectGraph, type ProjectGraphProjectNode, type TargetConfiguration, workspaceRoot } from '@nx/devkit';
import { Context, Effect, Layer, Record } from 'effect';
import { type NxJsonConfiguration, readNxJson } from 'nx/src/config/nx-json';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stated {
    readonly node: ProjectGraphProjectNode;
    readonly target: string;
    readonly configuration: TargetConfiguration;
    readonly task: string;
    readonly tasks: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SLOW = { timeout: 600_000 } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const stated = (projectGraph: ProjectGraph): readonly Stated[] =>
    Object.values(projectGraph.nodes).flatMap((node) =>
        Record.collect(node.data.targets ?? {}, (target, configuration) => ({ node, target, configuration, task: `${node.name}:${target}`, tasks: [`${node.name}:${target}`, ...Record.keys(configuration.configurations ?? {}).map((name) => `${node.name}:${target}:${name}`)] })),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class Composed extends Context.Service<Composed>()('@rasm/test-nx/workspace/graph/Composed', {
    make: Effect.map(
        Effect.tryPromise(() => createProjectGraphAsync({ exitOnError: false })),
        (projectGraph) => {
            const nxJson: NxJsonConfiguration = readNxJson(workspaceRoot);
            return { projectGraph, nxJson, targets: stated(projectGraph), seed: { projectGraph, nxJson } };
        },
    ),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Composed, SLOW, type Stated };
