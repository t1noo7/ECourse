using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Core.Enums
{
    public static class TempDataMessage
    {
        #region Success
        /// <summary>
        /// Đăng nhập, Đăng ký, Xác minh
        /// </summary>
        public const string LoginSuccess = "Đăng nhập thành công!";
        public const string RegisterSuccess = "Đăng ký thành công!";
        public const string VerifySuccess = "Xác minh thành công!";
        /// <summary>
        /// CheckOut
        /// </summary>
        public const string CheckOutSuccess = "Đơn hàng đã được tạo thành công! Chúng tôi sẽ kiểm tra và duyệt đơn hàng của bạn trong thời gian sớm nhất.";

        public const string AddSuccess = "Thêm mới thành công!";
        public const string UpdateSuccess = "Cập nhật thành công!";
        public const string ChangeStatusSuccess = "Thay đổi trạng thái thành công!";
        public const string DeleteSuccess = "Xóa thành công!";

        #endregion

        #region Error
        public const string GeneralError = "Đã xảy ra lỗi. Vui lòng thử lại sau.";
        public const string CheckOutError = "Có lỗi khi lưu đơn hàng, vui lòng thử lại.";
        public const string VerifyCodeNotMatched = "Mã xác minh không đúng.";
        #endregion

        #region Warning
        public const string IncompleteForm = "Vui lòng điền đầy đủ thông tin.";
        #endregion

        #region Info
        public const string NoChangeDetected = "Không có thay đổi nào được ghi nhận.";
        #endregion
    }
}
