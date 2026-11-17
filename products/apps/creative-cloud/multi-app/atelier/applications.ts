import { Launched, Launching } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Application } from '@rasm/creative-cloud-desktop';
import { Server } from '@rasm/mcp';
import { Array, Context, Effect, Schema, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';

// --- [MODELS] --------------------------------------------------------------------------

const Listing = Schema.Union([Schema.Struct({ installed: Schema.Array(Schema.Struct(Struct.omit(Application.fields, ['identifier', 'documents', 'url']))) }), Launched]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Effect.fnUntraced(function* (catalog: SubscriptionRef.SubscriptionRef<readonly Application[]>) {
    const [server, rows] = yield* Effect.all([ApplicationServer, SubscriptionRef.get(catalog)]);
    const named = Array.dedupe([...Array.map(rows, Struct.get('name')), ...Array.map(rows, Struct.get('product'))]);
    const opened = Effect.fnUntraced(function* (given: string) {
        const current = yield* SubscriptionRef.get(catalog);
        const { url } = yield* Effect.fromOption(Array.findFirst(current, ({ name, product }) => name === given || product === given));
        return yield* Launching.launched(server.launch({ url }));
    });
    yield* Server.registerTool(
        Tool.make('applications', {
            description: 'Lists installed Adobe applications grouped by product, newest form first, or opens the named one without activating it, below the frontmost window, reusing a running instance',
            parameters: Schema.Struct({
                application: Schema.optionalKey(
                    Array.match(named, { onEmpty: () => Schema.Never, onNonEmpty: (values) => Schema.Literals(values) }).annotate({
                        description: 'Listed name opens that application, product code opens its first listed form, each product listing prerelease, then Beta, then year releases newest first',
                    }),
                ),
            }),
            success: Listing,
        }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false), Context.add(Tool.OpenWorld, false))),
        ({ application }) => UndefinedOr.match(application, { onUndefined: () => Effect.map(SubscriptionRef.get(catalog), (installed) => ({ installed })), onDefined: opened }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications };
