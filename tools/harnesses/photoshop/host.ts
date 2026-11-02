import { kill } from 'node:process';
import { Installation, type RunningInstance, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Array, Data, Deferred, Duration, Effect, Exit, FileSystem, Match, Number, Option, Order, Path, type PlatformError, Record, Ref, Result, Schema, Scope, Stream, SubscriptionRef, String as Text } from 'effect';
import { Digest } from 'effect/crypto';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import jsesc from 'jsesc';
import { type Batch, Call, Probe } from '#/cases';
import { HarnessError } from '#/error';
import { conditionOf, expand, scriptOf } from '#/indesign/host';
import { crashed, type Exited, ended, Launch, lock, owner, settled, watch } from '#/instance';
import { Answer, Document, HostCase, Outcome, Recorded, Reply, type Request, type Survey } from '#/photoshop/contract';
import { precondition } from '#/photoshop/precondition';
import type { Answered, Hang } from '#/results';
import { bury } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Control = NonNullable<Element['detail']>['controls'][number];
type Row = (typeof Batch.Type.cases)[number];
type Note = readonly [Option.Option<number>, typeof Answered.Type];
type Header = typeof Answer.cases.Answered.Type;
type Verdict = Data.TaggedEnum<{ Exited: { readonly exit: Exited }; Hung: { readonly hang: typeof Hang.Type }; Lost: { readonly error: HarnessError | Schema.SchemaError | PlatformError.PlatformError } }>;
interface Release {
    readonly installation: Installation;
    readonly app: string;
    readonly version: string;
    readonly inode: number;
    readonly watched: Array.NonEmptyReadonlyArray<string>;
}
interface Mapped {
    readonly file: string;
    readonly line: number;
}
interface Prepared {
    readonly hosted: typeof HostCase.Type;
    readonly row: Row;
    readonly map: readonly Mapped[];
    readonly copies: readonly string[];
    readonly presses: readonly string[];
}
interface Plan {
    readonly sendable: readonly Prepared[];
    readonly settled: readonly (typeof Outcome.Type)[];
    readonly formats: ReadonlySet<string>;
}
interface Session {
    readonly pid: number;
    readonly release: Release;
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly gone: Deferred.Deferred<Exited>;
    readonly busy: Ref.Ref<boolean>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly shots: Ref.Ref<number>;
    readonly records: Ref.Ref<Option.Option<string>>;
    readonly presses: Ref.Ref<readonly (readonly string[])[]>;
    readonly folders: Ref.Ref<readonly string[]>;
    readonly usage: Ref.Ref<typeof State.Type>;
    readonly captures: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDENTIFIER = 'com.adobe.Photoshop';
const JAVASCRIPT = { command: 'miscDjxM', argument: 'JArg' } as const;
const RECORD = /^(?<index>\d+)\.(?<kind>json|started)$/u;
const VERSIONS = new Intl.Collator('en', { numeric: true });
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);
const BYTE_ORDER_MARK = /^﻿/u;
const JSXBIN = /(?<quote>["'])(?<literal>@JSXBIN@(?:\\.|(?!\k<quote>)[^\\])*)\k<quote>/su;
const CONTINUATION = /\\\r?\n/gu;
const ESCAPE = /\\(?<character>.)/gu;
const INCLUDE = /^[ \t]*(?:\/\/[ \t]*@|#)include[ \t]+["'](?<name>[^"']+)["'][ \t;]*$/u;
const INCLUDE_PATH = /^[ \t]*(?:\/\/[ \t]*@|#)includepath[ \t]+["'](?<paths>[^"']*)["']/mu;
const DIRECTIVE = /^[ \t]*#(?:target|targetengine|strict|script)\b/u;
const WRITABLE = /^[A-Z0-9]+$/u;
const SPACED = /\s+/u;

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const Info = Schema.fromJsonString(
    Schema.Struct({
        'Adobe Product Build': Schema.String.annotate({ description: 'Build each release and channel states, 20260929.r.26' }),
        RELEASECHANNEL: Schema.String.annotate({ description: 'public or beta' }),
    }),
);
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        (row: Release) => row.version,
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [BUNDLE]
const output = Effect.fn('output')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit' });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text : yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code}` }, subject: [command, ...args].join(' ') });
}, Effect.scoped);
const released = Effect.fn('released')(function* (installation: Installation) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const app = yield* path.fromFileUrl(installation.url);
    const unstated = (because: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: app });
    const [own, nested, info] = yield* Effect.all([Effect.flatMap(output('plutil', ['-convert', 'json', '-o', '-', path.join(app, 'Contents', 'Info.plist')]), Schema.decodeEffect(Info)), output('find', [path.join(app, 'Contents'), '-name', '*.app', '-prune']), fs.stat(app)], { concurrency: 'unbounded' });
    const version = yield* Effect.fromOption(installation.version, () => unstated('bundle states no version'));
    const inode = yield* Effect.fromOption(info.ino, () => unstated('file system states no inode'));
    return { installation, app, version: `${version}+${own['Adobe Product Build']}.${own.RELEASECHANNEL}`, inode: globalThis.Number(inode), watched: [installation.identifier, ...Array.fromIterable(Text.linesIterator(nested))] } satisfies Release;
});
const installations = Effect.fn('installations')(function* (versions: readonly string[]) {
    const automation = yield* Automation;
    const hosts = Array.sort(
        yield* Effect.forEach(
            (yield* automation.installed).filter((row) => row.scriptable && row.identifier === IDENTIFIER),
            released,
        ),
        newest,
    );
    const chosen = versions.length === 0 ? hosts : hosts.filter((row) => versions.some((prefix) => row.version.startsWith(prefix)));
    return yield* Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installed versions ${hosts.map(({ version }) => version).join(', ')}` }, subject: `${IDENTIFIER} ${versions.join(', ')}` }));
});

// --- [SOURCE]
const prepared = Effect.fn('prepared')(function* (script: string, lines: Option.Option<NonNullable<typeof Call.Type.lines>>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = (yield* fs.readFileString(script)).replace(BYTE_ORDER_MARK, '');
    const binary = Option.map(Option.fromNullishOr(JSXBIN.exec(text)?.groups?.literal), (literal) => literal.replace(CONTINUATION, '').replace(ESCAPE, '$<character>'));
    const located = (file: string, searched: readonly string[], name: string, chain: readonly string[]) =>
        Effect.gen(function* () {
            const found = yield* Effect.findFirst(
                searched.map((entry) => path.resolve(entry, name)),
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
    return { source: { script, code: kept.map(([line]) => line).join('\n') }, map: kept.map(([, at]) => at) };
});
const mapped = (map: readonly Mapped[]) => (row: typeof Outcome.Type) => {
    const at = (line: number) => Option.getOrElse(Array.get(map, line - 1), () => ({ line }));
    return row.completion._tag === 'Thrown' ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at(row.completion.line)), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(({ line }) => at(line)) }) } } : row;
};

// --- [RECORDS]
const recordOf = (file: string) => Option.flatMap(Option.fromNullishOr(RECORD.exec(file)?.groups), ({ index = '', kind = '' }) => Option.map(Number.parse(index), (at) => ({ file, at, kind })));
const recorded = Effect.fn('recorded')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = Array.getSomes((yield* fs.readDirectory(folder)).map(recordOf));
    const done = new Map(
        yield* Effect.forEach(
            entries.filter(({ kind }) => kind === 'json'),
            ({ file, at }) => Effect.map(Effect.flatMap(fs.readFileString(path.join(folder, file)), Schema.decodeEffect(Recorded)), (row) => [at, row] as const),
        ),
    );
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
const writable = Effect.map(output('exiftool', ['-listwf']), (text) => new Set(text.split(SPACED).filter((token) => WRITABLE.test(token))));
const digest = (formats: ReadonlySet<string>) =>
    Effect.fn('digest')(function* (file: string) {
        const [fs, path, hashing] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Digest.Digest]);
        const bytes = yield* fs.readFile(file);
        const content = formats.has(path.extname(file).slice(1).toUpperCase())
            ? yield* ChildProcess.make('exiftool', ['-XMP:all=', '-EXIF:all=', '-o', '-', file], { stdin: 'ignore', stderr: 'inherit' }).pipe(
                  Effect.flatMap(({ stdout }) => Stream.mkUint8Array(stdout)),
                  Effect.scoped,
              )
            : bytes;
        return { file, size: bytes.byteLength, sha256: yield* hashing.digestHex(Digest.sha256, content) };
    });
const writtenIn = Effect.fn('writtenIn')(function* (formats: ReadonlySet<string>, folder: string, copies: ReadonlySet<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const files = yield* Effect.filter(
        (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
        (file) => Effect.map(fs.stat(file), ({ type }) => type === 'File'),
    );
    return (yield* Effect.forEach(files, digest(formats))).filter(({ sha256 }) => !copies.has(sha256));
});

// --- [CASES]
const hostCase = Effect.fn('hostCase')(function* (product: string, captures: string, formats: ReadonlySet<string>, batch: typeof Batch.Type, Precondition: ReturnType<typeof precondition>, row: Row, index: number) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.join(captures, String(index));
    yield* fs.makeDirectory(folder, { recursive: true });
    const placed = yield* Schema.decodeUnknownEffect(Precondition)(conditionOf(batch, row));
    const documents = yield* Effect.forEach(placed.documents, (document) =>
        Document.match(document, {
            Made: (made) => Effect.succeed(made),
            Opened: ({ open }) => Effect.as(fs.copyFile(path.join(product, open), path.join(folder, path.basename(open))), Document.cases.Opened.make({ open: path.join(folder, path.basename(open)) })),
        }),
    );
    const copies = yield* Effect.forEach(documents.filter(Document.guards.Opened), ({ open }) => Effect.map(digest(formats)(open), ({ sha256 }) => sha256));
    const answered = Record.filter({ windows: row.windows, confirms: row.confirms, prompts: row.prompts, files: row.files?.map((file) => (file === null ? null : path.join(product, file))) }, (value) => value !== undefined);
    const shared = { name: row.name, precondition: { ...placed, documents }, read: { ...batch.read, ...row.read }, folder, ...answered };
    const loaded = yield* Option.match(scriptOf(batch, row), {
        onNone: () => Effect.succeedNone,
        onSome: (script) => Effect.asSome(prepared(path.join(product, script), Schema.is(Call)(row) ? Option.fromUndefinedOr(row.lines) : Option.none())),
    });
    const source = Option.map(loaded, (held) => held.source);
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
    return { hosted, row, map: Option.match(loaded, { onNone: () => [], onSome: ({ map }) => map }), copies, presses: row.presses ?? [] } satisfies Prepared;
});
const normalize = Effect.fn('normalize')(function* (product: string, file: string, captures: string, only: readonly string[], survey: typeof Survey.Type) {
    const fs = yield* FileSystem.FileSystem;
    const { batch, rows } = yield* expand(product, file);
    yield* Schema.decodeEffect(Schema.Array(Schema.Literals(rows.map(({ name }) => name))))(only);
    const kept = rows.filter(({ name }) => only.length === 0 || only.includes(name));
    yield* fs.remove(captures, { recursive: true, force: true });
    const [Precondition, formats] = [precondition(survey), yield* writable];
    const made = yield* Effect.forEach(kept, (row, index) =>
        hostCase(product, captures, formats, batch, Precondition, row, index).pipe(
            Effect.mapError((error) => Outcome.make({ name: row.name, completion: { _tag: 'Failed', reason: error.message } })),
            Effect.result,
        ),
    );
    return { batch, rows: kept, formats, sendable: Array.getSuccesses(made), settled: Array.getFailures(made) };
});

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
const pressable = (element: Element, label: Option.Option<string>, system: boolean): Option.Option<Control> => {
    const pressing = (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
    const labeled = (text: string) => Array.findFirst(pressing, (control) => control.label === text);
    const pressedFirst = ['AXCancelButton', 'CancelButton', 'AXDefaultButton', 'OKButton'] as const;
    return system
        ? Option.firstSomeOf([labeled('Ignore'), Array.findFirst(pressing, ({ attribute }) => attribute === 'AXCancelButton')])
        : Option.firstSomeOf([Option.flatMap(label, labeled), ...pressedFirst.map((name) => Array.findFirst(pressing, ({ attribute, identifier }) => attribute === name || identifier === name)), Array.head(pressing.length === 1 ? pressing : [])]);
};
const watcher = (session: Session) =>
    Effect.fn('watcher')(function* (row: { readonly identifier: string; readonly pid: number; readonly state: typeof State.Type }) {
        const [automation, path] = yield* Effect.all([Automation, Path.Path]);
        const host = row.identifier === session.release.installation.identifier && row.pid === session.pid;
        const running = yield* Effect.flatMap(Ref.get(session.records), Option.match({ onNone: () => Effect.succeedNone, onSome: (held: string) => Effect.map(recorded(held), ({ running: at }) => at) }));
        const detailed = (element: Element) =>
            element.detail === undefined
                ? Effect.map(
                      Effect.orElseSucceed(automation.status({ pid: row.pid }), () => row.state),
                      (state) => Option.getOrElse(State.guards.Responding(state) ? Array.findFirst(state.elements, ({ id }) => id === element.id) : Option.none(), () => element),
                  )
                : Effect.succeed(element);
        const answer = Effect.fnUntraced(function* (shown: Element) {
            const element = yield* detailed(shown);
            const ours = host || (yield* Effect.orElseSucceed(owner(row.pid), () => '')).startsWith(session.release.app);
            const text = [element.title ?? '', ...(element.detail?.text ?? [])].join(' ');
            const named = new RegExp(`(?<!\\w)${RegExp.escape(session.release.installation.name)}(?!\\w)`, 'u').test(text);
            if (!(ours || named)) {
                return;
            }
            const [presses, notes, folders, shot] = yield* Effect.all([Ref.get(session.presses), Ref.get(session.notes), Ref.get(session.folders), Ref.getAndUpdate(session.shots, (count) => count + 1)]);
            const used = notes.filter(([index]) => Option.getOrUndefined(index) === Option.getOrUndefined(running)).length;
            const control = pressable(
                element,
                Option.flatMap(running, (index) => Option.flatMap(Array.get(presses, index), Array.get(used))),
                !ours,
            );
            const pressed = yield* Option.match(control, {
                onNone: () => Effect.succeedNone,
                onSome: (held) => Effect.match(automation.perform({ pid: row.pid, actions: [{ perform: { element: held.id, action: 'AXPress' } }] }), { onFailure: () => Option.none<Control>(), onSuccess: () => Option.some(held) }),
            });
            const capture = path.join(
                Option.getOrElse(
                    Option.flatMap(running, (index) => Array.get(folders, index)),
                    () => session.captures,
                ),
                `dialog-${session.pid}-${shot}.png`,
            );
            const file = yield* Effect.orDie(path.toFileUrl(capture));
            const captured = Option.isSome(pressed) ? [] : [yield* Effect.match(automation.screenshot({ pid: row.pid, window: Option.fromUndefinedOr(element.window), longestSide: Option.none(), file }), { onFailure: () => `capture of ${capture} failed`, onSuccess: () => `capture ${capture}` })];
            const note = {
                ...(element.title === undefined ? {} : { title: element.title }),
                text: [...(element.detail?.text ?? []), ...captured],
                ...Option.match(
                    Option.map(pressed, ({ label, id }) => label ?? id),
                    { onNone: () => ({}), onSome: (label) => ({ pressed: label }) },
                ),
            };
            yield* Ref.update(session.notes, Array.append([running, note] as const));
        });
        yield* State.match(row.state, {
            Launching: () => Effect.void,
            Responding: ({ elements }) =>
                Effect.forEach(
                    elements.filter(({ requiresAnswer }) => requiresAnswer),
                    answer,
                    { discard: true },
                ),
            Unresponsive: () => (host ? Ref.set(session.usage, row.state) : Effect.void),
            Failed: () => Effect.logWarning(`process ${row.pid} ${described(row.state)}`),
            Exited: (exit) => (host && settled(exit) ? Effect.andThen(Deferred.succeed(session.gone, exit), Deferred.succeed(session.verdict, Verdict.Exited({ exit }))) : Effect.void),
        });
    });
const progress = Effect.fn('progress')(function* (session: Session, records: string, limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* fs.watch(records).pipe(Stream.prepend([undefined]), Stream.debounce(limit), Stream.runHead);
    const [{ running, finished }, usage, folders] = yield* Effect.all([recorded(records), Ref.get(session.usage), Ref.get(session.folders)]);
    const folder = Option.getOrElse(
        Array.get(
            folders,
            Option.getOrElse(running, () => finished.size),
        ),
        () => session.captures,
    );
    const sample = yield* ended(session.pid, path.join(folder, 'sample.txt'));
    yield* Deferred.succeed(session.verdict, Verdict.Hung({ hang: { cpu: usage._tag === 'Unresponsive' && usage.process === 'running' ? 'busy' : 'blocked', sample } }));
});

// --- [TRANSPORT]
const sent = Effect.fn('sent')(function* (session: Session, batch: typeof Request.Type) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const encoded = jsesc(batch, { json: true });
    const direct = `$.evalFile(new File(${jsesc(yield* path.fromFileUrl(ENGINE), { quotes: 'double', wrap: true })}))`;
    const { reply } = yield* automation.execute({ pid: session.pid, command: JAVASCRIPT.command, arguments: { direct, [JAVASCRIPT.argument]: [encoded] }, artifacts: [] });
    return yield* Effect.flatMap(Schema.decodeUnknownEffect(Reply)(reply), Answer.match({ Answered: (header): Effect.Effect<Header, HarnessError> => Effect.succeed(header), Failed: ({ reason }) => HarnessError.make({ reason: { _tag: 'Unavailable', because: reason }, subject: 'extendscript batch' }) }));
});
const responding = Effect.fn('responding')(function* (release: Release, pid: number) {
    const automation = yield* Automation;
    return yield* SubscriptionRef.changes(automation.instances).pipe(
        Stream.map((rows) => Array.findFirst(rows, (row: RunningInstance) => Installation.runs(release.installation)(row) && row.pid === pid)),
        Stream.filter(Option.exists((row: RunningInstance) => State.guards.Responding(row.state))),
        Stream.runHead,
        Effect.map(Option.flatten),
    );
});
const closed = (session: Session, launching: Scope.Closeable, limit: Duration.Duration) =>
    Effect.gen(function* () {
        yield* (yield* Ref.get(session.busy)) ? Effect.ignore(Effect.try(() => kill(session.pid, 'SIGKILL'))) : Scope.close(launching, Exit.void);
        yield* Effect.ignore(Effect.timeout(Deferred.await(session.gone), limit));
    });
const acquired = Effect.fn('acquired')(function* (release: Release, limit: Duration.Duration, captures: string) {
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    yield* fs.makeDirectory(captures, { recursive: true });
    yield* lock(release.installation.identifier, release.inode);
    yield* Option.match(Array.findFirst(yield* SubscriptionRef.get(automation.instances), Installation.runs(release.installation)), {
        onNone: () => Effect.void,
        onSome: ({ pid }) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `instance ${pid} runs on the settings folder a second instance would share` }, subject: release.app }),
    });
    const launching = yield* Scope.make();
    yield* Effect.addFinalizer((exit) => Scope.close(launching, exit));
    const instance = yield* Scope.provide(launching)(Launch({ app: release.app, fresh: true }));
    const lost = (because: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: `process ${instance.pid}` });
    const session: Session = {
        pid: instance.pid,
        release,
        verdict: yield* Deferred.make<Verdict>(),
        gone: yield* Deferred.make<Exited>(),
        busy: yield* Ref.make(false),
        notes: yield* Ref.make<readonly Note[]>([]),
        shots: yield* Ref.make(0),
        records: yield* Ref.make(Option.none<string>()),
        presses: yield* Ref.make<readonly (readonly string[])[]>([]),
        folders: yield* Ref.make<readonly string[]>([]),
        usage: yield* Ref.make<typeof State.Type>({ _tag: 'Launching' }),
        captures,
    };
    yield* settled(instance.state) ? Effect.andThen(Deferred.succeed(session.gone, instance.state), Deferred.succeed(session.verdict, Verdict.Exited({ exit: instance.state }))) : Effect.void;
    yield* Effect.forkScoped(
        Stream.runForEach(watch(release.watched, instance.pid), watcher(session)).pipe(
            Effect.andThen(lost('watch ended')),
            Effect.catch((error) => Deferred.succeed(session.verdict, Verdict.Lost({ error }))),
        ),
    );
    yield* Effect.addFinalizer(() => closed(session, launching, limit));
    const answered = yield* Effect.timeoutOption(Effect.flatMap(responding(release, instance.pid), Effect.fromOption), limit);
    yield* Option.isSome(answered)
        ? Effect.void
        : Effect.andThen(
              Ref.set(session.busy, true),
              Effect.flatMap(ended(instance.pid, path.join(captures, 'launch-sample.txt')), (sample) => lost(`no responding state past ${Duration.format(limit)}, sample ${sample}`)),
          );
    return session;
});
const batchIn = Effect.fn('batchIn')(function* (session: Session, batch: Omit<typeof Request.Type, 'records'>, presses: readonly (readonly string[])[], folders: readonly string[], limit: Duration.Duration) {
    const fs = yield* FileSystem.FileSystem;
    const records = yield* fs.makeTempDirectoryScoped({ prefix: 'photoshop-records-' });
    yield* Effect.addFinalizer(() => Ref.set(session.records, Option.none()));
    yield* Effect.all([Ref.set(session.records, Option.some(records)), Ref.set(session.presses, presses), Ref.set(session.folders, folders), Ref.set(session.notes, [])]);
    yield* Effect.forkScoped(progress(session, records, limit));
    const reply = yield* Effect.result(
        Effect.raceFirst(
            Ref.set(session.busy, true).pipe(
                Effect.andThen(sent(session, { ...batch, records })),
                Effect.onExit((exit) => (Exit.hasInterrupts(exit) ? Effect.void : Ref.set(session.busy, false))),
            ),
            Effect.flatMap(Deferred.await(session.verdict), (reached) => HarnessError.make({ reason: { _tag: 'Unavailable', because: verdictText(reached) }, subject: `process ${session.pid}` })),
        ),
    );
    const [{ finished, running }, notes, verdict] = yield* Effect.all([recorded(records), Ref.get(session.notes), Effect.flatMap(Deferred.poll(session.verdict), Option.match({ onNone: () => Effect.succeedNone, onSome: Effect.asSome }))]);
    return { reply, finished, running, notes, verdict };
}, Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(function* (session: Session, label: string, limit: Duration.Duration) {
    const { survey } = yield* Effect.flatMap(batchIn(session, { label, survey: true, cases: [] }, [], [], limit), ({ reply }) => Effect.fromResult(reply));
    return yield* Effect.fromOption(Option.fromUndefinedOr(survey), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'survey reply holds no survey' }, subject: `process ${session.pid}` }));
});
const fault = (limit: Duration.Duration, message: string, verdict: Option.Option<Verdict>) =>
    Option.match(verdict, {
        onNone: () => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: message } }),
        onSome: Verdict.$match({
            Exited: ({ exit }) => Effect.map(crashed(exit), (crash) => ({ completion: { _tag: 'Failed' as const, reason: described(exit) }, crashed: crash })),
            Hung: ({ hang }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: `hung ${hang.cpu} past ${Duration.format(limit)}` }, hung: hang }),
            Lost: ({ error }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: error.message } }),
        }),
    });
const execute = Effect.fn('execute')(function* <P extends Plan>(
    release: Release,
    label: string,
    captures: string,
    limit: Duration.Duration,
    known: Option.Option<typeof Survey.Type>,
    planned: (survey: typeof Survey.Type) => Effect.Effect<P, HarnessError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path | Digest.Digest | ChildProcessSpawner.ChildProcessSpawner>,
) {
    interface Step {
        readonly survey: typeof Survey.Type;
        readonly plan: P;
        readonly header: Option.Option<Header>;
        readonly answered: readonly (typeof Answered.Type)[];
        readonly outcomes: readonly (typeof Outcome.Type)[];
    }
    interface Held {
        readonly survey: Option.Option<typeof Survey.Type>;
        readonly plan: Option.Option<P>;
        readonly rest: Option.Option<readonly Prepared[]>;
    }
    const stepped = Effect.fnUntraced(function* (held: Held) {
        const session = yield* acquired(release, limit, captures);
        const survey = yield* Option.match(held.survey, { onSome: Effect.succeed, onNone: () => surveyIn(session, label, limit) });
        const plan = yield* Option.match(held.plan, { onSome: Effect.succeed, onNone: () => planned(survey) });
        const rest = Option.getOrElse(held.rest, () => plan.sendable);
        if (!Array.isReadonlyArrayNonEmpty(rest)) {
            return [{ survey, plan, header: Option.none(), answered: [], outcomes: [] } satisfies Step, Option.none<Held>()] as const;
        }
        const ran = yield* batchIn(
            session,
            { label, cases: rest.map(({ hosted }) => hosted) },
            rest.map(({ presses }) => presses),
            rest.map(({ hosted }) => hosted.folder),
            limit,
        );
        const on = (index: number) => ran.notes.filter(([at]) => Option.contains(at, index)).map(([, note]) => note);
        const noted = (index: number) => (row: typeof Outcome.Type) => mapped(rest[index]?.map ?? [])({ ...row, answered: [...row.answered, ...on(index)] });
        const kept = Array.getSomes(rest.map((_, index) => Option.map(Option.fromUndefinedOr(ran.finished.get(index)), noted(index))));
        const stopped = Option.getOrElse(ran.running, () => kept.length);
        const remaining = Result.isFailure(ran.reply) ? rest.slice(stopped + 1) : [];
        const relaunch = Option.exists(ran.verdict, (verdict) => !Verdict.$is('Lost')(verdict)) && remaining.length > 0;
        const unsent = relaunch ? [] : remaining.map(({ hosted }) => Outcome.make({ name: hosted.name, completion: { _tag: 'Failed', reason: 'batch ended before the case ran' } }));
        const interrupted = yield* Option.match(
            Option.zipWith(Result.getFailure(ran.reply), Array.get(rest, stopped), (error, { hosted }) => ({ name: hosted.name, message: error.message })),
            {
                onNone: () => Effect.succeed([]),
                onSome: ({ name, message }) =>
                    Effect.map(
                        fault(
                            limit,
                            message,
                            Option.filter(ran.verdict, () => Option.isSome(ran.running)),
                        ),
                        (parts) => [Outcome.make({ name, answered: on(stopped), ...parts })],
                    ),
            },
        );
        const step = { survey, plan, header: Result.getSuccess(ran.reply), answered: ran.notes.filter(([at]) => Option.isNone(at)).map(([, note]) => note), outcomes: [...kept, ...interrupted, ...unsent] } satisfies Step;
        return [step, Option.liftPredicate<Held>({ survey: Option.some(survey), plan: Option.some(plan), rest: Option.some(remaining) }, () => relaunch)] as const;
    }, Effect.scoped);
    const [first, next] = yield* stepped({ survey: known, plan: Option.none(), rest: Option.none() });
    const later = yield* Option.match(next, {
        onNone: () => Effect.succeed([]),
        onSome: (held) => Stream.runCollect(Stream.paginate(held, (state: Held) => Effect.map(stepped(state), ([step, after]) => [[step], after] as const))),
    });
    const steps = [first, ...later];
    const copies = new Set(first.plan.sendable.flatMap(({ copies: held }) => held));
    const files = new Map(yield* Effect.forEach(first.plan.sendable, ({ hosted }) => Effect.map(writtenIn(first.plan.formats, hosted.folder, copies), (rows) => [hosted.name, rows] as const)));
    const outcomes = [...first.plan.settled, ...steps.flatMap(({ outcomes: held }) => held)];
    return { survey: first.survey, plan: first.plan, header: Array.findFirst(steps, ({ header }) => header), answered: steps.flatMap(({ answered }) => answered), outcomes: outcomes.map((row) => ({ ...row, written: files.get(row.name) ?? [] })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, installations, normalize, type Release };
