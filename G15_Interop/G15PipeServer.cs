using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace G15_Interop
{
    public class G15PipeServer
    {
        private const int PollDelay = 100; // ms
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private IG15_Display display;

        public G15PipeServer(IG15_Display display)
        {
            this.display = display;
        }

        public void Start()
        {
            Task.Run(() => StartPipe("G15_Buttons", HandleButtonPipe, _cts.Token));
            Task.Run(() => StartPipe("G15_Text", HandleTextPipe, _cts.Token));
            Task.Run(() => StartPipe("G15_Background", HandleBackgroundPipe, _cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task StartPipe(string pipeName, Func<NamedPipeServerStream, CancellationToken, Task> handler, CancellationToken token)
        {
            var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 16, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine($"[{pipeName}] Waiting for connection...");

                await pipe.WaitForConnectionAsync(token);
                Console.WriteLine($"[{pipeName}] Client connected.");

                try
                {
                    await handler(pipe, token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{pipeName}] Error: {ex.Message}");
                }
            }
           // pipe.Dispose();

        }

        private async Task HandleButtonPipe(NamedPipeServerStream pipe, CancellationToken token)
        {
            var reader = new StreamReader(pipe, Encoding.UTF8);
            var writer = new StreamWriter(pipe, Encoding.UTF8) { AutoFlush = true };

            while (pipe.IsConnected && !token.IsCancellationRequested)
            {
                string request = await reader.ReadLineAsync();
                if (request == "GET")
                {
                    string buttonState = GetButtonState(); // e.g., "G1=pressed\nG2=released\n"
                    await writer.WriteLineAsync(buttonState);
                }

                await Task.Delay(PollDelay, token); // Poll delay
            }
            reader.Dispose();
            writer.Dispose();
        }

        private async Task HandleTextPipe(NamedPipeServerStream pipe, CancellationToken token)
        {
             var reader = new StreamReader(pipe, Encoding.UTF8);

            while (pipe.IsConnected && !token.IsCancellationRequested)
            {
                string text = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine($"[TEXT] Set: {text}");
                    SetDisplayText(text);
                }
                await Task.Delay(PollDelay, token);
            }
            reader.Dispose();
        }

        private async Task HandleBackgroundPipe(NamedPipeServerStream pipe, CancellationToken token)
        {
            var ms = new MemoryStream();

            byte[] buffer = new byte[4096];
            int bytesRead;

            // Keep reading until pipe disconnects or no more data
            while ((bytesRead = await pipe.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                ms.Write(buffer, 0, bytesRead);
            }

            Console.WriteLine("[BG] Received background image data");
            ms.Seek(0, SeekOrigin.Begin);
            SetDisplayBackground(ms.ToArray());
            ms.Dispose();
        }

        // Dummy implementations — replace with your real display logic
        private string GetButtonState() => "G1=pressed\nG2=released";
        private void SetDisplayText(string text) => Console.WriteLine($"[DISPLAY] Text: {text}");
        private void SetDisplayBackground(byte[] data) => Console.WriteLine($"[DISPLAY] BG: {data.Length} bytes");
    }
}