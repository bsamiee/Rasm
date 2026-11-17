import { HostError } from '@rasm/object-model';
import { crossed } from '@rasm/testing/errors';
import { Option } from 'effect';
import { describe, expect, it } from 'vitest';

describe('HostError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(HostError.thrown('place.jsx', Option.some(12))())).toStrictEqual(['line 12: place.jsx', 'line 12: place.jsx']);
    });
});
