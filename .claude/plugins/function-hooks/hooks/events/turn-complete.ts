// The matched hook over each answered turn, the spoken summary and the finding classifier each under its option

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { forEach, fromBoolean, fromPredicate, getOrElse, map, none, type Option } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { isKind, type Kind } from '../host/store.ts';
import { FINDING_VIEWS, type FindingInput, finding } from '../policies/findings.ts';
import { CLASSIFIER_PROMPT, type Fields, fieldsOf, LABELS } from '../policies/kinds.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// The one model both calls of the turn run on, the summary and the classifier
const _MODEL = 'haiku';
const _SUMMARY_TOKENS = 60;
const _SUMMARY_SYSTEM = 'The message is an assistant answer to the person, reply with one spoken sentence summarising it and nothing else.';
const _CONFIDENCE = 1;

// --- [REGISTRATION] --------------------------------------------------------------------

const turnComplete = (on: On, options: Options): void => {
    // The matcher pins the answered turns, the body tests the answer's text alone
    on('turn.complete', { reason: 'answer' }, async ($, e, next) => {
        const result = await next(e);
        const answered = e.answer !== '';
        // Both arms read the answer alone, each inside its own catch, a failed call drops the line or the row and never the turn
        await Promise.all([
            forEach(async (): Promise<void> => {
                // Failed completions speak nothing, and the answer is data under the system line
                const line = await $.model
                    .complete({ model: _MODEL, system: _SUMMARY_SYSTEM, prompt: e.answer, maxTokens: _SUMMARY_TOKENS })
                    .catch(() => '');
                fromBoolean(line !== '').match<void>({
                    some: () => {
                        $.audio.speak(line).catch(() => undefined);
                    },
                    none: () => undefined,
                });
            })(fromBoolean(options.speak && answered)),
            forEach(() =>
                (async (): Promise<void> => {
                    // The none label and an undefined answer end the arm here, the fork runs for a kind alone
                    const label = await $.model.classify(e.answer, LABELS, { model: _MODEL });
                    const input = getOrElse<Option<FindingInput>>(none)(
                        await forEach(async (kind: Kind): Promise<Option<FindingInput>> => {
                            // The fork reads the session's own transcript, and fieldsOf reads a row from a grounded reply alone
                            const reply = await $.model.fork({ prompt: CLASSIFIER_PROMPT(kind, e.answer) });
                            const session = await $.session.id();
                            return map(
                                (fields: Fields): FindingInput => ({
                                    session,
                                    kind,
                                    fields,
                                    confidence: _CONFIDENCE,
                                    now: $.clock.now(),
                                    random: crypto.randomUUID(),
                                }),
                            )(fieldsOf(reply));
                        })(fromPredicate(isKind)(label)),
                    );
                    await forEach(async (row: FindingInput): Promise<void> => {
                        const keyed = finding(row);
                        await $.store.set(keyed.key, keyed.row);
                        FINDING_VIEWS.map((view) => $.ui.invalidate(view));
                    })(input);
                })().catch(() => undefined),
            )(fromBoolean(options.classify && answered)),
        ]);
        return result;
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { turnComplete };
