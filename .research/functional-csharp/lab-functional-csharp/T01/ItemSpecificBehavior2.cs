namespace Lab.T01;

[SmartEnum<string>]
internal abstract partial class NotificationChannel {
    public static readonly NotificationChannel Email = new Typed<string>("email");
    public static readonly NotificationChannel Sms = new Typed<int>("sms");

    public abstract Type PayloadType { get; }

    private sealed class Typed<TPayload>(string key) : NotificationChannel(key) {
        public override Type PayloadType => typeof(TPayload);
    }
}
