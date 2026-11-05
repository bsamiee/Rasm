import { fileURLToPath, pathToFileURL } from 'node:url';
import { Lang, parse } from '@ast-grep/napi';
import { Automation } from '@rasm/applications/node';
import { uxpScript } from '@rasm/uxp/vite';
import { Parser } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Effect, FileSystem, Option, Predicate, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { build } from 'vite';
import { copied } from '#/cases/precondition';
import { HarnessError, thrown } from '#/harness-error';
import { Contract, type Held } from '#/hosts/adobe/indesign/contract';
import { precondition } from '#/hosts/adobe/indesign/precondition';
import { ADOBE, crashes, REPORTER, uxp } from '#/hosts/adobe/reports';
import { type Batch, Harness, type Instance, installation, type Live, type Services, scripted } from '#/session/harness';
import { checked, Launched, plist, printedLines } from '#/session/instance';
import { linked, type Route } from '#/session/link';
import { assembled } from '#/session/source';
import { HOME, type Rule } from '#/trace/reports';
import { located } from '../../../../../vite.shared.ts';
import manifest from './uxpscript/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const UXPSCRIPT_ROOT = fileURLToPath(new URL('uxpscript', import.meta.url));
const UXPSCRIPT_OUTPUT = located('.artifacts', UXPSCRIPT_ROOT);
const UXPSCRIPT = uxpScript(UXPSCRIPT_ROOT, Contract.engines.uxpscript.extensions[0], { build: { outDir: UXPSCRIPT_OUTPUT } });
const REVEALED = '^(?:revealInSystem|revealInBridge)$';
const REVEALING = {
    rule: {
        any: [
            { kind: 'member_expression', has: { field: 'property', regex: REVEALED, pattern: '$MEMBER' } },
            { kind: 'subscript_expression', has: { field: 'index', has: { kind: 'string_fragment', regex: REVEALED, pattern: '$MEMBER' } } },
        ],
    },
};
const ENGINE = Effect.runSync(Effect.cached(assembled(new URL('engine/main.ts', import.meta.url))));
const LOAD = ["const { pluginManager, script } = require('uxp');", 'const code = pluginManager.addPlugin(pluginManager.createPlugin(script.args[0], pluginManager.PLUGIN_TYPE_THIRD_PARTY_V2), true);', "script.setResult(code === 2 ? '' : 'addPlugin answered ' + code);"].join('\n');
const SHOW = [
    "const { pluginManager, script } = require('uxp');",
    'const [id, panel] = script.args;',
    'const plugin = Array.from(pluginManager.plugins).find((each) => each.id === id);',
    "script.setResult(plugin === undefined ? 'no plugin ' + id + ' loaded' : '');",
    'if (plugin !== undefined) Promise.resolve().then(() => plugin.showPanel(panel));',
].join('\n');

// --- [MODELS] --------------------------------------------------------------------------

const Continuing = Schema.Union([Schema.Struct({ type: Schema.Literal('AwaitExpression') }), Schema.Struct({ async: Schema.Literal(true) }), Schema.Struct({ type: Schema.Literal('MemberExpression'), property: Schema.Struct({ name: Schema.Literals(['then', 'catch', 'finally']) }) })]).annotate({
    description: 'Syntax tree node whose work runs after the script answered',
});
const Bundle = Schema.Struct({ CFBundleName: Schema.String.annotate({ description: 'InDesign or InDesign (Beta), naming the cache folder Adobe <name>' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const bundle = Effect.succeed(pathToFileURL(`${UXPSCRIPT_OUTPUT}/main.${Contract.engines.uxpscript.extensions[0]}`));
const built = Effect.tryPromise({ try: () => build({ ...UXPSCRIPT, configFile: false, logLevel: 'warn', root: UXPSCRIPT_ROOT }), catch: HarnessError.refused(manifest.name) });
const installed = Effect.gen(function* () {
    const [automation, toOpen] = yield* Effect.all([Automation, Effect.andThen(built, bundle)]);
    const [opening, rows] = yield* Effect.all([automation.opening(toOpen, ADOBE), automation.installed(ADOBE)]);
    return yield* Effect.forEach(opening, (row) => installation(row, rows, [REPORTER]));
});

// --- [SOURCE]
const continued = (code: string): Option.Option<string> =>
    Result.match(
        Result.try(() => Parser.parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true })),
        {
            onFailure: (error) => Option.some(`source does not parse: ${thrown(error)}`),
            onSuccess: (tree) => Option.map(Option.fromNullishOr(findNodeAt(tree, undefined, undefined, (_, node) => Schema.is(Continuing)(node))), ({ node }) => `UXPScript source ${code.slice(node.start, node.end)} continues after the answer`),
        },
    );
const refusal = (held: typeof Contract.Engine.Type, hosted: typeof Contract.Case.Type, binary: boolean) => {
    const { set = [], add = [], select = [], steps = [] } = hosted.precondition;
    const expressions = [
        ...Object.values(hosted.read),
        ...set.map(([target]) => target),
        ...add.map(({ to }) => to),
        ...select,
        ...Contract.Case.match(hosted, { Probe: ({ evaluate }) => [evaluate], Panel: ({ evaluate, until }) => [evaluate, ...Option.toArray(Option.fromUndefinedOr(until))], Call: ({ call }) => [call], Run: () => [] }),
    ];
    const statements = [...steps, ...Option.toArray(Option.flatMap(Option.fromUndefinedOr('source' in hosted ? hosted.source : undefined), ({ code }) => Option.liftPredicate(code, () => !binary)))];
    return held === 'uxpscript'
        ? Option.firstSomeOf([...expressions.map((expression) => continued(`(${expression});`)), ...statements.map(continued)])
        : Option.firstSomeOf([
              Option.liftPredicate(`panel case ${hosted.name} holds its precondition through the UXPScript engine, and the batch runs ExtendScript`, () => Contract.Case.isAnyOf(['Panel'])(hosted)),
              ...[...expressions, ...statements].map((text) => Option.map(Option.fromNullishOr(parse(Lang.JavaScript, text).root().find(REVEALING)?.getMatch('MEMBER')), (member) => `ExtendScript ${member.text()} activates Finder or Bridge`)),
          ]);
};

// --- [DIGESTS]
const unpacked = Effect.fn('unpacked')(function* (file: string) {
    const entries = (yield* printedLines('unzip', ['-Z1', file])).filter((entry) => entry !== 'META-INF/metadata.xml').toSorted();
    return Array.zip(entries, yield* Effect.forEach(entries, (entry) => checked('unzip', ['-p', file, entry]))).flat();
});

// --- [LOGS]
const logs = Effect.fn('logs')(function* (instance: Instance) {
    const { CFBundleName: name } = yield* plist(instance.chosen.app, Bundle);
    const [major] = String.split(instance.chosen.version, '.');
    const cache = `Library/Caches/Adobe ${name}/Version ${major}.*/*`;
    return [{ kind: 'shutdown', pattern: `${cache}/InDesign Recovery/ProtectiveShutdownLog`, root: HOME }, { kind: 'recovery', pattern: `${cache}/InDesign Recovery/{DBSharingShi,DBTmp}${instance.pid}*`, root: HOME }, uxp('Adobe InDesign*'), ...(yield* crashes(instance))] satisfies readonly Rule[];
});

// --- [TRANSPORT]
const plugins =
    (subject: string, direct: string) =>
    (live: Live, ...values: readonly string[]) =>
        Effect.flatMap(scripted(live, { direct, values, language: Contract.engines.uxpscript.language }), ({ reply }) => (Predicate.isString(reply) && String.isNonEmpty(reply) ? HarnessError.unavailable(`${subject} ${values.join(' ')}`)(reply) : Effect.void));
const route: Route = { load: plugins('plugin', LOAD), show: plugins('panel', SHOW) };
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Contract.Survey.Type, typeof Contract.Case.Type>) {
    const scripts = Array.getSomes(batch.cases.map((row) => Option.map(Option.fromUndefinedOr('source' in row ? row.source : undefined), Struct.get('script'))));
    const held = Option.isNone(batch.survey) ? 'uxpscript' : yield* Effect.fromResult(Contract.engine(scripts));
    const direct = held === 'extendscript' ? (yield* ENGINE).code : { _tag: 'file' as const, url: (yield* bundle).href };
    const link = yield* Array.match(batch.cases.filter(Contract.Case.isAnyOf(['Panel'])), { onEmpty: () => Effect.succeedNone, onNonEmpty: (rows) => Effect.asSome(linked(live, Array.dedupe(Array.map(rows, Struct.get('plugin'))), route)) });
    const call = (from: number) =>
        Schema.encodeEffect(Contract.Encoded)({ records: batch.records, label: batch.label, enumerations: Option.match(batch.survey, { onNone: () => ({}), onSome: ({ enumerations }) => enumerations }), survey: Option.isNone(batch.survey), from, cases: batch.cases }).pipe(
            Effect.flatMap((encoded) => scripted(live, { direct, values: [encoded], language: Contract.engines[held].language })),
            Effect.flatMap(({ reply }) => Contract.Reply.replied(`${held} batch`)(reply)),
        );
    const finished = Effect.fnUntraced(function* (pending: typeof Held.Type) {
        const [item, { panelled }] = yield* Effect.orDie(Effect.fromOption(Option.all([Option.filter(Array.get(batch.cases, pending.index), Contract.Case.isAnyOf(['Panel'])), link])));
        const failed = (reason: string) => Contract.Outcome.make({ name: item.name, completion: { _tag: 'Failed', reason } });
        const outcome = 'failed' in pending ? failed(pending.failed) : yield* Effect.orElseSucceed(Effect.map(panelled(item, pending.opened), Contract.Outcome.make), ({ message }) => failed(message));
        const [fs, line] = yield* Effect.all([FileSystem.FileSystem, Schema.encodeEffect(Contract.Recorded)({ _tag: 'Finished', outcome })]);
        yield* fs.writeFileString(batch.records, `${line}\n`, { flag: 'a' });
    });
    const rest = (reply: Effect.Success<ReturnType<typeof call>>): Effect.Effect<void, Error, Services> => UndefinedOr.match(reply.held, { onUndefined: () => Effect.void, onDefined: (pending) => finished(pending).pipe(Effect.andThen(call(pending.index + 1)), Effect.flatMap(rest)) });
    const header = yield* call(0);
    yield* rest(header);
    return header;
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const indesign = Harness({
    ...Contract,
    assembled: ENGINE,
    bundle,
    precondition,
    placed: copied,
    refusal,
    installed,
    launch: () => Effect.succeed(new Launched({})),
    send,
    quit: (live) => Effect.flatMap(Automation, (automation) => Effect.asVoid(automation.execute({ pid: live.pid, command: 'quit', arguments: { saving: { _tag: 'enumerator', name: 'no' } } }))),
    canonical: (file, bytes) => (file.endsWith('.idml') ? unpacked(file) : Effect.succeed([bytes])),
    logs,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { indesign };
