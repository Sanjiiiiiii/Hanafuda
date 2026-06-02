using UnityEngine;
using UnityEditor;
using System.IO;

public class CardGenerator
{
    [MenuItem("HanaFuda/Generate 32 Cards")]
    public static void GenerateCards()
    {
        // 保存先フォルダの作成
        string folderPath = "Assets/ScriptableObjects/Cards";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 8ヶ月分のデータ定義（月、キャラ名、色、カード構成）
        var monthConfigs = new[]
        {
            new { Month = 1, Name = "スペシャルウィーク", Color = new Color(0.9f, 0.2f, 0.2f), Patterns = new[] { ("光", CardType.Hikari, 20), ("短冊", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } },
            new { Month = 2, Name = "サイレンススズカ",   Color = new Color(0.6f, 0.3f, 0.8f), Patterns = new[] { ("種", CardType.Tane, 10), ("短冊", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } },
            new { Month = 3, Name = "トウカイテイオー",   Color = new Color(1.0f, 0.4f, 0.6f), Patterns = new[] { ("光", CardType.Hikari, 20), ("短冊", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } },
            new { Month = 6, Name = "メジロマックイーン", Color = new Color(0.0f, 0.7f, 0.5f), Patterns = new[] { ("種", CardType.Tane, 10), ("短冊(青)", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } },
            new { Month = 7, Name = "ゴールドシップ",     Color = new Color(0.8f, 0.6f, 0.2f), Patterns = new[] { ("種", CardType.Tane, 10), ("短冊", CardType.Tanzaku, 5), ("短冊", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1) } },
            new { Month = 8, Name = "ウオッカ",           Color = new Color(0.2f, 0.4f, 0.8f), Patterns = new[] { ("光", CardType.Hikari, 20), ("種", CardType.Tane, 10), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } },
            new { Month = 9, Name = "オグリキャップ",     Color = new Color(0.5f, 0.5f, 0.5f), Patterns = new[] { ("種", CardType.Tane, 10), ("短冊(青)", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } },
            new { Month = 10,Name = "シンボリルドルフ",   Color = new Color(0.6f, 0.1f, 0.1f), Patterns = new[] { ("種", CardType.Tane, 10), ("短冊(青)", CardType.Tanzaku, 5), ("カス", CardType.Kasu, 1), ("カス", CardType.Kasu, 1) } }
        };

        foreach (var config in monthConfigs)
        {
            for (int i = 0; i < config.Patterns.Length; i++)
            {
                var p = config.Patterns[i];
                CardData card = ScriptableObject.CreateInstance<CardData>();
                card.month = config.Month;
                card.charaName = config.Name;
                card.themeColor = config.Color;
                card.cardLabel = p.Item1;
                card.type = p.Item2;
                card.points = p.Item3;

                string assetPath = $"{folderPath}/{config.Month:D2}_{config.Name}_{i}.asset";
                AssetDatabase.CreateAsset(card, assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("🎉 32枚のウマ娘花札データを自動生成しました！");
    }
}