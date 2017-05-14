using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Overlay.NET;
using Overlay.NET.Common;
using Process.NET;
using Process.NET.Assembly.CallingConventions;
using Process.NET.Memory;

namespace EDMCOverlay
{
    public class OverlayRenderer
    {
        public const string EDProgramName = "EliteDangerous64";
        public const int FPS = 30;

        private OverlayController _controller;
        private ProcessSharp _processSharp;
        private System.Diagnostics.Process _game;

        private bool run = true;

        private Thread renderThread;

        public bool Attached
        {
            get { return _game != null && !_game.HasExited; }
        }

        public void Start(OverlayJsonServer service)
        {

            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(EDProgramName).FirstOrDefault();
            if (process == null)
            {
                throw new EntryPointNotFoundException(EDProgramName);
            }
            _game = process;

            _controller = new OverlayController();
            _controller.SetFrameRate(FPS);
            _controller.SetGraphics(service.Graphics);

            _processSharp = new ProcessSharp(process, MemoryType.Remote);
            _controller.Initialize(_processSharp.WindowFactory.MainWindow);
            _controller.Enable();
            _processSharp.ProcessExited += (sender, args) =>
            {
                this.run = false;
            };

            renderThread = new Thread(new ThreadStart(Update));
            renderThread.Start();
        }

        private void Update()
        {
            while (this.run)
            {
                _controller.Update();
            }
        }
    }
}