using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Concrete;
using Castle.DynamicProxy;
using Core.Entities.Concrete;
using Core.Utilities.Interceptors;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete.Entityframework;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Business.DependencyResolvers.Autofac
{
    public class AutofacBusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {


            builder.RegisterType<UserOperationClaimManager>().As<IUserOperationClaimService>().SingleInstance();
            builder.RegisterType<EfUserOperationClaimDal>().As<IUserOperationClaimDal>().SingleInstance();

            builder.RegisterType<OperationClaimManager>().As<IOperationClaimService>().SingleInstance();
            builder.RegisterType<EfOperationClaimDal>().As<IOperationClaimDal>().SingleInstance();

            builder.RegisterType<VerificationCodeManager>().As<IVerificationCodeService>().SingleInstance();
            builder.RegisterType<EfVerificationCodeDal>().As<IVerificationCodeDal>().SingleInstance();

            builder.RegisterType<ArticleManager>().As<IArticleService>().SingleInstance();
            builder.RegisterType<EfArticleDal>().As<IArticleDal>().SingleInstance();

            builder.RegisterType<CommentManager>().As<ICommentService>().SingleInstance();
            builder.RegisterType<EfCommentleDal>().As<ICommentDal>().SingleInstance();

            builder.RegisterType<TopicManager>().As<ITopicService>().SingleInstance();
            builder.RegisterType<EfTopicDal>().As<ITopicDal>().SingleInstance();

            builder.RegisterType<UserImageManager>().As<IUserImageService>().SingleInstance();
            builder.RegisterType<EfUserImageDal>().As<IUserImageDal>().SingleInstance();

            builder.RegisterType<UserManager>().As<IUserService>().SingleInstance();
            builder.RegisterType<EfUserDal>().As<IUserDal>().SingleInstance();

            builder.RegisterType<AuthManager>().As<IAuthService>();
            builder.RegisterType<JwtHelper>().As<ITokenHelper>();

            builder.RegisterType<UserFollowManager>().As<IUserFollowService>().SingleInstance();
            builder.RegisterType<EfUserFollowDal>().As<IUserFollowDal>().SingleInstance();
            
            builder.RegisterType<MessageManager>().As<IMessageService>().SingleInstance();
            builder.RegisterType<EfMessageDal>().As<IMessageDal>().SingleInstance();

            // Code Share and Comment services
            builder.RegisterType<CodeShareManager>().As<ICodeShareService>().SingleInstance();
            builder.RegisterType<EfCodeShareDal>().As<ICodeShareDal>().SingleInstance();
            
            builder.RegisterType<CodeCommentManager>().As<ICodeCommentService>().SingleInstance();
            builder.RegisterType<EfCodeCommentDal>().As<ICodeCommentDal>().SingleInstance();

            // Register HttpClientFactory for AI features
            builder.Register(c => new HttpClient()).As<HttpClient>().SingleInstance();
            builder.Register(c => 
            {
                var httpClientFactory = new Func<HttpClient>(() => c.Resolve<HttpClient>());
                return new HttpClientFactory(httpClientFactory);
            }).As<IHttpClientFactory>().SingleInstance();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();

            //builder.RegisterBuildCallback(cr => Console.WriteLine("Container built!"));
        }
    }

    // Simple HttpClientFactory implementation
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly Func<HttpClient> _httpClientFactory;

        public HttpClientFactory(Func<HttpClient> httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClientFactory();
        }
    }
}
