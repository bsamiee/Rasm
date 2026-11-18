import { crossed } from '@rasm/testing/errors';
import { TypographyError } from '@rasm/typography';
import { describe, expect, it } from 'vitest';

describe('TypographyError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(TypographyError.unsupported('Inter.woff3'))).toStrictEqual(['Unsupported Inter.woff3', 'Unsupported Inter.woff3']);
    });

    it('should keep the message it derives from a foreign cause across structured clone and Nx serialization', () => {
        const error = TypographyError.unreadable('mushaf JSON')(new SyntaxError('Unexpected end of JSON input'));

        expect(crossed(error)).toStrictEqual([error.message, error.message]);
    });
});
