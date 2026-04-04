using System;
using UnityEngine;
using TMPro;

public class HudTopPanelView : MonoBehaviour
{
    [Header("====런타임 참조====")]
    [Tooltip("현재 자원 상태와 계산값을 제공하는 런타임 컨트롤러")]
    [SerializeField] private ClickerRuntimeController _runtimeController;
    
    [Header("====HUDPanel_Top====\n값 텍스트")]
    [Tooltip("현재 Money 값")]
    [SerializeField] private TMP_Text _moneyValueText;
    
    [Tooltip("현재 Asset 값")]
    [SerializeField] private TMP_Text _assetValueText;
    
    [Tooltip("현재 시장 상태 이름")]
    [SerializeField] private TMP_Text _marketStateValueText;
    
    [Tooltip("현재 클릭 파워 값")]
    [SerializeField] private TMP_Text _clickPowerValueText;
    
    [Tooltip("현재 초당 수익 값")]
    [SerializeField] private TMP_Text _autoIncomeValueText;

    private void OnEnable()
    {
        if (_runtimeController == null) return;

        _runtimeController.StateChanged += RefreshView;
    }

    private void OnDisable()
    {
        if (_runtimeController == null) return;

        _runtimeController.StateChanged -= RefreshView;
    }

    private void Start() => RefreshView();

    private void RefreshView()
    {
        if (_runtimeController == null) return;
        
        _moneyValueText.text = FormatCurrency(_runtimeController.Money);
        _assetValueText.text = FormatCurrency(_runtimeController.Asset);
        _marketStateValueText.text = _runtimeController.CurrentMarketStateDisplayName;
        _clickPowerValueText.text = FormatNumber(_runtimeController.ClickPower);
        _autoIncomeValueText.text = $"{FormatCurrency(_runtimeController.AutoIncomePerSecond)}/s";
        
        // TODO: 큰 수 표기 (K / M / B)
    }

    private string FormatCurrency(double value) => $"${value:N0}";
    private string FormatNumber(double value) => value.ToString("N0");
}
