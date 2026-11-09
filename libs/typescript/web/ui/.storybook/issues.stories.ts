import { Option, Result, Schema, SchemaParser } from 'effect';
import { fn } from 'storybook/test';
import preview from '#/.storybook/preview';
import { Issues } from '#/issues.tsx';

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ name: Schema.NonEmptyString, entries: Schema.Array(Schema.Struct({ title: Schema.NonEmptyString })) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({ component: Issues.List, args: { issue: SchemaParser.decodeResult(Identity)({ name: '', entries: [{ title: '' }] }, { errors: 'all' }).pipe(Result.getFailure, Option.getOrThrow) } });

const Listed = meta.story();
const Linked = meta.story({ args: { onField: fn() } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Linked, Listed };
