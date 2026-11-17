import { crossed } from '@rasm/testing/errors';
import { TypographyError } from '@rasm/typography';
import { describe, expect, it } from 'vitest';

describe('TypographyError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(TypographyError.unsupported('Inter.woff3'))).toStrictEqual(['Unsupported Inter.woff3', 'Unsupported Inter.woff3']);
    });
});
