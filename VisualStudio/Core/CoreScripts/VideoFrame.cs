public class VideoFrame
{
    public byte[] Pixels { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Timestamp { get; set; }
    public int StreamIndex { get; set; }
}