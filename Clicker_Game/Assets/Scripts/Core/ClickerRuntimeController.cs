using System;
using UnityEngine;

public class ClickerRuntimeController : MonoBehaviour
{
    // TODO: 시장 상태 시스템이 생기면 enum 직접 보관 대신
    //  별도 시장 상태 런타임 시스템 참조 구조로 정리
    public enum MarketStateType
    {
        Bearish,
        Neutral,
        Bullish
    }
    
    [Header("====초기 시작 값====")]
    [Tooltip("게임 시작 시 보유 금액\n0부터 시작")]
    [SerializeField] private double _startMoney = 0d;
    
    [Tooltip("게임 시작 시 표시할 시장 상태\n현재는 보합장으로 시작")]
    [SerializeField] private MarketStateType _startMarketState = MarketStateType.Neutral;

    [Tooltip("자동 수익 시스템 연결 전까지 HUD 표시용으로 사용하는 임시 시작값\n현재 단계 기본값은 0")]
    [SerializeField] private double _startAutoIncomePerSecond = 0d;

    [Header("계산 참조")]
    [Tooltip("ResourceRoot에 함께 배치한 ClickIncomeCalculator\nHUD 클릭 파워와 거래 실행 수익 계산에 사용")]
    [SerializeField] private ClickIncomeCalculator _clickIncomeCalculator;
    
    public event Action StateChanged;

    public double Money => _money;
    public double Asset => _asset;
    public double ClickPower => _clickIncomeCalculator != null ? _clickIncomeCalculator.ClickPower : 1d;
    public MarketStateType CurrentMarketState => _currentMarketState;
    
    // TEMP: 자동 수익 계산기는 아직 만들지 않았으므로
    // 지금은 기존 임시 표시값 구조를 유지
    public double AutoIncomePerSecond => _autoIncomePerSecond;
    public double CurrentClickIncome => _clickIncomeCalculator != null ? _clickIncomeCalculator.ClickIncome : 1d;
    
    private double _money;
    private double _asset;
    private MarketStateType _currentMarketState;
    private double _autoIncomePerSecond;
    
    private void Awake()
    {
        ValidateReferences();
        InitializeRuntime();
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
    }
    
    private void InitializeRuntime()
    {
        _money = _startMoney;
        _currentMarketState = _startMarketState;
        _autoIncomePerSecond = _startAutoIncomePerSecond;

        RecalculateAsset();
        NotifyStateChanged();

        // TODO: 저장 / 로드 시스템이 연결되면 초기값 세팅 후 로드 값으로 덮어쓴다.
    }
    
    public string CurrentMarketStateDisplayName =>  _currentMarketState switch
    {
        MarketStateType.Bearish => "약세장",
        MarketStateType.Neutral => "보합장",
        MarketStateType.Bullish => "강세장",
        _ => "보합장"
    };

    public void ExecuteTrade()
    {
        _money += CurrentClickIncome;

        RecalculateAsset();
        NotifyStateChanged();
    }

    public bool TrySpendMoney(double amount)
    {
        if (amount <= 0d) return false;
        if (_money < amount) return false;

        _money -= amount;
        RecalculateAsset();
        return true;
    }
    
    private void RecalculateAsset()
    {
        // TEMP: 자동 투자 업그레이드 누적 비용 계산이 아직 없으므로
        // 지금은 Asset을 Money와 동일하게 표시
        _asset = _money;
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
