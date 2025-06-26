using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly ShopOrderDbContext _context;

        public PaymentTypeController(ShopOrderDbContext context)
        {
            _context = context;
        }

        // GET: api/paymenttype
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentType>>> GetAll()
        {
            return await _context.PaymentTypes.ToListAsync();
        }

        // GET: api/paymenttype/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentType>> GetById(int id)
        {
            var paymentType = await _context.PaymentTypes.FindAsync(id);
            if (paymentType == null)
                return NotFound();

            return paymentType;
        }

        // POST: api/paymenttype
        [HttpPost]
        public async Task<ActionResult<PaymentType>> Create(PaymentType paymentType)
        {
            _context.PaymentTypes.Add(paymentType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = paymentType.Id }, paymentType);
        }

        // DELETE: api/paymenttype/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var paymentType = await _context.PaymentTypes.FindAsync(id);
            if (paymentType == null)
                return NotFound();

            _context.PaymentTypes.Remove(paymentType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
