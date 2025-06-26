using Microsoft.AspNetCore.Mvc;
using trendyolApi.Data;
using trendyolApi.Models;
using Microsoft.EntityFrameworkCore;

namespace trendyolApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }


        // GET: api/user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(); // 404 döner
            }

            return user;
        }

        // GETALL: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.Dob = DateTime.SpecifyKind(user.Dob, DateTimeKind.Utc);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/user/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest(); // ID uyuşmuyor
            }

            _context.Entry(updatedUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                {
                    return NotFound(); // Kullanıcı yoksa 404
                }
                throw;
            }

            return NoContent(); // 204: Başarılı ama içerik yok
        }

        // DELETE: api/user/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{userId}/add-address/{addressId}")]
        public async Task<IActionResult> AddAddressToUser(int userId, int addressId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Adres mikroservisini çağır
            var client = _httpClientFactory.CreateClient("AddressService");
            var addressResponse = await client.GetAsync($"api/address/{addressId}");

            // Adres bulunamadıysa kullanıcıya ekleme
            if (!addressResponse.IsSuccessStatusCode)
                return BadRequest($"Adres {addressId} bulunamadı (Mikroservisten).");

            // Zaten varsa tekrar ekleme
            if (!user.AddressIds.Contains(addressId))
                user.AddressIds.Add(addressId);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/with-addresses")]
        public async Task<ActionResult<object>> GetUserWithAddresses(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var client = _httpClientFactory.CreateClient("AddressService");

            var addressTasks = user.AddressIds.Select(addrId =>
                client.GetFromJsonAsync<object>($"api/address/{addrId}")
            );

            var addresses = await Task.WhenAll(addressTasks);

            return new
            {
                user.Id,
                user.Fname,
                user.Lname,
                user.Email,
                user.Phone,
                user.Dob,
                Addresses = addresses
            };
        }

        [HttpDelete("{userId}/remove-address/{addressId}")]
        public async Task<IActionResult> RemoveAddressFromUser(int userId, int addressId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.AddressIds.Contains(addressId))
            {
                user.AddressIds.Remove(addressId);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest($"Adres {addressId} bulunamadı.");
            }

            return NoContent();
        }
    }
}
