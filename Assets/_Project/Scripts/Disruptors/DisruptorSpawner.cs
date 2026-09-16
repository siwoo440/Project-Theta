using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 장소의 방해 세력을 만든다 (22일차).
    ///
    /// 구역 경계도(<see cref="ZoneAlert"/>)를 먼저 만들고, 장소별 배치표대로 층마다 방해 세력을 세운다.
    /// 경계도가 비상에 닿아 증원을 부르면, 플레이어가 마지막으로 발견된 층 끝에서 교육 조교 한 명을 더 보낸다.
    /// </summary>
    public sealed class DisruptorSpawner : MonoBehaviour
    {
        /// <summary>방해 세력은 층 중앙보다 조금 안쪽 레인에 선다. NPC 무리와 겹쳐 가려지지 않게 한다.</summary>
        private const float LaneY = -1.6f;

        private StageSessionController _stage;
        private Transform _player;
        private FollowerManager _followers;
        private ZoneAlert _alert;
        private int _floorCount;

        public static DisruptorSpawner Create(
            LocationDefinition location,
            int floorCount,
            StageSessionController stage,
            Transform player,
            FollowerManager followers)
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(
                    location.Id,
                    floorCount);

            // 방해 세력이 없는 장소는 경계도도 만들지 않는다. HUD 막대도 뜨지 않는다.
            if (placements.Count == 0)
            {
                return null;
            }

            GameObject root =
                new GameObject(
                    "Disruptors");

            DisruptorSpawner spawner =
                root.AddComponent<DisruptorSpawner>();

            spawner._stage = stage;
            spawner._player = player;
            spawner._followers = followers;
            spawner._floorCount = floorCount;

            spawner._alert =
                root.AddComponent<ZoneAlert>();

            spawner._alert.Configure(
                stage);

            spawner._alert.ReinforcementRequested +=
                spawner.HandleReinforcement;

            for (int i = 0;
                 i < placements.Count;
                 i++)
            {
                spawner.Spawn(
                    placements[i].Kind,
                    placements[i].Floor,
                    placements[i].X);
            }

            return spawner;
        }

        private void OnDestroy()
        {
            if (_alert != null)
            {
                _alert.ReinforcementRequested -=
                    HandleReinforcement;
            }
        }

        private void HandleReinforcement(
            Vector2 lastSeen)
        {
            int floor =
                FloorPlanLogic.ClampFloor(
                    FloorSpace.FloorAt(
                        lastSeen.y),
                    _floorCount);

            // 발견 지점 반대편 복도 끝에서 들어온다. 바로 옆에 생겨나면 피할 틈이 없다.
            float x =
                lastSeen.x > 0f
                    ? FloorSpace.WalkMinX + 3f
                    : FloorSpace.WalkMaxX - 3f;

            DisruptorBase body =
                Spawn(
                    DisruptorKind.TrainingAssistant,
                    floor,
                    x);

            if (body != null)
            {
                body.MoveToward(
                    lastSeen,
                    6f);
            }
        }

        public DisruptorBase Spawn(
            DisruptorKind kind,
            int floor,
            float x)
        {
            DisruptorProfile profile =
                DisruptorCatalog.Get(kind);

            Vector2 position =
                FloorSpace.ToWorld(
                    floor,
                    new Vector2(
                        x,
                        LaneY));

            GameObject go =
                new GameObject(
                    $"{profile.DisplayName}_{floor + 1}F");

            go.transform.SetParent(
                transform,
                false);

            go.transform.position =
                new Vector3(
                    position.x,
                    position.y,
                    0f);

            go.AddComponent<SpriteRenderer>();

            RuntimeCharacterSpriteAnimator animator =
                go.AddComponent<RuntimeCharacterSpriteAnimator>();

            animator.Configure(
                profile.SpriteRoot,
                7f,
                390f);

            animator.SetBaseTint(
                profile.Tint);

            go.AddComponent<DepthSortByY>();

            DisruptorBase body =
                go.AddComponent<DisruptorBase>();

            body.Configure(
                profile,
                _stage,
                animator,
                floor);

            if (profile.Role == DisruptorRole.Watcher)
            {
                WatcherRole watcher =
                    go.AddComponent<WatcherRole>();

                watcher.Configure(
                    _player);
            }

            if (profile.Ability == SpecialAbilityKind.AttendanceCheck)
            {
                AttendanceCheckAbility ability =
                    go.AddComponent<AttendanceCheckAbility>();

                ability.Configure(
                    _followers);
            }

            DisruptorStatusView view =
                go.AddComponent<DisruptorStatusView>();

            view.Configure(
                body);

            return body;
        }
    }
}
