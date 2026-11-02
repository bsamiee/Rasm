import { Contract } from '@rasm/indesign/contract';
import { Render } from '@rasm/indesign/requests/render';
import { Script } from '@rasm/indesign/script';
import { Image } from '@rasm/mcp/content';
import { registerRpcGroup } from '@rasm/mcp/rpc';
import { Instructions } from '@rasm/mcp/server';
import { Array, Context, Effect, Match, Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { routes } from '#/routes';

// --- [COMPOSITION] ---------------------------------------------------------------------

const indesign = ({ indesign: route }: Effect.Success<ReturnType<typeof routes>>) =>
    Script.use((script) =>
        registerRpcGroup(
            Contract.annotate(Instructions, [
                'InDesign tools run as UXP scripts through do script',
                'Specifiers come from results',
                'menu: Menu actions of one area or every area',
                'preflight: Preflight rows per document, finished live results without profile. Other calls wait on a process restarting every live check',
                'kind: Dictionary class and enumeration names, or one declaration',
            ]),
            {
                tools: {
                    inspect: { annotations: Context.make(Tool.Readonly, true).pipe(Context.add(Instructions, ['inspect: Reads object fields with declarations and typed view, collection members, or application state without target. indesign://kinds lists classes and enumerations'])) },
                    change: { annotations: Context.make(Instructions, ['change: Writes object fields in one undo step, failures name every refused field by path']) },
                    create: { annotations: Context.make(Instructions, ['create: Adds collection member in one undo step, kind lists add signatures']) },
                    remove: { annotations: Context.make(Instructions, ['remove: Removes object in one undo step']) },
                    find_change: { annotations: Context.make(Instructions, ['find_change: Finds or changes matches in one undo step and restores search preferences. Values take change forms, kind lists preference fields']) },
                    invoke: { annotations: Context.make(Instructions, ['invoke: Invokes menu action in one undo step, a dialog it opens waits for an answer. indesign://menu lists actions by id, name, and area']) },
                    render: {
                        annotations: Context.make(Tool.Readonly, true).pipe(Context.add(Instructions, ['render: Renders PNG at 1 to 2400 ppi. Page items render alone, their page or spread shows context. Use screenshot for panels, dialogs, pasteboard, and parent spreads'])),
                        success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                    },
                },
                resources: {
                    menu: { uri: 'indesign://menu', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } },
                    preflight: { uri: 'indesign://preflight', complete: { document: Array.map((process) => process.targetObject.specifier) } },
                    kind: { uri: 'indesign://kinds', complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } },
                },
                route,
                invoke: (rpc) =>
                    Effect.fnUntraced(function* (payload, { pid }) {
                        const call = yield* script.client(pid);
                        return yield* call(rpc._tag, payload);
                    }, Effect.scoped),
            },
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { indesign };
