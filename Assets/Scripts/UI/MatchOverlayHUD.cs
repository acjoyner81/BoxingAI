using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-space Canvas HUD for arcade-style boxing matches.
/// Displays Player vs Opponent health bars, names, and round/match status.
/// </summary>
public class MatchOverlayHUD : MonoBehaviour
{
    [Header("Fighters")]
    public BoxerAgent redCornerBoxer;
    public BoxerAgent blueCornerBoxer;

    [Header("Red Corner UI (Player / Agent 1)")]
    public Slider redHealthSlider;
    public Image redFillImage;
    public Text redNameText;
    public GameObject redGuardIndicator;

    [Header("Blue Corner UI (Opponent / Agent 2)")]
    public Slider blueHealthSlider;
    public Image blueFillImage;
    public Text blueNameText;
    public GameObject blueGuardIndicator;

    [Header("Round Banner")]
    public Text statusBannerText;

    void Start()
    {
        if (redNameText != null && redCornerBoxer != null)
        {
            redNameText.text = redCornerBoxer.gameObject.name;
        }

        if (blueNameText != null && blueCornerBoxer != null)
        {
            blueNameText.text = blueCornerBoxer.gameObject.name;
        }
    }

    void Update()
    {
        UpdateCorner(redCornerBoxer, redHealthSlider, redFillImage, redGuardIndicator);
        UpdateCorner(blueCornerBoxer, blueHealthSlider, blueFillImage, blueGuardIndicator);

        // Check for Knockout Announcement
        if (statusBannerText != null)
        {
            if (redCornerBoxer != null && redCornerBoxer.currentHealth <= 0f)
            {
                statusBannerText.text = "KNOCKOUT! Blue Corner Wins!";
            }
            else if (blueCornerBoxer != null && blueCornerBoxer.currentHealth <= 0f)
            {
                statusBannerText.text = "KNOCKOUT! Red Corner Wins!";
            }
            else
            {
                statusBannerText.text = "FIGHT!";
            }
        }
    }

    private void UpdateCorner(BoxerAgent agent, Slider slider, Image fill, GameObject guardBadge)
    {
        if (agent == null) return;

        float ratio = Mathf.Clamp01(agent.currentHealth / Mathf.Max(agent.maxHealth, 1f));

        if (slider != null)
        {
            slider.maxValue = agent.maxHealth;
            slider.value = agent.currentHealth;
        }

        if (fill != null)
        {
            fill.color = Color.Lerp(Color.red, Color.green, ratio);
        }

        if (guardBadge != null)
        {
            guardBadge.SetActive(agent.isBlocking);
        }
    }
}
