import { Effect, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Fact = (typeof scalars)[number]['Type'] | { readonly _tag: 'list'; readonly values: readonly Fact[] } | { readonly _tag: 'record'; readonly fields: Readonly<Record<string, Fact>> };
type Node<Tag extends string, Fields extends Schema.Struct.Fields> = Schema.Struct.Type<Fields> & { readonly _tag: Tag; readonly children: readonly Element[] };
type Wire<Tag extends string, Fields extends Schema.Struct.Fields> = Schema.Struct.Encoded<Fields> & { readonly _tag: Tag; readonly children: readonly Encoded[] };
type Element = Node<'container', typeof container> | Node<'control', typeof control> | Node<'option', typeof option>;
type Encoded = Wire<'container', typeof container> | Wire<'control', typeof control> | Wire<'option', typeof option>;
interface OptionFromOptionalKeyLambda extends Struct.Lambda {
    <S extends Schema.Constraint>(self: S): Schema.OptionFromOptionalKey<S>;
    readonly '~lambda.out': this['~lambda.in'] extends Schema.Constraint ? Schema.OptionFromOptionalKey<this['~lambda.in']> : never;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRAPHEMES = new Intl.Segmenter(undefined, { granularity: 'grapheme' });

// --- [MODELS] --------------------------------------------------------------------------

const Ref = Schema.Natural;
const Locator = Schema.String;
const Point = Schema.Tuple([Schema.Finite, Schema.Finite]);
const Frame = Schema.Tuple([Point, Point]);
const WindowNumber = Schema.Finite.check(Schema.isUint32());
const Value = Schema.Union([Schema.Boolean, Schema.Finite, Schema.String]);
const TextRange = Schema.Struct({ location: Schema.Natural, length: Schema.Natural });
const scalars = [
    Schema.TaggedStruct('bool', { value: Schema.Boolean }),
    Schema.TaggedStruct('integer', { value: Schema.Int }),
    Schema.TaggedStruct('real', { value: Schema.Finite }),
    Schema.TaggedStruct('text', { value: Schema.String }),
    Schema.TaggedStruct('date', { value: Schema.String }),
    Schema.TaggedStruct('url', { value: Schema.String }),
    Schema.TaggedStruct('point', { value: Point }),
    Schema.TaggedStruct('size', { value: Point }),
    Schema.TaggedStruct('rect', { value: Frame }),
    Schema.TaggedStruct('color', { red: Schema.Finite, green: Schema.Finite, blue: Schema.Finite, alpha: Schema.Finite }),
    Schema.TaggedStruct('range', { location: Schema.Int, length: Schema.Int }),
    Schema.TaggedStruct('element', { id: Schema.Int }),
    Schema.TaggedStruct('refused', { error: Schema.Int }),
] as const;
const Fact: Schema.Codec<Fact> = Schema.Union([
    ...scalars,
    Schema.TaggedStruct('list', { values: Schema.Array(Schema.suspend((): Schema.Codec<Fact> => Fact)) }),
    Schema.TaggedStruct('record', {
        fields: Schema.Record(
            Schema.String,
            Schema.suspend((): Schema.Codec<Fact> => Fact),
        ),
    }),
]);
const Modifier = Schema.Literals(['command', 'shift', 'option', 'control', 'function']);
class Shortcut extends Schema.Opaque<Shortcut>()(
    Schema.Struct({
        key: Schema.Union([
            Schema.Literals(['return', 'enter', 'tab', 'space', 'delete', 'forwardDelete', 'escape', 'clear', 'home', 'end', 'pageUp', 'pageDown', 'left', 'right', 'down', 'up', 'help', 'contextualMenu', 'eisu', 'kana']),
            Schema.TemplateLiteral(['f', Schema.Literals([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19])]),
            Schema.String.check(Schema.makeFilter((text: string) => Iterator.from(GRAPHEMES.segment(text)).take(2).toArray().length === 1, { expected: 'one character or a special key name' })),
            Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 65_535 })),
        ]),
        modifiers: Schema.Array(Modifier).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    }),
) {}
const stated = {
    subrole: Schema.String,
    label: Schema.Struct({ text: Schema.String, attribute: Schema.Literals(['AXTitle', 'AXDescription', 'recognized', 'AXUserInputLabels', 'AXFilename', 'AXTitleUIElement', 'AXValue', 'AXHelp', 'AXRoleDescription']) }),
    labels: Schema.Array(Schema.String),
    section: Schema.String,
    help: Schema.String,
    range: Schema.Struct({ minimum: Schema.optionalKey(Schema.Finite), maximum: Schema.optionalKey(Schema.Finite), step: Schema.optionalKey(Schema.Finite), wraps: Schema.optionalKey(Schema.Boolean) }),
    mark: Schema.String,
    shortcut: Shortcut,
    keyEquivalent: Schema.String,
    separated: Schema.Boolean,
};
const control = {
    ref: Ref,
    locator: Locator,
    kind: Schema.Literals(['action', 'toggle', 'choice', 'multi', 'row', 'number', 'measure', 'text', 'color', 'date', 'item', 'region']),
    role: Schema.String,
    positional: Schema.Boolean,
    ...Struct.map(stated, Schema.optionalKey),
    label: Schema.optionalKey(Schema.Union([stated.label, Schema.Struct({ text: Schema.String })])),
    value: Schema.optionalKey(Fact),
    enabled: Schema.optionalKey(Schema.Boolean),
    focused: Schema.optionalKey(Schema.Boolean),
    selected: Schema.optionalKey(Schema.Literals(['attribute', 'marked'])),
    frame: Schema.optionalKey(Frame),
    opens: Schema.optionalKey(Ref),
    alternate: Schema.optionalKey(Ref),
    placeholder: Schema.optionalKey(Schema.String),
    settable: Schema.optionalKey(Schema.Boolean),
    level: Schema.optionalKey(Schema.Int),
    index: Schema.optionalKey(Schema.Natural),
    expanded: Schema.optionalKey(Schema.Boolean),
    actions: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    alternateUIVisible: Schema.optionalKey(Schema.Boolean),
    disclosedBy: Schema.optionalKey(Ref),
    total: Schema.optionalKey(Schema.Natural),
    sort: Schema.optionalKey(Schema.Literals(['ascending', 'descending'])),
    cells: Schema.Struct({ column: Schema.optionalKey(Schema.String), text: Schema.String }).pipe(Schema.Array, Schema.optionalKey),
    column: Schema.optionalKey(Schema.String),
    scroll: Schema.optionalKey(Schema.Struct({ contentSize: Schema.optionalKey(Point), horizontalScrollBar: Schema.optionalKey(Schema.Finite), verticalScrollBar: Schema.optionalKey(Schema.Finite) })),
    selectedTextRanges: TextRange.pipe(Schema.Array, Schema.optionalKey),
    numberOfCharacters: Schema.optionalKey(Schema.Natural),
    insertionPointLineNumber: Schema.optionalKey(Schema.Natural),
    runs: Schema.Struct({ range: TextRange, text: Schema.String, attributes: Schema.Record(Schema.String, Fact), frame: Schema.optionalKey(Frame) }).pipe(Schema.Array, Schema.optionalKey),
    invalid: Schema.optionalKey(Schema.Literals(['true', 'grammar', 'spelling'])),
    required: Schema.optionalKey(Schema.Boolean),
    current: Schema.optionalKey(Schema.Literals(['true', 'page', 'step', 'location', 'date', 'time'])),
};
const container = {
    ref: Ref,
    locator: Locator,
    kind: Schema.Literals(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt', 'menuBar', 'statusItem', 'menu', 'toolbar', 'web']),
    title: Schema.optionalKey(Schema.String),
    help: Schema.optionalKey(Schema.String),
    document: Schema.optionalKey(Schema.String),
    edited: Schema.optionalKey(Schema.Boolean),
    fullScreen: Schema.optionalKey(Schema.Boolean),
    loaded: Schema.optionalKey(Schema.Boolean),
    loadingProgress: Schema.optionalKey(Schema.Finite),
    window: Schema.optionalKey(WindowNumber),
    frame: Schema.optionalKey(Frame),
    layer: Schema.optionalKey(Schema.Int),
    closed: Schema.Boolean,
    modal: Schema.Boolean,
    asking: Schema.Boolean,
    focused: Schema.optionalKey(Schema.Boolean),
    default: Schema.optionalKey(Ref),
    cancel: Schema.optionalKey(Ref),
    close: Schema.optionalKey(Ref),
    text: Schema.Array(Schema.String),
    truncated: Schema.Boolean,
    unreadable: Schema.Boolean,
};
const option = { ref: Ref, label: Schema.String, ...Struct.map(Struct.pick(stated, ['help', 'shortcut']), Schema.optionalKey) };
const children = Schema.Array(Schema.suspend((): Schema.Codec<Element, Encoded> => Element));
const Pointer = Schema.Struct({
    tool: Schema.optionalKey(Schema.String),
    shortcut: Schema.optionalKey(Shortcut),
    help: Schema.optionalKey(Schema.String),
    holding: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    operations: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    cursor: Schema.optionalKey(Schema.TaggedUnion({ system: { name: Schema.String }, custom: { digest: Schema.String, hotSpot: Point, size: Point } })),
    draggingFrame: Schema.optionalKey(Frame),
});
const Condition = Schema.TaggedUnion({
    Value: { at: Locator, value: Value },
    Documents: { held: Schema.Boolean },
    Modal: { at: Schema.NullOr(Locator) },
});
const Opener = Schema.Struct({ container: Locator, control: Locator });
const Shape = Schema.Struct({
    ...Struct.pick(control, ['kind', 'role', 'positional']),
    ...Struct.map(stated, Struct.lambda<OptionFromOptionalKeyLambda>(Schema.OptionFromOptionalKey)),
    options: Schema.String.pipe(Schema.Array, Schema.OptionFromOptionalKey),
    opens: Schema.OptionFromOptionalKey(Locator),
    alternate: Schema.OptionFromOptionalKey(Locator),
    parent: Schema.OptionFromOptionalKey(Locator),
});
class Observation extends Schema.Opaque<Observation>()(
    Schema.Struct({
        build: Schema.OptionFromOptionalKey(Schema.String),
        container: Locator,
        ...Struct.pick(container, ['kind', 'modal', 'truncated']),
        title: Schema.OptionFromOptionalKey(Schema.String),
        layer: Schema.OptionFromOptionalKey(Schema.Int),
        size: Schema.OptionFromOptionalKey(Point),
        default: Schema.OptionFromOptionalKey(Locator),
        cancel: Schema.OptionFromOptionalKey(Locator),
        close: Schema.OptionFromOptionalKey(Locator),
        opened: Schema.DateTimeUtcFromString,
        at: Schema.DateTimeUtcFromString,
        opener: Schema.OptionFromOptionalKey(Opener),
        documents: Schema.Array(Schema.String),
        conditions: Schema.Array(Condition),
        controls: Schema.Record(Locator, Shape),
        layout: Schema.Record(Locator, Frame),
        values: Schema.Record(Locator, Value),
    }),
) {}
const Element = Object.assign(Schema.TaggedUnion({ container: { ...container, children }, control: { ...control, children }, option: { ...option, children } }), {
    Ref,
    Locator,
    Frame,
    WindowNumber,
    Value,
    Fact,
    Modifier,
    Shortcut,
    Pointer,
    Scope: Schema.TaggedUnion({
        application: {},
        container: { ref: Ref },
        menu: { ref: Ref },
        rows: { ref: Ref, from: Schema.Natural, count: Schema.Int.check(Schema.isGreaterThan(0)) },
        text: { ref: Ref },
        menus: {},
    }),
    Condition,
    Opener,
    Shape,
    Observation,
    Model: Schema.Struct({ sequence: Schema.Int, elements: children, pointer: Schema.optionalKey(Pointer) }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Element };
