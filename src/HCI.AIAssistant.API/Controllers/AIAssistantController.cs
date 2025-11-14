using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using HCI.AIAssistant.API.Models.DTOs.AIAssistantController;

namespace HCI.AIAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIAssistantController : ControllerBase
    {
        [HttpPost("/message")]
        public async Task<ActionResult<AIAssistantControllerPostMessageResponseDTO>> 
            PostMessageAsync([FromBody] AIAssistantControllerPostMessageRequestDTO request)
        {
            var response = new AIAssistantControllerPostMessageResponseDTO
            {
                TextMessage = "Hi"
            };

            return Ok(response);
        }
    }
}
