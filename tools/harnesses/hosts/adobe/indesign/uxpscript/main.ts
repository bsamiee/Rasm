import uxp from 'adobe:uxp';
import { Predicate, Result, Schema } from 'effect';
import { answerBatch } from '#/run';
import { Strings } from '#/values';

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.Struct({ script: Schema.Struct({ args: Strings, setResult: Schema.declare(Predicate.isFunction) }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const { script } = Result.getOrThrow(Schema.decodeUnknownResult(Host)(uxp));
script.setResult(answerBatch(script.args[0]));
