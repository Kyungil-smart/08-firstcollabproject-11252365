using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_runtimeController == null || _popupParent == null ||  _popupTemplate == null)
            return;
        
        CreatePopup(eventData.position, eventData.pressEventCamera, _runtimeController.CurrentClickIncome);
    }

    private void CreatePopup(Vector2 screenPos, Camera eventCamera, double reward)
    {
        TMP_Text popupInstance = Instantiate(_popupTemplate, _popupParent);
        popupInstance.gameObject.SetActive(true);
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
        
        // TODO : 오브젝트 풀 
        Destroy(popupInstance.gameObject);
    }
    
    private string FormatCurrency(double value) => NumberTextFormatter.FormatCurrency(value);
}
