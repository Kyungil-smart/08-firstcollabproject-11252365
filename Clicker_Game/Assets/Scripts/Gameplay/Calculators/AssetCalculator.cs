using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AssetCalculator : MonoBehaviour
{
    [FormerlySerializedAs("_UpgradeStateController")]
    [Header("====총 자산 계산 참조====")]
    [Tooltip("UpgradeRoot에 배치된 UpgradeStateController\n자동 투자 업그레이드 상태와 원본 데이터를 읽을 때 사용")]
    [SerializeField] private UpgradeStateController _upgradeStateController;

    public double CalculateAsset(double currentMoney)
    {
        return currentMoney + CalculateAutoUpgradeTotalPurchasedCost();
    }

    private double CalculateAutoUpgradeTotalPurchasedCost()
    {
        if (_upgradeStateController == null) return 0d;

        IReadOnlyList<UpgradeDataSO> definitions = _upgradeStateController.UpgradeDefinitions;
        if (definitions == null) return 0d;

        double totalCost = 0d;

        foreach (UpgradeDataSO definition in definitions)
        {
            if (definition == null) continue;
            if (definition.Category != UpgradeCategory.AutoUpgrade) continue;
            
            int purchaseCount = _upgradeStateController.GetPurchaseCount(definition.Id);
            totalCost += definition.GetTotalPurchasedCost(purchaseCount);
        }
        
        return totalCost;
    }
}
