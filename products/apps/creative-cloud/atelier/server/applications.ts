import { homedir } from 'node:os';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Target } from '@rasm/application-tools/handle/target';
import { Application, Registry } from '@rasm/creative-cloud-desktop/application';
import { Preferences, Tools } from '@rasm/creative-cloud-desktop/tools';
import { setting } from '@rasm/mcp/cli';
import { Link } from '@rasm/mcp/link';
import { Handle } from '@rasm/mcp/server';
import { Line, Report } from '@rasm/report';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Context, Effect, FileSystem, Function, flow, Layer, Option, Order, Predicate, Record, Result, Schema, String, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';
import { Flag } from 'effect/cli';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORDS = new Intl.Segmenter('en', { granularity: 'word' });

// --- [SERVICES] ------------------------------------------------------------------------

class Applications extends Context.Service<Applications>()('@rasm/atelier/applications', {
    make: Effect.gen(function* () {
        const [installed, tools, files] = yield* Effect.all([SubscriptionRef.make<readonly Application[]>([]), Tools, FileSystem.FileSystem]);
        const folder = join(homedir(), 'Library', 'Logs', 'Adobe');
        const byBundle = Function.memoize((rows: readonly Application[]) => Record.fromIterableBy(rows, ({ url }) => url.href));
        const find = ({ url, name }: Pick<Application, 'url' | 'name'>) =>
            Effect.flatMap(SubscriptionRef.get(installed), (rows) => Effect.fromOption(Record.get(byBundle(rows), url.href), () => Unmatched.of({ subject: 'installed Adobe application', query: Option.some(name), candidates: rows.map(Struct.get('name')) })));
        return {
            installed,
            find,
            toolbar: flow(find, Effect.flatMap(tools)),
            logs: Effect.fnUntraced(function* ({ name }: Pick<Application, 'name'>) {
                const held = yield* files.readDirectory(folder).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())));
                const named = words(name);
                const matching = held.filter((each) => words(each).isSubsetOf(named));
                return Array.match(matching, { onEmpty: () => [], onNonEmpty: (folders) => [{ url: pathToFileURL(`${join(folder, Array.max(folders, Order.mapInput(Order.Number, String.length)))}/`), glob: 'UXPLogs_*.log' }] });
            }),
            route: Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
                const [handle, known] = yield* Effect.all([Handle.current, SubscriptionRef.get(installed)]);
                const aim = yield* Effect.transposeOption(Option.map(handle, (state) => state.get(Target)));
                const held = Option.flatMap(aim, (state) => state.held);
                const target = Option.flatMap(held, ({ row }) => Array.findFirst(peers, Link.runsIn(row)));
                const names = Array.dedupe(peers.map(({ application }) => (Predicate.isString(application) ? application : Option.match(Record.get(byBundle(known), application.href), { onNone: () => application.href, onSome: ({ name }) => name }))));
                const rows = names.map((name) => new Line({ head: [name], call: Target.open(name) }));
                return yield* Effect.fromOption(target, () => McpSchema.InvalidParams.make({ message: Report.text(new Report({ lines: [new Line({ head: ['target runs no plugin serving this call'] }), new Line({ head: ['serving:'], rows })] })) }));
            }),
        };
    }),
}) {
    static readonly Registry = setting('registry', Schema.String, Flag.withDefault('/Library/Application Support/Adobe/caps/hdpim.db'), 'Creative Cloud registry database listing the Adobe applications it installed');
    static readonly catalog = flow(
        Application.fromInstallations,
        Effect.tap((rows) => Applications.use(({ installed }) => SubscriptionRef.set(installed, rows))),
        Effect.map(Array.map(({ url, identifier, name, product, channel, version, build }) => ({ url, identifier, name, product, aliases: [], facts: { product, channel, version, ...Record.getSomes({ build }) } }))),
    );
    static readonly layer = Layer.effect(Applications, Applications.make).pipe(Layer.provide(Tools.layer), Layer.provideMerge([Layer.effect(Registry, Applications.Registry), Layer.succeed(Preferences, join(homedir(), 'Library', 'Preferences'))]));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const words = (text: string) => new Set(Array.filterMap(WORDS.segment(text), ({ isWordLike, segment }) => (isWordLike === true ? Result.succeed(segment) : Result.failVoid)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Applications };
