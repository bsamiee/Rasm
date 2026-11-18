import { ApplicationServer } from '@rasm/applications/node';
import { ExtensionManifest } from '@rasm/cep';
import type { Application } from '@rasm/creative-cloud-desktop';
import { Completion } from '@rasm/extendscript';
import runtime from '@rasm/extendscript/host?inline';
import { lower, traced } from '@rasm/extendscript/node';
import shim from '@rasm/extendscript/shim?inline';
import { ScriptHost } from '@rasm/indesign';
import { Link, Server } from '@rasm/mcp';
import { Evaluation, HostError, Script } from '@rasm/object-model';
import type { Unmatched } from '@rasm/search';
import { UxpManifest } from '@rasm/uxp';
import { Array, Cause, Context, Effect, Match, Option, Record, Ref, Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { Inventory } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

interface Plan {
    readonly program: (body: string) => string;
    readonly target: typeof UxpManifest.target | typeof ExtensionManifest.target;
    readonly offset: number;
    readonly direct: (source: string) => string;
    readonly declared: Record.ReadonlyRecord<string, { readonly _tag: 'enumerator'; readonly name: string }>;
}
type Runtime = { readonly _tag: 'Linked' } | { readonly _tag: 'Uxp'; readonly parameter: string; readonly host: typeof ScriptHost } | { readonly _tag: 'Script' } | { readonly _tag: 'Unlinked' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTS: Partial<Record<keyof typeof UxpManifest.scripts, typeof ScriptHost>> = { ID: ScriptHost };

// --- [MODELS] --------------------------------------------------------------------------

const Scripted = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Completion)));
const ResultJson = Schema.toCodecJson(Evaluation.Result);
const Body = Schema.Struct({ code: Evaluation.fields.code.annotate({ description: 'Function body, return gives result' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (source: string) => `(function (self) {\n${source}\nif (typeof Object.keys !== 'function') {\n${shim}\n}\n${runtime}\nreturn encodeURIComponent(runtime.reply(function () { return { result: body.call(self) }; }));\n})(this)`;
const relined = (line: (at: number) => Option.Option<number>) => (error: HostError) => Effect.fail(HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? HostError.thrown(error.subject, Option.flatMap(error.reason.line, line))(error.cause) : error);
const planned = Match.typeTags<Runtime>()({
    Linked: (): Plan => ({ program: (body: string) => Evaluation.expression(body, true), target: UxpManifest.target, offset: 0, direct: (source: string) => source, declared: {} }),
    Unlinked: (): Plan => ({ program: (body: string) => Evaluation.expression(body, true), target: UxpManifest.target, offset: 0, direct: (source: string) => source, declared: {} }),
    Uxp: ({ parameter, host }): Plan => ({
        program: (body: string) => Evaluation.expression(body, false),
        target: UxpManifest.target,
        offset: UxpManifest.script.offset,
        direct: (source: string) => UxpManifest.script(source, { runtime, host }),
        declared: { [parameter]: { _tag: 'enumerator' as const, name: UxpManifest.scriptLanguage } },
    }),
    Script: (): Plan => ({ program: (body: string) => `var body = ${Evaluation.expression(body, false)}`, target: ExtensionManifest.target, offset: 1, direct: extendscript, declared: {} }),
});
const compiled = Effect.fnUntraced(function* (body: string, reached: Runtime) {
    const plan = planned(reached);
    const { code, map } = yield* lower(plan.program(body), plan.target, 'typescript').pipe(
        Effect.catchTag(
            'HostError',
            relined((line) => Option.liftPredicate(line - 1, (at) => at > 0)),
        ),
    );
    const original = yield* traced(map);
    return { code, direct: plan.direct(code), declared: plan.declared, relined: relined((line) => Option.map(original(line - plan.offset), (entry) => entry.line - 1)) };
});
const dictionary = Effect.fnUntraced(
    function* (server: ApplicationServer['Service'], { url, documents }: Application) {
        const { script } = yield* server.terms({ url });
        const language = Option.flatMap(script, ({ parameters }) => Array.findFirst(parameters, ({ enumerators }) => Array.contains(Array.map(enumerators, Struct.get('code')), UxpManifest.scriptLanguage)));
        return Option.match(
            Record.findFirst(UxpManifest.scripts, (extension) => documents.includes(extension)),
            {
                onNone: (): Option.Option<Runtime> => Option.as(script, { _tag: 'Script' } as const),
                onSome: ([app]): Option.Option<Runtime> =>
                    Option.some(
                        Option.match(Option.all({ parameter: language, host: Option.fromUndefinedOr(HOSTS[app]) }), {
                            onNone: (): Runtime => ({ _tag: 'Unlinked' }),
                            onSome: ({ parameter: { name }, host }): Runtime => ({ _tag: 'Uxp', parameter: name, host }),
                        }),
                    ),
            },
        );
    },
    Effect.tapError((cause) => Effect.logDebug('Scripting dictionary unread', cause)),
);
const reached = (peers: readonly Link[], row: Application) =>
    Option.as(
        Array.findFirst(peers, (peer) => Link.hosted(row)(peer) && Link.serves(Script.requests.keys())(peer)),
        { _tag: 'Linked' } as const satisfies Runtime,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const scripts = Effect.fnUntraced(function* (links: { readonly peers: Effect.Effect<readonly Link[]>; readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof Script>, RpcClientError.RpcClientError>, Unmatched> }) {
    const [server, { current, ends }, dictionaries] = yield* Effect.all([ApplicationServer, Inventory, Ref.make<Record.ReadonlyRecord<string, Option.Option<Runtime>>>({})]);
    const read = Effect.fnUntraced(function* (row: Application) {
        const known = Record.get(yield* Ref.get(dictionaries), row.url.href);
        const fresh = Effect.tap(dictionary(server, row), (found) => Ref.update(dictionaries, Record.set(row.url.href, found)));
        return yield* Option.match(known, { onSome: Effect.succeed, onNone: () => Effect.orElseSucceed(fresh, Option.none<Runtime>) });
    });
    yield* Server.registerTool(
        Tool.make('execute_script', {
            description:
                'Runs TypeScript function body in the target application through its newest runtime: a linked extension, do script uxpscript, or ExtendScript; returns its result as JSON, failures name the body line, a dialog needing an answer, the application exiting, or it turning unresponsive returns the call at once',
            parameters: Body,
            success: Evaluation.Result,
        }).annotateMerge(Context.add(ends, Tool.OpenWorld, false)),
        Effect.fnUntraced(function* ({ code }) {
            const [{ row, live }, peers] = yield* Effect.all([current, links.peers]);
            const declared = yield* Option.match(reached(peers, row), { onSome: Effect.succeedSome, onNone: () => read(row) });
            const found = yield* Effect.fromOption(declared, () => new Cause.NoSuchElementError(`${row.name} declares no script command and links no extension`));
            const built = yield* compiled(code, found);
            const { label } = Evaluation.make({ code: Evaluation.expression(code, false) });
            const evaluated = Effect.flatMap(
                links.call((peer) => Link.hosted(row)(peer) && Link.serves(Script.requests.keys())(peer), `linked extension of ${row.name}`),
                (client) => client('evaluate', { code: built.code }),
            );
            const scripted = server.execute({ instance: live.instance, arguments: { direct: built.direct, ...built.declared } }).pipe(
                Effect.flatMap(({ reply }) => Effect.mapError(Schema.decodeUnknownEffect(Scripted)(reply), HostError.refused(`${row.name} script reply`))),
                Effect.flatMap(Completion.answered(ResultJson, label)),
            );
            const unlinked = Effect.fail(HostError.of({ _tag: 'Unavailable' }, `linked Atelier extension running UXP scripts in ${row.name}`)());
            const run: Effect.Effect<typeof Evaluation.Result.Type, HostError | Error> = Match.valueTags(found, { Linked: () => evaluated, Unlinked: () => unlinked, Uxp: () => scripted, Script: () => scripted });
            return yield* Effect.catchTag(run, 'HostError', built.relined);
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { scripts };
