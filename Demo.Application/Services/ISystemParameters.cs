using Demo.Core.Models;
using System.ComponentModel;

namespace Demo.Application.Services
{
    public interface ISystemParameters
    {
        object GetValue(string name);
        void SetValue(string name, object value);
        List<SystemParamData> GetValues();

        [Description("Thông tin ngân hàng")]
        public string? BankInfo { get; }

        [Description("SMPT email host")]
        public string? SmtpHost { get; }

        [Description("SMPT email port")]
        public int SmtpPort { get; }

        [Description("SMPT email")]
        public string? SmtpEmail { get; }

        [Description("SMPT email password")]
        public string? SmtpPassword { get; }

        [Description("Email kế toán (Mỗi email một dòng)")]
        public string? AccountingEmails { get; }

        [Description("Tên miền")]
        public string? Domain { get; }

        [Description("Thời gian tối đa để thanh toán sau khi đặt hàng (ngày)")]
        public int PaymentPendingDays { get; }
    }
}
