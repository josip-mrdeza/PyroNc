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

        private static readonly string[] referencePointsTxt;
        private static Vector3[] _cachedValues = new Vector3[6];
        private static bool _isRuntimeConst;
        private static void Init()
        {
            var valid = referencePointsTxt.Where(l => !l.StartsWith("//"));
            _cachedValues = valid.Select(s => s.Split(':')[1])
                                 .Select(vc =>
            {
                var noFirst = vc.Remove(0, 1);
                var noSecond = noFirst.Remove(5, 1);
                var values = noSecond.Split(',')
                                     .Select(float.Parse)
                                     .ToArray();

                return new Vector3(values[0], values[1], values[2]);
            }).ToArray();
        }
        
        private static void InitPerIndex(int index)
        {
            var valid = referencePointsTxt.Where(l => !l.StartsWith("//"));
            _cachedValues[index] = valid.SkipWhile(x => x.StartsWith("//"))
                                        .Skip(index)
                                        .First()
                                        .Mutate(z => z.Substring(3, z.Length - 1))
                                        .Split(',')
                                        .Select(float.Parse)
                                        .ToArray()
                                        .Mutate(vec => new Vector3(vec[0], vec[1], vec[2]));
        }

        private static Vector3 Refresh(int index)
        {
            InitPerIndex(index);
            return _cachedValues[index];
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
            get => IsRuntimeConst ? _cachedValues[0] : Refresh(0);
            set => _cachedValues[0] = value;
        }
        public static Vector3 WorkpieceZeroPoint
        {
            get => IsRuntimeConst ? _cachedValues[1] : Refresh(1);
            set => _cachedValues[1] = value;
        }
        public static Vector3 TemporaryWorkpiecePoint 
        {
            get => IsRuntimeConst ? _cachedValues[2] : Refresh(2);
            set => _cachedValues[2] = value;
        }
        public static Vector3 ToolMountReferencePoint 
        { 
            get => IsRuntimeConst ? _cachedValues [3] : Refresh(3);
            set => _cachedValues[3] = value;
        }
        public static Vector3 ReferencePoint
        {
            get => IsRuntimeConst ? _cachedValues [4] : Refresh(4);
            set => _cachedValues[4] = value;
        }
        public static Vector3 BeginPoint
        {
            get => IsRuntimeConst ? _cachedValues [5] : Refresh(5);
            set => _cachedValues[5] = value;
        }
    }
}