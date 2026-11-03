import { createHash } from 'node:crypto';
import { homedir } from 'node:os';
import type { AppleEventValue } from '@rasm/automation/apple-events';
import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { transformSync } from '@swc/core';
import { Array, Effect, FileSystem, flow, Option, Path, type PlatformError, Record, Result, Schema, SchemaGetter, Stream, String as Text } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import jsesc from 'jsesc';
import { Answered, BOXES, Engine, HostCase, Lockdown, Outcome, type Placed, Recorded, Request, type Staged, Survey, Surveyed } from '#/acrobat/contract';
import { engine } from '#/acrobat/engine';
import { precondition } from '#/acrobat/precondition';
import { HarnessError } from '#/error';
import { type Batch, type Decoding, type Host as Harness, type Installed, type Live, recorded } from '#/session';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CODES = { suite: 'CARO', script: 'miscdosc', quit: 'aevtquit', menu: 'cmnu', item: 'cmen', title: 'ptit' } as const;
const LOCKDOWN = '/Library/Application Support/Adobe/Acrobat/DC/Preferences/FeatureLockDown';
const ELLIPSIS = /(?:…|\.\.\.)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Program = Schema.fromJsonString(Schema.String).annotate({ description: 'Lowered engine as the string literal the do script eval compiles, a syntax error answering as a failure' });
const Item = Schema.Struct({ seld: Schema.String }).annotate({ description: 'Menu item specifier, its name as the selector' });
const Modal = Schema.String.pipe(Schema.decodeTo(Schema.Boolean, { decode: SchemaGetter.transform(flow(Text.match(ELLIPSIS), Option.isSome)), encode: SchemaGetter.forbiddenEncoding })).annotate({ description: 'Menu item title ending in an ellipsis, the item opening a dialog' });
const Objects = Schema.fromJsonString(Schema.Struct({ qpdf: Schema.Tuple([Schema.Unknown, Schema.Record(Schema.String, Schema.Json)]) })).annotate({ description: 'qpdf JSON v2 object table' });
const Linked = Schema.Struct({ value: Schema.Struct({ '/Info': Schema.optionalKey(Schema.String), '/Root': Schema.optionalKey(Schema.String), '/Metadata': Schema.optionalKey(Schema.String) }) }).annotate({ description: 'Indirect references a dictionary holds' });
const Dictionary = Schema.Struct({ value: JsonObject });
const Canonical = Schema.fromJsonString(Schema.Array(Schema.Tuple([Schema.String, Schema.Json])));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const inside = (outer: Installation, inner: Installation) => inner.url.href !== outer.url.href && inner.url.href.startsWith(outer.url.href);
const installation = (rows: readonly Installation[]) =>
    Effect.fn('installation')(function* (row: Installation) {
        const path = yield* Path.Path;
        const app = yield* path.fromFileUrl(row.url);
        const version = yield* Effect.fromOption(row.version, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'Launch Services reports no version' }, subject: row.name }));
        const helpers = Array.dedupe(rows.filter((inner) => inside(row, inner)).map(({ identifier }) => identifier)).filter((identifier) => identifier !== row.identifier);
        return { identifier: row.identifier, name: row.name, app, version, watched: [row.identifier, ...helpers] } satisfies Installed;
    });
const installed = Effect.gen(function* () {
    const automation = yield* Automation;
    const rows = yield* automation.installed;
    const declaring = (row: Installation) =>
        Effect.map(
            Effect.option(automation.terms({ url: row.url })),
            Option.exists(({ suite, command }) => suite.some(({ code }) => code === CODES.suite) && command.some(({ code }) => code === CODES.script)),
        );
    const hosts = yield* Effect.filter(
        rows.filter((row) => row.scriptable && !rows.some((outer) => inside(outer, row))),
        declaring,
        { concurrency: 'unbounded' },
    );
    return yield* Effect.forEach(hosts, installation(rows));
});
const lockdown = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const digest = (yield* fs.exists(LOCKDOWN))
        ? Option.some(
              createHash('sha256')
                  .update(yield* fs.readFile(LOCKDOWN))
                  .digest('hex'),
          )
        : Option.none<string>();
    return Option.some<typeof Lockdown.Type>(Option.match(digest, { onNone: () => ({}), onSome: (held) => ({ lockdown: held }) }));
});

// --- [CASES]
const placed = Effect.fn('placed')(function* (decoded: typeof Staged.Type, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const sandbox = path.join('/private/tmp/com.adobe.Acrobat.tmp.sbx', folder);
    yield* fs.remove(sandbox, { recursive: true, force: true });
    yield* fs.makeDirectory(sandbox, { recursive: true });
    const documents = yield* Effect.forEach(
        decoded.documents,
        (document): Effect.Effect<(typeof Placed.Type.documents)[number], PlatformError.PlatformError> =>
            'open' in document ? Effect.as(fs.copyFile(path.join(product, document.open), path.join(sandbox, path.basename(document.open))), { open: path.join(sandbox, path.basename(document.open)) }) : Effect.succeed(document),
    );
    return { precondition: { ...decoded, documents, sandbox }, copies: decoded.documents.flatMap((document) => ('open' in document ? [path.join(product, document.open)] : [])) };
});
const staged = Effect.fn('staged')(function* (hosted: typeof HostCase.Type) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const into = (file: string) => path.join(hosted.precondition.sandbox, path.basename(file));
    const files = yield* Effect.forEach(hosted.files ?? [], (file) => (file === null ? Effect.succeed(null) : Effect.as(fs.copyFile(file, into(file)), into(file))));
    return { hosted: hosted.files === undefined ? hosted : { ...hosted, files }, inputs: files.filter((file) => file !== null) };
});
const collected = Effect.fn('collected')(function* (hosted: typeof HostCase.Type, skipped: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { sandbox } = hosted.precondition;
    const isFile = (file: string) => Effect.map(fs.stat(file), ({ type }) => type === 'File');
    const listed = (yield* fs.readDirectory(sandbox, { recursive: true })).map((file) => path.join(sandbox, file));
    const files = yield* Effect.filter(Array.difference(skipped)(listed), isFile);
    yield* Effect.forEach(files, (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(hosted.folder, path.relative(sandbox, file))), { recursive: true }), fs.copyFile(file, path.join(hosted.folder, path.relative(sandbox, file)))), { discard: true });
    yield* fs.remove(sandbox, { recursive: true, force: true });
});

// --- [DIGESTS]
const normalized = Effect.fn('normalized')(function* (file: string) {
    const printed = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.string(ChildProcess.make('qpdf', ['--json-output', '--json-stream-data=inline', '--decode-level=generalized', file, '-'])));
    const [, table] = (yield* Schema.decodeEffect(Objects)(printed)).qpdf;
    const linked = (key: string, link: '/Info' | '/Root' | '/Metadata') => Option.flatMap(Schema.decodeUnknownOption(Linked)(table[key]), ({ value }) => Option.map(Option.fromUndefinedOr(value[link]), (held) => `obj:${held}`));
    const info = linked('trailer', '/Info');
    const metadata = Option.flatMap(linked('trailer', '/Root'), (catalog) => linked(catalog, '/Metadata'));
    const dropped: Record.ReadonlyRecord<string, readonly string[]> = { trailer: ['/ID'], ...Option.match(info, { onNone: () => ({}), onSome: (key) => ({ [key]: ['/CreationDate', '/ModDate'] }) }) };
    const pruned = (entry: Schema.Json, names: readonly string[]): Schema.Json => Option.match(Schema.decodeUnknownOption(Dictionary)(entry), { onNone: () => entry, onSome: ({ value }) => ({ value: Record.filter(value, (_, name) => !names.includes(name)) }) });
    const kept = Record.filter(
        Record.map(table, (entry, key) => Option.match(Record.get(dropped, key), { onNone: () => entry, onSome: (names) => pruned(entry, names) })),
        (_, key) => !Option.contains(metadata, key),
    );
    const canonical = yield* Schema.encodeEffect(Canonical)(
        Record.keys(kept)
            .toSorted()
            .map((key) => [key, kept[key] ?? null] as const),
    );
    return createHash('sha256').update(canonical).digest('hex');
});
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    return { file, size: bytes.byteLength, sha256: file.endsWith('.pdf') ? yield* normalized(file) : createHash('sha256').update(bytes).digest('hex') };
});

// --- [TRANSPORT]
const specifier = (want: string, form: 'property' | 'index', seld: AppleEventValue, from: Option.Option<AppleEventValue>): AppleEventValue => ({ _tag: 'specifier', want, form, seld, ...Option.match(from, { onNone: () => ({}), onSome: (held) => ({ from: held }) }) });
const lowered = Effect.try({ try: () => transformSync(`var engine = ${engine.toString()};`, { jsc: { target: 'es5', parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } }, isModule: false }).code, catch: HarnessError.refused('engine lowering') });
const based = () => ({ home: homedir(), boxes: BOXES });
const got = Effect.fn('got')(function* <S extends Schema.Top & Schema.Decoder<unknown>>(pid: number, direct: AppleEventValue, schema: S) {
    const automation = yield* Automation;
    const { reply } = yield* automation.execute({ pid, command: 'get', arguments: { direct }, artifacts: [] });
    return yield* Schema.decodeEffect(Schema.NullOr(schema))(reply);
});
const menu = Effect.fn('menu')(function* (pid: number, index: number) {
    const items = specifier(CODES.item, 'index', 'every', Option.some(specifier(CODES.menu, 'index', index, Option.none())));
    const [named, modal] = yield* Effect.all([got(pid, items, Schema.Array(Item)), got(pid, specifier(CODES.title, 'property', { _tag: 'type', name: CODES.title }, Option.some(items)), Schema.Array(Schema.NullOr(Modal)))]);
    return Option.map(Option.fromNullOr(named), (found) => found.flatMap(({ seld }, at) => (modal?.[at] === true ? [seld] : [])));
});
const modalIn = (pid: number) => Effect.map(Stream.runCollect(Stream.paginate(1, (index) => Effect.map(menu(pid, index), (names) => [Option.toArray(names).flat(), Option.as(names, index + 1)] as const))), (pages) => pages.flat());
const answered = Effect.fn('answered')(function* <A>(pid: number, Reply: Decoding<Result.Result<A, { readonly reason: string }>>, request: typeof Request.Type) {
    const [automation, code] = yield* Effect.all([Automation, lowered]);
    const program = `(function (request) {\n${code}\nreturn engine.call(this, request);\n}).call(this, ${jsesc(yield* Schema.encodeEffect(Request)(request), { json: true })});`;
    const direct = `try { eval(${yield* Schema.encodeEffect(Program)(program)}); } catch (error) { encodeURIComponent(JSON.stringify({ _tag: 'Failure', failure: { reason: String(error && error.name) + ': ' + String(error && error.message) + ' at line ' + String(error && error.lineNumber) } })); }`;
    const { reply } = yield* automation.execute({ pid, command: CODES.script, arguments: { direct }, artifacts: [] });
    return yield* Effect.mapError(Schema.decodeEffect(Reply)(reply), (error) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `reply ${typeof reply} ${String(reply)}` }, subject: `process ${pid}`, cause: error }));
});
const surveyed = Effect.fn('surveyed')(function* (pid: number, opened: Option.Option<Schema.Json>) {
    const modal = yield* modalIn(pid);
    const held = yield* answered(pid, Surveyed, Request.cases.Survey.make({ ...based(), modal }));
    const { viewer, model } = yield* Effect.mapError(Effect.fromResult(held), ({ reason }) => HarnessError.make({ reason: { _tag: 'Unavailable', because: reason }, subject: `process ${pid}` }));
    const host = yield* Effect.orDie(Schema.decodeUnknownEffect(Lockdown)(Option.getOrUndefined(opened)));
    return { model: { ...model, modal }, viewer, host } satisfies typeof Survey.Type;
});
const cased = (pid: number, records: string, survey: typeof Survey.Type) =>
    Effect.fn('cased')(function* (hosted: typeof HostCase.Type, index: number) {
        const fs = yield* FileSystem.FileSystem;
        const line = (entry: typeof Recorded.Type) => Effect.flatMap(Schema.encodeEffect(Recorded)(entry), (encoded) => fs.writeFileString(records, `${encoded}\n`, { flag: 'a' }));
        yield* line({ _tag: 'Started', index });
        const { hosted: held, inputs } = yield* staged(hosted);
        const answer = yield* answered(pid, Answered, Request.cases.Case.make({ ...based(), modal: survey.model.modal, signatures: survey.model.signatures, case: held }));
        yield* collected(hosted, [...inputs, ...Result.match(answer, { onSuccess: ({ kept }) => kept, onFailure: () => [] })]);
        yield* line({ _tag: 'Finished', outcome: Result.match(answer, { onSuccess: ({ outcome }) => outcome, onFailure: ({ reason }) => Outcome.make({ name: hosted.name, completion: { _tag: 'Failed', reason } }) }) });
    });
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const header = { application: live.chosen.name, version: live.chosen.version };
    return yield* Option.match(batch.survey, {
        onNone: () => Effect.map(surveyed(live.pid, batch.opened), (survey) => ({ ...header, survey })),
        onSome: (survey) => Effect.as(Effect.forEach(batch.cases, cased(live.pid, batch.records, survey), { discard: true }), header),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'acrobat',
    Engine,
    survey: { question: 'Member names per object kind, signatures of answered members, dialog menu items, viewer, and FeatureLockDown digest the installation declares', Schema: Survey, host: (survey) => Option.some(survey.host) },
    Case: HostCase,
    shown: ['alerts', 'confirms', 'prompts', 'files', 'windows', 'activations', 'writes', 'events', 'console'],
    engine: () => Result.succeed(Engine.literal),
    precondition,
    placed,
    refusal: (_, hosted) => (hosted.dialogs === undefined ? Option.none<string>() : Option.some('execDialog answers through windows presses')),
    installed,
    launch: () => Effect.succeed({ fresh: true }),
    opened: () => lockdown,
    send,
    records: (_, file) => recorded(Recorded)(file),
    quit: (live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: CODES.quit, arguments: { saving: { _tag: 'enumerator', name: 'no' } }, artifacts: [] }))),
    digest,
    compare: { _tag: 'Imported' },
} as const satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Staged.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
