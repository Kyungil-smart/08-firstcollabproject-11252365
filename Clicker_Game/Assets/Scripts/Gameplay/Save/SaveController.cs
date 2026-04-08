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

    private readonly SaveFileCodec _saveFileCodec = new();
    private readonly SaveDataValidator _saveDataValidator = new();

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

    public bool Save()
    {
        if (_runtimeController == null) return false;
        if (_upgradeStateController == null) return false;

        try
        {
            ClickerSaveData saveData = CreateSaveData();
            SaveEnvelope envelope = CreateEnvelope(saveData);
            
            string envelopeJson = JsonUtility.ToJson(envelope, true);
            File.WriteAllText(GetSaveFilePath(), envelopeJson);
            
            _autoSaveElapsedTime = 0f;
            
            Debug.Log($"[SaveController] 저장 완료 : {GetSaveFilePath()}", this);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                $"[SaveController] 저장에 실패했습니다. reason : {e.Message}",
                this);
            return false;
        }
    }

    public bool Load()
    {
        if (_runtimeController == null) return false;
        if (_upgradeStateController == null) return false;
        if (_marketStateController == null) return false;
        if (!HasSaveFile()) return false;

        bool loadedSaveData = TryReadSaveData(out ClickerSaveData saveData);
        if (!loadedSaveData) return false;

        bool validated = _saveDataValidator.TryValidate(
            saveData,
            _upgradeStateController.StateById,
            out ValidatedSaveState validatedSaveState);

        if (!validated || validatedSaveState == null)
        {
            Debug.LogWarning("[SaveController] 저장 데이터 검증에 실패했습니다.", this);
            return false;
        }

        ApplyValidatedSaveState(validatedSaveState);

        _marketStateController.ResetStateForLoad();
        _runtimeController.RefreshCalculatedState();

        _autoSaveElapsedTime = 0f;

        Debug.Log($"[SaveController] 로드 완료 : {GetSaveFilePath()}", this);
        return true;
    }
    
    public void ResetProgressFromButton()
    {
        bool resetSucceeded = TryResetProgress();

        if (!resetSucceeded)
        {
            Debug.LogWarning("[SaveController] 버튼을 통한 세이브 초기화에 실패했습니다.", this);
        }
    }

    [ContextMenu("세이브 실행")]
    private void SaveFromContextMenu() => Save();
    
    [ContextMenu("로드 실행")]
    private void LoadFromContextMenu() => Load();

    [ContextMenu("초기화 실행")]
    private void ResetFromContextMenu() => TryResetProgress();

    private void HandlePurchaseSucceeded(string upgradeId) => Save();
    
    private bool TryResetProgress()
    {
        if (_runtimeController == null) return false;
        if (_upgradeStateController == null) return false;
        if (_marketStateController == null) return false;

        _runtimeController.ResetMoneyToStartValue();
        _upgradeStateController.ResetAllPurchaseCounts();
        _marketStateController.ResetStateForLoad();
        _runtimeController.RefreshCalculatedState();

        _autoSaveElapsedTime = 0f;

        bool saved = Save();
        if (!saved)
        {
            Debug.LogWarning("[SaveController] 초기화 후 기본 상태 저장에 실패했습니다.", this);
            return false;
        }

        Debug.Log("[SaveController] 세이브 데이터를 초기화했습니다.", this);
        return true;
    }
    
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

    private ClickerSaveData CreateSaveData()
    {
        ClickerSaveData saveData = new()
        {
            Money = _runtimeController.Money
        };

        foreach (UpgradeRuntimeState state in _upgradeStateController.StateById.Values)
        {
            if (state == null) continue;

            saveData.UpgradeStates.Add(new UpgradeStateSaveData
            {
                UpgradeId = state.UpgradeId,
                PurchaseCount =  state.PurchaseCount
            });
        }
        
        return saveData;
    }

    private SaveEnvelope CreateEnvelope(ClickerSaveData saveData)
    {
        string payloadJson = JsonUtility.ToJson(saveData);
        string encodedPayload = _saveFileCodec.Encode(payloadJson);

        return new SaveEnvelope
        {
            Version = 1,
            EncodedPayload = encodedPayload,
            Checksum = _saveFileCodec.ComputeChecksum(encodedPayload)
        };
    }
    
    private bool TryReadSaveData(out ClickerSaveData saveData)
    {
        saveData = null;

        string envelopeJson;
        try
        {
            envelopeJson = File.ReadAllText(GetSaveFilePath());
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                $"[SaveController] 저장 파일 읽기에 실패했습니다. reason : {e.Message}",
                this);
            return false;
        }

        SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(envelopeJson);
        if (envelope == null)
        {
            Debug.LogWarning("[SaveController] 저장 파일 envelope 역직렬화에 실패했습니다.", this);
            return false;
        }

        if (envelope.Version != 1)
        {
            Debug.LogWarning(
                $"[SaveController] 지원하지 않는 저장 파일 버전입니다. version : {envelope.Version}",
                this);
            return false;
        }

        bool checksumValid = _saveFileCodec.ValidateChecksum(
            envelope.EncodedPayload,
            envelope.Checksum);

        if (!checksumValid)
        {
            Debug.LogWarning("[SaveController] 저장 파일 checksum 검증에 실패했습니다.", this);
            return false;
        }

        bool decoded = _saveFileCodec.TryDecode(
            envelope.EncodedPayload,
            out string payloadJson);

        if (!decoded)
        {
            Debug.LogWarning("[SaveController] 저장 payload decode에 실패했습니다.", this);
            return false;
        }

        saveData = JsonUtility.FromJson<ClickerSaveData>(payloadJson);
        if (saveData == null)
        {
            Debug.LogWarning("[SaveController] 저장 payload 역직렬화에 실패했습니다.", this);
            return false;
        }

        return true;
    }
    
    private void ApplyValidatedSaveState(ValidatedSaveState validatedSaveState)
    {
        if (validatedSaveState == null)
        {
            Debug.LogWarning("[SaveController] 적용할 검증 완료 저장 상태가 없습니다.", this);
            return;
        }

        _runtimeController.SetMoneyFromSave(validatedSaveState.Money);
        _upgradeStateController.SetPurchaseCounts(validatedSaveState.PurchaseCountsById);
    }
    
    private string GetSaveFilePath() => Path.Combine(Application.persistentDataPath, _saveFileName);
}
