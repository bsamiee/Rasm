import { crossed } from '@rasm/testing/errors';
import { WebhookError } from '@rasm/webhooks';
import { describe, expect, it } from 'vitest';

describe('WebhookError', () => {
    it('should keep the message it derives across structured clone and Nx serialization', () => {
        expect(crossed(WebhookError.of({ _tag: 'Mismatch' }, 'stripe delivery')())).toStrictEqual(['Mismatch: stripe delivery', 'Mismatch: stripe delivery']);
    });
});
