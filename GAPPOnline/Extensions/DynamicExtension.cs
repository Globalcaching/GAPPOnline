using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System;

public static class DynamicExtension
{
    /// <summary>
    /// Try to cast param value to an ExpandoObject. If this is not possible, create an new ExpandoObject will all properties of param value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static dynamic ToDynamic(this object value)
    {
        IDictionary<string, object> result = null;
        if (value is ExpandoObject)
        {
            result = (ExpandoObject)value;
        }
        else
        {
            result = new ExpandoObject();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                result.Add(property.Name, property.GetValue(value));
            }
        }
        return result as ExpandoObject;
    }

    public static dynamic AddDynamic(this object value, object other)
    {
        IDictionary<string, object> result = value.ToDynamic();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(other.GetType()))
        {
            if (!result.ContainsKey(property.Name))
            {
                result.Add(property.Name, property.GetValue(other));
            }
        }
        return result as ExpandoObject;
    }

    public static bool PropertyExist(dynamic obj, string name)
    {
        if (obj == null)
            return false;

        if (obj is ExpandoObject)
            return ((IDictionary<string, object>)obj).ContainsKey(name);

        return obj.GetType().GetProperty(name) != null;
    }

    public static string GetSavePropertyValue(dynamic obj, string name)
    {
        if (obj == null)
            return "";

        if (obj is ExpandoObject)
            return (((IDictionary<string, object>)obj)[name]?.ToString() ?? "");

        var prop = obj.GetType().GetProperty(name);
        if (prop == null)
            return "";

        return prop.GetValue(obj)?.ToString() ?? "";
    }

}



