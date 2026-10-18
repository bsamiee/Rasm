import { expect, test } from './fixtures.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('editor dialog holds its actions, status, and tabs', async ({ owner }) => {
    await expect(owner.getByRole('dialog', { name: 'Portfolio editor.' })).toMatchAriaSnapshot(`
      - dialog "Portfolio editor.":
        - heading "Portfolio editor." [level=2]
        - button "Close portfolio editor"
        - button "Save draft"
        - button "Preview draft"
        - button "Publish"
        - status: Private draft
        - group:
          - tablist "Portfolio editing":
            - tab "Projects and studies" [selected]
            - tab "Files"
            - tab "Identity and hero"
          - tabpanel "Projects and studies":
            - button "New project"
            - button "New study"
            - grid "Projects and studies"
    `);
});
test('entry removal asks for confirmation and returns focus to the kept entry', async ({ owner }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const title = `Harbor Library ${testId}`;
    const entry = editor.getByRole('grid', { name: 'Projects and studies' }).getByRole('row', { name: title });
    await editor.getByRole('button', { name: 'New project' }).click();
    await editor.getByRole('textbox', { name: 'Title' }).fill(title);
    await editor.getByRole('button', { name: 'Change project' }).click();
    await entry.getByRole('button', { name: 'Remove entry' }).click();
    await expect(owner.getByRole('alertdialog', { name: 'Remove entry?' })).toMatchAriaSnapshot(`
      - alertdialog "Remove entry?":
        - heading "Remove entry?" [level=2]
        - paragraph: Remove this entry from your draft. Source files remain in the library. Publish to apply this change to the public site.
        - button "Keep"
        - button "Remove entry"
    `);
    await expect(owner.getByRole('alertdialog').getByRole('button', { name: 'Keep' })).toBeFocused();
    await owner.keyboard.press('Enter');
    await expect(owner.getByRole('alertdialog')).toBeHidden();
    await expect(entry).toBeFocused();
});
