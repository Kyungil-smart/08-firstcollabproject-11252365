using System.Collections.Generic;

public class SaveDataValidator
{
    public bool TryValidate(
        ClickerSaveData saveData,
        IReadOnlyDictionary<string, UpgradeRuntimeState> stateById,
        out ValidatedSaveState validatedState)
    {
        validatedState = null;

        if (saveData == null) return false;
        if (stateById == null || stateById.Count == 0) return false;

        if (double.IsNaN(saveData.Money)) return false;
        if (double.IsInfinity(saveData.Money)) return false;
        if (saveData.Money < 0d) return false;
        if (saveData.UpgradeStates == null) return false;

        Dictionary<string, int> purchaseCountsById = new();
        foreach (UpgradeStateSaveData loadedState in saveData.UpgradeStates)
        {
            if (loadedState == null) return false;
            if (string.IsNullOrWhiteSpace(loadedState.UpgradeId)) return false;
            if (loadedState.PurchaseCount < 0) return false;
            if (!stateById.ContainsKey(loadedState.UpgradeId)) return false;
            if (purchaseCountsById.ContainsKey(loadedState.UpgradeId)) return false;
            
            purchaseCountsById.Add(loadedState.UpgradeId, loadedState.PurchaseCount);
        }
        
        validatedState = new ValidatedSaveState(saveData.Money, purchaseCountsById);
        return true;
    }
}