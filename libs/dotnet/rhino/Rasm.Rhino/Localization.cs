using System.Globalization;
using Rhino.UI;

namespace Rasm.Rhino;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LocalText {
    public static CultureInfo Culture => CultureInfo.GetCultureInfo(Localization.CurrentLanguageId);

    public static LocalizeStringPair Of(string english, params ReadOnlySpan<object?> arguments) {
        LocalizeStringPair pair = Localization.LocalizeCommandOptionName(english, Session.Assembly, 0);
        return arguments.IsEmpty ? pair : new(string.Format(Culture, pair.English, arguments), string.Format(Culture, pair.Local, arguments));
    }
}
