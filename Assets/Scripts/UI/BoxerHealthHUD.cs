using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-Space or Screen-Space Combat HUD tracking Boxer health and guard status.
/// Works automatically whether attached directly above the fighter or as an overlay.
/// </summary>
public class BoxerHealthHUD : MonoBehaviour
{
    [Header("Boxer Reference")]
    public BoxerAgent boxer;
    public string fighterName = "Boxer";

    [Header("UI Elements (Optional Drag & Drop)")]
    public Slider healthSlider;
    public Image healthFillImage;
    public Text nameText;
    public GameObject guardBadge;

    [Header("Visual Colors")]
    public Color normalHealthColor = new Color(0.2f, 0.85f, 0.3f);
    public Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
    public Color guardActiveColor = new Color(0.2f, 0.6f, 1.0f);

    [Header("World Space Billboard")]
    public bool billboardToCamera = true;
    public Vector3 worldOffset = new Vector3(0, 2.2f, 0);

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        if (boxer == null)
        {
            boxer = GetComponentInParent<BoxerAgent>();
        }

        if (healthSlider != null && boxer != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = boxer.maxHealth;
            healthSlider.value = boxer.currentHealth;
        }

        if (nameText != null)
        {
            nameText.text = fighterName;
        }
    }

    void LateUpdate()
    {
        if (boxer == null) return;

        // Position HUD above the fighter if in world space
        if (billboardToCamera && boxer != null)
        {
            transform.position = boxer.transform.position + worldOffset;
            if (mainCam != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
            }
        }

        // Update Health Slider
        float currentHP = boxer.currentHealth;
        float maxHP = boxer.maxHealth;
        float ratio = Mathf.Clamp01(currentHP / Mathf.Max(maxHP, 1f));

        if (healthSlider != null)
        {
            healthSlider.value = currentHP;
        }

        // Color interpolation green -> red
        if (healthFillImage != null)
        {
            healthFillImage.color = Color.Lerp(lowHealthColor, normalHealthColor, ratio);
        }

        // Guard / Block indicator
        if (guardBadge != null)
        {
            guardBadge.SetActive(boxer.isBlocking);
        }
    }
}
