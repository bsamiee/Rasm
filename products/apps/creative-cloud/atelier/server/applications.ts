import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import type { Installation } from '@rasm/applications';
import { type Entry, Workspace } from '@rasm/applications/workspace';
import { Application, Registry } from '@rasm/creative-cloud-desktop';
import { Link } from '@rasm/mcp';
import { setting } from '@rasm/mcp/cli';
import { UxpManifest } from '@rasm/uxp';
import { Array, Context, Effect, FileSystem, Function, Layer, Option, Order, Path, Record, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTIONS = {
    applications:
        'Lists installed Adobe applications by product, newest form first, and their running instances, or one application with its installation and open command; open launches an application in the background or targets its running instance for later calls; close quits it, or every application this handle opened, each save dialog arriving as an event',
    view: 'Shows windows as images, a line naming each: the front window of the target, every window of every running Adobe application, the windows showing a document, or one reference cropped from its window, a menu or the item opening it showing one image per open level and the outline of those levels; object renders a document, page, spread, artboard, layer, composition, or sequence from its object specifier through the linked plugin of the target, no window needed',
    diagnostics: "Reads diagnostics newest first: without application, Atelier's own log lines as JSON; with one, its crash, hang, and diagnostic reports, its UXP log lines, and its unified log",
};

// --- [MODELS] --------------------------------------------------------------------------

const RegistryPath = setting('registry', Schema.String, '/Library/Application Support/Adobe/caps/hdpim.db', 'Creative Cloud registry database listing the Adobe applications it installed');

// --- [SERVICES] ------------------------------------------------------------------------

class RegistryApplications extends Context.Service<RegistryApplications, SubscriptionRef.SubscriptionRef<readonly Application[]>>()('@rasm/atelier/applications/RegistryApplications') {
    static readonly layer = Layer.merge(Layer.effect(RegistryApplications, SubscriptionRef.make<readonly Application[]>([])), Layer.effect(Registry, RegistryPath));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalog = Effect.fnUntraced(function* (offered: readonly Installation[]) {
    const [rows, registered] = yield* Effect.all([RegistryApplications, Application.installed]);
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
const readLogs = Effect.fnUntraced(function* ({ name }: Pick<Entry, 'name'>) {
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
const route = Effect.map(Workspace, ({ target }) =>
    Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
        const { held } = yield* target;
        const refused = McpSchema.InvalidParams.make({ message: `Linked in ${peers.map((peer) => peer.application.toString()).join(', ')}, applications open makes one the target` });
        return yield* Effect.fromOption(
            Option.flatMap(held, ({ row }) => Array.findFirst(peers, Link.runsIn(row))),
            () => refused,
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalog, DESCRIPTIONS, RegistryApplications, readLogs, RegistryPath, route };
