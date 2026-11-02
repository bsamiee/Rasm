import { Execute, Kind, Terms } from '@rasm/automation/apple-events';
import { type Installation, Perform } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { ExecuteScript } from '@rasm/indesign';
import { Attachment } from '@rasm/mcp/content';
import { registerResourceTemplate } from '@rasm/mcp/resource';
import { Instructions } from '@rasm/mcp/server';
import { registerRoutedTool } from '@rasm/mcp/tool';
import { transformSync } from 'amaro';
import { Array, Cause, Context, Effect, Function, Option, Predicate, Schema, SchemaAST, SchemaGetter, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { InDesignScript } from '#/indesign-script';
import { instance, lookup, type routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

const Form = Schema.TaggedUnion({ Command: Execute.fields, Element: Perform.fields });
const Dictionary = Schema.Struct({ application: McpSchema.param('application', Schema.String), kind: McpSchema.param('kind', Kind) });
const Reply = Schema.Union([Schema.NullOr(Schema.Literal('')).pipe(Schema.decodeTo(ExecuteScript.Result, { decode: SchemaGetter.succeed({}), encode: SchemaGetter.forbiddenEncoding })), Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(ExecuteScript.Result)))]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const dictionaries = Effect.gen(function* () {
    const [automation, indesignScript] = yield* Effect.all([Automation, InDesignScript]);
    const scriptable = (hosts: Predicate.Predicate<Installation>) => (row: Installation) => row.scriptable && !hosts(row);
    const described = (given: string) => Effect.flatMap(indesignScript.hosts, (hosts) => lookup(scriptable(hosts))(Option.some(given)));
    const completion = { application: () => Effect.zipWith(automation.installed, indesignScript.hosts, (rows, hosts) => rows.filter(scriptable(hosts)).map(Struct.get('name'))) };
    yield* registerResourceTemplate`adobe://dictionary/${Dictionary.fields.application}`({
        name: 'dictionary',
        description: SchemaAST.resolveDescription(Terms.ast),
        mimeType: 'application/json',
        schema: Terms,
        completion,
        changes: SubscriptionRef.changes(automation.installations),
        content: (_, given) => Effect.flatMap(described(given), automation.terms),
    });
    yield* registerResourceTemplate`adobe://dictionary/${Dictionary.fields.application}/${Dictionary.fields.kind}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML matching kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes: SubscriptionRef.changes(automation.installations),
        completion: {
            ...completion,
            kind: () => Effect.succeed([...Kind.literals]),
            name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                const chosen = yield* Schema.decodeEffect(Schema.toCodecStringTree(Dictionary))(context?.arguments);
                return Array.map((yield* automation.terms(yield* described(chosen.application)))[chosen.kind], Struct.get('name'));
            }),
        },
        content: (_, given, selected, term) => Effect.flatMap(described(given), ({ url }) => automation.definition({ url, kind: selected, name: term })),
    });
});
const command = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>, { reported, refused }: Effect.Success<typeof status>) {
    const [automation, hosts] = yield* Effect.all([Automation, InDesignScript.use(Struct.get('hosts'))]);
    const tool = Tool.make('command', {
        description: 'Command runs outside InDesign, refused while a dialog requires an answer. Element runs Accessibility actions on status elements, an answer during launch can activate application',
        parameters: Schema.Struct({}),
        success: Schema.Union([Execute.Result.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment, Schema.ErrorInstance())) })), Schema.Void]),
    });
    yield* registerRoutedTool({
        ...application,
        fields: { ...application.fields, run: Form },
        resolve: ({ run, application: given, pid }) => Effect.map(instance(lookup(Form.match(run, { Command: () => Predicate.not(hosts), Element: () => Function.constTrue }))(given), pid), (chosen) => ({ ...chosen, run })),
        depends: ({ pid, run }) => Effect.succeed([Form.match(run, { Command: () => refused(pid), Element: () => reported(pid) })]),
    })(tool, (_, { pid, run }) =>
        Form.match(run, {
            Command: (given) => automation.execute({ ...given, pid }),
            Element: (perform) => automation.perform({ ...perform, pid }),
        }),
    );
});
const executeScript = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, indesignScript] = yield* Effect.all([Automation, InDesignScript]);
    const hosts = yield* indesignScript.hosts;
    const tool = Tool.make('execute_script', {
        description: 'Runs UXP JavaScript or type-stripped TypeScript in InDesign, dictionary script command elsewhere, host errors unchanged. ExtendScript alert, confirm, and prompt activate application',
        parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body returning result' }) }),
        success: ExecuteScript.Result,
    }).annotateMerge(
        Context.make(
            Instructions,
            Array.map(Array.fromNullishOr(SchemaAST.resolveDescription(ExecuteScript.ast)), (text) => `execute_script: ${text}`),
        ),
    );
    yield* registerRoutedTool(application)(tool, ({ code }, { installation, pid }) => {
        const [open, close] = ['async function body() {\n', '\n}'];
        return hosts(installation)
            ? Effect.scoped(
                  Effect.flatMap(Effect.all([Effect.try({ try: () => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length), catch: (cause) => new Cause.UnknownError(cause, 'type strip') }), indesignScript.client(pid)]), ([source, call]) =>
                      call('execute_script', { code: source }),
                  ),
              )
            : automation
                  .script({
                      pid,
                      source: `(function (body) {
    function encoded(value) {
        var parts = [], key, at, kind;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        kind = Object.prototype.toString.call(value);
        if (kind === "[object Array]") { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (kind !== "[object Object]") return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }
    var value = eval("(function () {" + decodeURIComponent(body) + "\\n})").call(this);
    return value === undefined ? "" : encodeURIComponent('{"result":' + encoded(value) + '}');
}).call(this, "${encodeURIComponent(code)}")`,
                  })
                  .pipe(Effect.flatMap(Schema.decodeUnknownEffect(Reply)));
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { command, dictionaries, executeScript };
