import { createHash } from 'node:crypto';
import { pathToFileURL } from 'node:url';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Effect, FileSystem, Option, Path, type PlatformError, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { HarnessError } from '#/error';
import { ENGINES, Encoded, Engine, HostCase, type Placed, Recorded, Reply, Survey } from '#/indesign/contract';
import { precondition } from '#/indesign/precondition';
import uxpscript from '#/indesign/uxpscript/script.config';
import { type Batch, type Host as Harness, type Installed, type Live, recorded } from '#/session';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTER = 'com.adobe.crashreporter';
const REVEALING = /\.\s*(?<member>revealInSystem|revealInBridge)\s*\(/u;
const CONTINUING: readonly string[] = ['then', 'catch', 'finally'];
const XMP = 'META-INF/metadata.xml';
const SURVEY = 'Enumerations, working space profiles, languages, locale, and feature set the installation declares';
const SHARED = new URL('../extendscript.jsxinc', import.meta.url);
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);

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
const engine = (scripts: readonly string[]): Result.Result<typeof Engine.Type, string> => {
    const engines = Array.dedupe(
        scripts.map((script) =>
            Option.map(
                Record.findFirst(ENGINES, ({ extensions }) => extensions.some((extension) => script.endsWith(`.${extension}`))),
                ([held]) => held,
            ),
        ),
    );
    const unknown = scripts.filter((script) => !Object.values(ENGINES).some(({ extensions }) => extensions.some((extension) => script.endsWith(`.${extension}`))));
    return unknown.length > 0
        ? Result.fail(`${unknown.join(', ')} holds no extension of ${Record.collect(ENGINES, (held, { extensions }) => `${held} ${extensions.join(' ')}`).join(', ')}`)
        : Array.match(Array.getSomes(engines), { onEmpty: () => Result.succeed<typeof Engine.Type>('extendscript'), onNonEmpty: (held) => (held.length === 1 ? Result.succeed(held[0]) : Result.fail(`cases mix engines ${held.join(', ')}`)) });
};
const continued = (code: string): Option.Option<string> =>
    Result.match(
        Result.try(() => parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true })),
        {
            onFailure: (error) => Option.some(`source does not parse: ${String(error)}`),
            onSuccess: (tree) =>
                Option.map(
                    Option.fromNullishOr(findNodeAt(tree, undefined, undefined, (type, node) => type === 'AwaitExpression' || Reflect.get(node, 'async') === true || (type === 'MemberExpression' && CONTINUING.includes(String(Reflect.get(Reflect.get(node, 'property') ?? {}, 'name')))))),
                    ({ node }) => `UXPScript source ${code.slice(node.start, node.end)} continues after the answer`,
                ),
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
    return { precondition: { ...decoded, documents }, copies: decoded.documents.flatMap((document) => ('open' in document ? [path.join(product, document.open)] : [])) };
});

// --- [DIGESTS]
const entryOf = (file: string, entry: string) => Effect.flatMap(ChildProcess.make('unzip', ['-p', file, entry], { stdin: 'ignore', stderr: 'inherit' }), ({ stdout }) => Stream.runCollect(stdout));
const unpacked = Effect.fn('unpacked')(function* (file: string) {
    const listed = yield* ChildProcess.make('unzip', ['-Z1', file], { stdin: 'ignore', stderr: 'inherit' });
    const entries = (yield* Stream.runCollect(Stream.splitLines(Stream.decodeText(listed.stdout)))).filter((entry) => entry !== XMP).toSorted();
    const parts = yield* Effect.forEach(entries, (entry) => Effect.map(entryOf(file, entry), (chunks) => [entry, chunks] as const));
    return parts.reduce((hash, [entry, chunks]) => chunks.reduce((held, chunk) => held.update(chunk), hash.update(entry)), createHash('sha256')).digest('hex');
}, Effect.scoped);
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    return { file, size: bytes.byteLength, sha256: file.endsWith('.idml') ? yield* unpacked(file) : createHash('sha256').update(bytes).digest('hex') };
});

// --- [TRANSPORT]
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    const scripts = batch.cases.flatMap((row) => Option.toArray(Option.map(Option.fromUndefinedOr(row._tag === 'Probe' ? undefined : row.source), ({ script }) => script)));
    const held = Option.isNone(batch.survey) ? 'uxpscript' : yield* Effect.fromResult(Result.mapError(engine(scripts), (because) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: 'batch' })));
    const encoded = yield* Schema.encodeEffect(Encoded)({ records: batch.records, label: batch.label, enumerations: Option.match(batch.survey, { onNone: () => ({}), onSome: ({ enumerations }) => enumerations }), survey: Option.isNone(batch.survey), cases: batch.cases });
    const sources = yield* Effect.forEach([SHARED, ENGINE], (url) => Effect.flatMap(path.fromFileUrl(url), fs.readFileString));
    const direct = held === 'extendscript' ? `${sources.join('\n')}\nharness(arguments[0]);` : { _tag: 'file' as const, url: (yield* bundle).href };
    const { reply } = yield* automation.execute({ pid: live.pid, command: 'do script', arguments: { direct, language: ENGINES[held].language, 'with arguments': [encoded] }, artifacts: [] });
    const answer = yield* Schema.decodeUnknownEffect(Reply)(reply);
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: `${held} batch` }) : Effect.succeed(answer);
});
const quit = (live: Live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: 'quit', arguments: { saving: { _tag: 'enumerator', name: 'no' } }, artifacts: [] })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'indesign',
    Engine,
    survey: { question: SURVEY, Schema: Survey, host: (survey) => Option.some(survey.host) },
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
    quit,
    digest,
    compare: { _tag: 'Imported' },
} satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
