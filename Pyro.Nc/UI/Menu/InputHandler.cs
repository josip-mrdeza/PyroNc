using System;
using System.Linq;
using System.Reflection;
using Pyro.Nc.Configuration;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pyro.Nc.UI.Menu
{
    public class InputHandler : InitializerRoot
    {
        public string PropertyId;
        public string Description;
        public string DefaultValue;
        public TextMeshProUGUI DescriptionComponent;
        public TMP_InputField InputFieldComponent;
        public TextMeshProUGUI InputFieldPlaceHolderText;
        public Type ToolValuesType;
        public PropertyInfo ToolValuesIdTypeInfo;
        public Type ToolValuesIdType;
        public ITool Tool;
        public override void Initialize()
        {
            Tool = Globals.Tool;
            DescriptionComponent ??= GetComponentInChildren<TextMeshProUGUI>();
            InputFieldComponent ??= GetComponentInChildren<TMP_InputField>();
            DescriptionComponent.text = Description ??= "Not-Defined";
            InputFieldComponent.text = DefaultValue;
            CheckForInvalidId();
            InitTypeInfos();
            DefaultValue = (((Limiter) ToolValuesIdTypeInfo.GetValue(Tool.Values)).ToString());
            InputFieldPlaceHolderText.text = DefaultValue;
            InputFieldComponent.onEndEdit.AddListener(OnEndEdit);
        }

        public virtual void OnEndEdit(string s)
        {
            if (CheckForInvalidTool()) return;
            if (string.IsNullOrEmpty(s)) return;
            if (ToolValuesIdTypeInfo.CanWrite)
            {
                if (ToolValuesIdType == typeof(float))
                {
                    ToolValuesIdTypeInfo.SetValue(Tool.Values, float.Parse(s)); 
                }
            }
            else
            {
                Globals.Console.PushText($"Could not write to property: {ToolValuesIdTypeInfo.Name} in ToolValues.\n" +
                                         $"    --Value: {s}", LogType.Warning);
            }
        }

        public void InitTypeInfos()
        {
            ToolValuesType = typeof(ToolValues);
            ToolValuesIdTypeInfo = ToolValuesType.GetProperties()
                                                 .Single(x => x.Name == PropertyId);
            ToolValuesIdType = ToolValuesIdTypeInfo.PropertyType;
        }

        protected void CheckForInvalidId()
        {
            if (string.IsNullOrEmpty(PropertyId))
            {
                Globals.Console.PushText("Could not complete InputHandler's Start():\n" +
                                         "    --Reason: PropertyId is null or empty!", LogType.Error);
            }
        }
        
        protected bool CheckForInvalidTool()
        {
            if (Tool is null)
            {
                Globals.Console.PushText($"Could not write to property: {ToolValuesIdTypeInfo.Name} in ToolValues.\n" +
                                         $"    --Reason: Tool object is null!", LogType.Error);

                return true;
            }

            return false;
        }
    }
}