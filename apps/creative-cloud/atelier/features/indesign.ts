import { Contract, Render } from '@rasm/indesign';
import { Image } from '@rasm/mcp/content';
import { registerRpcGroup } from '@rasm/mcp/rpc';
import { Instructions } from '@rasm/mcp/server';
import { Array, Context, Effect, Match, Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { InDesignScript } from '#/indesign-script';
import type { routes } from '#/routes';

// --- [COMPOSITION] ---------------------------------------------------------------------

const indesign = Effect.fnUntraced(function* ({ indesign: route }: Effect.Success<ReturnType<typeof routes>>) {
    const indesignScript = yield* InDesignScript;
    yield* registerRpcGroup(Contract.annotate(Instructions, ['InDesign tools run as UXP scripts through do script']), {
        tools: {
            inspect: { annotations: Context.make(Tool.Readonly, true).pipe(Context.add(Instructions, ['inspect: indesign://kinds lists classes and enumerations'])) },
            change: {},
            create: {},
            remove: {},
            find_change: {},
            invoke: { annotations: Context.make(Instructions, ['invoke: indesign://menu lists actions by id, name, and area']) },
            render: { annotations: Context.make(Tool.Readonly, true).pipe(Context.add(Instructions, ['render: Use screenshot for panels, dialogs, pasteboard, and parent spreads'])), success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })) },
        },
        resources: {
            menu: { uri: 'indesign://menu', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } },
            preflight: { uri: 'indesign://preflight', complete: { document: Array.map((process) => process.targetObject.specifier) } },
            kind: { uri: 'indesign://kinds', complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } },
        },
        route,
        invoke: (rpc) =>
            Effect.fnUntraced(function* (payload, { pid }) {
                const call = yield* indesignScript.client(pid);
                return yield* call(rpc._tag, payload);
            }, Effect.scoped),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { indesign };
