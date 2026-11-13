import { executor } from '@rasm/nx-devkit/executor';
import { Schema } from 'effect';
import { applied, StackRequest } from '#/executors/stacks';

// --- [MODELS] --------------------------------------------------------------------------

const Deploy = Schema.Struct({
    programs: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Program packages, each resolved from the project and exporting stacks(request)' }),
    production: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Asks each program for the stacks of a released version' }),
    stack: Schema.optionalKey(Schema.String).annotate({ description: 'Name of the one stack to apply, absent applies every stack in dependency order' }),
    destroy: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Destroys the stacks and removes them' }),
}).annotate({ title: 'Deploy', description: 'Applies the stacks a program package states for a project through the Pulumi Automation API, group after group in dependency order, refreshing live state first' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Deploy, ({ programs, production = false, ...selection }, context, project) => applied(programs, StackRequest.Deploy({ context, production }), project, selection));

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
