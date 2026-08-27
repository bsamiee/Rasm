using Eto.Drawing;
using Grasshopper2.Framework;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Bridge.Cargo;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class Gh2Lane {
    private const int FallbackWidth = 1280;
    private const int FallbackHeight = 720;

    private readonly GhEditor editor;

    private Gh2Lane(GhEditor editor, int pluginsLoaded, int pluginsFailed) {
        this.editor = editor;
        PluginsLoaded = pluginsLoaded;
        PluginsFailed = pluginsFailed;
    }

    internal readonly record struct CaptureFile(string Path, int Width, int Height);

    internal static string Version => typeof(GhEditor).Assembly.GetName().Version?.ToString() ?? string.Empty;

    internal int PluginsLoaded { get; }
    internal int PluginsFailed { get; }
    internal static int Registered => ObjectProxies.Count;

    internal static Gh2Lane Acquire() {
        GhEditor live = GhEditor.Instance ?? GhEditor.ShowEditor(createVisible: false);
        FrozenSet<string> core = PluginServer.CorePlugins.ToFrozenSet(StringComparer.Ordinal);
        (int loaded, int failed) = PluginServer.LoadAllScopedPlugins(location => core.Contains(location) && !PluginServer.State.IsLocationLoaded(location));
        return new Gh2Lane(live, loaded, failed);
    }

    internal Fin<CaptureFile> DrawCanvas(string path) {
        if (editor is not { Canvas: { } canvas }) {
            return Error.New("Gh2Lane: editor canvas absent");
        }
        try {
            int width = canvas.Width > 0 ? canvas.Width : FallbackWidth;
            int height = canvas.Height > 0 ? canvas.Height : FallbackHeight;
            using Bitmap? bitmap = canvas.DrawToBitmap(width, height, drawBackground: true, drawWires: true, drawMessages: true);
            if (bitmap is null) {
                return Error.New("Canvas.DrawToBitmap returned null: GH2 swallowed a paint exception");
            }
            byte[] png = bitmap.ToByteArray(ImageFormat.Png);
            if (png.Length == 0) {
                return Error.New("Canvas.DrawToBitmap produced an empty PNG");
            }
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            string temp = path + ".tmp";
            File.WriteAllBytes(temp, png);
            File.Move(temp, path, overwrite: true);
            return new CaptureFile(path, width, height);
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Error.New($"canvas capture failed: {error.GetType().Name}: {error.Message}");
        }
    }
}
