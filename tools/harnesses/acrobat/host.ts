import { createHash } from 'node:crypto';
import { homedir } from 'node:os';
import type { AppleEventValue, Terms } from '@rasm/automation/apple-events';
import { Automation } from '@rasm/automation/automation';
import { transformSync } from '@swc/core';
import { Array, Effect, FileSystem, flow, Option, Path, Record, Result, Schema, SchemaGetter, Stream, Struct, String as Text } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import jsesc from 'jsesc';
import { Answered, BOXES, Engine, fields, HostCase, HostOutcome, Lockdown, Recorded, Request, type Staged, Survey, Surveyed } from '#/acrobat/contract';
import { engine } from '#/acrobat/engine';
import { precondition } from '#/acrobat/precondition';
import { HarnessError } from '#/error';
import { Launched } from '#/instance';
import { copied } from '#/precondition';
import { type Batch, type Decoding, filesIn, type Host as Harness, inside, installation, type Live } from '#/session';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CODES = { suite: 'CARO', script: 'miscdosc', quit: 'aevtquit', menu: 'cmnu', item: 'cmen', title: 'ptit' } as const;
const LOCKDOWN = '/Library/Application Support/Adobe/Acrobat/DC/Preferences/FeatureLockDown';
const SANDBOX = { root: '/private/tmp/com.adobe.Acrobat.tmp.sbx', digits: 16 } as const;
const ELLIPSIS = /(?:…|\.\.\.)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Item = Schema.Struct({ seld: Schema.String }).annotate({ description: 'Menu item specifier, its name as the selector' });
const Modal = Schema.String.pipe(Schema.decodeTo(Schema.Boolean, { decode: SchemaGetter.transform(flow(Text.match(ELLIPSIS), Option.isSome)), encode: SchemaGetter.forbiddenEncoding })).annotate({ description: 'Menu item title ending in an ellipsis, the item opening a dialog' });
const Objects = Schema.fromJsonString(Schema.Struct({ qpdf: Schema.Tuple([Schema.Unknown, Schema.Record(Schema.String, Schema.Json)]) })).annotate({ description: 'qpdf JSON v2 object table' });
const Linked = Schema.Struct({ value: Schema.Struct({ '/Info': Schema.optionalKey(Schema.String), '/Root': Schema.optionalKey(Schema.String), '/Metadata': Schema.optionalKey(Schema.String) }) }).annotate({ description: 'Indirect references a dictionary holds' });
const Dictionary = Schema.Struct({ value: JsonObject });
const Canonical = Schema.fromJsonString(Schema.Array(Schema.Tuple([Schema.String, Schema.Json])));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const declares = ({ suite, command }: Terms) => suite.some(({ code }) => code === CODES.suite) && command.some(({ code }) => code === CODES.script);
const installed = Effect.gen(function* () {
    const automation = yield* Automation;
    const rows = yield* automation.installed;
    const [read, unread] = yield* Effect.partition(
        rows.filter((row) => row.scriptable && Option.isSome(row.scriptingDefinitionFile) && !rows.some((outer) => inside(outer, row))),
        (row) => Effect.mapBoth(automation.terms({ url: row.url }), { onSuccess: flow(Option.liftPredicate(declares), Option.as(row)), onFailure: HarnessError.refused(row.name) }),
        { concurrency: 'unbounded' },
    );
    return yield* Array.match(Array.getSomes(read), {
        onEmpty: () => Array.match(unread, { onEmpty: () => Effect.succeed([]), onNonEmpty: (held) => HarnessError.unavailable('scripting dictionaries')(held.map(Struct.get('message')).join('; ')) }),
        onNonEmpty: (hosts) => Effect.forEach(hosts, (row) => installation(row, rows, [])),
    });
});
const lockdown = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    return (yield* fs.exists(LOCKDOWN))
        ? {
              lockdown: createHash('sha256')
                  .update(yield* fs.readFile(LOCKDOWN))
                  .digest('hex'),
          }
        : {};
});

// --- [CASES]
const placed = Effect.fn('placed')(function* (decoded: typeof Staged.Type, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const sandbox = path.join(SANDBOX.root, `rasm-${createHash('sha256').update(folder).digest('hex').slice(0, SANDBOX.digits)}`);
    yield* fs.remove(sandbox, { recursive: true, force: true });
    yield* fs.makeDirectory(sandbox, { recursive: true });
    const { precondition: copy, copies } = yield* copied(decoded, sandbox, product);
    return { precondition: { ...copy, sandbox }, copies };
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
    const into = (file: string) => path.join(hosted.folder, path.relative(sandbox, file));
    yield* Effect.forEach(Array.difference(skipped)(yield* filesIn(sandbox)), (file) => Effect.andThen(fs.makeDirectory(path.dirname(into(file)), { recursive: true }), fs.copyFile(file, into(file))), { discard: true });
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
    return yield* Schema.encodeEffect(Canonical)(
        Record.keys(kept)
            .toSorted()
            .map((key) => [key, kept[key] ?? null] as const),
    );
});

// --- [TRANSPORT]
const specifier = (want: string, form: 'property' | 'index', seld: AppleEventValue, from: Option.Option<AppleEventValue>): AppleEventValue => ({ _tag: 'specifier', want, form, seld, ...Option.match(from, { onNone: () => ({}), onSome: (held) => ({ from: held }) }) });
const lowered = Effect.try({ try: () => transformSync(`var engine = ${engine.toString()};`, { jsc: { target: 'es5', parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } }, isModule: false }).code, catch: HarnessError.refused('engine lowering') });
const based = () => ({ home: homedir(), boxes: BOXES });
const got = Effect.fn('got')(function* <S extends Schema.Top & Schema.Decoder<unknown>>(pid: number, direct: AppleEventValue, schema: S) {
    const automation = yield* Automation;
    const { reply } = yield* automation.execute({ pid, command: 'get', arguments: { direct } });
    return yield* Schema.decodeEffect(Schema.NullOr(schema))(reply);
});
const menu = Effect.fn('menu')(function* (pid: number, index: number) {
    const items = specifier(CODES.item, 'index', 'every', Option.some(specifier(CODES.menu, 'index', index, Option.none())));
    const [named, modal] = yield* Effect.all([got(pid, items, Schema.Array(Item)), got(pid, specifier(CODES.title, 'property', { _tag: 'type', name: CODES.title }, Option.some(items)), Schema.Array(Schema.NullOr(Modal)))]);
    return Option.map(
        Option.fromNullOr(named),
        Array.filterMap(({ seld }, at) => (modal?.[at] === true ? Result.succeed(seld) : Result.failVoid)),
    );
});
const modalIn = (pid: number) => Effect.map(Stream.runCollect(Stream.paginate(1, (index) => Effect.map(menu(pid, index), (names) => [Option.toArray(names).flat(), Option.as(names, index + 1)] as const))), (pages) => pages.flat());
const answered = Effect.fn('answered')(function* <A>(pid: number, Reply: Decoding<Result.Result<A, { readonly reason: string }>>, request: typeof Request.Type) {
    const [automation, code] = yield* Effect.all([Automation, lowered]);
    const program = `(function (request) {\n${code}\nreturn engine.call(this, request);\n}).call(this, ${jsesc(yield* Schema.encodeEffect(Request)(request), { json: true })});`;
    const direct = `try { eval(${jsesc(program, { json: true })}); } catch (error) { encodeURIComponent(JSON.stringify({ _tag: 'Failure', failure: { reason: String(error && error.name) + ': ' + String(error && error.message) + ' at line ' + String(error && error.lineNumber) } })); }`;
    const { reply } = yield* automation.script({ pid, direct });
    return yield* Effect.mapError(Schema.decodeEffect(Reply)(reply), (error) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `reply ${typeof reply} ${String(reply)}` }, subject: `process ${pid}`, cause: error }));
});
const surveyed = Effect.fn('surveyed')(function* (pid: number, opened: Option.Option<Schema.Json>) {
    const modal = yield* modalIn(pid);
    const held = yield* answered(pid, Surveyed, Request.cases.Survey.make({ ...based(), modal }));
    const { viewer, model } = yield* Effect.mapError(Effect.fromResult(held), ({ reason }) => HarnessError.unavailable(`process ${pid}`)(reason));
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
        yield* line({ _tag: 'Finished', outcome: Result.match(answer, { onSuccess: ({ outcome }) => outcome, onFailure: ({ reason }) => HostOutcome.make({ name: hosted.name, completion: { _tag: 'Failed', reason } }) }) });
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
    Engine,
    survey: { question: 'Member names per object kind, signatures of answered members, dialog menu items, viewer, and FeatureLockDown digest the installation declares', Schema: Survey },
    Case: HostCase,
    fields,
    engine: () => Result.succeed(Engine.literal),
    precondition,
    placed,
    refusal: (_, hosted) => (hosted.dialogs === undefined ? Option.none<string>() : Option.some('execDialog answers through windows presses')),
    installed,
    launch: () => Effect.succeed(new Launched({})),
    opened: () => lockdown,
    send,
    quit: (live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: CODES.quit, arguments: { saving: { _tag: 'enumerator', name: 'no' } } }))),
    canonical: (file, bytes) => (file.endsWith('.pdf') ? Effect.map(normalized(file), (text) => [text]) : Effect.succeed([bytes])),
} as const satisfies Harness<typeof Survey.Type, typeof HostCase.Type, typeof Engine.Type, typeof Staged.Type, typeof fields>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
