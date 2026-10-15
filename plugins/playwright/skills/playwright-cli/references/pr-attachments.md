# [PR_ATTACHMENTS]

`gh pr` and `gh issue` create, comment, and edit commands upload a `screenshot` or `video-start` output through the repeatable `--attach` flag:
- PNG, JPEG, GIF, WebP, SVG, MP4, MOV, and WebM files upload, up to 50 distinct files per command
- Image alt text follows the path after `#`, the file name without its extension and dots stands in, a video fails `cannot set alt text on video`
- Body references `![alt](<path>)` to an attached file, matched by absolute path, take the uploaded URL and keep their alt text
- Video references alone in their paragraph render as a player, inside a sentence as a link
- Unreferenced files append at the end of the body in flag order
- Uploads run in flag order and stop at the first failure, the body keeps the uploaded files, the command exits non-zero and prints the URL
- `gh` refuses images over 10 MB and videos over 100 MB, GitHub refuses videos over 10 MB on a free-plan repository
- Uploads need write access and an OAuth or personal access token, an Actions `GITHUB_TOKEN` fails `unsupported authentication type`
- GitHub Enterprise Server takes no upload, `gh pr create` with `--web` or `--dry-run` takes no `--attach`
- `ffmpeg` converts a recording's WebM to the H.264 MP4 GitHub recommends for playback in every browser

## [01]-[SESSION]

```bash
playwright cli open http://localhost:3000/settings
playwright cli screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/settings-after.png
playwright cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/settings-flow.webm
playwright cli click e5
playwright cli fill e7 "New name" --submit
playwright cli video-stop
playwright cli close
ffmpeg -i $PLAYWRIGHT_MCP_OUTPUT_DIR/settings-flow.webm -c:v libx264 -pix_fmt yuv420p -movflags +faststart \
    $PLAYWRIGHT_MCP_OUTPUT_DIR/settings-flow.mp4

gh pr create --title "Keep the name after save" --body-file body.md \
    --attach "$PLAYWRIGHT_MCP_OUTPUT_DIR/settings-after.png#Settings page after saving" \
    --attach $PLAYWRIGHT_MCP_OUTPUT_DIR/settings-flow.mp4
gh pr comment 123 --body "Recorded the new flow end to end." --attach $PLAYWRIGHT_MCP_OUTPUT_DIR/settings-flow.mp4
```
