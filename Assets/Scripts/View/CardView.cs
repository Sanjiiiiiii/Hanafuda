using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image bg;
    [SerializeField] private TMP_Text charaLabel;
    [SerializeField] private TMP_Text typeLabel;
    [SerializeField] private TMP_Text pointsLabel;

    public CardData Data { get; private set; }
    public bool IsSelectable { get; set; } = false;

    public void Setup(CardData data)
    {
        Data = data;
        bg.color = data.themeColor;
        // ⭐️ 修正後（ここをコピペするか書き換えてください）
        //charaLabel.text = data.charaName;
        typeLabel.text = data.month + "月 (" + data.cardLabel + ")"; // ➔ 例：「1月 (光)」や「2月 (種)」になります
        pointsLabel.text = data.points.ToString() + "pt";
    }

    public void SetHighlight(bool on)
    {
        if (Data == null) return;
        bg.color = on ? Color.Lerp(Data.themeColor, Color.yellow, 0.4f) : Data.themeColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsSelectable)
            GameManager.Instance.OnCardClicked(this);
    }
}