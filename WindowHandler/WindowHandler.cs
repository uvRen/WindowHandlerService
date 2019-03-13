using ApiCommon.WindowHandling;
using System.Diagnostics;
using System.IO;

namespace WindowServer
{
    public class WindowHandler
    {
        private string               _windowId;
        private readonly StreamWriter _output;
        private readonly StreamReader _input;

        public WindowHandler()
        {
            var info = new ProcessStartInfo
            {
                FileName               = "/bin/bash",
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                UseShellExecute        = false,
            };

            var process = new Process { StartInfo = info };
            process.Start();

            _input   = process.StandardOutput;
            _output  = process.StandardInput;

            _output.WriteLine("export DISPLAY=:0");
            _output.WriteLine("export XAUTHORITY=/home/ubuntu/.Xauthority");
        }

        public void FindWindow(string windowName)
        {
            var command = $"xdotool search --screen 0 --onlyvisible --class {windowName}";
            _output.WriteLine(command);

            var windowId = _input.ReadLine();

            if (!string.IsNullOrEmpty(windowId))
            {
                _windowId = windowId;
            }
        }

        public void MoveWindow(Position position)
        {
            if (_windowId == null)
                return;

            var command = $"xdotool windowmove {_windowId} {position.X} {position.Y}";
            _output.WriteLine(command);
        }

        public void MouseClick(Position position)
        {
            if (_windowId == null)
                return;

            var command = $"xdotool mousemove --window {_windowId} {position.X} {position.Y} click 1";
            _output.WriteLine(command);
        }

        public void KeyPress(string key)
        {
            if (_windowId == null)
                return;

            var command = $"xdotool key --window {_windowId} {key}";
            _output.WriteLine(command);
        }

    }
}
