// The matched hooks over each answered turn, the spoken summary and the finding classifier each registered under its option

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';
import { type Entry, findings, isKind, key, keys } from '../host/store.ts';
import { FINDING_VIEWS, finding, summarize } from '../policies/findings.ts';
import { classifierPrompt, fieldsOf, LABELS } from '../policies/kinds.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// The one model both calls of the turn run on, the summary and the classifier
const _MODEL = 'haiku';
const _SUMMARY_TOKENS = 60;
const _SUMMARY_SYSTEM = 'The message is an assistant answer to the person, reply with one spoken sentence summarising it and nothing else.';

// --- [REGISTRATION] --------------------------------------------------------------------

// A failed call drops the line and never the turn
const _speak = (on: On): void => {
    on('turn.complete', { reason: 'answer' }, async ($, e, next) => {
        const result = await next(e);
        if (e.answer !== '') {
            await $.model
                .complete({ model: _MODEL, system: _SUMMARY_SYSTEM, prompt: e.answer, maxTokens: _SUMMARY_TOKENS })
                .then((line) => (line === '' ? undefined : $.audio.speak(line)))
                .catch(() => undefined);
        }
        return result;
    });
};

// The none label and a failed call end the classification, the fork runs for a kind alone and a grounded reply alone writes a row
const _classify = (on: On): void => {
    on('turn.complete', { reason: 'answer' }, async ($, e, next) => {
        const result = await next(e);
        if (e.answer === '') {
            return result;
        }
        const label = await $.model.classify(e.answer, LABELS, { model: _MODEL }).catch((): undefined => undefined);
        if (!isKind(label)) {
            return result;
        }
        const fields = fieldsOf(await $.model.fork({ prompt: classifierPrompt(label, e.answer) }).catch((): null => null));
        if (fields === undefined) {
            return result;
        }
        const row = finding({ session: await $.session.id(), kind: label, fields, now: $.clock.now(), random: crypto.randomUUID() });
        await $.store.set(row.key, row.row);
        // The summary row is rebuilt beside the findings row, the band and the block read the summary alone
        const all = await $.store.keys();
        const entries = await Promise.all(keys('findings')(all).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) })));
        await $.store.set(key('summary'), summarize(findings(entries).map((entry) => entry.row)));
        FINDING_VIEWS.map((view) => $.ui.invalidate(view));
        return result;
    });
};

const turnComplete = (on: On, options: Options): void => {
    if (options.speak) {
        _speak(on);
    }
    if (options.classify) {
        _classify(on);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { turnComplete };
