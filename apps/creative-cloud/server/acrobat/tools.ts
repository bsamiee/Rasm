// --- [IMPORTS] -------------------------------------------------------------------------

import { DOMImplementation, DOMParser, Document, Element, MIME_TYPE, NAMESPACE, onWarningStopParsing, XMLSerializer } from '@xmldom/xmldom';
import { load } from 'cheerio';
import {
    Array,
    Context,
    Crypto,
    DateTime,
    Duration,
    Effect,
    Equal,
    FileSystem,
    Filter,
    flow,
    identity,
    Layer,
    Match,
    Option,
    Order,
    Path,
    Record,
    Result,
    Schema,
    SchemaGetter,
    SchemaIssue,
    Struct,
    Tuple,
} from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { type ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { answering, type Handlers, register, succeeded, tool, Value } from '../contract.ts';
import { compare, Drift } from '../drift.ts';
import { BridgeError, exited, HostRejection, inaccessible, notDecodable } from '../errors.ts';
import { Hosts, installed } from '../hosts.ts';
import { type Host, Jobs, processId, run } from '../jobs.ts';
import { doScript, read, reply } from '../osascript.ts';
import { AbsolutePath, DevicePath, OptionalNumber, OptionalPath, OptionalString, PageIndex, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { Action, ActionName, HOUSE, write } from './actions.ts';
import { literal, PAGE_ROTATIONS } from './native.ts';
import { defaults, plan, Scope, settle } from './preferences.ts';
import { Color, call, compiled, envelope, Field, Fields, matches, NAMES, Operation, printProduction, type Sources, setFields, withDocument } from './scripts.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Channel {
    readonly acrobat: Hosts['acrobat'];
    readonly host: Host;
    readonly program: string;
}

type Services = ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path;

// --- [TABLES] --------------------------------------------------------------------------

const _RDF_NAMESPACE = 'http://www.w3.org/1999/02/22-rdf-syntax-ns#';

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

const _Profile = Schema.Union([Schema.Struct({ kind: Schema.Literal('name'), name: Schema.String }), Schema.Struct({ kind: Schema.Literal('standard'), standard: Schema.String })]).pipe(
    Schema.toTaggedUnion('kind'),
);

const _Range = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('all') }),
    Schema.Struct({ kind: Schema.Literal('pages'), pages: Schema.NonEmptyArray(PageIndex).check(Schema.isUnique()) }),
    Schema.Struct({ kind: Schema.Literal('span'), start: PageIndex, end: PageIndex }).check(Schema.makeFilter(({ start, end }) => start <= end || 'Page range must end at or after its start')),
]);
const _Target = Schema.Union([Schema.Struct({ kind: Schema.Literal('page'), page: PageIndex }), Schema.Struct({ kind: Schema.Literal('label'), label: Schema.NonEmptyString })]);
const _Query = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('literal'), value: Schema.NonEmptyString, matchCase: Schema.Boolean }),
    Schema.Struct({ kind: Schema.Literal('regex'), expression: Schema.toCodecJson(Schema.RegExp) }),
]).pipe(
    Schema.decodeTo(Schema.RegExp, {
        decode: SchemaGetter.transform((query) => (query.kind === 'regex' ? query.expression : new RegExp(RegExp.escape(query.value), query.matchCase ? 'u' : 'iu'))),
        encode: SchemaGetter.transform((expression) => ({ kind: 'regex' as const, expression })),
    }),
);
const _Quad = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]);
const _Matches = Schema.Array(Schema.Struct({ page: PageIndex, wordIndex: PageIndex, wordIndices: _Ints, text: Schema.String, quads: Schema.Array(_Quad) }));
const _Names = Schema.Array(Schema.NonEmptyString).check(Schema.isUnique());
const _After = Schema.Int.check(Schema.isGreaterThanOrEqualTo(-1));
const _Length = Schema.Finite.check(Schema.isGreaterThan(0));
const _LayerState = Schema.Struct({ state: Schema.Boolean, initState: Schema.Boolean, locked: Schema.Boolean });
const _XmpPacket = Schema.String.pipe(
    Schema.decodeTo(
        Schema.Struct({
            kind: Schema.Literal('packet'),
            document: Schema.declare((value): value is Document => value instanceof Document),
            rdf: Schema.declare((value): value is Element => value instanceof Element),
        }),
        {
            decode: SchemaGetter.transformEffect(
                Effect.fnUntraced(function* (packet) {
                    const document = yield* Effect.try({
                        try: () =>
                            packet.length === 0
                                ? new DOMImplementation().createDocument(_RDF_NAMESPACE, 'rdf:RDF')
                                : new DOMParser({ onError: onWarningStopParsing }).parseFromString(packet, MIME_TYPE.XML_APPLICATION),
                        catch: (cause) => new SchemaIssue.InvalidValue({ message: 'Malformed XMP XML packet', cause }, packet),
                    });
                    const roots = Array.fromIterable(document.getElementsByTagNameNS(_RDF_NAMESPACE, 'RDF'));
                    const rdf = yield* Effect.fromOption(
                        Option.filter(Array.head(roots), () => roots.length === 1),
                        () => new SchemaIssue.InvalidValue({ message: 'XMP packet must contain exactly one RDF element' }, packet),
                    );
                    return { kind: 'packet' as const, document, rdf };
                }),
            ),
            encode: SchemaGetter.transform(({ document }) => new XMLSerializer().serializeToString(document)),
        },
    ),
);
const _XmpProperty = Schema.Struct({
    namespace: Schema.NonEmptyString,
    name: Schema.NonEmptyString,
    value: Schema.Union([
        Schema.String,
        Schema.Struct({ kind: Schema.Literals(['Bag', 'Seq']), items: _Strings }),
        Schema.Struct({ kind: Schema.Literal('Alt'), items: Schema.Record(Schema.NonEmptyString, Schema.String) }),
    ]),
}).pipe(
    Schema.decodeTo(
        Schema.declare((value): value is Element => value instanceof Element),
        {
            decode: SchemaGetter.transformEffect((property) =>
                Effect.try({
                    try: () => {
                        const document = new DOMImplementation().createDocument(null, '');
                        const node = document.createElementNS(property.namespace, property.name);
                        if (typeof property.value === 'string') {
                            node.textContent = property.value;
                            return node;
                        }
                        const container = node.appendChild(document.createElementNS(_RDF_NAMESPACE, `rdf:${property.value.kind}`));
                        const items =
                            property.value.kind === 'Alt'
                                ? Array.map(Record.toEntries(property.value.items), ([language, text]) => ({ text, language: Option.some(language) }))
                                : Array.map(property.value.items, (text) => ({ text, language: Option.none<string>() }));
                        Array.map(
                            Array.sort(
                                items,
                                Order.mapInput(Order.Boolean, (item: (typeof items)[number]) => !Option.contains(item.language, 'x-default')),
                            ),
                            ({ text, language }) => {
                                const item = document.createElementNS(_RDF_NAMESPACE, 'rdf:li');
                                item.textContent = text;
                                if (Option.isSome(language)) {
                                    item.setAttributeNS(NAMESPACE.XML, 'xml:lang', language.value);
                                }
                                return container.appendChild(item);
                            },
                        );
                        return node;
                    },
                    catch: (cause) => new SchemaIssue.InvalidValue({ message: 'Invalid XMP property', cause }, property),
                }),
            ),
            encode: SchemaGetter.forbiddenEncoding,
        },
    ),
    Schema.refine((node): node is Element & { readonly namespaceURI: string; readonly localName: string } => node.namespaceURI !== null && node.localName !== null, {
        expected: 'A namespace-qualified XMP property',
    }),
);
const _Text = Schema.Struct({
    kind: Schema.Literal('text'),
    pages: Schema.Array(
        Schema.Struct({
            page: PageIndex,
            label: Schema.String,
            words: Schema.Array(Schema.Struct({ wordIndex: PageIndex, word: Schema.String, quads: Schema.Array(_Quad) })),
        }),
    ),
});
const _Requests = Schema.Struct({
    pageBoxes: Schema.Struct({
        pages: _Range,
        boxes: Schema.NonEmptyArray(
            Schema.Struct({
                box: Schema.Literals(['Trim', 'Bleed', 'Art', 'Crop', 'Media']),
                set: Schema.Union([
                    Schema.Struct({ kind: Schema.Literal('rect'), rect: Field.fields.rect }),
                    Schema.Struct({ kind: Schema.Literal('inset'), inset: Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0)) }),
                    Schema.Struct({ kind: Schema.Literal('remove') }),
                ]),
            }),
        ),
    }),
    pageLabels: Schema.Struct({
        request: Schema.Struct({
            kind: Schema.Literal('sections'),
            sections: Schema.NonEmptyArray(
                Schema.Struct({ page: PageIndex, style: Schema.Literals(['D', 'R', 'r', 'A', 'a']), prefix: Schema.String, start: Schema.Int.check(Schema.isGreaterThan(0)) }),
            ),
        }),
    }),
    organize: Schema.Struct({
        operations: Schema.NonEmptyArray(
            Schema.Union([
                Schema.Struct({
                    op: Schema.Literal('replace'),
                    at: _Target,
                    from: Schema.Struct({ path: AbsolutePath, start: PageIndex, end: PageIndex }).check(
                        Schema.makeFilter(({ start, end }) => start <= end || 'Source range must end at or after its start'),
                    ),
                }),
                Schema.Struct({ op: Schema.Literal('extract'), pages: _Range, to: AbsolutePath }),
                Schema.Struct({ op: Schema.Literal('move'), page: PageIndex, after: _After }),
                Schema.Struct({ op: Schema.Literal('rotate'), pages: _Range, degrees: Schema.Literals(Record.values(PAGE_ROTATIONS)) }),
                Schema.Struct({ op: Schema.Literal('delete'), pages: _Range }),
                Schema.Struct({
                    op: Schema.Literal('blank'),
                    after: _After,
                    width: _Length,
                    height: _Length,
                }),
            ]),
        ),
    }),
    metadata: Schema.Struct({
        info: Schema.Record(Schema.NonEmptyString, Schema.String),
        xmp: Schema.OptionFromOptionalKey(
            Schema.Union([
                _XmpPacket,
                Schema.Struct({
                    kind: Schema.Literal('merge'),
                    properties: Schema.NonEmptyArray(_XmpProperty).check(
                        Schema.makeFilter(
                            (properties) =>
                                Array.dedupeWith(properties, (left, right) => left.namespaceURI === right.namespaceURI && left.localName === right.localName).length === properties.length ||
                                'Each XMP namespace and property name must occur once',
                        ),
                    ),
                }),
            ]),
        ),
    }),
    layers: Schema.Struct({
        layers: Schema.Array(
            Schema.Struct({
                name: Schema.NonEmptyString,
                ...Record.map(_LayerState.fields, Schema.OptionFromOptionalKey),
                intent: Schema.OptionFromOptionalKey(Schema.Literals(['view', 'design'])),
            }),
        ),
        order: Schema.OptionFromOptionalKey(_Names),
    }),
    attachments: Schema.Struct({
        attachments: Schema.Array(Schema.Struct({ name: Schema.NonEmptyString, path: AbsolutePath })),
        remove: _Names,
    }),
    comments: Schema.Struct({
        request: Schema.Union([
            Schema.Struct({ kind: Schema.Literal('list') }),
            Schema.Struct({ kind: Schema.Literal('export'), path: AbsolutePath, fields: Schema.Boolean }),
            Schema.Struct({ kind: Schema.Literal('import'), path: AbsolutePath, save: OptionalPath }),
        ]),
    }),
    stamp: Schema.Struct({
        appearance: Schema.NonEmptyString,
        pages: _Range,
        rect: Field.fields.rect,
        opacity: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }))),
        rotation: OptionalNumber,
        flatten: Schema.Boolean,
    }),
    export: Schema.Struct({ converter: Schema.NonEmptyString, output: AbsolutePath }),
    encrypt: Schema.Struct({ policy: Schema.NonEmptyString }),
});

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path]);

const _toolkit = Toolkit.make(
    _tool(
        'acrobat_get_state',
        'Returns viewer properties, open documents, and `saveAs` converters. With baseline, compares those keyed native fields and includes a structured drift report',
        Schema.Struct({ baseline: Schema.OptionFromOptionalKey(Schema.JsonObject) }),
        Schema.Struct({
            kind: Schema.Literal('state'),
            viewerVersion: Schema.Number,
            viewerType: Schema.String,
            language: Schema.String,
            platform: Schema.String,
            documents: Schema.Array(Schema.Struct({ path: AbsolutePath, fileName: Schema.String, numPages: Schema.Int, numFields: Schema.Int, dirty: Schema.Boolean })),
            converters: _Strings,
            drift: Schema.OptionFromOptionalKey(Drift),
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
        'Executes the native menu command `name`, on open `document` when given. Acrobat validates command availability, including commands omitted from its menu listing',
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
            rejected: Schema.Array(
                Schema.Struct({ name: Schema.String, reason: Schema.TaggedUnion({ fieldAbsent: {}, fieldTypeConflict: { type: Schema.String, properties: Schema.Array(Schema.String) } }) }),
            ),
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
        'acrobat_list_preflight',
        'Lists native Preflight libraries and the profiles of the selected library. Restores the original default library after reading',
        Schema.Struct({ library: OptionalString }),
        Schema.Struct({
            kind: Schema.Literal('preflightLibraries'),
            libraries: Schema.Array(Schema.Struct({ name: Schema.String, description: Schema.String, locked: Schema.Boolean, isDefault: Schema.Boolean })),
            profiles: Schema.Array(Schema.Struct({ name: Schema.String, description: Schema.String, hasFixups: Schema.Boolean, hasChecks: Schema.Boolean })),
            defaultLibrary: Schema.String,
            selectedLibrary: Schema.String,
        }),
        true,
    ),
    _tool(
        'acrobat_preflight',
        'Runs a native profile or supported compliance standard with optional fixups and XML report. An optional library is scoped and restored. Embedded audit information describes the native stored audit, which may predate this run',
        Schema.Struct({ document: AbsolutePath, profile: _Profile, library: OptionalString, fixups: Schema.Boolean, report: OptionalPath, save: OptionalPath }),
        Schema.Struct({
            kind: Schema.Literal('preflight'),
            ..._Counts.fields,
            reportPath: OptionalPath,
            auditTrail: Schema.OptionFromOptionalKey(
                Schema.Struct({
                    name: Schema.String,
                    creator: Schema.String,
                    creatorVersion: Schema.String,
                    formatVersion: Schema.String,
                    results: Schema.String,
                    description: Schema.String,
                    executedDate: Schema.String,
                    fingerprint: Schema.String,
                }),
            ),
        }),
        false,
    ),
    _tool(
        'acrobat_print_production',
        'Applies production operations in order and returns applied and rejected indices. Watermarks and redactions require a distinct output file to preserve the source',
        Schema.Struct({ document: AbsolutePath, operations: Schema.NonEmptyArray(Operation), save: OptionalPath }).check(
            Schema.makeFilter(
                ({ document, operations, save }) =>
                    !Array.some(operations, (operation) => operation.op === 'addWatermarkFromText' || operation.op === 'applyRedactions') ||
                    Option.exists(save, (output) => output !== document) ||
                    'Watermarks and redactions require a save path that differs from the source document',
            ),
        ),
        Schema.Struct({
            kind: Schema.Literal('production'),
            applied: _Ints,
            rejected: Schema.Array(Schema.Struct({ opIndex: Schema.Int, reason: Schema.Struct({ name: Schema.String, message: Schema.String }) })),
        }),
        false,
    ),
    _tool(
        'acrobat_find_text',
        'Finds literal or regular-expression matches across complete page text, including matches spanning words. Returns intersecting word indices and native quadrilaterals',
        Schema.Struct({ document: AbsolutePath, query: _Query, pages: _Range }),
        Schema.Struct({ kind: Schema.Literal('matches'), matches: _Matches }),
        true,
    ),
    _tool(
        'acrobat_set_links',
        'Finds text spans and creates links to page indices, unique page labels, or URLs. Preserves native word geometry for multiline and rotated text',
        Schema.Struct({
            document: AbsolutePath,
            rules: Schema.NonEmptyArray(
                Schema.Struct({
                    match: _Query,
                    target: Schema.Union([..._Target.members, Schema.Struct({ kind: Schema.Literal('url'), url: Schema.toCodecJson(Schema.URL) })]),
                    border: Schema.Boolean,
                }),
            ),
            pages: _Range,
            save: OptionalPath,
        }),
        Schema.Struct({
            kind: Schema.Literal('links'),
            added: Schema.Array(
                Schema.Struct({
                    page: PageIndex,
                    text: Schema.String,
                    target: Schema.Union([..._Target.members, Schema.Struct({ kind: Schema.Literal('url'), url: Schema.String })]),
                    rectangles: Schema.Array(Field.fields.rect),
                }),
            ),
        }),
        false,
    ),
    _tool(
        'acrobat_redact',
        'Finds text spans, marks and applies native redactions, then scans again to report remaining matches. Saves to a different file and preserves the source file',
        Schema.Struct({
            document: AbsolutePath,
            terms: Schema.NonEmptyArray(_Query),
            pages: _Range,
            overlay: Schema.OptionFromOptionalKey(Schema.Struct({ text: Schema.String, repeat: Schema.Boolean, alignment: Schema.Literals([0, 1, 2]), fill: Color })),
            keepMarks: Schema.Boolean,
            save: AbsolutePath,
        }),
        Schema.Struct({
            kind: Schema.Literal('redacted'),
            marks: Schema.Array(Schema.Struct({ page: PageIndex, wordIndex: PageIndex, text: Schema.String, quads: Schema.Array(_Quad) })),
            applied: Schema.Boolean,
            remaining: Schema.Int,
        }),
        false,
    ),
    _tool(
        'acrobat_set_page_boxes',
        'Sets or removes page boxes on selected pages, or derives a box by insetting each page’s Media box in points. Returns every box before and after',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.pageBoxes.fields, save: OptionalPath }),
        Schema.Struct({
            kind: Schema.Literal('pageBoxes'),
            applied: Schema.Array(Schema.Struct({ page: PageIndex, box: _Requests.fields.pageBoxes.fields.boxes.value.fields.box, before: Field.fields.rect, after: Field.fields.rect })),
        }),
        false,
    ),
    _tool(
        'acrobat_set_page_labels',
        'Applies numbered sections with optional prefixes and returns the resulting label of every page. Supports decimal, Roman, and alphabetic styles exposed by Acrobat',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.pageLabels.fields, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('pageLabels'), labels: _Strings }),
        false,
    ),
    _tool(
        'acrobat_organize_pages',
        'Replaces, extracts, moves, rotates, deletes, or inserts blank pages in order. Page indices refer to the document state at each operation; after -1 inserts before the first page',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.organize.fields, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('organized'), applied: _Ints, numPages: Schema.Int, labels: _Strings, rotations: _Ints }),
        false,
    ),
    _tool(
        'acrobat_set_metadata',
        'Writes document information and optional XMP. Accepts a complete packet or a namespace-aware merge of text, Bag, Seq, and language alternatives; merge preserves unrelated metadata. Explicit information keys take precedence over shared XMP properties. Returns native readback',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.metadata.fields, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('metadata'), info: Schema.Record(Schema.String, Schema.Json), xmp: Schema.String, xmpLength: Schema.Int }),
        false,
    ),
    _tool(
        'acrobat_set_layers',
        'Sets optional content visibility, initial visibility, locking, intent, and optional layer order. Returns changed states and missing names',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.layers.fields, save: OptionalPath }),
        Schema.Struct({
            kind: Schema.Literal('layers'),
            applied: Schema.Array(
                Schema.Struct({
                    name: Schema.String,
                    from: _LayerState,
                    to: _LayerState,
                }),
            ),
            absent: _Strings,
        }),
        false,
    ),
    _tool(
        'acrobat_attach',
        'Imports named file attachments, removes requested attachments, and returns actual native names, sizes, and MIME types',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.attachments.fields, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('attachments'), dataObjects: Schema.Array(Schema.Struct({ name: Schema.String, size: Schema.Int, mimeType: Schema.String })) }),
        false,
    ),
    _tool(
        'acrobat_comments',
        'Lists annotations, exports annotations and optionally fields to XFDF, or imports XFDF and reports annotation counts before and after',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.comments.fields }),
        Schema.Union([
            Schema.Struct({
                kind: Schema.Literal('comments'),
                rows: Schema.Array(
                    Schema.Struct({
                        name: Schema.String,
                        type: Schema.String,
                        page: PageIndex,
                        author: Schema.String,
                        contents: Schema.String,
                        modDate: Schema.String,
                        rect: Field.fields.rect,
                        inReplyTo: OptionalString,
                    }),
                ),
            }),
            Schema.Struct({ kind: Schema.Literal('commentsExported'), path: AbsolutePath, count: Schema.Int }),
            Schema.Struct({ kind: Schema.Literal('commentsImported'), before: Schema.Int, after: Schema.Int }),
        ]),
        false,
    ),
    _tool(
        'acrobat_stamp',
        'Places the native stamp appearance on selected pages at the requested rectangle and optionally flattens those pages',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.stamp.fields, save: OptionalPath }),
        Schema.Struct({ kind: Schema.Literal('stamped'), annots: Schema.Array(Schema.Struct({ page: PageIndex, name: Schema.String, appearance: Schema.String })) }),
        false,
    ),
    _tool(
        'acrobat_export',
        'Exports a copy through a converter returned by acrobat_get_state. Returns every file actually generated, including individual page images and companion assets, with any file delivery failures',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.export.fields }),
        Schema.Union([
            Schema.Struct({
                kind: Schema.Literal('exported'),
                converter: Schema.String,
                files: Schema.Array(AbsolutePath),
                rejected: Schema.Array(Schema.Struct({ path: AbsolutePath, reason: BridgeError })),
            }),
            Schema.Struct({ kind: Schema.Literal('converterAbsent'), available: _Strings }),
        ]),
        false,
    ),
    _tool(
        'acrobat_encrypt',
        'Encrypts with an installed native security policy and saves to the requested path. Reports available policy names when the policy is absent',
        Schema.Struct({ document: AbsolutePath, ..._Requests.fields.encrypt.fields, save: AbsolutePath }).check(
            Schema.makeFilter(({ document, save }) => document !== save || 'Encryption output must differ from the source document'),
        ),
        Schema.Union([
            Schema.Struct({ kind: Schema.Literal('encrypted'), policyId: Schema.String, securityHandler: Schema.String }),
            Schema.Struct({ kind: Schema.Literal('policyAbsent'), available: _Strings }),
            Schema.Struct({ kind: Schema.Literal('encryptionRefused'), code: Schema.Literals([1, 2]), message: Schema.String }),
        ]),
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
        run(channel.host, timeoutMs, () => read(row.id, row.bundlePath, timeoutMs, doScript(envelope(channel.program, code)), Option.none())).pipe(
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

const _preserveSource = Effect.fnUntraced(function* (channel: Channel, document: AbsolutePath, output: AbsolutePath) {
    const fs = yield* FileSystem.FileSystem;
    const exists = yield* Effect.mapError(fs.exists(output), inaccessible(channel.host.id));
    if (!exists) {
        return;
    }
    const files = yield* Effect.mapError(
        Effect.all({ source: fs.stat(document), target: fs.stat(output), sourcePath: fs.realPath(document), targetPath: fs.realPath(output) }, { concurrency: 'unbounded' }),
        inaccessible(channel.host.id),
    );
    const sameFile =
        files.sourcePath === files.targetPath || (files.source.dev === files.target.dev && Option.exists(Option.all([files.source.ino, files.target.ino]), ([source, target]) => source === target));
    yield* Effect.fail(BridgeError.cases.fileNotAccessible.make({ host: channel.host.id, path: Option.some(output), reason: 'AlreadyExists' })).pipe(Effect.when(Effect.succeed(sameFile)));
});

// --- [ACTIONS] -------------------------------------------------------------------------

const _targets = (channel: Channel, inputs: (typeof _Inputs)['Type'], output: Option.Option<AbsolutePath>): Effect.Effect<readonly (readonly [AbsolutePath, AbsolutePath])[], BridgeError, Services> =>
    Effect.flatMap(Path.Path, (path) => {
        const target = (folder: string, name: string): readonly [AbsolutePath, AbsolutePath] =>
            Tuple.make(
                AbsolutePath.make(path.resolve(folder, name)),
                AbsolutePath.make(
                    path.resolve(
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
            Match.whenOr({ op: 'command', items: Option.isSome }, { op: 'command', prompt: Option.contains(true) }, { op: 'instruction', pauseBefore: true }, () => index),
            Match.result,
        ),
    );
    if (Array.isReadonlyArrayNonEmpty(wizard)) {
        return { kind: 'needsWizard' as const, name, steps: wizard };
    }
    const runnable = Array.filterMap(Schema.encodeSync(Action.fields.groups.value.fields.steps)(steps), (step, index) =>
        step.op === 'instruction' || step.op === 'separator' ? Result.failVoid : Result.succeed({ step, index }),
    );
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const notAccessible = inaccessible(channel.host.id);
    const conflicts = Record.filter(
        Array.groupBy(targets, ([, output]) => output),
        (sources) => sources.length > 1,
    );
    const outcomes = yield* Effect.forEach(targets, ([source, output]) =>
        Effect.fromOption(
            Option.liftPredicate(output, (destination) => !Record.has(conflicts, destination)),
            () => BridgeError.cases.fileNotAccessible.make({ host: channel.host.id, path: Option.some(output), reason: 'AlreadyExists' }),
        ).pipe(
            Effect.andThen(Effect.mapError(fs.makeDirectory(path.dirname(output), { recursive: true }), notAccessible)),
            Effect.andThen(
                _call(
                    channel,
                    TIMEOUT_MS,
                    Schema.Null,
                    `runAction(${literal(source)}, function () { ${Array.map(runnable, ({ step }) => (step.op === 'execJs' ? `(function (d) { ${step.code} })(this);` : `${call('action', step, output)};`)).join('\n')} })`,
                ),
            ),
            Effect.andThen(
                Effect.filterOrFail(Effect.mapError(fs.exists(output), notAccessible), identity, () =>
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
    const expanded = yield* Effect.forEach(sources, (source) => Effect.map(_matches(channel, folder, source.pattern, [path.relative(folder, output)]), (paths) => Tuple.make(source, paths)), {
        concurrency: 'unbounded',
    });
    const [empty, filled] = Array.partition(
        expanded,
        Filter.fromPredicate(([, paths]) => Array.isReadonlyArrayNonEmpty(paths)),
    );
    const skipped = Array.map(empty, ([source]) => ({ path: AbsolutePath.make(path.join(folder, source.pattern)), reason: { _tag: 'sourceMissing' as const } }));
    const rows = Array.flatMap(filled, ([source, paths]) =>
        Array.map(paths, (match): (typeof Sources)['Type'][number] => ({
            path: AbsolutePath.make(path.join(folder, match)),
            start: Option.map(source.range, Struct.get('start')),
            end: Option.map(source.range, Struct.get('end')),
            label: label(source, match),
        })),
    );
    return yield* Array.match(rows, {
        onEmpty: () => Effect.succeed({ kind: 'nothingInserted' as const, skipped }),
        onNonEmpty: (inserted) =>
            Effect.map(
                _call(
                    channel,
                    TIMEOUT_MS,
                    succeeded(_toolkit.tools.acrobat_combine),
                    call(
                        'combine',
                        Array.map(inserted, (source) => ({ path: source.path, range: Record.getSomes({ nStart: source.start, nEnd: source.end }), ...Record.getSomes({ label: source.label }) })),
                        output,
                    ),
                ),
                (combined) => ({
                    ...combined,
                    skipped: [...skipped, ...combined.skipped],
                }),
            ),
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
    const rows = Array.flatMap($('tr:has(td.cattitle)').toArray(), (heading) => {
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
    });
    const rules = yield* Effect.fromOption(Option.liftPredicate(rows, Array.isReadonlyArrayNonEmpty), () => notDecodable(channel.host.id, rows)('no rule row under a td.cattitle heading'));

    return yield* Effect.mapError(
        Schema.decodeUnknownEffect(succeeded(_toolkit.tools.acrobat_check_accessibility))({ kind: 'accessibility', rules, reportPath: report }),
        notDecodable(channel.host.id, rules),
    );
});

const _preflight = Effect.fnUntraced(function* (channel: Channel, { document, profile, library, fixups, report, save }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_preflight']>) {
    const fs = yield* FileSystem.FileSystem;
    const value = yield* _document(
        channel,
        document,
        Schema.Struct({ ..._Counts.fields, auditTrail: succeeded(_toolkit.tools.acrobat_preflight).fields.auditTrail, report: OptionalString }),
        _Profile.match(profile, {
            name: ({ name }) => call('runPreflight', 'getProfileByName', name, fixups, Option.isSome(report), Option.getOrNull(save), Option.getOrNull(library)),
            standard: ({ standard }) => call('runPreflight', 'createComplianceProfile', standard, fixups, Option.isSome(report), Option.getOrNull(save), Option.getOrNull(library)),
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
    Layer.provide(
        register(_toolkit),
        _toolkit.toLayer(
            Effect.map(Channel, (channel) =>
                Struct.map(
                    {
                        [_toolkit.tools.acrobat_get_state.name]: Effect.fnUntraced(function* ({ baseline }) {
                            const found = yield* installed('acrobat', channel.acrobat);
                            const answer = succeeded(_toolkit.tools.acrobat_get_state);
                            const value = yield* _call(
                                channel,
                                TIMEOUT_MS,
                                Schema.Struct({ ...answer.fields, documents: Schema.Array(Schema.Struct({ ...answer.fields.documents.value.fields, path: DevicePath(found.startupVolume) })) }),
                                call('state'),
                            );
                            const observed = Record.map(Struct.omit(value, ['drift']), (field) => ({ _tag: 'value' as const, value: field }));
                            const drift = yield* Effect.transposeOption(Option.map(baseline, (expected) => compare(expected, observed)));
                            return { ...value, drift };
                        }),
                        [_toolkit.tools.acrobat_open_document.name]: ({ path, hidden }) =>
                            Effect.flatMap(installed('acrobat', channel.acrobat), (found) =>
                                _call(
                                    channel,
                                    TIMEOUT_MS,
                                    Schema.Struct({ ...succeeded(_toolkit.tools.acrobat_open_document).fields, path: DevicePath(found.startupVolume) }),
                                    call('openDocument', path, hidden),
                                ),
                            ),
                        [_toolkit.tools.acrobat_execute.name]: ({ code, timeoutMs }) =>
                            Effect.map(Effect.timed(_call(channel, timeoutMs, Schema.Json, `(function () { ${code} })()`)), ([took, value]) =>
                                Value.make({ kind: 'value', value, tookMs: Duration.toMillis(took), autocorrections: Option.none(), undo: Option.none() }),
                            ),
                        [_toolkit.tools.acrobat_list_menu_items.name]: () => _call(channel, TIMEOUT_MS, succeeded(_toolkit.tools.acrobat_list_menu_items), call('menu')),
                        [_toolkit.tools.acrobat_exec_menu_item.name]: ({ name, document }) =>
                            Option.match(document, {
                                onNone: () => _call(channel, TIMEOUT_MS, succeeded(_toolkit.tools.acrobat_exec_menu_item), call('execMenuItem', name, false)),
                                onSome: (path) => _document(channel, path, succeeded(_toolkit.tools.acrobat_exec_menu_item), call('execMenuItem', name, true)),
                            }),
                        [_toolkit.tools.acrobat_run_action.name]: ({ name, inputs, output }) => Effect.flatMap(_targets(channel, inputs, output), (targets) => _runAction(channel, name, targets)),
                        [_toolkit.tools.acrobat_write_action.name]: ({ action }) =>
                            Effect.map(
                                Effect.flatMap(installed('acrobat', channel.acrobat), (found) => Effect.mapError(write(found, action), inaccessible(found.id))),
                                (path) => ({ kind: 'saved' as const, path, steps: Array.length(Array.flatMap(action.groups, Struct.get('steps'))) }),
                            ),
                        [_toolkit.tools.acrobat_combine.name]: ({ request }) => _request(channel, request),
                        [_toolkit.tools.acrobat_get_fields.name]: ({ document, names }) =>
                            _document(channel, document, succeeded(_toolkit.tools.acrobat_get_fields), call('getFields', Option.getOrNull(names))),
                        [_toolkit.tools.acrobat_set_fields.name]: ({ document, fields, save }) => _document(channel, document, succeeded(_toolkit.tools.acrobat_set_fields), setFields(fields, save)),
                        [_toolkit.tools.acrobat_set_tab_order.name]: ({ document, pages, order, save }) =>
                            _document(channel, document, succeeded(_toolkit.tools.acrobat_set_tab_order), call('tabOrder', Option.getOrNull(pages), order, Option.getOrNull(save))),
                        [_toolkit.tools.acrobat_autotag.name]: ({ document, save }) =>
                            Effect.flatMap(_document(channel, document, Schema.Struct({ before: Schema.Int }), call('autotag', NAMES.pdfUa, NAMES.makeAccessible)), ({ before }) =>
                                _document(channel, document, succeeded(_toolkit.tools.acrobat_autotag), call('tagged', NAMES.pdfUa, before, Option.getOrNull(save))),
                            ),
                        [_toolkit.tools.acrobat_check_accessibility.name]: ({ report }) => _report(channel, report),
                        [_toolkit.tools.acrobat_list_preflight.name]: ({ library }) =>
                            _call(channel, TIMEOUT_MS, succeeded(_toolkit.tools.acrobat_list_preflight), call('listPreflight', Option.getOrNull(library))),
                        [_toolkit.tools.acrobat_preflight.name]: (input) => _preflight(channel, input),
                        [_toolkit.tools.acrobat_print_production.name]: Effect.fnUntraced(function* ({
                            document,
                            operations,
                            save,
                        }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_print_production']>) {
                            yield* Effect.forEach(Array.fromOption(save), (output) => _preserveSource(channel, document, output)).pipe(
                                Effect.when(Effect.succeed(Array.some(operations, (operation) => operation.op === 'addWatermarkFromText' || operation.op === 'applyRedactions'))),
                            );
                            return yield* _document(channel, document, succeeded(_toolkit.tools.acrobat_print_production), printProduction(operations, save));
                        }),
                        [_toolkit.tools.acrobat_find_text.name]: ({ document, query, pages }) =>
                            Effect.map(_document(channel, document, _Text, call('scan', pages)), (text) => ({ kind: 'matches' as const, matches: matches(text, query) })),
                        [_toolkit.tools.acrobat_set_links.name]: Effect.fnUntraced(function* ({ document, rules, pages, save }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_set_links']>) {
                            const text = yield* _document(channel, document, _Text, call('scan', pages));
                            return yield* _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_set_links),
                                call(
                                    'annotations',
                                    {
                                        op: 'links',
                                        matches: Array.flatMap(rules, (rule) =>
                                            Array.map(
                                                matches(text, rule.match),
                                                Struct.assign({
                                                    target: rule.target.kind === 'url' ? { kind: 'url' as const, url: rule.target.url.href } : rule.target,
                                                    border: rule.border,
                                                }),
                                            ),
                                        ),
                                    },
                                    Option.getOrNull(save),
                                ),
                            );
                        }),
                        [_toolkit.tools.acrobat_redact.name]: Effect.fnUntraced(function* ({
                            document,
                            terms,
                            pages,
                            overlay,
                            keepMarks,
                            save,
                        }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_redact']>) {
                            yield* _preserveSource(channel, document, save);
                            const before = yield* _document(channel, document, _Text, call('scan', pages));
                            const applied = yield* _document(
                                channel,
                                document,
                                Schema.Struct(Struct.omit(succeeded(_toolkit.tools.acrobat_redact).fields, ['remaining'])),
                                call('annotations', { op: 'redact', matches: Array.flatMap(terms, (term) => matches(before, term)), keepMarks, ...Record.getSomes({ overlay }) }, save),
                            );
                            const after = yield* _document(channel, save, _Text, call('scan', pages));
                            return { ...applied, remaining: Array.length(Array.flatMap(terms, (term) => matches(after, term))) };
                        }),
                        [_toolkit.tools.acrobat_set_page_boxes.name]: ({ document, save, ...request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_set_page_boxes),
                                call('pages', { op: 'pageBoxes', ...Schema.encodeSync(_Requests.fields.pageBoxes)(request) }, Option.getOrNull(save)),
                            ),
                        [_toolkit.tools.acrobat_set_page_labels.name]: ({ document, save, ...request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_set_page_labels),
                                call('pages', { op: 'pageLabels', ...Schema.encodeSync(_Requests.fields.pageLabels)(request) }, Option.getOrNull(save)),
                            ),
                        [_toolkit.tools.acrobat_organize_pages.name]: ({ document, save, ...request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_organize_pages),
                                call('pages', { op: 'organize', ...Schema.encodeSync(_Requests.fields.organize)(request) }, Option.getOrNull(save)),
                            ),
                        [_toolkit.tools.acrobat_set_metadata.name]: Effect.fnUntraced(function* ({ document, save, info, xmp }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_set_metadata']>) {
                            const answer = succeeded(_toolkit.tools.acrobat_set_metadata);
                            if (Option.isNone(xmp) || xmp.value.kind === 'packet') {
                                return yield* _document(
                                    channel,
                                    document,
                                    answer,
                                    call(
                                        'resources',
                                        {
                                            op: 'metadata',
                                            info: Record.toEntries(info),
                                            ...Record.getSomes({
                                                xmp: Option.map(
                                                    Option.filter(xmp, (request) => request.kind === 'packet'),
                                                    Schema.encodeSync(_XmpPacket),
                                                ),
                                            }),
                                        },
                                        Option.getOrNull(save),
                                    ),
                                );
                            }
                            const before = yield* _document(channel, document, answer, call('resources', { op: 'metadata', info: [] }, null));
                            const packet = yield* Effect.mapError(Schema.decodeEffect(_XmpPacket)(before.xmp), notDecodable(channel.host.id, before.xmp));
                            const descriptions = Array.filter(Array.fromIterable(packet.rdf.children), (node) => node.namespaceURI === _RDF_NAMESPACE && node.localName === 'Description');
                            const destination = Option.getOrElse(Array.head(descriptions), () => packet.document.createElementNS(_RDF_NAMESPACE, 'rdf:Description'));
                            if (destination.parentNode === null) {
                                destination.setAttributeNS(_RDF_NAMESPACE, 'rdf:about', '');
                                packet.rdf.appendChild(destination);
                            }
                            const pairs = Array.cartesian(descriptions, xmp.value.properties);
                            const replacements = Array.flatMap(pairs, ([description, property]) => Array.fromIterable(description.getElementsByTagNameNS(property.namespaceURI, property.localName)));
                            const direct = Array.filter(replacements, (node) => node.parentElement !== null && Array.contains(descriptions, node.parentElement));
                            Array.map(pairs, ([description, property]) => description.removeAttributeNS(property.namespaceURI, property.localName));
                            Array.map(direct, (node) => node.parentNode?.removeChild(node));
                            Array.map(xmp.value.properties, (property) => destination.appendChild(packet.document.importNode(property, true)));
                            return yield* _document(
                                channel,
                                document,
                                answer,
                                call('resources', { op: 'metadata', info: Record.toEntries(info), xmp: Schema.encodeSync(_XmpPacket)(packet) }, Option.getOrNull(save)),
                            );
                        }),
                        [_toolkit.tools.acrobat_set_layers.name]: ({ document, save, ...request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_set_layers),
                                call('resources', { op: 'layers', ...Schema.encodeSync(_Requests.fields.layers)(request) }, Option.getOrNull(save)),
                            ),
                        [_toolkit.tools.acrobat_attach.name]: ({ document, save, ...request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_attach),
                                call('resources', { op: 'attachments', ...Schema.encodeSync(_Requests.fields.attachments)(request) }, Option.getOrNull(save)),
                            ),
                        [_toolkit.tools.acrobat_comments.name]: ({ document, request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_comments),
                                request.kind === 'import'
                                    ? call('annotations', { op: 'comments', request: Struct.omit(request, ['save']) }, Option.getOrNull(request.save))
                                    : call('annotations', { op: 'comments', request }, null),
                            ),
                        [_toolkit.tools.acrobat_stamp.name]: ({ document, save, ...request }) =>
                            _document(
                                channel,
                                document,
                                succeeded(_toolkit.tools.acrobat_stamp),
                                call('annotations', { op: 'stamp', ...Schema.encodeSync(_Requests.fields.stamp)(request) }, Option.getOrNull(save)),
                            ),
                        [_toolkit.tools.acrobat_export.name]: Effect.fnUntraced(function* ({ document, converter, output }: Tool.Parameters<(typeof _toolkit)['tools']['acrobat_export']>) {
                            const fs = yield* FileSystem.FileSystem;
                            const path = yield* Path.Path;
                            const directory = path.dirname(output);
                            yield* Effect.mapError(fs.makeDirectory(directory, { recursive: true }), inaccessible(channel.host.id));
                            const staging = yield* Effect.mapError(fs.makeTempDirectoryScoped({ directory }), inaccessible(channel.host.id));
                            const answer = succeeded(_toolkit.tools.acrobat_export);
                            const result = yield* _document(
                                channel,
                                document,
                                Schema.Union([Schema.Struct(Struct.omit(answer.members[0].fields, ['files', 'rejected'])), answer.members[1]]),
                                call('delivery', { op: 'export', converter, output: path.join(staging, path.basename(output)) }, null),
                            );
                            if (result.kind === 'converterAbsent') {
                                return result;
                            }
                            const entries = yield* Effect.mapError(fs.readDirectory(staging, { recursive: true }), inaccessible(channel.host.id));
                            const generated = yield* Effect.mapError(
                                Effect.filter(entries, (name) => Effect.map(fs.stat(path.join(staging, name)), flow(Struct.get('type'), Equal.equals('File')))),
                                inaccessible(channel.host.id),
                            );
                            const files = yield* Effect.fromOption(Option.liftPredicate(Array.sort(generated, Order.String), Array.isReadonlyArrayNonEmpty), () =>
                                BridgeError.cases.fileNotAccessible.make({ host: channel.host.id, path: Option.some(output), reason: 'NotFound' }),
                            );
                            const targets = Array.map(files, (name) => ({ source: path.join(staging, name), target: AbsolutePath.make(path.join(directory, name)) }));
                            yield* Effect.forEach(targets, ({ target }) => _preserveSource(channel, document, target), { discard: true });
                            const delivered = yield* Effect.forEach(targets, ({ source, target }) =>
                                Effect.andThen(fs.makeDirectory(path.dirname(target), { recursive: true }), fs.rename(source, target)).pipe(
                                    Effect.mapError(inaccessible(channel.host.id)),
                                    Effect.as(target),
                                    Effect.result,
                                ),
                            );
                            const [rejected, written] = Array.partition(Array.zip(targets, delivered), ([{ target }, outcome]) =>
                                Result.isFailure(outcome) ? Result.fail({ path: target, reason: outcome.failure }) : Result.succeed(outcome.success),
                            );
                            return { kind: 'exported' as const, converter, files: written, rejected };
                        }, Effect.scoped),
                        [_toolkit.tools.acrobat_encrypt.name]: ({ document, save, ...request }) =>
                            Effect.andThen(
                                _preserveSource(channel, document, save),
                                _document(
                                    channel,
                                    document,
                                    succeeded(_toolkit.tools.acrobat_encrypt),
                                    call('delivery', { op: 'encrypt', ...Schema.encodeSync(_Requests.fields.encrypt)(request) }, save),
                                ),
                            ),
                        [_toolkit.tools.acrobat_set_preferences.name]: ({ request }) => _preferences(channel, request),
                    } satisfies Handlers<typeof _toolkit.tools>,
                    answering,
                ),
            ),
        ),
    ),
    Layer.effect(
        Channel,
        Effect.map(Effect.all([Hosts, Jobs, compiled]), ([hosts, jobs, program]) => ({ acrobat: hosts.acrobat, host: jobs.acrobat, program })),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
