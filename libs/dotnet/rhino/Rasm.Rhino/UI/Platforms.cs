using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;

namespace Rasm.Rhino.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IHostPlatform {
    public HostEvent<EventArgs> Changed { get; }

    public Font Digits(Font font);

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInLifetime owner);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPlatform {
    public static IHostPlatform Current => Platform.Instance.CreateShared<IHostPlatform>();
}
