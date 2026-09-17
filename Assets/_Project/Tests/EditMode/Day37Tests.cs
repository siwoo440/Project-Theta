using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Stage.Locations;
using ProjectTheta.Story;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class MusicLogicTests
    {
        [Test]
        public void Scene_Tracks()
        {
            Assert.AreEqual(MusicTrack.Title, MusicLogic.GetTrack(SceneNames.MainMenu, LocationId.Beach, false));
            Assert.AreEqual(MusicTrack.Room, MusicLogic.GetTrack(SceneNames.Hub, LocationId.Beach, false));
            Assert.AreEqual(MusicTrack.City, MusicLogic.GetTrack(SceneNames.Map, LocationId.Beach, false));
            Assert.AreEqual(MusicTrack.Beach, MusicLogic.GetTrack(SceneNames.Stage, LocationId.Beach, false));
            Assert.AreEqual(MusicTrack.Boss, MusicLogic.GetTrack(SceneNames.Stage, LocationId.RooftopClub, false));
            Assert.AreEqual(MusicTrack.Ending, MusicLogic.GetTrack(SceneNames.Stage, LocationId.RooftopClub, true));
            Assert.AreEqual(MusicTrack.None, MusicLogic.GetTrack(SceneNames.Boot, LocationId.Beach, false));
        }

        [Test]
        public void Every_Location_Has_Its_Own_Track()
        {
            HashSet<MusicTrack> tracks = new HashSet<MusicTrack>();

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                MusicTrack track = MusicLogic.GetLocationTrack(location.Id);

                Assert.AreNotEqual(MusicTrack.None, track, location.Id.ToString());
                Assert.IsTrue(tracks.Add(track), location.Id.ToString());
            }
        }

        [Test]
        public void Switch_Only_When_Needed()
        {
            Assert.IsTrue(MusicLogic.ShouldSwitch(MusicTrack.None, MusicTrack.Title));
            Assert.IsTrue(MusicLogic.ShouldSwitch(MusicTrack.Title, MusicTrack.Room));
            Assert.IsFalse(MusicLogic.ShouldSwitch(MusicTrack.Room, MusicTrack.Room));
            Assert.IsFalse(MusicLogic.ShouldSwitch(MusicTrack.Room, MusicTrack.None));
        }

        [Test]
        public void Paths()
        {
            Assert.AreEqual("Audio/Music/Beach", MusicLogic.GetPath(MusicTrack.Beach));
            Assert.AreEqual("Audio/Music/Beach_Tension", MusicLogic.GetTensionPath(MusicTrack.Beach));
            Assert.AreEqual(string.Empty, MusicLogic.GetPath(MusicTrack.None));
        }

        [Test]
        public void Intensity()
        {
            Assert.AreEqual(MusicIntensity.Calm, MusicLogic.GetIntensity(false, true, 5f, 0.1f));
            Assert.AreEqual(MusicIntensity.Calm, MusicLogic.GetIntensity(true, false, 100f, 1f));
            Assert.AreEqual(MusicIntensity.Chase, MusicLogic.GetIntensity(true, true, 100f, 1f));
            Assert.AreEqual(MusicIntensity.Danger, MusicLogic.GetIntensity(true, true, 10f, 1f));
            Assert.AreEqual(MusicIntensity.Danger, MusicLogic.GetIntensity(true, false, 100f, 0.2f));

            // 시간이 0이면 이미 끝난 것이라 위기로 보지 않는다.
            Assert.AreEqual(MusicIntensity.Calm, MusicLogic.GetIntensity(true, false, 0f, 1f));

            Assert.AreEqual(0f, MusicLogic.GetTensionVolume(MusicIntensity.Calm));
            Assert.Less(MusicLogic.GetTensionVolume(MusicIntensity.Chase), MusicLogic.GetTensionVolume(MusicIntensity.Danger));
        }

        [Test]
        public void Pitch_And_Volume()
        {
            Assert.AreEqual(1f, MusicLogic.GetPitch(false, MusicIntensity.Calm));
            Assert.Less(MusicLogic.GetPitch(true, MusicIntensity.Calm), 1f);
            Assert.Greater(MusicLogic.GetPitch(false, MusicIntensity.Danger), 1f);

            Assert.AreEqual(MusicLogic.BaseVolume, MusicLogic.GetVolume(1f, false, 1f), 0.0001f);
            Assert.AreEqual(MusicLogic.BaseVolume * MusicLogic.DialogueDuck, MusicLogic.GetVolume(1f, true, 1f), 0.0001f);
            Assert.AreEqual(0f, MusicLogic.GetVolume(0f, false, 1f));
            Assert.AreEqual(0f, MusicLogic.GetVolume(1f, false, 0f));
            Assert.AreEqual(MusicLogic.BaseVolume, MusicLogic.GetVolume(5f, false, 3f), 0.0001f);
            Assert.AreEqual(0f, MusicLogic.GetVolume(float.NaN, false, 1f));
        }

        [Test]
        public void Tension_Sync_And_Fade_Step()
        {
            Assert.AreEqual(0, MusicLogic.GetTensionSample(0, 1000));
            Assert.AreEqual(250, MusicLogic.GetTensionSample(3250, 1000));
            Assert.AreEqual(0, MusicLogic.GetTensionSample(3250, 0));

            Assert.AreEqual(0.5f, MusicLogic.Step(0f, 1f, 0.6f, 1.2f), 0.0001f);
            Assert.AreEqual(1f, MusicLogic.Step(0.9f, 1f, 1f, 1.2f));
            Assert.AreEqual(0f, MusicLogic.Step(0.1f, 0f, 1f, 1.2f));
            Assert.AreEqual(1f, MusicLogic.Step(0f, 1f, 0.01f, 0f));
        }

        [Test]
        public void Dialogue_Blips()
        {
            Assert.IsTrue(MusicLogic.ShouldBlip(0, 1, "안녕"));
            Assert.IsFalse(MusicLogic.ShouldBlip(1, 2, "안녕"));
            Assert.IsFalse(MusicLogic.ShouldBlip(2, 2, "안녕하세요"));
            Assert.IsFalse(MusicLogic.ShouldBlip(0, 1, " 안"));
            Assert.IsFalse(MusicLogic.ShouldBlip(0, 1, null));
            Assert.IsTrue(MusicLogic.ShouldBlip(0, 10, "가나다라마바사아자차"));

            Assert.AreNotEqual(MusicLogic.GetBlipPitch(StoryCatalog.Me), MusicLogic.GetBlipPitch(StoryCatalog.Rival));
            Assert.AreEqual(MusicLogic.GetBlipPitch("교육 조교"), MusicLogic.GetBlipPitch("교육 조교"));

            foreach (StoryScene scene in StoryCatalog.All)
            {
                foreach (StoryLine line in scene.Lines)
                {
                    Assert.That(MusicLogic.GetBlipPitch(line.Speaker), Is.InRange(0.5f, 2f));
                }
            }
        }
    }

    public sealed class TempAudioFileTests
    {
        private static string FindAudioFolder()
        {
            string directory = Environment.CurrentDirectory;

            for (int i = 0; i < 8 && !string.IsNullOrEmpty(directory); i++)
            {
                string candidate = Path.Combine(directory, "Assets", "_Project", "Resources", "Audio");

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = Path.GetDirectoryName(directory);
            }

            return null;
        }

        [Test]
        public void Every_Track_And_Sound_Has_A_Temp_File()
        {
            string folder = FindAudioFolder();

            if (folder == null)
            {
                Assert.Ignore("프로젝트 폴더 밖에서 실행되어 음원 파일을 확인하지 않습니다.");
            }

            foreach (MusicTrack track in Enum.GetValues(typeof(MusicTrack)))
            {
                if (track == MusicTrack.None)
                {
                    continue;
                }

                Assert.IsTrue(File.Exists(Path.Combine(folder, "Music", track + ".wav")), track.ToString());
                Assert.IsTrue(File.Exists(Path.Combine(folder, "Music", track + "_Tension.wav")), track + "_Tension");
            }

            foreach (GameSfx sfx in Enum.GetValues(typeof(GameSfx)))
            {
                Assert.IsTrue(File.Exists(Path.Combine(folder, sfx + ".wav")), sfx.ToString());
            }
        }
    }

    public sealed class HubSaveAndLeaveTests
    {
        [Test]
        public void Save_Slot_Buttons()
        {
            Save.SaveSlotSummary empty = new Save.SaveSlotSummary { Slot = 0 };
            Save.SaveSlotSummary used = new Save.SaveSlotSummary { Slot = 1, Exists = true };

            Assert.AreEqual("여기에 저장", Save.SaveSlotLogic.GetSaveButton(used, true, false));
            Assert.AreEqual("이 칸에 저장", Save.SaveSlotLogic.GetSaveButton(empty, false, false));
            Assert.AreEqual("덮어쓰기", Save.SaveSlotLogic.GetSaveButton(used, false, false));
            StringAssert.Contains("한 번 더", Save.SaveSlotLogic.GetSaveButton(used, false, true));

            // 지금 칸 · 빈 칸은 바로, 다른 기록이 있는 칸은 확인 뒤에만 저장한다.
            Assert.IsTrue(Save.SaveSlotLogic.CanSave(used, true, false));
            Assert.IsTrue(Save.SaveSlotLogic.CanSave(empty, false, false));
            Assert.IsFalse(Save.SaveSlotLogic.CanSave(used, false, false));
            Assert.IsTrue(Save.SaveSlotLogic.CanSave(used, false, true));
        }

        [Test]
        public void Saved_Message()
        {
            Assert.AreEqual("1번 칸에 저장했습니다  ·  2026-09-17 12:00", Save.SaveSlotLogic.GetSavedMessage(0, 0, "2026-09-17 12:00"));
            StringAssert.Contains("이제 3번 칸으로 플레이", Save.SaveSlotLogic.GetSavedMessage(2, 0, "2026-09-17 12:00"));
        }

        [Test]
        public void Leave_Needs_A_Second_Press()
        {
            Assert.IsFalse(UI.HubRoomLogic.ShouldLeave(0f));
            Assert.IsFalse(UI.HubRoomLogic.ShouldLeave(-1f));
            Assert.IsTrue(UI.HubRoomLogic.ShouldLeave(UI.HubRoomLogic.LeaveConfirmSeconds));
            Assert.Greater(UI.HubRoomLogic.LeaveConfirmSeconds, 0f);

            StringAssert.Contains("마지막 저장 2026-09-17 12:00", UI.HubRoomLogic.GetLeaveWarning("2026-09-17 12:00"));
            StringAssert.Contains("저장한 적이 없습니다", UI.HubRoomLogic.GetLeaveWarning(string.Empty));
            StringAssert.Contains("한 번 더", UI.HubRoomLogic.GetLeaveWarning(null));
        }
    }
}
