import { Contract, Render } from '@rasm/indesign';
import { Image, Instructions, READ_ONLY, Registration, registerRpcGroup } from '@rasm/mcp';
import { Array, Context, Effect, Match, Schema, Struct } from 'effect';
import { InDesignScript } from '#/indesign-script';
import type { routes } from '#/routes';

// --- [COMPOSITION] ---------------------------------------------------------------------

const indesign = Effect.fnUntraced(function* ({ indesign: route }: Effect.Success<ReturnType<typeof routes>>) {
    const indesignScript = yield* InDesignScript;
    yield* registerRpcGroup(
        Contract,
        {
            inspect: Registration.Tool({ annotations: READ_ONLY.pipe(Context.add(Instructions, ['inspect: indesign://kinds lists classes and enumerations'])) }),
            change: Registration.Tool({ annotations: Context.empty() }),
            create: Registration.Tool({ annotations: Context.empty() }),
            remove: Registration.Tool({ annotations: Context.empty() }),
            find_change: Registration.Tool({ annotations: Context.empty() }),
            invoke: Registration.Tool({ annotations: Context.make(Instructions, ['invoke: indesign://menu lists actions by id, name, and area']) }),
            render: Registration.Tool({ annotations: READ_ONLY.pipe(Context.add(Instructions, ['render: Use screenshot for panels, dialogs, pasteboard, and parent spreads'])), success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })) }),
            menu: Registration.Resource({ uri: 'indesign://menu', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } }),
            preflight: Registration.Resource({
                uri: 'indesign://preflight',
                complete: { document: Array.map((process) => process.targetObject.specifier) },
            }),
            kind: Registration.Resource({ uri: 'indesign://kinds', complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } }),
        },
        {
            route,
            invoke: (rpc) =>
                Effect.fnUntraced(function* (payload, { pid }) {
                    const call = yield* indesignScript.client(pid);
                    return yield* call(rpc._tag, payload);
                }, Effect.scoped),
            annotations: Context.make(Instructions, ['InDesign tools run as UXP scripts through do script']),
        },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { indesign };
