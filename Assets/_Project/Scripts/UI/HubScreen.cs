using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Save;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 계약 서큐버스 허브다.
    ///
    /// 스테이지에서 돌아온 결과를 세이브에 반영하고,
    /// 계약 정기로 4계열 영구 성장을 구매한다.
    /// 정식 UI는 이후 일차에 교체하므로 지금은 IMGUI를 유지한다.
    /// </summary>
    public sealed class HubScreen : MonoBehaviour
    {
        private static readonly UpgradeTrack[] Tracks =
        {
            UpgradeTrack.Hypnosis,
            UpgradeTrack.Control,
            UpgradeTrack.Stability,
            UpgradeTrack.Mobility
        };

        private static readonly DifficultyLevel[] Difficulties =
        {
            DifficultyLevel.Story,
            DifficultyLevel.Normal,
            DifficultyLevel.Challenge
        };

        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _leftStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _smallButtonStyle;

        private bool _resultApplied;
        private StageResultSummary _lastResult =
            StageResultSummary.Empty;

        private bool _hasLastResult;

        private void Start()
        {
            if (GameSession.Instance == null)
            {
                return;
            }

            _hasLastResult =
                GameSession.Instance.HasPendingResult;

            if (_hasLastResult)
            {
                _lastResult =
                    GameSession.Instance.PendingResult;
            }

            _resultApplied =
                GameSession.Instance.ConsumePendingResult();
        }

        private void OnGUI()
        {
            EnsureStyles();

            float width =
                Mathf.Min(
                    620f,
                    Screen.width *
                    0.82f);

            const float height = 520f;

            float x =
                (Screen.width -
                 width) *
                0.5f;

            float y =
                Mathf.Max(
                    8f,
                    (Screen.height -
                     height) *
                    0.5f);

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height),
                string.Empty);

            GUI.Label(
                new Rect(
                    x,
                    y + 18f,
                    width,
                    32f),
                "계약 서큐버스 허브",
                _titleStyle);

            SaveData save =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save;

            DrawLastResult(
                x + 34f,
                y + 58f,
                width - 68f);

            DrawEssence(
                x,
                y + 104f,
                width,
                save);

            DrawUpgrades(
                x + 34f,
                y + 142f,
                width - 68f,
                save);

            DrawDifficulty(
                x + 34f,
                y + 300f,
                width - 68f);

            DrawStats(
                x + 34f,
                y + 360f,
                width - 68f,
                save);

            DrawButtons(
                x,
                y + height - 76f,
                width);
        }

        private void DrawLastResult(
            float x,
            float y,
            float width)
        {
            if (!_hasLastResult)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y,
                        width,
                        22f),
                    "출격 준비 완료",
                    _smallStyle);

                return;
            }

            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    22f),
                _lastResult.Cleared
                    ? $"직전 결과   클리어   랭크 {_lastResult.RankLabel}   {_lastResult.TotalScore:N0}점   계약 정기 +{_lastResult.ContractEssence}"
                    : $"직전 결과   실패   {_lastResult.TotalScore:N0}점   계약 정기 +{_lastResult.ContractEssence}",
                _smallStyle);

            if (!_resultApplied)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y + 20f,
                        width,
                        20f),
                    "반영할 결과가 없습니다",
                    _smallStyle);
            }
        }

        private void DrawEssence(
            float x,
            float y,
            float width,
            SaveData save)
        {
            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    28f),
                save == null
                    ? "계약 정기 -"
                    : $"계약 정기   {save.ContractEssence:N0}",
                _titleStyle);
        }

        private void DrawUpgrades(
            float x,
            float y,
            float width,
            SaveData save)
        {
            if (save == null)
            {
                return;
            }

            const float rowHeight = 36f;

            for (int i = 0;
                 i < Tracks.Length;
                 i++)
            {
                UpgradeTrack track =
                    Tracks[i];

                int level =
                    SaveDataLogic.GetUpgradeLevel(
                        save,
                        track);

                float rowY =
                    y + (rowHeight * i);

                GUI.Label(
                    new Rect(
                        x,
                        rowY,
                        160f,
                        26f),
                    $"{UpgradeLogic.GetTrackName(track)}   {BuildPips(level)}",
                    _leftStyle);

                GUI.Label(
                    new Rect(
                        x + 170f,
                        rowY,
                        width - 290f,
                        26f),
                    GetTrackEffect(
                        track,
                        level),
                    _smallStyle);

                bool maxed =
                    UpgradeLogic.IsMaxLevel(
                        level);

                int cost =
                    UpgradeLogic.GetNextLevelCost(
                        level);

                bool affordable =
                    UpgradeLogic.CanPurchase(
                        level,
                        save.ContractEssence);

                Rect buttonRect =
                    new Rect(
                        x + width - 110f,
                        rowY,
                        110f,
                        28f);

                if (maxed)
                {
                    GUI.Label(
                        buttonRect,
                        "최대",
                        _smallStyle);

                    continue;
                }

                GUI.enabled =
                    affordable;

                if (GUI.Button(
                        buttonRect,
                        $"{cost} 구매",
                        _smallButtonStyle))
                {
                    Purchase(
                        track);
                }

                GUI.enabled =
                    true;
            }
        }

        private void DrawDifficulty(
            float x,
            float y,
            float width)
        {
            GUI.Label(
                new Rect(
                    x,
                    y,
                    140f,
                    24f),
                "난이도",
                _leftStyle);

            float buttonWidth =
                (width - 150f) /
                Difficulties.Length;

            for (int i = 0;
                 i < Difficulties.Length;
                 i++)
            {
                DifficultyLevel level =
                    Difficulties[i];

                bool selected =
                    BalanceOverrides.Difficulty != null &&
                    BalanceOverrides.Difficulty.Level ==
                    level;

                GUI.enabled =
                    !selected;

                if (GUI.Button(
                        new Rect(
                            x + 150f + (buttonWidth * i),
                            y - 2f,
                            buttonWidth - 6f,
                            28f),
                        DifficultyTable.Get(
                            level).DisplayName +
                        (selected
                            ? " ●"
                            : string.Empty),
                        _smallButtonStyle))
                {
                    BalanceOverrides.SetDifficulty(
                        level);
                }

                GUI.enabled =
                    true;
            }

            GUI.Label(
                new Rect(
                    x,
                    y + 28f,
                    width,
                    20f),
                "난이도는 목표 정기 · 제한 시간 · 충동 · 쟁탈 속도에 반영됩니다",
                _smallStyle);
        }

        private void DrawStats(
            float x,
            float y,
            float width,
            SaveData save)
        {
            if (save == null)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y,
                        width,
                        22f),
                    "저장 정보를 불러오지 못했습니다",
                    _smallStyle);

                return;
            }

            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    22f),
                $"플레이 {save.PlayCount}회   클리어 {save.ClearCount}회   최고 랭크 {save.BestRankLabel}   최고 점수 {save.BestScore:N0}",
                _smallStyle);

            GUI.Label(
                new Rect(
                    x,
                    y + 20f,
                    width,
                    20f),
                BalanceBootstrap.StageAssetApplied
                    ? "밸런스 자산 적용됨"
                    : "밸런스 자산 없음 - 코드 기본값 사용 중",
                _smallStyle);
        }

        private void DrawButtons(
            float x,
            float y,
            float width)
        {
            float buttonWidth =
                (width - 100f) *
                0.5f;

            if (GUI.Button(
                    new Rect(
                        x + 40f,
                        y,
                        buttonWidth,
                        42f),
                    "출 격",
                    _buttonStyle))
            {
                GameSession.Instance?.GoTo(
                    SceneDestination.Stage);
            }

            if (GUI.Button(
                    new Rect(
                        x + 60f + buttonWidth,
                        y,
                        buttonWidth,
                        42f),
                    "타이틀로",
                    _buttonStyle))
            {
                GameSession.Instance?.GoTo(
                    SceneDestination.MainMenu);
            }
        }

        private void Purchase(
            UpgradeTrack track)
        {
            if (GameSession.Instance == null)
            {
                return;
            }

            if (SaveDataLogic.TryPurchaseUpgrade(
                    GameSession.Instance.Save,
                    track))
            {
                GameAudio.Play(
                    GameSfx.Purchase);

                // 구매 즉시 저장한다. 허브에서 나가기 전에 껐을 때 손실되지 않도록.
                GameSession.Instance.WriteSave();
            }
        }

        private static string BuildPips(
            int level)
        {
            int clamped =
                UpgradeLogic.ClampLevel(
                    level);

            string pips =
                string.Empty;

            for (int i = 0;
                 i < UpgradeLogic.MaximumLevel;
                 i++)
            {
                pips +=
                    i < clamped
                        ? "●"
                        : "○";
            }

            return pips;
        }

        private static string GetTrackEffect(
            UpgradeTrack track,
            int level)
        {
            switch (track)
            {
                case UpgradeTrack.Hypnosis:
                    return $"최면 속도 ×{UpgradeLogic.GetHypnosisSpeedMultiplier(level):0.00}";

                case UpgradeTrack.Control:
                    return $"관리 한도 {UpgradeLogic.GetStableFollowerLimit(level, 4)}명";

                case UpgradeTrack.Stability:
                    return $"충동 ×{UpgradeLogic.GetImpulseBuildMultiplier(level):0.00}   경고 +{UpgradeLogic.GetRampageWarningBonus(level):0.0}초";

                case UpgradeTrack.Mobility:
                    return $"이동 ×{UpgradeLogic.GetMoveSpeedMultiplier(level):0.00}   대시 비용 ×{UpgradeLogic.GetDashCostMultiplier(level):0.00}";

                default:
                    return string.Empty;
            }
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 22,
                    fontStyle =
                        FontStyle.Bold
                };

            _labelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 14
                };

            _leftStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleLeft,
                    fontSize = 16,
                    fontStyle =
                        FontStyle.Bold
                };

            _smallStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleLeft,
                    fontSize = 13
                };

            _buttonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 17,
                    fontStyle =
                        FontStyle.Bold
                };

            _smallButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle =
                        FontStyle.Bold
                };
        }
    }
}
