using System;
using UnityEngine;

public class ClickerRuntimeController : MonoBehaviour
{
    [Header("====초기 시작 값====")]
    [Tooltip("게임 시작 시 보유 금액\n0부터 시작")]
    [SerializeField] private double _startMoney = 0d;

    [Header("====계산 참조====")]
    [Tooltip("ResourceRoot에 함께 배치한 AssetCalculator\n현재 총 자산 계산에 사용")]
    [SerializeField] private AssetCalculator _assetCalculator;
    
    [Tooltip("ResourceRoot에 함께 배치한 ClickIncomeCalculator\nHUD 클릭 파워와 거래 실행 수익 계산에 사용")]
    [SerializeField] private ClickIncomeCalculator _clickIncomeCalculator;
    
    [Tooltip("ResourceRoot에 함께 배치한 AutoIncomeCalculator\nHUD 초당 수익 계산에 사용")]
    [SerializeField] private AutoIncomeCalculator _autoIncomeCalculator;
    
    [Header("====시장 상태 참조====")]
    [Tooltip("MarketStateRoot에 배치한 MarketStateController\n현재 시장 상태 표시와 공통 배율 기준에 사용")]
    [SerializeField] private MarketStateController _marketStateController;
    
    public event Action StateChanged;

    public double Money => _money;
    public double Asset => _asset;
    public double ClickPower => 
        _clickIncomeCalculator != null ? _clickIncomeCalculator.ClickPower : 1d;
    public double AutoIncomePerSecond => 
        _autoIncomeCalculator != null ? _autoIncomeCalculator.AutoIncomePerSecond : 0d;
    public double CurrentClickIncome => 
        _clickIncomeCalculator != null ? _clickIncomeCalculator.ClickIncome : 1d;
    public string CurrentMarketStateDisplayName => 
        _marketStateController != null ? _marketStateController.CurrentStateName : "보합장";
    public double CurrentMarketIncomeMultiplier => 
        _marketStateController != null ? _marketStateController.CurrentIncomeMultiplier : 1d;
    
    private double _money;
    private double _asset;
    
    
    private void Awake()
    {
        ValidateReferences();
        InitializeRuntime();
    }
    private void OnEnable()
    {
        if (_marketStateController != null)
            _marketStateController.StateChanged += HandleMarketStateChanged;
    }

    private void OnDisable()
    {
        if (_marketStateController != null)
            _marketStateController.StateChanged -= HandleMarketStateChanged;
    }
    
    private void InitializeRuntime()
    {
        _money = _startMoney;

        RecalculateAsset();
        NotifyStateChanged();

        // TODO: 저장 / 로드 시스템이 연결되면 초기값 세팅 후 로드 값으로 덮어쓴다.
    }
    
    public void ExecuteTrade()
    {
        _money += CurrentClickIncome;
        RefreshCalculatedState();
    }
    
    public void AddMoneyFromAutoIncome(double amount)
    {
        if (amount <= 0d) return;

        _money += amount;
        RefreshCalculatedState();
    }
    public bool TrySpendMoney(double amount)
    {
        if (amount <= 0d) return false;
        if (_money < amount) return false;

        _money -= amount;
        return true;
    }
    public void RefreshCalculatedState()
    {
        RecalculateAsset();
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
    
    private void HandleMarketStateChanged()
    {
        NotifyStateChanged();
    }
    
    private void RecalculateAsset()
    {
        _asset = _assetCalculator != null
            ? _assetCalculator.CalculateAsset(_money)
            : _money;
    }
    
    private void ValidateReferences()
    {
        if (_clickIncomeCalculator == null)
        {
            Debug.LogWarning(
                "[ClickerRuntimeController] ClickIncomeCalculator가 연결되지 않았습니다. " +
                "현재는 기본 클릭 수익 1 기준으로 동작합니다.",
                this);
        }

        if (_autoIncomeCalculator == null)
        {
            Debug.LogWarning(
                "[ClickerRuntimeController] AutoIncomeCalculator가 연결되지 않았습니다. " +
                "현재는 초당 수익 0 기준으로 동작합니다.",
                this);
        }
        
        if (_marketStateController == null)
        {
            Debug.LogWarning(
                "[ClickerRuntimeController] MarketStateController가 연결되지 않았습니다. " +
                "현재는 HUD 시장 상태 표시가 기본값으로만 동작합니다.",
                this);
        }
    }
}
