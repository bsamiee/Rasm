import { emitReadySignal } from '@nx/devkit';
import type { StorybookConfigRaw } from 'storybook/internal/types';

// --- [COMPOSITION] ---------------------------------------------------------------------

const experimental_devServer = (app: NonNullable<StorybookConfigRaw['experimental_devServer']>) => {
    app.server.once('listening', () => emitReadySignal(app.server));
    return app;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { experimental_devServer };
