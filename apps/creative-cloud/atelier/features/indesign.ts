import { Contract } from '@rasm/indesign/contract.ts';
import { Render } from '@rasm/indesign/requests/render.ts';
import { Image } from '@rasm/mcp/content.ts';
import { Registration, registerRpcGroup } from '@rasm/mcp/rpc.ts';
import { Instructions } from '@rasm/mcp/server.ts';
import { READ_ONLY } from '@rasm/mcp/tools.ts';
import { Array, Context, Effect, Match, Schema, Struct } from 'effect';
import { InDesignScript } from '../indesign-script.ts';
import type { routes } from '../routes.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const indesign = Effect.fnUntraced(function* ({ indesign: route }: Effect.Success<ReturnType<typeof routes>>) {
    const indesignScript = yield* InDesignScript;
    yield* registerRpcGroup(
        Contract,
        {
            inspect: Registration.Tool({ annotations: READ_ONLY.pipe(Context.add(Instructions, ['inspect: The indesign://kinds resource lists class and enumeration declarations.'])) }),
            change: Registration.Tool({ annotations: Context.empty() }),
            create: Registration.Tool({ annotations: Context.empty() }),
            remove: Registration.Tool({ annotations: Context.empty() }),
            find_change: Registration.Tool({ annotations: Context.empty() }),
            invoke: Registration.Tool({ annotations: Context.make(Instructions, ['invoke: The indesign://menu resource lists menu actions by id, name, and area.']) }),
            render: Registration.Tool({ annotations: READ_ONLY.pipe(Context.add(Instructions, ['render: Application panels, dialogs, the pasteboard, and parent spreads show through screenshot.'])), success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })) }),
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
            annotations: Context.make(Instructions, ['InDesign tools run as UXP scripts through its do script command.']),
        },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { indesign };
