import { ObjectStorageError } from '@rasm/object-storage';
import { crossed } from '@rasm/testing/errors';
import { describe, expect, it } from 'vitest';

describe('ObjectStorageError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(ObjectStorageError.of('Unavailable', 'media/cover.png')())).toStrictEqual(['Unavailable: media/cover.png', 'Unavailable: media/cover.png']);
    });
});
