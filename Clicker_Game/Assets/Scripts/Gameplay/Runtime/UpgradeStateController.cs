using System.Collections.Generic;
using UnityEngine;

public class UpgradeStateController : MonoBehaviour
{
    [Header("====업그레이드 원본 데이터====")]
    [Tooltip("업그레이드 원본 데이터 목록")]
    [SerializeField] private UpgradeDataSO[] _upgradeDefinitions;

    private readonly Dictionary<string, UpgradeDataSO>  _definitionById = new();
    private readonly Dictionary<string, UpgradeRuntimeState> _stateById = new();
    
    public IReadOnlyList<UpgradeDataSO> UpgradeDefinitions => _upgradeDefinitions;
    public IReadOnlyDictionary<string, UpgradeRuntimeState> StateById => _stateById;

    private void Awake() => InitializeStates();

    private void InitializeStates()
    {
        _definitionById.Clear();
        _stateById.Clear();

        if (_upgradeDefinitions == null || _upgradeDefinitions.Length == 0)
        {
            Debug.LogWarning("[UpgradeStateController] 업그레이드 원본 데이터가 비어 있습니다.", this);
            return;
        }

        for (int i = 0; i < _upgradeDefinitions.Length; i++)
        {
            UpgradeDataSO upgradeDefinition = _upgradeDefinitions[i];

            if (upgradeDefinition == null)
            {
                Debug.LogWarning($"[UpgradeStateController] UpgradeDefinition is null. index : " +
                                 $"{i}", this);
                continue;
            }

            if (string.IsNullOrWhiteSpace(upgradeDefinition.Id))
            {
                Debug.LogWarning($"[UpgradeStateController] Id is null or invalid. name : " +
                                 $"{upgradeDefinition.Name}", this);
                continue;
            }

            if (_stateById.ContainsKey(upgradeDefinition.Id))
            {
                Debug.LogWarning($"[UpgradeStateController] Id already exists. id : " +
                                 $"{upgradeDefinition.Id}", this);
                continue;
            }

            _definitionById.Add(upgradeDefinition.Id, upgradeDefinition);
            _stateById.Add(upgradeDefinition.Id, new UpgradeRuntimeState(upgradeDefinition.Id));
        }
    }

    public bool TryGetDefinition(string upgradeId, out UpgradeDataSO definition)
    {
        return _definitionById.TryGetValue(upgradeId, out definition);
    }
    
    public bool TryGetState(string upgradeId, out UpgradeRuntimeState state)
    {
        return _stateById.TryGetValue(upgradeId, out state);
    }

    public int GetPurchaseCount(string upgradeId)
    {
        return _stateById.TryGetValue(upgradeId, out UpgradeRuntimeState state) ? state.PurchaseCount : 0;
    }

    public bool TryGetNextCost(string upgradeId, out double nextCost)
    {
        nextCost = 0;
        
        if (!_definitionById.TryGetValue(upgradeId, out UpgradeDataSO definition)) return false;
        
        int purchaseCount = GetPurchaseCount(upgradeId);
        nextCost = definition.GetNextCost(purchaseCount);
        return true;
    }
    
    public bool TryIncreasePurchaseCount(string upgradeId)
    {
        if (!_stateById.TryGetValue(upgradeId, out UpgradeRuntimeState state)) return false;

        state.IncreasePurchaseCount();
        return true;
    }

    public void ResetAllPurchaseCounts()
    {
        foreach (UpgradeRuntimeState state in _stateById.Values)
        {
            state.SetPurchaseCount(0);
        }
    }

    public void SetPurchaseCounts(IReadOnlyDictionary<string, int> purchaseCountsById)
    {
        if (purchaseCountsById == null)
        {
            Debug.LogWarning("[UpgradeStateController] 적용할 구매 수 데이터가 비어 있습니다.", this);
            return;
        }

        foreach (KeyValuePair<string, int> pair in purchaseCountsById)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                Debug.LogWarning("[UpgradeStateController] 업그레이드 ID가 비어 있습니다.", this);
                return;
            }

            if (pair.Value < 0)
            {
                Debug.LogWarning(
                    $"[UpgradeStateController] 업그레이드 구매 수는 음수가 될 수 없습니다. id : {pair.Key}",
                    this);
                return;
            }

            if (!_stateById.ContainsKey(pair.Key))
            {
                Debug.LogWarning(
                    $"[UpgradeStateController] 등록되지 않은 업그레이드 ID입니다. id : {pair.Key}",
                    this);
                return;
            }
        }

        ResetAllPurchaseCounts();
        foreach (KeyValuePair<string, int> pair in purchaseCountsById)
        {
            _stateById[pair.Key].SetPurchaseCount(pair.Value);
        }
    }
    
}
