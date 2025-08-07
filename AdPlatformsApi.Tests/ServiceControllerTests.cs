using AdPlatforms;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace AdPlatformsApi.Tests
{
    public enum EndPoints
    {
        UploadFile = 0,
        GetPlatforms
    }

    [TestFixture]
    public class ServiceControllerTests
    {
        readonly string validTestFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "testdata_valid.txt");
        readonly string invalidTestFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "testdata_invalid.txt");

        Dictionary<EndPoints, string> endPoints = new Dictionary<EndPoints, string>()
        {
            { EndPoints.UploadFile, "/api/service/upload" },
            { EndPoints.GetPlatforms, "/api/service/getplatforms" }
        };

        WebApplicationFactory<Program> webHost;
        HttpClient client;

        [OneTimeSetUp]
        public void Setup()
        {
            webHost = new WebApplicationFactory<Program>().WithWebHostBuilder(_ => { });
            client = webHost.CreateClient();
        }

        /// <summary>
        /// “ест метода загрузки рекламных площадок из файла с корректными данными
        /// </summary>
        /// <returns></returns>
        [Test, Order(1)]
        public async Task UploadFile_ValidDataInFile_ReturnsOk()
        {
            var fileData = File.ReadAllText(validTestFilePath);
            var content = new MultipartFormDataContent
            {
                { new StringContent(fileData), "file", "test.txt" }
            };

            var response = await client.PostAsync(endPoints[EndPoints.UploadFile], content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        /// <summary>
        /// “ест метода получени€ списка рекламных площадок дл€ локации, котора€ есть в словаре
        /// (ѕеред запуском необходимо выполнить первый тест выше)
        /// </summary>
        /// <returns></returns>
        [Test, Order(2)]
        public async Task GetPlatforms_ExistLocation_ReturnsPlatforms()
        {
            var location = "/ru/svrd/revda";
            var expectedPlatforms = new string[] { "яндекс.ƒирект", "–евдинский рабочий", " рута€ реклама" };
            var uri = endPoints[EndPoints.GetPlatforms] + "?location=" + location;

            var response = await client.GetAsync(uri);
            var responseData = await response.Content.ReadFromJsonAsync<string[]>();

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.IsNotNull(responseData);
                Assert.That(responseData, Is.EquivalentTo(expectedPlatforms));
            });
        }

        /// <summary>
        /// “ест метода получени€ списка рекламных площадок дл€ локации, которой нет в словаре
        /// </summary>
        /// <returns></returns>
        [Test, Order(3)]
        public async Task GetPlatforms_NotExistLocation_ReturnsNotFound()
        {
            var location = "/test";
            var uri = endPoints[EndPoints.GetPlatforms] + "?location=" + location;

            var response = await client.GetAsync(uri);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        /// <summary>
        /// “ест метода получени€ списка рекламных площадок без передачи локации в параметре запроса
        /// </summary>
        /// <returns></returns>
        [Test, Order(4)]
        public async Task GetPlatforms_NoLocation_ReturnsBadRequest()
        {
            var uri = endPoints[EndPoints.GetPlatforms];

            var response = await client.GetAsync(uri);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        /// <summary>
        /// “ест метода загрузки рекламных площадок без файла
        /// </summary>
        /// <returns></returns>
        [Test, Order(5)]
        public async Task UploadFile_NoFile_ReturnsBadRequest()
        {
            var content = new MultipartFormDataContent();

            var response = await client.PostAsync(endPoints[EndPoints.UploadFile], content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        /// <summary>
        /// “ест метода загрузки рекламных площадок из файла с некорректными данными
        /// </summary>
        /// <returns></returns>
        [Test, Order(6)]
        public async Task UploadFile_InvalidDataInFile_ReturnsBadRequest()
        {
            var fileData = File.ReadAllText(invalidTestFilePath);
            var content = new MultipartFormDataContent
            {
                { new StringContent(fileData), "file", "test.txt" }
            };

            var response = await client.PostAsync(endPoints[EndPoints.UploadFile], content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        /// <summary>
        /// “ест скорости выполнени€ одной тыс€чи запросов метода поиска рекламных площадок
        /// </summary>
        /// <returns></returns>
        [Test, Order(7)]
        public async Task GetPlatforms_1000RequestsElapsedTime_IsLessThan500Milliseconds()
        {
            var location = "/ru/svrd";
            var uri = endPoints[EndPoints.GetPlatforms] + "?location=" + location;

            Stopwatch sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000; i++)
            {
                var response = await client.GetAsync(uri);
            }
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500L));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client.Dispose();
            webHost.Dispose();
        }
    }
}