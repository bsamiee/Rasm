import { Executions, publish, routed } from '@rasm/ai/extension';
import { main, observability } from '@rasm/ai/server';
import { Galley } from '@rasm/galley/extension';
import { Array, Effect, Layer, Struct } from 'effect';
import { McpProtocol } from 'effect/ai';
import { Atelier, applications, font } from './applications.ts';
import { execute } from './execute.ts';
import manifest from './package.json' with { type: 'json' };
import { Scripting } from './scripting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const extensions = [Galley];

main(
    {
        ...Struct.pick(manifest, ['name', 'version', 'description']),
        instructions: Array.join(Array.map([Atelier, ...extensions], Struct.get('instructions')), '\n'),
        protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
        allowSessionTermination: true,
    },
    Layer.effectDiscard(Effect.flatMap(Effect.all([applications(extensions), routed([...extensions, Atelier])]), ([routing, folded]) => Effect.all([publish(folded.verbs, routing), execute(folded, routing), font(folded, routing)], { discard: true }))).pipe(
        Layer.provide([Atelier.layer, ...Array.map(extensions, Struct.get('layer'))]),
        Layer.provide([Scripting.layer, Executions.layer('adobe://executions')]),
    ),
    observability(manifest),
);
