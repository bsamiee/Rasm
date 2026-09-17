// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, FileSystem, Match, Option, Order, Path, type PlatformError, Record, Result, Schema, Struct } from 'effect';
import Builder, { type XmlBuilderOptions } from 'fast-xml-builder';
import type { Hosts } from '../hosts.ts';
import { AbsolutePath, DevicePath } from '../values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Step = (typeof Step)['Type'];
type Items = (typeof Items)['Type'];
type Action = (typeof Action)['Type'];
type ActionName = (typeof ActionName)['Type'];
type Folder = (text: string) => string;
type Node = { readonly [K in (typeof _Element)['Type']]?: readonly Node[] } & { readonly ':@'?: Readonly<Record<string, string>> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _OPTIONS: XmlBuilderOptions & { readonly entities: readonly { readonly regex: RegExp; readonly val: string }[] } = {
    attributeNamePrefix: '',
    format: true,
    ignoreAttributes: false,
    indentBy: '\t',
    preserveOrder: true,
    suppressBooleanAttributes: false,
    suppressEmptyNode: true,
    entities: [
        { regex: /&/gu, val: '&amp;' },
        { regex: />/gu, val: '&gt;' },
        { regex: /</gu, val: '&lt;' },
        { regex: /'/gu, val: '&apos;' },
        { regex: /"/gu, val: '&quot;' },
        { regex: /\n/gu, val: '&#xD;' },
    ],
};
const _builder = new Builder(_OPTIONS);

// --- [MODELS] --------------------------------------------------------------------------

const _optionalString = Schema.OptionFromOptionalKey(Schema.String);
const _Element = Schema.Literals(['?xml', 'Workflow', 'Group', 'Instruction', 'Separator', 'Command', 'Items', 'Item']);
const _Item: Schema.Struct<{
    readonly name: Schema.String;
    readonly value: Schema.Union<
        readonly [
            Schema.Boolean,
            Schema.Int,
            Schema.String,
            Schema.Null,
            Schema.Struct<{ readonly string: Schema.String }>,
            Schema.Struct<{ readonly atom: Schema.String }>,
            Schema.Struct<{ readonly folder: Schema.String }>,
        ]
    >;
}> = Schema.Struct({
    name: Schema.String,
    value: Schema.Union([
        Schema.Boolean,
        Schema.Int,
        Schema.String,
        Schema.Null,
        Schema.Struct({ string: Schema.String }),
        Schema.Struct({ atom: Schema.String }),
        Schema.Struct({ folder: Schema.String }),
    ]),
});
const Items = Schema.Array(Schema.Union([_Item, Schema.Struct({ name: Schema.String, items: Schema.Array(_Item) })]));

const Step: Schema.toTaggedUnion<
    'op',
    readonly [
        Schema.Struct<{
            readonly op: Schema.Literal<'command'>;
            readonly name: Schema.String;
            readonly prompt: Schema.OptionFromOptionalKey<Schema.Boolean>;
            readonly items: Schema.OptionFromOptionalKey<
                Schema.$Array<Schema.Union<readonly [typeof _Item, Schema.Struct<{ readonly name: Schema.String; readonly items: Schema.$Array<typeof _Item> }>]>>
            >;
        }>,
        Schema.Struct<{ readonly op: Schema.Literal<'instruction'>; readonly text: Schema.String; readonly pauseBefore: Schema.Boolean }>,
        Schema.Struct<{ readonly op: Schema.Literal<'separator'> }>,
        Schema.Struct<{
            readonly op: Schema.Literal<'preflight'>;
            readonly profile: Schema.String;
            readonly dictKey: Schema.OptionFromOptionalKey<Schema.String>;
            readonly fingerprint: Schema.OptionFromOptionalKey<Schema.String>;
            readonly fixups: Schema.Boolean;
        }>,
        Schema.Struct<{ readonly op: Schema.Literal<'save'>; readonly folder: Schema.OptionFromOptionalKey<Schema.String> }>,
        Schema.Struct<{ readonly op: Schema.Literal<'execJs'>; readonly code: Schema.String }>,
    ]
> = Schema.Union([
    Schema.Struct({ op: Schema.Literal('command'), name: Schema.String, prompt: Schema.OptionFromOptionalKey(Schema.Boolean), items: Schema.OptionFromOptionalKey(Items) }),
    Schema.Struct({ op: Schema.Literal('instruction'), text: Schema.String, pauseBefore: Schema.Boolean }),
    Schema.Struct({ op: Schema.Literal('separator') }),
    Schema.Struct({ op: Schema.Literal('preflight'), profile: Schema.String, dictKey: _optionalString, fingerprint: _optionalString, fixups: Schema.Boolean }),
    Schema.Struct({ op: Schema.Literal('save'), folder: _optionalString }),
    Schema.Struct({ op: Schema.Literal('execJs'), code: Schema.String }),
]).pipe(Schema.toTaggedUnion('op'));

const Action: Schema.Struct<{
    readonly name: Schema.String;
    readonly title: Schema.String;
    readonly description: Schema.String;
    readonly groups: Schema.$Array<Schema.Struct<{ readonly label: Schema.String; readonly steps: Schema.$Array<typeof Step> }>>;
}> = Schema.Struct({
    name: Schema.String,
    title: Schema.String,
    description: Schema.String,
    groups: Schema.Array(Schema.Struct({ label: Schema.String, steps: Schema.Array(Step) })),
});

// --- [HOUSE] ---------------------------------------------------------------------------

const _HOUSE = {
    'rasm-accessible': {
        title: 'Rasm Accessible',
        description: 'Sets document properties and initial view, tags document, detects form fields, sets tab order and alternate text, and writes full accessibility check report to reports folder',
        groups: [
            {
                label: 'Prepare',
                steps: [
                    { op: 'command', name: 'GeneralInfo', prompt: true },
                    {
                        op: 'command',
                        name: 'OpenInfo',
                        prompt: false,
                        items: [
                            { name: 'CenterWindow', value: false },
                            { name: 'DisplayDocTitle', value: true },
                            { name: 'FitWindow', value: false },
                            { name: 'FullScreen', value: false },
                            { name: 'HideMenubar', value: false },
                            { name: 'HideToolbar', value: false },
                            { name: 'HideWindowUI', value: false },
                            {
                                name: 'LeaveAsIs',
                                items: [
                                    { name: 'CenterWindow', value: true },
                                    { name: 'FitWindow', value: true },
                                    { name: 'FullScreen', value: true },
                                    { name: 'HideMenubar', value: true },
                                    { name: 'HideToolbar', value: true },
                                    { name: 'HideWindowUI', value: true },
                                    { name: 'Magnification', value: true },
                                    { name: 'OpenAction', value: true },
                                    { name: 'PageLayout', value: true },
                                    { name: 'PageMode', value: true },
                                    { name: 'PageNum', value: true },
                                ],
                            },
                            { name: 'Magnification', value: '100%' },
                            { name: 'PageLayout', value: 0 },
                            { name: 'PageMode', value: 1 },
                            { name: 'PageNum', value: '1' },
                            { name: 'ResetDest', value: false },
                        ],
                    },
                    { op: 'command', name: 'SetReadingLanguage', prompt: true },
                ],
            },
            {
                label: 'Tag',
                steps: [
                    { op: 'command', name: 'Adobe:MakeAccessible', prompt: true },
                    { op: 'command', name: 'Adobe:FindsFormFields', prompt: false, items: [{ name: 'PromptUser', value: false }] },
                    { op: 'command', name: 'SetTabOrder', prompt: false },
                    { op: 'command', name: 'SetAlternateText', prompt: true },
                ],
            },
            {
                label: 'Check',
                steps: [
                    {
                        op: 'command',
                        name: 'AccCheck:DoCheck',
                        prompt: false,
                        items: [
                            { name: 'AltText', value: true },
                            { name: 'AppletsPlugins', value: true },
                            { name: 'AttachAnnots', value: false },
                            { name: 'CharEnc', value: true },
                            { name: 'ChosenPath', value: { folder: '.artifacts/creative-cloud/acrobat/reports' } },
                            { name: 'ClientSideImageMaps', value: true },
                            { name: 'Color', value: true },
                            { name: 'ComplexTables', value: true },
                            { name: 'FieldNames', value: true },
                            { name: 'FlickerRate', value: true },
                            { name: 'Forms', value: true },
                            { name: 'Frames', value: true },
                            { name: 'FromPage', value: null },
                            { name: 'Hints', value: true },
                            { name: 'IsBatch', value: true },
                            { name: 'LangSpec', value: true },
                            { name: 'LogFilePath', value: null },
                            { name: 'Multimedia', value: true },
                            { name: 'NavigationLinks', value: true },
                            { name: 'OutputOurLog', value: true },
                            { name: 'Pages', value: 0 },
                            { name: 'Readability', value: true },
                            { name: 'Scripts', value: true },
                            { name: 'ServerSideImageMaps', value: true },
                            { name: 'StandardName', value: 0 },
                            { name: 'TabOrder', value: true },
                            { name: 'TableHeaders', value: true },
                            { name: 'TextOnly', value: true },
                            { name: 'TimedResponses', value: true },
                            { name: 'ToPage', value: null },
                            { name: 'UntaggedContents', value: true },
                            { name: 'ValidStruct', value: true },
                            { name: 'WCAGPriority1', value: true },
                            { name: 'WCAGPriority2', value: true },
                            { name: 'WCAGPriority3', value: true },
                        ],
                    },
                    { op: 'save' },
                ],
            },
        ],
    },
    'rasm-print': {
        title: 'Rasm Print',
        description: 'Runs Default Print preflight profile with fixups and saves',
        groups: [{ label: 'Preflight', steps: [{ op: 'preflight', profile: 'Default Print', fixups: true }, { op: 'save' }] }],
    },
    'rasm-digital': {
        title: 'Rasm Digital',
        description: 'Runs Default Digital preflight profile with fixups and saves',
        groups: [{ label: 'Preflight', steps: [{ op: 'preflight', profile: 'Default Digital', fixups: true }, { op: 'save' }] }],
    },
    'rasm-scan': {
        title: 'Rasm Scan',
        description: 'Recognizes text, deskews, descreens, and compresses scanned pages, and saves',
        groups: [
            {
                label: 'Enhance',
                steps: [
                    {
                        op: 'command',
                        name: 'Scan:OPT',
                        prompt: false,
                        items: [
                            { name: 'ApplyMRC', value: true },
                            { name: 'BkgrRemove', value: 0 },
                            { name: 'ColorCompression', value: 4 },
                            { name: 'Descreen', value: 1 },
                            { name: 'Deskew', value: 1 },
                            { name: 'Format', value: 1 },
                            { name: 'Language', value: -1 },
                            { name: 'MonoCompression', value: 1 },
                            { name: 'QualityLevel', value: 1 },
                            { name: 'TextSharpen', value: 1 },
                            { name: 'doOCR', value: true },
                        ],
                    },
                    { op: 'save' },
                ],
            },
        ],
    },
    'rasm-redact': {
        title: 'Rasm Redact',
        description: 'Pauses for redaction marks, applies them, removes hidden information except metadata, bookmarks, and form fields, and saves',
        groups: [
            {
                label: 'Redact',
                steps: [
                    { op: 'instruction', text: 'Mark text and images for redaction, then continue', pauseBefore: true },
                    { op: 'command', name: 'Annots:Tool:RedactMenuItem' },
                    { op: 'command', name: 'Annots:Tool:ApplyRedactionsMenuItem', prompt: true },
                ],
            },
            {
                label: 'Clean',
                steps: [
                    {
                        op: 'command',
                        name: 'RemoveHiddenInfo',
                        prompt: false,
                        items: [
                            { name: 'RmAnnots', value: true },
                            { name: 'RmAttachments', value: true },
                            { name: 'RmBookmarks', value: false },
                            { name: 'RmDeletedContent', value: true },
                            { name: 'RmFormFields', value: false },
                            { name: 'RmHiddenLayers', value: true },
                            { name: 'RmHiddenText', value: true },
                            { name: 'RmLinksActionsJS', value: true },
                            { name: 'RmMetadata', value: false },
                            { name: 'RmOverlappingObjects', value: true },
                            { name: 'RmSearchIndex', value: true },
                        ],
                    },
                    { op: 'save' },
                ],
            },
        ],
    },
} as const;

const ActionName: Schema.Literals<Array<keyof typeof _HOUSE>> = Schema.Literals(Struct.keys(_HOUSE));

const HOUSE: Readonly<Record<ActionName, Action>> = Record.map(Schema.decodeSync(Schema.Record(ActionName, Schema.Struct(Struct.omit(Action.fields, ['name']))))(_HOUSE), (body, name) => ({
    ...body,
    name,
}));

// --- [RENDERING] -----------------------------------------------------------------------

const _node = (tag: (typeof _Element)['Type'], attributes: Readonly<Record<string, string>>, children: readonly Node[]): Node => ({ [tag]: children, ':@': attributes });

const _leaf = ({ name, value }: (typeof _Item)['Type'], folder: Folder): Node =>
    Match.value(value).pipe(
        Match.withReturnType<Node>(),
        Match.when(Match.boolean, (flag) => _node('Item', { name, type: 'boolean', value: String(flag) }, [])),
        Match.when(Match.number, (integer) => _node('Item', { name, type: 'integer', value: String(integer) }, [])),
        Match.when(Match.string, (text) => _node('Item', { name, type: 'text', value: text }, [])),
        Match.when(null, () => _node('Item', { name, type: 'null' }, [])),
        Match.when({ string: Match.string }, ({ string }) => _node('Item', { name, type: 'string', value: string }, [])),
        Match.when({ atom: Match.string }, ({ atom }) => _node('Item', { name, type: 'atom', value: atom }, [])),
        Match.when({ folder: Match.string }, (leaf) => _node('Item', { name, type: 'text', value: `${folder(leaf.folder)}/` }, [])),
        Match.exhaustive,
    );

const _byName: Order.Order<{ readonly name: string }> = Order.mapInput(Order.String, Struct.get('name'));

const _items = (items: Items, folder: Folder): readonly Node[] =>
    Array.map(Array.sort(items, _byName), (item) =>
        'items' in item
            ? _node(
                  'Items',
                  { name: item.name },
                  Array.map(Array.sort(item.items, _byName), (leaf) => _leaf(leaf, folder)),
              )
            : _leaf(item, folder),
    );

const _command = (name: string, prompt: Option.Option<boolean>, items: Option.Option<Items>, folder: Folder): Node =>
    _node(
        'Command',
        Record.getSomes({ name: Option.some(name), pauseBefore: Option.some('false'), promptUser: Option.map(prompt, String) }),
        Array.fromOption(Option.map(items, (rows) => _node('Items', {}, _items(rows, folder)))),
    );

const _step = (step: Step, folder: Folder): Node =>
    Step.match(step, {
        command: ({ name, prompt, items }) => _command(name, prompt, items, folder),
        instruction: ({ text, pauseBefore }) => _node('Instruction', { label: text, pauseBefore: String(pauseBefore) }, []),
        separator: () => _node('Separator', {}, []),
        preflight: ({ profile, dictKey, fingerprint, fixups }) =>
            _command(
                'CALS:Preflight',
                Option.some(false),
                Option.some([
                    ...Array.flatMap(
                        ['ERR', 'SUC'],
                        (side): Items => [
                            { name: `CALS_PREFLIGHT_CMD_${side}_ACT_TYPE`, value: 0 },
                            { name: `CALS_PREFLIGHT_CMD_${side}_EMB_AT`, value: 0 },
                            { name: `CALS_PREFLIGHT_CMD_${side}_FOLDER`, value: '' },
                            { name: `CALS_PREFLIGHT_CMD_${side}_REP_TYPE`, value: 0 },
                        ],
                    ),
                    { name: 'CALS_PREFLIGHT_CMD_OMIT_FIXUPS', value: !fixups },
                    ...Array.fromOption(Option.map(dictKey, (key): Items[number] => ({ name: 'CALS_PREFLIGHT_CMD_PROFILE_DICTKEY', value: key }))),
                    ...Array.fromOption(Option.map(fingerprint, (print): Items[number] => ({ name: 'CALS_PREFLIGHT_CMD_PROFILE_FINGERPRINT', value: print }))),
                    { name: 'CALS_PREFLIGHT_CMD_PROFILE_NAME', value: profile },
                    { name: 'CALS_PREFLIGHT_CMD_REP_DISPLAY', value: false },
                ]),
                folder,
            ),
        save: ({ folder: output }) =>
            _command(
                'WorkflowPlaybackSaveFiles',
                Option.some(false),
                Option.some(
                    Option.match(output, {
                        onNone: (): Items => [{ name: 'DocSaveDestType', value: { string: 'WorkflowPlaybackSaveAs' } }],
                        onSome: (target): Items => [
                            { name: 'AddToBaseName', value: true },
                            { name: 'DocSaveDestType', value: { string: 'WorkflowPlaybackSaveInAFolder' } },
                            { name: 'DontOverwrite', value: true },
                            { name: 'EmbedIndex', value: false },
                            { name: 'FS', value: { atom: 'Mac' } },
                            { name: 'FileVariation', value: 0 },
                            { name: 'HandleOutput', value: true },
                            { name: 'InsertAfterBaseName', value: '' },
                            { name: 'InsertBeforeBaseName', value: '' },
                            { name: 'NumbericNaming', value: false },
                            { name: 'OptimizePDF', value: true },
                            { name: 'PreselectedFolderPath', value: { folder: target } },
                            { name: 'PresetName', value: 'Standard' },
                            { name: 'RunPDFOptimizer', value: false },
                        ],
                    }),
                ),
                folder,
            ),
        execJs: ({ code }) =>
            _command(
                'JavaScript',
                Option.some(false),
                Option.some([
                    { name: 'ScriptCode', value: code },
                    { name: 'ScriptName', value: '' },
                ]),
                folder,
            ),
    });

const _render = (action: Action, folder: Folder): string =>
    `${_builder.build([
        _node('?xml', { version: '1.0', encoding: 'UTF-8' }, []),
        _node(
            'Workflow',
            { xmlns: 'http://ns.adobe.com/acrobat/workflow/2012', title: action.title, description: action.description, majorVersion: '1', minorVersion: '0' },
            Array.map(
                action.groups,
                (group): Node =>
                    _node(
                        'Group',
                        { label: group.label },
                        Array.map(group.steps, (step) => _step(step, folder)),
                    ),
            ),
        ),
    ])}\n\n\n`;

const write: (acrobat: Hosts['acrobat'], action: Action) => Effect.Effect<AbsolutePath, PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> = Effect.fnUntraced(function* (
    acrobat: Hosts['acrobat'],
    action: Action,
) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const target = AbsolutePath.make(path.join(acrobat.sequencesFolder, `${action.name}.sequ`));
    const device = Schema.encodeSync(DevicePath(acrobat.startupVolume));
    yield* fs.writeFileString(
        target,
        _render(action, (text) => device(AbsolutePath.make(path.resolve(import.meta.dirname, '..', '..', '..', '..', text)))),
    );
    return target;
});

// --- [RUN] -----------------------------------------------------------------------------

const script = (step: Step): Result.Result<(output: string) => string, void> =>
    Step.match(step, {
        command: ({ name }) => Result.succeed(() => `app.execMenuItem(${JSON.stringify(name)}, d);`),
        instruction: () => Result.failVoid,
        separator: () => Result.failVoid,
        preflight: ({ profile, fixups }) => {
            const name = JSON.stringify(profile);
            return Result.succeed(
                () => `(function () { var p = Preflight.getProfileByName(${name}); if (p === undefined) { throw { _tag: 'profileAbsent', profile: ${name} }; } d.preflight(p, ${!fixups}); })();`,
            );
        },
        save: () => Result.succeed((output: string) => `d.saveAs({ cPath: ${JSON.stringify(output)} });`),
        execJs: ({ code }) => Result.succeed(() => `(function (d) { ${code} })(d);`),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Action, ActionName, HOUSE, script, write };
