using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CpuPlayer
{
    // ─────────────────────────────────────────────
    //  手順1：手札から出す1枚を選ぶ
    //
    //  戦略：
    //  ① 合わせられる手札がある → 獲得点数が最大になる組み合わせを選ぶ
    //  ② 合わせられる手札がない → 最も点数の低いカードを捨てる（高得点は温存）
    // ─────────────────────────────────────────────
    public static CardView PickHandCard(IReadOnlyList<CardView> hand,
                                        IReadOnlyList<CardView> field)
    {
        var playable = MatchingSystem.GetPlayableCards(hand, field);

        if (playable.Count > 0)
        {
            // 獲得点数が最大のカードを選択
            return playable
                .OrderByDescending(h =>
                {
                    var matches = MatchingSystem.GetMatches(h.Data, field);
                    return MatchingSystem.EvalGain(h.Data, matches);
                })
                .First();
        }
        else
        {
            // 捨て牌：最低点のカード（高得点カードは温存）
            return hand.OrderBy(h => h.Data.points).First();
        }
    }

    // ─────────────────────────────────────────────
    //  場に合わせ候補が複数ある場合、1枚を選ぶ
    //  戦略：最高点の場札を取る
    // ─────────────────────────────────────────────
    public static CardView PickFieldCard(List<CardView> matches)
    {
        return matches.OrderByDescending(c => c.Data.points).First();
    }
}