using GameOfLife.Api.Models;
using GameOfLife.Api.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Net;

namespace GameOfLife.e2e.Tests
{
    [Trait("Category", "E2E")]
    public class EndToEndTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private WebApplicationFactory<Program> _factory;
        private RestClient _client;
        private readonly ILogger _logger;

        public EndToEndTests(WebApplicationFactory<Program> factory)
        {
            // Use the provided factory for the initial tests.
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var loggerProvider = new FileLoggerProvider(configuration);
            _logger = loggerProvider.CreateLogger(nameof(EndToEndTests));
            InitializeRestClient();
        }

        private void InitializeRestClient()
        {
            // Create a HttpClient from the factory.
            var httpClient = _factory.CreateClient();
            // Use the HttpClient directly with RestSharp.
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

        #region UploadBoard Tests

        [Fact]
        public async Task UploadBoard_ValidBoard_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);

            // Act
            var response = await _client.ExecuteAsync<Guid>(request);

            // Assert
            _logger.LogInformation("UploadBoard_ValidBoard_ReturnsOk: StatusCode={StatusCode}, Data={Data}", response.StatusCode, response.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEqual(Guid.Empty, response.Data);
        }

        [Fact]
        public async Task UploadBoard_NullOrEmptyBoard_ReturnsBadRequest()
        {
            // Arrange: Using an empty board array (which should be considered invalid)
            var emptyBoardDto = new BoardStateDto { Board = Array.Empty<bool[]>() };
            var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(emptyBoardDto);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            _logger.LogInformation("UploadBoard_NullOrEmptyBoard_ReturnsBadRequest: StatusCode={StatusCode}", response.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region GetNextState Tests

        [Fact]
        public async Task GetNextState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/next", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            _logger.LogInformation("GetNextState_ValidBoardId_ReturnsOk: StatusCode={StatusCode}, Data={Data}", response.StatusCode, response.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        [Fact]
        public async Task GetNextState_InvalidBoardId_ReturnsNotFound()
        {
            // Arrange & Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/next", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            _logger.LogInformation("GetNextState_InvalidBoardId_ReturnsNotFound: StatusCode={StatusCode}", response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region GetStateAfterSteps Tests

        [Fact]
        public async Task GetStateAfterSteps_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            _logger.LogInformation("GetStateAfterSteps_ValidBoardId_ReturnsOk: StatusCode={StatusCode}, Data={Data}", response.StatusCode, response.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        [Fact]
        public async Task GetStateAfterSteps_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            _logger.LogInformation("GetStateAfterSteps_InvalidBoardId_ReturnsNotFound: StatusCode={StatusCode}", response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetStateAfterSteps_NegativeSteps_ReturnsBadRequest()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act: Negative steps should trigger a BadRequest.
            var request = new RestRequest($"/api/boards/{boardId}/states?steps=-1", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            _logger.LogInformation("GetStateAfterSteps_NegativeSteps_ReturnsBadRequest: StatusCode={StatusCode}", response.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region GetFinalState Tests

        [Fact]
        public async Task GetFinalState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/final?maxIterations=10", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            _logger.LogInformation("GetFinalState_ValidBoardId_ReturnsOk: StatusCode={StatusCode}, Data={Data}", response.StatusCode, response.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        [Fact]
        public async Task GetFinalState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/final?maxIterations=10", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            _logger.LogInformation("GetFinalState_InvalidBoardId_ReturnsNotFound: StatusCode={StatusCode}", response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetFinalState_NonPositiveMaxIterations_ReturnsBadRequest()
        {
            _logger.LogInformation("GetFinalState_NonPositiveMaxIterations_ReturnsBadRequest: Starting test...");
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act: Using 0 for maxIterations.
            var request = new RestRequest($"/api/boards/{boardId}/final?maxIterations=0", Method.Get);
            var response = await _client.ExecuteAsync(request);
            _logger.LogDebug(response.Content ?? "No content in response");

            // Assert
            _logger.LogInformation("GetFinalState_NonPositiveMaxIterations_ReturnsBadRequest: StatusCode={StatusCode}", response.StatusCode);
            Console.WriteLine(response.Content);
            Console.WriteLine(response.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Persistence / Crash Recovery Tests

        /// <summary>
        /// Simulates a service restart (crash recovery) by re-creating the WebApplicationFactory,
        /// and verifies that an uploaded board persists between restarts.
        /// </summary>
        [Fact]
        public async Task DataPersistence_AfterServiceRestart_BoardStateIsPersisted()
        {
            // Arrange: Upload a board using the current factory.
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            Assert.NotEqual(Guid.Empty, boardId);

            // Simulate a service restart by disposing the current factory and creating a new one.
            _factory.Dispose();

            // Create a new factory instance.
            using var newFactory = new WebApplicationFactory<Program>();
            var httpClient = newFactory.CreateClient();
            var newClient = new RestClient(httpClient);

            // Act: Retrieve the next state using the new client (which should reflect persisted data).
            var request = new RestRequest($"/api/boards/{boardId}/next", Method.Get);
            var response = await newClient.ExecuteAsync<BoardStateDto>(request);

            // Assert: The board state should persist across restarts.
            _logger.LogInformation("DataPersistence_AfterServiceRestart_BoardStateIsPersisted: StatusCode={StatusCode}, Data={Data}", response.StatusCode, response.Data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        #endregion

        // Clean up test data
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
                _logger.LogError("Error during cleanup in Dispose method.", ex);
            }
        }
    }
}
