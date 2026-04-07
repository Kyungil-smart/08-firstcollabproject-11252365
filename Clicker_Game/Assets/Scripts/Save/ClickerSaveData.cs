using System;
using System.Collections.Generic;

[Serializable]
public class ClickerSaveData
{
    public double Money;
    public List<UpgradeStateSaveData> UpgradeStates = new();
}
