import { createHash } from 'node:crypto';
import { homedir } from 'node:os';
import process from 'node:process';
import type { AppleEventValue } from '@rasm/automation/apple-events';
import { type Installation, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { transformSync } from '@swc/core';
import { Array, Clock, Data, Deferred, Duration, Effect, FileSystem, Match, Number, Option, Order, Path, Record, Ref, Result, Schema, Stream, String as Text } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import jsesc from 'jsesc';
import { BOXES, Condition, Engine, fields, Host, HostCase, type Model, Outcome, Reply, Request } from '#/acrobat/contract';
import { engine } from '#/acrobat/engine';
import { type Batch, Call, Probe } from '#/cases';
import { HarnessError } from '#/error';
import { expand, scriptOf } from '#/indesign/host';
import { crashed, type Exited, Launch, lock, ended, settled, watch } from '#/instance';
import { type Answered, type Hang, results } from '#/results';
import { bury, JsonObject } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Row = (typeof Batch.Type.cases)[number];
type Note = readonly [number, typeof Answered.Type];
type Answer = Exclude<typeof Reply.Type, { readonly _tag: 'Failed' }>;
type Verdict = Data.TaggedEnum<{ Exited: { readonly exit: Exited }; Hung: { readonly hang: typeof Hang.Type }; Lost: { readonly error: Error }; Failed: { readonly error: Error } }>;
interface Prepared {
    readonly hosted: typeof HostCase.Type;
    readonly row: Row;
    readonly copies: readonly (readonly [string, string])[];
}
interface Pending {
    readonly queue: readonly (readonly Prepared[])[];
    readonly model: Option.Option<typeof Model.Type>;
    readonly host: Option.Option<typeof Host.Type>;
    readonly rerun: boolean;
    readonly ended: boolean;
}
interface Session {
    readonly pid: number;
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly presses: Ref.Ref<readonly string[]>;
    readonly usage: Ref.Ref<typeof State.Type>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CODES = { suite: 'CARO', script: 'miscdosc', quit: 'aevtquit', warnings: 'pskw', menu: 'cmnu', item: 'cmen', name: 'pnam', title: 'ptit' } as const;
const SANDBOX = 'com.apple.security.temporary-exception.files.absolute-path.read-write';
const LOCKDOWN = '/Library/Application Support/Adobe/Acrobat/DC/Preferences/FeatureLockDown';
const OPENING = ['open', 'document', 'application'];
const ELLIPSIS = /(?:…|\.\.\.)$/u;
const BYTE_ORDER_MARK = /^\u{FEFF}/u;
const PRESSED_FIRST = ['AXCancelButton', 'CancelButton', 'AXDefaultButton', 'OKButton'] as const;
const VERSIONS = new Intl.Collator('en', { numeric: true });

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const Results = results(Engine, { host: Host }, fields);
const Program = Schema.fromJsonString(Schema.String).annotate({ description: 'Lowered engine as the string literal the do script eval compiles, a syntax error answering as failed' });
const Entitlements = Schema.fromJsonString(Schema.Struct({ [SANDBOX]: Schema.NonEmptyArray(Schema.String) }));
const sameHost = Schema.toEquivalence(Host);
const Item = Schema.Struct({ seld: Schema.String }).annotate({ description: 'Menu item specifier, its name as the selector' });
const Objects = Schema.fromJsonString(Schema.Struct({ qpdf: Schema.Tuple([Schema.Unknown, Schema.Record(Schema.String, Schema.Json)]) })).annotate({ description: 'qpdf JSON v2 object table' });
const Linked = Schema.Struct({ value: Schema.Struct({ '/Info': Schema.optionalKey(Schema.String), '/Root': Schema.optionalKey(Schema.String), '/Metadata': Schema.optionalKey(Schema.String) }) }).annotate({ description: 'Indirect references a dictionary holds' });
const Dictionary = Schema.Struct({ value: JsonObject });
const Canonical = Schema.fromJsonString(Schema.Array(Schema.Tuple([Schema.String, Schema.Json])));
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        (row: Installation) => Option.getOrElse(row.version, () => ''),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const text = (command: ChildProcess.Command) => ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.string(command));
const nested = (rows: readonly Installation[]) => (row: Installation) => rows.some((other) => other.url.href !== row.url.href && row.url.href.startsWith(other.url.href));
const installations = Effect.fn('installations')(function* (versions: readonly string[]) {
    const automation = yield* Automation;
    const rows = yield* automation.installed;
    const declaring = (row: Installation) =>
        Effect.map(
            Effect.option(automation.terms({ url: row.url })),
            Option.exists(({ suite, command }) => suite.some(({ code }) => code === CODES.suite) && command.some(({ code }) => code === CODES.script)),
        );
    const hosts = Array.sort(
        yield* Effect.filter(
            rows.filter((row) => row.scriptable && !nested(rows)(row)),
            declaring,
            { concurrency: 'unbounded' },
        ),
        newest,
    );
    const listed = hosts.map((row) => `${row.identifier} ${Option.getOrElse(row.version, () => row.name)}`).join(', ');
    const chosen = versions.length === 0 ? hosts : hosts.filter((row) => Option.exists(row.version, (held) => versions.some((prefix) => held.startsWith(prefix))));
    return yield* Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installed ${listed || 'none'}` }, subject: `Acrobat ${versions.join(', ')}` }));
});
const sandbox = Effect.fn('sandbox')(function* (app: string) {
    const plist = yield* text(ChildProcess.make('codesign', ['--display', '--xml', '--entitlements', '-', app]).pipe(ChildProcess.pipeTo(ChildProcess.make('plutil', ['-convert', 'json', '-o', '-', '-']))));
    const {
        [SANDBOX]: [writable],
    } = yield* Schema.decodeEffect(Entitlements)(plist);
    return writable;
});
const lockdown = Effect.fn('lockdown')(function* () {
    const fs = yield* FileSystem.FileSystem;
    return (yield* fs.exists(LOCKDOWN))
        ? Option.some(
              createHash('sha256')
                  .update(yield* fs.readFile(LOCKDOWN))
                  .digest('hex'),
          )
        : Option.none();
});

// --- [CASES]
const sourceOf = Effect.fn('sourceOf')(function* (script: string, ranges: Option.Option<typeof Call.Type.lines & {}>) {
    const fs = yield* FileSystem.FileSystem;
    const rows = Array.fromIterable(Text.linesIterator((yield* fs.readFileString(script)).replace(BYTE_ORDER_MARK, ''))).map((line, index) => [line, index + 1] as const);
    const kept = Option.match(ranges, { onNone: () => rows, onSome: (held) => held.flatMap(([from, to]) => rows.slice(from - 1, to)) });
    return { script, code: kept.map(([line]) => line).join('\n'), lines: kept.map(([, at]) => at) };
});
const conditionOf = (batch: Pick<typeof Batch.Type, 'precondition'>, row: Row): typeof JsonObject.Type => {
    const own = row.precondition ?? {};
    const replaced = OPENING.some((key) => key in own);
    return { ...Record.filter(batch.precondition ?? {}, (_, key) => !(replaced && OPENING.includes(key))), ...own };
};
const prepared = (product: string, root: string, batch: typeof Batch.Type) => (row: Row, index: number) =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const folder = path.join(root, String(index));
        const into = (file: string) => [path.join(product, file), path.join(folder, path.basename(file))] as const;
        const placed = yield* Schema.decodeUnknownEffect(Condition)(conditionOf(batch, row), { onExcessProperty: 'error' });
        yield* row.dialogs === undefined ? Effect.void : HarnessError.make({ reason: { _tag: 'Unavailable', because: 'execDialog answers through windows presses' }, subject: row.name });
        const documents = placed.documents.map((document) => ('open' in document ? { open: into(document.open)[1] } : document));
        const source = yield* Option.match(scriptOf(batch, row), { onNone: () => Effect.succeedNone, onSome: (script) => Effect.asSome(sourceOf(path.join(product, script), Schema.is(Call)(row) ? Option.fromUndefinedOr(row.lines) : Option.none())) });
        const shared = {
            name: row.name,
            precondition: { ...placed, documents },
            read: { ...batch.read, ...row.read },
            folder,
            ...Record.filter({ windows: row.windows, confirms: row.confirms, prompts: row.prompts, files: row.files?.map((file) => (file === null ? null : into(file)[1])) }, (value) => value !== undefined),
        };
        const hosted = yield* Match.value(row).pipe(
            Match.when(Schema.is(Probe), ({ evaluate }) => Effect.succeed(HostCase.cases.Probe.make({ ...shared, evaluate }))),
            Match.when(Schema.is(Call), ({ call, args }) => Effect.succeed(HostCase.cases.Call.make({ ...shared, ...Option.match(source, { onNone: () => ({}), onSome: (held) => ({ source: held }) }), call, args: args.map((arg) => bury(arg)) }))),
            Match.orElse(() =>
                Effect.map(
                    Effect.fromOption(source, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no script for the run case or batch' }, subject: row.name })),
                    (held) => HostCase.cases.Run.make({ ...shared, source: held }),
                ),
            ),
        );
        const copies = [...placed.documents.flatMap((document) => ('open' in document ? [into(document.open)] : [])), ...(row.files ?? []).flatMap((file) => (file === null ? [] : [into(file)]))];
        return { hosted, row, copies } satisfies Prepared;
    }).pipe(Effect.result, Effect.map(Result.mapError((error) => Outcome.make({ name: row.name, completion: { _tag: 'Failed', reason: error.message } }))));
const staged = Effect.fn('staged')(function* ({ hosted, copies }: Prepared) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* fs.remove(hosted.folder, { recursive: true, force: true });
    yield* fs.makeDirectory(hosted.folder, { recursive: true });
    yield* Effect.forEach(copies, ([from, to]) => Effect.andThen(fs.makeDirectory(path.dirname(to), { recursive: true }), fs.copyFile(from, to)), { discard: true });
});
const normalize = Effect.fn('normalize')(function* (product: string, file: string, root: string, only: readonly string[]) {
    const { batch, rows } = yield* expand(product, file);
    yield* Schema.decodeEffect(Schema.Array(Schema.Literals(rows.map(({ name }) => name))))(only);
    const kept = rows.filter(({ name }) => only.length === 0 || only.includes(name));
    const made = yield* Effect.forEach(kept, prepared(product, root, batch));
    return { batch, rows: kept, sendable: Array.getSuccesses(made), settled: Array.getFailures(made) };
});
const mapped = (hosted: typeof HostCase.Type) => (row: typeof Outcome.Type) => {
    const lines = HostCase.match(hosted, { Probe: () => Option.none(), Call: ({ source }) => Option.fromUndefinedOr(source), Run: ({ source }) => Option.some(source) });
    const at = (line: number) =>
        Option.getOrElse(
            Option.flatMap(lines, ({ script, lines: held }) => Option.map(Array.get(held, line - 1), (found) => ({ file: script, line: found }))),
            () => ({ line }),
        );
    return row.completion._tag === 'Thrown' ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at(row.completion.line)), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(({ line }) => at(line)) }) } } : row;
};

// --- [WRITTEN]
const normalized = Effect.fn('normalized')(function* (file: string) {
    const [, table] = (yield* Schema.decodeEffect(Objects)(yield* text(ChildProcess.make('qpdf', ['--json-output', '--json-stream-data=inline', '--decode-level=generalized', file, '-'])))).qpdf;
    const linked = (key: string, link: '/Info' | '/Root' | '/Metadata') => Option.flatMap(Schema.decodeUnknownOption(Linked)(table[key]), ({ value }) => Option.map(Option.fromUndefinedOr(value[link]), (held) => `obj:${held}`));
    const info = linked('trailer', '/Info');
    const metadata = Option.flatMap(linked('trailer', '/Root'), (catalog) => linked(catalog, '/Metadata'));
    const pruned = (entry: Schema.Json, dropped: readonly string[]): Schema.Json => Option.match(Schema.decodeUnknownOption(Dictionary)(entry), { onNone: () => entry, onSome: ({ value }) => ({ value: Record.filter(value, (_, name) => !dropped.includes(name)) }) });
    const kept = Record.filter(
        Record.map(table, (entry, key) => (key === 'trailer' ? pruned(entry, ['/ID']) : Option.contains(info, key) ? pruned(entry, ['/CreationDate', '/ModDate']) : entry)),
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
const writtenIn = Effect.fn('writtenIn')(function* (folder: string, captures: string, copied: ReadonlySet<string>, kept: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = yield* Effect.filter(
        Array.difference(
            (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
            kept,
        ),
        (file) => Effect.map(fs.stat(file), ({ type }) => type === 'File'),
    );
    const fresh = (yield* Effect.forEach(files, digest)).filter(({ sha256 }) => !copied.has(sha256));
    return yield* Effect.forEach(fresh, (row) =>
        Effect.gen(function* () {
            const capture = path.join(captures, path.relative(path.dirname(folder), row.file));
            yield* fs.makeDirectory(path.dirname(capture), { recursive: true });
            yield* fs.copyFile(row.file, capture);
            return { ...row, file: capture };
        }),
    );
});

// --- [WATCHER]
const described = (state: typeof State.Type) =>
    State.match(state, {
        Launching: () => 'launching',
        Responding: () => 'responding',
        Unresponsive: ({ process: run }) => `unresponsive ${run}`,
        Failed: ({ reason }) => `status failed: ${reason}`,
        Exited: ({ signal, status, report }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url.href}`}`,
    });
const verdictText = Verdict.$match({ Exited: ({ exit }) => described(exit), Hung: ({ hang }) => `hung ${hang.cpu}, ${hang.sample}`, Lost: ({ error }) => error.message, Failed: ({ error }) => error.message });
const pressable = (element: Element, wanted: Option.Option<string>) => {
    const pressing = (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
    return Option.firstSomeOf([Option.flatMap(wanted, (label) => Array.findFirst(pressing, (control) => control.label === label)), ...PRESSED_FIRST.map((name) => Array.findFirst(pressing, ({ attribute, identifier }) => attribute === name || identifier === name)), Array.head(pressing.length === 1 ? pressing : [])]);
};
const descends = Effect.fn('descends')(function* (pid: number, from: number) {
    const lines = Text.split(yield* text(ChildProcess.make('ps', ['-A', '-o', 'pid=,ppid='])), '\n');
    const parents = new Map(lines.flatMap((line) => Option.toArray(Option.map(Option.fromNullishOr(/^\s*(?<child>\d+)\s+(?<parent>\d+)/u.exec(line)?.groups), ({ child = '', parent = '' }) => [globalThis.Number(child), globalThis.Number(parent)] as const))));
    const chain = Array.unfold(pid, (at) =>
        Option.map(
            Option.filter(Option.fromUndefinedOr(parents.get(at)), (parent) => parent > 1),
            (parent) => [parent, parent] as const,
        ),
    );
    return chain.includes(from);
});
const watcher = (chosen: Installation, session: Session) =>
    Effect.fn('watcher')(function* (row: { readonly identifier: string; readonly pid: number; readonly state: typeof State.Type }) {
        const automation = yield* Automation;
        const ours = row.pid === session.pid || (yield* Effect.orElseSucceed(descends(row.pid, session.pid), () => false));
        const answer = Effect.fnUntraced(function* (element: Element) {
            const [at, presses] = yield* Effect.all([Clock.currentTimeMillis, Ref.getAndUpdate(session.presses, (held) => held.slice(1))]);
            const control = pressable(element, Array.head(presses));
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
                onNone: () => Effect.logWarning(`${row.identifier} ${element.title ?? element.role} holds no button to press`),
                onSome: (pressed) => Effect.catch(automation.perform({ pid: row.pid, actions: [{ perform: { element: pressed.id, action: 'AXPress' } }] }), (cause) => Effect.logWarning(`press of ${pressed.label ?? pressed.id} refused: ${String(cause)}`)),
            });
        });
        yield* ours
            ? State.match(row.state, {
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
              })
            : Effect.void;
    });

// --- [TRANSPORT]
const specifier = (want: string, form: 'property' | 'index', seld: AppleEventValue, from: Option.Option<AppleEventValue>): AppleEventValue => ({ _tag: 'specifier', want, form, seld, ...Option.match(from, { onNone: () => ({}), onSome: (held) => ({ from: held }) }) });
const property = (code: string, from: Option.Option<AppleEventValue> = Option.none()) => specifier(code, 'property', { _tag: 'type', name: code }, from);
const every = (want: string, from: Option.Option<AppleEventValue> = Option.none()) => specifier(want, 'index', 'every', from);
const acquired = Effect.fn('acquired')(function* (chosen: Installation, helpers: readonly string[]) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const instance = yield* Launch({ app: yield* path.fromFileUrl(chosen.url), fresh: true });
    yield* lock(chosen.identifier, instance.pid);
    const session: Session = {
        pid: instance.pid,
        verdict: yield* Deferred.make<Verdict>(),
        notes: yield* Ref.make<readonly Note[]>([]),
        presses: yield* Ref.make<readonly string[]>([]),
        usage: yield* Ref.make<typeof State.Type>({ _tag: 'Launching' }),
    };
    yield* settled(instance.state) ? HarnessError.make({ reason: { _tag: 'Unavailable', because: described(instance.state) }, subject: `process ${instance.pid}` }) : Effect.void;
    yield* Effect.forkScoped(
        Stream.runForEach(watch([chosen.identifier, ...helpers], instance.pid), watcher(chosen, session)).pipe(
            Effect.andThen(HarnessError.make({ reason: { _tag: 'Unavailable', because: 'watch ended' }, subject: `process ${instance.pid}` })),
            Effect.catch((error) => Deferred.succeed(session.verdict, Verdict.Lost({ error }))),
        ),
    );
    yield* Effect.addFinalizer(() => Effect.catch(automation.execute({ pid: instance.pid, command: CODES.quit, arguments: { saving: { _tag: 'enumerator', name: 'no' } }, artifacts: [] }), (error) => Effect.logWarning(`quit of process ${instance.pid} refused: ${error.message}`)));
    const warnings = property(CODES.warnings);
    const { reply: before } = yield* automation.execute({ pid: instance.pid, command: 'get', arguments: { direct: warnings }, artifacts: [] });
    yield* Effect.acquireRelease(automation.execute({ pid: instance.pid, command: 'set', arguments: { direct: warnings, to: true }, artifacts: [] }), () => Effect.ignore(automation.execute({ pid: instance.pid, command: 'set', arguments: { direct: warnings, to: before }, artifacts: [] })));
    return session;
});
const modalIn = Effect.fn('modalIn')(function* (session: Session) {
    const automation = yield* Automation;
    const got = <S extends Schema.Top & Schema.Decoder<unknown>>(direct: AppleEventValue, schema: S) => Effect.flatMap(automation.execute({ pid: session.pid, command: 'get', arguments: { direct }, artifacts: [] }), ({ reply }) => Schema.decodeUnknownEffect(Schema.NullOr(schema))(reply));
    const menu = (index: number) => {
        const items = every(CODES.item, Option.some(specifier(CODES.menu, 'index', index, Option.none())));
        return Effect.map(Effect.all([got(items, Schema.Array(Item)), got(property(CODES.title, Option.some(items)), Schema.Array(Schema.NullOr(Schema.String)))]), ([named, titles]) => Option.map(Option.fromNullOr(named), (found) => found.flatMap(({ seld }, at) => (ELLIPSIS.test(titles?.[at] ?? '') ? [seld] : []))));
    };
    return (yield* Stream.runCollect(Stream.paginate(1, (index) => Effect.map(menu(index), (names) => [Option.toArray(names).flat(), Option.as(names, index + 1)] as const)))).flat();
});
const sent = Effect.fn('sent')(function* (session: Session, lowered: string, request: typeof Request.Type) {
    const automation = yield* Automation;
    const program = `(function (request) {\n${lowered}\nreturn engine.call(this, request);\n}).call(this, ${jsesc(yield* Schema.encodeEffect(Request)(request), { json: true })});`;
    const direct = `try { eval(${yield* Schema.encodeEffect(Program)(program)}); } catch (error) { encodeURIComponent(JSON.stringify({ _tag: 'Failed', reason: String(error && error.name) + ': ' + String(error && error.message) + ' at line ' + String(error && error.lineNumber) })); }`;
    const { reply } = yield* automation.execute({ pid: session.pid, command: CODES.script, arguments: { direct }, artifacts: [] });
    const answer = yield* Effect.mapError(Schema.decodeUnknownEffect(Reply)(reply), (error) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `reply ${typeof reply} ${String(reply)}` }, subject: `process ${session.pid} batch`, cause: error }));
    return yield* answer._tag === 'Failed' ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.reason }, subject: `process ${session.pid} batch` }) : Effect.succeed(answer);
});
const attempt = Effect.fn('attempt')(function* (session: Session, lowered: string, request: typeof Request.Type, presses: readonly string[], limit: Duration.Duration) {
    yield* Effect.all([Ref.set(session.notes, []), Ref.set(session.presses, presses)]);
    const raced = yield* Effect.result(
        Effect.raceFirst(
            Effect.mapError(sent(session, lowered, request), (error) => Verdict.Failed({ error })),
            Effect.flatMap(Deferred.await(session.verdict), Effect.fail),
        ).pipe(Effect.timeoutOption(Duration.times(limit, Math.max(1, request.cases.length)))),
    );
    const notes = yield* Ref.get(session.notes);
    const killed = Effect.gen(function* () {
        const [path, usage] = yield* Effect.all([Path.Path, Ref.get(session.usage)]);
        const [first] = request.cases;
        const folder = request.cases.length === 1 && first !== undefined ? first.folder : path.dirname(first?.folder ?? homedir());
        const sample = yield* Effect.result(ended(session.pid, path.join(folder, 'sample.txt'))).pipe(Effect.ensuring(Effect.ignore(Effect.try({ try: () => process.kill(session.pid, 'SIGKILL'), catch: HarnessError.refused(`kill of process ${session.pid}`) }))));
        return { cpu: usage._tag === 'Unresponsive' && usage.process === 'running' ? ('busy' as const) : ('blocked' as const), sample: Result.match(sample, { onSuccess: (file) => file, onFailure: (error) => error.message }) };
    });
    const outcome: Result.Result<Answer, Verdict> = yield* Result.match(raced, {
        onSuccess: Option.match({ onNone: () => Effect.map(killed, (hang) => Result.fail(Verdict.Hung({ hang }))), onSome: (reply) => Effect.succeed(Result.succeed<Answer>(reply)) }),
        onFailure: (verdict) => (Verdict.$is('Lost')(verdict) ? Effect.as(killed, Result.fail(verdict)) : Effect.succeed(Result.fail(verdict))),
    });
    return { outcome, notes };
});
const fault = (limit: Duration.Duration, verdict: Verdict) =>
    Verdict.$match(verdict, {
        Exited: ({ exit }) => Effect.map(crashed(exit), (crash) => ({ completion: { _tag: 'Failed' as const, reason: described(exit) }, crashed: crash })),
        Hung: ({ hang }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: `hung ${hang.cpu} past ${Duration.format(limit)}` }, hung: hang }),
        Lost: ({ error }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: error.message } }),
        Failed: ({ error }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: error.message } }),
    });
const grouped = (sendable: readonly Prepared[]) =>
    sendable.reduce<readonly (readonly Prepared[])[]>((groups, item) => {
        const last = groups.at(-1);
        const alone = (held: Prepared) => (held.row.presses ?? []).length > 0;
        return last === undefined || alone(item) || last.some(alone) ? [...groups, [item]] : [...groups.slice(0, -1), [...last, item]];
    }, []);
const execute = Effect.fn('execute')(function* (chosen: Installation, limit: Duration.Duration, known: Option.Option<typeof Model.Type>, sendable: readonly Prepared[], deferred: { readonly host: Option.Option<typeof Host.Type>; readonly cases: readonly Prepared[] }) {
    const automation = yield* Automation;
    const rows = yield* automation.installed;
    const helpers = Array.dedupe(rows.filter((row) => row.url.href !== chosen.url.href && row.url.href.startsWith(chosen.url.href)).map(({ identifier }) => identifier));
    const lowered = yield* Effect.try({ try: () => transformSync(`var engine = ${engine.toString()};`, { jsc: { target: 'es5', parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } }, isModule: false }).code, catch: HarnessError.refused('engine lowering') });
    const sealed = yield* lockdown();
    const sessionOf = Effect.fnUntraced(function* (held: Pending) {
        const session = yield* acquired(chosen, helpers);
        const modal = yield* Option.match(held.model, { onSome: ({ modal: names }) => Effect.succeed(names), onNone: () => modalIn(session) });
        const steps = yield* Stream.runCollect(
            Stream.paginate({ ...held, ended: false }, (state: Pending) =>
                Effect.gen(function* () {
                    const [group = [], ...rest] = state.queue;
                    yield* Effect.forEach(group, staged, { discard: true });
                    const base = { home: homedir(), boxes: BOXES, modal, cases: group.map(({ hosted }) => hosted) };
                    const request = Option.match(state.model, { onNone: () => Request.cases.Survey.make(base), onSome: ({ signatures }) => Request.cases.Cases.make({ ...base, signatures }) });
                    const { outcome, notes } = yield* attempt(
                        session,
                        lowered,
                        request,
                        group.flatMap(({ row }) => row.presses ?? []),
                        limit,
                    );
                    const made = yield* Result.match(outcome, {
                        onSuccess: (reply) => {
                            const within = (index: number) =>
                                Option.match(Array.get(reply.spans, index), {
                                    onNone: () => () => false,
                                    onSome:
                                        ({ start, end }) =>
                                        ([at]: Note) =>
                                            at >= start && at <= end,
                                });
                            const outcomes = reply.outcomes.map((row, index) => Option.match(Array.get(group, index), { onNone: () => row, onSome: ({ hosted }) => mapped(hosted)({ ...row, answered: notes.filter(within(index)).map(([, note]) => note) }) }));
                            const host = { ...reply.viewer, ...Option.match(sealed, { onNone: () => ({}), onSome: (held) => ({ lockdown: held }) }) };
                            const model = reply._tag === 'Survey' ? Option.some({ ...reply.model, modal }) : state.model;
                            const changed = !state.rerun && Option.exists(state.host, (before) => !sameHost(before, host));
                            const kept = new Map(group.map(({ hosted }, index) => [hosted.name, Option.match(Array.get(reply.spans, index), { onNone: () => [], onSome: ({ kept: files }) => files })] as const));
                            return Effect.succeed({
                                outcomes,
                                kept,
                                answered: notes.filter((note) => !reply.spans.some((_, index) => within(index)(note))).map(([, note]) => note),
                                model,
                                host: Option.some(host),
                                queue: changed ? [...rest, ...grouped(deferred.cases)] : rest,
                                rerun: state.rerun || changed,
                                ended: false,
                            });
                        },
                        onFailure: (verdict) =>
                            group.length === 1
                                ? Effect.map(fault(limit, verdict), (parts) => ({
                                      outcomes: group.map(({ hosted }) => Outcome.make({ name: hosted.name, answered: notes.map(([, note]) => note), ...parts })),
                                      kept: new Map<string, readonly string[]>(),
                                      answered: [],
                                      model: state.model,
                                      host: state.host,
                                      queue: rest,
                                      rerun: state.rerun,
                                      ended: !Verdict.$is('Failed')(verdict),
                                  }))
                                : Effect.succeed({ outcomes: [], kept: new Map<string, readonly string[]>(), answered: notes.map(([, note]) => note), model: state.model, host: state.host, queue: [...group.map((item) => [item]), ...rest], rerun: state.rerun, ended: !Verdict.$is('Failed')(verdict) }),
                    });
                    return [[made], Option.liftPredicate<Pending>({ queue: made.queue, model: made.model, host: made.host, rerun: made.rerun, ended: made.ended }, (next) => next.queue.length > 0 && !next.ended)] as const;
                }),
            ),
        );
        const last = Array.last(steps);
        const next = Option.flatMap(last, (step) => Option.liftPredicate<Pending>({ queue: step.queue, model: step.model, host: step.host, rerun: step.rerun, ended: false }, ({ queue }) => step.ended && queue.length > 0));
        return [[steps], next] as const;
    }, Effect.scoped);
    const sessions = (yield* Stream.runCollect(Stream.paginate({ queue: grouped(sendable).length === 0 ? [[]] : grouped(sendable), model: known, host: deferred.host, rerun: false, ended: false }, sessionOf))).flat();
    const last = yield* Effect.fromOption(Array.last(sessions), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no session ran' }, subject: chosen.name }));
    const host = yield* Effect.fromOption(Option.firstSomeOf(sessions.map(({ host: held }) => held)), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `no batch answered, ${sessions.flatMap(({ outcomes }) => outcomes.map(({ name, completion }) => `${name} ${completion._tag === 'Failed' ? completion.reason : completion._tag}`)).join('; ')}` }, subject: chosen.name }));
    const model = yield* Effect.fromOption(last.model, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no survey answered' }, subject: chosen.name }));
    const prepared = [...sendable, ...deferred.cases];
    const copied = new Set(yield* Effect.forEach(Array.dedupe(prepared.flatMap(({ copies }) => copies.map(([from]) => from))), (file) => Effect.map(digest(file), ({ sha256 }) => sha256)));
    const kept = new Map(sessions.flatMap(({ kept: held }) => [...held]));
    return { model, host, copied, kept, answered: sessions.flatMap(({ answered }) => answered), outcomes: sessions.flatMap(({ outcomes }) => outcomes) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { conditionOf, execute, installations, lockdown, normalize, Results, sandbox, writtenIn };
