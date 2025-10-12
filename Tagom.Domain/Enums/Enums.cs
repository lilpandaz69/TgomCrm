namespace TgomCRM.Domain.Enums
{
    public enum PaymentMethod
    {
        Cash = 0,
        Card = 1,
        BankTransfer = 2
    }

    public enum SaleStatus
    {
        Draft = 0,
        Completed = 1,
        Cancelled = 2
    }

    public enum StockMovementType
    {
        Inbound = 0,
        Outbound = 1
    }
}
