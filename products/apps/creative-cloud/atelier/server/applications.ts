import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import type { Installation } from '@rasm/applications';
import { type Entry, Inventory } from '@rasm/applications/workspace';
import { Application, Registry } from '@rasm/creative-cloud-desktop';
import { Link } from '@rasm/mcp';
import { setting } from '@rasm/mcp/cli';
import { UxpManifest } from '@rasm/uxp';
import { Array, Context, Effect, FileSystem, Function, Layer, Option, Order, Path, Record, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTIONS = {
    applications:
        'Lists installed Adobe applications grouped by product, newest form first, with the running instance of each, and one named application with its installation and the open command of its dictionary; open launches one in the background, or makes its running instance the target of the handle; close quits one, or each the handle opened or targeted, each save dialog arriving as an event act answers',
    view: 'Shows windows as images, each with its file at full resolution: the front window of the target application, every window of every running Adobe application, one element cropped from its window, or the windows showing one document; object shows one document, page, spread, artboard, layer, composition, or sequence the linked extension of the target application renders from its object specifier, with no window needed',
    diagnostics: "Reads diagnostics newest first: without an application, Atelier's own log lines from both of its processes as JSON; with one, its crash, hang, and diagnostic reports from both DiagnosticReports folders, its UXP log lines, and its unified log",
};

// --- [MODELS] --------------------------------------------------------------------------

const Registered = setting('registry', 'registry', Schema.String, '/Library/Application Support/Adobe/caps/hdpim.db', 'Creative Cloud registry database listing the Adobe applications it installed');

// --- [SERVICES] ------------------------------------------------------------------------

class Installed extends Context.Service<Installed, SubscriptionRef.SubscriptionRef<readonly Application[]>>()('@rasm/atelier/applications/Installed') {
    static readonly layer = Layer.merge(Layer.effect(Installed, SubscriptionRef.make<readonly Application[]>([])), Layer.effect(Registry, Registered));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalogued = Effect.fnUntraced(function* (offered: readonly Installation[]) {
    const [rows, registered] = yield* Effect.all([Installed, Application.installed]);
    const known = new Set(registered.map((row) => row.url.href));
    const outside = Array.getSomes(
        offered
            .filter(({ url }) => !known.has(url.href))
            .map(({ url, identifier, name, version, documentTypes }) =>
                Option.map(version, (stated) => Application.make({ name, product: identifier, channel: 'release', version: stated, build: Option.none(), identifier, documents: Array.dedupe(Array.flatMap(Option.getOrElse(documentTypes, Array.empty), Struct.get('extensions')).map(String.toLowerCase)), url })),
            ),
    );
    const [listed, displayed] = [[...registered, ...outside], Record.fromIterableBy(offered, (each) => each.url.href)];
    const newest = Record.map(Array.groupBy(listed, Struct.get('product')), (forms) => Array.headNonEmpty(forms).url.href);
    yield* SubscriptionRef.set(rows, listed);
    return listed.map(({ url, identifier, name, product, channel, version, build }) => ({
        url,
        identifier,
        name,
        aliases: [...(newest[product] === url.href ? [product] : []), ...Option.toArray(Option.map(Record.get(displayed, url.href), Struct.get('name')))],
        facts: { product, channel, version, ...Record.getSomes({ build }) },
    }));
});
const logged = Effect.fnUntraced(function* ({ name }: Pick<Entry, 'name'>) {
    const [files, path, home] = [yield* FileSystem.FileSystem, yield* Path.Path, homedir()];
    const vendored = yield* files.readDirectory(path.join(home, 'Library/Logs/Adobe')).pipe(Effect.catchReason('PlatformError', 'NotFound', Function.constant(Effect.succeed(Array.empty<string>()))));
    const folder = Array.findFirst(Array.sort(vendored, Order.flip(Order.mapInput(Order.Number, String.length))), (each) => name.startsWith(each));
    return Option.toArray(
        Option.map(folder, (each) => {
            const { folder: logs, glob } = UxpManifest.logFolder(each);
            return { url: pathToFileURL(`${path.join(home, logs)}/`), glob };
        }),
    );
});
const routed = Effect.map(Inventory, ({ target }) =>
    Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
        const { held } = yield* target;
        const refused = McpSchema.InvalidParams.make({ message: `Linked in ${peers.map((peer) => peer.application.toString()).join(', ')}, applications open makes one the target` });
        return yield* Effect.fromOption(
            Option.flatMap(held, ({ row }) => Array.findFirst(peers, Link.hosted(row))),
            () => refused,
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalogued, DESCRIPTIONS, Installed, logged, Registered, routed };
