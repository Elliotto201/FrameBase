using ClickableTransparentOverlay;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FrameBase
{
    internal class LauncherWindow : Overlay
    {
        private bool InProject = false;
        private EditorWindow EditorWindow;

        private string projectName = "";
        private static string ProjectsFolder = string.Empty;
        private string[] ProjectFiles = Array.Empty<string>();
        private string CurrentSelectedProject = string.Empty;
        private double lastClickTime = 0.0;
        private string lastClickedProject = string.Empty;

        internal LauncherWindow()
        {
            string dirPath = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/FrameBase/").FullName;

            if (!File.Exists(dirPath + ".projectsfolder"))
            {
                File.WriteAllText(dirPath + ".projectsfolder", dirPath);
            }

            ProjectsFolder = File.ReadAllText(dirPath + ".projectsfolder");
            RefreshProjects();
        }

        protected override void Render()
        {
            if (InProject)
            {
                EditorWindow.Render();
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(1000, 600), ImGuiCond.FirstUseEver);
            ImGui.Begin("FrameBase Launcher", ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse);

            ImGui.SameLine(ImGui.GetWindowWidth() - 30);
            if (ImGui.Button("X", new Vector2(20, 20)))
            {
                Environment.Exit(0);
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 210, ImGui.GetWindowHeight() - 50));
            if (ImGui.Button("Create Project", new Vector2(200, 40)))
            {
                ImGui.OpenPopup("CreateProject");
                RefreshProjects();
            }

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 430, ImGui.GetWindowHeight() - 50));
            if (ImGui.Button("Delete Project", new Vector2(200, 40)))
            {
                ImGui.OpenPopup("DeleteProject");
            }

            ImGui.SetNextWindowPos(ImGui.GetWindowSize() / 2);
            if (ImGui.BeginPopup("CreateProject"))
            {
                ImGui.InputText("New Project Name", ref projectName, 12);

                if (ImGui.Button("OK"))
                {
                    if (File.Exists(ProjectsFolder + projectName + ".fbp"))
                    {
                        int prefix = 0;

                        while (File.Exists(ProjectsFolder + projectName + prefix + ".fbp"))
                        {
                            prefix++;
                        }

                        File.Create(ProjectsFolder + projectName + prefix + ".fbp").Close();
                        RefreshProjects();

                        ImGui.CloseCurrentPopup();
                    }
                    else
                    {
                        File.Create(ProjectsFolder + projectName + ".fbp").Close();
                        RefreshProjects();

                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            ImGui.SetNextWindowPos(ImGui.GetWindowSize() / 2);
            if (ImGui.BeginPopup("DeleteProject"))
            {
                if (ImGui.Button("OK"))
                {
                    if (File.Exists(CurrentSelectedProject))
                    {
                        File.Delete(CurrentSelectedProject);
                    }

                    RefreshProjects();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            ImGui.SetCursorPos(new Vector2(10, 30));
            foreach (var file in ProjectFiles)
            {
                if (CurrentSelectedProject.Equals(file))
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.1f, 1f, 1));
                }
                if (ImGui.Button(Path.GetFileNameWithoutExtension(file), new Vector2(90, 50)))
                {
                    CurrentSelectedProject = file;
                    double currentTime = ImGui.GetTime();
                    if (currentTime - lastClickTime < 0.8 && lastClickedProject == file)
                    {
                        LaunchProject(file);
                    }
                    lastClickTime = currentTime;
                    lastClickedProject = file;
                }
                if (CurrentSelectedProject.Equals(file))
                {
                    ImGui.PopStyleColor(1);
                }
            }

            ImGui.End();
        }

        private void RefreshProjects()
        {
            ProjectFiles = Directory.GetFiles(ProjectsFolder).Where(t => t.EndsWith(".fbp")).ToArray();
        }

        private void LaunchProject(string projectFile)
        {
            EditorWindow = new EditorWindow(projectFile);
            InProject = true;
        }
    }
}