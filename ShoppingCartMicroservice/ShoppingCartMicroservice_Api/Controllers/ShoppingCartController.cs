using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ShoppingCartMicroservice_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        /// <summary>
        /// Sepete ürün ekler veya var olan ürünün adedini günceller.
        /// </summary>
        /// <param name="dto">Sepete eklenecek ürün bilgileri</param>
        /// <returns>Başarı veya hata mesajı</returns>
        [HttpPost("item")]
        [SwaggerOperation(Summary = "Sepete ürün ekle/güncelle", Description = "Kullanıcı kendi sepetine ürün ekler veya miktar günceller.")]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CreateShoppingCartDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await _shoppingCartService.AddOrUpdateItemAsync(userId, dto);
                return Ok(new { success = true, message = "Ürün başarıyla sepete eklendi!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Sepetten ürün siler.
        /// </summary>
        /// <param name="productItemId">Silinecek ürünün ID'si</param>
        /// <returns>Başarı mesajı</returns>
        [HttpDelete("item/{productItemId}")]
        [SwaggerOperation(Summary = "Sepetten ürün siler", Description = "Kullanıcı kendi sepetinden bir ürünü siler.")]
        public async Task<IActionResult> RemoveItem(int productItemId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _shoppingCartService.RemoveItemAsync(userId, productItemId);
            return Ok(new { success = true, message = "Ürün başarıyla sepetten silindi!" });
        }

        /// <summary>
        /// Sepeti temizler.
        /// </summary>
        /// <remarks>
        /// Bu endpoint, giriş yapmış kullanıcının kendi sepetini tamamen temizler.  
        /// Sepetteki tüm ürünler silinir ve sepet boş hale gelir.
        /// </remarks>
        /// <returns>
        /// Başarı durumunda, işlemin başarılı olduğunu belirten mesaj döner.
        /// </returns>
        [HttpDelete("clear")]
        [SwaggerOperation(Summary = "Sepeti temizler", Description = "Kullanıcı kendi sepetini tamamen temizler.")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _shoppingCartService.ClearAsync(userId);
            return Ok(new { success = true, message = "Sepet başarıyla temizlendi!" });
        }

        /// <summary>
        /// Kullanıcıya ait detaylı sepet bilgisini getirir.
        /// </summary>
        /// <remarks>
        /// Sadece giriş yapmış kullanıcının kendi sepetindeki ürünlerin tam detaylarını döner.  
        /// Ürünlerin adı, görseli, fiyatı, stok durumu ve sepetteki miktar bilgileriyle birlikte.
        /// </remarks>
        /// <returns>
        /// Sepetteki ürünlerin detaylarını içeren liste döner.
        /// </returns>
        [HttpGet("details")]
        [SwaggerOperation(Summary = "Kullanıcıya ait detaylı sepet bilgisi", Description = "Sadece giriş yapmış kullanıcının kendi sepetini, ürünlerin tam detaylarıyla döner.")]
        public async Task<IActionResult> GetCartDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var details = await _shoppingCartService.GetCartDetailsForUser(userId);
            return Ok(details);
        }
    }
}
