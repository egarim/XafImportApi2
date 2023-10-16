using System.Configuration;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win.ApplicationBuilder;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.XtraEditors;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Design;

namespace XafImportApi.Win;

public class ApplicationBuilder : IDesignTimeApplicationFactory {
    public static WinApplication BuildApplication() {
        var builder = WinApplication.CreateBuilder();
        // Register custom services for Dependency Injection. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/404430/
        // builder.Services.AddScoped<CustomService>();
        // Register 3rd-party IoC containers (like Autofac, Dryloc, etc.)
        // builder.UseServiceProviderFactory(new DryIocServiceProviderFactory());
        // builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.UseApplication<XafImportApiWindowsFormsApplication>();
        builder.Modules
            .AddConditionalAppearance()
            .AddValidation(options => {
                options.AllowValidationDetailsAccess = false;
            })
            .Add<XafImportApi.Module.XafImportApiModule>()
        	.Add<XafImportApiWinModule>();
        builder.ObjectSpaceProviders
            .AddNonPersistent();
        //builder.Security
        //    .UseMiddleTierMode(options => {
        //        options.BaseAddress = new Uri("https://localhost:44319/");
        //        options.Events.OnHttpClientCreated = client => client.DefaultRequestHeaders.Add("Accept", "application/json");
        //        options.Events.OnCustomAuthenticate = (sender, security, args) => {
        //            args.Handled = true;
        //            HttpResponseMessage msg = args.HttpClient.PostAsJsonAsync("api/Authentication/Authenticate", (AuthenticationStandardLogonParameters)args.LogonParameters).GetAwaiter().GetResult();
        //            string token = (string)msg.Content.ReadFromJsonAsync(typeof(string)).GetAwaiter().GetResult();
        //            if(msg.StatusCode == HttpStatusCode.Unauthorized) {
        //                throw new UserFriendlyException(token);
        //            }
        //            msg.EnsureSuccessStatusCode();
        //            args.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        //        };
        //    })
        //    .UsePasswordAuthentication();
        builder.AddBuildStep(application => {
        });
        var winApplication = builder.Build();
        return winApplication;
    }

    XafApplication IDesignTimeApplicationFactory.Create() {
        DevExpress.ExpressApp.Security.ClientServer.MiddleTierClientSecurity.DesignModeUserType = typeof(XafImportApi.Module.BusinessObjects.ApplicationUser);
        DevExpress.ExpressApp.Security.ClientServer.MiddleTierClientSecurity.DesignModeRoleType = typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyRole);
        return BuildApplication();
    }
}
