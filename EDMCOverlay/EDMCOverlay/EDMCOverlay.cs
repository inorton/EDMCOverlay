namespace EDMCOverlay
{
    public class EDMCOverlay
    {
        public static void Main(string[] argv)
        {
            OverlayRenderer renderer = new OverlayRenderer();

            new OverlayJsonServer(5010, renderer).Start();
        }
    }
}