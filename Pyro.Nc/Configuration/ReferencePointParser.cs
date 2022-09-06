using System.IO;
using System.Linq;
using Pyro.IO;
using Pyro.Nc.Parsing;
using UnityEngine;

namespace Pyro.Nc.Configuration
{
    public static class ReferencePointParser
    {
        static ReferencePointParser()
        {
            var fullPath = $"{CommandHelper._storage.StorageDirectory.FullName}\\Configuration\\referencePoints.txt";
            referencePointsTxt = File.ReadAllLines(fullPath);
            Init();
        }

        private static string[] referencePointsTxt;
        private static Vector3[] cachedValues = new Vector3[6];
        private static bool _isRuntimeConst;
        private static void Init()
        {
            var valid = referencePointsTxt.Where(l => !l.StartsWith("//"));
            cachedValues = valid.Select(s => s.Split(':')[1]).Select(vc =>
            {
                var noFirst = vc.Remove(0, 1);
                var noSecond = noFirst.Remove(5, 1);
                var values = noSecond.Split(',').Select(value => float.Parse(value)).ToArray();

                return new Vector3(values[0], values[1], values[2]);
            }).ToArray();
        }

        private static Vector3 Refresh(int index)
        {
            Init();
            return cachedValues[index];
        }

        public static bool IsRuntimeConst
        {
            get => _isRuntimeConst;
            set
            {
                bool previous = _isRuntimeConst;
                _isRuntimeConst = value;
                if (previous != _isRuntimeConst)
                {
                    Init();
                }
            }
        }

        public static Vector3 MachineZeroPoint
        {
            get => IsRuntimeConst ? cachedValues[0] : Refresh(0);
            set => cachedValues[0] = value;
        }

        public static Vector3 WorkpieceZeroPoint
        {
            get => IsRuntimeConst ? cachedValues[1] : Refresh(1);
            set => cachedValues[1] = value;
        }

        public static Vector3 TemporaryWorkpiecePoint 
        {
            get => IsRuntimeConst ? cachedValues[2] : Refresh(2);
            set => cachedValues[2] = value;
        }
        public static Vector3 ToolMountReferencePoint 
        { 
            get => IsRuntimeConst ? cachedValues [3] : Refresh(3);
            set => cachedValues[3] = value;
        }
        public static Vector3 ReferencePoint
        {
            get => IsRuntimeConst ? cachedValues [4] : Refresh(4);
            set => cachedValues[4] = value;
        }
        public static Vector3 BeginPoint
        {
            get => IsRuntimeConst ? cachedValues [5] : Refresh(5);
            set => cachedValues[5] = value;
        }
    }
}