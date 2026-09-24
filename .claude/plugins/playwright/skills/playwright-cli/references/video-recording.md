# Video Recording

Capture browser automation sessions as video for debugging, documentation, or verification. Produces WebM (VP8/VP9 codec).

## Basic Recording

`video-chapter` adds a chapter marker at each section transition:

```bash
playwright-cli open

playwright-cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/demo.webm

playwright-cli video-chapter "Getting Started" --description="Opening the homepage" --duration=2000

playwright-cli goto https://example.com
playwright-cli snapshot
playwright-cli click e1

playwright-cli video-chapter "Filling Form" --description="Entering test data" --duration=2000
playwright-cli fill e2 "test input"

playwright-cli video-stop
```

## Best Practices

### 1. Use Descriptive Filenames

```bash
playwright-cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/login-flow-2024-01-15.webm
playwright-cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/checkout-test-run-42.webm
```

### 2. Record entire hero scripts.

When recording a video for the user or as a proof of work, it is best to create a code snippet and execute it with run-code.
It allows inserting appropriate pauses between the actions and annotating the video. There are new Playwright APIs for that.

1) Perform scenario using CLI and take note of all locators and actions. You'll need those locators to request their bounding boxes for highlight.
2) Create a file with the intended script for video (below). Use pressSequentially w/ delay for nice typing, make reasonable pauses.
3) Use playwright-cli run-code --filename your-script.js

**Important**: Overlays are `pointer-events: none` — they do not interfere with page interactions. You can safely keep sticky overlays visible while clicking, filling, or performing any actions on the page.

- `page.screencast.showChapter` serves simple cases, blocks until its duration expires, then removes its card
- `page.screencast.showOverlay` builds a custom card in place of `showChapter`

```js
async page => {
  await page.screencast.start({ path: 'video.webm', size: { width: 1280, height: 800 } });
  await page.goto('https://demo.playwright.dev/todomvc');

  await page.screencast.showChapter('Adding Todo Items', {
    description: 'We will add several items to the todo list.',
    duration: 2000,
  });

  await page.getByRole('textbox', { name: 'What needs to be done?' }).pressSequentially('Walk the dog', { delay: 60 });
  await page.getByRole('textbox', { name: 'What needs to be done?' }).press('Enter');
  await page.waitForTimeout(1000);

  await page.screencast.showChapter('Verifying Results', {
    description: 'Checking the item appeared in the list.',
    duration: 2000,
  });

  const annotation = await page.screencast.showOverlay(`
    <div style="position: absolute; top: 8px; right: 8px;
      padding: 6px 12px; background: rgba(0,0,0,0.7);
      border-radius: 8px; font-size: 13px; color: white;">
      ✓ Item added successfully
    </div>
  `);

  await page.getByRole('textbox', { name: 'What needs to be done?' }).pressSequentially('Buy groceries', { delay: 60 });
  await page.getByRole('textbox', { name: 'What needs to be done?' }).press('Enter');
  await page.waitForTimeout(1500);

  await annotation.dispose();

  const bounds = await page.getByText('Walk the dog').boundingBox();
  await page.screencast.showOverlay(`
    <div style="position: absolute;
      top: ${bounds.y}px;
      left: ${bounds.x}px;
      width: ${bounds.width}px;
      height: ${bounds.height}px;
      border: 1px solid red;">
    </div>
    <div style="position: absolute;
      top: ${bounds.y + bounds.height + 5}px;
      left: ${bounds.x + bounds.width / 2}px;
      transform: translateX(-50%);
      padding: 6px;
      background: #808080;
      border-radius: 10px;
      font-size: 14px;
      color: white;">Check it out, it is right above this text
    </div>
  `, { duration: 2000 });

  await page.screencast.stop();
}
```

Embrace creativity, overlays are powerful.

### Overlay API Summary

| Method | Use Case |
|--------|----------|
| `page.screencast.showChapter(title, { description?, duration?, styleSheet? })` | Full-screen chapter card with blurred backdrop — ideal for section transitions |
| `page.screencast.showOverlay(html, { duration? })` | Custom HTML overlay — use for callouts, labels, highlights |
| `disposable.dispose()` | Remove a sticky overlay added without duration |
| `page.screencast.hideOverlays()` / `page.screencast.showOverlays()` | Temporarily hide/show all overlays |

## Tracing vs Video

| Feature | Video | Tracing |
|---------|-------|---------|
| Output | WebM file | Trace file (viewable in Trace Viewer) |
| Shows | Visual recording | DOM snapshots, network, console, actions |
| Use case | Demos, documentation | Debugging, analysis |
| Size | Larger | Smaller |

## Limitations

- Recording adds slight overhead to automation
- Large recordings can consume significant disk space
