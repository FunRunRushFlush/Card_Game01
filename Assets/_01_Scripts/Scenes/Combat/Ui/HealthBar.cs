using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthText;

    [Header("Block")]
    [SerializeField] private Image _blockOverlay;
    [SerializeField] private SpriteRenderer _shieldIcon;
    [SerializeField] private TMP_Text _blockText;

    [Header("Behavior")]
    [SerializeField] private bool _hideBlockWhenZero = true;

    private int _maxHealth;

    public void SetMaxHealth(int maxHealth)
    {
        _maxHealth = Mathf.Max(1, maxHealth);

        _healthSlider.maxValue = _maxHealth;
        _healthSlider.value = _maxHealth;
        UpdateHealthText();
        SetCurrentBlock(0);
    }


    public void SetCurrentHealth(int health)
    {
        int clamped = Mathf.Clamp(health, 0, _maxHealth);
        _healthSlider.value = clamped;
        UpdateHealthText();
    }

    public void SetCurrentBlock(int block)
    {
        int safeBlock = Mathf.Max(0, block);

        // show/hide
        bool show = !_hideBlockWhenZero || safeBlock > 0;

        if (_blockOverlay != null)
            _blockOverlay.gameObject.SetActive(show);
        if (_shieldIcon != null)
            _shieldIcon.gameObject.SetActive(show);
        if (_blockText != null)
            _blockText.gameObject.SetActive(show);

        if (!show)
            return;

        if (_blockText != null)
            _blockText.text = safeBlock.ToString();

    }

    public void SetHealthAndBlock(int health, int block)
    {
        SetCurrentHealth(health);
        SetCurrentBlock(block);
    }

    private void UpdateHealthText()
    {
        if (_healthText == null)
            return;

        _healthText.text = $"{(int)_healthSlider.value}/{(int)_healthSlider.maxValue}";
    }
}
