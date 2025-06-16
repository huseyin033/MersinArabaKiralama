using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MersinArabaKiralama;

namespace MersinArabaKiralama.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Test sunucusunu yapılandır
            builder.ConfigureServices(services =>
            {
                // Kimlik doğrulama şemasını ekle
                services.AddAuthentication(TestAuthHandler.TestScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.TestScheme, options => { });
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Test servislerini burada yapılandırabilirsiniz
            });
        }
    }
}
