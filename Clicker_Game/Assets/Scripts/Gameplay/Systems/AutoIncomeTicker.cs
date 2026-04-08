using UnityEngine;

public class AutoIncomeTicker : MonoBehaviour
{
    [Header("====자동 수익 지급 참조====")]
    [Tooltip("ResourceRoot에 배치한 ClickerRuntimeController\n자동 수익을 실제 Money에 반영할 때 사용")]
    [SerializeField] private ClickerRuntimeController _runtimeController;
    
    [Tooltip("ResourceRoot에 배치한 AutoIncomeCalculator\n현재 초당 자동 수익 계산값을 읽을 때 사용")]
    [SerializeField] private AutoIncomeCalculator _autoIncomeCalculator;
    
    [Header("====자동 수익 지급 설정====")]
    [Tooltip("자동 수익을 Money에 반영하는 tick 주기(초)")]
    [SerializeField, Min(0.05f)] private float _tickInterval = 0.1f;
    
    private float _elapsed;

    private void Reset()
    {
        if (_runtimeController == null)
            _runtimeController = GetComponent<ClickerRuntimeController>();
        
        if (_autoIncomeCalculator == null)
            _autoIncomeCalculator = GetComponent<AutoIncomeCalculator>();
    }

    private void Update() => Tick(Time.deltaTime);

    private void Tick(float deltaTime)
    {
        if (_runtimeController == null) return;
        if (_autoIncomeCalculator == null) return;

        double autoIncomePerSecond = _autoIncomeCalculator.AutoIncomePerSecond;
        if (autoIncomePerSecond <= 0)
        {
            _elapsed = 0f;
            return;
        }
        
        _elapsed += deltaTime;

        while (_elapsed >= _tickInterval)
        {
            _elapsed -= _tickInterval;
            
            double tickIncome = autoIncomePerSecond * _tickInterval;
            _runtimeController.AddMoneyFromAutoIncome(tickIncome);
        }
    }
    
}
