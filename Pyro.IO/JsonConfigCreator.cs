using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Pyro.IO;

public static class JsonConfigCreator
{
    private const char OpeningBracket = '{';
    private const char ClosingBracket = '}';
    private const char Quote = '"';
    private const char Colon = ':';
    public static List<StoreAsJson> Stores;
    public static void TraverseAssemblyAndCreateJsonFiles(this Assembly assembly, LocalRoaming roaming)
    {
        var types = assembly.GetTypes();
        Stores = new List<StoreAsJson>();
        foreach (var type in types)
        {
            if (type.IsInterface)
            {
                continue;
            }
            var props = type.GetRuntimeProperties().Cast<MemberInfo>().ToArray();
            var fields = type.GetRuntimeFields().Cast<MemberInfo>();
            var members = props.Concat(fields).ToArray();
            List<StoreJsonPart> parts = new List<StoreJsonPart>();
            for (var i = 0; i < members.Length; i++)
            {
                var prop = members[i];
                var attr = prop.GetCustomAttribute<StoreAsJson>();
                if (attr == null)
                {
                    continue;
                }

                attr.Parent = type;
                Stores.Add(attr);
                object propBaseValue;
                if (prop is FieldInfo info)
                {
                    if (info.FieldType == typeof(string))
                    {
                        propBaseValue = "";
                    }
                    else
                    {
                        propBaseValue = Activator.CreateInstance(info.FieldType);
                    }
                }
                else
                {
                    if ((prop as PropertyInfo).PropertyType == typeof(string))
                    {
                        propBaseValue = "";
                    }
                    else
                    {
                        propBaseValue = Activator.CreateInstance((prop as PropertyInfo).PropertyType);
                    }
                }

                parts.Add(new StoreJsonPart(attr.Name, propBaseValue));
            }

            if (parts.Count == 0)
            {
                continue;
            }
            var name = $"{type.Name}.json";
            if (roaming.Exists(name))
            {
                var e = roaming.ReadFileAs<StoreJsonPart[]>(name);
                if (e.Length < parts.Count)
                {
                    roaming.ModifyFile(name, parts);
                }
                else
                {
                    for (int i = 0; i < e.Length; i++)
                    {
                        var q = e[i];
                        var t = Type.GetType(q.TypeAsString);
                        if (t.IsEnum)
                        {
                            var js = (JsonElement) q.Value;
                            q.StringifiedValue = Enum.GetName(t, js.GetInt32());
                        }
                        else
                        {
                            q.StringifiedValue = q.Value.ToString();
                        }
                    }
                    roaming.ModifyFile(name, e);
                }
            }
            else
            {
                roaming.AddFile(name, parts);
            }
        }
    }

    public static void AssignJsonStoresToStaticInstances(List<StoreAsJson> parts, LocalRoaming roaming, Action<string, string, object> onCompleteEach, Action<string> onFailed)
    {
        foreach (var store in parts)
        {
            AssignSingleJsonStoreToStaticInstance(roaming, onCompleteEach, onFailed, store);
        }
    }

    public static void AssignSingleJsonStoreToStaticInstance(LocalRoaming roaming, Action<string, string, object> onCompleteEach, Action<string> onFailed,
        StoreAsJson store, object valueOverride = null)
    {
        try
        {
            var typeData = roaming.ReadFileAs<StoreJsonPart[]>($"{store.Parent.Name}.json");
            var type = store.Parent;
            var obj = typeData.FirstOrDefault(x => x.Name == store.Name);
            AssignJsonStore(onCompleteEach, store, obj, type, valueOverride);
        }
        catch (IndexOutOfRangeException)
        {
            onFailed?.Invoke($"~[{store.Name}] - Potential Duplicate (Inherited Member)!~");
        }
        catch (Exception e)
        {
            onFailed?.Invoke($"~[{store.Name}, '{store.Parent.Name}'] - {e.Message}!~");
        }
    }

    private static void AssignJsonStore(Action<string, string, object> onCompleteEach, StoreAsJson store, StoreJsonPart obj,
        Type type, object valueOverride = null)
    {
        Type t = Type.GetType(obj.TypeAsString);
        var val = (JsonElement)obj.Value;
        var o = valueOverride ?? val.Deserialize(t!);
        object field = type.GetMember(store.Name)[0];
        if (field is FieldInfo fi)
        {
            fi.SetValue(null, o);
            onCompleteEach?.Invoke(type.Name, fi.Name, o);
        }
        else
        {
            var prop = (field as PropertyInfo);
            prop.SetValue(null, o);
            onCompleteEach?.Invoke(type.Name, prop.Name, o);
        }
    }
}     

[AttributeUsage(AttributeTargets.Property)]
public class StoreAsJson : Attribute
{
    public StoreAsJson([CallerMemberName] string name = "defaultName", Type type = null)
    {
        Name = name;
        _type = type;
    }
    public string Name { get; }
    public Type Parent { get; internal set; }
    public Type VariableType
    {
        get
        {
            if (_type == null)
            {
                if (Parent is null)
                {
                    throw new ArgumentException("Parent type cannot be null!");
                }
                var temp = Parent.GetMember(Name, BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
                if (temp is FieldInfo info)
                {
                    _type = info.FieldType;
                }
                else
                {
                    _type = (temp as PropertyInfo).PropertyType;
                }
            }

            return _type;
        }
    }

    private Type _type;
}

public class StoreJsonPart
{
    public string Name { get; set; }
    public object Value { get; set; }
    public string StringifiedValue { get; set; }
    public string TypeAsString { get; set; }

    public StoreJsonPart(string name, object value)
    {
        Name = name;
        Value = value;
        StringifiedValue = value.ToString();
        TypeAsString = value.GetType().AssemblyQualifiedName;
    }
}