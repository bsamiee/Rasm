import { homedir } from 'node:os';
import { pathToFileURL } from 'node:url';
import { Target } from '@rasm/application-tools/handle/target';
import type { Entry } from '@rasm/applications/catalog';
import type { Installation } from '@rasm/applications/server/installation';
import { Application, Registry } from '@rasm/creative-cloud-desktop/application';
import { Preferences, Tools } from '@rasm/creative-cloud-desktop/tools';
import { setting } from '@rasm/mcp/cli';
import { Link } from '@rasm/mcp/link';
import { Handle } from '@rasm/mcp/server';
import { Array, Cause, Context, Effect, FileSystem, Layer, Option, Order, Path, Predicate, Record, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';
import { Flag } from 'effect/cli';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DESCRIPTIONS = {
    applications: 'each Adobe product lists its Beta first, the form a product code or name opens',
    view: 'object renders a document, page, spread, artboard, layer, composition, or sequence from its object specifier through the linked plugin of the target, no window needed',
    diagnostics: 'an Adobe application adds its UXP log lines',
};
const WORDS = new Intl.Segmenter('en', { granularity: 'word' });
const LISTING = new Intl.ListFormat('en');

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
const route = Effect.map(RegistryApplications, (rows) =>
    Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
        const [handle, listed] = yield* Effect.all([Handle.current, SubscriptionRef.get(rows)]);
        const aim = yield* Effect.transposeOption(Option.map(handle, (held) => held.get(Target)));
        const named = new Map(listed.map(({ url, name }) => [url.href, name]));
        const names = Array.map(peers, ({ application }) => (Predicate.isString(application) ? application : (named.get(application.href) ?? application.href)));
        const [first] = names;
        return yield* Effect.fromOption(
            Option.flatMap(Option.flatMap(aim, Struct.get('held')), ({ row }) => Array.findFirst(peers, Link.runsIn(row))),
            () => McpSchema.InvalidParams.make({ message: `The plugins serving this call run in ${LISTING.format(names)}, none of them the target, so applications {action:"open", application:${JSON.stringify(first)}} makes ${first} the target` }),
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalog, DESCRIPTIONS, RegistryApplications, readLogs, RegistryPath, route, toolsOf };
