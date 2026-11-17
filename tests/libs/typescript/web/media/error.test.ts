import { MediaError } from '@rasm/media';
import { crossed } from '@rasm/testing/errors';
import { describe, expect, it } from 'vitest';

describe('MediaError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(MediaError.of('Protected', 'statement.pdf')())).toStrictEqual(['Protected: statement.pdf', 'Protected: statement.pdf']);
    });
});
