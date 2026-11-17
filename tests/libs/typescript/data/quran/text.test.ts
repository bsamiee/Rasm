import { QuranError } from '@rasm/quran';
import { crossed } from '@rasm/testing/errors';
import { describe, expect, it } from 'vitest';

describe('QuranError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        const error = QuranError.of('mushaf JSON')(new SyntaxError('Unexpected end of JSON input'));

        expect(crossed(error)).toStrictEqual([error.message, error.message]);
    });
});
