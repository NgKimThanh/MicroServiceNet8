namespace MicroServiceNet8.Helper.Middleware
{
    public class HelperCrossSiteScriptingValidation
    {
        private static readonly string[] StartingLabels = { "{{", "<script>" };
        private static readonly string[] EndingLabels = { "}}", "</script>" };

        public static bool IsDangerousString(string s, out string label)
        {
            label = string.Empty;
            for (var i = 0; ;)
            {
                if (i == StartingLabels.Length) return false;

                label = StartingLabels[i];
                if (ContainsOrdinalIgnoreCase(s, label))
                    if (ContainsOrdinalIgnoreCase(s, EndingLabels[i]))
                    {
                        label = label.Replace("<", string.Empty).Replace(">", string.Empty);
                        return true;
                    }
                i++;
            }
        }

        public static bool ContainsDangerousString(object obj, out string label)
        {
            label = string.Empty;
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(string))
                {
                    var value = prop.GetValue(obj)?.ToString();
                    if (!string.IsNullOrEmpty(value) && IsDangerousString(value, out label))
                    {
                        return true;
                    }
                }
                else if (prop.PropertyType.Assembly.GetName().Name != "mscorlib")
                {
                    return ContainsDangerousString(prop.GetValue(obj), out label);
                }
            }
            return false;
        }

        private static bool ContainsOrdinalIgnoreCase(string currentString, string containString)
        {
            return currentString.IndexOf(containString, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
