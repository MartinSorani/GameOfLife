using GameOfLife.Api.Models;
using GameOfLife.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using GameOfLife.Api.Examples;
using GameOfLife.Api.Utils;
using Serilog;

namespace GameOfLife.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly IGameOfLifeService _gameService;
        private readonly Serilog.ILogger _logger;

        public BoardsController(IGameOfLifeService gameService)
        {
            _gameService = gameService;
            _logger = Log.ForContext<BoardsController>();
        }

        /// <summary>
        /// Uploads a new board state.
        /// Expects a JSON body representing a jagged array of booleans.
        /// Returns the generated board Id.
        /// </summary>
        /// <param name="request">DTO containing the board state.</param>
        /// <returns>The board's unique identifier.</returns>
        [HttpPost]
        [SwaggerRequestExample(typeof(BoardStateDto), typeof(BoardDtoExample))]
        public ActionResult<Guid> UploadBoard([FromBody] BoardStateDto request)
        {
            if (request?.Board == null)
            {
                _logger.Warning("UploadBoard: Board state must be provided.");
                return BadRequest("Board state must be provided.");
            }

            try
            {
                // Convert the jagged array to a 2D array for internal processing.
                // If the conversion fails, return a bad request.
                bool[,] board2D;
                try
                {
                    board2D = ArrayConverter.ConvertTo2DArray(request.Board);
                }
                catch (ArgumentException ex)
                {
                    _logger.Warning(ex, "UploadBoard: Invalid board state provided.");
                    return BadRequest(ex.Message);
                }
                Guid boardId = _gameService.UploadBoard(board2D);
                _logger.Information("UploadBoard: Board uploaded successfully with Id {BoardId}.", boardId);
                return Ok(boardId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UploadBoard: An error occurred while uploading the board.");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the next state for the specified board.
        /// </summary>
        /// <param name="id">The board Id.</param>
        /// <returns>The next state of the board wrapped in a DTO.</returns>
        [HttpGet("{id}/next")]
        [SwaggerResponseExample(200, typeof(BoardDtoExample))]
        public ActionResult<BoardStateDto> GetNextState(Guid id)
        {
            try
            {
                var nextState2D = _gameService.GetNextState(id);
                var nextStateJagged = ArrayConverter.ConvertToJaggedArray(nextState2D);
                _logger.Information("GetNextState: Retrieved next state for board Id {BoardId}.", id);
                return Ok(new BoardStateDto { Board = nextStateJagged });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "GetNextState: Board Id {BoardId} not found.", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetNextState: An error occurred while retrieving the next state for board Id {BoardId}.", id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the state after a specified number of generations.
        /// </summary>
        /// <param name="id">The board Id.</param>
        /// <param name="steps">Number of generations to simulate.</param>
        /// <returns>The board state after the specified generations, wrapped in a DTO.</returns>
        [HttpGet("{id}/states")]
        [SwaggerResponseExample(200, typeof(BoardDtoExample))]
        public ActionResult<BoardStateDto> GetStateAfterSteps(Guid id, [FromQuery] int steps)
        {
            try
            {
                var state2D = _gameService.GetStateAfterSteps(id, steps);
                var stateJagged = ArrayConverter.ConvertToJaggedArray(state2D);
                _logger.Information("GetStateAfterSteps: Retrieved state after {Steps} steps for board Id {BoardId}.", steps, id);
                return Ok(new BoardStateDto { Board = stateJagged });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warning(ex, "GetStateAfterSteps: Invalid number of steps {Steps} for board Id {BoardId}.", steps, id);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "GetStateAfterSteps: Board Id {BoardId} not found.", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetStateAfterSteps: An error occurred while retrieving the state after {Steps} steps for board Id {BoardId}.", steps, id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the final (stable) state for the specified board.
        /// If a stable state is not reached within the maximum iterations, returns an error.
        /// </summary>
        /// <param name="id">The board Id.</param>
        /// <param name="maxIterations">The maximum number of iterations to simulate.</param>
        /// <returns>The final board state wrapped in a DTO if stable.</returns>
        [HttpGet("{id}/final")]
        [SwaggerResponseExample(200, typeof(BoardDtoExample))]
        public ActionResult<BoardStateDto> GetFinalState(Guid id, [FromQuery] int maxIterations)
        {
            try
            {
                var finalState2D = _gameService.GetFinalState(id, maxIterations);
                var finalStateJagged = ArrayConverter.ConvertToJaggedArray(finalState2D);
                _logger.Information("GetFinalState: Retrieved final state for board Id {BoardId} after {MaxIterations} iterations.", id, maxIterations);
                return Ok(new BoardStateDto { Board = finalStateJagged });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warning(ex, "GetFinalState: Invalid max iterations {MaxIterations} for board Id {BoardId}.", maxIterations, id);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "GetFinalState: Board Id {BoardId} not found.", id);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex, "GetFinalState: Stable state not reached for board Id {BoardId} within {MaxIterations} iterations.", id, maxIterations);
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetFinalState: An error occurred while retrieving the final state for board Id {BoardId}.", id);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
