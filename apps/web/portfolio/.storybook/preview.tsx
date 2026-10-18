import { RegistryProvider } from '@effect/atom-react';
import { addons } from '@rasm/test-support/preview';
import { definePreview } from '@storybook/react-vite';
import '../site/theme.css';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default definePreview({
    addons,
    tags: ['autodocs'],
    parameters: { layout: 'fullscreen' },
    decorators: [
        (Story) => (
            <RegistryProvider>
                <Story />
            </RegistryProvider>
        ),
    ],
});
