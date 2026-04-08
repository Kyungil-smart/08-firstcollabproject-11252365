using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Pool;

public class ClickRewardPopupSpawner : MonoBehaviour, IPointerClickHandler
{
    [FormerlySerializedAs("_RuntimeController")]
    [Header("====런타임 참조====")]
    [Tooltip("클릭 수익 값을 읽어오는 런타임 컨트롤러")]
    [SerializeField] private ClickerRuntimeController _runtimeController;
    
    [Header("====팝업 생성 참조====")]
    [Tooltip("클릭 팝업 배치할 부모 RectTransform\nFeedbackLayer연결")]
    [SerializeField] private RectTransform _popupParent;
    
    [Tooltip("재사용할 클릭 팝업 탬플릿 텍스트")]
    [SerializeField] private TMP_Text _popupTemplate;
    
    [Header("====팝업 설정====")]
    [Tooltip("위로 위동할 거리")]
    [SerializeField] private float _riseDistance = 60f;
    
    [Tooltip("유지 되는 시간")]
    [SerializeField] private float _duration = 0.6f;
    
    [Header("====오브젝트 풀 설정====")]
    [Tooltip("시작 시 미리 생성해 둘 클릭 팝업 개수")]
    [SerializeField, Min(0)] private int _prewarmCount = 10;

    [Tooltip("풀 내부 스택의 기본 용량")]
    [SerializeField, Min(1)] private int _defaultPoolCapacity = 10;

    [Tooltip("동시에 유지할 최대 팝업 개수")]
    [SerializeField, Min(1)] private int _maxPoolSize = 25;
    
    private ObjectPool<TMP_Text> _popupPool;
    private Color _defaultPopupColor;

    private void Awake()
    {
        if (_popupTemplate == null || _popupParent == null) return;
        
        _defaultPopupColor = _popupTemplate.color;
        _popupTemplate.gameObject.SetActive(false);

        _popupPool = new ObjectPool<TMP_Text>(
            createFunc: CreatePopupInstance,
            actionOnGet: OnGetPopup,
            actionOnRelease: OnReleasePopup,
            actionOnDestroy: OnDestroyPopup,
            collectionCheck: true,
            defaultCapacity: _defaultPoolCapacity,
            maxSize: _maxPoolSize
        );

        PrewarmPool();
    }
    
    private void OnDestroy() => _popupPool?.Clear();
    
    private TMP_Text CreatePopupInstance()
    {
        TMP_Text popupInstance = Instantiate(_popupTemplate, _popupParent);
        popupInstance.gameObject.SetActive(false);
        popupInstance.color = _defaultPopupColor;
        return popupInstance;
    }

    private void OnGetPopup(TMP_Text popupInstance)
    {
        popupInstance.gameObject.SetActive(true);
        popupInstance.color = _defaultPopupColor;
    }

    private void OnReleasePopup(TMP_Text popupInstance)
    {
        popupInstance.text = string.Empty;
        popupInstance.color = _defaultPopupColor;
        popupInstance.rectTransform.anchoredPosition = Vector2.zero;
        popupInstance.gameObject.SetActive(false);
    }

    private void OnDestroyPopup(TMP_Text popupInstance)
    {
        if (popupInstance == null) return;
        Destroy(popupInstance.gameObject);
    }

    private void PrewarmPool()
    {
        if (_popupPool == null) return;
        if (_prewarmCount <= 0) return;
        
        List<TMP_Text> prewarmedItems = new(_prewarmCount);

        for (int i = 0; i < _prewarmCount; i++)
        {
            prewarmedItems.Add(_popupPool.Get());
        }

        for (int i = 0; i < prewarmedItems.Count; i++)
        {
            _popupPool.Release(prewarmedItems[i]);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_runtimeController == null || _popupParent == null || _popupTemplate == null || _popupPool == null)
            return;
        
        CreatePopup(eventData.position, eventData.pressEventCamera, _runtimeController.CurrentClickIncome);
    }

    private void CreatePopup(Vector2 screenPos, Camera eventCamera, double reward)
    {
        TMP_Text popupInstance = _popupPool.Get();
        popupInstance.text = $"+{FormatCurrency(reward)}";
        
        RectTransform popupRect = popupInstance.rectTransform;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _popupParent,
            screenPos,
            eventCamera,
            out Vector2 localPoint);
        
        popupRect.anchoredPosition = localPoint;

        StartCoroutine(PlayPopup(popupInstance));
    }

    // 위로 이동하면서 사라짐
    private IEnumerator PlayPopup(TMP_Text popupInstance)
    {
        RectTransform popupRect = popupInstance.rectTransform;
        Vector2 startPos = popupRect.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * _riseDistance;
        
        Color startColor = popupInstance.color;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            
            popupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, t);
            popupInstance.color = currentColor;
            
            yield return null;
        }
        
        _popupPool.Release(popupInstance);
    }
    
    private string FormatCurrency(double value) => NumberTextFormatter.FormatCurrency(value);
}
