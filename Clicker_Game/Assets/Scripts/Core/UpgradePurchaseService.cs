using System;
using UnityEngine;

public class UpgradePurchaseService : MonoBehaviour
{
    [Header("====구매 처리 참조====")]
    [Tooltip("업그레이드 정의 조회와 구매 수 변경에 사용")]
    [SerializeField] private UpgradeStateController _upgradeStateController;
    
    [Tooltip("현재 Money 차감과 구매 완료 후 상태 갱신에 사용")]
    [SerializeField] private ClickerRuntimeController _runtimeController;

    public event Action<string> PurchaseSucceeded;
    
    public bool TryPurchase(string upgradeId)
    {
        if (_upgradeStateController == null)
        {
            Debug.LogWarning("[UpgradePurchaseService] UpgradeStateController가 연결되지 않았습니다.", this);
            return false;
        }

        if (_runtimeController == null)
        {
            Debug.LogWarning("[UpgradePurchaseService] ClickerRuntimeController가 연결되지 않았습니다.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(upgradeId))
        {
            Debug.LogWarning("[UpgradePurchaseService] upgradeId가 비어 있습니다.", this);
            return false;
        }
        
        if (!_upgradeStateController.TryGetNextCost(upgradeId, out double nextCost)) return false;
        
        bool purchased = _runtimeController.TryApplyPurchaseTransaction(
            nextCost, () => _upgradeStateController.TryIncreasePurchaseCount(upgradeId));
        
        if (!purchased) return false;

        PurchaseSucceeded?.Invoke(upgradeId);
        return true;
    }
}
