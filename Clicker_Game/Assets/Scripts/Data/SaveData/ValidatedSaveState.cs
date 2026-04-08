using System.Collections.Generic;

public class ValidatedSaveState
{
    public double Money { get; }
    public IReadOnlyDictionary<string, int> PurchaseCountsById { get; }

    public ValidatedSaveState(double money, IReadOnlyDictionary<string, int> purchaseCountsById)
    {
        Money = money;
        PurchaseCountsById = purchaseCountsById != null
            ? new Dictionary<string, int>(purchaseCountsById)
            : new Dictionary<string, int>();
    }
}