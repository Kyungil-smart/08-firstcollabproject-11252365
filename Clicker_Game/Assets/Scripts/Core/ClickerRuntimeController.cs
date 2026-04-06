using System;
using UnityEngine;

public class ClickerRuntimeController : MonoBehaviour
{
    // TODO : 시장 상태 원본 데이터가 생기면 ID / 데이터 참조 구조로 확장
    public enum MarketStateType
    {
        Bearish,
        Neutral,
        Bullish
    }
    
    [Header("====초기 시작 값====")]
    [Tooltip("게임 시작 시 보유 금액\n0부터 시작")]
    [SerializeField] private double _startMoney = 0d;
    
    [Tooltip("게임 시작 시 기본 클릭 파워\n초기값은 1")]
    [SerializeField] private double _startClickPower = 1d;
    
    [Tooltip("게임 시작 시 표시할 시장 상태 이름\n시작 상태는 보합장")]
    [SerializeField] private MarketStateType _startMarketState = MarketStateType.Neutral;
    
    [Tooltip("게임 시작 시 자동 수익 값\n초기값은 0")]
    [SerializeField] private double _startAutoIncomePerSecond = 0d;
    
    public event Action StateChanged;

    public double Money => _money;
    public double Asset => _asset;
    public double ClickPower => _clickPower;
    public MarketStateType CurrentMarketState => _currentMarketState;
    public double AutoIncomePerSecond => _autoIncomePerSecond;
    public double CurrentClickIncome => _clickPower;
    
    private double _money;
    private double _asset;
    private double _clickPower;
    private MarketStateType _currentMarketState;
    private double _autoIncomePerSecond;
    

    private void Awake()
    {
        InitializeRuntime();
    }
    
    private void InitializeRuntime()
    {
        _money = _startMoney;
        _clickPower = _startClickPower;
        _currentMarketState = _startMarketState;
        _autoIncomePerSecond = _startAutoIncomePerSecond;
        
        RecalculateAsset();
        NotifyStateChanged();
        
        // TODO : 저장 / 로드 
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
        // 아직은 업그레이드와 시장 상태 배율 없으므로
        // 현재는 클릭 1회 수익을 ClickPower 그대로 사용한다.
        // TODO: 업그레이드 / 시장 상태가 붙으면 최종 클릭 수익 계산 함수로 분리
        _money += _clickPower;
        
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
        // 현재는 자동 투자 업그레이드가 없으므로 Asset을 Money와 동일하게 처리
        // TODO: 자동 투자 업그레이드 구매 상태가 붙으면 누적 구매 비용 합을 반영한다.
        _asset = _money;
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
