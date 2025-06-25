using Microsoft.EntityFrameworkCore;
using AddressMicroservice.Data;
using AddressMicroservice.DTOs;
using AddressMicroservice.Models;

namespace AddressMicroservice.Services
{
    public class AddressService : IAddressService
    {
        private readonly AddressDbContext _context;

        public AddressService(AddressDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AddressDto>> GetAllAsync()
        {
            var addresses = await _context.Addresses
                .OrderBy(a => a.Id)
                .ToListAsync();

            return addresses.Select(MapToDto);
        }

        public async Task<AddressDto?> GetByIdAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            return address != null ? MapToDto(address) : null;
        }

        public async Task<AddressDto> CreateAsync(CreateAddressDto createAddressDto)
        {
            var address = new Address
            {
                AddressLine = createAddressDto.AddressLine,
                City = createAddressDto.City,
                Phone = createAddressDto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return MapToDto(address);
        }

        public async Task<AddressDto?> UpdateAsync(int id, UpdateAddressDto updateAddressDto)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                return null;

            address.AddressLine = updateAddressDto.AddressLine;
            address.City = updateAddressDto.City;
            address.Phone = updateAddressDto.Phone;

            await _context.SaveChangesAsync();
            return MapToDto(address);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Addresses.AnyAsync(a => a.Id == id);
        }

        private static AddressDto MapToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                AddressLine = address.AddressLine,
                City = address.City,
                Phone = address.Phone,
                CreatedAt = address.CreatedAt
            };
        }
    }
}