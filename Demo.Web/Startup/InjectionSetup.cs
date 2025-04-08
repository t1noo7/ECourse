using Demo.Application.Repositories;
using Demo.Core.Repositories;
using Demo.Core.Services;
using Demo.Database.Repositories;
using Demo.Application.Services;
using Demo.Database;
using MongoDB.Driver;
using Demo.Infrastructure.File;
using Demo.Infrastructure.Mail;
using Demo.Web.Helpers;

namespace Demo.Web.Startup
{
    public static class InjectionSetup
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddTransient<IEmailTemplate, EmailTemplate>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<ISystemParameters, SystemParameters>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IFileService, StorageAccount>();
            services.AddTransient<IUserGroupManager, UserGroupManager>();

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IBannerRepository, BannerRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<INewRepository, NewRepository>();
            services.AddTransient<IUserGroupRepository, UserGroupRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IPaymentRepository, PaymentRepository>();
            //services.AddTransient<IContactRepository, ContactRepository>();
            services.AddTransient<IVoucherRepository, VoucherRepository>();
            //services.AddTransient<IGeneralItemRepository, GeneralItemRepository>();
            services.AddTransient<ICourseRepository, CourseRepository>();
            services.AddTransient<IChapterRepository, ChapterRepository>();
            services.AddTransient<ILessonRepository, LessonRepository>();
            services.AddTransient<IClassRepository, ClassRepository>();
            //services.AddTransient<IEventRepository, EventRepository>();
            //services.AddTransient<IBookRepository, BookRepository>();
            services.AddSingleton<IMongoClient>(s =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                return new MongoClient(connectionString);
            });

            // services.RegisterJobAsync().GetAwaiter().GetResult();

            return services;
        }
    }
}
