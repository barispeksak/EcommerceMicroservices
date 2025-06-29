using Microsoft.AspNetCore.Mvc;
using AddressMicroservice.Service.DTOs;
using AddressMicroservice.Service.Interfaces;

namespace AddressMicroservice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // GET: api/address
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll()
        {
            var addresses = await _addressService.GetAllAsync();
            return Ok(addresses);
        }

        // GET: api/address/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDto>> Get(int id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null)
            {
                return NotFound($"Address with ID {id} not found");
            }
            return Ok(address);
        }

        // POST: api/address
        [HttpPost]
        public async Task<ActionResult<AddressDto>> Post([FromBody] CreateAddressDto createAddressDto)
        {
            // FluentValidation will automatically validate and return 400 if invalid
            var address = await _addressService.CreateAsync(createAddressDto);
            return CreatedAtAction(nameof(Get), new { id = address.Id }, address);
        }

        // PUT: api/address/5
        [HttpPut("{id}")]
        public async Task<ActionResult<AddressDto>> Put(int id, [FromBody] UpdateAddressDto updateAddressDto)
        {
            // FluentValidation will automatically validate and return 400 if invalid
            var address = await _addressService.UpdateAsync(id, updateAddressDto);
            if (address == null)
            {
                return NotFound($"Address with ID {id} not found");
            }

            return Ok(address);
        }

        // DELETE: api/address/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null)
            {
                return NotFound($"Address with ID {id} not found");
            }

            await _addressService.DeleteAsync(id);
            return NoContent();
        }
    }
}