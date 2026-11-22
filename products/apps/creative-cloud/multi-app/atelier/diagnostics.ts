import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import { Diagnostics, Installation } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Server } from '@rasm/mcp';
import { Output } from '@rasm/mcp/node';
import { UxpManifest } from '@rasm/uxp';
import { Array, Effect, FileSystem, Function, Option, Order, Path, Schema, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Inventory } from '#/applications.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 50;

// --- [MODELS] --------------------------------------------------------------------------

const Diagnosing = Tool.make('diagnostics', {
    description: "Reads diagnostics newest first: without an application, Atelier's own log lines from both of its processes as JSON; with one, its crash, hang, and diagnostic reports from both DiagnosticReports folders, its UXP log lines, and its unified log",
    parameters: Schema.Struct({
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name or product code from applications' })),
        since: Diagnostics.fields.since,
        limit: Diagnostics.fields.limit.annotate({ default: LIMIT }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(LIMIT))),
    }),
    success: Diagnostics.Result,
}).annotate(Tool.Readonly, true);

// --- [COMPOSITION] ---------------------------------------------------------------------

const diagnostics = Effect.gen(function* () {
    const [server, { located, running }, { logs }, files, path] = yield* Effect.all([ApplicationServer, Inventory, Output, FileSystem.FileSystem, Path.Path]);
    const home = homedir();
    const longest = Order.flip(Order.mapInput(Order.Number, String.length));
    const own = { folders: [{ url: pathToFileURL(`${logs}/`), glob: '*.log' }] };
    const vendored = files.readDirectory(path.join(home, 'Library/Logs/Adobe')).pipe(Effect.catchReason('PlatformError', 'NotFound', Function.constant(Effect.succeed(Array.empty<string>()))));
    const application = Effect.fnUntraced(function* (given: string) {
        const [row, instances, adobe] = yield* Effect.all([located(given), SubscriptionRef.get(running), vendored]);
        const uxp = Option.map(
            Array.findFirst(Array.sort(adobe, longest), (name) => row.name.startsWith(name)),
            (name) => {
                const { folder, glob } = UxpManifest.logs(name);
                return { url: pathToFileURL(`${path.join(home, folder)}/`), glob };
            },
        );
        const instance = Option.map(Array.findFirst(instances, Installation.runs(row)), Struct.get('instance'));
        return { application: { url: row.url, ...Option.getOrUndefined(Option.map(instance, (named) => ({ instance: named }))) }, folders: Option.toArray(uxp) };
    });
    yield* Server.registerTool(
        Diagnosing,
        Effect.fnUntraced(function* ({ application: given, since, limit }) {
            const read = yield* UndefinedOr.match(given, { onUndefined: () => Effect.succeed(own), onDefined: application });
            return yield* server.diagnostics({ ...read, limit, ...(since === undefined ? {} : { since }) });
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { diagnostics };
