using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using Overlay.NET.Common;
using Overlay.NET.Directx;
using Process.NET.Windows;

namespace EDMCOverlay
{
    public class OverlayController : DirectXOverlayPlugin
    {
        private Dictionary<String, InternalGraphic> _graphics;
        private int _framerate = 0;
        private readonly TickEngine _tickEngine = new TickEngine();

        private int _displayFps;
        private int _font;
        private int _hugeFont;
        private int _i;
        private int _interiorBrush;
        private int _redBrush;
        private int _redOpacityBrush;
        private float _rotation;
        private Stopwatch _watch;

        Dictionary<String, int> brushes = new Dictionary<string, int>();

        public void SetFrameRate(int rate)
        {
            this._framerate = rate;
        }

        public void SetGraphics(Dictionary<String, InternalGraphic> graphics)
        {
            _graphics = graphics;
        }

        public override void Initialize(IWindow targetWindow)
        {
            // Set target window by calling the base method
            base.Initialize(targetWindow);


            OverlayWindow = new DirectXOverlayWindow(targetWindow.Handle, false);
            _watch = Stopwatch.StartNew();

            brushes.Add("red", OverlayWindow.Graphics.CreateBrush(0x7FFF0000));
            brushes.Add("yellow", OverlayWindow.Graphics.CreateBrush(0x7FFFFF00));
            brushes.Add("green", OverlayWindow.Graphics.CreateBrush(0x7F00FF00));
            brushes.Add("blue", OverlayWindow.Graphics.CreateBrush(0x7F0000FF));

            _redBrush = OverlayWindow.Graphics.CreateBrush(0x7FFF0000);
            _redOpacityBrush = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(80, 255, 0, 0));
            _interiorBrush = OverlayWindow.Graphics.CreateBrush(0x7FFFFF00);

            _font = OverlayWindow.Graphics.CreateFont("Arial", 16);
            _hugeFont = OverlayWindow.Graphics.CreateFont("Arial", 24, bold: true);

            _i = 0;
            // Set up update interval and register events for the tick engine.

            _tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!OverlayWindow.IsVisible)
            {
                return;
            }

            InternalRender();
        }

        private void OnPreTick(object sender, EventArgs e)
        {
            var targetWindowIsActivated = TargetWindow.IsActivated;
            if (!targetWindowIsActivated && OverlayWindow.IsVisible)
            {
#if false
                _watch.Stop();
                ClearScreen();
                OverlayWindow.Hide();
#endif
            }
            else if (targetWindowIsActivated && !OverlayWindow.IsVisible)
            {
                OverlayWindow.Show();
            }
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Enable()
        {
            _tickEngine.Interval = TimeSpan.FromMilliseconds(1000.0 / this._framerate);
            _tickEngine.IsTicking = true;
            base.Enable();
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Disable()
        {
            _tickEngine.IsTicking = false;
            base.Disable();
        }

        public override void Update()
        {
            _tickEngine.Pulse();
            // sleep to prevent the main thread going mad
            Thread.Sleep((int) (1000.0 / this._framerate));
        }

        protected void InternalRender()
        {
            if (!_watch.IsRunning)
            {
                _watch.Start();
            }

            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();

            lock (_graphics)
            {
                foreach (string gid in _graphics.Keys.ToArray())
                {
                    InternalGraphic g = _graphics[gid];
                    if (g.Expired)
                    {
                        _graphics.Remove(gid);
                    }
                    else
                    {

                        var draw = _graphics[gid].RealGraphic;
                        if (brushes.ContainsKey(draw.Color))
                        {
                            int brush = brushes[draw.Color];
                            int font = _font;
                            if (draw.Size != null)
                            {
                                if (draw.Size.Equals("large"))
                                {
                                    font = _hugeFont;
                                }
                            }
                            OverlayWindow.Graphics.DrawText(
                                draw.Text, font, brush, draw.X, draw.Y);
                        }
                    }
                }
            }

            /*
            //first row
            OverlayWindow.Graphics.DrawText("DrawBarH", _font, _redBrush, 50, 40);
            OverlayWindow.Graphics.DrawBarH(50, 70, 20, 100, 80, 2, _redBrush, _interiorBrush);

            OverlayWindow.Graphics.DrawText("DrawBarV", _font, _redBrush, 200, 40);
            OverlayWindow.Graphics.DrawBarV(200, 120, 100, 20, 80, 2, _redBrush, _interiorBrush);

            OverlayWindow.Graphics.DrawText("DrawBox2D", _font, _redBrush, 350, 40);
            OverlayWindow.Graphics.DrawBox2D(350, 70, 50, 100, 2, _redBrush, _redOpacityBrush);

            OverlayWindow.Graphics.DrawText("DrawBox3D", _font, _redBrush, 500, 40);
            OverlayWindow.Graphics.DrawBox3D(500, 80, 50, 100, 10, 2, _redBrush, _redOpacityBrush);

            OverlayWindow.Graphics.DrawText("DrawCircle3D", _font, _redBrush, 650, 40);
            OverlayWindow.Graphics.DrawCircle(700, 120, 35, 2, _redBrush);

            OverlayWindow.Graphics.DrawText("DrawEdge", _font, _redBrush, 800, 40);
            OverlayWindow.Graphics.DrawEdge(800, 70, 50, 100, 10, 2, _redBrush);

            OverlayWindow.Graphics.DrawText("DrawLine", _font, _redBrush, 950, 40);
            OverlayWindow.Graphics.DrawLine(950, 70, 1000, 200, 2, _redBrush);

            //second row
            OverlayWindow.Graphics.DrawText("DrawPlus", _font, _redBrush, 50, 250);
            OverlayWindow.Graphics.DrawPlus(70, 300, 15, 2, _redBrush);

            OverlayWindow.Graphics.DrawText("DrawRectangle", _font, _redBrush, 200, 250);
            OverlayWindow.Graphics.DrawRectangle(200, 300, 50, 100, 2, _redBrush);

            OverlayWindow.Graphics.DrawText("DrawRectangle3D", _font, _redBrush, 350, 250);
            OverlayWindow.Graphics.DrawRectangle3D(350, 320, 50, 100, 10, 2, _redBrush);

            OverlayWindow.Graphics.DrawText("FillCircle", _font, _redBrush, 800, 250);
            OverlayWindow.Graphics.FillCircle(850, 350, 50, _redBrush);

            OverlayWindow.Graphics.DrawText("FillRectangle", _font, _redBrush, 950, 250);
            OverlayWindow.Graphics.FillRectangle(950, 300, 50, 100, _redBrush);

*/

            if (_watch.ElapsedMilliseconds > 1000)
            {
                _watch.Restart();
            }

            OverlayWindow.Graphics.EndScene();
        }

        public override void Dispose()
        {
            OverlayWindow.Dispose();
            base.Dispose();
        }

        private void ClearScreen()
        {
            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();
            OverlayWindow.Graphics.EndScene();
        }
    }
}