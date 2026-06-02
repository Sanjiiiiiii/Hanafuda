using System.Collections.Generic;
using System.Linq;

public class YakuResult
{
    public string Name;
    public int Points;
    public YakuResult(string name, int pts) { Name = name; Points = pts; }
}

public static class ScoreCalculator
{
    // ─────────────────────────────────────────────
    //  カード点数合計（役なし）
    // ─────────────────────────────────────────────
    public static int CalcCardPoints(List<CardData> taken)
    {
        return taken.Sum(c => c.points);
    }

    // ─────────────────────────────────────────────
    //  役判定（ちょっと花合わせ 8か月版）
    //  使用月：1月松・2月梅・3月桜・6月牡丹・7月萩・8月芒・9月菊・10月紅葉
    // ─────────────────────────────────────────────
    public static List<YakuResult> CalcYaku(List<CardData> taken)
    {
        var results = new List<YakuResult>();

        // ─── 光系 ───────────────────────────────
        // 三光：光3枚（1月鶴 / 3月幕 / 8月月）すべて
        int hikariCount = taken.Count(c => c.type == CardType.Hikari);
        if (hikariCount >= 3)
            results.Add(new YakuResult("三光", 30));

        bool hasCurtain = taken.Any(c => c.month == 3 && c.type == CardType.Hikari);
        bool hasMoon = taken.Any(c => c.month == 8 && c.type == CardType.Hikari);
        bool hasSakeCup = taken.Any(c => c.month == 9 && c.type == CardType.Tane);

        // 花見酒：3月光（幕）＋ 9月種（盃）
        if (hasCurtain && hasSakeCup)
            results.Add(new YakuResult("花見酒", 20));

        // 月見酒：8月光（月）＋ 9月種（盃）
        if (hasMoon && hasSakeCup)
            results.Add(new YakuResult("月見酒", 20));

        // ─── 短冊系 ──────────────────────────────
        // 赤短：1月・2月・3月の短冊3枚すべて（松・梅・桜の赤短冊）
        bool hasAkaTan =
            taken.Any(c => c.month == 1 && c.type == CardType.Tanzaku) &&
            taken.Any(c => c.month == 2 && c.type == CardType.Tanzaku) &&
            taken.Any(c => c.month == 3 && c.type == CardType.Tanzaku);
        if (hasAkaTan)
            results.Add(new YakuResult("赤短", 20));

        // 青短：6月・9月・10月の短冊3枚すべて（牡丹・菊・紅葉の青短冊）
        bool hasAoTan =
            taken.Any(c => c.month == 6 && c.type == CardType.Tanzaku) &&
            taken.Any(c => c.month == 9 && c.type == CardType.Tanzaku) &&
            taken.Any(c => c.month == 10 && c.type == CardType.Tanzaku);
        if (hasAoTan)
            results.Add(new YakuResult("青短", 20));

        // 五タン：短冊5枚以上（基本10pt、超過1枚ごと+10pt）
        int tanCount = taken.Count(c => c.type == CardType.Tanzaku);
        if (tanCount >= 5)
            results.Add(new YakuResult($"五タン（{tanCount}枚）", 10 + (tanCount - 5) * 10));

        // ─── 種系 ────────────────────────────────
        // 五タネ：種5枚以上（基本10pt、超過1枚ごと+10pt）
        int taneCount = taken.Count(c => c.type == CardType.Tane);
        if (taneCount >= 5)
            results.Add(new YakuResult($"五タネ（{taneCount}枚）", 10 + (taneCount - 5) * 10));

        return results;
    }

    // ─────────────────────────────────────────────
    //  合計点計算（カード点 + 役点）
    //  yakuList は呼び出し元で使う役一覧
    // ─────────────────────────────────────────────
    public static int CalcTotal(List<CardData> taken, out List<YakuResult> yakuList)
    {
        int cardPts = CalcCardPoints(taken);
        yakuList = CalcYaku(taken);
        int yakuPts = yakuList.Sum(y => y.Points);
        return cardPts + yakuPts;
    }

    // ─────────────────────────────────────────────
    //  結果サマリー文字列（StatusTextに渡す用）
    // ─────────────────────────────────────────────
    public static string BuildResultText(List<CardData> playerTaken, List<CardData> cpuTaken)
    {
        int pTotal = CalcTotal(playerTaken, out var pYaku);
        int cTotal = CalcTotal(cpuTaken, out var cYaku);

        string pYakuStr = pYaku.Count > 0
            ? string.Join(" / ", pYaku.Select(y => $"{y.Name}+{y.Points}"))
            : "役なし";
        string cYakuStr = cYaku.Count > 0
            ? string.Join(" / ", cYaku.Select(y => $"{y.Name}+{y.Points}"))
            : "役なし";

        string winner = pTotal > cTotal ? "★ あなたの勝ち！"
                      : pTotal < cTotal ? "CPU の勝ち"
                      : "引き分け";

        return $"─── ゲーム終了 ───\n"
             + $"あなた  : 札{ScoreCalculator.CalcCardPoints(playerTaken)}pt  {pYakuStr}  合計{pTotal}pt\n"
             + $"CPU     : 札{ScoreCalculator.CalcCardPoints(cpuTaken)}pt  {cYakuStr}  合計{cTotal}pt\n"
             + $"{winner}";
    }
}