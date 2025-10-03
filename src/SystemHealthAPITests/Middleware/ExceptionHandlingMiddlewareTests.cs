using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using SystemHealthAPI.Exceptions;
using SystemHealthAPI.Middleware;

namespace SystemHealthAPITests.Middleware;

[TestClass]
public class ExceptionHandlingMiddlewareTests
{
    private Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
    private Mock<RequestDelegate> _mockNext;
    private ExceptionHandlingMiddleware _middleware;
    private DefaultHttpContext _httpContext;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
        _middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [TestMethod]
    public async Task Invoke_CallsNext_WhenNoExceptionOccurs()
    {
        // Arrange
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(_httpContext);

        // Assert
        _mockNext.Verify(x => x(_httpContext), Times.Once);
        Assert.AreEqual(200, _httpContext.Response.StatusCode); // Default status code
    }

    [TestMethod]
    public async Task Invoke_HandlesMetricsNotAvailableException_ReturnsNotFound()
    {
        // Arrange
        var exception = new MetricsNotAvailableException("No metrics available");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(_httpContext);

        // Assert
        Assert.AreEqual((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
        Assert.AreEqual("application/json", _httpContext.Response.ContentType);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No metrics available")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Invoke_HandlesGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(_httpContext);

        // Assert
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        Assert.AreEqual("application/json", _httpContext.Response.ContentType);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unexpected exception occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Invoke_LogsCorrectMessage_ForGenericException()
    {
        // Arrange
        var exception = new ArgumentException("Test argument exception");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unexpected exception occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}