# signingdemo-dotnet-client

Dette er en lab hvor du skal kalle en GET-tjeneste i et REST API som krever at du signerer requestene du sender. Vi bruker Apache CXF som rammeverk for håndtering av klient og HTTP-signering.

___

For å få det til må vi:
* importere en P12-keystore som inneholder den private nøkkelen
* sette opp tjenester som signerer HTTP-kall
* lage en signert request ved hjelp av en RequestSignerFactory
* sende den signerte requesten med en HttpClient som logger request og response

___

1. Ta utgangspunkt i SampleRSA-klassen i Console-prosjektet.
2. Sett opp en metode for konfigurere nødvendige tjenester for signering. Last inn keystore som heter "signing_demo.p12". Passord er "pw":
      ```csharp
        public static void ConfigureServices(IServiceCollection services)
        {
            var cert = new X509Certificate2(File.ReadAllBytes("./signing_demo.p12"), "pw", X509KeyStorageFlags.Exportable);

            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddHttpMessageSigning()
                .UseKeyId("signing-demo-v1")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning(cert));
        }
3. I Run-metoden, sett opp en serviceProvider:
   ```csharp
        public static async Task Run(string[] args)
        {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider())
            {
            }
        }


4. Lag en SignerFactory som bruker for å generere signaturer:
   ```csharp
        public static async Task Run(string[] args)
        {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider())
            {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>())
                {
                   
                }
            }
        }
5. Lag en "SampleSignRSA"-metode som oppretter en HttpRequest med metode GET mot "https://signingtest.azurewebsites.net/api/stuff"
      ```csharp
        private static async Task<HttpRequestMessage> SampleSignRSA(IRequestSignerFactory requestSignerFactory)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://signingtest.azurewebsites.net/api/stuff"),
                Method = HttpMethod.Get,
                Content = new StringContent("", Encoding.ASCII, "application/json")
            };

            var requestSigner = requestSignerFactory.CreateFor("signing-demo-v1");
            await requestSigner.Sign(request);

            return request;
        }
6. I Run-metoden, lag en signert request ved hjelp av SampleSignRSA:
   ```csharp
        public static async Task Run(string[] args)
        {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider())
            {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>())
                {
                    var logger = serviceProvider.GetService<ILogger<SampleRSA>>();
                    var signedRequestForRSA = await SampleSignRSA(signerFactory);
                }
            }
        }
7. Opprett en HttpClient og send den signerte requesten. 
   ```csharp
        public static async Task Run(string[] args)
        {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider())
            {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>())
                {
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
8. Kjør Run-metoden og sjekk request/respons i konsoll-loggen
9. Prøv å lag og send en POST request og se hvordan Digest blir lagt til i headerne.