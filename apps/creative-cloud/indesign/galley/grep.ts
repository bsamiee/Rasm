import { type LinkError, ranked } from '@rasm/ai/extension';
import { Array, Effect, flow, Option, Schema, Struct } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';
import type { RpcClient, RpcClientError } from 'effect/rpc';
import { GalleyError, Inspect, type Panel, Queries } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const grep = Effect.fn('grep')(function* (call: <A>(use: (client: RpcClient.FromGroup<typeof Panel, RpcClientError.RpcClientError>) => Effect.Effect<A, GalleyError | RpcClientError.RpcClientError>) => Effect.Effect<A, GalleyError | LinkError>) {
    const uri = 'indesign://grep/reference';
    const dialect = {
        supported: {
            escapes: ['\\d and \\w over Unicode', '\\s \\h \\v \\R \\X \\b \\A \\G \\Z \\z', '\\Q…\\E', '\\K at the top level', '\\x41 \\x{2014} \\x{1F600}', '\\N{EM DASH}'],
            groups: ['a++', '(?>…)', '(?i) (?-i) (?i:…) (?s) (?m) (?x)', '(?|…)', '(?(1)…)', '(?<name>…) with \\k<name>', 'lookbehind whose alternatives share one length'],
            classes: ['[[=e=]]', '[[:alpha:]] [[:upper:]] [[:lower:]] [[:punct:]] [[:word:]]', '[[:unicode:]] above U+00FF'],
            properties: ['\\p{Lu} \\p{Ll} \\p{Lo} \\p{Nd} \\p{Zs} \\p{Sc} \\p{Sm} \\p{Mn} \\p{No} \\P{Lu}', '\\p{Letter} \\p{Number} \\p{Punctuation} \\p{Symbol} \\p{Mark} \\p{Lowercase_Letter} \\p{Uppercase_Letter}'],
            tilde: { '~b': 'paragraph return', '~a': 'U+FFFC anchored object marker', '~s': 'U+202F', '~S': 'U+00A0', '~_': 'U+2014', '~<': 'U+2009 thin space', '~#': 'any page number marker', '~N': 'current page number marker' },
        },
        traps: [
            { written: '\\p{L} \\p{S} \\p{P} \\p{N} \\p{M}', reads: 'lowercase, whitespace, nothing, nothing, nothing', write: '\\p{Letter} \\p{Symbol} \\p{Punctuation} \\p{Number} \\p{Mark}' },
            { written: '\\uXXXX', reads: 'an uppercase letter, then the digits', write: '\\x{XXXX}' },
            { written: '\\x{D83D}\\x{DE00}', reads: 'nothing', write: '\\x{1F600}' },
            { written: '[a-z\\p{Lu}]', reads: '\\p{Lu} as literal characters', write: '(?:[a-z]|\\p{Lu}), negated (?![a-z]|\\p{Lu})(?s:.)' },
            { written: '^a|bc, c$|x', reads: 'the anchor binds every alternative', write: '(?<![^\\r])a|bc, c(?![^\\r])|x' },
            { written: '(?<=\\$|USD)', reads: 'invalid', write: '(?:(?<=\\$)|(?<=USD)), negated (?<!\\$)(?<!USD)' },
            { written: '(?<=a+), (?<=a{1,2})', reads: 'invalid', write: 'a+\\K' },
            { written: '\\p{Han}, \\p{Script=Han}, \\p{IsArabic}', reads: 'nothing', write: 'a class of code points' },
            { written: '\\n', reads: 'the forced line break U+000A alone', write: '\\r ends a paragraph' },
            { written: '^ $', reads: 'paragraph start and end', write: '(?-m)^ for the story start' },
            { written: '.', reads: 'any character but \\r, special characters and markers included', write: '(?s) to cross paragraphs' },
            { written: 'a case-insensitive option', reads: 'absent from findChangeGrepOptions', write: '(?i)' },
        ],
        change: {
            groups: '$0 to $9, one digit each: $10 is $1 then 0',
            case: { '\\U': 'uppercases what follows', '\\u': 'capitalizes', '\\L': 'lowercases' },
            inserts: { '\\r': 'paragraph return', '~b': 'paragraph return', '\\t': 'tab', '\\n': 'forced line break', '~~': 'nonbreaking hyphen', '~8': 'bullet', '~_': 'em dash', '\\x{2014}': 'em dash', '\\$': '$', '\\\\': '\\' },
            literal: ['\\1', '$&', 'braced and named group references', '\\E', '$$'],
        },
    };
    const inspected = Effect.fnUntraced(function* (target: string) {
        return yield* call((client) => client.Inspect({ target: Option.some(target), fields: Option.none() }));
    });
    const reference = Effect.map(Effect.flatMap(inspected('SpecialCharacters'), flow(Schema.encodeUnknownEffect(Inspect.successSchema.cases.Enumeration), Effect.orDie)), ({ members }) => JSON.stringify({ ...dialect, specialCharacters: members }));
    const queries = call((client) => client.Queries());
    yield* McpServer.registerResource({ uri, name: 'InDesign GREP dialect', description: 'Constructs InDesign GREP supports, the forms it reads differently with their working equivalents, Change To grammar, and special-character names', mimeType: 'application/json', content: reference });
    yield* McpServer.registerResource({
        uri: 'indesign://grep/queries',
        name: 'Saved Find/Change queries',
        description: 'User and factory Find/Change queries of every mode with their find, change, and options records',
        mimeType: 'application/json',
        content: Effect.flatMap(queries, flow(Schema.encodeEffect(Schema.fromJsonString(Queries.successSchema)), Effect.orDie)),
    });
    yield* McpServer.registerResource`indesign://grep/query/${McpSchema.param('name', Schema.String)}`({
        name: 'Saved Find/Change query',
        description: 'One saved Find/Change query by name',
        mimeType: 'application/json',
        completion: { name: (input) => Effect.map(queries, (rows) => [...ranked(input, Array.map(rows, Struct.get('name')))]) },
        content: (_uri, name) =>
            Effect.flatMap(queries, (rows) =>
                Effect.flatMap(
                    Effect.fromOption(
                        Array.findFirst(rows, (row) => row.name === name),
                        () => new GalleyError({ reason: { _tag: 'Missing', subject: `saved query ${name}`, query: name, candidates: ranked(name, Array.map(rows, Struct.get('name'))), range: Option.none() } }),
                    ),
                    flow(Schema.encodeEffect(Schema.fromJsonString(Queries.successSchema.value)), Effect.orDie),
                ),
            ),
    });
    yield* McpServer.registerPrompt({
        name: 'grep',
        description: 'Compose and run an InDesign GREP find/change with the dialect reference and the current find/change preferences',
        parameters: { task: Schema.String, scope: Schema.String },
        completion: {
            scope: Effect.fnUntraced(function* (input: string) {
                const state = yield* call((client) => client.Inspect({ target: Option.none(), fields: Option.none() }));
                const active = Option.flatMap(Option.liftPredicate(state, Inspect.successSchema.guards.State), ({ status }) => (status._tag === 'Ready' ? status.active : Option.none()));
                const listed = yield* Effect.transposeOption(
                    Option.map(active, ({ document, selection }) =>
                        Effect.map(inspected(`${document}/story`), (stories) => [
                            document,
                            ...Array.map(selection, Struct.get('specifier')),
                            ...Inspect.successSchema.match(stories, { Items: ({ items }) => Array.map(items, Struct.get('specifier')), Object: ({ specifier }) => [specifier], State: () => [], Class: () => [], Enumeration: () => [] }),
                        ]),
                    ),
                );
                return [
                    ...ranked(
                        input,
                        Option.getOrElse(listed, () => []),
                    ),
                ];
            }),
        },
        content: ({ task, scope }) =>
            Effect.map(Effect.all([reference, Effect.forEach(['/@find-grep-preferences', '/@change-grep-preferences', '/@find-change-grep-options'], flow(inspected, Effect.flatMap(flow(Schema.encodeEffect(Inspect.successSchema), Effect.orDie))))]), ([text, preferences]) => [
                McpSchema.PromptMessage.make({ role: 'user', content: McpSchema.EmbeddedResource.make({ resource: McpSchema.TextResourceContents.make({ uri, mimeType: 'application/json', text }) }) }),
                McpSchema.PromptMessage.make({ role: 'user', content: McpSchema.TextContent.make({ text: JSON.stringify(preferences) }) }),
                McpSchema.PromptMessage.make({ role: 'user', content: McpSchema.TextContent.make({ text: `${task}\nScope: ${scope}\nCompose the pattern in InDesign's dialect, run find_change with preview, read the matches, then run find_change without preview to apply.` }) }),
            ]),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { grep };
