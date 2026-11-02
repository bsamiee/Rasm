import { Array, Effect, FileSystem, Match, Option, Path, Record, Schema, Stream, UndefinedOr } from 'effect';
import { ChildProcess } from 'effect/process';
import { cases, Vector } from '#/cases';
import { HarnessError } from '#/error';
import { Precondition } from '#/indesign/precondition';
import { results } from '#/results';
import { bury } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const APPLICATIONS = { '2027': 'Adobe InDesign 2027 (Beta)', '2026': 'Adobe InDesign 2026' } as const;
const JSXBIN = /(?<quote>["'])(?<literal>@JSXBIN@(?:\\.|(?!\k<quote>)[^\\])*)\k<quote>/su;
const BYTE_ORDER_MARK = /^﻿/u;
const CONTINUATION = /\\\r?\n/gu;
const ESCAPE = /\\(?<character>.)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.Literals(Record.keys(APPLICATIONS)).annotate({ description: 'InDesign release, 2027 the Beta' });
const Engine = Schema.Literals(['extendscript', 'uxpscript']);
const Kind = Schema.Literals(['run', 'call', 'probe']);
const { Batch, Call, Probe } = cases(Precondition);
const HostCase = Schema.Struct({
    name: Schema.String,
    kind: Kind,
    fn: Schema.optionalKey(Schema.String),
    script: Schema.optionalKey(Schema.String),
    code: Schema.optionalKey(Schema.String),
    lines: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.Int, Schema.Int]))),
    call: Schema.optionalKey(Schema.String),
    evaluate: Schema.optionalKey(Schema.String),
    args: Schema.Array(Schema.Json),
    precondition: Precondition,
    read: Schema.Record(Schema.String, Schema.String),
    dialogs: Schema.optionalKey(Schema.Array(Schema.Json)),
    windows: Schema.optionalKey(Schema.Array(Schema.Json)),
    confirms: Schema.optionalKey(Schema.Array(Schema.Boolean)),
    prompts: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String))),
    files: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String))),
});
const HostBatch = Schema.fromJsonString(Schema.Struct({ product: Schema.String, cases: Schema.Array(HostCase) }));
const Results = results(Engine);
const Bundle = Schema.fromJsonString(Schema.Struct({ CFBundleShortVersionString: Schema.String }));
const Literal = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const stdout = Effect.fn('stdout')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit' });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text : yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code}` }, subject: command });
}, Effect.scoped);
const engineOf = (script: string): typeof Engine.Type => (script.endsWith('.idjs') ? 'uxpscript' : 'extendscript');
const version = Effect.fn('version')(function* (host: typeof Host.Type) {
    const path = yield* Path.Path;
    const application = APPLICATIONS[host];
    const plist = path.join('/Applications', application, `${application}.app`, 'Contents', 'Info.plist');
    return (yield* Effect.flatMap(stdout('plutil', ['-convert', 'json', '-o', '-', plist]), Schema.decodeEffect(Bundle))).CFBundleShortVersionString;
});
const source = Effect.fn('source')(function* (script: string) {
    const fs = yield* FileSystem.FileSystem;
    const text = (yield* fs.readFileString(script)).replace(BYTE_ORDER_MARK, '');
    const code = engineOf(script) === 'uxpscript' ? Option.some(text) : Option.map(Option.fromNullishOr(JSXBIN.exec(text)?.groups?.literal), (literal) => literal.replace(CONTINUATION, '').replace(ESCAPE, '$<character>'));
    return Option.match(code, { onNone: () => ({ script }), onSome: (body) => ({ script, code: body }) });
});
const vectorCalls =
    ({ calls, script, lines }: NonNullable<typeof Batch.Type.vector>) =>
    (rows: typeof Vector.Type.cases) =>
        Array.getSomes(rows.map((row) => Option.map(Record.get(calls, row.fn), (call) => ({ name: row.name, fn: row.fn, call, args: row.args, ...(script === undefined ? {} : { script }), ...(lines === undefined ? {} : { lines }) }))));
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
const kindOf = Match.type<(typeof Batch.Type.cases)[number]>().pipe(
    Match.when(Schema.is(Probe), () => 'probe' as const),
    Match.when(Schema.is(Call), () => 'call' as const),
    Match.orElse(() => 'run' as const),
);
const hostCase = (product: string, batch: typeof Batch.Type) =>
    Effect.fn('hostCase')(function* (row: (typeof Batch.Type.cases)[number]) {
        const path = yield* Path.Path;
        const kind = kindOf(row);
        const missing = () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no script for the run case or batch' }, subject: row.name });
        const script = Match.value(kind).pipe(
            Match.when('probe', () => Effect.succeed(Option.none<string>())),
            Match.when('call', () => Effect.succeed(Option.fromUndefinedOr('script' in row ? row.script : undefined))),
            Match.orElse(() => Effect.asSome(Effect.fromOption(Option.fromUndefinedOr(('script' in row ? row.script : undefined) ?? batch.script), missing))),
        );
        const resolved = yield* Effect.flatMap(script, Option.match({ onNone: () => Effect.succeed({}), onSome: (relative: string) => source(path.join(product, relative)) }));
        const args = 'args' in row ? (row.args ?? []) : [];
        return { ...row, ...resolved, kind, args: args.map(bury), precondition: { ...batch.precondition, ...row.precondition }, read: { ...batch.read, ...row.read } };
    });
const normalize = Effect.fn('normalize')(function* (product: string, file: string, only: readonly string[]) {
    const { batch, rows } = yield* expand(product, file);
    const hosted = yield* Effect.forEach(
        rows.filter(({ name }) => only.length === 0 || only.includes(name)),
        hostCase(product, batch),
    );
    const engines = Array.dedupe(Array.getSomes(hosted.map((row) => Option.map(Option.fromUndefinedOr('script' in row ? row.script : undefined), engineOf))));
    const mixed = () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'cases mix ExtendScript and UXPScript' }, subject: file });
    const engine = yield* Effect.fromOption(
        Option.filter(
            Option.orElseSome(Array.head(engines), () => 'extendscript' as const),
            () => engines.length <= 1,
        ),
        mixed,
    );
    return { batch, engine, hosted };
});
const boot = Effect.fn('boot')(function* (engine: typeof Engine.Type, folder: string, input: string, output: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [quotedInput, quotedOutput] = yield* Effect.all([Schema.encodeEffect(Literal)(input), Schema.encodeEffect(Literal)(output)]);
    if (engine === 'extendscript') {
        return `$.evalFile(File(${yield* Schema.encodeEffect(Literal)(path.join(import.meta.dirname, 'engine.jsxinc'))})); harness(${quotedInput}, ${quotedOutput});`;
    }
    const file = path.join(folder, 'boot.idjs');
    const fallback = `{ application: "", version: "", engine: "uxpscript", failed: String(error && error.message || error), cases: [] }`;
    yield* fs.writeFileString(file, `${yield* fs.readFileString(path.join(import.meta.dirname, 'engine.idjs'))}\ntry { harness(${quotedInput}, ${quotedOutput}); } catch (error) { require("fs").writeFileSync(${quotedOutput}, JSON.stringify(${fallback})); }\n`);
    return `app.doScript(File(${yield* Schema.encodeEffect(Literal)(file)}), ScriptLanguage.UXPSCRIPT);`;
});
const execute = Effect.fn('execute')(function* (host: typeof Host.Type, engine: typeof Engine.Type, product: string, hosted: readonly (typeof HostCase.Type)[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped({ prefix: 'indesign-harness-' });
    const [input, output] = [path.join(folder, 'batch.json'), path.join(folder, 'results.json')];
    yield* fs.writeFileString(input, yield* Schema.encodeEffect(HostBatch)({ product, cases: hosted }));
    const script = yield* boot(engine, folder, input, output);
    yield* stdout('osascript', ['-e', 'on run argv', '-e', 'with timeout of 3000 seconds', '-e', `tell application "${APPLICATIONS[host]}"`, '-e', 'do script (item 1 of argv) language javascript', '-e', 'end tell', '-e', 'end timeout', '-e', 'end run', script]);
    return yield* Schema.decodeEffect(Results)(yield* fs.readFileString(output));
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPLICATIONS, Engine, engineOf, execute, expand, Host, HostCase, normalize, Results, version };
