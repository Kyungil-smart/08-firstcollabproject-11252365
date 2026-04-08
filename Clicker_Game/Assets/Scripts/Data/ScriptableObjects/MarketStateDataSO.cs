using UnityEngine;

[CreateAssetMenu(fileName = "MarketStateData", menuName = "Stock Clicker/Data/Market State Data")]
public class MarketStateDataSO : ScriptableObject
{
    [Header("====식별 정보====")]
    [Tooltip("저장 호환성과 내부 판정에 사용할 시장 상태 ID")]
    [SerializeField] private string _id;
    
    [Tooltip("HUD에 표시할 시장 상태 이름")]
    [SerializeField] private string _name;
    
    [Header("====수익 배율 설정====")]
    [Tooltip("클릭 수익과 자동 수익에 공통 적용할 시장 상태 배율")]
    [SerializeField] private double _incomeMultiplier = 1d;
    
    public string Id => _id;
    public string Name => _name;
    public double IncomeMultiplier => _incomeMultiplier;
}
