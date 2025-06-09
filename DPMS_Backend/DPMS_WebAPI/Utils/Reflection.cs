using System.Dynamic;

namespace DPMS_WebAPI.Utils
{
    public static class Reflection
    {
        public static object GetPropValue(this object src, string propName)
        {
            return src.GetType().GetProperty(propName)?.GetValue(src, null);
        }
        public static object CombineObjects(this object item, object add)
        {
            var ret = new ExpandoObject() as IDictionary<string, object>;

            var props = item.GetType().GetProperties();
            foreach (var property in props)
            {
                if (property.CanRead)
                {
                    ret[property.Name] = property.GetValue(item);
                }
            }

            props = add.GetType().GetProperties();
            foreach (var property in props)
            {
                if (property.CanRead)
                {
                    ret[property.Name] = property.GetValue(add);
                }
            }

            return ret;
        }
        public static object AddProp(this object src, object value)
        {
            var dapperRowProperties = src as IDictionary<string, object>;

            IDictionary<string, object> expando = new ExpandoObject();

            if (dapperRowProperties != null)
                foreach (var property in dapperRowProperties)
                    expando.Add(property.Key, property.Value);
            var props = value.GetType().GetProperties();
            foreach (var property in props)
            {
                if (property.CanRead)
                {
                    expando[property.Name] = property.GetValue(value);
                }
            }
            return expando;
        }
        public static ExpandoObject ToExpandoObject(this object value)
        {
            var dapperRowProperties = value as IDictionary<string, object>;

            IDictionary<string, object> expando = new ExpandoObject();

            if (dapperRowProperties != null)
                foreach (var property in dapperRowProperties)
                    expando.Add(property.Key, property.Value);

            return (ExpandoObject)expando;
        }
        public static object ChangeType(this object value, Type conversion)
        {
            var t = conversion;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }
    }

    public static class DapperHelper
    {
        public static T GetPropValue<T>(this object src, string propName)
        {
            var dapperRowProperties = src as IDictionary<string, object>;
            if (dapperRowProperties == null) return default;
            var value = dapperRowProperties[propName];
            if (value == null) return default;
            return (T)value;
        }
    }

}
