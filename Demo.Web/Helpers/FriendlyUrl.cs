namespace Demo.Web.Helpers
{
    /// <summary>
    /// Làm các url để seo tốt hơn
    /// </summary>
    public static class FriendlyUrl
    {
        public static string CoursesListFrUrl = "/dach-sach-khoa-hoc";
        public static string CourseDetailFrm = "/thong-tin-khoa-hoc/{url}";
        public static string EventsListFrUrl = "/su-kien-sap-toi";
        public static string MyCoursesFrUrl = "/khoa-hoc-cua-toi";
        public static string BookShopFrUrl = "/cua-hang-sach";
        public static string MyOrdersFrUrl = "/don-hang-cua-toi";
        public static string MyProfileFrUrl = "/thong-tin-cua-toi";
        public static string CourseShopFrUrl = "/mua-khoa-hoc";
        public static string NewsFrUrl = "/tin-tuc/{url}";
        public static string NewsListFrUrl = "/dach-sach-tin";
        public static string AboutUsFrUrl = "/ve-chung-toi";
        public static string MyLessonFrUrl = "/lop-hoc-cua-toi";

        public static string Login
        {
            get { return "/account/login"; }
        }

        public static string Home
        {
            get { return "/home/index"; }
        }

        public static string CourseList()
        {
            return CoursesListFrUrl;
        }
        public static string CourseDetail(string url)
        {
            return TrimStar(CourseDetailFrm).Replace("{url}", url);
        }
        public static string EventList()
        {
            return EventsListFrUrl;
        }
        public static string MyCourses()
        {
            return MyCoursesFrUrl;
        }
        public static string MyOrders()
        {
            return MyOrdersFrUrl;
        }
        public static string MyLessons()
        {
            return MyLessonFrUrl;
        }
        public static string MyProfile()
        {
            return MyProfileFrUrl;
        }
        public static string CourseShop()
        {
            return CourseShopFrUrl;
        }
        public static string BookShop()
        {
            return BookShopFrUrl;
        }
        public static string NewsList()
        {
            return NewsListFrUrl;
        }

        public static string News(string url)
        {
            return TrimStar(NewsFrUrl).Replace("{url}", url);
        }
        public static string AboutUs()
        {
            return AboutUsFrUrl;
        }

        #region admin

        public static string Admin
        {
            get { return "/admin/home/index"; }
        }

        #endregion

        private static string TrimStar(string url)
        {
            return url.Replace("*", "");
        }
    }
}
