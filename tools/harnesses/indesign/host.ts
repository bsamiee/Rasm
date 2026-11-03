import { pathToFileURL } from 'node:url';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Effect, Option, Record, Result, Schema, String } from 'effect';
import { crashes, HOME, REPORTER, uxp } from '#/adobe';
import { HarnessError, thrown } from '#/error';
import { ENGINES, Encoded, Engine, fields, HostCase, type Placed, Reply, Survey } from '#/indesign/contract';
import { precondition } from '#/indesign/precondition';
import uxpscript from '#/indesign/uxpscript/script.config';
import { checked, Launched, plist, printed } from '#/instance';
import { copied } from '#/precondition';
import type { Rule } from '#/report';
import { type Batch, type Host as Harness, type Instance, installation, type Live } from '#/session';
import { assembled, engineOf } from '#/source';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REVEALING = /\.\s*(?<member>revealInSystem|revealInBridge)\s*\(/u;
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);

// --- [MODELS] --------------------------------------------------------------------------

const Continuing = Schema.Union([Schema.Struct({ type: Schema.Literal('AwaitExpression') }), Schema.Struct({ async: Schema.Literal(true) }), Schema.Struct({ type: Schema.Literal('MemberExpression'), property: Schema.Struct({ name: Schema.Literals(['then', 'catch', 'finally']) }) })]).annotate({
    description: 'Syntax tree node whose work runs after the script answered',
});
const Bundle = Schema.Struct({ CFBundleName: Schema.String.annotate({ description: 'InDesign or InDesign (Beta), naming the cache folder Adobe <name>' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const bundle = Effect.map(
    Effect.fromOption(Option.fromUndefinedOr(uxpscript.build?.outDir), () => HarnessError.unavailable('@rasm/harnesses-indesign-uxpscript')('script config states no build.outDir')),
    (folder) => pathToFileURL(`${folder}/main.${ENGINES.uxpscript.extensions[0]}`),
);
const installed = Effect.gen(function* () {
    const [automation, toOpen] = yield* Effect.all([Automation, bundle]);
    const [opening, rows] = yield* Effect.all([automation.opening(toOpen), automation.installed]);
    return yield* Effect.forEach(opening, (row) => installation(row, rows, [REPORTER]));
});

// --- [SOURCE]
const engine = engineOf(ENGINES, 'extendscript');
const continued = (code: string): Option.Option<string> =>
    Result.match(
        Result.try(() => parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true })),
        {
            onFailure: (error) => Option.some(`source does not parse: ${thrown(error)}`),
            onSuccess: (tree) => Option.map(Option.fromNullishOr(findNodeAt(tree, undefined, undefined, (_, node) => Schema.is(Continuing)(node))), ({ node }) => `UXPScript source ${code.slice(node.start, node.end)} continues after the answer`),
        },
    );
const refusal = (held: typeof Engine.Type, hosted: typeof HostCase.Type, binary: boolean) => {
    const { set = [], add = [], select = [], steps = [] } = hosted.precondition;
    const expressions = [...Object.values(hosted.read), ...set.map(([target]) => target), ...add.map(({ to }) => to), ...select, ...HostCase.match(hosted, { Probe: ({ evaluate }) => [evaluate], Call: ({ call }) => [call], Run: () => [] })];
    const statements = [...steps, ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(hosted._tag === 'Probe' ? undefined : hosted.source), ({ code }) => Option.liftPredicate(code, () => !binary)))];
    return held === 'uxpscript'
        ? Option.firstSomeOf([...expressions.map((expression) => continued(`(${expression});`)), ...statements.map(continued)])
        : Option.firstSomeOf([...expressions, ...statements].map((text) => Option.map(Option.fromNullishOr(REVEALING.exec(text)?.groups?.member), (member) => `ExtendScript ${member} activates Finder or Bridge`)));
};

// --- [DIGESTS]
const unpacked = Effect.fn('unpacked')(function* (file: string) {
    const entries = Array.fromIterable(String.linesIterator(yield* printed('unzip', ['-Z1', file])))
        .filter((entry) => entry !== 'META-INF/metadata.xml')
        .toSorted();
    return (yield* Effect.forEach(entries, (entry) => Effect.map(checked('unzip', ['-p', file, entry]), (bytes) => [entry, bytes] as const))).flat();
});

// --- [LOGS]
const logs = Effect.fn('logs')(function* (instance: Instance) {
    const { CFBundleName: name } = yield* plist(instance.chosen.app, Bundle);
    const [major] = String.split(instance.chosen.version, '.');
    const cache = `Library/Caches/Adobe ${name}/Version ${major}.*/*`;
    const folders: Record.ReadonlyRecord<string, string> = { '21': 'Adobe InDesign 2024', '22': 'Adobe InDesign' };
    return [
        { kind: 'shutdown', pattern: `${cache}/InDesign Recovery/ProtectiveShutdownLog`, root: HOME },
        { kind: 'recovery', pattern: `${cache}/InDesign Recovery/{DBSharingShi,DBTmp}${instance.pid}*`, root: HOME },
        ...Option.toArray(Option.map(Record.get(folders, major), uxp)),
        ...(yield* crashes(instance)),
    ] satisfies readonly Rule[];
});

// --- [TRANSPORT]
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const automation = yield* Automation;
    const scripts = Array.getSomes(batch.cases.map((row) => Option.map(Option.fromUndefinedOr(row._tag === 'Probe' ? undefined : row.source), ({ script }) => script)));
    const held = Option.isNone(batch.survey) ? 'uxpscript' : yield* Effect.fromResult(engine(scripts));
    const encoded = yield* Schema.encodeEffect(Encoded)({ records: batch.records, label: batch.label, enumerations: Option.match(batch.survey, { onNone: () => ({}), onSome: ({ enumerations }) => enumerations }), survey: Option.isNone(batch.survey), cases: batch.cases });
    const direct = held === 'extendscript' ? yield* assembled(ENGINE) : { _tag: 'file' as const, url: (yield* bundle).href };
    const { reply } = yield* automation.script({ pid: live.pid, direct, values: [encoded], language: ENGINES[held].language });
    return yield* Reply.replied(`${held} batch`)(reply);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    Engine,
    survey: { question: 'Enumerations, working space profiles, languages, locale, and feature set the installation declares', Schema: Survey },
    Case: HostCase,
    fields,
    engine,
    precondition,
    placed: copied,
    refusal,
    installed,
    launch: () => Effect.succeed(new Launched({ fresh: false })),
    send,
    quit: (live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: 'quit', arguments: { saving: { _tag: 'enumerator', name: 'no' } } }))),
    canonical: (file, bytes) => (file.endsWith('.idml') ? unpacked(file) : Effect.succeed([bytes])),
    logs,
} as const satisfies Harness<typeof Survey.Type, typeof HostCase.Type, typeof Engine.Type, typeof Placed.Type, typeof fields>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
