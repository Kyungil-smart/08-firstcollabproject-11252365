using System.Collections.Generic;
using UnityEngine;

public class AutoIncomeCalculator : MonoBehaviour
{
    [Header("====자동 수익 계산 참조")]
    [Tooltip("UpgradeRoot에 배치된 UpgradeStateController\n자동 투자 업그레이드 상태와 원본 데이터를 읽을 때 사용")]
    [SerializeField] private UpgradeStateController _upgradeStateController;

    public double BaseAutoIncomePerSecond => CalculateBaseAutoIncomePerSecond();
    public double AutoIncomePerSecond => CalculateAutoIncomePerSecond();

    private double CalculateBaseAutoIncomePerSecond()
    {
        return GetAutoUpgradeEffectSum(UpgradeEffectType.AutoIncomeFlat);
    }

    private double CalculateAutoIncomePerSecond()
    {
        double baseAutoIncomePerSecond = CalculateBaseAutoIncomePerSecond();
        double autoIncomeEfficiencyRate = GetAutoUpgradeEffectSum(UpgradeEffectType.AutoIncomeEfficiencyRate);
        double autoIncomeBonusRate = GetAutoUpgradeEffectSum(UpgradeEffectType.AutoIncomeBonusRate);

        return baseAutoIncomePerSecond
               * (1d + autoIncomeEfficiencyRate)
               * (1d + autoIncomeBonusRate)
               * GetCurrentMarketMultiplier();
    }
    
    private double GetAutoUpgradeEffectSum(UpgradeEffectType effectType)
    {
        if (_upgradeStateController == null) return 0d;

        IReadOnlyList<UpgradeDataSO> definitions = _upgradeStateController.UpgradeDefinitions;
        if (definitions == null) return 0d;

        double effectSum = 0d;

        foreach (UpgradeDataSO definition in definitions)
        {
            if (definition == null) continue;
            if (definition.Category != UpgradeCategory.AutoUpgrade) continue;
            if (definition.EffectType != effectType) continue;

            int purchaseCount = _upgradeStateController.GetPurchaseCount(definition.Id);
            effectSum += definition.GetCurrentEffectValue(purchaseCount);
        }

        return effectSum;
    }
    
    private double GetCurrentMarketMultiplier()
    {
        // TEMP : 시장 상태 시스템 아직없으므로 임시로 배율 1.0 사용
        return 1d;
    }
}
