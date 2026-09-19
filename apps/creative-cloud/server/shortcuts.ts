// --- [IMPORTS] -------------------------------------------------------------------------

import { load } from 'cheerio';
import { createToken, EmbeddedActionsParser, EOF, type IToken, Lexer } from 'chevrotain';
import { Array, Effect, Equal, HashMap, HashSet, Match, Option, Order, Record, Schema, String, Struct } from 'effect';
import { SyntaxValidator } from 'fast-xml-validator';

// --- [MODELS] --------------------------------------------------------------------------

const toolType = 1;
const keys = Schema.Array(Schema.NonEmptyString);
const nativeAction = Schema.TemplateLiteralParser([Schema.String.check(Schema.isPattern(/^\s*(?:0x)?[\da-f]+\s*$/iu)), '+', Schema.NumberFromString.check(Schema.isInt())]);
const uint16 = Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 65_535 }));

const Shortcut: Schema.TaggedUnion<{
    readonly illustrator: Schema.TaggedStruct<
        'illustrator',
        {
            readonly target: Schema.Struct<{ readonly section: Schema.Literals<readonly ['Menus', 'Tools']>; readonly command: Schema.NonEmptyString }>;
            readonly keys: Schema.Struct<{ readonly [K in 'context' | 'modifiers' | 'represent' | 'key']: Schema.Int }>;
        }
    >;
    readonly indesign: Schema.TaggedStruct<
        'indesign',
        {
            readonly target: Schema.Struct<{ readonly action: Schema.Int; readonly context: Schema.NonEmptyString }>;
            readonly keys: Schema.$Array<Schema.NonEmptyString>;
        }
    >;
    readonly photoshop: Schema.TaggedStruct<
        'photoshop',
        {
            readonly target: Schema.Union<
                readonly [
                    Schema.Struct<{ readonly kind: Schema.Literal<'static'>; readonly id: Schema.Int }>,
                    Schema.Struct<{ readonly kind: Schema.Literal<'dynamic'>; readonly name: Schema.NonEmptyString }>,
                    Schema.Struct<{ readonly kind: Schema.Literal<'tool'>; readonly type: Schema.Literal<typeof toolType>; readonly key: Schema.Int }>,
                    Schema.Struct<{ readonly kind: Schema.Literal<'tool'>; readonly type: Schema.Int }>,
                    Schema.Struct<{ readonly kind: Schema.Literal<'taskspace-tool'>; readonly taskspace: Schema.NonEmptyString; readonly type: Schema.Int; readonly key: Schema.Int }>,
                    Schema.Struct<{ readonly kind: Schema.Literal<'taskspace-property'>; readonly taskspace: Schema.NonEmptyString; readonly name: Schema.NonEmptyString }>,
                ]
            >;
            readonly keys: Schema.$Array<Schema.NonEmptyString>;
        }
    >;
}> = Schema.TaggedUnion({
    illustrator: {
        target: Schema.Struct({ section: Schema.Literals(['Menus', 'Tools']), command: Schema.NonEmptyString }),
        keys: Schema.Struct({
            context: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 255 })),
            modifiers: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 4_294_967_295 })),
            represent: uint16,
            key: uint16,
        }),
    },
    indesign: {
        target: Schema.Struct({ action: Schema.Int, context: Schema.NonEmptyString }),
        keys,
    },
    photoshop: {
        target: Schema.Union([
            Schema.Struct({ kind: Schema.Literal('static'), id: Schema.Int }),
            Schema.Struct({ kind: Schema.Literal('dynamic'), name: Schema.NonEmptyString }),
            Schema.Struct({ kind: Schema.Literal('tool'), type: Schema.Literal(toolType), key: Schema.Int }),
            Schema.Struct({ kind: Schema.Literal('tool'), type: Schema.Int.check(Schema.isGreaterThan(toolType)) }),
            Schema.Struct({ kind: Schema.Literal('taskspace-tool'), taskspace: Schema.NonEmptyString, type: Schema.Int, key: Schema.Int }),
            Schema.Struct({ kind: Schema.Literal('taskspace-property'), taskspace: Schema.NonEmptyString, name: Schema.NonEmptyString }),
        ]),
        keys,
    },
});

const ShortcutError: Schema.TaggedUnion<{
    readonly shortcutInvalid: Schema.TaggedStruct<
        'shortcutInvalid',
        {
            readonly reason: Schema.Literals<readonly ['invalidSource', 'unknownCommand', 'duplicateCommand', 'unsupportedBinding']>;
            readonly detail: Schema.String;
        }
    >;
    readonly shortcutXmlInvalid: Schema.TaggedStruct<'shortcutXmlInvalid', { readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({
    shortcutInvalid: { reason: Schema.Literals(['invalidSource', 'unknownCommand', 'duplicateCommand', 'unsupportedBinding']), detail: Schema.String },
    shortcutXmlInvalid: { cause: Schema.Defect() },
});

interface NamespaceCommand {
    readonly section: IToken;
    readonly command: IToken;
    readonly fields: readonly (readonly [IToken, IToken])[];
}

interface Entry {
    readonly binding: (typeof Shortcut)['Type'];
    readonly ranges: readonly { readonly start: number; readonly end: number }[];
    readonly fields: Readonly<Record<string, IToken>>;
}

// --- [ILLUSTRATOR GRAMMAR] -------------------------------------------------------------

const tokens = {
    whitespace: createToken({ name: 'Whitespace', pattern: /\s+/u, group: Lexer.SKIPPED }),
    name: createToken({ name: 'Name', pattern: /\/(?:\\[^\r\n]|[^\\\s{}])+/u }),
    integer: createToken({ name: 'Integer', pattern: /-?\d+/u }),
    open: createToken({ name: 'Open', pattern: /\{/u }),
    close: createToken({ name: 'Close', pattern: /\}/u }),
};

class Namespace extends EmbeddedActionsParser {
    readonly field = this.RULE('field', (): readonly [IToken, IToken] => [this.CONSUME(tokens.name), this.CONSUME(tokens.integer)]);

    readonly command = this.RULE('command', (): Omit<NamespaceCommand, 'section'> => {
        const command = this.CONSUME(tokens.name);
        const fields: (readonly [IToken, IToken])[] = [];
        this.OPTION(() => {
            this.CONSUME(tokens.open);
            this.MANY(() => fields.push(this.SUBRULE(this.field)));
            this.CONSUME(tokens.close);
        });
        return { command, fields };
    });

    readonly section = this.RULE('section', (): readonly NamespaceCommand[] => {
        const section = this.CONSUME(tokens.name);
        this.CONSUME(tokens.open);
        const commands: Omit<NamespaceCommand, 'section'>[] = [];
        this.MANY(() => commands.push(this.SUBRULE(this.command)));
        this.CONSUME(tokens.close);
        return Array.map(commands, (command) => ({ section, ...command }));
    });

    readonly document = this.RULE('document', (): readonly NamespaceCommand[] => {
        const sections: (readonly NamespaceCommand[])[] = [];
        this.MANY(() => sections.push(this.SUBRULE(this.section)));
        this.CONSUME(EOF);
        return Array.flatten(sections);
    });

    constructor() {
        super(tokens, { recoveryEnabled: false });
        this.performSelfAnalysis();
    }
}

// --- [READ] ----------------------------------------------------------------------------

const read: (host: (typeof Shortcut)['Type']['_tag'], source: string) => Effect.Effect<readonly Entry[], (typeof ShortcutError)['Type'] | Schema.SchemaError> = Effect.fnUntraced(function* (
    host: (typeof Shortcut)['Type']['_tag'],
    source: string,
) {
    if (host === 'illustrator') {
        const lexed = new Lexer(Record.values(tokens)).tokenize(source);
        const parser = new Namespace();
        parser.input = lexed.tokens;
        const commands = parser.document();
        const failures = [...lexed.errors, ...parser.errors];
        if (Array.isArrayNonEmpty(failures)) {
            return yield* Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: Array.join(Array.map(failures, Struct.get('message')), '\n') }));
        }
        const entries = yield* Effect.forEach(
            Array.filter(commands, ({ section }) => Array.contains(Shortcut.cases.illustrator.fields.target.fields.section.literals, section.image.slice(1))),
            Effect.fnUntraced(function* ({ section, command, fields }: NamespaceCommand) {
                const tokensByField = Record.fromIterableWith(fields, ([name, value]) => [String.uncapitalize(name.image.slice(1)), value]);
                yield* Record.size(tokensByField) === fields.length
                    ? Effect.void
                    : Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: 'A native shortcut has duplicate integer fields.' }));
                const binding = yield* Schema.decodeUnknownEffect(Shortcut.cases.illustrator)({
                    _tag: host,
                    target: { section: section.image.slice(1), command: command.image.slice(1).replaceAll(/\\(?<escaped>[^\r\n])/gu, '$<escaped>') },
                    keys: Record.map(tokensByField, (token) => Number(token.image)),
                });
                return { binding, fields: tokensByField, ranges: [] };
            }),
        );
        if (HashSet.size(HashSet.fromIterable(Array.map(entries, ({ binding }) => binding.target))) !== entries.length) {
            return yield* Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: 'The native set has duplicate command identities.' }));
        }
        return entries;
    }
    yield* Effect.try({
        try: () => SyntaxValidator.validate(source),
        catch: (cause) => ShortcutError.cases.shortcutXmlInvalid.make({ cause }),
    });
    const xml = load(source, { xml: { withStartIndices: true, withEndIndices: true } });
    const root = xml.root().children();
    const expected = host === 'indesign' ? 'shortcut-set' : 'photoshop-keyboard-shortcuts';
    if (root.length !== 1 || !root.is(expected)) {
        return yield* Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: `Expected one ${expected} root.` }));
    }
    const nodes = host === 'indesign' ? root.children('shortcut') : root.children('taskspace').children('taskspace-tool, taskspace-property').add(root.children('command, tool'));
    const entries = yield* Effect.forEach(
        nodes.toArray(),
        Effect.fnUntraced(function* (node) {
            const element = xml(node);
            const range = yield* Effect.fromOption(Option.all({ start: Option.fromNullishOr(node.startIndex), end: Option.map(Option.fromNullishOr(node.endIndex), (end) => end + 1) }), () =>
                ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: 'XML parser omitted a source range.' }),
            );
            if (host === 'indesign') {
                const [base, , offset] = yield* Schema.decodeUnknownEffect(nativeAction)(element.children('action-id').attr('value'));
                const binding = yield* Schema.decodeUnknownEffect(Shortcut.cases.indesign)({
                    _tag: host,
                    target: { action: Number.parseInt(base, 16) + offset, context: element.children('context').text() },
                    keys: [element.children('string').text()],
                });
                return { binding, ranges: [range], fields: {} };
            }
            const kind = element.is('command') ? element.attr('kind') : node.name;
            const target = Match.value(kind).pipe(
                Match.when('static', () => ({ kind, id: Number(element.attr('id')) })),
                Match.when('dynamic', () => ({ kind, name: element.attr('name') })),
                Match.when('taskspace-property', () => ({ kind, taskspace: element.parent().attr('name'), name: element.attr('name') })),
                Match.orElse(() => ({ kind, type: Number(element.attr('type')), key: Number(element.attr('key')), taskspace: element.parent('taskspace').attr('name') })),
            );
            const content = element.is('command') ? element.children('shortcut').toArray() : [node];
            const binding = yield* Schema.decodeUnknownEffect(Shortcut.cases.photoshop)({
                _tag: host,
                target,
                keys: Array.filter(
                    Array.map(content, (child) => xml(child).text()),
                    String.isNonEmpty,
                ),
            });
            return { binding, ranges: [range], fields: {} };
        }),
    );
    const grouped = Array.reduce(entries, HashMap.empty<(typeof entries)[number]['binding']['target'], Array.NonEmptyArray<(typeof entries)[number]>>(), (acc, entry) =>
        HashMap.modifyAt(acc, entry.binding.target, (prior) => Option.some(Option.match(prior, { onNone: () => Array.of(entry), onSome: Array.append(entry) }))),
    );
    return yield* Effect.forEach(HashMap.values(grouped), ([first, ...rest]) => {
        if (first.binding._tag === 'indesign') {
            return Effect.succeed({
                ...first,
                binding: Shortcut.cases.indesign.make({ target: first.binding.target, keys: [...first.binding.keys, ...Array.flatMap(rest, (entry) => entry.binding.keys)] }),
                ranges: [...first.ranges, ...Array.flatMap(rest, Struct.get('ranges'))],
            });
        }
        return rest.length === 0
            ? Effect.succeed(first)
            : Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: 'The native set has duplicate command identities.' }));
    });
});

// --- [COMPILE] -------------------------------------------------------------------------

const compile: (
    host: (typeof Shortcut)['Type']['_tag'],
    source: string,
    changes: readonly (typeof Shortcut)['Type'][],
) => Effect.Effect<
    { readonly source: string; readonly bindings: readonly (typeof Shortcut)['Type'][] },
    (typeof ShortcutError)['Type'] | Schema.SchemaError | Array.NonEmptyArray<(typeof ShortcutError)['Type']>
> = Effect.fnUntraced(function* (host: (typeof Shortcut)['Type']['_tag'], source: string, changes: readonly (typeof Shortcut)['Type'][]) {
    const entries = yield* read(host, source);
    const index = HashMap.fromIterable(Array.map(entries, (entry) => [{ ...entry.binding.target, host: entry.binding._tag }, entry] as const));
    const identities = Array.map(changes, (binding) => ({ ...binding.target, host: binding._tag }));
    if (HashSet.size(HashSet.fromIterable(identities)) !== changes.length) {
        return yield* Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'duplicateCommand', detail: 'Each command may occur once in the replacement table.' }));
    }
    const patches = yield* Effect.validate(
        changes,
        Effect.fnUntraced(function* (binding, indexOfChange) {
            const entry = yield* Effect.fromOption(HashMap.get(index, { ...binding.target, host: binding._tag }), () =>
                ShortcutError.cases.shortcutInvalid.make({ reason: 'unknownCommand', detail: `Replacement ${indexOfChange} does not identify a command in the source set.` }),
            );
            if (Equal.equals(entry.binding, binding)) {
                return [];
            }
            if (binding._tag === 'illustrator') {
                return yield* Effect.forEach(Record.toEntries(binding.keys), ([field, value]) =>
                    Effect.map(
                        Effect.fromOption(Record.get(entry.fields, field), () => ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: `Missing native ${field} field.` })),
                        (token) => ({ start: token.startOffset, end: token.startOffset + token.image.length, text: value.toString() }),
                    ),
                );
            }
            if (binding._tag === 'photoshop' && binding.target.kind !== 'static' && binding.target.kind !== 'dynamic' && binding.keys.length > 1) {
                return yield* Effect.fail(ShortcutError.cases.shortcutInvalid.make({ reason: 'unsupportedBinding', detail: 'Photoshop tool and taskspace rows each store one shortcut.' }));
            }
            const { ranges } = entry;
            const first = yield* Effect.fromOption(Array.head(ranges), () => ShortcutError.cases.shortcutInvalid.make({ reason: 'invalidSource', detail: 'The command has no XML source element.' }));
            const xml = load(source.slice(first.start, first.end), { xml: true }, false);
            const element = xml.root().children();
            if (binding._tag === 'indesign') {
                const rows = Array.map(binding.keys, (key) => xml.xml(element.clone().children('string').text(key).end()));
                return Array.map(ranges, (range, position) => ({ ...range, text: position === 0 ? Array.join(rows, '') : '' }));
            }
            if (binding.target.kind === 'static' || binding.target.kind === 'dynamic') {
                element.children('shortcut').remove();
                element.append(Array.flatMap(binding.keys, (key) => xml('<shortcut/>').text(key).toArray()));
            } else {
                element.text(Array.join(binding.keys, ''));
            }
            return [{ ...first, text: xml.xml(element) }];
        }),
    );
    const ordered = Array.sort(Array.flatten(patches), Order.Struct({ start: Order.Number }));
    const [end, pieces] = Array.mapAccum(ordered, 0, (position, patch) => [patch.end, source.slice(position, patch.start) + patch.text]);
    const result = Array.join([...pieces, source.slice(end)], '');
    const parsed = yield* read(host, result);
    return { source: result, bindings: Array.map(parsed, Struct.get('binding')) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { compile, read, Shortcut, ShortcutError };
