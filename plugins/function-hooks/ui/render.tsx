import type { Elements, RenderElement, RenderSurface } from 'claude-code';
import { none, type Option, some } from '../composition.ts';
import type { Capture, Notice, Service } from '../hooks/state.d.ts';
import { basename } from '../policies/invocation.ts';
import { caption, diff } from './capture.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Row {
    readonly label: 'hooks' | 'services';
    readonly facts: string;
    readonly button: Option<{ readonly hotkey: string; readonly label: string; readonly onPress: () => void }>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [BAND]

const bandRows = (notice: Option<Notice>, down: readonly Service[], restart: () => void): readonly Row[] => [
    ...(notice.kind === 'none' ? [] : [{ label: 'hooks' as const, facts: notice.value.text, button: none }]),
    ...(down.length === 0 ? [] : [{ label: 'services' as const, facts: down.map(({ name, port }) => `${name} down (${port})`).join(' · '), button: some({ hotkey: '1', label: 'restart', onPress: restart }) }]),
];

const band = ({ Box, Button, Text }: Elements[RenderSurface], rows: readonly Row[], below: RenderElement): RenderElement => (
    <Box flexDirection="column">
        {below}
        {rows.map((row) => (
            <Box flexDirection="row" justifyContent="space-between" key={row.label}>
                <Box>
                    <Box flexShrink={0} width={2}>
                        <Text color="suggestion">✦</Text>
                    </Box>
                    <Box flexShrink={0} width={10}>
                        <Text dimColor={true}>{row.label}</Text>
                    </Box>
                    <Text wrap="truncate">{row.facts}</Text>
                </Box>
                {row.button.kind === 'none' ? null : (
                    <Box flexShrink={0} marginLeft={2}>
                        <Button hotkey={row.button.value.hotkey} label={row.button.value.label} onPress={row.button.value.onPress} plain={true} />
                    </Box>
                )}
            </Box>
        ))}
    </Box>
);

// --- [RESULTS]

const resultRow = ({ Box, Text }: Elements[RenderSurface], below: RenderElement, line: string): RenderElement => (
    <Box flexDirection="column">
        {below}
        <Box flexDirection="row">
            <Box flexShrink={0} paddingLeft={2} width={5}>
                <Text dimColor={true}>⎿</Text>
            </Box>
            <Text dimColor={true} wrap="wrap">
                {line}
            </Text>
        </Box>
    </Box>
);

const captureRow = (elements: Elements['terminal'], below: RenderElement, capture: Capture, columns: number): RenderElement => {
    const { Box, Image } = elements;
    const indent = 5;
    const widest = 255;
    const described = caption(basename(capture.path), capture.record);
    const compared = diff(capture);
    const pictures = [capture.path, ...(compared.kind === 'some' ? [compared.value] : [])];
    return (
        <Box flexDirection="column">
            {resultRow(elements, below, described)}
            <Box flexDirection="row" paddingLeft={indent}>
                {pictures.map((file) => (
                    <Image alt={described} columns={Math.min(widest, Math.floor((columns - indent) / pictures.length))} key={file} rows={20} source={{ file, format: 'png', generation: capture.generation }} />
                ))}
            </Box>
        </Box>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { band, bandRows, captureRow, resultRow };
