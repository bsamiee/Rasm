import { createHash } from 'node:crypto';
import { pathToFileURL } from 'node:url';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Effect, FileSystem, Option, Result, Schema, Stream, String } from 'effect';
import { ChildProcess } from 'effect/process';
import { HarnessError, thrown } from '#/error';
import { ENGINES, Encoded, Engine, HostCase, type Placed, Recorded, Reply, Survey } from '#/indesign/contract';
import { precondition } from '#/indesign/precondition';
import { copied } from '#/precondition';
import { replied } from '#/results';
import uxpscript from '#/indesign/uxpscript/script.config';
import { type Batch, type Host as Harness, installation, type Live } from '#/session';
import { assembled, engineOf } from '#/source';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTER = 'com.adobe.crashreporter';
const REVEALING = /\.\s*(?<member>revealInSystem|revealInBridge)\s*\(/u;
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);

// --- [MODELS] --------------------------------------------------------------------------

const Continuing = Schema.Union([Schema.Struct({ type: Schema.Literal('AwaitExpression') }), Schema.Struct({ async: Schema.Literal(true) }), Schema.Struct({ type: Schema.Literal('MemberExpression'), property: Schema.Struct({ name: Schema.Literals(['then', 'catch', 'finally']) }) })]).annotate({
    description: 'Syntax tree node whose work runs after the script answered',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const bundle = Effect.map(
    Effect.fromOption(Option.fromUndefinedOr(uxpscript.build?.outDir), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'script config states no build.outDir' }, subject: '@rasm/harnesses-indesign-uxpscript' })),
    (folder) => pathToFileURL(`${folder}/main.${ENGINES.uxpscript.extensions[0]}`),
);
const installed = Effect.gen(function* () {
    const automation = yield* Automation;
    const [rows, opening] = yield* Effect.all([automation.installed, Effect.flatMap(bundle, (toOpen) => automation.applications({ toOpen }))]);
    return yield* Effect.forEach(
        rows.filter((row) => opening.some((url) => url.href === row.url.href)),
        (row) => installation(row, [REPORTER]),
    );
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
const unzipped = Effect.fn('unzipped')(function* (args: readonly string[]) {
    const handle = yield* ChildProcess.make('unzip', args, { stdin: 'ignore' });
    const [bytes, errors, code] = yield* Effect.all([Stream.mkUint8Array(handle.stdout), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? bytes : yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code}: ${errors}` }, subject: ['unzip', ...args].join(' ') });
}, Effect.scoped);
const unpacked = Effect.fn('unpacked')(function* (file: string) {
    const entries = Array.fromIterable(String.linesIterator(new TextDecoder().decode(yield* unzipped(['-Z1', file]))))
        .filter((entry) => entry !== 'META-INF/metadata.xml')
        .toSorted();
    const parts = yield* Effect.forEach(entries, (entry) => Effect.map(unzipped(['-p', file, entry]), (bytes) => [entry, bytes] as const));
    return parts.reduce((hash, [entry, bytes]) => hash.update(entry).update(bytes), createHash('sha256')).digest('hex');
});
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    return { file, size: bytes.byteLength, sha256: file.endsWith('.idml') ? yield* unpacked(file) : createHash('sha256').update(bytes).digest('hex') };
});

// --- [TRANSPORT]
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const automation = yield* Automation;
    const scripts = Array.getSomes(batch.cases.map((row) => Option.map(Option.fromUndefinedOr(row._tag === 'Probe' ? undefined : row.source), ({ script }) => script)));
    const held = Option.isNone(batch.survey) ? 'uxpscript' : yield* Effect.fromResult(engine(scripts));
    const encoded = yield* Schema.encodeEffect(Encoded)({ records: batch.records, label: batch.label, enumerations: Option.match(batch.survey, { onNone: () => ({}), onSome: ({ enumerations }) => enumerations }), survey: Option.isNone(batch.survey), cases: batch.cases });
    const direct = held === 'extendscript' ? yield* assembled(ENGINE) : { _tag: 'file' as const, url: (yield* bundle).href };
    const { reply } = yield* automation.execute({ pid: live.pid, command: 'do script', arguments: { direct, language: ENGINES[held].language, 'with arguments': [encoded] }, artifacts: [] });
    return yield* replied(`${held} batch`)(yield* Schema.decodeUnknownEffect(Reply)(reply));
});
// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'indesign',
    Engine,
    survey: { question: 'Enumerations, working space profiles, languages, locale, and feature set the installation declares', Schema: Survey, host: (survey) => Option.some(survey.host) },
    Case: HostCase,
    Recorded,
    shown: ['alerts', 'activations', 'console'],
    progress: [],
    engine,
    precondition,
    placed: copied,
    refusal,
    installed,
    windowed: () => true,
    launch: () => Effect.succeed({ fresh: false }),
    opened: () => Effect.succeedNone,
    send,
    quit: (live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: 'quit', arguments: { saving: { _tag: 'enumerator', name: 'no' } }, artifacts: [] }))),
    digest,
    compare: { _tag: 'Imported' },
} as const satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
