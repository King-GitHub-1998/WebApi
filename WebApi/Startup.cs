using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using API.DataAccess.Comm;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApi
{
    public class Startup
    {
        [Obsolete]
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            //Configuration = configuration;
            var builder = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(env.ContentRootPath, "Config"))
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)//���ӻ��������ļ����½���ĿĬ����
               .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                //��application���ע����ӵ�swaggerui��
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                var CommentsFileName = @"WebApi.xml";
                var CommentsFile = Path.Combine(baseDirectory, CommentsFileName);
                //��ע�͵�Xml�ĵ���ӵ�swaggerUi��
                c.IncludeXmlComments(CommentsFile);
            });

            ConfigDatabase(services);
            services.AddMvc();
            //���cors ���� ���ÿ�����            
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("any", builder =>
            //    {
            //        builder.AllowAnyOrigin() //�����κ���Դ����������
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials();//ָ������cookie
            //    });
            //});

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //��������
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
                //builder.WithOrigins("*");
            });
            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //�����м����������Swagger��ΪJSON�ս��
            app.UseSwagger();
            //�����м�������swagger-ui��ָ��Swagger JSON�ս��
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

        }


        /// <summary>
        /// �������ݿ�
        /// </summary>
        /// <param name="services"></param>
        public void ConfigDatabase(IServiceCollection services)
        {
            //var DataBaseSetting = Configuration.GetSection("DataBase");
            var sqlConnection = Configuration.GetConnectionString("SqlServerConnection");
            //var comDbConect = Configuration["DataBase:DbConnections"];
            services.AddDbContext<APIDBContext>(option =>
                option.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("WebApi")));
            //services.Configure<AppSettings>
            //DataBase.GetConnectionString
        }


        public static ILoggerRepository repository { get; set; }
        /// <summary>
        /// ������־��Ϣ
        /// </summary>
        public void ConfigLog()
        {
            repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(repository, new FileInfo("Config/log4net.config"));
            //SDKProperties.LogRepository = repository;
        }

    }
}
