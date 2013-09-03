using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeRulerLibrary;
using Microsoft.Xna.Framework;

namespace Amphibian.Debug
{
    public static class Performance
    {
        private static TimeRuler _currentRuler;
        private static DebugManager _debugManager;
        private static DebugCommandUI _debugCommandUI;
        private static FpsCounter _fpsCounter;
        private static MemoryTracker _memTracker;
        private static TimeHistory _timeHistory;
        private static bool _firstFrame = true;

        private static DebugOutputState _state = DebugOutputState.Basic;

        public static TimeRuler TimeRuler
        {
            get { return _currentRuler; }
        }

        public static bool CommandUIOpen
        {
            get { return _debugCommandUI.Focused; }
        }

        public static void Initialize (Game g)
        {
            _debugManager = new DebugManager(g);
            g.Components.Add(_debugManager);

            _debugCommandUI = new DebugCommandUI(g);
            _debugCommandUI.DrawOrder = int.MaxValue;
            g.Components.Add(_debugCommandUI);

            _fpsCounter = new FpsCounter(g);
            g.Components.Add(_fpsCounter);

            _memTracker = new MemoryTracker(g);
            g.Components.Add(_memTracker);

            _currentRuler = new TimeRuler(g);
            g.Components.Add(_currentRuler);

            _timeHistory = new TimeHistory(g, _currentRuler);
            g.Components.Add(_timeHistory);
        }

        public static void StartFrame ()
        {
            if (_firstFrame) {
                _firstFrame = false;
                _debugCommandUI.ExecuteCommand("tr on log:on");
                _debugCommandUI.ExecuteCommand("fps on");
                _debugCommandUI.ExecuteCommand("memory on");
            }
            _currentRuler.StartFrame();
            _currentRuler.Update(null);
        }

        public static void AdvanceOutputState ()
        {
            switch (_state) {
                case DebugOutputState.None:
                    _debugCommandUI.ExecuteCommand("tr on log:on");
                    _debugCommandUI.ExecuteCommand("fps on");
                    _debugCommandUI.ExecuteCommand("memory on");

                    _state = DebugOutputState.Basic;
                    break;

                case DebugOutputState.Basic:
                    _debugCommandUI.ExecuteCommand("fps off");
                    _debugCommandUI.ExecuteCommand("memory off");
                    _debugCommandUI.ExecuteCommand("th on");

                    _state = DebugOutputState.TimeHistory;
                    break;

                case DebugOutputState.TimeHistory:
                    _debugCommandUI.ExecuteCommand("tr off log:off");
                    _debugCommandUI.ExecuteCommand("fps off");
                    _debugCommandUI.ExecuteCommand("memory off");
                    _debugCommandUI.ExecuteCommand("th off");

                    _state = DebugOutputState.None;
                    break;
            }
        }
    }

    public enum DebugOutputState
    {
        None,
        Basic,
        TimeHistory,
    }

    public struct PerformanceRuler : IDisposable
    {
        private int _index;
        private string _name;

        public PerformanceRuler (int index, string name, Color c)
        {
            _index = index;
            _name = name;
            Performance.TimeRuler.BeginMark(_index, _name, c);
        }

        public PerformanceRuler (string name, Color c)
            : this(0, name, c)
        {
        }

        public void Dispose ()
        {
            Performance.TimeRuler.EndMark(_index, _name);
        }
    }
}
