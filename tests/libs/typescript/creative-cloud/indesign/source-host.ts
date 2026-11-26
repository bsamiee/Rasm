import { Description } from '@rasm/applications/source';
import { Dom } from '@rasm/indesign/uxp/dom';
import { preflight } from '@rasm/indesign/uxp/operations';
import { toolbar, workspace } from '@rasm/indesign/uxp/source';
import { Preflight } from '@rasm/indesign/views';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import fs from 'adobe:fs';
import uxp from 'adobe:uxp';
import { Array, Cause, Effect, Exit, Match, pipe, Schema, Scope, Stream } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Path = Schema.toType(Description.Locator);
const Request = Schema.fromJsonString(
    Schema.Union([
        Schema.Struct({ describe: Schema.Struct({ within: Schema.optionalKey(Path) }) }),
        Schema.Struct({ operate: Schema.Struct({ locator: Path, to: Schema.optionalKey(Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])) }) }),
        Schema.Struct({ listen: Schema.Struct({ plugin: Schema.String, tools: Schema.Tuple([Schema.String, Schema.String]) }) }),
        Schema.Struct({ preflight: Preflight }),
    ]),
);
const Heard = Schema.Struct({ both: Schema.Boolean, after: Schema.Boolean, listeners: Schema.Int });
const Answer = Schema.fromJsonString(Schema.Union([Schema.Struct({ described: Description }), Schema.Struct({ operated: Schema.Boolean }), Schema.Struct({ heard: Heard }), Schema.Struct({ preflighted: Preflight.Result }), Schema.Struct({ failed: Schema.String })]));

// --- [OPERATIONS] ----------------------------------------------------------------------

const { describe, operate } = pipe(Description.tools(toolbar), Description.merge(workspace));
const listened = Effect.fnUntraced(function* ({ plugin, tools: [first, second] }: { readonly plugin: string; readonly tools: readonly [string, string] }) {
    const { app } = yield* Dom;
    const ours = () => app.toolBoxTools.eventListeners.everyItem().name.filter((name) => name.startsWith(`${plugin}:`));
    yield* Effect.addFinalizer(() =>
        Effect.sync(() => {
            for (const name of ours()) {
                app.toolBoxTools.eventListeners.itemByName(name).remove();
            }
        }),
    );
    const heard = Dom.events(({ app: held, ToolBox }) => [held.toolBoxTools, [ToolBox.AFTER_ATTRIBUTE_CHANGED]]);
    const closing = yield* Scope.make();
    const [kept, closed] = yield* Effect.all([Stream.toPull(heard), Scope.provide(Stream.toPull(heard), closing)]);
    yield* toolbar.select(first);
    const both = Array.every(yield* Effect.all([kept, closed]), Array.isReadonlyArrayNonEmpty);
    yield* Scope.close(closing, Exit.void);
    yield* toolbar.select(second);
    const after = Array.isReadonlyArrayNonEmpty(yield* kept);
    return { both, after, listeners: ours().length };
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve =
    (declarations: string) =>
    (text: string): string =>
        Effect.runSync(
            Effect.gen(function* () {
                const request = yield* Schema.decodeEffect(Request)(text);
                const answer = yield* Match.value(request).pipe(
                    Match.when({ describe: Match.defined }, ({ describe: within }) => Effect.map(describe(within), (described) => ({ described }))),
                    Match.when({ operate: Match.defined }, ({ operate: payload }) => Effect.as(operate(payload), { operated: true })),
                    Match.when({ preflight: Match.defined }, ({ preflight: payload }) => Effect.map(preflight(payload), (preflighted) => ({ preflighted }))),
                    Match.orElse(({ listen }) => Effect.map(Effect.provideService(listened(listen), Plugin, { id: listen.plugin, host: { app: 'ID' }, requiredPermissions: {} }), (heard) => ({ heard }))),
                );
                return yield* Schema.encodeEffect(Answer)(answer);
            }).pipe(
                Effect.provideServiceEffect(Dom, Dom.make(declarations)),
                Effect.catchCause((cause) => Schema.encodeEffect(Answer)({ failed: Cause.pretty(cause) })),
                Effect.orDie,
                Effect.provide(Uxp.layer({ uxp, fs })),
            ),
        );

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
