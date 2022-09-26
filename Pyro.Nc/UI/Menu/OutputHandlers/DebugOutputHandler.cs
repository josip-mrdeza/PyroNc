using System;
using System.Reflection;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using TMPro;

namespace Pyro.Nc.UI.Menu.OutputHandlers
{
    public class DebugOutputHandler : OutputHandler
    {
        public TMP_InputField InputField;
        public Type ToolType = typeof(ITool);
        private void Update()
        {
            var id = InputField.text;
            PropertyInfo property;
            if (id.StartsWith("Values::"))
            {
                id = id.Replace("Values::", string.Empty);
                property = typeof(ToolValues).GetProperty(id);
                if (property is not null)
                {
                    ValueText.text = property.GetValue(Globals.Tool.Values).ToString();
                }
            }
            else
            {
                property = ToolType.GetProperty(id);
                if (property is not null)
                {
                    ValueText.text = property.GetValue(Globals.Tool).ToString();
                }
            }
        }
    }
}