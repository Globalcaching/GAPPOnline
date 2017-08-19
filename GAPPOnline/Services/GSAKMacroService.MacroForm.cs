using GAPPOnline.Hubs;
using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public partial class GSAKMacroService
    {
        public class MacroForm
        {
            public string Name { get; set; }
            public string Caption { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }

            public List<MacroFormElement> Elements { get; set; }

            public static MacroForm Parse(Line line, string s)
            {
                var result = new MacroForm();
                result.Elements = new List<MacroFormElement>();
                var lines = s.Split('\n');
                var index = 0;
                object currentObject = null;
                string objectName = null;
                while (index < lines.Length)
                {
                    var parts = lines[index].Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        if (string.Compare(parts[0].Trim(), "Name", true) == 0)
                        {
                            objectName = parts[1].Trim();
                        }
                        else if (string.Compare(parts[0].Trim(), "Type", true) == 0)
                        {
                            var t = parts[1].Trim().ToLower();
                            switch (t)
                            {
                                case "form":
                                    currentObject = result;
                                    break;
                                case "groupbox":
                                    currentObject = new MacroFormGroupbox();
                                    break;
                                case "browser":
                                    currentObject = new MacroFormBrowser();
                                    break;
                                case "button":
                                    currentObject = new MacroFormButton();
                                    break;
                                case "checkbox":
                                    currentObject = new MacroFormCheckbox();
                                    break;
                                case "checkgroup":
                                    currentObject = new MacroFormCheckGroup();
                                    break;
                                case "checklistbox":
                                    currentObject = new MacroFormCheckListbox();
                                    break;
                                case "combobox":
                                    currentObject = new MacroFormCombobox();
                                    break;
                                case "date":
                                    currentObject = new MacroFormDate();
                                    break;
                                case "edit":
                                    currentObject = new MacroFormEdit();
                                    break;
                                case "file":
                                    currentObject = new MacroFormFile();
                                    break;
                                case "folder":
                                    currentObject = new MacroFormFolder();
                                    break;
                                case "image":
                                    currentObject = new MacroFormImage();
                                    break;
                                case "label":
                                    currentObject = new MacroFormLabel();
                                    break;
                                case "memo":
                                    currentObject = new MacroFormMemo();
                                    break;
                                case "radiobutton":
                                    currentObject = new MacroFormRadiobutton();
                                    break;
                                default:
                                    line.SyntaxError("Unknown element in form");
                                    break;
                            }
                            if (currentObject is MacroFormElement)
                            {
                                (currentObject as MacroFormElement).ControlType = currentObject.GetType().Name;
                                (currentObject as MacroFormElement).Name = objectName;
                                result.Elements.Add(currentObject as MacroFormElement);

                                //No variable for: Form, Group, Image, Browser,and Label
                                if (currentObject is MacroForm
                                    || currentObject is MacroFormGroupbox
                                    || currentObject is MacroFormImage
                                    || currentObject is MacroFormBrowser
                                    || currentObject is MacroFormLabel
                                    )
                                {
                                    //no variable
                                }
                                else
                                {
                                    var varName = $"${objectName}";
                                    Variable v;
                                    if (!line.Macro.Variables.TryGetValue(varName, out v))
                                    {
                                        v = new Variable(line.Macro, varName);
                                        if (currentObject is MacroFormCheckbox || currentObject is MacroFormRadiobutton)
                                        {
                                            v.Type = typeof(bool);
                                        }
                                        else if (currentObject is MacroFormDate)
                                        {
                                            v.Type = typeof(DateTime);
                                        }
                                        else
                                        {
                                            v.Type = typeof(string);
                                        }
                                        line.Macro.Variables.Add(objectName, v);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var propName = parts[0].Trim();
                            var p = currentObject?.GetType().GetProperty(propName);
                            if (p != null)
                            {
                                var v = Convert.ChangeType(parts[1].Trim(), p.PropertyType);
                                p.SetValue(currentObject, v);
                            }
                        }
                    }
                    index++;
                }
                return result;
            }
        }
    }
}
