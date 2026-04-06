using System;
using UnityEngine;

[Serializable]
public class UpgradeRuntimeState
{
    [Header("====업그레이드 런타임 상태====")]
    [Tooltip("원본 업그레이드 데이터와 연결하는 ID")]
    [SerializeField] private string _upgradeId;
    
    [Tooltip("현재 구매 횟수")]
    [SerializeField, Min(0)] private int _purchaseCount;
    
    public string UpgradeId => _upgradeId;
    public int PurchaseCount => _purchaseCount;

    public UpgradeRuntimeState(string upgradeId, int purchaseCount = 0)
    {
        _upgradeId = upgradeId;
        _purchaseCount = Math.Max(0, purchaseCount);
    }

    public void SetPurchaseCount(int purchaseCount)
    {
        _purchaseCount = Math.Max(0, purchaseCount);
    }

    public void IncreasePurchaseCount() => _purchaseCount++;
}
