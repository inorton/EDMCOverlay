using System;

namespace EDMCOverlay
{
    public class Graphic
    {
        public String Id { get; set; }

        public String Text { get; set; }

        // a colour name "red", "yellow", "green", "blue"
        public String Color { get; set; }

        // divide the screen by 10 rows
        public int Y { get; set; }

        // divide the screen by 12 cols
        public int X { get; set; }

        // seconds to display
        public int TTL { get; set; }
    }
}