using GameOfLifeAPI.Models;
using GameOfLifeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLifeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly IGameOfLifeService _gameService;
        private readonly ILogger<BoardsController> _logger;

        public BoardsController(IGameOfLifeService gameService, ILogger<BoardsController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new game board.
        /// </summary>
        /// <param name="boardStateModel">The initial state of the game board. Minimum size is 3x3.</param>
        /// <returns>The ID of the created board.</returns>
        /// <response code="400">If the board size is less than 3x3.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadBoard([FromBody] BoardStateModel boardStateModel)
        {
            if (boardStateModel == null || boardStateModel.State == null)
            {
                _logger.LogWarning("UploadBoard called with null boardState.");
                return BadRequest("Board state cannot be null.");
            }

            var boardState = boardStateModel.State;

            // Validate minimum board size
            if (boardState.Length < 3 || boardState.Any(row => row.Length < 3))
            {
                _logger.LogWarning("Board size is too small. Minimum size is 3x3.");
                return BadRequest("Board size is too small. Minimum size is 3x3.");
            }

            var boardId = await _gameService.CreateBoardAsync(boardState);
            _logger.LogInformation("Board created with ID: {BoardId}", boardId);
            return CreatedAtAction(nameof(GetNextState), new { boardId }, new { BoardId = boardId });
        }

        /// <summary>
        /// Gets the next state of the game board.
        /// </summary>
        /// <param name="boardId">The ID of the board.</param>
        /// <returns>The next state of the board.</returns>
        [HttpGet("{boardId}/next")]
        public async Task<IActionResult> GetNextState(Guid boardId)
        {
            try
            {
                var nextState = await _gameService.GetNextStateAsync(boardId);
                _logger.LogInformation("Next state retrieved for board ID: {BoardId}", boardId);
                return Ok(nextState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next state for board ID: {BoardId}", boardId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets the future state of the game board after a specified number of generations.
        /// </summary>
        /// <param name="boardId">The ID of the board.</param>
        /// <param name="x">The number of generations to simulate.</param>
        /// <returns>The future state of the board.</returns>
        [HttpGet("{boardId}/next/{x}")]
        public async Task<IActionResult> GetFutureState(Guid boardId, int x)
        {
            try
            {
                var futureState = await _gameService.GetFutureStateAsync(boardId, x);
                _logger.LogInformation("Future state retrieved for board ID: {BoardId} after {Generations} generations", boardId, x);
                return Ok(futureState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving future state for board ID: {BoardId} after {Generations} generations", boardId, x);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets the final state of the game board after a specified number of attempts.
        /// </summary>
        /// <param name="boardId">The ID of the board.</param>
        /// <param name="maxAttempts">The maximum number of attempts to reach a stable state.</param>
        /// <returns>The final state of the board.</returns>
        [HttpGet("{boardId}/final")]
        public async Task<IActionResult> GetFinalState(Guid boardId, [FromQuery] int maxAttempts = 1000)
        {
            try
            {
                var finalState = await _gameService.GetFinalStateAsync(boardId, maxAttempts);
                _logger.LogInformation("Final state retrieved for board ID: {BoardId} after {MaxAttempts} attempts", boardId, maxAttempts);
                return Ok(finalState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving final state for board ID: {BoardId} after {MaxAttempts} attempts", boardId, maxAttempts);
                return BadRequest(ex.Message);
            }
        }
    }
}
