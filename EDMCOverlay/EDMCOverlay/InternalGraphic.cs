using System;

namespace EDMCOverlay
{
    public class InternalGraphic
    {
        private DateTime expires = DateTime.Now;
        public Graphic RealGraphic { get; private set; }
        public int ClientId { get; private set; }

        public InternalGraphic(Graphic g, int clientId)
        {
            RealGraphic = g;
            this.Update(g);
            this.ClientId = clientId;
        }

        public void Update(Graphic g)
        {
            expires = DateTime.Now.AddSeconds(g.TTL);
            RealGraphic.Text = g.Text;
            RealGraphic.Color = g.Color;
            RealGraphic.OldX = RealGraphic.X;
            RealGraphic.OldY = RealGraphic.Y;
            RealGraphic.X = g.X;
            RealGraphic.Y = g.Y;
        }

        public bool Expired
        {
            get
            {
                var lifeleft = expires.Subtract(DateTime.Now).TotalSeconds;
                return !(lifeleft > 0);
            }
        }
        
        public Rect InvalidateRect
        {
            get
            {
                var r = new Rect();
                r.X = RealGraphic.X - 3;
                r.Y = RealGraphic.Y - 3;

                r.W = RealGraphic.W;
                r.H = RealGraphic.H;

                if (!String.IsNullOrWhiteSpace(RealGraphic.Text))
                {
                    r.H = 32;
                    r.W = 24 * RealGraphic.Text.Length;
                }

                return r;
            }
        } 
    }
}