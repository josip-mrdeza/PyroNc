using System;
using UnityEngine;

namespace Pyro.Nc.UI;

public class BoundingBoxFixer : MonoBehaviour
{
    private void Start()
    {
        var tr = transform;
        var position = tr.position;
        position = new Vector3(0, position.y, position.z);
        tr.position = position;
    }
}