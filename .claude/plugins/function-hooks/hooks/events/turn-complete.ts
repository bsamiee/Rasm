// The matched hooks over each answered turn, the spoken summary and the finding classifier each registered under its option

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { flatMap, forEach, fromBoolean, fromPredicate, liftPredicate, map, none, type Option } from '../composition/option.ts';
import { type Options, whenEnabled } from '../host/options.ts';
import { decodeSummary, isKind, type Kind, key, type Summary } from '../host/store.ts';
import { FINDING_VIEWS, type FindingInput, finding, withFinding } from '../policies/findings.ts';
import { CLASSIFIER_PROMPT, type Fields, fieldsOf, LABELS } from '../policies/kinds.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// The one model both calls of the turn run on, the summary and the classifier
const _MODEL = 'haiku';
const _SUMMARY_TOKENS = 60;
const _SUMMARY_SYSTEM = 'The message is an assistant answer to the person, reply with one spoken sentence summarising it and nothing else.';

// --- [REGISTRATION] --------------------------------------------------------------------

// Each step holds its own catch, a failed call drops the line and never the turn
const _speak = (on: On): void => {
    on('turn.complete', { reason: 'answer' }, async ($, e, next) => {
        const result = await next(e);
        // Failed completions speak nothing, and the answer is data under the system line
        const line = await forEach(() => $.model.complete({ model: _MODEL, system: _SUMMARY_SYSTEM, prompt: e.answer, maxTokens: _SUMMARY_TOKENS }))(
            fromBoolean(e.answer !== ''),
        ).catch((): Option<string> => none());
        await forEach((spoken: string) => $.audio.speak(spoken))(flatMap(liftPredicate<string>((text) => text !== ''))(line)).catch(() => undefined);
        return result;
    });
};

// Each step holds its own catch, a failed call drops the row and never the turn
const _classify = (on: On): void => {
    on('turn.complete', { reason: 'answer' }, async ($, e, next) => {
        const result = await next(e);
        // The none label and a failed call end the classification here, the fork runs for a kind alone
        const label = await forEach(() => $.model.classify(e.answer, LABELS, { model: _MODEL }))(fromBoolean(e.answer !== '')).catch(
            (): Option<string | undefined> => none(),
        );
        const kind = flatMap(fromPredicate(isKind))(label);
        // The fork reads the session's own transcript and answers null in place of a rejection, and fieldsOf reads a row from a grounded reply alone
        const reply = await forEach((named: Kind) => $.model.fork({ prompt: CLASSIFIER_PROMPT(named, e.answer) }))(kind);
        const session = await $.session.id();
        const input = flatMap((fields: Fields) =>
            map((named: Kind): FindingInput => ({ session, kind: named, fields, now: $.clock.now(), random: crypto.randomUUID() }))(kind),
        )(flatMap(fieldsOf)(reply));
        // The summary row session.start wrote is rewritten beside the findings row, the band and the block read the summary alone
        await forEach(async (row: FindingInput): Promise<void> => {
            const keyed = finding(row);
            const summary = decodeSummary(await $.store.get(key('summary')));
            await forEach((current: Summary) =>
                Promise.all([$.store.set(keyed.key, keyed.row), $.store.set(key('summary'), withFinding(current, keyed.row))]),
            )(summary);
            FINDING_VIEWS.map((view) => $.ui.invalidate(view));
        })(input).catch(() => undefined);
        return result;
    });
};

const turnComplete = (on: On, options: Options): void => {
    whenEnabled(options.speak, () => _speak(on));
    whenEnabled(options.classify, () => _classify(on));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { turnComplete };
