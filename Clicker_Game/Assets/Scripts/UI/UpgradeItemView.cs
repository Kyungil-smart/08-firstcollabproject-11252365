using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemView : MonoBehaviour
{
    [Header("====런타임 참조")]
    [Tooltip("업그레이드 상태 및 계산값 조회 참조")]
    [SerializeField] private UpgradeStateController _upgradeStateController;
    
    [Header("=====업그레이드 식별====")]
    [Tooltip("표시할 업그레이드의 ID")]
    [SerializeField] private string _upgradeId;
    
    [Header("====UI 참조====")]
    [Tooltip("업그레이드 이름 텍스트")]
    [SerializeField] private TMP_Text _nameText;
    
    [Tooltip("업그레이드 설명 텍스트")]
    [SerializeField] private TMP_Text _descriptionText;

    [Tooltip("현재 구매 수 텍스트")]
    [SerializeField] private TMP_Text _purchaseCountText;

    [Tooltip("다음 구매 비용 텍스트")]
    [SerializeField] private TMP_Text _nextCostText;
    
    [Tooltip("구매 버튼 참조")]
    [SerializeField] private Button _purchaseButton;

    private void Start() => Refresh();

    public void Refresh()
    {
        if (_upgradeStateController == null)
        {
            Debug.LogWarning("[UpgradeItemView] UpgradeStateController가 연결되지 않았습니다.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(_upgradeId))
        {
            Debug.LogWarning("[UpgradeItemView] UpgradeId가 비어 있습니다.", this);
            return;
        }

        if (_upgradeStateController.TryGetDefinition(_upgradeId, out UpgradeDataSO definition))
        {
            if (_nameText != null) _nameText.text = definition.Name;
            if (_descriptionText != null) _descriptionText.text = definition.Description;
        }
        
        int purchaseCount = _upgradeStateController.GetPurchaseCount(_upgradeId);
        if (_purchaseCountText != null) _purchaseCountText.text = $"구매 수 : {purchaseCount}";

        if (_upgradeStateController.TryGetNextCost(_upgradeId, out double nextCost))
        {
            if (_nextCostText != null) _nextCostText.text = $"다음 비용 : ${nextCost:N0}";
        }
        else
        {
            if (_nextCostText != null) _nextCostText.text = "다음 비용 : -";
        }

    }

}
