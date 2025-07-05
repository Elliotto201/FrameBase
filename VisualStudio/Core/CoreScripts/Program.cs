using FrameBase;

internal class Program
{
    public static void Main(string[] args)
    {
        var lWindow = new LauncherWindow();
        Thread rendererThread = new Thread(lWindow.Start().Wait);
        rendererThread.Start();
    }
}