// --- [IMPORTS] -------------------------------------------------------------------------

import { load } from 'cheerio';
import { Array, Context, Crypto, DateTime, Duration, Effect, FileSystem, Filter, identity, Layer, Match, Option, Order, Path, Record, Result, Schema, Struct, Tuple } from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { type ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { answering, host, succeeded, tool, Value } from '../contract.ts';
import { BridgeError, exited, HostRejection, inaccessible, notDecodable } from '../errors.ts';
import { Hosts, installed } from '../hosts.ts';
import { type Host, Jobs, processId, run } from '../jobs.ts';
import { doScript, read, reply } from '../osascript.ts';
import { AbsolutePath, DevicePath, OptionalPath, OptionalString, PageIndex, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { Action, ActionName, HOUSE, script, write } from './actions.ts';
import { defaults, plan, Scope, settle } from './preferences.ts';
import {
    autotag,
    combine,
    envelope,
    execMenuItem,
    Field,
    Fields,
    getFields,
    menu,
    NAMES,
    Operation,
    openDocument,
    perFile,
    preflight,
    printProduction,
    type Sources,
    setFields,
    state,
    tabOrder,
    tagged,
    withDocument,
} from './scripts.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Channel {
    readonly acrobat: Hosts['acrobat'];
    readonly host: Host;
}

type Services = ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path;

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

// --- [MODELS] --------------------------------------------------------------------------

const Channel: Context.Service<Channel, Channel> = Context.Service<Channel>('AcrobatChannel');

const _Ints = Schema.Array(Schema.Int);
const _Strings = Schema.Array(Schema.String);
const _Skipped = Schema.Array(Schema.Struct({ path: AbsolutePath, reason: Schema.TaggedUnion({ sourceMissing: {}, insertRefused: { message: Schema.String } }) }));
const _Counts = Schema.Struct({ numErrors: Schema.Int, numWarnings: Schema.Int, numInfos: Schema.Int, numFixed: Schema.Int, numNotFixed: Schema.Int });
const _Source = Schema.Struct({ pattern: Schema.String, range: Schema.OptionFromOptionalKey(Schema.Struct({ start: PageIndex, end: PageIndex })), label: OptionalString });
const _Bookmarks = Schema.Literals(['filename', 'label', 'none']);
const _Preset = Schema.Literals(Struct.keys(_PRESETS));
const _presets = Schema.decodeSync(Schema.Record(_Preset, Schema.Struct({ sources: Schema.NonEmptyArray(_Source), bookmarks: _Bookmarks })))(_PRESETS);

const _Inputs = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('files'), files: Schema.NonEmptyArray(AbsolutePath) }),
    Schema.Struct({ kind: Schema.Literal('glob'), folder: AbsolutePath, pattern: Schema.String }),
]).pipe(Schema.toTaggedUnion('kind'));

const _Combine = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('preset'), name: _Preset, folder: AbsolutePath }),
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

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path]);

const _toolkit = Toolkit.make(
    _tool(
        'acrobat_get_state',
        'Returns viewer properties, open documents, and `saveAs` converters',
        Tool.EmptyParams,
        Schema.Struct({
            kind: Schema.Literal('state'),
            viewerVersion: Schema.Number,
            viewerType: Schema.String,
            language: Schema.String,
            platform: Schema.String,
            documents: Schema.Array(Schema.Struct({ path: AbsolutePath, fileName: Schema.String, numPages: Schema.Int, numFields: Schema.Int, dirty: Schema.Boolean })),
            converters: _Strings,
        }),
        true,
    ),
    _tool(
        'acrobat_open_document',
        'Opens `path` as a document, without a window when `hidden`',
        Schema.Struct({ path: AbsolutePath, hidden: Schema.Boolean }),
        Schema.Struct({ kind: Schema.Literal('opened'), path: AbsolutePath, fileName: Schema.String, numPages: Schema.Int }),
        false,
    ),
    _tool(
        'acrobat_execute',
        'Runs `code` as a function body in Acrobat JavaScript and returns its value as JSON. `app`, `Doc`, `Field`, and `Preflight` members are available',
        Schema.Struct({ code: Schema.String, timeoutMs: TimeoutMs.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TIMEOUT_MS))) }),
        Value,
        false,
    ),
    _tool(
        'acrobat_list_menu_items',
        'Returns every menu item `cName` with its depth in the menu tree',
        Tool.EmptyParams,
        Schema.Struct({ kind: Schema.Literal('menu'), items: Schema.Array(Schema.Struct({ cName: Schema.String, depth: Schema.Int })) }),
        true,
    ),
    _tool(
        'acrobat_exec_menu_item',
        'Executes menu item `name` when `app.listMenuItems` lists it, on open `document` when given',
        Schema.Struct({ name: Schema.String, document: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('executed'), name: Schema.String }),
        false,
    ),
    _tool(
        'acrobat_run_action',
        'Runs action `name` over a file list or a glob under a folder and saves each result under `output`, or over its source without `output`. Every input answers its own saved path or its error. An action holding command items, a prompting command, or a pausing instruction runs in Action Wizard alone and answers the indices of those steps instead',
        Schema.Struct({ name: ActionName, inputs: _Inputs, output: OptionalPath }),
        Schema.Union([
            Schema.Struct({
                kind: Schema.Literal('action'),
                name: ActionName,
                ran: _Ints,
                perFile: Schema.Array(Schema.Struct({ path: AbsolutePath, outcome: Schema.toCodecJson(Schema.Result(AbsolutePath, BridgeError)) })),
            }),
            Schema.Struct({ kind: Schema.Literal('needsWizard'), name: ActionName, steps: _Ints }),
        ]),
        false,
    ),
    _tool(
        'acrobat_write_action',
        'Writes `action` as `<name>.sequ` to Acrobat Sequences folder. Acrobat lists it under Use guided actions at next launch',
        Schema.Struct({ action: Action }),
        Schema.Struct({ kind: Schema.Literal('saved'), path: AbsolutePath, steps: Schema.Int }),
        false,
    ),
    _tool(
        'acrobat_combine',
        'Combines PDFs matching each source glob under `folder` into one document with bookmarks by file name or label. A preset saves into `folder` under the preset name and the current date',
        Schema.Struct({ request: _Combine }),
        Schema.Union([
            Schema.Struct({ kind: Schema.Literal('combined'), path: AbsolutePath, numPages: Schema.Int, bookmarks: Schema.Int, skipped: _Skipped }),
            Schema.Struct({ kind: Schema.Literal('nothingInserted'), skipped: _Skipped }),
        ]),
        false,
    ),
    _tool(
        'acrobat_get_fields',
        'Returns every form field of open `document`, or fields in `names`, with properties each field type supports',
        Schema.Struct({ document: AbsolutePath, names: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)) }),
        Schema.Struct({ kind: Schema.Literal('fields'), fields: Schema.Array(Field), absent: _Strings }),
        true,
    ),
    _tool(
        'acrobat_set_fields',
        'Sets form fields of open `document`, creates fields with `create`, applies format scripts per `format.kind`, and returns each field before and after',
        Schema.Struct({ document: AbsolutePath, fields: Fields, save: OptionalPath }),
        Schema.Struct({
            kind: Schema.Literal('fieldsApplied'),
            applied: Schema.Array(Schema.Struct({ name: Schema.String, from: Schema.OptionFromNullOr(Field), to: Field })),
            rejected: Schema.Array(Schema.Struct({ name: Schema.String, reason: Schema.TaggedUnion({ fieldAbsent: {} }) })),
        }),
        false,
    ),
    _tool(
        'acrobat_set_tab_order',
        'Sets tab order `order` on every page of open `document`, or on the pages in `pages`',
        Schema.Struct({
            document: AbsolutePath,
            pages: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(PageIndex)),
            order: Schema.Literals(['rows', 'columns', 'structure']),
            save: OptionalPath,
        }),
        Schema.Struct({ kind: Schema.Literal('tabOrder'), applied: Schema.Array(PageIndex) }),
        false,
    ),
    _tool(
        'acrobat_autotag',
        `Tags open \`document\` through the \`${NAMES.makeAccessible}\` menu item and returns the \`${NAMES.pdfUa}\` preflight error counts before and after`,
        Schema.Struct({ document: AbsolutePath, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('tagged'), numErrors: Schema.Struct({ before: Schema.Int, after: Schema.Int }), dirty: Schema.Boolean }),
        false,
    ),
    _tool(
        'acrobat_check_accessibility',
        `Returns one row per rule with its category, status, description, and reference link from the HTML report \`${NAMES.accessibilityCheck}\` wrote at \`report\``,
        Schema.Struct({ report: AbsolutePath }),
        Schema.Struct({
            kind: Schema.Literal('accessibility'),
            rules: Schema.Array(
                Schema.Struct({
                    category: Schema.String,
                    name: Schema.String,
                    status: Schema.Literals(['Needs manual check', 'Passed manually', 'Failed manually', 'Skipped', 'Passed', 'Failed']),
                    description: Schema.String,
                    reference: OptionalString,
                }),
            ),
            reportPath: AbsolutePath,
        }),
        true,
    ),
    _tool(
        'acrobat_preflight',
        'Runs a preflight profile by name or compliance standard on open `document` and returns result counts. `fixups` applies profile fixups, `report` writes the XML report to that path',
        Schema.Struct({ document: AbsolutePath, profile: _Profile, fixups: Schema.Boolean, report: OptionalPath, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('preflight'), ..._Counts.fields, reportPath: OptionalPath }),
        false,
    ),
    _tool(
        'acrobat_print_production',
        'Applies `operations` to open `document` in order and returns indices applied and rejected',
        Schema.Struct({ document: AbsolutePath, operations: Schema.NonEmptyArray(Operation), save: OptionalPath }),
        Schema.Struct({
            kind: Schema.Literal('production'),
            applied: _Ints,
            rejected: Schema.Array(Schema.Struct({ opIndex: Schema.Int, reason: Schema.Struct({ name: Schema.String, message: Schema.String }) })),
        }),
        false,
    ),
    _tool(
        'acrobat_set_preferences',
        'Writes every preference row of `scope`, or the rows in `rows`, through `defaults import` and reads the domain back, so each applied row carries whether the readback holds its target. Machine scope writes through `sudo`, leaves the file root-owned and world-readable, and reads it back unprivileged; a running Acrobat answers its process id instead',
        Schema.Struct({ request: Scope }),
        Schema.Union([
            Schema.Struct({
                kind: Schema.Literal('preferences'),
                applied: Schema.Array(Schema.Struct({ path: Schema.String, from: Schema.OptionFromNullOr(Schema.Boolean), to: Schema.Boolean, verified: Schema.Boolean })),
                unchanged: _Strings,
            }),
            Schema.Struct({ kind: Schema.Literal('hostRunning'), pid: Schema.Int }),
        ]),
        false,
    ),
);

// --- [CHANNEL] -------------------------------------------------------------------------

const _call = <S extends Schema.Codec<unknown, unknown>>(channel: Channel, timeoutMs: number, schema: S, code: string): Effect.Effect<S['Type'], BridgeError, Services> =>
    Effect.flatMap(installed('acrobat', channel.acrobat), (row) =>
        run(channel.host, timeoutMs, () => read(row.id, row.bundleId, timeoutMs, doScript(envelope(code)), Option.none())).pipe(
            Effect.flatMap((text) => Effect.mapError(Schema.decodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(schema, HostRejection))))(text), notDecodable(row.id, text))),
            Effect.flatMap((answer) => Effect.fromResult(Result.mapError(answer, (rejection) => BridgeError.cases.hostThrew.make({ host: row.id, rejection, autocorrections: Option.none() })))),
        ),
    );

const _document = <S extends Schema.Codec<unknown, unknown>>(channel: Channel, document: AbsolutePath, schema: S, code: string): Effect.Effect<S['Type'], BridgeError, Services> =>
    Effect.flatMap(installed('acrobat', channel.acrobat), (row) => _call(channel, TIMEOUT_MS, schema, withDocument(Schema.encodeSync(DevicePath(row.startupVolume))(document), document, code)));

const _command = (channel: Channel, command: ChildProcess.StandardCommand): Effect.Effect<string, BridgeError, ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.mapError(reply(command), exited(channel.host.id));

const _matches = (channel: Channel, folder: AbsolutePath, pattern: string, exclude: readonly string[]): Effect.Effect<readonly string[], BridgeError, FileSystem.FileSystem> =>
    FileSystem.FileSystem.use((fs) => Effect.map(Effect.mapError(fs.glob(pattern, { root: folder, exclude }), inaccessible(channel.host.id)), Array.sort(Order.String)));

// --- [ACTIONS] -------------------------------------------------------------------------

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

const _runAction = Effect.fnUntraced(function* (channel: Channel, name: ActionName, targets: readonly (readonly [AbsolutePath, AbsolutePath])[]) {
    const steps = Array.flatMap(HOUSE[name].groups, Struct.get('steps'));
    const wizard = Array.filterMap(steps, (step, index) =>
        Match.value(step).pipe(
            Match.whenOr({ op: 'command', items: Option.isSome }, { op: 'command', prompt: Option.contains(true) }, { op: 'instruction', pauseBefore: true }, () => Result.succeed(index)),
            Match.orElse(() => Result.failVoid),
        ),
    );
    if (Array.isReadonlyArrayNonEmpty(wizard)) {
        return { kind: 'needsWizard' as const, name, steps: wizard };
    }
    const runnable = Array.filterMap(steps, (step, index) => Result.map(script(step), (render) => ({ index, render })));
    const body = (output: string): string =>
        Array.join(
            Array.map(runnable, ({ render }) => render(output)),
            ' ',
        );
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const outcomes = yield* Effect.forEach(targets, ([source, output]) =>
        Effect.mapError(fs.makeDirectory(path.dirname(output), { recursive: true }), inaccessible(channel.host.id)).pipe(
            Effect.andThen(_call(channel, TIMEOUT_MS, Schema.Null, perFile(source, body(output)))),
            Effect.andThen(
                Effect.filterOrFail(Effect.mapError(fs.exists(output), inaccessible(channel.host.id)), identity, () =>
                    BridgeError.cases.fileNotAccessible.make({ host: channel.host.id, path: Option.some(output), reason: 'NotFound' }),
                ),
            ),
            Effect.as(output),
            Effect.result,
            Effect.map((outcome) => ({ path: source, outcome })),
        ),
    );
    return { kind: 'action' as const, name, ran: Array.map(runnable, Struct.get('index')), perFile: outcomes };
});

// --- [COMBINE] -------------------------------------------------------------------------

const _combine = Effect.fnUntraced(function* (
    channel: Channel,
    folder: AbsolutePath,
    sources: Array.NonEmptyReadonlyArray<(typeof _Source)['Type']>,
    bookmarks: (typeof _Bookmarks)['Type'],
    output: AbsolutePath,
) {
    const path = yield* Path.Path;
    const label = Match.value(bookmarks).pipe(
        Match.withReturnType<(source: (typeof _Source)['Type'], match: string) => Option.Option<string>>(),
        Match.when(
            'filename',
            () =>
                (_source, match): Option.Option<string> =>
                    Option.some(path.basename(match, path.extname(match))),
        ),
        Match.when('label', () => Struct.get('label')),
        Match.when('none', () => (): Option.Option<string> => Option.none()),
        Match.exhaustive,
    );
    const expanded = yield* Effect.forEach(sources, (source) => Effect.map(_matches(channel, folder, source.pattern, [path.relative(folder, output)]), (matches) => Tuple.make(source, matches)), {
        concurrency: 'unbounded',
    });
    const [empty, filled] = Array.partition(
        expanded,
        Filter.fromPredicate(([, matches]) => Array.isReadonlyArrayNonEmpty(matches)),
    );
    const skipped = Array.map(empty, ([source]) => ({ path: AbsolutePath.make(path.join(folder, source.pattern)), reason: { _tag: 'sourceMissing' as const } }));
    const rows = Array.flatMap(filled, ([source, matches]) =>
        Array.map(matches, (match): (typeof Sources)['Type'][number] => ({
            path: AbsolutePath.make(path.join(folder, match)),
            start: Option.map(source.range, Struct.get('start')),
            end: Option.map(source.range, Struct.get('end')),
            label: label(source, match),
        })),
    );
    return yield* Array.match(rows, {
        onEmpty: () => Effect.succeed({ kind: 'nothingInserted' as const, skipped }),
        onNonEmpty: (inserted) =>
            Effect.map(_call(channel, TIMEOUT_MS, succeeded(_toolkit.tools.acrobat_combine), combine(inserted, output)), (combined) => ({ ...combined, skipped: [...skipped, ...combined.skipped] })),
    });
});

const _request = (channel: Channel, request: (typeof _Combine)['Type']): ReturnType<typeof _combine> =>
    _Combine.match(request, {
        preset: ({ name, folder }) =>
            Effect.flatMap(Effect.all([Path.Path, DateTime.withCurrentZoneLocal(DateTime.nowInCurrentZone)]), ([path, now]) =>
                _combine(channel, folder, _presets[name].sources, _presets[name].bookmarks, AbsolutePath.make(path.join(folder, `${name}-${DateTime.formatIsoDate(now)}.pdf`))),
            ),
        sources: ({ sources, folder, bookmarks, output }) => _combine(channel, folder, sources, bookmarks, output),
    });

// --- [DOCUMENTS] -----------------------------------------------------------------------

const _report = Effect.fnUntraced(function* (channel: Channel, report: AbsolutePath) {
    const html = yield* FileSystem.FileSystem.use((fs) => Effect.mapError(fs.readFileString(report), inaccessible(channel.host.id)));
    const $ = load(html);
    const rules = yield* Effect.filterOrFail(
        Effect.succeed(
            Array.flatMap($('tr:has(td.cattitle)').toArray(), (heading) => {
                const category = $(heading).find('h3').text();
                return Array.map($(heading).nextUntil('tr:has(td.cattitle)').filter('tr:has(td)').toArray(), (row): Record<string, Schema.Json> => {
                    const cells = $(row).children('td');
                    return {
                        category,
                        name: cells.eq(0).text(),
                        status: cells.eq(1).text(),
                        description: cells.eq(2).text(),
                        ...Record.getSomes<string, Schema.Json>({ reference: Option.fromNullishOr(cells.eq(0).find('a').attr('href')) }),
                    };
                });
            }),
        ),
        Array.isReadonlyArrayNonEmpty,
        (rows) => notDecodable(channel.host.id, rows)('no rule row under a td.cattitle heading'),
    );
    return yield* Effect.mapError(
        Schema.decodeUnknownEffect(succeeded(_toolkit.tools.acrobat_check_accessibility))({ kind: 'accessibility', rules, reportPath: report }),
        notDecodable(channel.host.id, rules),
    );
});

const _preflight = Effect.fnUntraced(function* (channel: Channel, { document, profile, fixups, report, save }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_preflight']>) {
    const fs = yield* FileSystem.FileSystem;
    const value = yield* _document(
        channel,
        document,
        Schema.Struct({ ..._Counts.fields, report: OptionalString }),
        _Profile.match(profile, {
            name: ({ name }) => preflight('getProfileByName', name, fixups, Option.isSome(report), save),
            standard: ({ standard }) => preflight('createComplianceProfile', standard, fixups, Option.isSome(report), save),
        }),
    );
    const reportPath = yield* Effect.transposeOption(
        Option.map(Option.all([report, value.report]), ([target, xml]) => Effect.as(Effect.mapError(fs.writeFileString(target, xml), inaccessible(channel.host.id)), target)),
    );
    return { kind: 'preflight' as const, ...Struct.omit(value, ['report']), reportPath };
});

const _preferences = Effect.fnUntraced(function* (channel: Channel, request: Scope) {
    const pid = yield* processId(channel.host);
    if (Option.isSome(pid)) {
        return { kind: 'hostRunning' as const, pid: pid.value };
    }
    const row = yield* installed('acrobat', channel.acrobat);
    const readback = _command(channel, defaults(request, row.bundleId, Option.none()));
    const before = plan(request, yield* readback);
    const after = yield* Effect.when(
        Effect.andThen(
            _command(channel, defaults(request, row.bundleId, Option.some(before.tree))),
            Effect.andThen(
                Effect.forEach(settle(request, row.bundleId), (command) => _command(channel, command), { discard: true }),
                readback,
            ),
        ),
        Effect.succeed(Array.isReadonlyArrayNonEmpty(before.applied)),
    );
    const held = Option.map(after, (xml) => plan(request, xml).unchanged);
    return {
        kind: 'preferences' as const,
        applied: Array.map(before.applied, (entry) => ({ ...entry, verified: Option.exists(held, Array.contains(entry.path)) })),
        unchanged: before.unchanged,
    };
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Hosts | Jobs | Services> = Layer.provide(
    host(_toolkit, Channel, (channel) =>
        answering(_toolkit, {
            [_toolkit.tools.acrobat_get_state.name]: () =>
                Effect.flatMap(installed('acrobat', channel.acrobat), (found) => {
                    const answer = succeeded(_toolkit.tools.acrobat_get_state);
                    return _call(
                        channel,
                        TIMEOUT_MS,
                        Schema.Struct({ ...answer.fields, documents: Schema.Array(Schema.Struct({ ...answer.fields.documents.value.fields, path: DevicePath(found.startupVolume) })) }),
                        state,
                    );
                }),
            [_toolkit.tools.acrobat_open_document.name]: ({ path, hidden }) =>
                Effect.flatMap(installed('acrobat', channel.acrobat), (found) =>
                    _call(channel, TIMEOUT_MS, Schema.Struct({ ...succeeded(_toolkit.tools.acrobat_open_document).fields, path: DevicePath(found.startupVolume) }), openDocument(path, hidden)),
                ),
            [_toolkit.tools.acrobat_execute.name]: ({ code, timeoutMs }) =>
                Effect.map(Effect.timed(_call(channel, timeoutMs, Schema.Json, code)), ([took, value]) =>
                    Value.make({ kind: 'value', value, tookMs: Duration.toMillis(took), autocorrections: Option.none(), undo: Option.none() }),
                ),
            [_toolkit.tools.acrobat_list_menu_items.name]: () => _call(channel, TIMEOUT_MS, succeeded(_toolkit.tools.acrobat_list_menu_items), menu),
            [_toolkit.tools.acrobat_exec_menu_item.name]: ({ name, document }) =>
                Option.match(document, {
                    onNone: () => _call(channel, TIMEOUT_MS, succeeded(_toolkit.tools.acrobat_exec_menu_item), execMenuItem(name, false)),
                    onSome: (path) => _document(channel, path, succeeded(_toolkit.tools.acrobat_exec_menu_item), execMenuItem(name, true)),
                }),
            [_toolkit.tools.acrobat_run_action.name]: ({ name, inputs, output }) => Effect.flatMap(_targets(channel, inputs, output), (targets) => _runAction(channel, name, targets)),
            [_toolkit.tools.acrobat_write_action.name]: ({ action }) =>
                Effect.map(
                    Effect.flatMap(installed('acrobat', channel.acrobat), (found) => Effect.mapError(write(found, action), inaccessible(found.id))),
                    (path) => ({ kind: 'saved' as const, path, steps: Array.length(Array.flatMap(action.groups, Struct.get('steps'))) }),
                ),
            [_toolkit.tools.acrobat_combine.name]: ({ request }) => _request(channel, request),
            [_toolkit.tools.acrobat_get_fields.name]: ({ document, names }) => _document(channel, document, succeeded(_toolkit.tools.acrobat_get_fields), getFields(names)),
            [_toolkit.tools.acrobat_set_fields.name]: ({ document, fields, save }) => _document(channel, document, succeeded(_toolkit.tools.acrobat_set_fields), setFields(fields, save)),
            [_toolkit.tools.acrobat_set_tab_order.name]: ({ document, pages, order, save }) =>
                _document(channel, document, succeeded(_toolkit.tools.acrobat_set_tab_order), tabOrder(pages, order, save)),
            [_toolkit.tools.acrobat_autotag.name]: ({ document, save }) =>
                Effect.flatMap(_document(channel, document, Schema.Struct({ before: Schema.Int }), autotag), ({ before }) =>
                    _document(channel, document, succeeded(_toolkit.tools.acrobat_autotag), tagged(before, save)),
                ),
            [_toolkit.tools.acrobat_check_accessibility.name]: ({ report }) => _report(channel, report),
            [_toolkit.tools.acrobat_preflight.name]: (input) => _preflight(channel, input),
            [_toolkit.tools.acrobat_print_production.name]: ({ document, operations, save }) =>
                _document(channel, document, succeeded(_toolkit.tools.acrobat_print_production), printProduction(operations, save)),
            [_toolkit.tools.acrobat_set_preferences.name]: ({ request }) => _preferences(channel, request),
        }),
    ),
    Layer.effect(
        Channel,
        Effect.map(Effect.all([Hosts, Jobs]), ([hosts, jobs]) => ({ acrobat: hosts.acrobat, host: jobs.acrobat })),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
