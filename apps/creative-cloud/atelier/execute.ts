import { defaulted, unit } from '@rasm/mcp/bridge';
import { register, type routed, WRITING } from '@rasm/mcp/extension';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema, String, Struct } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import type { applications } from './applications.ts';
import { type Application, Scripting, ScriptingError } from './scripting.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const execute = Effect.fn('execute')(function* ({ runs, preview }: Effect.Success<ReturnType<typeof routed>>, { fields, reach }: Effect.Success<ReturnType<typeof applications>>) {
    const [scripting, files, path] = yield* Effect.all([Scripting, FileSystem.FileSystem, Path.Path]);
    const steps: Record.ReadonlyRecord<string, { readonly command: string; readonly arguments: Schema.JsonObject }> = {
        'com.adobe.InDesign': {
            command: 'do script',
            arguments: {
                language: 'javascript',
                direct: `(function (given) {
        var source = given[0], name = given[1], preview = given[2], rest = [], opened = {}, out, done = false, at;
        for (at = 3; at < given.length; at++) rest.push(given[at]);
        for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
        try {
            app.doScript(function () { out = (function () { return eval(source); }).apply(null, rest); done = true; if (preview) throw new Error(name); }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT, name);
        } catch (error) {
            if (!(done && preview)) throw error;
        }
        for (at = app.documents.length - 1; preview && at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.NO);
        return out;
    })(arguments)`,
            },
        },
        'com.adobe.Photoshop': {
            command: 'do javascript',
            arguments: {
                direct: `(function (given) {
        var source = given[0], name = given[1], preview = given[2], rest = [], opened = {}, document = app.documents.length ? app.activeDocument : null, state = document ? document.activeHistoryState : null, at;
        if (preview && !document) throw new Error('Photoshop records history per document and none is open: open a document to preview');
        for (at = 3; at < given.length; at++) rest.push(given[at]);
        for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
        var undone = function () {
            for (at = app.documents.length - 1; at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.DONOTSAVECHANGES);
            if (document) { app.activeDocument = document; document.activeHistoryState = state; }
        };
        $.global.atelier = function () { $.global.atelier.out = (function () { return eval(source); }).apply(null, rest); };
        try {
            if (document) document.suspendHistory(name, 'atelier()'); else $.global.atelier();
        } catch (error) {
            undone();
            throw error;
        }
        if (preview) undone();
        return $.global.atelier.out;
    })(arguments)`,
            },
        },
    };
    const Command = Schema.TaggedStruct('Command', {
        command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
        arguments: defaulted(
            Schema.JsonObject.annotate({
                description: 'Dictionary parameters by name, direct for the direct parameter, in the JSON form replies use; a reply value goes into a later request unchanged',
            }),
            {},
        ),
        artifacts: defaulted(Schema.Array(Schema.URL).annotate({ description: 'File URLs the command writes, returned with the reply' }), []),
    }).annotate({ description: 'Scripting dictionary command sent as an Apple Event' });
    const forms = Array.map(runs, (run) => ({ form: Schema.TaggedStruct(run.tool.name, run.tool.parametersSchema.fields).annotate({ description: Tool.getDescription(run.tool) }), serve: run.serve }));
    const tool = Tool.make('execute', {
        description: "Runs a scripting dictionary command, or a form an application's extension adds, in one application",
        parameters: fields.pipe(Schema.fieldsAssign({ ...unit(preview).fields, run: Schema.Union([Command, ...Array.map(forms, Struct.get('form'))]) })),
        success: Schema.Union([Schema.Struct({ reply: Schema.Json, failures: Schema.Array(Schema.String) }), ...Array.map(runs, (run) => run.tool.successSchema)]),
    }).annotateMerge(WRITING);
    const command = Effect.fn('command')(function* (application: typeof Application.Type, { command: name, arguments: given, artifacts }: typeof Command.Type, description: string, previewed: boolean) {
        const step = Option.filter(Record.get(steps, application.identifier), (row) => row.command === name && Predicate.isString(given.direct) && URL.parse(given.direct)?.protocol !== 'file:' && (given.language ?? row.arguments.language) === row.arguments.language);
        const sent = yield* Option.match(step, {
            onNone: () => (previewed ? Effect.fail(ScriptingError.make({ reason: { _tag: 'Unpreviewable', application: application.name } })) : Effect.succeed(given)),
            onSome: (row) => Effect.succeed({ ...given, ...row.arguments, 'with arguments': [given.direct ?? '', description, previewed, ...Option.getOrElse(Schema.decodeUnknownOption(Schema.Array(Schema.Json))(given['with arguments']), Array.empty)] }),
        });
        const reply = yield* scripting.call((sender) => sender.execute({ application: application.url, command: name, arguments: sent }));
        const [blocks, failures] = yield* Effect.partition(
            artifacts,
            (artifact) =>
                Effect.map(Effect.all([scripting.call((sender) => sender.contentType({ url: artifact })), Effect.flatMap(path.fromFileUrl(artifact), files.readFile)], { concurrency: 'unbounded' }), ([mimeType, bytes]) =>
                    Option.match(Option.filter(mimeType, String.startsWith('image/')), {
                        onSome: (image): McpSchema.ContentBlock => McpSchema.ImageContent.make({ data: bytes, mimeType: image }),
                        onNone: () => McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri: artifact.href, blob: bytes, mimeType: Option.getOrUndefined(mimeType) }) }),
                    }),
                ),
            { concurrency: 'unbounded' },
        );
        return [{ reply, failures: Array.map(failures, Struct.get('message')) }, blocks] as const;
    });
    yield* register(tool, (parameters) => {
        const { description, preview: previewed, run } = parameters;
        return Schema.is(Command)(run)
            ? Option.match(
                  Option.filter(previewed, (given) => given !== true),
                  {
                      onSome: () => Effect.fail(McpSchema.InvalidParams.make({ message: 'preview with run Command takes true: a dictionary command renders no image' })),
                      onNone: () => Effect.map(reach(parameters, Option.some), (application) => [Option.some(description), command(application, run, description, Option.isSome(previewed))] as const),
                  },
              )
            : Effect.flatMap(
                  reach(parameters, (target) => Array.findFirst(forms, ({ form, serve }) => (Schema.is(form)(run) ? serve(target) : Option.none()))),
                  (serve) => serve(run, { description, preview: previewed }),
              );
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
