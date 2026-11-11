import { Dry, dried, executor } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Schema, Struct } from 'effect';
import { sourced } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Dry.mapFields(
    Struct.assign({
        removeData: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Also removes the data folders UXP hosts keep for this plug-in id, for a clean install' }),
        variables: Schema.optionalKey(Schema.Tuple([Schema.String, Schema.String])).annotate({ description: 'Environment variables holding the base64 .p12 and its password that sign a CEP extension; unset, a self-signed certificate ZXPSignCmd creates once in the Nx workspace data folder signs it' }),
    }),
).annotate({ title: 'Place', description: "Places the unpacked plug-in the project's builds wrote into the host's per-user plug-in folder in one exchange, registers a UXP plug-in in its host's registry file, and signs a CEP extension before placing it" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Options,
    Effect.fnUntraced(function* ({ removeData, variables, ...stated }, context) {
        const [dry, target] = yield* Effect.all([dried(stated), Effect.fromNullishOr(context.targetName)]);
        const { project, manifests } = yield* sourced(context, Option.fromUndefinedOr(context.configurationName), target);
        const placements = Array.getSomes(manifests.map(({ host, folder, names, text }) => Option.map(Option.fromUndefinedOr(host.place), (place) => place({ folder, names, text, dry, removeData: removeData === true, variables: Option.fromUndefinedOr(variables) }))));
        yield* Array.match(placements, {
            onEmpty: () => ToolError.make({ subject: project, detail: 'finds no plug-in manifest a host loads from a folder in what its builds wrote' }),
            onNonEmpty: (all) => Effect.all(all, { discard: true }),
        });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
