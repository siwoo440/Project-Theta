using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Core;
using ProjectTheta.Save;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class InputBindingLogicTests
    {
        [Test]
        public void Default_Table_Matches_The_Old_Controls()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual("W", table.Get(GameAction.MoveUp, 0));
            Assert.AreEqual("UpArrow", table.Get(GameAction.MoveUp, 1));
            Assert.AreEqual("LeftShift", table.Get(GameAction.Dash, 0));
            Assert.AreEqual("Space", table.Get(GameAction.Dash, 1));
            Assert.AreEqual("F", table.Get(GameAction.Interact, 0));
            Assert.AreEqual(InputBindingLogic.MouseRight, table.Get(GameAction.Wave, 0));
            Assert.AreEqual("Digit1", table.Get(GameAction.Item1, 0));
            Assert.AreEqual("Digit2", table.Get(GameAction.Item2, 0));
        }

        [Test]
        public void Hypnosis_Default_Is_Left_Click_Only()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual(InputBindingLogic.MouseLeft, table.Get(GameAction.Hypnosis, 0));
            Assert.AreEqual(string.Empty, table.Get(GameAction.Hypnosis, 1));
        }

        [Test]
        public void Every_Action_Has_A_Primary_Key_And_Default_Has_No_Conflict()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            for (int i = 0; i < InputBindingLogic.ActionCount; i++)
            {
                GameAction action = (GameAction)i;

                Assert.IsFalse(string.IsNullOrEmpty(table.Get(action, 0)), action.ToString());
                Assert.IsFalse(string.IsNullOrEmpty(InputBindingLogic.GetLabel(action)));
                Assert.IsFalse(string.IsNullOrEmpty(InputBindingLogic.GetGroup(action)));
            }

            Assert.IsFalse(InputBindingLogic.HasAnyConflict(table));
        }

        [Test]
        public void Conflict_Is_Warned_And_Not_Saved()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            BindingResult result =
                InputBindingLogic.TrySet(table, GameAction.Dash, 0, "W", out BindingSlot conflict);

            Assert.AreEqual(BindingResult.Conflict, result);
            Assert.AreEqual(GameAction.MoveUp, conflict.Action);
            Assert.AreEqual(0, conflict.Slot);

            // 표는 그대로다.
            Assert.AreEqual("LeftShift", table.Get(GameAction.Dash, 0));
            Assert.AreEqual("W", table.Get(GameAction.MoveUp, 0));

            StringAssert.Contains("위로 이동", InputBindingLogic.DescribeResult(result, "W", conflict));
            StringAssert.Contains("저장하지 않았습니다", InputBindingLogic.DescribeResult(result, "W", conflict));
        }

        [Test]
        public void Same_Action_Other_Slot_Is_Also_A_Conflict()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual(
                BindingResult.Conflict,
                InputBindingLogic.TrySet(table, GameAction.Dash, 1, "LeftShift", out _));
        }

        [Test]
        public void Struggle_Keys_Are_Checked_Separately_From_Field_Keys()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            // 기본값에서 최면과 탈출 A가 둘 다 좌클릭이어도 겹침이 아니다.
            Assert.IsFalse(InputBindingLogic.FindConflict(table, GameAction.StruggleLeft, 0, InputBindingLogic.MouseLeft, out _));

            // 탈출 A · B끼리는 겹치면 안 된다.
            Assert.AreEqual(
                BindingResult.Conflict,
                InputBindingLogic.TrySet(table, GameAction.StruggleLeft, 0, InputBindingLogic.MouseRight, out _));

            // 평소 조작 키(W)를 탈출 입력으로 쓰는 것은 된다.
            Assert.AreEqual(
                BindingResult.Ok,
                InputBindingLogic.TrySet(table, GameAction.StruggleLeft, 0, "W", out _));
        }

        [Test]
        public void Free_Key_Is_Saved()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual(BindingResult.Ok, InputBindingLogic.TrySet(table, GameAction.Hypnosis, 1, "E", out _));
            Assert.AreEqual("E", table.Get(GameAction.Hypnosis, 1));

            Assert.AreEqual(BindingResult.Unchanged, InputBindingLogic.TrySet(table, GameAction.Hypnosis, 1, "E", out _));
        }

        [Test]
        public void Reserved_And_Invalid_Keys_Are_Refused()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual(BindingResult.Reserved, InputBindingLogic.TrySet(table, GameAction.Dash, 0, "Escape", out _));
            Assert.AreEqual(BindingResult.Reserved, InputBindingLogic.TrySet(table, GameAction.Dash, 0, "F1", out _));
            Assert.AreEqual(BindingResult.Invalid, InputBindingLogic.TrySet(table, GameAction.Dash, 0, "", out _));
            Assert.AreEqual(BindingResult.Invalid, InputBindingLogic.TrySet(table, GameAction.Dash, 0, "A=B", out _));
            Assert.AreEqual(BindingResult.Invalid, InputBindingLogic.TrySet(table, GameAction.Dash, 5, "Q", out _));

            Assert.AreEqual("LeftShift", table.Get(GameAction.Dash, 0));
        }

        [Test]
        public void Only_Secondary_Slot_Can_Be_Cleared()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual(BindingResult.EmptyPrimary, InputBindingLogic.Clear(table, GameAction.Dash, 0));
            Assert.AreEqual(BindingResult.Ok, InputBindingLogic.Clear(table, GameAction.Dash, 1));
            Assert.AreEqual(string.Empty, table.Get(GameAction.Dash, 1));
            Assert.AreEqual(BindingResult.Unchanged, InputBindingLogic.Clear(table, GameAction.Dash, 1));
        }

        [Test]
        public void Serialize_Round_Trip()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            InputBindingLogic.TrySet(table, GameAction.Interact, 0, "G", out _);
            InputBindingLogic.TrySet(table, GameAction.Hypnosis, 1, "Mouse2", out _);

            string[] lines = InputBindingLogic.Serialize(table);
            KeyBindingTable loaded = InputBindingLogic.Deserialize(lines);

            Assert.AreEqual(InputBindingLogic.ActionCount, lines.Length);
            Assert.Contains("Interact=G,", lines);
            Assert.AreEqual("G", loaded.Get(GameAction.Interact, 0));
            Assert.AreEqual("Mouse2", loaded.Get(GameAction.Hypnosis, 1));
            Assert.AreEqual("W", loaded.Get(GameAction.MoveUp, 0));
        }

        [Test]
        public void Missing_Or_Bad_Lines_Fall_Back_To_Defaults()
        {
            KeyBindingTable loaded =
                InputBindingLogic.Deserialize(
                    new[]
                    {
                        "Dash=Q,",
                        "NoSuchAction=K,",
                        "Interact=",
                        "Wave=Escape,",
                        "MoveUp=NotAKey,",
                        "garbage",
                        null,
                        "7=Z,"
                    },
                    code => code != "NotAKey");

            Assert.AreEqual("Q", loaded.Get(GameAction.Dash, 0));
            Assert.AreEqual(string.Empty, loaded.Get(GameAction.Dash, 1));
            Assert.AreEqual("F", loaded.Get(GameAction.Interact, 0));
            Assert.AreEqual(InputBindingLogic.MouseRight, loaded.Get(GameAction.Wave, 0));
            Assert.AreEqual("W", loaded.Get(GameAction.MoveUp, 0));

            Assert.AreEqual("W", InputBindingLogic.Deserialize(null).Get(GameAction.MoveUp, 0));
        }

        [Test]
        public void Hand_Edited_Conflicts_Reset_Everything()
        {
            KeyBindingTable loaded =
                InputBindingLogic.Deserialize(
                    new[] { "Dash=W,", "Interact=G," });

            // 대시가 W라서 위로 이동과 겹친다 → 전체 기본값.
            Assert.AreEqual("LeftShift", loaded.Get(GameAction.Dash, 0));
            Assert.AreEqual("F", loaded.Get(GameAction.Interact, 0));
        }

        [Test]
        public void Display_Names()
        {
            Assert.AreEqual("Shift", InputBindingLogic.GetDisplayName("LeftShift"));
            Assert.AreEqual("1", InputBindingLogic.GetDisplayName("Digit1"));
            Assert.AreEqual("숫자패드 3", InputBindingLogic.GetDisplayName("Numpad3"));
            Assert.AreEqual("마우스 오른쪽", InputBindingLogic.GetDisplayName("Mouse1"));
            Assert.AreEqual("↑", InputBindingLogic.GetDisplayName("UpArrow"));
            Assert.AreEqual("Q", InputBindingLogic.GetDisplayName("Q"));
            Assert.AreEqual("-", InputBindingLogic.GetDisplayName(""));
            Assert.AreEqual("좌클릭", InputBindingLogic.GetShortName("Mouse0"));
            Assert.AreEqual("F", InputBindingLogic.GetShortName("F"));

            KeyBindingTable table = InputBindingLogic.CreateDefault();

            Assert.AreEqual("Shift / Space", InputBindingLogic.Describe(table, GameAction.Dash));
            Assert.AreEqual("마우스 왼쪽", InputBindingLogic.Describe(table, GameAction.Hypnosis));
        }

        [Test]
        public void Clone_Is_Independent()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();
            KeyBindingTable copy = table.Clone();

            copy.Set(GameAction.Dash, 0, "Q");

            Assert.AreEqual("LeftShift", table.Get(GameAction.Dash, 0));
        }
    }

    public sealed class KeyBindingSaveTests
    {
        [Test]
        public void Save_Keeps_And_Clones_Bindings()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            Assert.IsNotNull(save.KeyBindings);

            save.KeyBindings = new[] { "Dash=Q," };

            SaveData copy = save.Clone();
            copy.KeyBindings[0] = "Dash=Z,";

            Assert.AreEqual("Dash=Q,", save.KeyBindings[0]);
        }

        [Test]
        public void Old_Save_Without_Bindings_Uses_Defaults()
        {
            SaveData old = new SaveData { KeyBindings = null };

            SaveDataLogic.Normalize(old);

            Assert.IsNotNull(old.KeyBindings);
            Assert.AreEqual("LeftShift", InputBindingLogic.Deserialize(old.KeyBindings).Get(GameAction.Dash, 0));
        }

        [Test]
        public void Controls_Window_Follows_The_Table()
        {
            KeyBindingTable table = InputBindingLogic.CreateDefault();

            InputBindingLogic.TrySet(table, GameAction.Interact, 0, "G", out _);
            InputBindingLogic.TrySet(table, GameAction.Hypnosis, 1, "E", out _);

            List<string> keys = new List<string>();

            foreach (ControlRow row in ControlsCatalog.Build(table))
            {
                keys.Add(row.Keys);
            }

            string all = string.Join(" | ", keys);

            StringAssert.Contains("G  (계단", all);
            StringAssert.Contains("마우스 왼쪽 / E", all);
            StringAssert.Contains("W A S D  /  ↑ ← ↓ →", all);
            StringAssert.Contains("1 · 2", all);
        }
    }
}
