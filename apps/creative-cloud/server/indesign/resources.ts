// --- [IMPORTS] -------------------------------------------------------------------------

import type { McpUiResourceMeta } from '@modelcontextprotocol/ext-apps';
import { RESOURCE_MIME_TYPE } from '@modelcontextprotocol/ext-apps/server';
import { Array, Data, Effect, Encoding, flow, Layer, Option, Order, Path, Record, Schema, String, Struct, Tuple } from 'effect';
import { McpSchema, McpServer } from 'effect/unstable/ai';
import { defaultTreeAdapter, html, serialize } from 'parse5';
import { build } from 'vite';
import { enumerationDocumentation, enumerations, phrases, properties } from './indesign.ts';

// --- [REFERENCE] -----------------------------------------------------------------------

const _names = Array.sort(Record.keys(enumerations), Order.String);
const _matching = (fragment: string): string[] => Array.filter(_names, flow(String.toLowerCase, String.includes(String.toLowerCase(fragment))));
const _slots = Array.flatMap(Record.toEntries(properties), ([owner, fields]) => Array.map(Record.toEntries(fields), ([property, slot]) => ({ owner, property, slot })));
const _uses = Array.groupBy(
    Array.flatMap(_slots, ({ owner, property, slot }) => Array.map(slot.enumerations, (name) => ({ name, owner, property, types: slot.types, list: slot.list }))),
    Struct.get('name'),
);
const _reference = Record.map(enumerations, (constants, name) => {
    const documentation = Record.get(enumerationDocumentation, name);
    return {
        name,
        description: Array.flatMap(Option.toArray(documentation), Struct.get('description')),
        constants: Record.fromIterableWith(Object.entries(constants), ([member, value]) => {
            const lookup = Record.get(member);
            return Tuple.make(member, {
                value,
                expression: `${name}.${member}`,
                description: Array.flatten(Option.toArray(Option.flatMap(Option.map(documentation, Struct.get('members')), lookup))),
                ...Option.match(Option.flatMap(Record.get(phrases, name), lookup), { onNone: () => ({}), onSome: (phrase) => ({ phrase }) }),
            });
        }),
        properties: Array.map(
            Option.getOrElse(Record.get(_uses, name), () => []),
            Struct.omit(['name']),
        ),
    };
});

const enumResources: Layer.Layer<never> = McpServer.resource`enum://${McpSchema.param('enumName', Schema.String)}`({
    name: 'indesign-enumeration',
    description: 'Offline Adobe InDesign enumeration reference, including Adobe documentation, constants, native phrases and the owner properties that accept each enumeration.',
    mimeType: 'application/json',
    completion: { enumName: flow(_matching, Effect.succeed) },
    content: (uri, name) =>
        Effect.succeed({
            contents: [
                {
                    uri,
                    mimeType: 'application/json',
                    text: JSON.stringify(
                        Option.match(
                            Array.findFirst(_names, (key) => key === name),
                            {
                                onNone: () => ({ kind: 'unknownEnumeration', name, candidates: _matching(name), totalEnums: _names.length }),
                                onSome: (key) => _reference[key],
                            },
                        ),
                    ),
                },
            ],
        }),
});

// --- [PRESENTATION] --------------------------------------------------------------------

class SnapshotBuildFailed extends Data.TaggedError('SnapshotBuildFailed')<{ readonly cause: unknown }> {}

const SNAPSHOT_VIEW = 'ui://indesign/snapshot';
const snapshotResources: Layer.Layer<never, never, Path.Path> = Layer.unwrap(
    Effect.map(
        Effect.cached(
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const entry = yield* path.fromFileUrl(new URL('./snapshot-view.ts', import.meta.url));
                const result = yield* Effect.tryPromise({
                    try: () =>
                        build({
                            configFile: false,
                            root: path.dirname(entry),
                            envDir: false,
                            publicDir: false,
                            logLevel: 'warn',
                            clearScreen: false,
                            build: { write: false, lib: { entry, formats: ['es'] }, rolldownOptions: { output: { codeSplitting: false } } },
                        }),
                    catch: (cause) => new SnapshotBuildFailed({ cause }),
                });
                const [
                    {
                        output: [{ code }],
                    },
                ] = yield* Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ output: Schema.Tuple([Schema.Struct({ code: Schema.String })]) })]))(result);
                const source = new URL(`data:text/javascript;base64,${Encoding.encodeBase64(code)}`);
                const document = defaultTreeAdapter.createDocument();
                defaultTreeAdapter.setDocumentType(document, html.TAG_NAMES.HTML, '', '');
                defaultTreeAdapter.appendChild(
                    document,
                    defaultTreeAdapter.createElement(html.TAG_NAMES.SCRIPT, html.NS.HTML, [
                        { name: 'type', value: 'module' },
                        { name: 'src', value: source.href },
                    ]),
                );
                return {
                    contents: [
                        {
                            uri: SNAPSHOT_VIEW,
                            mimeType: RESOURCE_MIME_TYPE,
                            text: serialize(document),
                            _meta: { ui: { csp: { resourceDomains: [source.protocol] } } satisfies McpUiResourceMeta },
                        },
                    ],
                };
            }),
        ),
        (content) => McpServer.resource({ uri: SNAPSHOT_VIEW, name: 'indesign-snapshot', description: 'Interactive InDesign snapshot with fit, pan and zoom.', mimeType: RESOURCE_MIME_TYPE, content }),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { enumResources, SNAPSHOT_VIEW, snapshotResources };
