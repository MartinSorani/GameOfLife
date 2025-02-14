using System.Net;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using Allure.Xunit.Attributes.Steps;
using GameOfLife.Api.Models;
using GameOfLife.Api.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using Parameter = Allure.Net.Commons.Parameter;

namespace GameOfLife.e2e.Tests
{
    [AllureSuite("End-to-End Tests")]
    [Trait("Category", "E2E")]
    public class EndToEndTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private WebApplicationFactory<Program> _factory;
        public required RestClient _client;
        private readonly ILogger _logger;

        [AllureBefore("Setup test context")]
        public EndToEndTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            // Set up the logger.
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var loggerProvider = new FileLoggerProvider(configuration);
            _logger = loggerProvider.CreateLogger(nameof(EndToEndTests));

            // Initialize the REST client.
            InitializeRestClient();
        }

        [AllureStep("Initialize the REST client.")]
        private void InitializeRestClient()
        {
            var httpClient = _factory.CreateClient();
            _client = new RestClient(httpClient);
        }

        /// <summary>
        /// Helper method to create a sample board state.
        /// </summary>
        [AllureStep("Create a sample board state.")]
        private BoardStateDto CreateSampleBoard()
        {
            var boardState = new BoardStateDto
            {
                Board = new bool[][]
                {
                    new bool[] { false, true, false },
                    new bool[] { true, false, true },
                    new bool[] { false, true, false },
                },
            };
            var boardJson = JsonConvert.SerializeObject(boardState.Board);
            AllureLifecycle.Instance.UpdateStep(step =>
            {
                step.parameters.Add(new Parameter { name = "Board Data", value = boardJson });
            });
            return boardState;
        }

        /// <summary>
        /// Helper method to create a new board and validate the response.
        /// </summary>
        /// <returns></returns>
        [AllureStep("Initialize board with sample data")]
        private async Task<Guid> CreateNewBoard()
        {
            // Arrange: create a sample board.
            var boardState = CreateSampleBoard();

            // Act: Execute board upload step, which returns the board ID.
            Guid boardId = await AllureApi.Step(
                "Execute board upload",
                async () =>
                {
                    var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(
                        boardState
                    );
                    var response = await _client.ExecuteAsync<Guid>(request);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Response Status",
                                value = response.StatusCode.ToString(),
                            }
                        );
                    });
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotEqual(Guid.Empty, response.Data);
                    return response.Data;
                }
            );

            // Assert: Validate the returned board ID.
            await AllureApi.Step(
                "Validate uploaded board",
                async () =>
                {
                    _logger.LogInformation("CreateNewBoard: BoardId={BoardId}", boardId);
                    Assert.NotEqual(Guid.Empty, boardId);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter { name = "BoardId", value = boardId.ToString() }
                        );
                    });
                    await Task.CompletedTask;
                }
            );

            return boardId;
        }

        #region UploadBoard Tests
        [AllureDescription("Verify that uploading a valid board returns a valid GUID.")]
        [AllureFeature("/boards")]
        [Fact]
        public async Task UploadBoard_ValidBoard_ReturnsOk()
        {
            await CreateNewBoard();
        }

        [AllureDescription("Verify that uploading an empty board returns a BadRequest.")]
        [AllureFeature("/boards")]
        [Fact]
        public async Task UploadBoard_NullOrEmptyBoard_ReturnsBadRequest()
        {
            // Arrange
            var emptyBoardDto = await AllureApi.Step(
                "Prepare empty board",
                () =>
                {
                    return Task.FromResult(new BoardStateDto { Board = Array.Empty<bool[]>() });
                }
            );

            // Act
            var response = await AllureApi.Step(
                "Execute board upload",
                async () =>
                {
                    var request = new RestRequest("/api/boards", Method.Post).AddJsonBody(
                        emptyBoardDto
                    );
                    return await _client.ExecuteAsync(request);
                }
            );

            // Assert
            await AllureApi.Step(
                "Validate response status",
                async () =>
                {
                    _logger.LogInformation(
                        "UploadBoard_NullOrEmptyBoard_ReturnsBadRequest: StatusCode={StatusCode}",
                        response.StatusCode
                    );
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Response Status",
                                value = response.StatusCode.ToString(),
                            }
                        );
                    });
                    await Task.CompletedTask;
                }
            );
        }

        #endregion

        #region GetNextState Tests

        [AllureDescription("Verify that getting the next state of a valid board returns OK.")]
        [AllureFeature("/next")]
        [Fact]
        public async Task GetNextState_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardId = CreateNewBoard();

            // Act
            var response = await AllureApi.Step(
                "Get next state",
                async () =>
                {
                    var request = new RestRequest($"/api/boards/{boardId}/next", Method.Get);
                    return await _client.ExecuteAsync<BoardStateDto>(request);
                }
            );

            // Assert
            await AllureApi.Step(
                "Validate response status",
                async () =>
                {
                    _logger.LogInformation(
                        "GetNextState_ValidBoardId_ReturnsOk: StatusCode={StatusCode}, Data={Data}",
                        response.StatusCode,
                        response.Data
                    );
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Board);
                    Assert.NotEmpty(response.Data.Board);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Response Status",
                                value = response.StatusCode.ToString(),
                            }
                        );

                        var boardJson = JsonConvert.SerializeObject(response.Data.Board);
                        step.parameters.Add(
                            new Parameter { name = "Board Data", value = boardJson }
                        );
                    });
                    await Task.CompletedTask;
                }
            );
        }

        [AllureDescription(
            "Verify that getting the next state of an invalid board returns NotFound."
        )]
        [AllureFeature("/next")]
        [Fact]
        public async Task GetNextState_InvalidBoardId_ReturnsNotFound()
        {
            // Arrange & Act
            var response = await AllureApi.Step(
                "Get next state",
                async () =>
                {
                    var request = new RestRequest($"/api/boards/{Guid.NewGuid()}/next", Method.Get);
                    return await _client.ExecuteAsync(request);
                }
            );

            // Assert
            await AllureApi.Step(
                "Validate response status",
                async () =>
                {
                    _logger.LogInformation(
                        "GetNextState_InvalidBoardId_ReturnsNotFound: StatusCode={StatusCode}",
                        response.StatusCode
                    );
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Response Status",
                                value = response.StatusCode.ToString(),
                            }
                        );
                    });
                    await Task.CompletedTask;
                }
            );
        }

        #endregion

        #region GetStateAfterSteps Tests

        [AllureDescription(
            "Verify that getting the state after a specified number of steps returns OK."
        )]
        [AllureFeature("/states")]
        [Fact]
        public async Task GetStateAfterSteps_ValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardId = await CreateNewBoard();

            // Act
            var response = await AllureApi.Step(
                "Get state after steps",
                async () =>
                {
                    var request = new RestRequest($"/api/boards/{boardId}/states?steps=1", Method.Get);
                    return await _client.ExecuteAsync<BoardStateDto>(request);
                }
            );

            // Assert
            await AllureApi.Step(
                "Validate response status",
                async () =>
                {
                    _logger.LogInformation(
                        "GetStateAfterSteps_ValidBoardId_ReturnsOk: StatusCode={StatusCode}, Data={Data}",
                        response.StatusCode,
                        response.Data
                    );
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Board);
                    Assert.NotEmpty(response.Data.Board);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Response Status",
                                value = response.StatusCode.ToString(),
                            }
                        );
                        var boardJson = JsonConvert.SerializeObject(response.Data.Board);
                        step.parameters.Add(
                            new Parameter { name = "Board Data", value = boardJson }
                        );
                    });
                    await Task.CompletedTask;
                }
            );
        }

        [Fact]
        public async Task GetStateAfterSteps_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var response = await AllureApi.Step(
                "Get state after steps using invalid board id",
                async () =>
                {
                    var invalidBoardId = Guid.NewGuid();
                    var request = new RestRequest(
                        $"/api/boards/{invalidBoardId}/states?steps=1",
                        Method.Get
                    );
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Invalid BoardId",
                                value = invalidBoardId.ToString(),
                            }
                        );
                    });
                    return await _client.ExecuteAsync(request);
                }
            );

            // Assert
            await AllureApi.Step(
                "Validate response status",
                async () =>
                {
                    _logger.LogInformation(
                        "GetStateAfterSteps_InvalidBoardId_ReturnsNotFound: StatusCode={StatusCode}",
                        response.StatusCode
                    );
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                    AllureLifecycle.Instance.UpdateStep(step =>
                    {
                        step.parameters.Add(
                            new Parameter
                            {
                                name = "Response Status",
                                value = response.StatusCode.ToString(),
                            }
                        );
                    });
                    await Task.CompletedTask;
                }
            );
        }

        [Fact]
        public async Task GetStateAfterSteps_NegativeSteps_ReturnsBadRequest()
        {
            // Arrange
            var boardId = await CreateNewBoard();

            // Act: Negative steps should trigger a BadRequest.
            var response = await AllureApi.Step(
                "Get state using negative steps",
                async () =>
                {
                    var request = new RestRequest(
                        $"/api/boards/{boardId}/states?steps=-1",
                        Method.Get
                    );
                    return await _client.ExecuteAsync(request);
                }
            );

            // Assert
            _logger.LogInformation(
                "GetStateAfterSteps_NegativeSteps_ReturnsBadRequest: StatusCode={StatusCode}",
                response.StatusCode
            );
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region GetFinalState Tests

        [Fact]
        public async Task GetFinalState_ValidBoardId_ReturnsOk()
        {
            _logger.LogDebug("GetFinalState_ValidBoardId_ReturnsOk: Starting test...");
            // Arrange
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await _client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            _logger.LogDebug("Created board with id " + boardId);

            // Act
            var request = new RestRequest(
                $"/api/boards/{boardId}/final?maxIterations=10",
                Method.Get
            );
            var response = await _client.ExecuteAsync<BoardStateDto>(request);

            // Assert
            _logger.LogInformation(
                "GetFinalState_ValidBoardId_ReturnsOk: StatusCode={StatusCode}, Data={Data}",
                response.StatusCode,
                response.Data
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        [Fact]
        public async Task GetFinalState_InvalidBoardId_ReturnsNotFound()
        {
            // Act
            var request = new RestRequest(
                $"/api/boards/{Guid.NewGuid()}/final?maxIterations=10",
                Method.Get
            );
            var response = await _client.ExecuteAsync(request);

            // Assert
            _logger.LogInformation(
                "GetFinalState_InvalidBoardId_ReturnsNotFound: StatusCode={StatusCode}",
                response.StatusCode
            );
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetFinalState_NonPositiveMaxIterations_ReturnsBadRequest()
        {
            // Use the custom factory so that this test uses its own persistent file.
            using var factory = new CustomWebApplicationFactory();
            var httpClient = factory.CreateClient();
            var client = new RestClient(httpClient);

            // Arrange: Upload a valid board.
            var boardState = CreateSampleBoard();
            var uploadRequest = new RestRequest("/api/boards", Method.Post).AddJsonBody(boardState);
            var uploadResponse = await client.ExecuteAsync<Guid>(uploadRequest);
            var boardId = uploadResponse.Data;
            Assert.NotEqual(Guid.Empty, boardId);

            // Act: Using 0 for maxIterations should trigger a BadRequest.
            var request = new RestRequest($"/api/boards/{boardId}/final", Method.Get);
            request.AddQueryParameter("maxIterations", "0");
            var response = await client.ExecuteAsync(request);

            // For debugging:
            Console.WriteLine("Response Content: " + response.Content);
            Console.WriteLine("Status Code: " + response.StatusCode);

            // Assert: Expect BadRequest.
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
            _logger.LogInformation(
                "DataPersistence_AfterServiceRestart_BoardStateIsPersisted: StatusCode={StatusCode}, Data={Data}",
                response.StatusCode,
                response.Data
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Board);
            Assert.NotEmpty(response.Data.Board);
        }

        #endregion

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
                _logger.LogError(ex, "Error during cleanup in Dispose method.");
            }
        }
    }
}
