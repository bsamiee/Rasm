import { Unmatched } from '@rasm/search';
import { crossed } from '@rasm/testing/errors';
import { Option } from 'effect';
import { describe, expect, it } from 'vitest';

describe('Unmatched', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(Unmatched.of({ subject: 'layer', query: Option.some('Walls'), candidates: ['Walls', 'Doors'] }))).toStrictEqual(['No layer matching Walls, candidates: Walls and 1 more', 'No layer matching Walls, candidates: Walls and 1 more']);
    });
});
