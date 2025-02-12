using GameOfLife.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using RestSharp;
using System.Net;

namespace GameOfLife.Integration.Tests
{
    [Trait("Category", "Integration")]
    public class ControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly RestClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            // Create a test HttpClient from the factory.
            var httpClient = _factory.CreateClient();

            // Pass the HttpClient directly to the RestClient.
            _client = new RestClient(httpClient);
        }

        /// <summary>
        /// Helper method that returns a sample board state.
        /// </summary>
        private BoardStateDto CreateSampleBoard()
        {
            return new BoardStateDto
            {
                Board = new bool[][]
                {
                    new bool[] { false, true, false },
                    new bool[] { true, false, true },
                    new bool[] { false, true, false }
                }
            };
        }

        /// <summary>
        /// Tests that uploading a valid board returns an OK status and a non-empty GUID.
        /// </summary>
        [Fact]
        public async Task UploadBoard_ValidBoard_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var request = new RestRequest("/api/boards", Method.Post)
                .AddJsonBody(boardState);

            // Act
            var response = await _client.ExecuteAsync<Guid>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEqual(Guid.Empty, response.Data);
        }

        /// <summary>
        /// Tests that uploading a null board returns a BadRequest status.
        /// </summary>
        [Fact]
        public async Task UploadBoard_NullBoard_ReturnsBadRequest()
        {
            // Arrange
            var emptyBoardDto = new BoardStateDto { Board = Array.Empty<bool[]>() };
            var request = new RestRequest("/api/boards", Method.Post)
                .AddJsonBody(emptyBoardDto);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Tests that getting the next state of a valid board ID returns an OK status and a non-null board state.
        /// </summary>
        [Fact]
        public async Task GetNextState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post)
                .AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/next", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        /// <summary>
        /// Tests that getting the next state of an invalid board ID returns a NotFound status.
        /// </summary>
        [Fact]
        public async Task GetNextState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/next", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Tests that getting the state after a number of steps for a valid board ID returns an OK status and a non-null board state.
        /// </summary>
        [Fact]
        public async Task GetStateAfterSteps_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post)
                .AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        /// <summary>
        /// Tests that getting the state after a number of steps for an invalid board ID returns a NotFound status.
        /// </summary>
        [Fact]
        public async Task GetStateAfterSteps_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Tests that getting the final state of a valid board ID returns an OK status and a non-null board state.
        /// </summary>
        [Fact]
        public async Task GetFinalState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post)
                .AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/final?maxIterations=10", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        /// <summary>
        /// Tests that getting the final state of an invalid board ID returns a NotFound status.
        /// </summary>
        [Fact]
        public async Task GetFinalState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/final?maxIterations=10", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
