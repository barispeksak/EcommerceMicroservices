using AutoMapper;
using Variation_OptionMicroservice.Data.Entities;
using Variation_OptionMicroservice.Data.Repositories;
using Variation_OptionMicroservice.Service.DTOs;
using Variation_OptionMicroservice.Service.Interfaces;

namespace Variation_OptionMicroservice.Service.Services
{
    public class VariationOptionService : IVariationOptionService
    {
        private readonly IVariationOptionRepository _repository;
        private readonly IMapper _mapper;

        public VariationOptionService(IVariationOptionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VariationOptionDto>> GetAllOptionsAsync()
        {
            var options = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VariationOptionDto>>(options);
        }

        public async Task<VariationOptionDto?> GetOptionByIdAsync(int id)
        {
            var option = await _repository.GetByIdAsync(id);
            return option == null ? null : _mapper.Map<VariationOptionDto>(option);
        }

        public async Task<IEnumerable<VariationOptionDto>> GetOptionsByVariationIdAsync(int variationId)
        {
            var options = await _repository.GetByVariationIdAsync(variationId);
            return _mapper.Map<IEnumerable<VariationOptionDto>>(options);
        }

        public async Task<VariationOptionDto> CreateOptionAsync(CreateVariationOptionDto createDto)
        {
            // Varyasyon var mı kontrol et
            var variationExists = await _repository.VariationExistsAsync(createDto.VariationId);
            if (!variationExists)
            {
                throw new InvalidOperationException($"VariationId {createDto.VariationId} bulunamadı.");
            }

            var option = _mapper.Map<VariationOption>(createDto);
            var createdOption = await _repository.CreateAsync(option);
            return _mapper.Map<VariationOptionDto>(createdOption);
        }

        public async Task<bool> UpdateOptionAsync(UpdateVariationOptionDto updateDto)
        {
            var exists = await _repository.ExistsAsync(updateDto.Id);
            if (!exists) return false;

            // Varyasyon var mı kontrol et
            var variationExists = await _repository.VariationExistsAsync(updateDto.VariationId);
            if (!variationExists)
            {
                throw new InvalidOperationException($"VariationId {updateDto.VariationId} bulunamadı.");
            }

            var option = _mapper.Map<VariationOption>(updateDto);
            await _repository.UpdateAsync(option);
            return true;
        }

        public async Task<bool> DeleteOptionAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}