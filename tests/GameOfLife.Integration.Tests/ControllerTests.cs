using Allure.Xunit.Attributes;
using Allure.Xunit.Attributes.Steps;
using GameOfLife.Api.Models;
using GameOfLife.Api.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Net;

namespace GameOfLife.Integration.Tests
{
    [AllureSuite("End-to-End Tests")]
    [Trait("Category", "Integration")]
    public class ControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private WebApplicationFactory<Program> _factory;
        public required RestClient _client;
        private readonly ILogger _logger;

        [AllureBefore("Setup test context")]
        public ControllerTests(WebApplicationFactory<Program> factory)
        {
            // Use the provided factory for the initial tests.
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var loggerProvider = new FileLoggerProvider(configuration);
            _logger = loggerProvider.CreateLogger(nameof(ControllerTests));
            InitializeRestClient();
        }

        /// <summary>
        /// Initializes the REST client with the base URL from the factory.
        /// </summary>
        private void InitializeRestClient()
        {
            var httpClient = _factory.CreateClient();
            _client = new RestClient(httpClient);
        }

        /// <summary>
        /// Helper method to create a sample board state.
        /// </summary>
        private BoardStateDto CreateSampleBoard()
        {
            return new BoardStateDto
            {
                Board = new bool[][]
                {
                    new bool[] { false, true, false },
                    new bool[] { true, false, true },
                    new bool[] { false, true, false },
                },
            };
        }

        [AllureDescription("Tests that uploading a valid board returns an OK status and a non-empty GUID.")]
        [AllureFeature("Upload Board")]
        [Fact]
        public async Task UploadBoard_ValidBoard_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            _logger.LogInformation("Uploading a valid board.");

            // Act
            var response = await _client.ExecuteAsync<Guid>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEqual(Guid.Empty, response.Data);
            _logger.LogInformation("UploadBoard_ValidBoard_ReturnsOk test passed.");
        }

        [AllureDescription("Tests that uploading a null board returns a BadRequest status.")]
        [AllureFeature("Upload Board")]
        [Fact]
        public async Task UploadBoard_NullBoard_ReturnsBadRequest()
        {
            // Arrange
            var emptyBoardDto = new BoardStateDto { Board = Array.Empty<bool[]>() };
            var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(emptyBoardDto);
            _logger.LogInformation("Uploading a null board.");

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _logger.LogInformation("UploadBoard_NullBoard_ReturnsBadRequest test passed.");
        }

        [AllureDescription("Tests that getting the next state of a valid board ID returns an OK status and a non-null board state.")]
        [AllureFeature("Get Next State")]
        [Fact]
        public async Task GetNextState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            _logger.LogInformation("Getting the next state for a valid board ID.");

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/next", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
            _logger.LogInformation("GetNextState_ValidBoardId_ReturnsOk test passed.");
        }

        [AllureDescription("Tests that getting the next state of an invalid board ID returns a NotFound status.")]
        [AllureFeature("Get Next State")]
        [Fact]
        public async Task GetNextState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/next", Method.Get);
            _logger.LogInformation("Getting the next state for an invalid board ID.");
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _logger.LogInformation("GetNextState_InvalidBoardId_ReturnsNotFound test passed.");
        }

        [AllureDescription("Tests that getting the state after a number of steps for a valid board ID returns an OK status and a non-null board state.")]
        [AllureFeature("Get State After Steps")]
        [Fact]
        public async Task GetStateAfterSteps_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            _logger.LogInformation("Getting the state after steps for a valid board ID.");

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
            _logger.LogInformation("GetStateAfterSteps_ValidBoardId_ReturnsOk test passed.");
        }

        [AllureDescription("Tests that getting the state after a number of steps for an invalid board ID returns a NotFound status.")]
        [AllureFeature("Get State After Steps")]
        [Fact]
        public async Task GetStateAfterSteps_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/states?steps=1", Method.Get);
            _logger.LogInformation("Getting the state after steps for an invalid board ID.");
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _logger.LogInformation("GetStateAfterSteps_InvalidBoardId_ReturnsNotFound test passed.");
        }

        [AllureDescription("Tests that getting the state after negative steps returns a BadRequest status.")]
        [AllureFeature("Get State After Steps")]
        [Fact]
        public async Task GetStateAfterSteps_NegativeSteps_ReturnsBadRequest()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            _logger.LogInformation("Getting the state after negative steps.");

            // Act: Use negative steps.
            var request = new RestRequest($"/api/boards/{boardId}/states?steps=-1", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert: Expect BadRequest.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _logger.LogInformation("GetStateAfterSteps_NegativeSteps_ReturnsBadRequest test passed.");
        }

        [AllureDescription("Tests that getting the final state of a valid board ID returns an OK status and a non-null board state.")]
        [AllureFeature("Get Final State")]
        [Fact]
        public async Task GetFinalState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            _logger.LogInformation("Getting the final state for a valid board ID.");

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/final?maxIterations=10", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
            _logger.LogInformation("GetFinalState_ValidBoardId_ReturnsOk test passed.");
        }

        [AllureDescription("Tests that getting the final state of an invalid board ID returns a NotFound status.")]
        [AllureFeature("Get Final State")]
        [Fact]
        public async Task GetFinalState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/final?maxIterations=10", Method.Get);
            _logger.LogInformation("Getting the final state for an invalid board ID.");
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _logger.LogInformation("GetFinalState_InvalidBoardId_ReturnsNotFound test passed.");
        }

        [AllureDescription("Tests that getting the final state with non-positive max iterations returns a BadRequest status.")]
        [AllureFeature("Get Final State")]
        [Fact]
        public async Task GetFinalState_NonPositiveMaxIterations_ReturnsBadRequest()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            _logger.LogInformation("Getting the final state with non-positive max iterations.");

            // Act: Use 0 for maxIterations.
            var request = new RestRequest($"/api/boards/{boardId}/final?maxIterations=0", Method.Get);
            var response = await _client.ExecuteAsync(request);
            _logger.LogDebug(response.Content ?? "Response content is null");

            // Assert: Expect BadRequest.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _logger.LogInformation("GetFinalState_NonPositiveMaxIterations_ReturnsBadRequest test passed.");
        }

        // Clean up test data
        [AllureAfter("Clean test context")]
        public void Dispose()
        {
            try
            {
                var filePath = "boards.json";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during cleanup in Dispose method: {ExceptionMessage}", ex.Message);
            }
        }
    }
}
