using Eto;
using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IHostPlatform {
    public HostEvent<Unit> Changed { get; }

    public int DropDownIconSize { get; }

    public int RenderTabIconSize { get; }

    public IO<Unit> Digits(Control control);

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, bool> tick);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPlatform {
    public static IHostPlatform Current => Platform.Instance.CreateShared<IHostPlatform>();
}
