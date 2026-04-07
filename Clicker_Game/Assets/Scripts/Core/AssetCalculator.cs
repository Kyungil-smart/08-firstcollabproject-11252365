using System.Collections.Generic;
using UnityEngine;

public class AssetCalculator : MonoBehaviour
{
    [Header("====총 자산 계산 참조====")]
    [Tooltip("UpgradeRoot에 배치된 UpgradeStateController\n자동 투자 업그레이드 상태와 원본 데이터를 읽을 때 사용")]
    [SerializeField] private UpgradeStateController _UpgradeStateController;

    public double CalculateAsset(double currentMoney)
    {
        return currentMoney + CalculateAutoUpgradeTotalPurchasedCost();
    }

    private double CalculateAutoUpgradeTotalPurchasedCost()
    {
        if (_UpgradeStateController == null) return 0d;

        IReadOnlyList<UpgradeDataSO> definitions = _UpgradeStateController.UpgradeDefinitions;
        if (definitions == null) return 0d;

        double totalCost = 0d;

        foreach (UpgradeDataSO definition in definitions)
        {
            if (definition == null) continue;
            if (definition.Category != UpgradeCategory.AutoUpgrade) continue;
            
            int purchaseCount = _UpgradeStateController.GetPurchaseCount(definition.Id);
            totalCost += definition.GetTotalPurchasedCost(purchaseCount);
        }
        
        return totalCost;
    }
}
