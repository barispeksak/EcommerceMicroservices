using AutoMapper;
using AddressMicroservice.Data.Entities;
using AddressMicroservice.Data.Repositories;
using AddressMicroservice.Service.DTOs;
using AddressMicroservice.Service.Interfaces;

namespace AddressMicroservice.Service.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repository;
        private readonly IMapper _mapper;

        public AddressService(IAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AddressDto>> GetAllAsync()
        {
            var addresses = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }

        public async Task<AddressDto?> GetByIdAsync(int id)
        {
            var address = await _repository.GetByIdAsync(id);
            return address != null ? _mapper.Map<AddressDto>(address) : null;
        }

        public async Task<AddressDto> CreateAsync(CreateAddressDto createAddressDto)
        {
            var address = _mapper.Map<Address>(createAddressDto);
            var createdAddress = await _repository.AddAsync(address);
            await _repository.SaveChangesAsync();
            
            return _mapper.Map<AddressDto>(createdAddress);
        }

        public async Task<AddressDto?> UpdateAsync(int id, UpdateAddressDto updateAddressDto)
        {
            var address = await _repository.GetByIdAsync(id);
            if (address == null)
                return null;

            _mapper.Map(updateAddressDto, address);
            await _repository.UpdateAsync(address);
            await _repository.SaveChangesAsync();
            
            return _mapper.Map<AddressDto>(address);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var address = await _repository.GetByIdAsync(id);
            if (address == null)
                return false;

            await _repository.DeleteAsync(address);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}