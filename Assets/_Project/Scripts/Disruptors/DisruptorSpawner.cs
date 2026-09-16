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
    /// 경계도가 비상에 닿아 증원을 부르면, 플레이어가 마지막으로 발견된 층 끝에서 한 명을 더 보낸다.
    ///
    /// 24일차: 처음부터 전부 세우지 않는다. 판이 흐르는 시간에 따라 층당 허용 인원이 0 → 4로 늘어나고,
    /// 빈자리가 생길 때마다 배치표 순서대로 한 명씩 등장한다 (<see cref="PopulationLogic"/>).
    /// 사물(안내 방송실)은 인원에 세지 않고 처음부터 세운다.
    /// </summary>
    public sealed class DisruptorSpawner : MonoBehaviour
    {
        private static DisruptorSpawner _current;

        /// <summary>지금 스테이지의 스포너다. 열차 도착처럼 판 도중에 방해 세력을 내보내는 규칙이 쓴다.</summary>
        public static DisruptorSpawner Current =>
            _current;

        /// <summary>방해 세력은 층 중앙보다 조금 안쪽 레인에 선다. NPC 무리와 겹쳐 가려지지 않게 한다.</summary>
        private const float LaneY = -1.6f;

        private StageSessionController _stage;
        private Transform _player;
        private FollowerManager _followers;
        private ZoneAlert _alert;
        private int _floorCount;
        private LocationId _location;

        private readonly List<DisruptorPlacement> _pending =
            new List<DisruptorPlacement>();

        private readonly List<DisruptorBase> _alive =
            new List<DisruptorBase>();

        private float _elapsed;

        /// <summary>지금 한 층에 허용되는 인원이다.</summary>
        public int AllowedPerFloor =>
            PopulationLogic.GetAllowed(
                _elapsed,
                Balance.BalanceOverrides.StageOrDefault.DisruptorRampSeconds);

        /// <summary>아직 등장하지 않은 배치 인원이다.</summary>
        public int PendingCount =>
            _pending.Count;

        public static DisruptorSpawner Create(
            LocationDefinition location,
            int floorCount,
            StageSessionController stage,
            Transform player,
            FollowerManager followers)
        {
            // 24일차: 지난 구역의 인파 흐름 · 단체 PT 상태를 지운다. 정적 상태라 씬이 바뀌어도 남는다.
            CrowdFlow.ResetState();
            GymZone.ResetState();
            RadioLink.ResetState();

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
            spawner._location = location.Id;
            _current = spawner;

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
                if (DisruptorCatalog.Get(placements[i].Kind).IsObject)
                {
                    spawner.Spawn(
                        placements[i].Kind,
                        placements[i].Floor,
                        placements[i].X,
                        placements[i].Variant);
                }
                else
                {
                    spawner._pending.Add(placements[i]);
                }
            }

            spawner.SpawnDue();

            return spawner;
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            _elapsed += Time.deltaTime;

            SpawnDue();
        }

        /// <summary>디버그 치트: 시간을 건너뛰어 층마다 최대 인원까지 바로 내보낸다.</summary>
        public void DebugFillAll()
        {
            _elapsed =
                Mathf.Max(
                    _elapsed,
                    PopulationLogic.MaxPerFloor *
                    Mathf.Max(
                        0f,
                        Balance.BalanceOverrides.StageOrDefault.DisruptorRampSeconds));

            SpawnDue(true);
        }

        /// <summary>그 층에서 더 내보낼 수 있는 인원이다. 열차 행인이 이만큼만 내린다.</summary>
        public int GetFreeSlots(
            int floor)
        {
            return PopulationLogic.GetFreeSlots(
                AllowedPerFloor,
                CountAlive(floor));
        }

        /// <summary>빈자리가 있는 층부터 배치표 순서대로 내보낸다.</summary>
        private void SpawnDue(
            bool ignorePlayer = false)
        {
            int allowed = AllowedPerFloor;

            if (allowed <= 0)
            {
                return;
            }

            for (int i = 0;
                 i < _pending.Count;
                 i++)
            {
                DisruptorPlacement placement = _pending[i];

                if (CountAlive(placement.Floor) >= allowed)
                {
                    continue;
                }

                // 플레이어 눈앞에서 갑자기 생기지 않게, 가까이 있으면 다음에 내보낸다.
                if (!ignorePlayer &&
                    IsPlayerNear(placement))
                {
                    continue;
                }

                _pending.RemoveAt(i);
                i--;

                DisruptorBase body =
                    Spawn(
                        placement.Kind,
                        placement.Floor,
                        placement.X,
                        placement.Variant);

                if (body != null)
                {
                    Presentation.GameVfx.Ripple(
                        (Vector2)body.transform.position + new Vector2(0f, 1f),
                        new Color(1f, 1f, 1f, 0.6f),
                        0.8f,
                        0.4f);
                }
            }
        }

        private bool IsPlayerNear(
            DisruptorPlacement placement)
        {
            if (_player == null ||
                FloorSpace.FloorAt(_player.position.y) != placement.Floor)
            {
                return false;
            }

            return Mathf.Abs(_player.position.x - placement.X) <
                   PopulationLogic.SpawnClearDistance;
        }

        private int CountAlive(
            int floor)
        {
            int count = 0;

            for (int i = _alive.Count - 1;
                 i >= 0;
                 i--)
            {
                DisruptorBase body = _alive[i];

                // 퇴장한 소매치기 · 끝에 닿은 행인은 자리를 비운다.
                if (body == null ||
                    !body.gameObject.activeInHierarchy)
                {
                    _alive.RemoveAt(i);

                    continue;
                }

                if (body.Floor == floor)
                {
                    count++;
                }
            }

            return count;
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }

            if (_alert != null)
            {
                _alert.ReinforcementRequested -=
                    HandleReinforcement;
            }
        }

        private void HandleReinforcement(
            Vector2 lastSeen)
        {
            DisruptorKind? kind =
                DisruptorCatalog.GetReinforcementKind(
                    _location);

            // 증원을 보내지 않는 장소(야시장)는 회수 지점 잠금만 걸린다.
            if (kind == null)
            {
                return;
            }

            int floor =
                FloorPlanLogic.ClampFloor(
                    FloorSpace.FloorAt(
                        lastSeen.y),
                    _floorCount);

            // 24일차: 증원도 층당 최대 인원을 넘지 않는다.
            if (CountAlive(floor) >= PopulationLogic.MaxPerFloor)
            {
                return;
            }

            // 발견 지점 반대편 복도 끝에서 들어온다. 바로 옆에 생겨나면 피할 틈이 없다.
            float x =
                lastSeen.x > 0f
                    ? FloorSpace.WalkMinX + 3f
                    : FloorSpace.WalkMaxX - 3f;

            DisruptorBase body =
                Spawn(
                    kind.Value,
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
            float x,
            int variant = 0)
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

            RuntimeCharacterSpriteAnimator animator = null;

            if (profile.IsObject)
            {
                // 사람이 아닌 사물(안내 방송실 · CCTV)은 벽이나 천장에 붙은 상자로 그린다.
                bool camera = kind == DisruptorKind.SecurityCamera;

                LocationProps.Box(
                    go.transform,
                    camera ? "Camera" : "Speaker",
                    position + new Vector2(0f, camera ? 2.6f : 0.5f),
                    camera ? new Vector2(0.5f, 0.35f) : new Vector2(1.6f, 0.8f),
                    new Color(0.18f, 0.20f, 0.26f),
                    -40);
            }
            else
            {
                go.AddComponent<SpriteRenderer>();

                animator =
                    go.AddComponent<RuntimeCharacterSpriteAnimator>();

                animator.Configure(
                    profile.SpriteRoot,
                    7f,
                    390f);

                animator.SetBaseTint(
                    profile.Tint);

                go.AddComponent<DepthSortByY>();
            }

            DisruptorBase body =
                go.AddComponent<DisruptorBase>();

            body.Configure(
                profile,
                _stage,
                animator,
                floor);

            // 촬영팀처럼 부채꼴 대신 조명으로 보는 감시자는 감시 역할을 붙이지 않는다.
            if (profile.Role == DisruptorRole.Watcher &&
                profile.SightRange > 0f)
            {
                WatcherRole watcher =
                    go.AddComponent<WatcherRole>();

                watcher.Configure(
                    _player);
            }

            AddRole(
                go,
                profile,
                variant);

            AddAbility(
                go,
                profile);

            DisruptorStatusView view =
                go.AddComponent<DisruptorStatusView>();

            view.Configure(
                body);

            if (!profile.IsObject)
            {
                _alive.Add(body);
            }

            return body;
        }

        /// <summary>감시 외 역할을 붙인다 (23일차).</summary>
        private void AddRole(
            GameObject go,
            DisruptorProfile profile,
            int variant)
        {
            switch (profile.Kind)
            {
                case DisruptorKind.BeachHunter:
                    go.AddComponent<ContesterRole>().Configure(
                        _followers,
                        _player,
                        variant);
                    break;

                case DisruptorKind.StallTout:
                    go.AddComponent<StallToutRole>().Configure(
                        _followers,
                        _player);
                    break;

                case DisruptorKind.Drunkard:
                    go.AddComponent<BlockerRole>().Configure(
                        _followers,
                        _player);
                    break;

                case DisruptorKind.FlyerStaff:
                    go.AddComponent<StallToutRole>().Configure(
                        _followers,
                        _player,
                        true);
                    break;

                case DisruptorKind.StationAttendant:
                    go.AddComponent<GateRole>().Configure(
                        _followers,
                        _player,
                        DisruptorCatalog.SubwayGateX);
                    break;

                case DisruptorKind.PhoneCommuter:
                    go.AddComponent<CommuterRole>().Configure(
                        _followers,
                        _player,
                        variant);
                    break;

                case DisruptorKind.PersonalTrainer:
                    go.AddComponent<RescuerRole>().Configure();
                    break;

                case DisruptorKind.SecurityGuard:
                    go.AddComponent<RadioLink>().Configure(true);
                    break;

                case DisruptorKind.SecurityCamera:
                    go.AddComponent<RadioLink>().Configure(false);
                    break;

                case DisruptorKind.PromoStaff:
                    go.AddComponent<StallToutRole>().Configure(
                        _followers,
                        _player,
                        true,
                        3f,
                        "시음해 보고 가세요~");
                    break;

                case DisruptorKind.OfficeManager:
                    go.AddComponent<ManagerRole>().Configure();
                    break;

                case DisruptorKind.ClubMd:
                    go.AddComponent<ContesterRole>().Configure(
                        _followers,
                        _player,
                        0);
                    break;

                case DisruptorKind.OfficeRomeo:
                    go.AddComponent<ContesterRole>().Configure(
                        _followers,
                        _player,
                        0);
                    break;

                case DisruptorKind.GymVeteran:
                    go.AddComponent<BrawlerRole>().Configure(
                        _followers,
                        _player);
                    break;
            }
        }

        private void AddAbility(
            GameObject go,
            DisruptorProfile profile)
        {
            switch (profile.Ability)
            {
                case SpecialAbilityKind.AttendanceCheck:
                    go.AddComponent<AttendanceCheckAbility>().Configure(
                        _followers);
                    break;

                case SpecialAbilityKind.WhistleAlarm:
                    go.AddComponent<WhistleAlarmAbility>().Configure(
                        _player);
                    break;

                case SpecialAbilityKind.LiveBroadcast:
                    go.AddComponent<LiveBroadcastAbility>().Configure(
                        _player);
                    break;

                case SpecialAbilityKind.Pickpocket:
                    go.AddComponent<PickpocketAbility>().Configure(
                        _player,
                        _followers,
                        _stage);
                    break;

                case SpecialAbilityKind.PlatformChange:
                    go.AddComponent<PlatformChangeAbility>().Configure(
                        _stage);
                    break;

                case SpecialAbilityKind.GroupPt:
                    go.AddComponent<GroupPtAbility>().Configure(
                        _stage,
                        _followers);
                    break;

                case SpecialAbilityKind.AllIn:
                    go.AddComponent<AllInAbility>();
                    break;

                case SpecialAbilityKind.ShutterLock:
                    go.AddComponent<ShutterLockAbility>().Configure(
                        _player);
                    break;

                case SpecialAbilityKind.EmergencyMeeting:
                    go.AddComponent<EmergencyMeetingAbility>().Configure(
                        _followers,
                        _player);
                    break;

                case SpecialAbilityKind.TempoUp:
                    go.AddComponent<TempoUpAbility>().Configure();
                    break;

                case SpecialAbilityKind.DroneTracking:
                    go.AddComponent<DroneTrackingAbility>().Configure(
                        _player,
                        _stage);
                    break;
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
