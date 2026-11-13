import { Manifested } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Effect, Schema } from 'effect';
import { analyzer } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Versioned = Schema.fromJsonString(Schema.Struct({ version: Schema.NullOr(Schema.String), project: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const versioned = (text: string, marketing: readonly string[]) =>
    Effect.flatMap(
        Effect.flatMap(analyzer, (core) => printed(core, ['version', ...marketing], { input: text })),
        Schema.decodeEffect(Versioned),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class XcodeVersionActions extends Manifested {
    override stated(text: string, at: string) {
        return Effect.flatMap(versioned(text, []), ({ version }) => Effect.mapError(Effect.fromNullishOr(version), ToolError.of(at, 'states no MARKETING_VERSION')));
    }

    override restated(text: string, version: string) {
        return Effect.map(versioned(text, ['--marketing-version', version]), ({ project }) => project);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default XcodeVersionActions;
