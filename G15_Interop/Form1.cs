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
            Display.ButtonsChanged += OnButtonsChanged;
            Display.Clear();

            fileSystemWatcher1.Path = "C:\\G15";

            OnButtonsChanged(this, new ButtonsChangedEventArgs(Logitech_LCD.Buttons.MonoButton0,false));    
      
            //var npServer = new G15PipeServer(Display);
            //npServer.Start();
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            var index = 0;

            switch(e.Button)
            {
                case Logitech_LCD.Buttons.MonoButton0:
                    index = 0;
                    break;

                case Logitech_LCD.Buttons.MonoButton1:
                    index = 1;
                    break;

                case Logitech_LCD.Buttons.MonoButton2:
                    index = 2;
                    break;

                case Logitech_LCD.Buttons.MonoButton3:
                    index = 3;
                    break;

            }

            var message = $"Button {index} {(e.IsPressed ? "Pressed" : "Released")}";
            LogMessage(message);

            File.WriteAllLines($"{fileSystemWatcher1.Path}\\Buttons.txt", Display.ButtonStates.Select(x => x.Value.ToString()));
        }

        private void OnScreenUpdated(object sender, ScreenUpdatedEventArgs e)
        {
            pictureBox1.Image = e.Preview;

            LogMessage(e.UpdateMessage);
        }

        private void LogMessage(string updateMessage)
        {
            if (log.Count >= 20)
                log.Dequeue();
            log.Enqueue(updateMessage);
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
