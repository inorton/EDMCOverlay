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

        public EDGlassForm Glass { get; private set; }

        private bool run = true;

        private Thread renderThread;

        public OverlayController Controller
        {
            get
            {
                return _controller;
            }
        }

        public bool Attached
        {
            get { return _game != null && !_game.HasExited; }
        }

        public void Start(OverlayJsonServer service)
        {

            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(EDProgramName).FirstOrDefault();
            if (process == null)
            {
                Console.WriteLine("ED not running");
                System.Environment.Exit(0);
            }
            _game = process;

            //_controller = new OverlayController();
            //_controller.SetFrameRate(FPS);
            //_controller.SetGraphics(service.Graphics);
            
            Glass = new EDGlassForm(process);
            
            /*
            _controller.Initialize(_processSharp.WindowFactory.MainWindow);
            _controller.Enable();
            _processSharp.ProcessExited += (sender, args) =>
            {
                this.run = false;
                Environment.Exit(0);
            };

            renderThread = new Thread(new ThreadStart(Update));
            renderThread.Start();
            */
        }

        private void _processSharp_ProcessExited(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
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