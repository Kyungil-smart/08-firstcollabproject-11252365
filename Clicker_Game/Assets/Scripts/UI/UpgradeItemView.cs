using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemView : MonoBehaviour
{
    [Header("====런타임 참조====")]
    [Tooltip("업그레이드 상태 및 계산값 조회 참조")]
    [SerializeField] private UpgradeStateController _upgradeStateController;
    
    [Tooltip("현재 보유 Money 변화 런타임 참조")]
    [SerializeField] private ClickerRuntimeController _runtimeController;
    
    [Tooltip("구매 트랜잭션 처리 참조")]
    [SerializeField] private UpgradePurchaseService _upgradePurchaseService;
    
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
    
    [Tooltip("구매 버튼")]
    [SerializeField] private Button _purchaseButton;

    private void OnEnable()
    {
        if (_runtimeController != null)
        {
            _runtimeController.StateChanged += Refresh;
        }

        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.AddListener(HandlePurchaseButtonClicked);
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (_runtimeController != null)
        {
            _runtimeController.StateChanged -= Refresh;
        }

        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.RemoveListener(HandlePurchaseButtonClicked);
        }
    }

    public void Refresh()
    {
        if (_upgradeStateController == null)
        {
            Debug.LogWarning("[UpgradeItemView] UpgradeStateController가 연결되지 않았습니다.", this);
            SetButtonInteractable(false);
            return;
        }
        
        if (_runtimeController == null)
        {
            Debug.LogWarning("[UpgradeItemView] RuntimeController가 연결되지 않았습니다.", this);
            SetButtonInteractable(false);
            return;
        }

        if (string.IsNullOrWhiteSpace(_upgradeId))
        {
            Debug.LogWarning("[UpgradeItemView] UpgradeId가 비어 있습니다.", this);
            SetButtonInteractable(false);
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
            if (_nextCostText != null) 
                _nextCostText.text = $"다음 비용 : {NumberTextFormatter.FormatCurrency(nextCost)}";

            bool canPurchase = _runtimeController.Money >= nextCost && _upgradePurchaseService != null;
            SetButtonInteractable(canPurchase);
        }
        else
        {
            if (_nextCostText != null) _nextCostText.text = "다음 비용 : -";
            SetButtonInteractable(false);
        }
    }

    private void HandlePurchaseButtonClicked()
    {
        if (_upgradePurchaseService == null) return;
        _upgradePurchaseService.TryPurchase(_upgradeId);
    }

    private void SetButtonInteractable(bool canPurchase)
    {
        if (_purchaseButton == null) return;
        _purchaseButton.interactable = canPurchase;
    }
}
