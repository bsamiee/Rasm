import { assert, describe, it } from '@effect/vitest';
import { Array, Effect, Option, Order, Predicate, Ref, Schema, String } from 'effect';
import { McpProtocol } from 'effect/ai';
import { opened, PROTOCOLS } from '#/conformance';
import { EMBEDDED, FAILURE, IDS, PNG, STATIC, TEXT } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS: readonly string[] = PROTOCOLS.map(({ protocolVersion }) => protocolVersion);
const TOOLS = ['gated_read', 'gated_write', 'test_defect', 'test_embedded_resource', 'test_error_handling', 'test_image_content', 'test_simple_text', 'test_structured', 'test_tool_with_progress', 'test_value'];
const ABSENT = { modern: -32_602, legacy: -32_002 } as const;
const COMPLETED = { most: 100 } as const;
const WEATHER = { temperature: 22.5, conditions: 'Partly cloudy' };

// --- [MODELS] --------------------------------------------------------------------------

const Block = Schema.Struct({
    type: Schema.String,
    text: Schema.optionalKey(Schema.String),
    data: Schema.optionalKey(Schema.String),
    mimeType: Schema.optionalKey(Schema.String),
    resource: Schema.optionalKey(Schema.Struct({ uri: Schema.String, mimeType: Schema.optionalKey(Schema.String), blob: Schema.optionalKey(Schema.String), text: Schema.optionalKey(Schema.String) })),
});
const Called = Schema.Struct({ result: Schema.Struct({ content: Schema.Array(Block), isError: Schema.optionalKey(Schema.Boolean), structuredContent: Schema.optionalKey(Schema.Unknown) }) });
const Listed = Schema.Struct({
    result: Schema.Struct({
        tools: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                description: Schema.String,
                inputSchema: Schema.Struct({ type: Schema.Literal('object'), properties: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)) }),
                outputSchema: Schema.optionalKey(Schema.Struct({ properties: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)) })),
                annotations: Schema.optionalKey(Schema.Struct({ readOnlyHint: Schema.optionalKey(Schema.Boolean) })),
            }),
        ),
    }),
});
const Outcome = Schema.Struct({ error: Schema.optionalKey(Schema.Struct({ code: Schema.Finite, message: Schema.String })), result: Schema.optionalKey(Schema.Struct({ isError: Schema.optionalKey(Schema.Boolean) })) });
const Resources = Schema.Struct({ result: Schema.Struct({ resources: Schema.Array(Schema.Struct({ uri: Schema.String, name: Schema.String, description: Schema.optionalKey(Schema.String), mimeType: Schema.optionalKey(Schema.String) })) }) });
const Templates = Schema.Struct({ result: Schema.Struct({ resourceTemplates: Schema.Array(Schema.Struct({ uriTemplate: Schema.String, name: Schema.String })) }) });
const Read = Schema.Struct({ result: Schema.Struct({ contents: Schema.Tuple([Schema.Struct({ uri: Schema.String, mimeType: Schema.optionalKey(Schema.String), text: Schema.optionalKey(Schema.String), blob: Schema.optionalKey(Schema.String) })]) }) });
const Completed = Schema.Struct({ result: Schema.Struct({ completion: Schema.Struct({ values: Schema.Array(Schema.String), total: Schema.optionalKey(Schema.Finite) }) }) });
const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = Effect.fnUntraced(function* (protocol: (typeof PROTOCOLS)[number]) {
    const test = yield* opened(protocol);
    const held = yield* test.session;
    const tool = (name: string, given: Record<string, unknown> = {}) => test.answer(held, 'tools/call', { name, arguments: given });
    return { test, held, tool, content: (name: string) => Effect.map(Effect.flatMap(tool(name), Schema.decodeUnknownEffect(Called)), ({ result }) => result) };
});

const since = (protocol: (typeof PROTOCOLS)[number], first: (typeof PROTOCOLS)[number]) => VERSIONS.indexOf(protocol.protocolVersion) >= VERSIONS.indexOf(first.protocolVersion);

const unquoted = (text: string | undefined) => Option.getOrElse(Option.filter(Schema.decodeUnknownOption(Json)(text), Predicate.isString), () => text);

const outcome = (message: unknown) => Effect.map(Schema.decodeUnknownEffect(Outcome)(message), ({ error, result }) => ({ code: error?.code, failed: result?.isError === true }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(PROTOCOLS)('$protocolVersion tools', (protocol) => {
    const [annotated, structured, validating] = [since(protocol, McpProtocol.v2025_03_26), since(protocol, McpProtocol.v2025_06_18), since(protocol, McpProtocol.v2025_11_25)];

    it.live(
        'lists every visible tool with its description and an object input schema holding its parameters, lists no hidden tool, marks read-only tools from 2025-03-26, and states an output schema from 2025-06-18',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'tools/list'), Schema.decodeUnknownEffect(Listed));
            const listed = (name: string) => Array.findFirst(result.tools, (tool) => tool.name === name);
            assert.deepStrictEqual(
                {
                    names: Array.sort(
                        result.tools.map(({ name }) => name),
                        Order.String,
                    ),
                    readOnly: Array.sort(
                        result.tools.filter(({ annotations }) => annotations?.readOnlyHint === true).map(({ name }) => name),
                        Order.String,
                    ),
                    parameters: Option.map(listed('test_value'), ({ inputSchema }) => Object.keys(inputSchema.properties ?? {})),
                    structured: Option.map(listed('test_structured'), ({ outputSchema }) => Array.sort(Object.keys(outputSchema?.properties ?? {}), Order.String)),
                    described: Option.map(listed('test_simple_text'), ({ description }) => description),
                },
                {
                    names: TOOLS,
                    readOnly: Array.difference(TOOLS, annotated ? ['gated_write'] : TOOLS),
                    parameters: Option.some(['value']),
                    structured: Option.some(structured ? ['conditions', 'temperature'] : []),
                    described: Option.some('Returns simple text'),
                },
            );
        }, Effect.scoped),
    );

    it.live(
        'returns the text of a tool in a text block, verbatim or as the serialized JSON string a structured result mirrors',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks, isError } = yield* content('test_simple_text');
            assert.deepStrictEqual([blocks.filter(({ type }) => type === 'text').map(({ text }) => unquoted(text)), isError === true], [[TEXT], false]);
        }, Effect.scoped),
    );

    it.live(
        'returns image bytes as an image block holding base64 data and the media type',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks } = yield* content('test_image_content');
            assert.deepStrictEqual(
                blocks.filter(({ type }) => type === 'image').map(({ data, mimeType }) => ({ data, mimeType })),
                [{ data: PNG, mimeType: 'image/png' }],
            );
        }, Effect.scoped),
    );

    it.live(
        'returns a file as an embedded resource block holding its URI, media type, and its content as text or base64 bytes',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks } = yield* content('test_embedded_resource');
            assert.deepStrictEqual(
                blocks.filter(({ type }) => type === 'resource').map(({ resource }) => ({ uri: resource?.uri, mimeType: resource?.mimeType, held: resource?.text === EMBEDDED || resource?.blob === new TextEncoder().encode(EMBEDDED).toBase64() })),
                [{ uri: 'test://embedded-resource', mimeType: 'text/plain', held: true }],
            );
        }, Effect.scoped),
    );

    it.live(
        'returns a struct serialized in a text block, and as structured content from 2025-06-18',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks, structuredContent } = yield* content('test_structured');
            assert.deepStrictEqual([structuredContent, Array.getSomes(blocks.map(({ text }) => Schema.decodeUnknownOption(Json)(text)))], [structured ? WEATHER : undefined, [WEATHER]]);
        }, Effect.scoped),
    );

    it.live(
        'returns a failure of the tool as a result with isError and the failure text, never as a JSON-RPC error',
        Effect.fnUntraced(function* () {
            const { content } = yield* served(protocol);
            const { content: blocks, isError } = yield* content('test_error_handling');
            assert.deepStrictEqual([isError, blocks.some(({ text }) => text?.includes(FAILURE) === true)], [true, true]);
        }, Effect.scoped),
    );

    it.live(
        'answers a call to a tool it does not hold with JSON-RPC error -32602',
        Effect.fnUntraced(function* () {
            const { tool } = yield* served(protocol);
            assert.deepStrictEqual(yield* Effect.flatMap(tool('no_such_tool'), outcome), { code: -32_602, failed: false });
        }, Effect.scoped),
    );

    it.live(
        'reports a defect of a tool as an error holding no text of the defect',
        Effect.fnUntraced(function* () {
            const { tool } = yield* served(protocol);
            const message = yield* tool('test_defect');
            const { code, failed } = yield* outcome(message);
            assert.deepStrictEqual([code !== undefined || failed, (yield* Schema.encodeUnknownEffect(Json)(message)).includes('db.internal')], [true, false]);
        }, Effect.scoped),
    );

    it.live(
        'refuses a call to a hidden tool and never runs it',
        Effect.fnUntraced(function* () {
            const { test, tool } = yield* served(protocol);
            const { code, failed } = yield* Effect.flatMap(tool('test_hidden'), outcome);
            assert.deepStrictEqual([code !== undefined || failed, yield* Ref.get(test.probe.calls)], [true, []]);
        }, Effect.scoped),
    );

    it.live(
        'refuses arguments of the wrong type without running the tool: JSON-RPC error -32602 through 2025-06-18, a result with isError from 2025-11-25',
        Effect.fnUntraced(function* () {
            const { test, tool } = yield* served(protocol);
            const refusal = yield* Effect.flatMap(tool('test_value', { value: 123 }), outcome);
            assert.deepStrictEqual([refusal, yield* Ref.get(test.probe.calls)], [validating ? { code: undefined, failed: true } : { code: -32_602, failed: false }, []]);
        }, Effect.scoped),
    );
});

describe.each(PROTOCOLS)('$protocolVersion resources', (protocol) => {
    const absent = since(protocol, McpProtocol.v2026_07_28) ? ABSENT.modern : ABSENT.legacy;

    it.live(
        'lists each registered resource under the server scheme with its name, description, and media type',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'resources/list'), Schema.decodeUnknownEffect(Resources));
            assert.includeDeepMembers(
                result.resources.map(({ uri, name, description, mimeType }) => ({ uri, name, description, mimeType })),
                [
                    { uri: 'test://static-text', name: 'static-text', description: 'A static text resource for testing', mimeType: 'text/plain' },
                    { uri: 'test://static-binary', name: 'static-binary', description: 'A static binary resource for testing', mimeType: 'image/png' },
                    { uri: 'test://watched-resource', name: 'watched-resource', description: 'A resource whose value changes', mimeType: 'text/plain' },
                ],
            );
        }, Effect.scoped),
    );

    it.live.each([
        { uri: 'test://static-text', content: { mimeType: 'text/plain', text: STATIC } },
        { uri: 'test://static-binary', content: { mimeType: 'image/png', blob: PNG } },
        { uri: 'test://template/alpha/data', content: { mimeType: 'application/json', text: '{"id":"alpha","templateTest":true,"data":"Data for ID: alpha"}' } },
    ] as const)(
        'reads $uri as one content entry holding that URI, its media type, and its text or base64 bytes',
        Effect.fnUntraced(function* ({ uri, content }) {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'resources/read', { uri }), Schema.decodeUnknownEffect(Read));
            assert.deepStrictEqual(result.contents, [{ uri, ...content }]);
        }, Effect.scoped),
    );

    it.live(
        'answers a read of a resource that does not exist with the resource-not-found error of the revision: -32002 through 2025-11-25, -32602 from 2026-07-28',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            assert.deepStrictEqual(yield* Effect.flatMap(test.answer(held, 'resources/read', { uri: 'test://nonexistent-resource-for-conformance-testing' }), outcome), { code: absent, failed: false });
        }, Effect.scoped),
    );

    it.live(
        'lists a resource template by its URI template and name',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'resources/templates/list'), Schema.decodeUnknownEffect(Templates));
            assert.deepInclude(result.resourceTemplates, { uriTemplate: 'test://template/{id}/data', name: 'template' });
        }, Effect.scoped),
    );

    it.live(
        'completes a template argument with the values nearest the typed text first, at most 100 of them, and their count as total',
        Effect.fnUntraced(function* () {
            const { test, held } = yield* served(protocol);
            const { result } = yield* Effect.flatMap(test.answer(held, 'completion/complete', { ref: { type: 'ref/resource', uri: 'test://template/{id}/data' }, argument: { name: 'id', value: 'al' } }), Schema.decodeUnknownEffect(Completed));
            const [{ values, total }, nearest] = [result.completion, IDS.filter(String.startsWith('al'))];
            assert.deepStrictEqual({ nearest: Array.sort(values.slice(0, nearest.length), Order.String), bounded: values.length <= COMPLETED.most, total }, { nearest: Array.sort(nearest, Order.String), bounded: true, total: values.length });
        }, Effect.scoped),
    );
});
