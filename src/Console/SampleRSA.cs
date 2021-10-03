using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Console {
    public class SampleRSA {
        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                 
                        var logger = serviceProvider.GetService<ILogger<SampleRSA>>();
                        var signedRequestForRSA = await SampleSignRSA(signerFactory);

                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.SendAsync(signedRequestForRSA);
                        if (response.IsSuccessStatusCode)
                        {
                            logger?.LogInformation("{0} - GET request response: {1}", response.StatusCode, response.StatusCode);
                        }
                        else
                        {
                            logger?.LogError("{0} - GET request response: {1}, {2}", response.StatusCode, response.ReasonPhrase, response.Content.ToString());
                        }
                        var responseContentTask = response.Content?.ReadAsStringAsync();
                        var responseContent = responseContentTask == null ? null : await responseContentTask;
                        if (responseContent != null) logger?.LogInformation(responseContent);
                    }
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            var cert = new X509Certificate2(File.ReadAllBytes("./signing_demo.pfx"), "pw", X509KeyStorageFlags.Exportable);

            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddHttpMessageSigning()
                .UseKeyId("signing-demo-v1")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning(cert))
                .Services
                .AddHttpMessageSignatureVerification()
                .UseClient(Client.Create(
                    "signing-demo-v1",
                    "HttpMessageSigningSampleRSA",
                    SignatureAlgorithm.CreateForVerification(cert),
                    options => options.Claims = new[] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ));
        }

        private static async Task<HttpRequestMessage> SampleSignRSA(IRequestSignerFactory requestSignerFactory) {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://signingtest.azurewebsites.net/api/stuff"),
                Method = HttpMethod.Get,
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                },
                Content = new StringContent("", Encoding.ASCII, "application/json")

        };

            var requestSigner = requestSignerFactory.CreateFor("signing-demo-v1");
            await requestSigner.Sign(request);
           
            return request;
        }
    }
}