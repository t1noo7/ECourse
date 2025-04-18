using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Reflection;

namespace Demo.Web.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Lấy mô tả từ [Description] cho một giá trị enum cụ thể
        /// </summary>
        public static string GetEnumDescriptionValue<T>(T item) where T : struct, Enum
        {
            var type = typeof(T);
            var field = type.GetField(item.ToString());

            if (field == null) return item.ToString();

            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? item.ToString();
        }

        /// <summary>
        /// Trả về danh sách (giá trị, mô tả) của toàn bộ enum
        /// </summary>
        public static IEnumerable<(T value, string text)> GetEnumDescriptionList<T>() where T : struct, Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>()
                .Select(e => (e, GetEnumDescriptionValue(e)));
        }

        /// <summary>
        /// Trả về danh sách SelectListItem từ enum (dùng cho dropdown)
        /// </summary>
        public static IEnumerable<SelectListItem> EnumToListItems<T>() where T : struct, Enum
        {
            return GetEnumDescriptionList<T>()
                .Select(x => new SelectListItem
                {
                    Value = x.value.ToString(),
                    Text = x.text
                });
        }
    }
}
