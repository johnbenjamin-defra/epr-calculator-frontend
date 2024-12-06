﻿using System.Net;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityUploadFileProcessingControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };
        private TelemetryClient _telemetryClient = new();

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_Success_Result_Test()
        {
            // Mock HttpMessageHandler
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = new StringContent("response content"),
                    });

            // Create HttpClient with the mocked handler
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            // Create controller with the mocked factory
            var controller = new LocalAuthorityUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object, _telemetryClient)
            {
                TempData = tempData
            };

            // Arrange
            var fileUploadFileName = "LocalAuthorityData.csv";

            // Mock ISession
            var sessionMock = new Mock<ISession>();
            var sessionStorage = new Dictionary<string, byte[]>();

            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                       .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                       .Returns((string key, out byte[] value) =>
                       {
                           var success = sessionStorage.TryGetValue(key, out var storedValue);
                           value = storedValue;
                           return success;
                       });

            sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
                       .Callback<string>((key) => sessionStorage.Remove(key));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.Session).Returns(sessionMock.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.HttpContext.Session.SetString("LapcapFileName", fileUploadFileName);

            // Act
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("LocalAuthorityData.csv", controller.FileName);
            Assert.IsTrue(sessionStorage.ContainsKey("LapcapFileName"));
        }

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_Failure_Result_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("response content"),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);

            var controller = new LocalAuthorityUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object, _telemetryClient)
            {
                TempData = tempData
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreNotEqual(201, result.StatusCode);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_ArgumentNullExceptionForAPIConfig_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("response content"),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            config.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value = string.Empty;
            var controller = new LocalAuthorityUploadFileProcessingController(config, mockHttpClientFactory.Object, _telemetryClient);
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_ArgumentNullExceptionForYearConfig_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("response content"),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            config.GetSection("LapcapSettings").GetSection("ParameterYear").Value = string.Empty;
            var controller = new LocalAuthorityUploadFileProcessingController(config, mockHttpClientFactory.Object, _telemetryClient);
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        private static IConfiguration GetConfigurationValues()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(Separator, StringSplitOptions.None)[0];
            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("appsettings.Test.json")
               .Build();

            return config;
        }
    }
}