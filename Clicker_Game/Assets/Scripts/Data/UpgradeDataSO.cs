using System;
using UnityEngine;

public enum UpgradeCategory
{
    ClickUpgrade,
    AutoUpgrade
}

public enum UpgradeEffectType
{
    ClickPowerFlat,
    ClickIncomeRate,
    AutoIncomeFlat,
    AutoIncomeEfficiencyRate,
    AutoIncomeBonusRate
}

[CreateAssetMenu(fileName = "UpgradeData_", menuName = "Stock Clicker/Data/Upgrade Data SO")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("====식별 정보====")]
    [Tooltip("업그레이드 식별용 고유 ID")]
    [SerializeField] private string _id;
    
    [Tooltip("업그레이드 이름")]
    [SerializeField] private string _name;
    
    [Tooltip("수동 거래 강화(ClickUpgrade) / 자동 투자 시스템(AutoUpgrade) 구분")]
    [SerializeField] private UpgradeCategory _category;
    
    [Tooltip("UI 표시용 설명")]
    [SerializeField, TextArea(2, 3)] private string _description;
    
    [Header("====비용 설정====")]
    [Tooltip("구매 0회 기준 기본 비용")]
    [SerializeField, Min(1f)] private double _baseCost = 10d;
    
    [Tooltip("비용 증가 배율\n구매할수록 얼마나 증가하는지 결정하는 배율")]
    [SerializeField, Min(1f)] private double _costGrowthMultiplier = 1.15d;
    
    [Header("====효과 설정====")]
    [Tooltip("구매 1회당 증가하는 효과량")]
    [SerializeField, Min(0f)] private double _effectValue = 1d;
    
    [Tooltip("효과 적용 방식 구분\n이 업그레이드가 어떤 계산에 반영되는지 구분하는 타입")]
    [SerializeField] private UpgradeEffectType _effectType;
    
    [Header("====정렬 설정====")]
    [Tooltip("UI 정렬 순서\n값이 작을수록 먼저 표시")]
    [SerializeField, Min(0)] private int _sortOrder;
    
    public string Id => _id;
    public string Name => _name;
    public UpgradeCategory Category => _category;
    public string Description => _description;
    public double BaseCost => _baseCost;
    public double CostGrowthMultiplier => _costGrowthMultiplier;
    public double EffectValue => _effectValue;
    public UpgradeEffectType EffectType => _effectType;
    public int SortOrder => _sortOrder;
    
    private void OnValidate()
    {
        if (_baseCost < 1d) _baseCost = 1d;
        if (_costGrowthMultiplier < 1d) _costGrowthMultiplier = 1d;
        if (_effectValue < 0d) _effectValue = 0d;
        if (_sortOrder < 0) _sortOrder = 0;
    }

    // 현재 구매 횟수 기준 다음 1회 구매 비용
    public double GetNextCost(int purchaseCount)
    {
        purchaseCount = Mathf.Max(0, purchaseCount);
        return Math.Ceiling(_baseCost * Math.Pow(_costGrowthMultiplier, purchaseCount));
    }

    // 현재 구매 횟수 기준 누적 효과량
    public double GetCurrentEffectValue(int purchaseCount)
    {
        purchaseCount = Mathf.Max(0, purchaseCount);
        return _effectValue * purchaseCount;
    }
}
