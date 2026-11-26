import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import type { Installation } from '@rasm/applications/contract';
import { type Entry, Workspace } from '@rasm/applications/workspace';
import { Application, Registry } from '@rasm/creative-cloud-desktop/application';
import { Preferences, Tools } from '@rasm/creative-cloud-desktop/tools';
import { setting } from '@rasm/mcp/cli';
import { Link } from '@rasm/mcp/link';
import { Array, Cause, Context, Effect, FileSystem, Function, Layer, Option, Order, Path, Record, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';
import { Flag } from 'effect/cli';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTIONS = {
    applications:
        'Lists installed Adobe applications by product, newest form first, and their running instances, or one application with its installation, its App Intents actions with whether each opens it, and open command; open launches an application in the background or targets its running instance for later calls; close quits it, or every application this handle opened, each save dialog arriving as an event',
    view: 'Shows windows as images, a line naming each: the front window of the target, every window of every running Adobe application, the windows showing a document, or one reference cropped from its window, a menu or the item opening it showing one image per open level and the outline of those levels; object renders a document, page, spread, artboard, layer, composition, or sequence from its object specifier through the linked plugin of the target, no window needed',
    diagnostics: "Reads diagnostics newest first, one line each: without application, Atelier's own log lines under the file holding them; with one, its crash, hang, and diagnostic reports, its UXP log lines, and its unified log",
};

// --- [MODELS] --------------------------------------------------------------------------

const RegistryPath = setting('registry', Schema.String, Flag.withDefault('/Library/Application Support/Adobe/caps/hdpim.db'), 'Creative Cloud registry database listing the Adobe applications it installed');

// --- [SERVICES] ------------------------------------------------------------------------

class RegistryApplications extends Context.Service<RegistryApplications, SubscriptionRef.SubscriptionRef<readonly Application[]>>()('@rasm/atelier/applications/RegistryApplications') {
    static readonly layer = Layer.mergeAll(
        Layer.effect(RegistryApplications, SubscriptionRef.make<readonly Application[]>([])),
        Layer.effect(Registry, RegistryPath),
        Tools.layer.pipe(
            Layer.provideMerge(
                Layer.effect(
                    Preferences,
                    Effect.map(Path.Path, (path) => path.join(homedir(), 'Library', 'Preferences')),
                ),
            ),
        ),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalog = Effect.fnUntraced(function* (offered: readonly Installation[]) {
    const [rows, listed] = yield* Effect.all([RegistryApplications, Application.fromInstallations(offered)]);
    const newest = Record.map(Array.groupBy(listed, Struct.get('product')), (forms) => Array.headNonEmpty(forms).url.href);
    yield* SubscriptionRef.set(rows, listed);
    return listed.map(({ url, identifier, name, product, channel, version, build }) => ({
        url,
        identifier,
        name,
        aliases: newest[product] === url.href ? [product] : [],
        facts: { product, channel, version, ...Record.getSomes({ build }) },
    }));
});
const readLogs = Effect.fnUntraced(function* ({ name }: Pick<Entry, 'name'>) {
    const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const logs = path.join(homedir(), 'Library', 'Logs', 'Adobe');
    const vendored = yield* files.readDirectory(logs).pipe(Effect.catchReason('PlatformError', 'NotFound', Function.constant(Effect.succeed(Array.empty<string>()))));
    const folder = Array.findFirst(Array.sort(vendored, Order.flip(Order.mapInput(Order.Number, String.length))), (each) => name.startsWith(each));
    return Option.toArray(Option.map(folder, (each) => ({ url: pathToFileURL(`${path.join(logs, each)}/`), glob: 'UXPLogs_*.log' })));
});
const toolsOf = Effect.map(
    Effect.all([RegistryApplications, Tools]),
    ([rows, read]) =>
        (row: Pick<Entry, 'url' | 'name'>) =>
            SubscriptionRef.get(rows).pipe(
                Effect.flatMap((held) =>
                    Effect.fromOption(
                        Array.findFirst(held, (each) => each.url.href === row.url.href),
                        () => new Cause.NoSuchElementError(`${row.name} is no application the registry lists`),
                    ),
                ),
                Effect.flatMap(read),
            ),
);
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

export { catalog, DESCRIPTIONS, RegistryApplications, readLogs, RegistryPath, route, toolsOf };
