using UnityEngine;
using UnityEngine.UI;

public class GameAudioController : MonoBehaviour
{
    [Header("====오디오 소스 참조====")]
    [Tooltip("배경음 BgmSource")]
    [SerializeField] private AudioSource _bgmSource;

    [Tooltip("버튼 UI 효과음을 재생 SfxSource")]
    [SerializeField] private AudioSource _sfxSource;
    
    [Header("====옵션 슬라이더 참조====")]
    [Tooltip("옵션 팝업의 BGM 볼륨 슬라이더")]
    [SerializeField] private Slider _bgmSlider;

    [Tooltip("옵션 팝업의 SFX 볼륨 슬라이더")]
    [SerializeField] private Slider _sfxSlider;
    
    [Header("====볼륨 설정====")]
    [Tooltip("저장값 없을 때 사용할 BGM 볼륨")]
    [SerializeField, Range(0f, 1f)] private float _defaultBgmVolume = 1f;

    [Tooltip("저장값 없을 때 사용할 SFX 볼륨")]
    [SerializeField, Range(0f, 1f)] private float _defaultSfxVolume = 1f;

    private const string BGM_VOLUME_KEY = "option.bgm_volume";
    private const string SFX_VOLUME_KEY = "option.sfx_volume";

    private void Start()
    {
        LoadSavedVolumes();
        SyncSlidersWithoutNotify();
    }
    
    private void LoadSavedVolumes()
    {
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, _defaultBgmVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, _defaultSfxVolume);

        if (_bgmSource != null) _bgmSource.volume = Mathf.Clamp01(bgmVolume);
        if (_sfxSource != null) _sfxSource.volume = Mathf.Clamp01(sfxVolume);
    }
    
    private void SyncSlidersWithoutNotify()
    {
        if (_bgmSlider != null)
            _bgmSlider.SetValueWithoutNotify(_bgmSource != null ? _bgmSource.volume : _defaultBgmVolume);

        if (_sfxSlider != null)
            _sfxSlider.SetValueWithoutNotify(_sfxSource != null ? _sfxSource.volume : _defaultSfxVolume);
    }

    public void SetBgmVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);

        if (_bgmSource != null) _bgmSource.volume = clampedVolume;
        
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();
    }
    
    public void SetSfxVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);

        if (_sfxSource != null) _sfxSource.volume = clampedVolume;

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();
    }
}
