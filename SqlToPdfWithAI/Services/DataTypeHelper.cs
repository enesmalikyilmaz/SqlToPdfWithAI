namespace SqlToPdfWithAI.Services
{
    public class DataTypeHelper
    {
        public static bool IsNumeric(object? v)
        {
            if (v is null) return false;
            return v is byte or sbyte or short or ushort or int or uint
                or long or ulong or float or double or decimal;
        }

        public static bool IsDate(object? v)
        {
            if (v is null) return false;
            if (v is DateTime) return true;
            // string tarih gibi gelirse kaba deneme:
            if (v is string s && DateTime.TryParse(s, out _)) return true;
            return false;
        }
    }
}