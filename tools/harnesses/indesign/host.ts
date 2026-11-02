import { createHash } from 'node:crypto';
import { pathToFileURL } from 'node:url';
import { type Installation, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Data, Deferred, Duration, Effect, FileSystem, Match, Number, Option, Order, Path, type PlatformError, Record, Ref, Result, Schema, Stream, String as Text, UndefinedOr } from 'effect';
import { ChildProcess } from 'effect/process';
import { Batch, Call, Probe, Vector } from '#/cases';
import { HarnessError } from '#/error';
import { ENGINES, Encoded, Engine, fields, Header, HostCase, Outcome, Recorded, Reply, Survey } from '#/indesign/contract';
import { engineOf, merged, type precondition } from '#/indesign/precondition';
import uxpscript from '#/indesign/uxpscript/script.config';
import { crashed, type Exited, Launch, lock, owner, sampled, settled, watch } from '#/instance';
import { type Answered, type Hang, results } from '#/results';
import { bury } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Control = NonNullable<Element['detail']>['controls'][number];
type Row = (typeof Batch.Type.cases)[number];
type Note = readonly [Option.Option<number>, typeof Answered.Type];
type Verdict = Data.TaggedEnum<{ Exited: { readonly exit: Exited }; Hung: { readonly index: Option.Option<number>; readonly hang: typeof Hang.Type }; Lost: { readonly error: HarnessError | Schema.SchemaError | PlatformError.PlatformError } }>;
interface Mapped {
    readonly file: string;
    readonly line: number;
}
interface Prepared {
    readonly hosted: typeof HostCase.Type;
    readonly row: Row;
    readonly map: readonly Mapped[];
    readonly presses: readonly string[];
}
interface Plan {
    readonly engine: typeof Engine.Type;
    readonly sendable: readonly Prepared[];
    readonly settled: readonly (typeof Outcome.Type)[];
}
interface Session {
    readonly pid: number;
    readonly launched: boolean;
    readonly app: string;
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly records: Ref.Ref<Option.Option<string>>;
    readonly presses: Ref.Ref<readonly (readonly string[])[]>;
    readonly usage: Ref.Ref<typeof State.Type>;
    readonly log: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTER = 'com.adobe.crashreporter';
const RECORD = /^(?<index>\d+)\.(?<kind>json|started)$/u;
const JSXBIN = /(?<quote>["'])(?<literal>@JSXBIN@(?:\\.|(?!\k<quote>)[^\\])*)\k<quote>/su;
const BYTE_ORDER_MARK = /^﻿/u;
const CONTINUATION = /\\\r?\n/gu;
const ESCAPE = /\\(?<character>.)/gu;
const INCLUDE = /^[ \t]*(?:\/\/[ \t]*@|#)include[ \t]+["'](?<name>[^"']+)["'][ \t;]*$/u;
const INCLUDE_PATH = /^[ \t]*(?:\/\/[ \t]*@|#)includepath[ \t]+["'](?<paths>[^"']*)["']/mu;
const DIRECTIVE = /^[ \t]*#(?:target|targetengine|strict|script)\b/u;
const REVEALING = /\.\s*(?<member>revealInSystem|revealInBridge)\s*\(/u;
const CONTINUING: readonly string[] = ['then', 'catch', 'finally'];
const LOG = /^n(?<file>.*\/UXPLogs_[^/]*\.log)$/u;
const VERSIONS = new Intl.Collator('en', { numeric: true });
const PRESSED_FIRST = ['AXCancelButton', 'CancelButton', 'AXDefaultButton', 'OKButton'] as const;
const DISMISS = 'Ignore';
const XMP = 'META-INF/metadata.xml';

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const Results = results(Engine, { state: Schema.optionalKey(Header.fields.state), host: Survey.fields.host, shared: Schema.Boolean.annotate({ description: 'Instance another process launched, left as it was' }) }, fields);
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        (row: Installation) => Option.getOrElse(row.version, () => ''),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const bundle = Effect.map(
    Effect.fromOption(Option.fromUndefinedOr(uxpscript.build?.outDir), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'script config states no build.outDir' }, subject: '@rasm/harnesses-indesign-uxpscript' })),
    (folder) => pathToFileURL(`${folder}/main.${ENGINES.uxpscript.extensions[0]}`),
);
const prefixed = (versions: readonly string[]) => (held: string) => versions.some((prefix) => held.startsWith(prefix));
const installations = Effect.fn('installations')(function* (versions: readonly string[]) {
    const automation = yield* Automation;
    const [rows, opening] = yield* Effect.all([automation.installed, Effect.flatMap(bundle, (toOpen) => automation.applications({ toOpen }))]);
    const hosts = Array.sort(
        rows.filter((row) => opening.some((url) => url.href === row.url.href)),
        newest,
    );
    const listed = hosts.map((row) => Option.getOrElse(row.version, () => row.name)).join(', ');
    const chosen = versions.length === 0 ? hosts : hosts.filter((row) => Option.exists(row.version, prefixed(versions)));
    return yield* Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installed versions ${listed}` }, subject: `InDesign ${versions.join(', ')}` }));
});

// --- [SOURCE]
const prepared = Effect.fn('prepared')(function* (script: string, lines: Option.Option<typeof Call.Type.lines & {}>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = (yield* fs.readFileString(script)).replace(BYTE_ORDER_MARK, '');
    const binary = Option.map(Option.fromNullishOr(JSXBIN.exec(text)?.groups?.literal), (literal) => literal.replace(CONTINUATION, '').replace(ESCAPE, '$<character>'));
    const located = (file: string, searched: readonly string[], name: string, chain: readonly string[]) =>
        Effect.gen(function* () {
            const found = yield* Effect.findFirst(
                searched.map((entry): string => path.resolve(entry, name)),
                (candidate: string) => fs.exists(candidate),
            );
            const included = yield* Effect.fromOption(found, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `include ${name} found in none of ${searched.join(', ')}` }, subject: file }));
            return chain.includes(included) ? yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `include cycle ${[...chain, included].join(' > ')}` }, subject: file }) : included;
        });
    const expanded = (file: string, body: string, chain: readonly string[]): Effect.Effect<readonly (readonly [string, Mapped])[], HarnessError | PlatformError.PlatformError> =>
        Effect.gen(function* () {
            const folder = path.dirname(file);
            const searched = [
                folder,
                ...(INCLUDE_PATH.exec(body)?.groups?.paths ?? '')
                    .split(';')
                    .filter((entry) => entry !== '')
                    .map((entry) => path.resolve(folder, entry)),
            ];
            const nested = (included: string) => Effect.flatMap(fs.readFileString(included), (source) => expanded(included, source.replace(BYTE_ORDER_MARK, ''), [...chain, included]));
            const lined = (source: string, index: number) =>
                Option.match(Option.fromNullishOr(INCLUDE.exec(source)?.groups?.name), {
                    onNone: () => Effect.succeed([[DIRECTIVE.test(source) ? '' : source, { file, line: index + 1 }] as const]),
                    onSome: (name) => Effect.flatMap(located(file, searched, name, chain), nested),
                });
            return (yield* Effect.forEach(Array.fromIterable(Text.linesIterator(body)), lined)).flat();
        });
    const rows = Option.isSome(binary) ? [[binary.value, { file: script, line: 1 }] as const] : yield* expanded(script, text, [script]);
    const kept = Option.match(lines, { onNone: () => rows, onSome: (ranges) => ranges.flatMap(([from, to]) => rows.slice(from - 1, to)) });
    return { code: kept.map(([line]) => line).join('\n'), map: kept.map(([, at]) => at), binary: Option.isSome(binary) };
});
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
const refusal = (engine: typeof Engine.Type, hosted: typeof HostCase.Type, binary: boolean) => {
    const { set = [], add = [], select = [], steps = [] } = hosted.precondition;
    const expressions = [...Object.values(hosted.read), ...set.map(([target]) => target), ...add.map(({ to }) => to), ...select, ...HostCase.match(hosted, { Probe: ({ evaluate }) => [evaluate], Call: ({ call }) => [call], Run: () => [] })];
    const statements = [...steps, ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(hosted._tag === 'Probe' ? undefined : hosted.source), ({ code }) => Option.liftPredicate(code, () => !binary)))];
    return engine === 'uxpscript'
        ? Option.firstSomeOf([...expressions.map((expression) => continued(`(${expression.replaceAll('[*]', '[0]')});`)), ...statements.map(continued)])
        : Option.firstSomeOf([...expressions, ...statements].map((text) => Option.map(Option.fromNullishOr(REVEALING.exec(text)?.groups?.member), (member) => `ExtendScript ${member} activates Finder or Bridge`)));
};

// --- [CASES]
const vectorCalls =
    ({ calls, script, lines }: NonNullable<typeof Batch.Type.vector>) =>
    (rows: typeof Vector.Type.cases) =>
        Array.getSomes(rows.map((row) => Option.map(Record.get(calls, row.fn), (original) => ({ name: row.name, fn: row.fn, call: original, args: row.args, ...(script === undefined ? {} : { script }), ...(lines === undefined ? {} : { lines }) }))));
const expand = Effect.fn('expand')(function* (product: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const batch = yield* Schema.decodeEffect(Schema.fromJsonString(Batch))(yield* fs.readFileString(file));
    const vectored: readonly (typeof Call.Type)[] = yield* UndefinedOr.match(batch.vector, {
        onUndefined: () => Effect.succeed([]),
        onDefined: (vector) =>
            fs.readFileString(path.join(product, vector.file)).pipe(
                Effect.flatMap(Schema.decodeEffect(Vector)),
                Effect.map(({ cases: rows }) => vectorCalls(vector)(rows)),
            ),
    });
    return { batch, rows: [...batch.cases, ...vectored] };
});
const conditionOf = (batch: Pick<typeof Batch.Type, 'precondition'>, row: Row) => merged(batch.precondition ?? {}, row.precondition ?? {});
const scriptOf = (batch: Pick<typeof Batch.Type, 'script'>, row: Row) => Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : (row.script ?? batch.script));
const engineFor = (batch: Pick<typeof Batch.Type, 'script' | 'precondition'>, row: Row): Result.Result<Option.Option<typeof Engine.Type>, string> =>
    Option.match(engineOf(conditionOf(batch, row)), {
        onSome: (engine) => Result.succeed(Option.some(engine)),
        onNone: () =>
            Option.match(scriptOf(batch, row), {
                onNone: () => Result.succeed(Option.none()),
                onSome: (script) =>
                    Result.fromOption(
                        Option.map(
                            Record.findFirst(ENGINES, ({ extensions }) => extensions.some((extension) => script.endsWith(`.${extension}`))),
                            ([engine]) => Option.some(engine),
                        ),
                        () => `${script} holds no extension of ${Record.collect(ENGINES, (engine, { extensions }) => `${engine} ${extensions.join(' ')}`).join(', ')}`,
                    ),
            }),
    });
const hostCase = (product: string, captures: string, batch: Pick<typeof Batch.Type, 'script' | 'precondition' | 'read'>, Precondition: ReturnType<typeof precondition>, engine: typeof Engine.Type) => (row: Row, index: number) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(captures, String(index));
        yield* fs.makeDirectory(folder, { recursive: true });
        const placed = yield* Schema.decodeUnknownEffect(Precondition)(conditionOf(batch, row));
        const documents = yield* Effect.forEach(
            placed.documents,
            (document): Effect.Effect<(typeof placed.documents)[number], PlatformError.PlatformError> =>
                'open' in document ? Effect.as(fs.copyFile(path.join(product, document.open), path.join(folder, path.basename(document.open))), { open: path.join(folder, path.basename(document.open)) }) : Effect.succeed(document),
        );
        const shared = { name: row.name, precondition: { ...placed, documents }, read: { ...batch.read, ...row.read }, folder, ...Record.filter({ dialogs: row.dialogs, windows: row.windows, confirms: row.confirms, prompts: row.prompts, files: row.files }, (value) => value !== undefined) };
        const loaded = yield* Option.match(scriptOf(batch, row), {
            onNone: () => Effect.succeedNone,
            onSome: (script) => Effect.asSome(Effect.map(prepared(path.join(product, script), Schema.is(Call)(row) ? Option.fromUndefinedOr(row.lines) : Option.none()), (held) => ({ ...held, source: { script: path.join(product, script), code: held.code } }))),
        });
        const { map, binary } = Option.getOrElse(loaded, () => ({ map: [], binary: false }));
        const args = (Schema.is(Probe)(row) ? [] : (row.args ?? [])).map((arg) => bury(arg));
        const source = Option.map(loaded, (held) => held.source);
        const hosted = yield* Match.value(row).pipe(
            Match.when(Schema.is(Probe), ({ evaluate }) => Effect.succeed(HostCase.cases.Probe.make({ ...shared, evaluate }))),
            Match.when(Schema.is(Call), ({ call }) => Effect.succeed(HostCase.cases.Call.make({ ...shared, ...Option.match(source, { onNone: () => ({}), onSome: (held) => ({ source: held }) }), call, args }))),
            Match.orElse(() =>
                Effect.map(
                    Effect.fromOption(source, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no script for the run case or batch' }, subject: row.name })),
                    (held) => HostCase.cases.Run.make({ ...shared, source: held, args }),
                ),
            ),
        );
        const made = { hosted, map, binary };
        return yield* Option.match(refusal(engine, made.hosted, made.binary), {
            onNone: () => Effect.succeed(Result.succeed<Prepared>({ hosted: made.hosted, row, map: made.map, presses: row.presses ?? [] })),
            onSome: (reason) => Effect.succeed(Result.fail(Outcome.make({ name: row.name, completion: { _tag: 'Failed', reason } }))),
        });
    }).pipe(Effect.catchTags({ SchemaError: (error) => Effect.succeed(Result.fail(Outcome.make({ name: row.name, completion: { _tag: 'Failed', reason: error.message } }))), HarnessError: (error) => Effect.succeed(Result.fail(Outcome.make({ name: row.name, completion: { _tag: 'Failed', reason: error.message } }))) }));
const normalize = Effect.fn('normalize')(function* (product: string, file: string, captures: string, only: readonly string[], Precondition: ReturnType<typeof precondition>) {
    const fs = yield* FileSystem.FileSystem;
    const { batch, rows } = yield* expand(product, file);
    yield* Schema.decodeEffect(Schema.Array(Schema.Literals(rows.map(({ name }) => name))))(only);
    const kept = rows.filter(({ name }) => only.length === 0 || only.includes(name));
    const engines = Array.dedupe(Array.getSomes(Array.getSuccesses(kept.map((row) => engineFor(batch, row)))));
    const engine = yield* Effect.fromOption(
        Option.filter(
            Option.orElse(Array.head(engines), () => Option.some('extendscript' as const)),
            () => engines.length <= 1,
        ),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `cases mix engines ${engines.join(', ')}` }, subject: file }),
    );
    yield* Effect.ignore(fs.remove(captures, { recursive: true }));
    const made = yield* Effect.forEach(kept, (row, index) =>
        Result.match(engineFor(batch, row), {
            onFailure: (reason) => Effect.succeed(Result.fail(Outcome.make({ name: row.name, completion: { _tag: 'Failed', reason } }))),
            onSuccess: () => hostCase(product, captures, batch, Precondition, engine)(row, index),
        }),
    );
    return { batch, rows: kept, engine, sendable: Array.getSuccesses(made), settled: Array.getFailures(made) };
});

// --- [RECORDS]
const recordOf = (file: string) => Option.flatMap(Option.fromNullishOr(RECORD.exec(file)?.groups), ({ index = '', kind = '' }) => Option.map(Number.parse(index), (at) => ({ file, at, kind })));
const recorded = Effect.fn('recorded')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = Array.getSomes((yield* fs.readDirectory(folder)).map(recordOf));
    const finished = yield* Effect.forEach(
        entries.filter(({ kind }) => kind === 'json'),
        ({ file, at }) => Effect.map(Effect.flatMap(fs.readFileString(path.join(folder, file)), Schema.decodeEffect(Recorded)), (row) => [at, row] as const),
    );
    const done = new Map(finished);
    return {
        finished: done,
        running: Array.findFirst(
            Array.sort(
                entries.map(({ at }) => at),
                Order.flip(Number.Order),
            ),
            (at) => !done.has(at),
        ),
    };
});
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
const writtenIn = Effect.fn('writtenIn')(function* (folder: string, copies: ReadonlySet<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = yield* Effect.filter(
        (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
        (file) => Effect.map(fs.stat(file), ({ type }) => type === 'File'),
    );
    return (yield* Effect.forEach(files, digest)).filter(({ sha256 }) => !copies.has(sha256));
});
const mapped = (map: readonly Mapped[]) => (row: typeof Outcome.Type) => {
    const at = (line: number) => Option.match(Array.get(map, line - 1), { onNone: () => ({ line }), onSome: (held) => held });
    return row.completion._tag === 'Thrown' ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at(row.completion.line)), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(({ line }) => at(line)) }) } } : row;
};

// --- [WATCHER]
const described = (state: typeof State.Type) =>
    State.match(state, {
        Launching: () => 'launching',
        Responding: () => 'responding',
        Unresponsive: ({ process, usage }) => `unresponsive ${process}${usage === undefined ? '' : ` cpu ${usage.cpu} s footprint ${usage.footprint} B`}`,
        Failed: ({ reason }) => `status failed: ${reason}`,
        Exited: ({ signal, status, report, reportFailure }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url.href}`}${reportFailure === undefined ? '' : `, ${reportFailure}`}`,
    });
const verdictText = Verdict.$match({ Exited: ({ exit }) => described(exit), Hung: ({ hang }) => `hung ${hang.cpu}, ${hang.sample}`, Lost: ({ error }) => error.message });
const finishedIn = (rest: readonly Prepared[], ran: Effect.Success<ReturnType<typeof batchIn>>) => {
    const on = (index: number) => ran.notes.filter(([at]) => Option.contains(at, index)).map(([, note]) => note);
    const finished = (index: number) => Option.fromUndefinedOr(ran.finished.get(index));
    const kept = rest.flatMap(({ map }, index) => Option.toArray(Option.map(finished(index), (row) => ({ ...mapped(map)(row), answered: [...row.answered, ...on(index)], logs: ran.logs(index) }))));
    const ended = Option.getOrElse(ran.running, () => kept.length);
    const failure = Option.zipWith(Result.getFailure(ran.reply), Array.get(rest, ended), (error, { hosted }) => ({ name: hosted.name, message: error.message }));
    const remaining = Result.isFailure(ran.reply) ? rest.slice(ended + 1) : [];
    const relaunch = Option.exists(ran.verdict, Verdict.$is('Exited')) && remaining.length > 0;
    const unsent = relaunch ? [] : remaining.map(({ hosted }) => Outcome.make({ name: hosted.name, completion: { _tag: 'Failed', reason: 'batch ended before the case ran' } }));
    return { on, kept, ended, failure, remaining, relaunch, unsent };
};
const fault = (limit: Duration.Duration, message: string, verdict: Option.Option<Verdict>) =>
    Option.match(verdict, {
        onNone: () => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: message } }),
        onSome: Verdict.$match({
            Exited: ({ exit }) => Effect.map(crashed(exit), (crash) => ({ completion: { _tag: 'Failed' as const, reason: described(exit) }, crashed: crash })),
            Hung: ({ hang }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: `hung ${hang.cpu} past ${Duration.format(limit)}` }, hung: hang }),
            Lost: ({ error }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: error.message } }),
        }),
    });
const pressable = (element: Element, label: Option.Option<string>, system: boolean): Option.Option<Control> => {
    const pressing = (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
    const labeled = (text: string) => Array.findFirst(pressing, (control) => control.label === text);
    return system
        ? Option.firstSomeOf([labeled(DISMISS), Array.findFirst(pressing, ({ attribute }) => attribute === 'AXCancelButton')])
        : Option.firstSomeOf([Option.flatMap(label, labeled), ...PRESSED_FIRST.map((name) => Array.findFirst(pressing, ({ attribute, identifier }) => attribute === name || identifier === name)), Array.head(pressing.length === 1 ? pressing : [])]);
};
const watcher = (chosen: Installation, session: Session) =>
    Effect.fn('watcher')(function* (row: { readonly identifier: string; readonly pid: number; readonly state: typeof State.Type }) {
        const automation = yield* Automation;
        const running = Effect.flatMap(Ref.get(session.records), (folder) => Option.match(folder, { onNone: () => Effect.succeedNone, onSome: (held) => Effect.map(recorded(held), ({ running: at }) => at) }));
        const ours = row.identifier === chosen.identifier || (row.identifier === REPORTER && (yield* Effect.orElseSucceed(owner(row.pid), () => '')).startsWith(session.app));
        const answer = Effect.fnUntraced(function* (element: Element) {
            const system = !ours;
            const shown = [element.title ?? '', ...(element.detail?.text ?? [])].join(' ');
            const named = new RegExp(`\\b${chosen.name.replaceAll(/[.*+?^${}()|[\]\\]/gu, '\\$&')}\\b`, 'u').test(shown);
            if (system && !named) {
                return;
            }
            const [at, presses] = yield* Effect.all([running, Ref.get(session.presses)]);
            const used = (yield* Ref.get(session.notes)).filter(([index]) => Option.getOrUndefined(index) === Option.getOrUndefined(at)).length;
            const wanted = Option.flatMap(at, (index) => Option.flatMap(Array.get(presses, index), Array.get(used)));
            const control = pressable(element, wanted, system);
            const note = {
                ...(element.title === undefined ? {} : { title: element.title }),
                text: element.detail?.text ?? [],
                ...Option.match(
                    Option.flatMap(control, ({ label }) => Option.fromUndefinedOr(label)),
                    { onNone: () => ({}), onSome: (pressed) => ({ pressed }) },
                ),
            };
            yield* Ref.update(session.notes, Array.append([at, note] as const));
            yield* Option.match(control, {
                onNone: () => Effect.logWarning(`${row.identifier} ${element.title ?? element.role} holds no button to press: ${shown}`),
                onSome: (pressed) => Effect.catch(automation.perform({ pid: row.pid, actions: [{ perform: { element: pressed.id, action: 'AXPress' } }] }), (cause) => Effect.logWarning(`press of ${pressed.label ?? pressed.id} refused: ${String(cause)}`)),
            });
        });
        yield* State.match(row.state, {
            Launching: () => Effect.void,
            Responding: ({ elements }) =>
                Effect.forEach(
                    elements.filter(({ requiresAnswer }) => requiresAnswer),
                    answer,
                    { discard: true },
                ),
            Unresponsive: () => Ref.set(session.usage, row.state),
            Failed: () => Effect.logWarning(`process ${row.pid} ${described(row.state)}`),
            Exited: (exit) => (row.identifier === chosen.identifier && row.pid === session.pid && settled(exit) ? Deferred.succeed(session.verdict, Verdict.Exited({ exit })) : Effect.void),
        });
    });
const progress = (session: Session, records: string, folders: readonly string[], limit: Duration.Duration) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        yield* Stream.concat(Stream.make(undefined), fs.watch(records)).pipe(
            Stream.switchMap(() => Stream.fromEffect(Effect.sleep(limit))),
            Stream.runHead,
        );
        const [{ running, finished }, usage] = yield* Effect.all([recorded(records), Ref.get(session.usage)]);
        const folder = Option.getOrElse(
            Array.get(
                folders,
                Option.getOrElse(running, () => finished.size),
            ),
            () => records,
        );
        const sample = yield* sampled(session.pid, `${folder}/sample.txt`);
        yield* Deferred.succeed(session.verdict, Verdict.Hung({ index: running, hang: { cpu: usage._tag === 'Unresponsive' && usage.process === 'running' ? 'busy' : 'blocked', sample } }));
    });

// --- [TRANSPORT]
const sent = Effect.fn('sent')(function* (engine: typeof Engine.Type, pid: number, batch: typeof Encoded.Type) {
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    const encoded = yield* Schema.encodeEffect(Encoded)(batch);
    const direct = engine === 'extendscript' ? `${yield* fs.readFileString(yield* path.fromFileUrl(new URL('extendscript.jsxinc', import.meta.url)))}\nharness(arguments[0]);` : { _tag: 'file' as const, url: (yield* bundle).href };
    const { reply } = yield* automation.execute({ pid, command: 'do script', arguments: { direct, language: ENGINES[engine].language, 'with arguments': [encoded] }, artifacts: [] });
    const answer = yield* Schema.decodeUnknownEffect(Reply)(reply);
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: `${engine} batch` }) : Effect.succeed(answer);
});
const acquired = Effect.fn('acquired')(function* (chosen: Installation) {
    const path = yield* Path.Path;
    const app = yield* path.fromFileUrl(chosen.url);
    const instance = yield* Launch({ app });
    yield* lock(chosen.identifier, instance.pid);
    const listed = yield* Effect.orElseSucceed(Stream.runCollect(Stream.splitLines(Stream.decodeText(Stream.unwrap(Effect.map(ChildProcess.make('lsof', ['-Fn', '-p', String(instance.pid)], { stdin: 'ignore', stderr: 'ignore' }), (handle) => handle.stdout))))), () => []);
    const session: Session = {
        pid: instance.pid,
        launched: instance.launched,
        app,
        verdict: yield* Deferred.make<Verdict>(),
        notes: yield* Ref.make<readonly Note[]>([]),
        records: yield* Ref.make(Option.none<string>()),
        presses: yield* Ref.make<readonly (readonly string[])[]>([]),
        usage: yield* Ref.make<typeof State.Type>({ _tag: 'Launching' }),
        log: Array.findFirst(listed, (line) => LOG.test(line)).pipe(Option.flatMap((line) => Option.fromNullishOr(LOG.exec(line)?.groups?.file))),
    };
    yield* settled(instance.state) ? Deferred.succeed(session.verdict, Verdict.Exited({ exit: instance.state })) : Effect.void;
    yield* Effect.forkScoped(
        Stream.runForEach(watch([chosen.identifier, REPORTER], instance.pid), watcher(chosen, session)).pipe(
            Effect.andThen(HarnessError.make({ reason: { _tag: 'Unavailable', because: 'watch ended' }, subject: `process ${instance.pid}` })),
            Effect.catch((error) => Deferred.succeed(session.verdict, Verdict.Lost({ error }))),
        ),
    );
    return session;
});
const batchIn = (session: Session, engine: typeof Engine.Type, batch: Omit<typeof Encoded.Type, 'records'>, presses: readonly (readonly string[])[], folders: readonly string[], limit: Duration.Duration) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const records = yield* fs.makeTempDirectoryScoped({ prefix: 'indesign-records-' });
        const sizes = yield* Ref.make<Record.ReadonlyRecord<string, number>>({});
        const size = Option.match(session.log, { onNone: () => Effect.succeed(0), onSome: (file) => Effect.map(fs.stat(file), ({ size: bytes }) => globalThis.Number(bytes)) });
        yield* Effect.all([Ref.set(session.records, Option.some(records)), Ref.set(session.presses, presses), Ref.set(session.notes, [])]);
        yield* Effect.forkScoped(progress(session, records, folders, limit));
        yield* Effect.forkScoped(
            Stream.runForEach(
                Stream.filterMap(fs.watch(records), (event) =>
                    Result.fromOption(
                        Option.filter(recordOf(event.path.split('/').at(-1) ?? ''), ({ kind }) => kind === 'started'),
                        () => undefined,
                    ),
                ),
                ({ at }) => Effect.flatMap(size, (bytes) => Ref.update(sizes, Record.set(String(at), bytes))),
            ),
        );
        const start = yield* size;
        const reply = yield* Effect.result(
            Effect.raceFirst(
                sent(engine, session.pid, { ...batch, records }),
                Effect.flatMap(Deferred.await(session.verdict), (ended) => HarnessError.make({ reason: { _tag: 'Unavailable', because: verdictText(ended) }, subject: `process ${session.pid}` })),
            ),
        );
        const [{ finished, running }, notes, verdict, offsets, end] = yield* Effect.all([recorded(records), Ref.get(session.notes), Effect.flatMap(Deferred.isDone(session.verdict), (done) => (done ? Effect.asSome(Deferred.await(session.verdict)) : Effect.succeedNone)), Ref.get(sizes), size]);
        const text = yield* Option.match(session.log, { onNone: () => Effect.succeed(new Uint8Array()), onSome: (file) => fs.readFile(file) });
        const logs = (index: number) => {
            const from = offsets[String(index)] ?? start;
            const to = offsets[String(index + 1)] ?? end;
            return engine === 'uxpscript' ? Array.fromIterable(Text.linesIterator(new TextDecoder().decode(text.slice(from, to)))).filter((line) => line.trim() !== '') : [];
        };
        return { reply, finished, running, notes, verdict, logs };
    }).pipe(Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(function* (session: Session, label: string, limit: Duration.Duration) {
    const { reply } = yield* batchIn(session, 'uxpscript', { product: '', label, enumerations: {}, survey: true, cases: [] }, [], [], limit);
    const header = yield* Effect.fromResult(reply);
    return yield* Effect.fromOption(Option.fromUndefinedOr(header.survey), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'survey reply holds no survey' }, subject: `process ${session.pid}` }));
});
const execute = Effect.fn('execute')(function* <P extends Plan>(
    chosen: Installation,
    label: string,
    limit: Duration.Duration,
    known: Option.Option<typeof Survey.Type>,
    planned: (survey: typeof Survey.Type) => Effect.Effect<P, HarnessError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path>,
) {
    interface Step {
        readonly survey: typeof Survey.Type;
        readonly plan: P;
        readonly header: Option.Option<typeof Header.Type>;
        readonly shared: boolean;
        readonly answered: readonly (typeof Answered.Type)[];
        readonly outcomes: readonly (typeof Outcome.Type)[];
    }
    interface Held {
        readonly survey: Option.Option<typeof Survey.Type>;
        readonly plan: Option.Option<P>;
        readonly rest: Option.Option<readonly Prepared[]>;
    }
    const stepped = (held: Held) =>
        Effect.gen(function* () {
            const session = yield* acquired(chosen);
            const survey = yield* Option.match(held.survey, { onSome: Effect.succeed, onNone: () => surveyIn(session, label, limit) });
            const plan = yield* Option.match(held.plan, { onSome: Effect.succeed, onNone: () => planned(survey) });
            const rest = Option.getOrElse(held.rest, () => plan.sendable);
            const base = { survey, plan, shared: !session.launched };
            if (!Array.isReadonlyArrayNonEmpty(rest)) {
                return [[{ ...base, header: Option.none(), answered: [], outcomes: [] } satisfies Step], Option.none<Held>()] as const;
            }
            const ran = yield* batchIn(
                session,
                plan.engine,
                { product: '', label, enumerations: survey.enumerations, survey: false, cases: rest.map(({ hosted }) => hosted) },
                rest.map(({ presses }) => presses),
                rest.map(({ hosted }) => hosted.folder),
                limit,
            );
            const { on, kept, ended, failure, remaining, relaunch, unsent } = finishedIn(rest, ran);
            const interrupted = yield* Option.match(failure, {
                onNone: () => Effect.succeed([]),
                onSome: ({ name, message }) =>
                    Effect.map(
                        fault(
                            limit,
                            message,
                            Option.filter(ran.verdict, () => Option.isSome(ran.running)),
                        ),
                        (parts) => [Outcome.make({ name, answered: on(ended), logs: ran.logs(ended), ...parts })],
                    ),
            });
            const step = { ...base, header: Result.getSuccess(ran.reply), answered: ran.notes.filter(([at]) => Option.isNone(at)).map(([, note]) => note), outcomes: [...kept, ...interrupted, ...unsent] } satisfies Step;
            return [[step], Option.liftPredicate<Held>({ survey: Option.some(survey), plan: Option.some(plan), rest: Option.some(remaining) }, () => relaunch)] as const;
        }).pipe(Effect.scoped);
    const steps = yield* Stream.runCollect(Stream.paginate({ survey: known, plan: Option.none(), rest: Option.none() }, stepped));
    const first = yield* Effect.fromOption(Array.head(steps), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no session ran' }, subject: chosen.name }));
    const outcomes = [...first.plan.settled, ...steps.flatMap(({ outcomes: held }) => held)];
    const copies = new Set(
        yield* Effect.forEach(
            first.plan.sendable.flatMap(({ hosted }) => hosted.precondition.documents.flatMap((document) => ('open' in document ? [document.open] : []))),
            (file) => Effect.map(digest(file), ({ sha256 }) => sha256),
        ),
    );
    const files = new Map(yield* Effect.forEach(first.plan.sendable, ({ hosted }) => Effect.map(writtenIn(hosted.folder, copies), (rows) => [hosted.name, rows] as const)));
    return { survey: first.survey, plan: first.plan, header: Array.findFirst(steps, ({ header }) => header), shared: first.shared, answered: steps.flatMap(({ answered }) => answered), outcomes: outcomes.map((row) => ({ ...row, written: files.get(row.name) ?? [] })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { conditionOf, execute, expand, installations, normalize, Results, scriptOf };
