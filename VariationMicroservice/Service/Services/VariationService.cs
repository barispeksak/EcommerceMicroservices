using AutoMapper;
using VariationMicroservice.Data.Entities;
using VariationMicroservice.Data.Repositories;
using VariationMicroservice.Service.DTOs;
using VariationMicroservice.Service.Interfaces;

namespace VariationMicroservice.Service.Services
{
    public class VariationService : IVariationService
    {
        private readonly IVariationRepository _repository;
        private readonly IMapper _mapper;

        public VariationService(IVariationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VariationDto>> GetAllVariationsAsync()
        {
            var variations = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VariationDto>>(variations);
        }

        public async Task<VariationDto?> GetVariationByIdAsync(int id)
        {
            var variation = await _repository.GetByIdAsync(id);
            return variation == null ? null : _mapper.Map<VariationDto>(variation);
        }

        public async Task<VariationDto> CreateVariationAsync(CreateVariationDto createDto)
        {
            // Kategori var mı kontrol et
            var categoryExists = await _context.ProductCategories.AnyAsync(c => c.Id == createDto.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"CategoryId {createDto.CategoryId} bulunamadı.");
            }
            
            var variation = _mapper.Map<Variation>(createDto);
            var createdVariation = await _repository.CreateAsync(variation);
            return _mapper.Map<VariationDto>(createdVariation);
        }

        public async Task<bool> UpdateVariationAsync(UpdateVariationDto updateDto)
        {
            var exists = await _repository.ExistsAsync(updateDto.Id);
            if (!exists) return false;

            var variation = _mapper.Map<Variation>(updateDto);
            await _repository.UpdateAsync(variation);
            return true;
        }

        public async Task<bool> DeleteVariationAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}