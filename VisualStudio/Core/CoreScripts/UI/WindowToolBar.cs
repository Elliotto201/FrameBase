using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameBase
{
    internal sealed class WindowToolBar : IWindowRenderer
    {
        public void Render()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save")) { /* Handle save */ }
                    if (ImGui.BeginMenu("Import"))
                    {
                        if (ImGui.MenuItem("Media"))
                        {

                        }

                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View"))
                {
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        }
    }
}
