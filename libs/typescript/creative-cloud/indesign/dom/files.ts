import type { InDesignDOMObject } from 'adobe:indesign';
import { Cause, Effect, flow, Option, Random, Schema } from 'effect';
import { Model } from './model.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const extended = <S extends Schema.Top>(schema: S, at: string, source: string, ...given: readonly string[]) =>
    Model.use(({ app, ScriptLanguage }) => Effect.try({ try: (): unknown => app.doScript(source, ScriptLanguage.JAVASCRIPT, [...given]), catch: (cause) => new Cause.UnknownError(cause, at) })).pipe(Effect.flatMap(flow(Schema.decodeUnknownEffect(schema), Effect.orDie)));
const file = (owner: InDesignDOMObject<unknown>, field: string) => Effect.map(extended(Schema.NullOr(Schema.String), `${owner.toSpecifier()} ${field}`, 'var value = resolve(arguments[0])[arguments[1]]; value === null ? null : value.fsName', owner.toSpecifier(), field), Option.fromNullOr);
const temporary = (name: string) =>
    Effect.acquireRelease(
        Effect.zipWith(extended(Schema.String, `temporary ${name}`, 'Folder.temp.fsName'), Random.nextInt, (folder, unique) => `${folder}/${unique}-${name}`),
        (path) => Effect.orDie(extended(Schema.Boolean, `removal of ${path}`, 'File(arguments[0]).remove()', path)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { file, temporary };
