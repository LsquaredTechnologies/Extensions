namespace Lsquared
{
    public static class StringExtensions
    {
        public static string Right(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(value.Length - length) : value;
        }

        public static string Left(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(0, length) : value;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
        
        public static string Join(this string[] array, string separator)
        {
            return string.Join(separator, array);
        }

#if NET40
        public static SecureString ToSecureString(this String str)
        {
            var secureString = new SecureString();
            foreach (Char c in str)
                secureString.AppendChar(c);

            return secureString;
        }
#endif
    }
}
