using System.Text.Json;
using AmazonSQS.DTO;
using AmazonSQS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmazonSQS.Controllers
{
    [ApiController]
    [Route("Api/[controller]")]
    [Produces("application/json")]
    public class MessageQueueController : ControllerBase
    {
        private readonly IMessageQueueService _messageQueueService;

        public MessageQueueController(IMessageQueueService MessageQueueService)
        {
            _messageQueueService = MessageQueueService;
        }

        /// <summary>
        /// Enqueue um webhook na fila.
        /// </summary>
        /// <param name="dto">Objeto contendo a URL e o Payload.</param>
        /// <returns>Confirmação de que o webhook foi enfileirado.</returns>
        /// <response code="200">Webhook enfileirado com sucesso</response>
        /// <response code="400">Url inválida ou vazia</response>
        [HttpPost("Send")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Send([FromBody] SendWebhookDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Url))
                return BadRequest("Url cannot be empty.");

            var messageJson = JsonSerializer.Serialize(dto);

            await _messageQueueService.EnqueueAsync(messageJson);

            return Ok(new { Message = "Webhook enviado com sucesso para a fila" });
        }
    }
}
