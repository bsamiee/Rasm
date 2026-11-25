import type { RunningInstance, Terms } from '@rasm/applications';
import type { Entry } from '@rasm/applications/inventory';
import { ApplicationServer } from '@rasm/applications/node';
import type { Application } from '@rasm/creative-cloud-desktop';
import { Completion } from '@rasm/extendscript';
import runtime from '@rasm/extendscript/host?inline';
import { lower, traced } from '@rasm/extendscript/node';
import { ScriptHost as Illustrator } from '@rasm/illustrator';
import { ScriptHost as InDesign } from '@rasm/indesign';
import { Link } from '@rasm/mcp';
import { Evaluation, HostError, Script } from '@rasm/object-model';
import type { Unmatched } from '@rasm/search';
import { HOSTS, UxpManifest } from '@rasm/uxp';
import { Array, Cause, Effect, Match, Option, Predicate, Record, Ref, Schema, SemVer, Struct, SubscriptionRef } from 'effect';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import shim from 'es5-shim?inline';
import sham from 'es5-shim/es5-sham.js?inline';
import { Installed } from '#/applications';
import { Declarations } from '#/declarations';

// --- [TYPES] ---------------------------------------------------------------------------

interface Plan {
    readonly program: (body: string) => string;
    readonly engine: Parameters<typeof lower>[1];
    readonly offset: number;
    readonly direct: (source: string, label: string) => string;
    readonly declared: Record.ReadonlyRecord<string, { readonly _tag: 'enumerator'; readonly name: string }>;
}
type Runtime = { readonly _tag: 'Linked' } | { readonly _tag: 'Uxp'; readonly parameter: string; readonly host: typeof InDesign } | { readonly _tag: 'Unlinked' } | { readonly _tag: 'Script'; readonly around: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const UXP: Record.ReadonlyRecord<string, typeof InDesign> = { [HOSTS.ID.product]: InDesign };
const EXTENDSCRIPT: Record.ReadonlyRecord<string, typeof Illustrator> = { [HOSTS.AI.product]: Illustrator };
const PLUGGED = Record.fromIterableBy(Object.values(HOSTS), Struct.get('product'));
const RELEASE = /^\d+\.\d+\.\d+/u;
const DESCRIPTION = `Runs TypeScript function body, return giving its result, in the target application through its newest runtime: a linked extension, do script uxpscript, or ExtendScript; returns its result as JSON, failures name the body line; a dialog needing an answer, open before the call or raised by it, the application exiting, or it turning unresponsive returns the call at once, its result arriving in finished of a later result. UXP bodies take their host module from require('<host>') (${Declarations.checked.join(', ')}), checked first against its declarations, which refuse a wrong member, type, read-only write, or enumeration value with a link to atelier://object-model/{host}/{name}`;

// --- [MODELS] --------------------------------------------------------------------------

const Scripted = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Completion)));
const ResultJson = Schema.toCodecJson(Evaluation.Result);

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (source: string) => `(function (self) {\n${source}\nif (typeof Object.keys !== 'function') {\n${shim}\n${sham}\n}\n${runtime}\nreturn encodeURIComponent(runtime.reply(function () { return around(self, function () { return { result: body.call(self) }; }, runtime); }));\n})(this)`;
const relined = (line: (at: number) => Option.Option<number>) => (error: HostError) => Effect.fail(HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? HostError.thrown(error.subject, Option.flatMap(error.reason.line, line))(error.cause) : error);
const evaluated = (): Plan => ({ program: (body: string) => Evaluation.expression(body, true), engine: { targets: UxpManifest.engine }, offset: 0, direct: (source: string) => source, declared: {} });
const planned = Match.typeTags<Runtime>()({
    Linked: evaluated,
    Unlinked: evaluated,
    Uxp: ({ parameter, host }): Plan => ({
        program: (body: string) => Evaluation.expression(body, false),
        engine: { targets: UxpManifest.engine },
        offset: UxpManifest.script.offset,
        direct: (source: string, label: string) => UxpManifest.script(source, { runtime, host, label }),
        declared: { [parameter]: { _tag: 'enumerator' as const, name: UxpManifest.scriptLanguage } },
    }),
    Script: ({ around }): Plan => ({ program: (body: string) => `var body = ${Evaluation.expression(body, false)};\nvar around = ${around};`, engine: lower.extendscript, offset: 1, direct: extendscript, declared: {} }),
});
const opened = (line: number) => Option.liftPredicate(line - 1, (at) => at > 0);
const compiled = Effect.fnUntraced(function* (body: string, plan: Plan) {
    const { code, map } = yield* lower(plan.program(body), plan.engine, 'typescript').pipe(Effect.catchTag('HostError', relined(opened)));
    const [original, label] = [yield* traced(map), Evaluation.labelled(body)];
    return { code, label, direct: plan.direct(code, label), declared: plan.declared, relined: relined((line) => Option.map(original(line - plan.offset), (entry) => entry.line - 1)) };
});
const routed = Effect.fnUntraced(
    function* <E, R>(terms: (target: Pick<Application, 'url'>) => Effect.Effect<Pick<Terms, 'script'>, E, R>, { url, product, version }: Pick<Application, 'url' | 'product' | 'version'>) {
        const released = Option.flatMap(Option.fromNullishOr(RELEASE.exec(version)?.[0]), Schema.decodeOption(Schema.SemVerFromString));
        const plugged = Option.exists(Record.get(PLUGGED, product), ({ runs, since }) => runs && Option.exists(released, (at) => SemVer.isGreaterThanOrEqualTo(at, since)));
        const unlinked = Option.liftPredicate<Runtime>({ _tag: 'Unlinked' }, () => plugged);
        const host = Record.get(UXP, product);
        if (plugged && Option.isNone(host)) {
            return unlinked;
        }
        const { script } = yield* terms({ url });
        const language = Option.flatMap(script, ({ parameters }) => Array.findFirst(parameters, ({ enumerators }) => Array.contains(Array.map(enumerators, Struct.get('code')), UxpManifest.scriptLanguage)));
        const around = Option.getOrElse(Option.map(Record.get(EXTENDSCRIPT, product), Struct.get('around')), () => String(<A>(_: unknown, body: () => A): A => body()));
        return Option.map(Option.all({ parameter: language, host }), ({ parameter: { name }, host: scripted }): Runtime => ({ _tag: 'Uxp', parameter: name, host: scripted })).pipe(
            Option.orElse(() => unlinked),
            Option.orElse(() => Option.as(script, { _tag: 'Script', around } as const)),
        );
    },
    Effect.tapError((cause) => Effect.logDebug('Scripting dictionary unread', cause)),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const scripts = Effect.fnUntraced(function* (links: { readonly peers: Effect.Effect<readonly Link[]>; readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof Script>, RpcClientError.RpcClientError>, Unmatched> }) {
    const [server, rows, dictionaries, declarations] = yield* Effect.all([ApplicationServer, Installed, Ref.make<Record.ReadonlyRecord<string, Option.Option<Runtime>>>({}), Declarations]);
    const read = Effect.fnUntraced(function* (row: Application) {
        const known = Record.get(yield* Ref.get(dictionaries), row.url.href);
        const fresh = Effect.tap(routed(server.terms, row), (found) => Ref.update(dictionaries, Record.set(row.url.href, found)));
        return yield* Option.match(known, { onSome: Effect.succeed, onNone: () => fresh });
    });
    return Effect.fnUntraced(function* ({ row: { url, name }, live }: { readonly row: Entry; readonly live: RunningInstance }, code: string) {
        const [listed, peers] = yield* Effect.all([SubscriptionRef.get(rows), links.peers]);
        const row = yield* Effect.fromOption(
            Array.findFirst(listed, (each) => each.url.href === url.href),
            () => new Cause.NoSuchElementError(`${name} left the installed Adobe applications`),
        );
        const serving = Predicate.and(Link.hosted(row), Link.serves(Script.requests.keys()));
        const declared = yield* Option.match(Array.findFirst(peers, serving), { onSome: () => Effect.succeedSome<Runtime>({ _tag: 'Linked' }), onNone: () => read(row) });
        const found = yield* Effect.fromOption(declared, () => new Cause.NoSuchElementError(`${row.name} declares no script command and links no extension`));
        const plan = planned(found);
        yield* declarations.check(row, plan.program(code), opened);
        const built = yield* compiled(code, plan);
        const linked = Effect.flatMap(links.call(serving, `linked extension of ${row.name}`), (client) => client('evaluate', { code: built.code, label: built.label }));
        const scripted = server.execute({ instance: live.instance, arguments: { direct: built.direct, ...built.declared } }).pipe(
            Effect.flatMap((reply) => Effect.mapError(Schema.decodeUnknownEffect(Scripted)(reply), HostError.refused(`${row.name} script reply`))),
            Effect.flatMap(Completion.answered(ResultJson, built.label)),
        );
        const unlinked = Effect.fail(HostError.of({ _tag: 'Unavailable' }, `linked Atelier extension running UXP scripts in ${row.name}`)());
        const run: Effect.Effect<Effect.Success<typeof linked>, HostError | Error> = Match.valueTags(found, { Linked: () => linked, Unlinked: () => unlinked, Uxp: () => scripted, Script: () => scripted });
        return yield* Effect.catchTag(run, 'HostError', built.relined);
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { DESCRIPTION, scripts };
