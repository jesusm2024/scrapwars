using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DisplayPopUp : MonoBehaviour
{
    private TMP_Text Text;

    // Start is called before the first frame update
    void Start()
    {
        Text = transform.GetChild(0).GetComponent<TMP_Text>();

        HideDisplay();
    }

    public void HideDisplay()
    {
        GetComponent<Image>().enabled = false;
        Text.gameObject.SetActive(false);
    }

    public void DisplayAttackInfo(Node node)
    {
        GetComponent<Image>().enabled = true;
        Text.gameObject.SetActive(true);
        Text.text = $"<size=40><b>Attack Level {node.nodeLevel + 1}</b></size>\nTakes {node.attackUpgrades[node.nodeLevel].cost} seconds to increase current unit speed " +
                    $"by {node.attackUpgrades[node.nodeLevel].speed} and current unit damage by {node.attackUpgrades[node.nodeLevel].damage} with 100 units (more units faster upgrade).";
    }

    public void DisplayGrowthInfo(Node node)
    {
        GetComponent<Image>().enabled = true;
        Text.gameObject.SetActive(true);
        Text.text = $"<size=40><b>Growth Level {node.nodeLevel + 1}</b></size>\nTakes {node.growthUpgrades[node.nodeLevel].cost} seconds " +
                    $"to increase current growth value by {node.growthUpgrades[node.nodeLevel].value} with 100 units (more units faster upgrade).";
    }
}
