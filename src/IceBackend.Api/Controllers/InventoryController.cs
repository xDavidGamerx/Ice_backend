using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IceBackend.Application.UseCases.Inventory;
using IceBackend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceBackend.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly EquipCosmeticUseCase _equipUseCase;
        private readonly UnequipCosmeticUseCase _unequipUseCase;

        public InventoryController(
            EquipCosmeticUseCase equipUseCase,
            UnequipCosmeticUseCase unequipUseCase)
        {
            _equipUseCase = equipUseCase;
            _unequipUseCase = unequipUseCase;
        }

        public class EquipRequest
        {
            public Guid CosmeticId { get; set; }
            public CosmeticType Slot { get; set; }
        }

        public class UnequipRequest
        {
            public CosmeticType Slot { get; set; }
        }

        /// <summary>
        /// Equipa un cosmético en el slot indicado.
        /// Requiere header X-Client-Architecture (Legacy o Modern).
        /// </summary>
        [HttpPost("equip")]
        public async Task<IActionResult> Equip([FromBody] EquipRequest request)
        {
            var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (playerIdClaim == null || !Guid.TryParse(playerIdClaim.Value, out var playerId))
                return Unauthorized();

            var result = await _equipUseCase.ExecuteAsync(playerId, request.CosmeticId, request.Slot);

            return result switch
            {
                EquipResult.Success => Ok(new { message = "Cosmetic equipped successfully" }),
                EquipResult.ArchitectureNotProvided => BadRequest(new { message = "Header X-Client-Architecture is required" }),
                EquipResult.NotOwned => StatusCode(403, new { message = "You do not own this cosmetic" }),
                EquipResult.CosmeticNotFound => NotFound(new { message = "Cosmetic not found" }),
                EquipResult.ArchitectureIncompatible => BadRequest(new { message = "Cosmetic is not compatible with your client architecture" }),
                EquipResult.SlotMismatch => BadRequest(new { message = "Cosmetic type does not match the requested slot" }),
                EquipResult.PlayerNotFound => NotFound(new { message = "Player not found" }),
                _ => StatusCode(500, new { message = "Unexpected error" })
            };
        }

        /// <summary>
        /// Desequipa el cosmético del slot indicado, dejándolo vacío.
        /// </summary>
        [HttpPost("unequip")]
        public async Task<IActionResult> Unequip([FromBody] UnequipRequest request)
        {
            var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (playerIdClaim == null || !Guid.TryParse(playerIdClaim.Value, out var playerId))
                return Unauthorized();

            var success = await _unequipUseCase.ExecuteAsync(playerId, request.Slot);

            return success
                ? Ok(new { message = "Slot cleared successfully" })
                : NotFound(new { message = "Player not found" });
        }
    }
}
