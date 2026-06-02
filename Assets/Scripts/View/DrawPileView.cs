using UnityEngine;
using TMPro;

public class DrawPileView : MonoBehaviour
{
    [SerializeField] private TMP_Text countLabel;

    public void UpdateDisplay(int count)
    {
        if (countLabel != null)
            countLabel.text = "山札\n" + count + "枚";
    }
}