using Microsoft.AspNetCore.Mvc;
using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        // ────────────────────────────────────────────────
        // BOŞ SEPET OLUŞTUR – CartId döner
        // ────────────────────────────────────────────────
        /// <summary>
        /// Yeni, boş bir sepet oluşturur ve <c>CartId</c> döner.
        /// </summary>
        /// <remarks>
        /// Frontend ilk alışverişte bu CartId'yi saklamalı ve sonraki çağrılarda path
        /// parametresi olarak göndermelidir.
        /// </remarks>
        [HttpPost]
        [SwaggerOperation(Summary = "Yeni boş sepet oluşturur ve CartId döner")]
        public async Task<ActionResult<int>> CreateCart([FromBody] CreateShoppingCartDto dto)
        {
            // Service, cartId=0 geldiğinde ‘totalRow’ oluşturarak yeni sepet yaratıyor.
            var added = await _shoppingCartService.AddItemAsync(dto, cartId: 0);
            // CartId artık DTO’da var → döndür
            return CreatedAtAction(nameof(GetCart),
                new { cartId = added.CartId }, added);
        }

        // ────────────────────────────────────────────────
        // MEVCUT SEPETE ÜRÜN EKLE
        // ────────────────────────────────────────────────
        /// <summary>
        /// Mevcut sepete yeni ürün ekler. Aynı ürün tekrar eklenirse miktar artırılır.
        /// </summary>
        [HttpPost("{cartId:int}/items")]
        [SwaggerOperation(Summary = "Mevcut sepete ürün ekler")]
        public async Task<ActionResult<ShoppingCartDto>> AddItem(
            int cartId,
            [FromBody] CreateShoppingCartDto dto)
        {
            var added = await _shoppingCartService.AddItemAsync(dto, cartId);
            return Ok(added);
        }

        // ────────────────────────────────────────────────
        // SEPETİ GÖRÜNTÜLE
        // ────────────────────────────────────────────────
        /// <summary>
        /// Belirtilen <c>CartId</c>’deki tüm ürünleri ve toplam fiyatı getirir.
        /// </summary>
        [HttpGet("{cartId:int}")]
        [SwaggerOperation(Summary = "Sepetteki ürünleri ve toplam fiyatı getirir")]
        public async Task<ActionResult<ShoppingCartSummaryDto>> GetCart(int cartId)
        {
            var summary = await _shoppingCartService.GetAllItemsAsync(cartId);
            return Ok(summary);
        }

        // ────────────────────────────────────────────────
        // SEPETTEKİ BELİRLİ ÜRÜNÜN ADETİNİ SORGULA
        // ────────────────────────────────────────────────
        /// <summary>
        /// Sepetteki belirli ürünün toplam adedini getirir.
        /// </summary>
        [HttpGet("{cartId:int}/quantity/{productItemId:int}")]
        [SwaggerOperation(Summary = "Sepetteki belirli ürüne ait adet bilgisini getirir")]
        public async Task<ActionResult<int>> GetQuantity(int cartId, int productItemId)
        {
            int qty = await _shoppingCartService.GetItemQuantityAsync(productItemId, cartId);
            return Ok(qty);
        }


        // ────────────────────────────────────────────────
        // SEPETİ TOPLU GÜNCELLE (aded değiştir vs.)
        // ────────────────────────────────────────────────
        /// <summary>
        /// Sepetteki ürün satırlarını topluca günceller (adet güncelleme vb.).
        /// </summary>
        [HttpPut("{cartId:int}")]
        [SwaggerOperation(Summary = "Sepetteki ürün miktarlarını günceller")]
        public async Task<IActionResult> UpdateCart(
            int cartId,
            [FromBody] UpdateShoppingCartDto dto)
        {
            await _shoppingCartService.UpdateCartAsync(dto, cartId);
            return NoContent();
        }

        // ────────────────────────────────────────────────
        // SEPETTEN ÜRÜN SATIRI SİL
        // ────────────────────────────────────────────────
        /// <summary>
        /// Sepetten belirli bir ürün satırını siler.
        /// </summary>
        [HttpDelete("items/{itemId:int}")]
        [SwaggerOperation(Summary = "Sepetten tek bir ürün satırını siler")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            await _shoppingCartService.DeleteItemAsync(itemId);
            return NoContent();
        }
    }
}
