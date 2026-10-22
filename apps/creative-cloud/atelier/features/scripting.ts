import { Automation, Execute, type Installation, Kind, Perform, Terms } from '@rasm/automation';
import { ExecuteScript } from '@rasm/indesign';
import { Attachment, Instructions, registerResourceTemplate, registerRoutedTool } from '@rasm/mcp';
import { transformSync } from 'amaro';
import { Array, Context, Effect, Function, Match, Option, Predicate, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { InDesignScript } from '#/indesign-script';
import { instance, lookup, type routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

const Form = Schema.TaggedUnion({ Command: Execute.fields, Element: Perform.fields });

// --- [COMPOSITION] ---------------------------------------------------------------------

const dictionaries = Effect.gen(function* () {
    const [automation, indesignScript] = yield* Effect.all([Automation, InDesignScript]);
    const scriptable = (hosts: Predicate.Predicate<Installation>) => (row: Installation) => row.scriptable && !hosts(row);
    const described = (given: string) => Effect.flatMap(indesignScript.hosts, (hosts) => lookup(scriptable(hosts))(Option.some(given)));
    const [application, kind] = [McpSchema.param('application', Schema.String), McpSchema.param('kind', Kind)];
    const completion = { application: () => Effect.zipWith(automation.installed, indesignScript.hosts, (rows, hosts) => rows.filter(scriptable(hosts)).map(Struct.get('name'))) };
    yield* registerResourceTemplate`adobe://dictionary/${application}`({
        name: 'dictionary',
        description: SchemaAST.resolveDescription(Terms.ast),
        mimeType: 'application/json',
        schema: Terms,
        completion,
        changes: SubscriptionRef.changes(automation.installations),
        content: (_, given) => Effect.flatMap(described(given), automation.terms),
    });
    yield* registerResourceTemplate`adobe://dictionary/${application}/${kind}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML of every declaration with the kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes: SubscriptionRef.changes(automation.installations),
        completion: {
            ...completion,
            kind: () => Effect.succeed([...Kind.literals]),
            name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                const chosen = yield* Schema.decodeUnknownEffect(Schema.Struct({ application, kind }))(context?.arguments);
                return Array.map((yield* automation.terms(yield* described(chosen.application)))[chosen.kind], Struct.get('name'));
            }),
        },
        content: (_, given, selected, term) => Effect.flatMap(described(given), ({ url }) => automation.definition({ url, kind: selected, name: term })),
    });
});
const command = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>, { reported, refused }: Effect.Success<typeof status>) {
    const [automation, hosts] = yield* Effect.all([Automation, InDesignScript.use(Struct.get('hosts'))]);
    const forms = { Command: Predicate.not(hosts), Element: Function.constTrue } satisfies Record<(typeof Form.Type)['_tag'], Predicate.Predicate<Installation>>;
    const tool = Tool.make('command', {
        description: "Runs a dictionary command of an application other than InDesign, or Accessibility actions on an application's status elements; Element actions answer a dialog requiring an answer, Command is refused while one shows, and answering a dialog an application shows while launching can activate it",
        parameters: Schema.Struct({}),
        success: Schema.Union([Execute.Result.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment, Schema.ErrorInstance())) })), Schema.Void]),
    });
    yield* registerRoutedTool({
        ...application,
        fields: { ...application.fields, run: Form },
        resolve: ({ run, application: given, pid }) => Effect.map(instance(lookup(forms[run._tag])(given), pid), (chosen) => ({ ...chosen, run })),
        depends: ({ pid, run }) => Effect.succeed([run._tag === 'Element' ? reported(pid) : refused(pid)]),
    })(tool, (_, { pid, run }) =>
        Match.valueTags(run, {
            Command: (given) => automation.execute({ ...given, pid }),
            Element: (perform) => automation.perform({ ...perform, pid }),
        }),
    );
});
const executeScript = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, indesignScript] = yield* Effect.all([Automation, InDesignScript]);
    const hosts = yield* indesignScript.hosts;
    const tool = Tool.make('execute_script', {
        description: "Runs source in InDesign as UXP JavaScript, TypeScript stripped of types, and in any other application in the runtime of its dictionary's script command, whose errors arrive as the host raises them; ExtendScript alert, confirm, and prompt activate their application before showing",
        parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body with its return value as result' }) }),
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
            ? Effect.scoped(Effect.flatMap(Effect.all([Effect.try(() => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length)), indesignScript.client(pid)]), ([source, call]) => call('execute_script', { code: source })))
            : Effect.map(automation.script(pid, code), (result) => ({ result }));
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { command, dictionaries, executeScript };
