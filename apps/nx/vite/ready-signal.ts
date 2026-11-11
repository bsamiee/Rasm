import type { Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAME = '@rasm/nx-vite/ready-signal';

// --- [COMPOSITION] ---------------------------------------------------------------------

const readySignal = (): Plugin => ({
    name: NAME,
    configureServer: ({ httpServer }) => {
        httpServer?.once('listening', () => import('@nx/devkit').then(({ emitReadySignal }) => emitReadySignal(httpServer)));
    },
    configurePreviewServer: ({ httpServer }) => {
        httpServer.once('listening', () => import('@nx/devkit').then(({ emitReadySignal }) => emitReadySignal(httpServer)));
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { NAME, readySignal };
