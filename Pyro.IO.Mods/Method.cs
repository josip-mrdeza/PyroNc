using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TrCore;

namespace Pyro.IO.Mods
{
    public class Method : Method<object, object>
    {
        public Method(object target, MethodInfo methodData) : base(target, methodData)
        {
        }
        
        public Method(object target, MethodBase method) : base(target, method)
        {
        }

        public Method(object target, Func<object, object> dg) : base(target, dg.Method)
        {
        }
        
        public Method(object target, IEnumerable<MethodInfo> infos) : base(target, infos)
        {
        }
        
        public Method(object target, Delegate dg) : base(target, dg)
        {
        }
    }
    /// <typeparam name="T">The Target Type.</typeparam>
    /// <typeparam name="TR">The Return Type.</typeparam>
    public class Method<T, TR>
    {
        public T Target;
        public Delegate MethodData;
        public string Name;

        public Method(T target, MethodInfo methodData)
        {
            Target = target;
            MethodData = Delegate.CreateDelegate(typeof(Func<T, TR>), Target, methodData);
            Name = methodData.Name;
        }
        
        public Method(T target, MethodBase method)
        {
            Target = target;
            MethodData = Delegate.CreateDelegate(typeof(Func<T, TR>), Target, (MethodInfo) method);

            Name = method.Name;
        }
        
        public Method(T target, IEnumerable<MethodInfo> methods)
        {
            Target = target;
            MethodData = Delegate.Combine(methods.ToArray().Select(x => Delegate.CreateDelegate(typeof(Func<T, TR>), Target, x)).ToArray());
            Name = MethodData.Method.Name;
        }
        
        public Method(T target, Delegate dg)
        {
            Target = target;
            MethodData = dg;
            Name = MethodData.Method.Name;
        }
        
        public ReturnValue<TR> Call()
        {
            return new ReturnValue<TR>((TR) MethodData.DynamicInvoke(EmptyArray));
        }
        
        public ReturnValue<TR> Call(params object[] parameters)
        {
            return new ReturnValue<TR>((TR) MethodData.DynamicInvoke(parameters));
        }

        public Method CastDown()
        {
            if ((CurrentType ??= GetType()) == typeof(Method))
            {
                return this as Method;
            }
            
            return new Method(Target, MethodData);
        }

        public Method<TN, TRN> CastUp<TN, TRN>()
        {
            if ((CurrentType ??= GetType()) == typeof(Method<TN, TRN>))
            {
                return this as Method<TN, TRN>;
            }

            return new Method<TN, TRN>((TN) (object) Target, MethodData);
        }

        public void Prefix(Method<T, TR> prefixMethod)
        {
            var dg = Delegate.Combine(prefixMethod.MethodData, MethodData);
            MethodData = dg;
        }
        
        public void Postfix(Method<T, TR> postfixMethod)
        {
            var dg = Delegate.Combine(MethodData, postfixMethod.MethodData);
            MethodData = dg;
        }

        protected object[] EmptyArray = new object[0];
        private Type CurrentType;
        private MethodInfo _last;
        public class ReturnValue<TR>
        {
            public TR Value;

            public ReturnValue(TR value)
            {
                Value = value;
            }
            
            public static implicit operator TR(ReturnValue<TR> storage)
            {
                return storage.Value;
            }
        }
    }

    public class MethodNonBox<T, TR> : Method<T, TR>
        where T : struct where TR : struct
    {
        public MethodNonBox(T target, MethodInfo methodData) : base(target, methodData)
        {
        }
    }
}