using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public enum GameState
{
    Dealing,
    PlayerSelectHandCard,   // プレイヤーが手札を選ぶ
    PlayerSelectFieldCard,  // 場に2枚合う場合、プレイヤーが場札を選ぶ
    DrawFromPile,           // 山札をめくる（プレイヤーターン後）
    CpuTurn,                // CPUの手順1+2をまとめて処理
    GameEnd
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefab")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Areas")]
    [SerializeField] private HandArea playerHandArea;
    [SerializeField] private HandArea cpuHandArea;
    [SerializeField] private FieldArea fieldArea;
    [SerializeField] private DrawPileView drawPileView;

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text cpuScoreText;

    // ゲームデータ
    private Stack<CardData> drawPileData = new Stack<CardData>();
    public List<CardData> PlayerTaken { get; } = new List<CardData>();
    public List<CardData> CpuTaken { get; } = new List<CardData>();

    // 手札→場の合わせ待ち用
    private CardData pendingCardData;
    private CardView pendingCardView;

    private GameState _currentState;

    // ─────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start() => DealCards();

    // ─────────────────────────────────────────────
    //  配 札
    // ─────────────────────────────────────────────
    private void DealCards()
    {
        SetStatus("配札中...");

        CardData[] allCards = Resources.LoadAll<CardData>("Cards");
        if (allCards.Length == 0)
        {
            Debug.LogError("Resources/Cards/ にカードデータが見つかりません！フォルダを確認してください");
            return;
        }

        var deck = new List<CardData>(allCards);
        Shuffle(deck);

        // 場に出る最初の8枚に同月4枚がないかチェック
        if (deck.GetRange(0, 8).GroupBy(c => c.month).Any(g => g.Count() >= 4))
        {
            Debug.Log("同月4枚検出 → 配りなおし");
            DealCards();
            return;
        }

        int i = 0;
        for (int n = 0; n < 6; n++) SpawnToHand(deck[i++], playerHandArea);
        for (int n = 0; n < 6; n++) SpawnToHand(deck[i++], cpuHandArea);
        for (int n = 0; n < 8; n++) SpawnToField(deck[i++]);

        drawPileData.Clear();
        for (; i < deck.Count; i++) drawPileData.Push(deck[i]);
        drawPileView.UpdateDisplay(drawPileData.Count);

        SetState(GameState.PlayerSelectHandCard);
    }

    // ─────────────────────────────────────────────
    //  CardView 生成ヘルパー
    // ─────────────────────────────────────────────
    private CardView SpawnToHand(CardData data, HandArea area)
    {
        var cv = CreateView(data);
        area.AddCard(cv);
        return cv;
    }

    private CardView SpawnToField(CardData data)
    {
        var cv = CreateView(data);
        fieldArea.AddCard(cv);
        return cv;
    }

    private CardView CreateView(CardData data)
    {
        var go = Instantiate(cardPrefab);
        var cv = go.GetComponent<CardView>();
        cv.Setup(data);
        return cv;
    }

    // ─────────────────────────────────────────────
    //  状態遷移
    // ─────────────────────────────────────────────
    private void SetState(GameState next)
    {
        _currentState = next; // 状態を更新

        // まず全選択解除
        playerHandArea.SetSelectableNone();
        fieldArea.SetSelectableNone();

        switch (next)
        {
            case GameState.PlayerSelectHandCard:
                SetStatus("手札を1枚選んでください");
                playerHandArea.SetAllSelectable(true);
                break;

            case GameState.PlayerSelectFieldCard:
                SetStatus("取る場札を選んでください");
                fieldArea.SetSelectableByMonth(pendingCardData.month);
                break;

            case GameState.DrawFromPile:
                SetStatus("山札をめくります...");
                StartCoroutine(DrawFromPileCoroutine());
                break;

            case GameState.CpuTurn:
                SetStatus("CPUのターン...");
                StartCoroutine(CpuTurnCoroutine());
                break;

            case GameState.GameEnd:
                SetStatus(ScoreCalculator.BuildResultText(PlayerTaken, CpuTaken));
                UpdateScoreDisplay();
                break;
        }
    }

    // ─────────────────────────────────────────────
    //  クリック処理（CardViewから呼ばれる）
    // ─────────────────────────────────────────────
    public void OnCardClicked(CardView clicked)
    {
        switch (_currentState)
        {
            case GameState.PlayerSelectHandCard:
                HandleHandCardSelected(clicked);
                break;
            case GameState.PlayerSelectFieldCard:
                HandleFieldCardSelected(clicked);
                break;
        }
    }

    private void HandleHandCardSelected(CardView handCard)
    {
        playerHandArea.SetSelectableNone();
        playerHandArea.RemoveCard(handCard);

        pendingCardData = handCard.Data;
        pendingCardView = handCard;

        var matches = fieldArea.GetCardsByMonth(handCard.Data.month);

        if (matches.Count == 0)
        {
            // 場に追加
            fieldArea.AddCard(handCard);
            pendingCardData = null;
            pendingCardView = null;
            SetState(GameState.DrawFromPile);
        }
        else if (matches.Count == 1)
        {
            // 1枚合わせ → 自動取得
            TakePair(handCard.Data, matches[0], isPlayer: true);
            Destroy(handCard.gameObject);
            pendingCardData = null;
            pendingCardView = null;
            SetState(GameState.DrawFromPile);
        }
        else
        {
            // 2枚以上合う → 選ばせる（手札カードは一時非表示）
            handCard.gameObject.SetActive(false);
            SetState(GameState.PlayerSelectFieldCard);
        }
    }

    private void HandleFieldCardSelected(CardView fieldCard)
    {
        TakePair(pendingCardData, fieldCard, isPlayer: true);
        if (pendingCardView != null)
            Destroy(pendingCardView.gameObject);
        pendingCardData = null;
        pendingCardView = null;
        SetState(GameState.DrawFromPile);
    }

    // ─────────────────────────────────────────────
    //  取り札処理
    // ─────────────────────────────────────────────
    private void TakePair(CardData playedData, CardView fieldCardView, bool isPlayer)
    {
        var takenList = isPlayer ? PlayerTaken : CpuTaken;
        fieldArea.RemoveCard(fieldCardView);
        takenList.Add(playedData);
        takenList.Add(fieldCardView.Data);
        Destroy(fieldCardView.gameObject);
        UpdateScoreDisplay();
    }

    // ─────────────────────────────────────────────
    //  山札をめくる（プレイヤーターン後）
    // ─────────────────────────────────────────────
    private IEnumerator DrawFromPileCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (drawPileData.Count == 0)
        {
            SetState(GameState.GameEnd);
            yield break;
        }

        CardData drawn = drawPileData.Pop();
        drawPileView.UpdateDisplay(drawPileData.Count);
        SetStatus($"山札: {drawn.charaName}（{drawn.cardLabel}）");
        yield return new WaitForSeconds(0.8f);

        var matches = fieldArea.GetCardsByMonth(drawn.month);
        if (matches.Count == 0)
        {
            SpawnToField(drawn);
            SetStatus($"{drawn.charaName} → 場に置く");
        }
        else
        {
            TakePair(drawn, matches[0], isPlayer: true);
            SetStatus($"{drawn.charaName} → 合わせ取り！");
        }

        yield return new WaitForSeconds(0.5f);

        // ゲーム終了チェック
        if (playerHandArea.Cards.Count == 0 || drawPileData.Count == 0)
            SetState(GameState.GameEnd);
        else
            SetState(GameState.CpuTurn);
    }

    // ─────────────────────────────────────────────
    //  CPUターン（手順1+2をまとめて）
    // ─────────────────────────────────────────────
    private IEnumerator CpuTurnCoroutine()
    {
        yield return new WaitForSeconds(0.8f);

        // ── CPU 手順1：手札から1枚出す ──
        var cpuCards = cpuHandArea.Cards.ToList();
        if (cpuCards.Count == 0) { SetState(GameState.GameEnd); yield break; }

        // CpuPlayer で最善の手を選ぶ
        var toPlay = CpuPlayer.PickHandCard(cpuHandArea.Cards, fieldArea.Cards);
        cpuHandArea.RemoveCard(toPlay);

        var matches = MatchingSystem.GetMatches(toPlay.Data, fieldArea.Cards);

        if (matches.Count == 0)
        {
            SetStatus($"CPU: {toPlay.Data.charaName} を場に出す");
            fieldArea.AddCard(toPlay);
        }
        else
        {
            // 複数の合わせ候補がある場合は最高点の場札を選ぶ
            var fieldTarget = CpuPlayer.PickFieldCard(matches);
            SetStatus($"CPU: {toPlay.Data.charaName} で合わせ取り！");
            TakePair(toPlay.Data, fieldTarget, isPlayer: false);
            Destroy(toPlay.gameObject);
        }

        yield return new WaitForSeconds(0.8f);

        // ── CPU 手順2：山札をめくる ──
        if (drawPileData.Count == 0) { SetState(GameState.GameEnd); yield break; }

        CardData drawn = drawPileData.Pop();
        drawPileView.UpdateDisplay(drawPileData.Count);
        SetStatus($"CPU山札: {drawn.charaName}（{drawn.cardLabel}）");
        yield return new WaitForSeconds(0.8f);

        var drawnMatches = fieldArea.GetCardsByMonth(drawn.month);
        if (drawnMatches.Count == 0)
        {
            SpawnToField(drawn);
            SetStatus($"CPU山札 {drawn.charaName} → 場に置く");
        }
        else
        {
            TakePair(drawn, drawnMatches[0], isPlayer: false);
            SetStatus($"CPU山札 {drawn.charaName} → 合わせ取り！");
        }

        yield return new WaitForSeconds(0.5f);

        // ゲーム終了チェック
        if (cpuHandArea.Cards.Count == 0 || drawPileData.Count == 0)
            SetState(GameState.GameEnd);
        else
            SetState(GameState.PlayerSelectHandCard);
    }

    // ─────────────────────────────────────────────
    //  UI更新
    // ─────────────────────────────────────────────
    private void UpdateScoreDisplay()
    {
        if (playerScoreText != null)
        {
            int pTotal = ScoreCalculator.CalcTotal(PlayerTaken, out var pYaku);
            string pYakuStr = pYaku.Count > 0 ? $"（{string.Join("/", pYaku.Select(y => y.Name))}）" : "";
            playerScoreText.text = $"あなた: {pTotal}pt {pYakuStr}";
        }
        if (cpuScoreText != null)
        {
            int cTotal = ScoreCalculator.CalcTotal(CpuTaken, out var cYaku);
            string cYakuStr = cYaku.Count > 0 ? $"（{string.Join("/", cYaku.Select(y => y.Name))}）" : "";
            cpuScoreText.text = $"CPU: {cTotal}pt {cYakuStr}";
        }
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log("[Game] " + msg);
    }

    // ─────────────────────────────────────────────
    //  ユーティリティ
    // ─────────────────────────────────────────────
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}