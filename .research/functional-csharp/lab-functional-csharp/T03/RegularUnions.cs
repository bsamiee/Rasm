namespace Lab.T03;

[Union]
internal abstract partial record OrderState {
    public abstract bool CanCancel();

    internal sealed record Placed(string CreatedBy) : OrderState {
        public override bool CanCancel() => true;
    }

    internal sealed record Processing(DateTime StartedAt) : OrderState {
        public override bool CanCancel() => true;
    }

    internal sealed record Shipped(DateTime ShippedAt, string TrackingNumber) : OrderState {
        public override bool CanCancel() => false;
    }
}
