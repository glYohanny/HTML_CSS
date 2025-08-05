using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI manaText;

    void Update()
    {
        goldText.text = ResourceManager.Instance.GetResource("Oro").ToString();
        woodText.text = ResourceManager.Instance.GetResource("Madera").ToString();
        foodText.text = ResourceManager.Instance.GetResource("Alimentos").ToString();
        stoneText.text = ResourceManager.Instance.GetResource("Piedra").ToString();
        manaText.text = ResourceManager.Instance.GetResource("Mana").ToString();
    }
} 