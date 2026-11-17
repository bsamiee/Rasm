import { ApplicationServer } from '@rasm/applications/node';
import { ExtensionManifest } from '@rasm/cep';
import type { Application } from '@rasm/creative-cloud-desktop';
import { Completion } from '@rasm/extendscript';
import runtime from '@rasm/extendscript/host?inline';
import { lower, traced } from '@rasm/extendscript/node';
import shim from '@rasm/extendscript/shim?inline';
import { Link, Server } from '@rasm/mcp';
import { Evaluation, HostError, Script } from '@rasm/object-model';
import type { Unmatched } from '@rasm/search';
import { UxpManifest } from '@rasm/uxp';
import { Array, Cause, Context, Effect, Match, Option, Record, Ref, Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { XMLParser } from 'fast-xml-parser';
import { Inventory } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

interface Plan {
    readonly program: (body: string) => string;
    readonly target: typeof UxpManifest.target | typeof ExtensionManifest.target;
    readonly offset: number;
    readonly direct: (source: string) => string;
    readonly declared: Record.ReadonlyRecord<string, { readonly _tag: 'enumerator'; readonly name: string }>;
}
type Runtime = { readonly _tag: 'Linked' } | { readonly _tag: 'Uxp'; readonly parameter: string } | { readonly _tag: 'Script'; readonly command: string } | { readonly _tag: 'Unlinked' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENTS = ['DjxM', 'dosc'] as const;
const UXP_SCRIPT = 'uxpscript';
const PARSER = new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '@_', isArray: (name) => ['suite', 'command', 'parameter', 'enumeration', 'enumerator'].includes(name) });

// --- [MODELS] --------------------------------------------------------------------------

const Parameters = Schema.Struct({
    dictionary: Schema.Struct({ suite: Schema.Array(Schema.Struct({ command: Schema.Array(Schema.Struct({ parameter: Schema.Array(Schema.Struct({ '@_code': Schema.String, '@_type': Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })) })) }),
}).annotate({ description: 'Scripting definition of one command, its parameters with their codes and types' });
const Enumerators = Schema.Struct({
    dictionary: Schema.Struct({ suite: Schema.Array(Schema.Struct({ enumeration: Schema.Array(Schema.Struct({ enumerator: Schema.Array(Schema.Struct({ '@_name': Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })) })) }),
}).annotate({ description: 'Scripting definition of one enumeration, its enumerator names' });
const Scripted = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Completion)));
const ResultJson = Schema.toCodecJson(Evaluation.Result);
const Body = Schema.Struct({ code: Evaluation.fields.code.annotate({ description: 'Function body, return gives result' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (source: string) => `(function (self) {\n${source}\nif (typeof Object.keys !== 'function') {\n${shim}\n}\n${runtime}\nreturn encodeURIComponent(runtime.reply(function () { return { result: body.call(self) }; }));\n})(this)`;
const uxpscript = (source: string) =>
    [
        `(() => { const { script } = require('uxp'); const answer = (reply) => script.setResult(encodeURIComponent(reply)); try {`,
        `const { app, UserInteractionLevels } = require('indesign'); const preferences = app.scriptPreferences; const level = preferences.userInteractionLevel; const prepare = Error.prepareStackTrace; const lines = [];`,
        runtime,
        `preferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;`,
        `Error.prepareStackTrace = (error, frames) => { lines.push(...frames.filter((frame) => frame.getScriptNameOrSourceURL() === 'body.js').map((frame) => frame.getLineNumber())); return String(error); };`,
        `try { answer(runtime.reply(() => { let value; try { value = new Function(${JSON.stringify(`return ${source}\n//# sourceURL=body.js`)})()(); } catch (error) { void error?.stack; throw { message: String(error?.message ?? error), line: lines[0] }; }`,
        `if (typeof value?.then === 'function') { value.then(undefined, () => undefined); throw { _tag: 'IllegalArgument', message: 'body returned a promise, a do script body runs synchronously' }; } return { result: value }; }));`,
        `} finally { preferences.userInteractionLevel = level; Error.prepareStackTrace = prepare; } } catch (error) { answer(JSON.stringify({ _tag: 'Thrown', message: String(error?.message ?? error) })); } })();`,
    ].join('\n');
const parsed = (xml: string) => Effect.try((): unknown => PARSER.parse(xml));
const relined = (line: (at: number) => Option.Option<number>) => (error: HostError) => Effect.fail(HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? HostError.thrown(error.subject, Option.flatMap(error.reason.line, line))(error.cause) : error);
const planned = Match.typeTags<Runtime>()({
    Linked: (): Plan => ({ program: (body: string) => Evaluation.expression(body, true), target: UxpManifest.target, offset: 0, direct: (source: string) => source, declared: {} }),
    Unlinked: (): Plan => ({ program: (body: string) => Evaluation.expression(body, true), target: UxpManifest.target, offset: 0, direct: (source: string) => source, declared: {} }),
    Uxp: ({ parameter }): Plan => ({ program: (body: string) => Evaluation.expression(body, false), target: UxpManifest.target, offset: 2, direct: uxpscript, declared: { [parameter]: { _tag: 'enumerator' as const, name: UXP_SCRIPT } } }),
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
const enumerates = Effect.fnUntraced(function* (server: ApplicationServer['Service'], url: URL, type: string) {
    const { dictionary: defined } = yield* server.definition({ url, kind: 'enumeration', name: type }).pipe(Effect.flatMap(parsed), Effect.flatMap(Schema.decodeUnknownEffect(Enumerators)));
    return defined.suite.flatMap(Struct.get('enumeration')).flatMap(Struct.get('enumerator')).map(Struct.get('@_name')).includes(UXP_SCRIPT);
});
const commanded = Effect.fnUntraced(function* (server: ApplicationServer['Service'], url: URL, name: string, enumerations: readonly string[]) {
    const { dictionary: defined } = yield* server.definition({ url, kind: 'command', name }).pipe(Effect.flatMap(parsed), Effect.flatMap(Schema.decodeUnknownEffect(Parameters)));
    const typed = defined.suite
        .flatMap(Struct.get('command'))
        .flatMap(Struct.get('parameter'))
        .filter((parameter): parameter is { readonly '@_code': string; readonly '@_type': string } => parameter['@_type'] !== undefined && enumerations.includes(parameter['@_type']));
    const language = yield* Effect.findFirst(typed, (parameter) => enumerates(server, url, parameter['@_type']));
    return Option.map(language, (parameter) => parameter['@_code']);
});
const dictionary = Effect.fnUntraced(
    function* (server: ApplicationServer['Service'], { url, documents }: Application) {
        const { command, enumeration } = yield* server.terms({ url });
        const chosen = Array.findFirst(EVENTS, (event) => Array.findFirst(command, ({ code }) => code.endsWith(event)));
        const language = Option.flatten(yield* Effect.transposeOption(Option.map(chosen, ({ name }) => commanded(server, url, name, Array.map(enumeration, Struct.get('name'))))));
        const uxp = Object.values(UxpManifest.scripts).some((extension) => documents.includes(extension));
        return Option.match(language, {
            onSome: (parameter) => Option.some<Runtime>({ _tag: 'Uxp', parameter }),
            onNone: () => (uxp ? Option.some<Runtime>({ _tag: 'Unlinked' }) : Option.map(chosen, ({ name }): Runtime => ({ _tag: 'Script', command: name }))),
        });
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
            const scripted = server.script({ instance: live.instance, url: row.url, events: EVENTS, declared: built.declared, direct: built.direct }).pipe(
                Effect.flatMap(({ reply }) => Effect.mapError(Schema.decodeUnknownEffect(Scripted)(reply), HostError.refused(`${row.name} script reply`))),
                Effect.flatMap(Completion.answered(ResultJson, label)),
            );
            const unlinked = Effect.fail(HostError.of({ _tag: 'Unavailable' }, `${row.name} runs UXP scripts through its Atelier extension, which is not linked`)());
            const run: Effect.Effect<typeof Evaluation.Result.Type, HostError | Error> = Match.valueTags(found, { Linked: () => evaluated, Unlinked: () => unlinked, Uxp: () => scripted, Script: () => scripted });
            return yield* Effect.catchTag(run, 'HostError', built.relined);
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { scripts };
