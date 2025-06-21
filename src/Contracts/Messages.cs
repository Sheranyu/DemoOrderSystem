namespace Contracts;

public record CreateOrder(int OrderId);
public record OrderCreated(int OrderId);
public record InvoiceCreated(int OrderId);
public record PaymentBooked(int OrderId);
public record OrderShipped(int OrderId);
