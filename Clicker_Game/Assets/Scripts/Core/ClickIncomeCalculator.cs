using System.Collections.Generic;
using UnityEngine;

public class ClickIncomeCalculator : MonoBehaviour
{
    [Header("====클릭 수익 계산 참조====")]
    [Tooltip("UpgradeRoot에 배치된 UpgradeStateController\n" +
             "수동 거래 강화 업그레이드 상태와 원본 데이터 읽을때 사용")]
    [SerializeField] private UpgradeStateController _upgradeStateController;
    
    [Header("====클릭 수익 계산 기준 값====")]
    [Tooltip("업그레이드 효과를 더하기 전 기본 클릭 파워")]
    [SerializeField, Min(0f)] private double _baseClickPower = 1d;

    public double ClickPower => CalculateClickPower();
    public double ClickIncome => CalculateClickIncome();

    private double CalculateClickPower()
    {
        return _baseClickPower + GetManualUpgradeEffectSum(UpgradeEffectType.ClickPowerFlat);
    }
    
    private double CalculateClickIncome()
    {
        double clickPower = CalculateClickPower();
        double clickIncome = GetManualUpgradeEffectSum(UpgradeEffectType.ClickIncomeRate);

        return clickPower * (1d + clickIncome) * GetCurrentMarketMultiplier();
    }

    private double GetManualUpgradeEffectSum(UpgradeEffectType effectType)
    {
        if (_upgradeStateController == null) return 0d;

        IReadOnlyList<UpgradeDataSO> definitions = _upgradeStateController.UpgradeDefinitions;
        if (definitions == null) return 0d;
        
        double effectSum = 0d;
        foreach (UpgradeDataSO definition in definitions)
        {
            if (definition == null) continue;
            if (definition.Category != UpgradeCategory.ClickUpgrade) continue;
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
