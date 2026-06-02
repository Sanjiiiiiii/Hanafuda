using UnityEngine;

[CreateAssetMenu(menuName = "HanaFuda/Card")]
public class CardData : ScriptableObject
{
    public int month;
    public CardType type;
    public int points;
    public string charaName;
    public Color themeColor;
    public string cardLabel;
}

public enum CardType
{
    Hikari = 20,
    Tane = 10,
    Tanzaku = 5,
    Kasu = 1
}