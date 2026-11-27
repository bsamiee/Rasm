import { homedir } from 'node:os';
import dialogs from '@rasm/acrobat/host?inline';
import { HostScript as Acrobat } from '@rasm/acrobat/script';
import { ApplicationServer } from '@rasm/applications/client';
import type { ScriptingDictionary } from '@rasm/applications/installations';
import type { InstanceSnapshot, Written } from '@rasm/applications/instances';
import type { Entry } from '@rasm/applications/workspace';
import { Application } from '@rasm/creative-cloud-desktop/application';
import { Completion } from '@rasm/extendscript/completion';
import host from '@rasm/extendscript/host?inline';
import { lower, originalLine } from '@rasm/extendscript/node';
import { HostScript as Illustrator } from '@rasm/illustrator/script';
import { HostScript as InDesign } from '@rasm/indesign/script';
import { Link } from '@rasm/mcp/link';
import { HostError } from '@rasm/object-model/model';
import { Evaluation, EvaluationRpcs } from '@rasm/object-model/rpcs';
import type { Unmatched } from '@rasm/search/unmatched';
import { HOSTS, PluginsInfo, UxpManifest } from '@rasm/uxp/manifest';
import { Array, Cache, Cause, Duration, Effect, Exit, FileSystem, Match, Option, Order, Path, Predicate, Record, Schema, Struct, SubscriptionRef } from 'effect';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import shim from 'es5-shim?inline';
import sham from 'es5-shim/es5-sham.js?inline';
import { RegistryApplications } from '#/applications';
import { Declarations } from '#/declarations';

// --- [TYPES] ---------------------------------------------------------------------------

interface Plan {
    readonly program: (body: string) => string;
    readonly engine: Parameters<typeof lower>[1];
    readonly offset: number;
    readonly direct: (source: string, label: string) => string;
    readonly declared: Record.ReadonlyRecord<string, { readonly _tag: 'enumerator'; readonly name: string }>;
}
type Route = { readonly _tag: 'Linked' } | { readonly _tag: 'Uxp'; readonly parameter: string; readonly script: typeof InDesign } | { readonly _tag: 'Unlinked' } | { readonly _tag: 'Acrobat' } | { readonly _tag: 'Script'; readonly around: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const UXP: Record.ReadonlyRecord<string, typeof InDesign> = { [HOSTS.ID.product]: InDesign };
const EXTENDSCRIPT: Record.ReadonlyRecord<string, typeof Illustrator> = { [HOSTS.AI.product]: Illustrator };
const DESCRIPTION = `Runs TypeScript function body, return giving its result, in the target application through its newest script engine: a linked plugin, do script uxpscript, Acrobat JavaScript, or ExtendScript; returns its result as JSON, a failure naming the body line; a dialog, an exit, or a hang returns the call at once, its result arriving under finished in a later result. Acrobat bodies reach app, this, event, security, spell, util, and Report, and every object read from them, through stand-ins that answer each dialog the Acrobat JavaScript reference states as dismissed without showing it, or refuse it naming the member and the parameter that avoids it (a dirty closeDoc, execMenuItem SaveAs or Open, a picker whose result has no stated cancel value); code a body hands Acrobat as a string (Function, indirect eval, setTimeOut and action scripts) runs against the real app, whose global binding Acrobat holds non-writable and non-configurable, so its dialogs show. UXP bodies take their host module from require('<host>') (${Declarations.checked.join(', ')}), checked first against its declarations, which refuse a wrong member, type, read-only write, or enumeration value with a link to atelier://object-model/{host}/{name}`;

// --- [MODELS] --------------------------------------------------------------------------

const Reply = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Completion)));
const ResultJson = Schema.toCodecJson(Evaluation.Result);
const Loaded = Schema.Struct({ pluginId: Schema.String, status: Schema.Literal('enabled'), hostMinVersion: Schema.String }).annotate({ description: 'PluginsInfo row of a plugin its host loads at or above hostMinVersion' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (source: string) => `(function (self) {\n${source}\nif (typeof Object.keys !== 'function') {\n${shim}\n${sham}\n}\n${host}\nreturn encodeURIComponent(host.reply(function () { return around(self, function () { return { result: body.call(self) }; }, host); }));\n})(this)`;
const reline = (line: (at: number) => Option.Option<number>) => (error: HostError) => Effect.fail(HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? HostError.thrown(error.subject, Option.flatMap(error.reason.line, line))(error.cause) : error);
const evaluate = (): Plan => ({ program: (body: string) => Evaluation.expression(body, true), engine: { targets: UxpManifest.engine }, offset: 0, direct: (source: string) => source, declared: {} });
const plan = Match.typeTags<Route>()({
    Linked: evaluate,
    Unlinked: evaluate,
    Uxp: ({ parameter, script }): Plan => ({
        program: (body: string) => Evaluation.expression(body, false),
        engine: { targets: UxpManifest.engine },
        offset: script.offset,
        direct: (source: string, label: string) => script.wrap(source, host, label),
        declared: { [parameter]: { _tag: 'enumerator' as const, name: script.language } },
    }),
    Acrobat: (): Plan => ({ program: (body: string) => Acrobat.program(Evaluation.expression(body, false)), engine: { targets: Acrobat.engine }, offset: Acrobat.offset, direct: (source: string) => Acrobat.wrap(source, host, dialogs), declared: {} }),
    Script: ({ around }): Plan => ({ program: (body: string) => `var body = ${Evaluation.expression(body, false)};\nvar around = ${around};`, engine: lower.extendscript, offset: 1, direct: extendscript, declared: {} }),
});
const openLine = (line: number) => Option.liftPredicate(line - 1, (at) => at > 0);
const compile = Effect.fnUntraced(function* (body: string, scriptPlan: Plan) {
    const { code, map } = yield* lower(scriptPlan.program(body), scriptPlan.engine, 'typescript').pipe(Effect.catchTag('HostError', reline(openLine)));
    const [original, label] = [yield* originalLine(map), Evaluation.formatLabel(body)];
    return { code, label, direct: scriptPlan.direct(code, label), declared: scriptPlan.declared, relined: reline((line) => Option.map(original(line - scriptPlan.offset), (at) => at - 1)) };
});
const route = Effect.fnUntraced(function* <E, R>(dictionary: (url: URL) => Effect.Effect<Pick<ScriptingDictionary, 'script' | 'suite'>, E, R>, owned: (pluginId: string) => boolean, { url, product, version }: Pick<Application, 'url' | 'product' | 'version'>) {
    const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const app = Array.findFirst(Struct.keys(HOSTS), (key) => HOSTS[key].pluginsInfo && HOSTS[key].product === product);
    const registries = Array.cartesianWith(Option.toArray(app), ['/', homedir()], (key, root) => path.join(root, 'Library', 'Application Support', 'Adobe', ...PluginsInfo.file(key)));
    const listed = yield* Effect.forEach(
        registries,
        (file) =>
            files.readFileString(file).pipe(
                Effect.flatMap(Schema.decodeEffect(PluginsInfo)),
                Effect.map(Struct.get('plugins')),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
            ),
        { concurrency: 'unbounded' },
    );
    const plugged = Array.some(Array.flatten(listed), (row) => Option.exists(Schema.decodeUnknownOption(Loaded)(row), ({ pluginId, hostMinVersion }) => owned(pluginId) && Order.isGreaterThanOrEqualTo(Application.VersionOrder)(version, hostMinVersion)));
    const unlinked = Option.liftPredicate<Route>({ _tag: 'Unlinked' }, () => plugged);
    const uxpScript = Record.get(UXP, product);
    if (plugged && Option.isNone(uxpScript)) {
        return unlinked;
    }
    const { script, suite } = yield* dictionary(url);
    const parameter = Option.flatMap(Option.all([script, uxpScript]), ([{ parameters }, { language }]) => Array.findFirst(parameters, ({ enumerators }) => Array.contains(Array.map(enumerators, Struct.get('code')), language)));
    const around = Option.getOrElse(Option.map(Record.get(EXTENDSCRIPT, product), Struct.get('around')), () => String(<A>(_: unknown, body: () => A): A => body()));
    const runtime: Route = Array.some(suite, ({ code }) => code === Acrobat.suite) ? { _tag: 'Acrobat' } : { _tag: 'Script', around };
    return Option.map(Option.all({ parameter, script: uxpScript }), ({ parameter: { name }, script: hostScript }): Route => ({ _tag: 'Uxp', parameter: name, script: hostScript })).pipe(
        Option.orElse(() => unlinked),
        Option.orElse(() => Option.as(script, runtime)),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const scripts = Effect.fnUntraced(function* (links: { readonly peers: Effect.Effect<readonly Link[]>; readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof EvaluationRpcs>, RpcClientError.RpcClientError>, Unmatched> }, identifier: string) {
    const [server, rows, declarations] = yield* Effect.all([ApplicationServer, RegistryApplications, Declarations]);
    const dictionaries = yield* Cache.makeWith((href: string) => server.dictionary({ url: new URL(href) }), { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) });
    const dictionary = (url: URL) => Cache.get(dictionaries, url.href);
    const owned = Schema.is(Schema.TemplateLiteral([`${identifier}.`, Schema.String]));
    return Effect.fnUntraced(function* ({ row: { url, name }, live }: { readonly row: Entry; readonly live: InstanceSnapshot }, code: string) {
        const [listed, peers] = yield* Effect.all([SubscriptionRef.get(rows), links.peers]);
        const row = yield* Effect.fromOption(
            Array.findFirst(listed, (each) => each.url.href === url.href),
            () => new Cause.NoSuchElementError(`${name} left the installed Adobe applications`),
        );
        const serving = Predicate.and(Link.runsIn(row), Link.serves(EvaluationRpcs));
        const declared = yield* Option.match(Array.findFirst(peers, serving), { onSome: () => Effect.succeedSome<Route>({ _tag: 'Linked' }), onNone: () => route(dictionary, owned, row) });
        const found = yield* Effect.fromOption(declared, () => new Cause.NoSuchElementError(`${row.name} declares no script command and links no plugin`));
        const scriptPlan = plan(found);
        yield* declarations.check(row, scriptPlan.program(code), openLine);
        const built = yield* compile(code, scriptPlan);
        const linked = Effect.flatMap(links.call(serving, `linked plugin of ${row.name}`), (client) => client('evaluate', { code: built.code, label: built.label }));
        const scripted = Effect.gen(function* () {
            const { reply, written } = yield* server.execute({ instance: live.instance, arguments: { direct: built.direct, ...built.declared } });
            const completion = yield* Effect.mapError(Schema.decodeUnknownEffect(Reply)(reply), HostError.refused(`${row.name} script reply`));
            return { ...(yield* Completion.decode(ResultJson, built.label)(completion)), written };
        });
        const unlinked = Effect.fail(HostError.of({ _tag: 'Unavailable' }, `linked Atelier plugin running UXP scripts in ${row.name}`)());
        const run: Effect.Effect<Effect.Success<typeof linked> & { readonly written?: typeof Written.Type }, HostError | Error> = Match.valueTags(found, { Linked: () => linked, Unlinked: () => unlinked, Uxp: () => scripted, Acrobat: () => scripted, Script: () => scripted });
        return yield* Effect.catchTag(run, 'HostError', built.relined);
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { DESCRIPTION, scripts };
