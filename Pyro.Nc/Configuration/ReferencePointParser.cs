using System;
using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.Parsing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public class ReferencePointParser : InitializerRoot
    {
        public static ReferencePointParser Instance;
        public override void Initialize()
        {
            Globals.ReferencePointParser = this;
            Instance = this;
            PyroConsoleView.PushTextStatic("Starting ReferencePointParser in path:", Globals.Roaming?.Site);
            referencePointsTxt = LocalRoaming.OpenOrCreate("PyroNc\\Configuration").ReadFileAsText("referencePoints.txt").Split('\n');
            var valid = referencePointsTxt.Where(l => !l.StartsWith("//")).ToArray();
            PyroConsoleView.PushTextStatic(new string[]{""}.Concat(valid).ToArray());
            if (valid.Length == 0)
            {
                return;
            }
            _cachedValues = valid.Select(s => s.Split(':')[1])
                                 .Select(vc =>
                                 {
                                     var noFirst = vc.Remove(0, 1);
                                     var noSecond = noFirst.Remove(noFirst.LastIndexOf(")", StringComparison.InvariantCulture), 1);
                                     var values = noSecond.Split(',')
                                                          .Select(float.Parse)
                                                          .ToArray();

                                     return new Vector3(values[0], values[1], values[2]);
                                 }).ToArray();
            PyroConsoleView.PushTextStatic("Cached vector3 values!");
        }

        private string[] referencePointsTxt;
        [SerializeField]
        private Vector3[] _cachedValues = new Vector3[6];
        [StoreAsJson]
        public static bool _isRuntimeConst { get; set; }

        private void InitPerIndex(int index)
        {
            if (_cachedValues == null || _cachedValues.Length == 0 || referencePointsTxt == null || referencePointsTxt.Length == 0)
            {
                Initialize();
            }
            var valid = referencePointsTxt.Where(l => !l.StartsWith("//"));
            _cachedValues[index] = valid.SkipWhile(x => x.StartsWith("//"))
                                        .Skip(index)
                                        .First()
                                        .Mutate(z => z.Substring(3).Replace(")", ""))
                                        .Split(',')
                                        .Select(float.Parse)
                                        .ToArray()
                                        .Mutate(vec => new Vector3(vec[0], vec[1], vec[2]));
        }

        private Vector3 Refresh(int index)
        {
            InitPerIndex(index);
            return _cachedValues[index];
        }

        public bool IsRuntimeConst
        {
            get => _isRuntimeConst;
            set
            {
                bool previous = _isRuntimeConst;
                _isRuntimeConst = value;
                if (previous != _isRuntimeConst)
                {
                    Initialize();
                }
            }
        }

        public Vector3 MachineZeroPoint
        {
            get => IsRuntimeConst ? _cachedValues[0] : Refresh(0);
            set => _cachedValues[0] = value;
        }
        public Vector3 WorkpieceZeroPoint
        {
            get => IsRuntimeConst ? _cachedValues[1] : Refresh(1);
            set => _cachedValues[1] = value;
        }
        public Vector3 TemporaryWorkpiecePoint 
        {
            get => IsRuntimeConst ? _cachedValues[2] : Refresh(2);
            set => _cachedValues[2] = value;
        }
        public Vector3 ToolMountReferencePoint 
        { 
            get => IsRuntimeConst ? _cachedValues [3] : Refresh(3);
            set => _cachedValues[3] = value;
        }
        public Vector3 ReferencePoint
        {
            get => IsRuntimeConst ? _cachedValues [4] : Refresh(4);
            set => _cachedValues[4] = value;
        }
        public Vector3 BeginPoint
        {
            get => IsRuntimeConst ? _cachedValues [5] : Refresh(5);
            set => _cachedValues[5] = value;
        }
    }
}