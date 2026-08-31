namespace Lab.T03;

internal sealed record ShipRequest(DateTime Now, string TrackingNumber, bool CanShip);

internal static class OrderTransitions {
    public static OrderState Ship(OrderState state, ShipRequest request) =>
        state.Switch<ShipRequest, OrderState>(request,
            placed: static (_, placed) => placed,
            processing: static (ship, processing) => ship.CanShip ? new OrderState.Shipped(ship.Now, ship.TrackingNumber) : processing,
            shipped: static (_, shipped) => shipped);
}
