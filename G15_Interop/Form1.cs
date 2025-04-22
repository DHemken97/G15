using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Windows.Forms;
namespace G15_Interop
{
    public partial class Form1 : Form
    {
        public IG15_Display Display { get; set; }
        private Queue<string> log;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG 
            Display = new G15_Display(true);
#else
            Display = new G15_Display();
#endif

            log = new Queue<string>(20);
            Display.ScreenUpdated += OnScreenUpdated;
            Display.Clear();

            fileSystemWatcher1.Path = "C:\\G15";
      
            var npServer = new G15PipeServer(Display);
            npServer.Start();
        }


        private void OnScreenUpdated(object sender, ScreenUpdatedEventArgs e)
        {
            pictureBox1.Image = e.Preview;
            if (log.Count >= 20)
                log.Dequeue();
            log.Enqueue(e.UpdateMessage);
            richTextBox1.Text = string.Join("\r\n", log.Reverse());
        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                if (e.Name == "background.bmp")
                    Display.SetBackground((Bitmap)Bitmap.FromFile(e.FullPath));
                if (e.Name == "lines.txt")
                {
                    var lines = File.ReadAllLines(e.FullPath);
                    Display.SetLine(0, lines[0]);
                    Display.SetLine(1, lines[1]);
                    Display.SetLine(2, lines[2]);
                    Display.SetLine(3, lines[3]);
                }
            }
            catch { }


        }
    }
}
