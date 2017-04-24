using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System;

namespace GAPPOnline.DynamicExtensions
{
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
    }
}


