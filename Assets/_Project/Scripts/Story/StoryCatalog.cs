using System.Collections.Generic;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Story
{
    /// <summary>이야기 장면이 나오는 조건이다 (36일차).</summary>
    public enum StoryTrigger
    {
        /// <summary>그 장소에 처음 들어갈 때(장소 안에서).</summary>
        LocationEnter = 0,

        /// <summary>그 장소를 처음 클리어한 뒤(지도 · 허브에서).</summary>
        LocationClear = 1,

        /// <summary>서로 다른 장소를 <see cref="StoryScene.Threshold"/>곳 이상 클리어한 뒤.</summary>
        LocationsCleared = 2,

        /// <summary>엔딩을 <see cref="StoryScene.Threshold"/>회 이상 본 뒤.</summary>
        Endings = 3,

        /// <summary>도시 지배도가 <see cref="StoryScene.Threshold"/>% 이상이 된 뒤.</summary>
        Dominion = 4
    }

    /// <summary>대사 한 줄이다. 말하는 사람이 비어 있으면 해설이다.</summary>
    public struct StoryLine
    {
        public readonly string Speaker;
        public readonly string Text;

        public StoryLine(
            string speaker,
            string text)
        {
            Speaker = speaker ?? string.Empty;
            Text = text ?? string.Empty;
        }
    }

    /// <summary>이야기 장면 하나다. <see cref="Id"/>는 세이브에 남으므로 바꾸지 않는다.</summary>
    public sealed class StoryScene
    {
        public readonly string Id;
        public readonly string Title;
        public readonly StoryTrigger Trigger;
        public readonly LocationId Location;
        public readonly int Threshold;
        public readonly StoryLine[] Lines;

        public StoryScene(
            string id,
            string title,
            StoryTrigger trigger,
            LocationId location,
            int threshold,
            params StoryLine[] lines)
        {
            Id = id;
            Title = title;
            Trigger = trigger;
            Location = location;
            Threshold = threshold;
            Lines = lines ?? new StoryLine[0];
        }

        public bool HasLocation =>
            Trigger == StoryTrigger.LocationEnter ||
            Trigger == StoryTrigger.LocationClear;
    }

    /// <summary>
    /// 이야기 장면 표다 (36일차). 대사는 초안이며 자유롭게 고쳐도 된다(ID만 유지).
    ///
    ///   장소마다 입장 1 · 첫 클리어 1 (루프탑 클럽 입장은 라이벌 대면)
    ///   라이벌 도발 2 (클리어 장소 3 · 6곳)
    ///   후일담 2 (엔딩 1회 · 도시 지배도 100%)
    /// 순서는 다시 보기 목록 순서이기도 하다.
    /// </summary>
    public static class StoryCatalog
    {
        public const string Me = "나";
        public const string Rival = "라이벌";

        private static StoryLine L(string speaker, string text)
        {
            return new StoryLine(speaker, text);
        }

        private static StoryLine N(string text)
        {
            return new StoryLine(string.Empty, text);
        }

        public static readonly StoryScene[] All =
        {
            // 기업 연수원 -------------------------------------------------
            new StoryScene("enter_training", "첫 출근", StoryTrigger.LocationEnter, LocationId.TrainingCenter, 0,
                N("신입 사원 합숙 교육 첫날. 복도에는 피곤한 얼굴들이 가득하다."),
                L(Me, "지친 사람일수록 마음이 쉽게 열리지. 여기서부터 시작하자."),
                L("교육 조교", "거기! 교육 중에 복도에서 뭐 하는 거죠?")),

            new StoryScene("clear_training", "첫 계약", StoryTrigger.LocationClear, LocationId.TrainingCenter, 0,
                L(Me, "첫 정기가 몸에 스며든다… 이 정도면 버틸 수 있어."),
                N("창밖으로 도시의 불빛이 보인다. 저 너머에 더 많은 사람들이 있다.")),

            // 해변가 -----------------------------------------------------
            new StoryScene("enter_beach", "뜨거운 백사장", StoryTrigger.LocationEnter, LocationId.Beach, 0,
                N("한낮의 백사장. 숨을 곳은 파라솔 그늘뿐이다."),
                L("라이프가드", "물때 바뀝니다! 물가에서 떨어지세요!"),
                L(Me, "한 명씩은 비효율적이야. 여럿을 한 번에 데려가자.")),

            new StoryScene("clear_beach", "파도 소리", StoryTrigger.LocationClear, LocationId.Beach, 0,
                L("헌팅남", "저기요, 아까 그 사람들 다 어디 갔어요?"),
                L(Me, "글쎄? 파도에 휩쓸려 간 거 아닐까.")),

            // 지하철 환승역 ------------------------------------------------
            new StoryScene("enter_subway", "퇴근길 인파", StoryTrigger.LocationEnter, LocationId.SubwayStation, 0,
                N("열차가 설 때마다 사람들이 쏟아져 나온다."),
                L("안내 방송", "승강장이 변경되었습니다. 승객 여러분께서는…"),
                L(Me, "이 물결 속에서 내 사람들을 놓치면 안 돼.")),

            new StoryScene("clear_subway", "막차", StoryTrigger.LocationClear, LocationId.SubwayStation, 0,
                L(Me, "인파는 파도 같아. 버티는 법만 알면 무섭지 않아."),
                N("막차가 떠나고, 역에 정적이 내려앉았다.")),

            // 헬스장 -----------------------------------------------------
            new StoryScene("enter_fitness", "뜨거운 심장", StoryTrigger.LocationEnter, LocationId.FitnessCenter, 0,
                N("쇠 부딪히는 소리와 거친 숨소리."),
                L("관장", "자, 전원 단체 PT 들어갑니다! 하나, 둘!"),
                L(Me, "심장이 빨리 뛰는 사람은 최면도 빨리 걸려. 대신 다루기 어렵겠지.")),

            new StoryScene("clear_fitness", "대회 전날", StoryTrigger.LocationClear, LocationId.FitnessCenter, 0,
                L("퍼스널 트레이너", "회원님들이 오늘따라 왜 이렇게 멍하시지…?"),
                L(Me, "운동보다 좋은 휴식을 줬을 뿐이야.")),

            // 야시장 -----------------------------------------------------
            new StoryScene("enter_market", "등불 골목", StoryTrigger.LocationEnter, LocationId.NightMarket, 0,
                N("좁은 골목에 등불이 흔들린다. 어둠 속에는 낯선 손길도 섞여 있다."),
                L("호객꾼", "언니, 이거 한번 드셔 보고 가세요!"),
                L(Me, "등불 밖은 눈이 흐려져. 빛 아래에서 움직이자.")),

            new StoryScene("clear_market", "빈 주머니", StoryTrigger.LocationClear, LocationId.NightMarket, 0,
                L("소매치기", "쳇, 오늘은 내가 털린 기분이네."),
                L(Me, "훔치는 건 네 일이 아니라 내 일이야.")),

            // 쇼핑몰 -----------------------------------------------------
            new StoryScene("enter_mall", "폐점 30분 전", StoryTrigger.LocationEnter, LocationId.ShoppingMall, 0,
                N("반짝이는 진열대, 천장마다 달린 카메라."),
                L("보안요원", "3층 이상 없음. 계속 순찰한다."),
                L(Me, "들키지 않고 해내면 더 많이 얻을 수 있어. 기둥 뒤로.")),

            new StoryScene("clear_mall", "꺼진 화면", StoryTrigger.LocationClear, LocationId.ShoppingMall, 0,
                L("보안팀장", "CCTV에 아무것도 안 찍혔다고? 그럼 손님들은 어디로…"),
                L(Me, "카메라는 마음까지는 못 보니까.")),

            // 오피스 타워 -------------------------------------------------
            new StoryScene("enter_office", "야근의 탑", StoryTrigger.LocationEnter, LocationId.OfficeTower, 0,
                N("밤 열한 시. 꺼지지 않는 층들."),
                L("꼰대 부장", "다들 집에 갈 생각 하지 마. 보고서 끝날 때까지!"),
                L(Me, "위층으로 가려면 출입증이 필요해. 직원 한 명을 데려가야겠어.")),

            new StoryScene("clear_office", "정시 퇴근", StoryTrigger.LocationClear, LocationId.OfficeTower, 0,
                L("비서실장", "대표님 비서가… 퇴근을 했다고요? 이 시간에?"),
                L(Me, "가끔은 쉬어야지. 누군가 대신 일해 주면 되니까.")),

            // 루프탑 클럽 -------------------------------------------------
            new StoryScene("enter_club", "라이벌", StoryTrigger.LocationEnter, LocationId.RooftopClub, 0,
                N("도시 꼭대기. 음악이 심장처럼 울린다."),
                L(Rival, "드디어 왔네, 꼬마 서큐버스. 이 도시의 밤은 내 거야."),
                L(Me, "그 밤, 오늘 내가 가져갈게."),
                L(Rival, "그럼 춤으로 증명해 봐. 내 손님들을 빼앗을 수 있다면.")),

            new StoryScene("clear_club", "음악이 멈춘 밤", StoryTrigger.LocationClear, LocationId.RooftopClub, 0,
                L(Rival, "…나보다 빛나는 밤은 처음이야."),
                L(Me, "이제부터 이 도시는 내가 지킬게.")),

            // 라이벌 도발 --------------------------------------------------
            new StoryScene("rival_taunt_3", "지켜보는 눈", StoryTrigger.LocationsCleared, LocationId.TrainingCenter, 3,
                N("휴대폰에 낯선 메시지가 도착했다."),
                L(Rival, "제법이네. 연수원, 바닷가… 귀여운 동네 산책은 즐거웠어?"),
                L(Me, "…누구지? 이 기척, 같은 서큐버스야.")),

            new StoryScene("rival_taunt_6", "초대장", StoryTrigger.LocationsCleared, LocationId.TrainingCenter, 6,
                N("창틀에 보라색 초대장이 꽂혀 있다. '루프탑 클럽, 오늘 밤.'"),
                L(Rival, "도시의 반을 가져갔다고 우쭐하지 마. 꼭대기는 아직 내 거니까."),
                L(Me, "기다려. 곧 올라갈게.")),

            // 후일담 ------------------------------------------------------
            new StoryScene("epilogue_ending", "후일담 · 새벽", StoryTrigger.Endings, LocationId.TrainingCenter, 1,
                N("루프탑의 음악이 멈추고, 도시에 새벽이 왔다."),
                L(Rival, "착각하지 마. 난 사라지지 않아. 밤마다 다시 올라올 거야."),
                L(Me, "그럼 밤마다 다시 이기면 되지."),
                N("더 깊은 밤, 심야의 도시가 열렸다.")),

            new StoryScene("epilogue_dominion", "후일담 · 도시의 주인", StoryTrigger.Dominion, LocationId.TrainingCenter, 100,
                N("어느 골목을 걸어도, 사람들은 그녀의 이름을 속삭인다."),
                L(Rival, "…인정할게. 이 도시의 밤은 네 거야."),
                L(Me, "그래도 가끔은 같이 춤춰 줄게."),
                N("학생의 작은 방 창문 너머로, 도시의 불빛이 온통 보랏빛이었다."))
        };

        private static Dictionary<string, StoryScene> _byId;

        public static StoryScene Get(
            string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, StoryScene>();

                foreach (StoryScene scene in All)
                {
                    _byId[scene.Id] = scene;
                }
            }

            return id != null &&
                   _byId.TryGetValue(id, out StoryScene found)
                ? found
                : null;
        }
    }
}
