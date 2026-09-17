// --- [IMPORTS] -------------------------------------------------------------------------

import { load } from 'cheerio';
import { Array, Context, Crypto, DateTime, Duration, Effect, FileSystem, Filter, Layer, Match, Option, Order, Path, Record, Result, Schema, Struct, Tuple } from 'effect';
import { Tool } from 'effect/unstable/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { contract, Failure } from '../contract.ts';
import { BridgeError, classify, HostRejection, inaccessible, notDecodable } from '../errors.ts';
import { Hosts } from '../hosts.ts';
import { Jobs, processId, run } from '../jobs.ts';
import { doScript, read, reply } from '../osascript.ts';
import { AbsolutePath, DevicePath, PageIndex, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { Action, ActionName, HOUSE, script, write } from './actions.ts';
import { plan, Scope } from './preferences.ts';
import {
    autotag,
    Color,
    combine,
    envelope,
    execMenuItem,
    getFields,
    menu,
    Operation,
    openDocument,
    perFile,
    preflight,
    printProduction,
    setFields,
    state,
    tabOrder,
    tagged,
    withDocument,
} from './scripts.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Channel {
    readonly row: Hosts['acrobat'];
    readonly host: Jobs['acrobat'];
    readonly device: Schema.Codec<AbsolutePath, string>;
}

interface Optional extends Struct.Lambda {
    <S extends Schema.Constraint>(self: S): Schema.OptionFromOptionalKey<S>;
    readonly '~lambda.out': this['~lambda.in'] extends Schema.Constraint ? Schema.OptionFromOptionalKey<this['~lambda.in']> : never;
}

type Services = ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path;

type Reply<Kind extends keyof (typeof Reply)['cases']> = Effect.Effect<(typeof Reply)['cases'][Kind]['Type'], BridgeError, Services>;

// --- [TABLES] --------------------------------------------------------------------------

const _PRESETS = {
    'issue-set': { sources: [{ pattern: '*.pdf' }], bookmarks: 'filename' },
    'portfolio-print': {
        sources: [
            { pattern: 'cover.pdf', label: 'Cover' },
            { pattern: 'plans.pdf', label: 'Plans' },
            { pattern: 'elevations.pdf', label: 'Elevations' },
            { pattern: 'sections.pdf', label: 'Sections' },
            { pattern: 'details.pdf', label: 'Details' },
        ],
        bookmarks: 'label',
    },
    'review-pack': {
        sources: [
            { pattern: 'cover.pdf', label: 'Cover' },
            { pattern: 'drawings/*.pdf', label: 'Drawings' },
            { pattern: 'appendix.pdf', label: 'Appendix' },
        ],
        bookmarks: 'label',
    },
} as const;
const _FIELD_TYPES = ['text', 'button', 'combobox', 'listbox', 'checkbox', 'radiobutton', 'signature'] as const;
const _RULE_CELLS = 3;

// --- [MODELS] --------------------------------------------------------------------------

const Channel: Context.Service<Channel, Channel> = Context.Service<Channel>('AcrobatChannel');

const _optional = Struct.map(Struct.lambda<Optional>(Schema.OptionFromOptionalKey));
const _Path = Schema.OptionFromOptionalKey(AbsolutePath);
const _names = Schema.Array(Schema.String);
const _ints = Schema.Array(Schema.Int);
const _rect = Schema.Tuple([Schema.Number, Schema.Number, Schema.Number, Schema.Number]);
const _Items = Schema.Array(Schema.Struct({ label: Schema.String, export: Schema.String }));
const _Document = Schema.Struct({ path: AbsolutePath, fileName: Schema.String, numPages: Schema.Int, numFields: Schema.Int, dirty: Schema.Boolean });
const _Skipped = Schema.Array(Schema.Struct({ path: AbsolutePath, reason: Schema.TaggedUnion({ sourceMissing: {}, insertRefused: { message: Schema.String } }) }));
const _Field = Schema.Struct({
    name: Schema.String,
    type: Schema.Literals(_FIELD_TYPES),
    page: Schema.Union([Schema.Int, _ints]),
    rect: _rect,
    ..._optional({ readonly: Schema.Boolean, required: Schema.Boolean, multiline: Schema.Boolean }),
    ..._optional({ value: Schema.Json, defaultValue: Schema.Json, calcOrderIndex: Schema.Int, charLimit: Schema.Int, numItems: Schema.Int, textSize: Schema.Number, lineWidth: Schema.Number }),
    ..._optional({ textFont: Schema.String, style: Schema.String, exportValues: _names, strokeColor: Color, fillColor: Color, items: _Items }),
    ..._optional({ borderStyle: Schema.Literals(['solid', 'dashed', 'beveled', 'inset', 'underline']), alignment: Schema.Literals(['left', 'center', 'right']) }),
});
const _Counts = Schema.Struct({ numErrors: Schema.Int, numWarnings: Schema.Int, numInfos: Schema.Int, numFixed: Schema.Int, numNotFixed: Schema.Int });
const _Rules = Schema.Array(
    Schema.Struct({ category: Schema.String, name: Schema.String, status: Schema.Literals(['Needs manual check', 'Passed manually', 'Failed manually', 'Skipped', 'Passed', 'Failed']) }),
);
const Reply = Schema.Union([
    Schema.Struct({
        kind: Schema.Literal('state'),
        viewerVersion: Schema.Number,
        viewerType: Schema.String,
        language: Schema.String,
        platform: Schema.String,
        documents: Schema.Array(_Document),
        converters: _names,
    }),
    Schema.Struct({ kind: Schema.Literal('opened'), path: AbsolutePath, fileName: Schema.String, numPages: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json, tookMs: Schema.Number }),
    Schema.Struct({ kind: Schema.Literal('menu'), items: Schema.Array(Schema.Struct({ cName: Schema.String, depth: Schema.Int })) }),
    Schema.Struct({ kind: Schema.Literal('executed'), name: Schema.String }),
    Schema.Struct({
        kind: Schema.Literal('action'),
        name: ActionName,
        ran: _ints,
        perFile: Schema.Array(Schema.Struct({ path: AbsolutePath, outcome: Schema.toCodecJson(Schema.Result(AbsolutePath, BridgeError)) })),
    }),
    Schema.Struct({ kind: Schema.Literal('needsWizard'), name: ActionName, steps: _ints }),
    Schema.Struct({ kind: Schema.Literal('saved'), path: AbsolutePath, steps: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('combined'), path: AbsolutePath, numPages: Schema.Int, bookmarks: Schema.Int, skipped: _Skipped }),
    Schema.Struct({ kind: Schema.Literal('nothingInserted'), skipped: _Skipped }),
    Schema.Struct({ kind: Schema.Literal('batch'), total: Schema.Int, failed: Schema.Array(Schema.Struct({ path: AbsolutePath, error: BridgeError })) }),
    Schema.Struct({ kind: Schema.Literal('fields'), fields: Schema.Array(_Field), absent: _names }),
    Schema.Struct({
        kind: Schema.Literal('fieldsApplied'),
        applied: Schema.Array(Schema.Struct({ name: Schema.String, from: Schema.OptionFromNullOr(_Field), to: _Field })),
        rejected: Schema.Array(Schema.Struct({ name: Schema.String, reason: Schema.TaggedUnion({ fieldAbsent: {} }) })),
    }),
    Schema.Struct({ kind: Schema.Literal('tabOrder'), applied: Schema.Array(PageIndex) }),
    Schema.Struct({ kind: Schema.Literal('tagged'), numErrors: Schema.Struct({ before: Schema.Int, after: Schema.Int }), dirty: Schema.Boolean }),
    Schema.Struct({ kind: Schema.Literal('accessibility'), rules: _Rules, reportPath: AbsolutePath }),
    Schema.Struct({ kind: Schema.Literal('preflight'), ..._Counts.fields, reportPath: _Path }),
    Schema.Struct({
        kind: Schema.Literal('production'),
        applied: _ints,
        rejected: Schema.Array(Schema.Struct({ opIndex: Schema.Int, reason: Schema.Struct({ name: Schema.String, message: Schema.String }) })),
    }),
    Schema.Struct({
        kind: Schema.Literal('preferences'),
        applied: Schema.Array(Schema.Struct({ path: Schema.String, from: Schema.OptionFromNullOr(Schema.String), to: Schema.String })),
        unchanged: _names,
    }),
    Schema.Struct({ kind: Schema.Literal('hostRunning'), pid: Schema.Int }),
    Failure,
]).pipe(Schema.toTaggedUnion('kind'));
const _Format = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('plain'), charLimit: Schema.OptionFromOptionalKey(Schema.Int) }),
    Schema.Struct({ kind: Schema.Literal('number'), decimals: Schema.Int, range: Schema.OptionFromOptionalKey(Schema.Struct({ min: Schema.Number, max: Schema.Number })) }),
    Schema.Struct({ kind: Schema.Literal('percent'), decimals: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('date') }),
    Schema.Struct({ kind: Schema.Literal('time') }),
    Schema.Struct({ kind: Schema.Literal('total'), decimals: Schema.Int, operands: Schema.NonEmptyArray(Schema.String) }),
]);
const _Specs = Schema.NonEmptyArray(
    Schema.Struct({
        name: Schema.String,
        ..._optional({ create: Schema.Struct({ type: Schema.Literals(_FIELD_TYPES), page: PageIndex, rect: _rect }), format: _Format, items: _Items, exportValues: _names, caption: Schema.String }),
        ..._optional({
            mouseUp: Schema.Union([Schema.Literal('resetForm'), Schema.Struct({ submitForm: Schema.String })]),
            readOnly: Schema.Boolean,
            required: Schema.Boolean,
            defaultValue: Schema.String,
        }),
    }),
);
const _Source = Schema.Struct({ pattern: Schema.String, ..._optional({ range: Schema.Struct({ start: PageIndex, end: PageIndex }), label: Schema.String }) });
const _Bookmarks = Schema.Literals(['filename', 'label', 'none']);
const _PresetName = Schema.Literals(Struct.keys(_PRESETS));
const _presets = Schema.decodeSync(Schema.Record(_PresetName, Schema.Struct({ sources: Schema.NonEmptyArray(_Source), bookmarks: _Bookmarks })))(_PRESETS);
const _Inputs = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('files'), files: Schema.NonEmptyArray(AbsolutePath) }),
    Schema.Struct({ kind: Schema.Literal('glob'), folder: AbsolutePath, pattern: Schema.String }),
]).pipe(Schema.toTaggedUnion('kind'));
const _Combine = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('preset'), name: _PresetName, folder: AbsolutePath }),
    Schema.Struct({ kind: Schema.Literal('sources'), sources: Schema.NonEmptyArray(_Source), folder: AbsolutePath, bookmarks: _Bookmarks, output: AbsolutePath }),
]).pipe(Schema.toTaggedUnion('kind'));
const _Profile = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('name'), name: Schema.String }),
    Schema.Struct({
        kind: Schema.Literal('standard'),
        standard: Schema.Literals([
            'PDF/X-1a:2001',
            'PDF/X-1a:2003',
            'PDF/X-3:2002',
            'PDF/X-3:2003',
            'PDF/X-4:2008',
            'PDF/X-4p:2008',
            'PDF/X-5g:2008',
            'PDF/X-5n:2008',
            'PDF/X-5pg:2008',
            'PDF/A-1a:2005',
            'PDF/A-1b:2005',
            'PDF/E-1:2008',
        ]),
    }),
]).pipe(Schema.toTaggedUnion('kind'));
const _Pages = Schema.Union([Schema.Struct({ kind: Schema.Literal('all') }), Schema.Struct({ kind: Schema.Literal('pages'), pages: Schema.NonEmptyArray(PageIndex) })]).pipe(
    Schema.toTaggedUnion('kind'),
);
const _PreflightInput = Schema.Struct({ document: AbsolutePath, profile: _Profile, fixups: Schema.Boolean, report: _Path, save: _Path });
const _BatchInput = Schema.Struct({ action: ActionName, folder: AbsolutePath, pattern: Schema.String, output: AbsolutePath });

// --- [CHANNEL] -------------------------------------------------------------------------

const _call = <S extends Schema.Codec<unknown, unknown>>(channel: Channel, timeoutMs: number, schema: S, code: string): Effect.Effect<S['Type'], BridgeError, Services> =>
    run(channel.host, timeoutMs, () => read(channel.row.id, channel.row.bundleId, timeoutMs, doScript(envelope(code)), Option.none())).pipe(
        Effect.flatMap((text) => Effect.mapError(Schema.decodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(schema, HostRejection))))(text), notDecodable(channel.row.id, text))),
        Effect.flatMap((outcome) => Effect.fromResult(Result.mapError(outcome, (rejection) => BridgeError.cases.hostThrew.make({ host: channel.row.id, rejection, autocorrections: Option.none() })))),
    );

const _document = <S extends Schema.Codec<unknown, unknown>>(channel: Channel, document: AbsolutePath, schema: S, code: string): Effect.Effect<S['Type'], BridgeError, Services> =>
    _call(channel, TIMEOUT_MS, schema, withDocument(Schema.encodeSync(channel.device)(document), document, code));

const _command = (channel: Channel, command: ChildProcess.StandardCommand): Effect.Effect<string, BridgeError, ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.mapError(reply(command), (exit) => classify(channel.row.id, exit));

const _matches = (channel: Channel, folder: AbsolutePath, pattern: string, exclude: readonly string[]): Effect.Effect<readonly string[], BridgeError, FileSystem.FileSystem> =>
    FileSystem.FileSystem.use((fs) => Effect.map(Effect.mapError(fs.glob(pattern, { root: folder, exclude }), inaccessible(channel.row.id)), Array.sort(Order.String)));

// --- [ACTIONS] -------------------------------------------------------------------------

const _perFile = Effect.fnUntraced(function* (channel: Channel, body: (output: string) => string, source: AbsolutePath, output: AbsolutePath) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const outcome = yield* Effect.result(
        Effect.andThen(
            Effect.mapError(fs.makeDirectory(path.dirname(output), { recursive: true }), inaccessible(channel.row.id)),
            Effect.as(_call(channel, TIMEOUT_MS, Schema.Struct({ saved: Schema.Literal(true) }), perFile(source, body(output))), output),
        ),
    );
    return { path: source, outcome };
});

const _runAction = (channel: Channel, name: ActionName, targets: readonly (readonly [AbsolutePath, AbsolutePath])[]): Reply<'action' | 'needsWizard'> => {
    const steps = Array.flatMap(HOUSE[name].groups, Struct.get('steps'));
    const runnable = Array.filterMap(steps, (step, index) => Result.map(script(step), (render) => ({ index, render })));
    const body = (output: string): string =>
        Array.join(
            Array.map(runnable, ({ render }) => render(output)),
            ' ',
        );
    return Array.match(
        Array.filterMap(steps, (step, index) => (step.op === 'command' && Option.isSome(step.items) ? Result.succeed(index) : Result.failVoid)),
        {
            onNonEmpty: (indices): Reply<'action' | 'needsWizard'> => Effect.succeed({ kind: 'needsWizard', name, steps: indices }),
            onEmpty: () =>
                Effect.map(
                    Effect.forEach(targets, ([source, output]) => _perFile(channel, body, source, output)),
                    (outcomes) => ({ kind: 'action' as const, name, ran: Array.map(runnable, Struct.get('index')), perFile: outcomes }),
                ),
        },
    );
};

const _targets = (channel: Channel, inputs: (typeof _Inputs)['Type'], output: Option.Option<AbsolutePath>): Effect.Effect<readonly (readonly [AbsolutePath, AbsolutePath])[], BridgeError, Services> =>
    Effect.flatMap(Path.Path, (path) => {
        const target = (folder: string, name: string): readonly [AbsolutePath, AbsolutePath] =>
            Tuple.make(
                AbsolutePath.make(path.join(folder, name)),
                AbsolutePath.make(
                    path.join(
                        Option.getOrElse(output, () => folder),
                        name,
                    ),
                ),
            );
        return _Inputs.match(inputs, {
            files: ({ files }) => Effect.succeed(Array.map(files, (source) => target(path.dirname(source), path.basename(source)))),
            glob: ({ folder, pattern }) =>
                Effect.map(
                    _matches(channel, folder, pattern, []),
                    Array.map((match) => target(folder, match)),
                ),
        });
    });

const _batch = (channel: Channel, { action, folder, pattern, output }: (typeof _BatchInput)['Type']): Reply<'batch' | 'needsWizard'> =>
    Effect.map(
        Effect.flatMap(_targets(channel, { kind: 'glob', folder, pattern }, Option.some(output)), (targets) => _runAction(channel, action, targets)),
        (ran) =>
            ran.kind === 'needsWizard'
                ? ran
                : {
                      kind: 'batch' as const,
                      total: Array.length(ran.perFile),
                      failed: Array.filterMap(ran.perFile, ({ path, outcome }) => Result.map(Result.flip(outcome), (error) => ({ path, error }))),
                  },
    );

// --- [COMBINE] -------------------------------------------------------------------------

const _insert = (path: Path.Path, folder: AbsolutePath, bookmarks: (typeof _Bookmarks)['Type'], source: (typeof _Source)['Type'], match: string): Record<string, Schema.Json> => ({
    path: AbsolutePath.make(path.join(folder, match)),
    ...Record.getSomes<string, Schema.Json>({
        start: Option.map(source.range, Struct.get('start')),
        end: Option.map(source.range, Struct.get('end')),
        label: Match.value(bookmarks).pipe(
            Match.when('filename', () => Option.some(path.basename(match, path.extname(match)))),
            Match.when('label', () => source.label),
            Match.when('none', () => Option.none()),
            Match.exhaustive,
        ),
    }),
});

const _combine = Effect.fnUntraced(function* (
    channel: Channel,
    folder: AbsolutePath,
    sources: Array.NonEmptyReadonlyArray<(typeof _Source)['Type']>,
    bookmarks: (typeof _Bookmarks)['Type'],
    output: AbsolutePath,
) {
    const path = yield* Path.Path;
    const expanded = yield* Effect.forEach(sources, (source) =>
        Effect.map(
            _matches(channel, folder, source.pattern, [path.relative(folder, output)]),
            Array.match({
                onEmpty: () => Result.fail({ path: AbsolutePath.make(path.join(folder, source.pattern)), reason: { _tag: 'sourceMissing' as const } }),
                onNonEmpty: (listed) => Result.succeed(Array.map(listed, (match) => _insert(path, folder, bookmarks, source, match))),
            }),
        ),
    );
    const [missing, found] = Array.separate(expanded);
    return yield* Array.match(Array.flatten(found), {
        onEmpty: (): Reply<'combined' | 'nothingInserted'> => Effect.succeed({ kind: 'nothingInserted', skipped: missing }),
        onNonEmpty: (rows) =>
            Effect.map(_call(channel, TIMEOUT_MS, Schema.Union([Reply.cases.combined, Reply.cases.nothingInserted]), combine(rows, output)), (combined) => ({
                ...combined,
                skipped: [...missing, ...combined.skipped],
            })),
    });
});

const _request = (channel: Channel, request: (typeof _Combine)['Type']): Reply<'combined' | 'nothingInserted'> =>
    _Combine.match(request, {
        preset: ({ name, folder }) =>
            Effect.flatMap(Effect.all([Path.Path, DateTime.withCurrentZoneLocal(DateTime.nowInCurrentZone)]), ([path, now]) =>
                _combine(channel, folder, _presets[name].sources, _presets[name].bookmarks, AbsolutePath.make(path.join(folder, `${name}-${DateTime.formatIsoDate(now)}.pdf`))),
            ),
        sources: ({ sources, folder, bookmarks, output }) => _combine(channel, folder, sources, bookmarks, output),
    });

// --- [DOCUMENTS] -----------------------------------------------------------------------

const _report = (channel: Channel, report: AbsolutePath): Reply<'accessibility'> =>
    Effect.flatMap(
        FileSystem.FileSystem.use((fs) => Effect.mapError(fs.readFileString(report), inaccessible(channel.row.id))),
        (html) => {
            const $ = load(html);
            const rows = Array.filterMap($('table tr').toArray(), (tr) => {
                const cells = $(tr).children('td');
                return cells.length === _RULE_CELLS
                    ? Result.succeed({ category: $(tr).prevAll('tr:has(td.cattitle)').first().find('h3').text(), name: cells.first().text(), status: cells.eq(1).text() })
                    : Result.failVoid;
            });
            return Effect.map(Effect.mapError(Schema.decodeUnknownEffect(_Rules)(rows), notDecodable(channel.row.id, rows)), (rules) => ({
                kind: 'accessibility' as const,
                rules,
                reportPath: report,
            }));
        },
    );

const _preflight = Effect.fnUntraced(function* (channel: Channel, { document, profile, fixups, report, save }: (typeof _PreflightInput)['Type']) {
    const value = yield* _document(
        channel,
        document,
        Schema.Struct({ ..._Counts.fields, report: Schema.OptionFromOptionalKey(Schema.String) }),
        _Profile.match(profile, {
            name: ({ name }) => preflight('getProfileByName', name, fixups, Option.isSome(report), save),
            standard: ({ standard }) => preflight('createComplianceProfile', standard, fixups, Option.isSome(report), save),
        }),
    );
    const reportPath = yield* Effect.transposeOption(
        Option.map(Option.all([report, value.report]), ([target, xml]) =>
            FileSystem.FileSystem.use((fs) => Effect.as(Effect.mapError(fs.writeFileString(target, xml), inaccessible(channel.row.id)), target)),
        ),
    );
    return { kind: 'preflight' as const, ...Struct.omit(value, ['report']), reportPath };
});

const _preferences = Effect.fnUntraced(function* (channel: Channel, request: Scope) {
    const pid = yield* processId(channel.host);
    if (Option.isSome(pid)) {
        return { kind: 'hostRunning' as const, pid: pid.value };
    }
    const plist = request.scope === 'user' ? channel.row.prefsFolder : `/Library/Preferences/${channel.row.bundleId}.plist`;
    const xml = yield* Effect.when(
        _command(channel, ChildProcess.make('plutil', ['-convert', 'xml1', '-o', '-', plist])),
        FileSystem.FileSystem.use((fs) => Effect.mapError(fs.exists(plist), inaccessible(channel.row.id))),
    );
    const writes = plan(request, xml);
    const sudo = request.scope === 'machine';
    const buddy = (command: string): ChildProcess.StandardCommand =>
        sudo ? ChildProcess.make('sudo', ['/usr/libexec/PlistBuddy', '-c', command, plist]) : ChildProcess.make('/usr/libexec/PlistBuddy', ['-c', command, plist]);
    const ownership = sudo ? [ChildProcess.make('sudo', ['chown', 'root:wheel', plist]), ChildProcess.make('sudo', ['chmod', '755', plist])] : [];
    yield* Effect.forEach([...Array.map(Array.flatMap(writes, Struct.get('commands')), buddy), ...ownership, ChildProcess.make('defaults', ['read', plist])], (command) => _command(channel, command), {
        discard: true,
    });
    const [unchanged, applied] = Array.partition(
        writes,
        Filter.fromPredicate((row) => row.commands.length > 0),
    );
    return { kind: 'preferences' as const, applied: Array.map(applied, ({ path, from, to }) => ({ path, from, to })), unchanged: Array.map(unchanged, Struct.get('path')) };
});

// --- [TOOLS] ---------------------------------------------------------------------------

const _row = contract(Channel, [ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path]);

const layer: Layer.Layer<never, never, Hosts | Jobs | Services> = Layer.provide(
    Layer.mergeAll(
        _row('acrobat_get_state', 'Returns viewer properties, open documents, and `saveAs` converters', Tool.EmptyParams, Reply.cases.state, true, (channel) =>
            _call(channel, TIMEOUT_MS, Schema.Struct({ ...Reply.cases.state.fields, documents: Schema.Array(Schema.Struct({ ..._Document.fields, path: channel.device })) }), state),
        ),
        _row(
            'acrobat_open_document',
            'Opens `path` as a document, without a window when `hidden`',
            Schema.Struct({ path: AbsolutePath, hidden: Schema.Boolean }),
            Reply.cases.opened,
            false,
            (channel, { path, hidden }) => _call(channel, TIMEOUT_MS, Reply.cases.opened, openDocument(path, hidden)),
        ),
        _row(
            'acrobat_execute',
            'Runs `code` as a function body in Acrobat JavaScript and returns its value as JSON. `app`, `Doc`, `Field`, and `Preflight` members are available',
            Schema.Struct({ code: Schema.String, timeoutMs: Schema.OptionFromOptionalKey(TimeoutMs) }),
            Reply.cases.value,
            false,
            (channel, { code, timeoutMs }) =>
                Effect.map(
                    Effect.timed(
                        _call(
                            channel,
                            Option.getOrElse(timeoutMs, () => TIMEOUT_MS),
                            Schema.Json,
                            code,
                        ),
                    ),
                    ([took, value]) => ({ kind: 'value' as const, value, tookMs: Duration.toMillis(took) }),
                ),
        ),
        _row('acrobat_list_menu_items', 'Returns every menu item `cName` with its depth in the menu tree', Tool.EmptyParams, Reply.cases.menu, true, (channel) =>
            _call(channel, TIMEOUT_MS, Reply.cases.menu, menu),
        ),
        _row(
            'acrobat_exec_menu_item',
            'Executes menu item `name` when `app.listMenuItems` lists it, on open `document` when given',
            Schema.Struct({ name: Schema.String, document: _Path }),
            Reply.cases.executed,
            false,
            (channel, { name, document }) =>
                Option.match(document, {
                    onNone: () => _call(channel, TIMEOUT_MS, Reply.cases.executed, execMenuItem(name, false)),
                    onSome: (path) => _document(channel, path, Reply.cases.executed, execMenuItem(name, true)),
                }),
        ),
        _row(
            'acrobat_run_action',
            'Runs action `name` on each input file and saves each result under `output`, or over its source without `output`. Actions with command items run in Action Wizard alone and return `needsWizard`',
            Schema.Struct({ name: ActionName, inputs: _Inputs, output: _Path }),
            Schema.Union([Reply.cases.action, Reply.cases.needsWizard]),
            false,
            (channel, { name, inputs, output }) => Effect.flatMap(_targets(channel, inputs, output), (targets) => _runAction(channel, name, targets)),
        ),
        _row(
            'acrobat_write_action',
            'Writes `action` as `<name>.sequ` to Acrobat Sequences folder. Acrobat lists it under Use guided actions at next launch',
            Schema.Struct({ action: Action }),
            Reply.cases.saved,
            false,
            (channel, { action }) =>
                Effect.map(Effect.mapError(write(channel.row, action), inaccessible(channel.row.id)), (path) => ({
                    kind: 'saved' as const,
                    path,
                    steps: Array.length(Array.flatMap(action.groups, Struct.get('steps'))),
                })),
        ),
        _row(
            'acrobat_combine',
            'Combines PDFs matching each source glob under `folder` into one document with bookmarks by file name or label. Presets save as `<preset>-<date>.pdf` in `folder`',
            Schema.Struct({ request: _Combine }),
            Schema.Union([Reply.cases.combined, Reply.cases.nothingInserted]),
            false,
            (channel, { request }) => _request(channel, request),
        ),
        _row(
            'acrobat_batch',
            'Runs action `action` on every file matching `pattern` under `folder` and saves each result at its relative path under `output`',
            _BatchInput,
            Schema.Union([Reply.cases.batch, Reply.cases.needsWizard]),
            false,
            _batch,
        ),
        _row(
            'acrobat_get_fields',
            'Returns every form field of open `document`, or fields in `names`, with properties each field type supports',
            Schema.Struct({ document: AbsolutePath, names: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)) }),
            Reply.cases.fields,
            true,
            (channel, { document, names }) => _document(channel, document, Reply.cases.fields, getFields(names)),
        ),
        _row(
            'acrobat_set_fields',
            'Sets form fields of open `document`, creates fields with `create`, applies format scripts per `format.kind`, and returns each field before and after',
            Schema.Struct({ document: AbsolutePath, fields: _Specs, save: _Path }),
            Reply.cases.fieldsApplied,
            false,
            (channel, { document, fields, save }) => _document(channel, document, Reply.cases.fieldsApplied, setFields(Schema.encodeSync(_Specs)(fields), save)),
        ),
        _row(
            'acrobat_set_tab_order',
            'Sets tab order `order` on every page of open `document`, or on listed pages',
            Schema.Struct({ document: AbsolutePath, pages: _Pages, order: Schema.Literals(['rows', 'columns', 'structure']), save: _Path }),
            Reply.cases.tabOrder,
            false,
            (channel, { document, pages, order, save }) =>
                _document(channel, document, Reply.cases.tabOrder, tabOrder(_Pages.match(pages, { all: () => Option.none(), pages: (listed) => Option.some(listed.pages) }), order, save)),
        ),
        _row(
            'acrobat_autotag',
            'Tags open `document` through `Adobe:MakeAccessible` and returns PDF/UA-1 preflight error counts before and after',
            Schema.Struct({ document: AbsolutePath, save: _Path }),
            Reply.cases.tagged,
            false,
            (channel, { document, save }) =>
                Effect.flatMap(_document(channel, document, Schema.Struct({ before: Schema.Int }), autotag), ({ before }) => _document(channel, document, Reply.cases.tagged, tagged(before, save))),
        ),
        _row(
            'acrobat_check_accessibility',
            'Returns one row per rule with category and status from the HTML report `AccCheck:DoCheck` wrote at `report`',
            Schema.Struct({ report: AbsolutePath }),
            Reply.cases.accessibility,
            true,
            (channel, { report }) => _report(channel, report),
        ),
        _row(
            'acrobat_preflight',
            'Runs a preflight profile by name or compliance standard on open `document` and returns result counts. `fixups` applies profile fixups, `report` writes the XML report to that path',
            _PreflightInput,
            Reply.cases.preflight,
            false,
            _preflight,
        ),
        _row(
            'acrobat_print_production',
            'Applies `operations` to open `document` in order and returns indices applied and rejected',
            Schema.Struct({ document: AbsolutePath, operations: Schema.NonEmptyArray(Operation), save: _Path }),
            Reply.cases.production,
            false,
            (channel, { document, operations, save }) => _document(channel, document, Reply.cases.production, printProduction(operations, save)),
        ),
        _row(
            'acrobat_set_preferences',
            'Writes every preference row of `scope`, or rows in `rows`, to user or machine plist. Running Acrobat returns `hostRunning`, machine scope writes through `sudo`',
            Schema.Struct({ request: Scope }),
            Schema.Union([Reply.cases.preferences, Reply.cases.hostRunning]),
            false,
            (channel, { request }) => _preferences(channel, request),
        ),
    ),
    Layer.effect(
        Channel,
        Effect.map(Effect.all([Hosts, Jobs]), ([hosts, jobs]) => ({ row: hosts.acrobat, host: jobs.acrobat, device: DevicePath(hosts.acrobat.startupVolume) })),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
