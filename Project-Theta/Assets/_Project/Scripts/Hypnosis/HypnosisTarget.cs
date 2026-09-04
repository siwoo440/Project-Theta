using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.NPC;
using ProjectTheta.Player;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(NpcAgent))]
    [RequireComponent(typeof(FollowerController))]
    public sealed class HypnosisTarget : MonoBehaviour
    {
        [SerializeField] private float _maxHypnosis = 100f; // 최대 최면
        [SerializeField] private float _hypnosisRate = 35f; // 최면 속도
        [SerializeField] private float _decayRate = 18f; // 감소 속도
        [SerializeField] private float _focusGrace = 1.5f; // 이탈 유예
        [SerializeField] private float _resistanceMultiplier = 1f; // 저항 보정
        [SerializeField] private float _impulseGrowth = 7f; // 충동 증가
        [SerializeField] private int _essenceValue = 25; // 정기 가치
        private HypnosisMeter _hypnosisMeter; // 최면 게이지
        private ImpulseMeter _impulseMeter; // 충동 게이지
        private NpcAgent _npcAgent; // NPC 상태
        private FollowerController _follower; // 동행 제어
        private bool _isFocused; // 시선 여부
        private bool _rampageTriggered; // 폭주 시작 여부
        private float _rampageTimer; // 폭주 시간
        private Vector3 _rampageTarget; // 돌진 목표

        public bool IsHypnotized { get; private set; } // 최면 상태
        public bool IsFollowing => _follower != null && _follower.IsFollowing; // 동행 상태
        public float HypnosisNormalized => _hypnosisMeter?.Normalized ?? 0f; // 최면 비율
        public float ImpulseNormalized => _impulseMeter?.Normalized ?? 0f; // 충동 비율
        public int EssenceValue => _essenceValue; // 정기 가치
        public HypnosisCaster Owner { get; private set; } // 소유 시전자

        private void Awake()
        {
            _npcAgent = GetComponent<NpcAgent>(); // NPC 참조
            _follower = GetComponent<FollowerController>(); // 동행 참조
            _hypnosisMeter = new HypnosisMeter(_maxHypnosis, _hypnosisRate, _decayRate, _focusGrace); // 최면 생성
            _impulseMeter = new ImpulseMeter(100f, 70f, _impulseGrowth, 25f); // 충동 생성
        }

        private void Update()
        {
            if (!IsHypnotized && !_isFocused) // 비최면 상태 확인
            {
                _hypnosisMeter.Decay(Time.deltaTime); // 최면 감소
            }

            if (IsFollowing && !_rampageTriggered) // 동행 상태 확인
            {
                _impulseMeter.TickFollowing(Time.deltaTime, 1f); // 충동 증가
                if (_impulseMeter.IsRampaging) // 폭주 조건 확인
                {
                    BeginRampage(); // 폭주 시작
                }
            }

            if (_rampageTriggered) // 폭주 상태 확인
            {
                TickRampage(); // 폭주 이동
            }
        }

        public bool ApplyFocus(float deltaTime)
        {
            if (IsHypnotized) // 최면 완료 확인
            {
                return false; // 추가 최면 차단
            }

            _isFocused = true; // 시선 상태 설정
            _hypnosisMeter.Build(deltaTime, _resistanceMultiplier); // 최면 상승
            return _hypnosisMeter.IsComplete; // 완료 반환
        }

        public void EndFocus()
        {
            _isFocused = false; // 시선 해제
            _hypnosisMeter.BeginGrace(); // 유예 시작
        }

        public void BeginFollowing(HypnosisCaster owner, Transform leader, PlayerSideViewController leaderController, int slotIndex)
        {
            Owner = owner; // 소유자 설정
            IsHypnotized = true; // 최면 완료
            _npcAgent.SetState(NpcState.Following); // 동행 상태
            _follower.BeginFollowing(leader, leaderController, slotIndex); // 추종 시작
            _hypnosisMeter.Reset(); // 최면 게이지 초기화
        }

        public void SetFollowerSlot(int slotIndex)
        {
            _follower.SetSlotIndex(slotIndex); // 슬롯 갱신
        }

        public int Collect()
        {
            int value = _essenceValue; // 회수값 저장
            _follower.StopFollowing(); // 추종 종료
            Owner = null; // 소유 해제
            IsHypnotized = false; // 최면 해제
            _npcAgent.SetState(NpcState.Idle); // 대기 상태
            return value; // 정기 반환
        }

        private void BeginRampage()
        {
            if (Owner == null) // 소유자 확인
            {
                return; // 폭주 취소
            }

            _rampageTriggered = true; // 폭주 활성
            _rampageTimer = 1.2f; // 폭주 시간
            _rampageTarget = Owner.transform.position; // 돌진 목표
            _follower.StopFollowing(); // 추종 일시 중단
            _npcAgent.SetState(NpcState.Rampage); // 폭주 상태
        }

        private void TickRampage()
        {
            transform.position = Vector3.MoveTowards(transform.position, _rampageTarget, 8f * Time.deltaTime); // 돌진 이동
            _rampageTimer -= Time.deltaTime; // 폭주 시간 감소
            if (_rampageTimer > 0f) // 폭주 지속 확인
            {
                return; // 종료 대기
            }

            _rampageTriggered = false; // 폭주 해제
            _impulseMeter.SetValue(35f); // 충동 감소
            if (Owner != null) // 소유자 확인
            {
                _npcAgent.SetState(NpcState.Following); // 동행 상태 복귀
                _follower.BeginFollowing(Owner.transform, Owner.PlayerController, Owner.IndexOf(this)); // 추종 복귀
            }
        }
    }
}
