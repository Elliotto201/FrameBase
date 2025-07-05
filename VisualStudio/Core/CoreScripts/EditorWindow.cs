using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;

namespace FrameBase
{
    internal class EditorWindow
    {
        public static EditorWindow Current { get; private set; }

        private bool IsFullscreen;
        private Vector2 WindowSize = new Vector2(800, 600);

        private WindowToolBar ToolBarRenderer;

        public bool showWindow = true;
        public string ProjectFilePath { get; private set; }

        internal EditorWindow(string projectFilePath)
        {
            Current = this;

            ToolBarRenderer = new WindowToolBar();
            ProjectFilePath = projectFilePath;

            MediaLoader.Initialize();
        }

        public void Render()
        {
            if (IsFullscreen)
            {
                WindowSize = ImGui.GetIO().DisplaySize;
                ImGui.SetNextWindowSize(WindowSize, ImGuiCond.Always);
            }
            else
            {
                ImGui.SetNextWindowSize(WindowSize, ImGuiCond.Once);
            }

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 1));
            ImGui.Begin("FrameBase", ref showWindow, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.MenuBar);
            if (IsFullscreen)
            {
                ImGui.SetWindowPos(new Vector2(0, 0));
            }

            ToolBarRenderer.Render();

            string resizeText = IsFullscreen ? "Minimize" : "FullScreen";

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 75, 35));
            if (ImGui.Button(resizeText))
            {
                IsFullscreen = !IsFullscreen;

                if (!IsFullscreen)
                {
                    WindowSize = new Vector2(800, 600);
                }
            }

            ImGui.End();
        }
    }
}