using System;
using UnityEngine;

namespace Pyro.Nc.UI.Cycles;

public class CycleTester : MonoBehaviour
{
    private void Start()
    {
        gameObject.OpenCycleCreatorOnGameObject(CycleCreator.CycleType.Cycle81);
    }
}