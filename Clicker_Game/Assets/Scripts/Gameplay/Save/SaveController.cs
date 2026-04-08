using System;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("====세이브 참조====")]
    [Tooltip("현재 Money와 계산값 재갱신 대상 런타임 참조")]
    [SerializeField] private ClickerRuntimeController _runtimeController;
    
    [Tooltip("업그레이드 구매 수 상태 조회/복원 대상 참조")]
    [SerializeField] private UpgradeStateController _upgradeStateController;
    
    [Tooltip("로드 후 시장 상태 초기화 대상 참조")]
    [SerializeField] private MarketStateController _marketStateController;
    
    [Tooltip("업그레이드 구매 성공 이벤트를 구독할 구매 서비스 참조")]
    [SerializeField] private UpgradePurchaseService _upgradePurchaseService;
    
    [Header("====세이브 파일 설정====")]
    [Tooltip("진행 상태를 저장할 JSON 파일 이름")]
    [SerializeField] private string _saveFileName = "clicker_save.json";
    
    [Header("====자동 저장 설정====")]
    [Tooltip("주기 자동 저장 간격(초)\n기본값은 60초")]
    [SerializeField, Min(1f)] private float _autoSaveIntervalSeconds = 60f;

    private float _autoSaveElapsedTime;
    
    private void OnEnable()
    {
        if (_upgradePurchaseService != null)
            _upgradePurchaseService.PurchaseSucceeded += HandlePurchaseSucceeded;
    }
    
    private void OnDisable()
    {
        if (_upgradePurchaseService != null)
            _upgradePurchaseService.PurchaseSucceeded -= HandlePurchaseSucceeded;
    }

    private void Start()
    {
        TryAutoLoadOnStart();
        _autoSaveElapsedTime = 0f;
    }
    
    private void Update() => TryAutoSaveByInterval();
    private void OnApplicationQuit() => Save();

    public bool HasSaveFile() => File.Exists(GetSaveFilePath());

    public void Save()
    {
        if (_runtimeController == null) return;
        if (_upgradeStateController == null) return;

        var saveData = new ClickerSaveData { Money = _runtimeController.Money };

        foreach (UpgradeRuntimeState state in _upgradeStateController.StateById.Values)
        {
            if (state == null) continue;
            
            saveData.UpgradeStates.Add(new UpgradeStateSaveData
            {
                UpgradeId = state.UpgradeId,
                PurchaseCount = state.PurchaseCount
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(GetSaveFilePath(), json);
        
        _autoSaveElapsedTime = 0f;
        
        Debug.Log($"[SaveController] 저장 완료 : {GetSaveFilePath()}", this);
    }

    public bool Load()
    {
        if (_runtimeController == null) return false;
        if (_upgradeStateController == null) return false;
        if (_marketStateController == null) return false;
        if (!HasSaveFile()) return false;
        
        string json = File.ReadAllText(GetSaveFilePath());
        ClickerSaveData saveData = JsonUtility.FromJson<ClickerSaveData>(json);

        if (saveData == null) return false;
        
        _runtimeController.SetMoneyFromSave(saveData.Money);
        _upgradeStateController.ApplyLoadedPurchaseCounts(saveData.UpgradeStates);
        _marketStateController.ResetStateForLoad();
        _runtimeController.RefreshCalculatedState();
        
        _autoSaveElapsedTime = 0f;
        
        Debug.Log($"[SaveController] 로드 완료 : {GetSaveFilePath()}", this);
        return true;
    }

    [ContextMenu("세이브 실행")]
    private void SaveFromContextMenu() => Save();
    
    [ContextMenu("로드 실행")]
    private void LoadFromContextMenu() => Load();

    private void HandlePurchaseSucceeded(string upgradeId) => Save();
    
    private void TryAutoLoadOnStart()
    {
        if (!HasSaveFile())
        {
            Debug.Log("[SaveController] 저장 파일이 없어 기본 시작 상태로 진행합니다.", this);
            return;
        }
        
        bool loaded = Load();
        
        if (!loaded)
        {
            Debug.LogWarning("[SaveController] 자동 로드에 실패해 기본 시작 상태로 진행합니다.", this);
        }
    }

    private void TryAutoSaveByInterval()
    {
        if (_autoSaveIntervalSeconds <= 0f) return;
        if (_runtimeController == null) return;
        if (_upgradeStateController == null) return;

        _autoSaveElapsedTime += Time.unscaledDeltaTime;

        if (_autoSaveElapsedTime < _autoSaveIntervalSeconds) return;

        Save();
    }
    
    private string GetSaveFilePath() => Path.Combine(Application.persistentDataPath, _saveFileName);
}
