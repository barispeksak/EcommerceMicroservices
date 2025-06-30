// Services/ShippingTypeService.cs
using AutoMapper;
using ShippingTypeMicroservice.Data.Dtos;
using ShippingTypeMicroservice.Data.Repositories;
using ShippingTypeMicroservice.Entities;
using ShippingTypeMicroservice.Services.Interfaces;

namespace ShippingTypeMicroservice.Services
{
    public class ShippingTypeService : IShippingTypeService
    {
        private readonly IShippingTypeRepository _repository;
        private readonly IMapper _mapper;

        public ShippingTypeService(IShippingTypeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShippingTypeDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShippingTypeDto>>(list);
        }

        public async Task<ShippingTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ShippingTypeDto>(entity);
        }

        public async Task<ShippingTypeDto> CreateAsync(CreateShippingTypeDto dto)
        {
            var entity = _mapper.Map<ShippingType>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<ShippingTypeDto>(created);
        }

        public async Task<bool> UpdateAsync(UpdateShippingTypeDto dto)
        {
            var entity = _mapper.Map<ShippingType>(dto);
            return await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
            => await _repository.DeleteAsync(id);
    }
}
