using System;
using ApiCommon.WindowHandling;
using System.Diagnostics;
using System.IO;

namespace WindowServer
{
    public class WindowHandler
    {
        private string                _machineWindowId;
        private string                _ioSimWindowId;
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

        public bool FindWindow(string windowName, bool ioSim)
        {
            Console.WriteLine("Searching window for window: " + windowName);
            var command = $@"xdotool search --screen 0 --onlyvisible --name ""{windowName}""";
            
            _output.WriteLine(command);

            var windowId = _input.ReadLine();
            Console.WriteLine("Found Window: " + windowId);

            if (!string.IsNullOrEmpty(windowId))
            {
                if (!ioSim)
                {
                    _machineWindowId = windowId;
                }
                else
                {
                    _ioSimWindowId = windowId;
                }

                return true;
            }

            return false;
        }

        public bool MoveWindow(Position position, bool ioSim)
        {
            Console.WriteLine("Moving window");

            var windowId = ioSim
                ? _ioSimWindowId
                : _machineWindowId;

            if (string.IsNullOrEmpty(windowId))
                return false;

            var command = $"xdotool windowmove {windowId} {position.X} {position.Y}";
            _output.WriteLine(command);

            Console.WriteLine("Window with ID " + windowId + " moved");

            return true;
        }

        public bool MouseClick(Position position, bool ioSim)
        {
            var windowId = ioSim
                ? _ioSimWindowId
                : _machineWindowId;

            if (string.IsNullOrEmpty(windowId))
                return false;

            var command = $"xdotool windowactivate --sync {windowId} mousemove --window {windowId} {position.X} {position.Y} click 1 sleep 1";
            _output.WriteLine(command);

            return true;
        }

        public bool MouseDown(Position position, bool ioSim)
        {
            var windowId = ioSim
                ? _ioSimWindowId
                : _machineWindowId;

            if (string.IsNullOrEmpty(windowId))
                return false;

            var command = $"xdotool windowactivate --sync {windowId} mousemove --window {windowId} {position.X} {position.Y} mousedown 1 sleep 1";
            _output.WriteLine(command);

            return true;
        }

        public bool MouseUp(Position position, bool ioSim)
        {
            var windowId = ioSim
                ? _ioSimWindowId
                : _machineWindowId;

            if (string.IsNullOrEmpty(windowId))
                return false;

            var command = $"xdotool windowactivate --sync {windowId} mousemove --window {windowId} {position.X} {position.Y} mouseup 1 sleep 1";
            _output.WriteLine(command);

            return true;
        }

        public bool KeyPress(string key, bool ioSim)
        {
            var windowId = ioSim
                ? _ioSimWindowId
                : _machineWindowId;

            if (string.IsNullOrEmpty(windowId))
                return false;

            var command = $"xdotool windowactivate --sync {windowId} key --window {windowId} {key}";
            _output.WriteLine(command);

            return true;
        }

    }
}
