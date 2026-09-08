import type { ModelForkReply } from 'claude-code';
import { describe, expect, it } from 'vitest';
import { KIND_NAMES } from '../host/store.ts';
import { classifierPrompt, fieldsOf, hasEvidence, KINDS, LABELS } from './kinds.ts';

const _USAGE: ModelForkReply['usage'] = {
    ['input_tokens']: 0,
    ['output_tokens']: 0,
    ['cache_read_input_tokens']: 0,
    ['cache_creation_input_tokens']: 0,
};

const _reply = (fields: Readonly<Record<string, unknown>>): ModelForkReply => ({ text: JSON.stringify(fields), usage: _USAGE });

describe('kinds', () => {
    it('lists every kind with the none label and states each criterion in the prompt', () => {
        expect(LABELS).toStrictEqual([...KIND_NAMES, 'none']);
        expect(Object.keys(KINDS)).toStrictEqual([...KIND_NAMES]);
        expect(classifierPrompt('stale', 'the answer')).toContain(`weakness of kind "stale": ${KINDS.stale.criterion}`);
        expect(classifierPrompt('stale', 'the answer').endsWith('---\nthe answer')).toBe(true);
    });

    it('accepts a command with output, a documentation sentence, or two locations as evidence and a recollection as none', () => {
        expect(hasEvidence('$ ls\nfile')).toBe(true);
        expect(hasEvidence('docs/a.md says the tool moved on')).toBe(true);
        expect(hasEvidence('a.md:1 b.md:2')).toBe(true);
        expect(hasEvidence('I recall it was so')).toBe(false);
    });

    it('reads the fields of a grounded reply, null section and change as empty, and nothing from a null, malformed, or ungrounded one', () => {
        expect(fieldsOf(_reply({ file: 'f', section: null, evidence: 'a:1 b:2', change: null }))).toStrictEqual({
            file: 'f',
            section: '',
            evidence: 'a:1 b:2',
            change: '',
        });
        expect(fieldsOf(_reply({ file: null, section: 's', evidence: 'a:1 b:2', change: 'c' }))).toBeUndefined();
        expect(fieldsOf(_reply({ file: 'f', section: 's', evidence: 'recalled', change: 'c' }))).toBeUndefined();
        expect(fieldsOf({ text: '{', usage: _USAGE })).toBeUndefined();
        expect(fieldsOf(null)).toBeUndefined();
    });
});
