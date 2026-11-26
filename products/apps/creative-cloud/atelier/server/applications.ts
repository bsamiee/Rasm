import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import type { Installation } from '@rasm/applications/contract';
import { type Entry, Workspace } from '@rasm/applications/workspace';
import { Application, Registry } from '@rasm/creative-cloud-desktop/application';
import { Preferences, Tools } from '@rasm/creative-cloud-desktop/tools';
import { setting } from '@rasm/mcp/cli';
import { Link } from '@rasm/mcp/link';
import { Array, Cause, Context, Effect, FileSystem, Layer, Option, Order, Path, Record, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';
import { Flag } from 'effect/cli';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTIONS = {
    applications:
        'Lists installed Adobe applications by product, the Beta first as the form a product code or name opens and a year release opened by its listed name only for a file that needs it, and their running instances with the status items each shows in the menu bar, or one application with its installation, its App Intents actions with whether each opens it, its open command, and per interface language what its interface map holds: containers by kind read in the newest build, containers read only in earlier builds, and openers never seen open; open launches an application in the background or targets its running instance for later calls; close quits it, or every application this handle opened, each save dialog arriving as an event',
    view: 'Shows windows as images, a line naming each: the front window of the target, every window of every running Adobe application, the windows showing a document, a document, page, spread, artboard, layer, composition, or sequence the linked plugin of the target renders from its object specifier with no window needed, or one reference cropped from its window with any menu or panel of the application above it; a menu or the item opening it shows one image per open level and the outline of those levels, a menu the application cannot open while behind drawn from what was read, and a remembered container or control its outline from the map, nothing opened; with seconds it records what it shows for that long and returns its key frames in place of one image: the state when recording began, then each state it settled in for a beat, each line naming when the state appeared after the first and the pixel rectangle that changed, which view of that rectangle shows again',
    diagnostics: "Reads diagnostics newest first, one line each: without application, Atelier's own log lines under the file holding them; with one, its crash, hang, and diagnostic reports, its UXP log lines, and its unified log",
};
const WORDS = new Intl.Segmenter('en', { granularity: 'word' });

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
                    Path.Path.useSync((path) => path.join(homedir(), 'Library', 'Preferences')),
                ),
            ),
        ),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalog = Effect.fnUntraced(function* (offered: readonly Installation[]) {
    const [rows, listed] = yield* Effect.all([RegistryApplications, Application.fromInstallations(offered)]);
    yield* SubscriptionRef.set(rows, listed);
    return listed.map(({ url, identifier, name, product, channel, version, build }) => ({ url, identifier, name, product, aliases: [], facts: { product, channel, version, ...Record.getSomes({ build }) } }));
});
const readLogs = Effect.fnUntraced(function* ({ name }: Pick<Entry, 'name'>) {
    const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const logs = path.join(homedir(), 'Library', 'Logs', 'Adobe');
    const vendored = yield* files.readDirectory(logs).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())));
    const named = new Set(
        Iterator.from(WORDS.segment(name))
            .filter(({ isWordLike }) => isWordLike === true)
            .map(({ segment }) => segment),
    );
    const held = vendored.filter((folder) => Iterator.from(WORDS.segment(folder)).every(({ segment, isWordLike }) => isWordLike !== true || named.has(segment)));
    const folder = Option.map(Option.liftPredicate(held, Array.isArrayNonEmpty), Array.max(Order.mapInput(Order.Number, String.length)));
    return Option.toArray(Option.map(folder, (each) => ({ url: pathToFileURL(`${path.join(logs, each)}/`), glob: 'UXPLogs_*.log' })));
});
const toolsOf = Effect.map(Effect.all([RegistryApplications, Tools]), ([rows, read]) =>
    Effect.fnUntraced(function* (row: Pick<Entry, 'url' | 'name'>) {
        const held = Array.findFirst(yield* SubscriptionRef.get(rows), (each) => each.url.href === row.url.href);
        return yield* read(yield* Effect.fromOption(held, () => new Cause.NoSuchElementError(`${row.name} is no application the registry lists`)));
    }),
);
const route = Effect.map(Workspace, ({ target }) =>
    Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
        const { held } = yield* target;
        return yield* Effect.fromOption(
            Option.flatMap(held, ({ row }) => Array.findFirst(peers, Link.runsIn(row))),
            () => McpSchema.InvalidParams.make({ message: `Linked in ${peers.map(Struct.get('application')).join(', ')}, applications open makes one the target` }),
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalog, DESCRIPTIONS, RegistryApplications, readLogs, RegistryPath, route, toolsOf };
