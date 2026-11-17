import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import { Diagnostics, State } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { ExtensionManifest } from '@rasm/cep';
import { Server } from '@rasm/mcp';
import { Output } from '@rasm/mcp/node';
import { UxpManifest } from '@rasm/uxp';
import { Array, Effect, FileSystem, Option, Order, Path, Schema, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Inventory } from '#/applications';

// --- [MODELS] --------------------------------------------------------------------------

const Diagnosing = Tool.make('diagnostics', {
    description: "Reads diagnostics newest first: without an application, Atelier's own log lines from both of its processes as JSON; with one, its crash, hang, and diagnostic reports from both DiagnosticReports folders, its UXP and CEP log lines, and its unified log",
    parameters: Schema.Struct({
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name or product code from applications' })),
        since: Diagnostics.fields.since,
        limit: Schema.optionalKey(Diagnostics.fields.limit.annotate({ description: 'Most reports, log lines, and unified log events, 50 when absent' })),
    }),
    success: Diagnostics.Result,
}).annotate(Tool.Readonly, true);

// --- [COMPOSITION] ---------------------------------------------------------------------

const diagnostics = Effect.gen(function* () {
    const [server, { located, running }, { logs }, files, path] = yield* Effect.all([ApplicationServer, Inventory, Output, FileSystem.FileSystem, Path.Path]);
    const home = homedir();
    const folder = ({ folder: relative, glob }: { readonly folder: string; readonly glob: string }) => ({ url: pathToFileURL(`${path.join(home, relative)}/`), glob });
    const longest = Order.flip(Order.mapInput(Order.Number, String.length));
    const own = { folders: [{ url: pathToFileURL(`${logs}/`), glob: '*.log' }] };
    const application = Effect.fnUntraced(function* (given: string) {
        const [row, instances, adobe] = yield* Effect.all([located(given), SubscriptionRef.get(running), files.readDirectory(path.join(home, 'Library/Logs/Adobe')).pipe(Effect.orElseSucceed(Array.empty<string>))]);
        const uxp = Option.map(
            Array.findFirst(Array.sort(adobe, longest), (name) => row.name.startsWith(name)),
            (name) => folder(UxpManifest.logs(name)),
        );
        const cep = Option.map(Schema.decodeUnknownOption(ExtensionManifest.fields.host.fields.app)(row.product), (code) => folder(ExtensionManifest.logs(code)));
        const instance = Option.map(
            Array.findFirst(instances, (each) => each.url.href === row.url.href && !State.isAnyOf(['exited'])(each.state)),
            Struct.get('instance'),
        );
        return { application: { identifier: row.identifier, ...Option.getOrUndefined(Option.map(instance, (named) => ({ instance: named }))) }, folders: [...Option.toArray(uxp), ...Option.toArray(cep)] };
    });
    yield* Server.registerTool(
        Diagnosing,
        Effect.fnUntraced(function* ({ application: given, since, limit = 50 }) {
            const read = yield* UndefinedOr.match(given, { onUndefined: () => Effect.succeed(own), onDefined: application });
            return yield* server.diagnostics({ ...read, limit, ...(since === undefined ? {} : { since }) });
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { diagnostics };
