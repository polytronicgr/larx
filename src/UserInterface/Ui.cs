using System;
using System.Collections.Generic;
using System.Linq;
using Larx.UserInterface.Components;
using Larx.UserInterface.Widgets;
using OpenTK;

namespace Larx.UserInterface
{
    public class Ui
    {
        private Matrix4 pMatrix;
        public static readonly UiState State;
        private readonly Dock page;
        private readonly MainMenu mainMenu;
        private readonly RightMenu rightMenu;
        private readonly ApplicationInfo applicationInfo;

        static Ui()
        {
            State = new UiState();
        }

        public Ui()
        {
            pMatrix = Matrix4.CreateOrthographicOffCenter(0, Larx.State.Window.Size.Width, Larx.State.Window.Size.Height, 0f, 0f, -1f);

            mainMenu = new MainMenu();
            rightMenu = new RightMenu();
            applicationInfo = new ApplicationInfo();

            page = new Dock("page", new Vector2(1280, 720), new List<Child>() {
                new Child(DockPosition.BottomLeft, mainMenu.Component),
                new Child(DockPosition.BottomRight, rightMenu.Component),
                new Child(DockPosition.TopLeft, applicationInfo.Component),
            });
        }

        public bool Update()
        {
            pMatrix = Matrix4.CreateOrthographicOffCenter(0, Larx.State.Window.Size.Width, Larx.State.Window.Size.Height, 0f, 0f, -1f);

            State.MouseRepeat = State.MousePressed;
            State.MousePressed = Larx.State.Mouse.LeftButton;
            State.HoverKey = page.Intersect(Larx.State.Mouse.Position, new Vector2(0.0f, 0.0f));

            applicationInfo.Update();

            if (State.HoverKey == null) return false;
            if (!State.MousePressed || State.MouseRepeat) return true;

            mainMenu.Update();
            rightMenu.Update();

            return true;
        }

        public void Render()
        {
            page.Render(pMatrix, new Vector2(0, 0));
        }

        public void KeyPress(Char key)
        {
            // textBox.KeyPress(key, 200.0f);
        }
    }
}