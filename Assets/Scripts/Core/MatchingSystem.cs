using System.Collections.Generic;
using System.Linq;

public static class MatchingSystem
{
    // ─────────────────────────────────────────────
    //  場札から同じ月のカードを返す
    // ─────────────────────────────────────────────
    public static List<CardView> GetMatches(CardData played, IReadOnlyList<CardView> field)
    {
        return field.Where(c => c.Data.month == played.month).ToList();
    }

    // ─────────────────────────────────────────────
    //  手札の中で場に合わせられるカードを返す
    // ─────────────────────────────────────────────
    public static List<CardView> GetPlayableCards(IReadOnlyList<CardView> hand,
                                                   IReadOnlyList<CardView> field)
    {
        return hand.Where(h => field.Any(f => f.Data.month == h.Data.month)).ToList();
    }

    // ─────────────────────────────────────────────
    //  あるカードを出したとき獲得できる点数を試算
    //  （手札カード点 + 最高値の場札カード点）
    // ─────────────────────────────────────────────
    public static int EvalGain(CardData handCard, List<CardView> matches)
    {
        if (matches.Count == 0) return 0;
        return handCard.points + matches.Max(c => c.Data.points);
    }
}