import { executor } from '@rasm/nx-devkit/executor';
import { Schema } from 'effect';
import { applied, StackRequest } from '#/executors/stacks';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({
    programs: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Program packages, each resolved from the project and exporting stacks(request)' }),
    destroy: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Destroys the local stacks and removes them' }),
}).annotate({ title: 'Install', description: "Applies the stacks a program package states for a project's install on this machine through the Pulumi Automation API, refreshing live state first" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Install, ({ programs, ...selection }, context, project) => applied(programs, StackRequest.Install({ context }), project, selection));

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
