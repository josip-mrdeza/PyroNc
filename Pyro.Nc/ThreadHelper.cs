using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pyro.Threading;
using UnityEngine;

namespace Pyro.Nc;

public static class ThreadMTHelper
{
    public static ThreadTaskQueue ThreadQueue { get; internal set; }
    public static bool IsMainThread => ThreadQueue.Name == Thread.CurrentThread.ManagedThreadId;

    private static TransformMT _createTMT(GameObject go)
    {
        var tr = go.transform;
        TransformMT t = new TransformMT(tr);

        return t;
    }
    public static async Task<TransformMT> CreateThreadedTransform(this GameObject go)
    {
        var tr = await ThreadQueue.Run(_createTMT, go);

        return tr;
    }

    public class TransformMT
    {
        public TransformMT(Transform tr)
        {
            ParentTransform = tr;
        }

        public Transform ParentTransform { get; }

        public ValueTask<Vector3> Position
        {
            get
            {
                return GetPosition();
            }
            set
            {
                SetPosition(value.Result).GetAwaiter().GetResult();
            }
        }

        public async ValueTask SetPosition(Vector3 v)
        {
            if (IsMainThread)
            {
                SetPositionMainThread(v);

                return;
            }

            await ThreadQueue.Run(SetPositionMainThread, v);
        }
        public ValueTask<Vector3> LocalPosition
        {
            get
            {
                return GetLocalPosition();
            }
            set
            {
                SetLocalPosition(value.Result).GetAwaiter().GetResult();
            }
        }
        public async ValueTask SetLocalPosition(Vector3 v)
        {
            if (IsMainThread)
            {
                SetLocalPositionMainThread(v);

                return;
            }

            await ThreadQueue.Run(SetLocalPositionMainThread, v);
        }
        public async ValueTask<Vector3> GetLocalPosition()
        {
            if (IsMainThread)
            {
                return GetLocalPositionMainThread();
            }

            var vt = ThreadQueue.Run(GetLocalPositionMainThread);

            return await vt;
        }

        public async ValueTask<Vector3> GetPosition()
        {
            if (IsMainThread)
            {
                return GetPositionMainThread();
            }
            
            var vt = ThreadQueue.Run(GetPositionMainThread);

            return await vt;
        }

        private Vector3 GetPositionMainThread()
        {
            return ParentTransform.position;
        }

        private void SetPositionMainThread(Vector3 v)
        {
            ParentTransform.position = v;
        }

        private Vector3 GetLocalPositionMainThread()
        {
            var v3 = ParentTransform.localPosition;

            return v3;
        }

        private void SetLocalPositionMainThread(Vector3 v)
        {
            ParentTransform.localPosition = v;
        }
    }
}