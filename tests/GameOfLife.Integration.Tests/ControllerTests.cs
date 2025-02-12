using GameOfLife.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using RestSharp;

namespace GameOfLife.Integration.Tests
{
    public class ControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly RestClient _client;

        public ControllerTests(WebApplicationFactory<Program> factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var clientOptions = new RestClientOptions
            {
                BaseUrl = new Uri("http://localhost")
            };
            _client = new RestClient(clientOptions);
        }

        [Fact]
        public async Task UploadBoard_ValidBoard_ReturnsOk()
        {
            // Arrange
            var boardState = new BoardStateDto
            {
                Board = new bool[][]
                {
                        new bool[] { false, true, false },
                        new bool[] { true, false, true },
                        new bool[] { false, true, false }
                }
            };
            var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);

            // Act
            var response = await _client.ExecuteAsync<Guid>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEqual(Guid.Empty, response.Data);
        }

        [Fact]
        public async Task UploadBoard_NullBoard_ReturnsBadRequest()
        {
            // Act
            var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(new BoardStateDto { Board = Array.Empty<bool[]>() });
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetNextState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = new BoardStateDto
            {
                Board = new bool[][]
                {
                        new bool[] { false, true, false },
                        new bool[] { true, false, true },
                        new bool[] { false, true, false }
                }
            };
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/next", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetNextState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/next", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetStateAfterSteps_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = new BoardStateDto
            {
                Board = new bool[][]
                {
                        new bool[] { false, true, false },
                        new bool[] { true, false, true },
                        new bool[] { false, true, false }
                }
            };
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetStateAfterSteps_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/states?steps=1", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetFinalState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardState = new BoardStateDto
            {
                Board = new bool[][]
                {
                        new bool[] { false, true, false },
                        new bool[] { true, false, true },
                        new bool[] { false, true, false }
                }
            };
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;

            // Act
            var request = new RestRequest($"/api/boards/{boardId}/final?maxIterations=10", Method.Get);
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
        }

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
