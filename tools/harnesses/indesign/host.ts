import { createHash } from 'node:crypto';
import { pathToFileURL } from 'node:url';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Effect, FileSystem, Option, Path, type PlatformError, Record, Result, Schema, Stream, String } from 'effect';
import { ChildProcess } from 'effect/process';
import { HarnessError, Thrown } from '#/error';
import { ENGINES, Encoded, Engine, HostCase, type Placed, Recorded, Reply, Survey } from '#/indesign/contract';
import { precondition } from '#/indesign/precondition';
import uxpscript from '#/indesign/uxpscript/script.config';
import { type Batch, type Host as Harness, type Installed, type Live, recorded } from '#/session';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTER = 'com.adobe.crashreporter';
const REVEALING = /\.\s*(?<member>revealInSystem|revealInBridge)\s*\(/u;
const SHARED = new URL('../extendscript.jsxinc', import.meta.url);
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
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const [rows, opening] = yield* Effect.all([automation.installed, Effect.flatMap(bundle, (toOpen) => automation.applications({ toOpen }))]);
    return yield* Effect.forEach(
        rows.filter((row) => opening.some((url) => url.href === row.url.href)),
        (row) =>
            Effect.map(
                Effect.all([path.fromFileUrl(row.url), Effect.fromOption(row.version, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'Launch Services reports no version' }, subject: row.name }))]),
                ([app, version]): Installed => ({ identifier: row.identifier, name: row.name, app, version, watched: [row.identifier, REPORTER] }),
            ),
    );
});

// --- [SOURCE]
const engine = (scripts: readonly string[]): Result.Result<typeof Engine.Type, HarnessError> => {
    const engines = Array.dedupe(
        scripts.map((script) =>
            Option.map(
                Record.findFirst(ENGINES, ({ extensions }) => extensions.some((extension) => script.endsWith(`.${extension}`))),
                ([held]) => held,
            ),
        ),
    );
    const unknown = scripts.filter((script) => !Object.values(ENGINES).some(({ extensions }) => extensions.some((extension) => script.endsWith(`.${extension}`))));
    const unavailable = (because: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: scripts.join(', ') });
    return unknown.length > 0
        ? Result.fail(unavailable(`${unknown.join(', ')} holds no extension of ${Record.collect(ENGINES, (held, { extensions }) => `${held} ${extensions.join(' ')}`).join(', ')}`))
        : Array.match(Array.getSomes(engines), { onEmpty: () => Result.succeed<typeof Engine.Type>('extendscript'), onNonEmpty: (held) => (held.length === 1 ? Result.succeed(held[0]) : Result.fail(unavailable(`cases mix engines ${held.join(', ')}`))) });
};
const continued = (code: string): Option.Option<string> =>
    Result.match(
        Result.try(() => parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true })),
        {
            onFailure: (error) => Option.map(Schema.decodeUnknownOption(Thrown)(error), (text) => `source does not parse: ${text}`),
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

// --- [CASES]
const placed = Effect.fn('placed')(function* (decoded: typeof Placed.Type, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const documents = yield* Effect.forEach(
        decoded.documents,
        (document): Effect.Effect<(typeof Placed.Type.documents)[number], PlatformError.PlatformError> =>
            'open' in document ? Effect.as(fs.copyFile(path.join(product, document.open), path.join(folder, path.basename(document.open))), { open: path.join(folder, path.basename(document.open)) }) : Effect.succeed(document),
    );
    return { precondition: { ...decoded, documents }, copies: Array.filterMap(decoded.documents, (document) => ('open' in document ? Result.succeed(path.join(product, document.open)) : Result.failVoid)) };
});

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
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    const scripts = Array.getSomes(batch.cases.map((row) => Option.map(Option.fromUndefinedOr(row._tag === 'Probe' ? undefined : row.source), ({ script }) => script)));
    const held = Option.isNone(batch.survey) ? 'uxpscript' : yield* Effect.fromResult(engine(scripts));
    const encoded = yield* Schema.encodeEffect(Encoded)({ records: batch.records, label: batch.label, enumerations: Option.match(batch.survey, { onNone: () => ({}), onSome: ({ enumerations }) => enumerations }), survey: Option.isNone(batch.survey), cases: batch.cases });
    const sources = yield* Effect.forEach([SHARED, ENGINE], (url) => Effect.flatMap(path.fromFileUrl(url), fs.readFileString));
    const direct = held === 'extendscript' ? sources.join('\n') : { _tag: 'file' as const, url: (yield* bundle).href };
    const { reply } = yield* automation.execute({ pid: live.pid, command: 'do script', arguments: { direct, language: ENGINES[held].language, 'with arguments': [encoded] }, artifacts: [] });
    const answer = yield* Schema.decodeUnknownEffect(Reply)(reply);
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: `${held} batch` }) : Effect.succeed(answer);
});
// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'indesign',
    Engine,
    survey: { question: 'Enumerations, working space profiles, languages, locale, and feature set the installation declares', Schema: Survey, host: (survey) => Option.some(survey.host) },
    Case: HostCase,
    shown: ['alerts', 'activations', 'console'],
    engine,
    precondition,
    placed,
    refusal,
    installed,
    launch: () => Effect.succeed({ fresh: false }),
    opened: () => Effect.succeedNone,
    send,
    records: (_, file) => recorded(Recorded)(file),
    quit: (live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: 'quit', arguments: { saving: { _tag: 'enumerator', name: 'no' } }, artifacts: [] }))),
    digest,
    compare: { _tag: 'Imported' },
} as const satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
