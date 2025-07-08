using AutoMapper;
using FluentValidation;
using VariationOptionMicroservice.Data.Entities;
using VariationOptionMicroservice.Data.Repositories;
using VariationOptionMicroservice.Service.DTOs;
using VariationOptionMicroservice.Service.Interfaces;

namespace VariationOptionMicroservice.Service.Services
{
    public class VariationOptionService : IVariationOptionService
    {
        private readonly IVariationOptionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateVariationOptionDto> _createValidator;
        private readonly IValidator<UpdateVariationOptionDto> _updateValidator;
        private readonly CategoryApiClient _categoryApiClient;

        public VariationOptionService(
            IVariationOptionRepository repository,
            IMapper mapper,
            IValidator<CreateVariationOptionDto> createValidator,
            IValidator<UpdateVariationOptionDto> updateValidator,
            CategoryApiClient categoryApiClient)
        {
            _repository = repository;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _categoryApiClient = categoryApiClient;
        }

        public async Task<IEnumerable<VariationOptionDto>> GetAllAsync()
        {
            var options = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VariationOptionDto>>(options);
        }

        public async Task<VariationOptionDto> GetAsync(int id)
        {
            var option = await _repository.GetByIdAsync(id);
            if (option == null)
                throw new KeyNotFoundException($"VariationOption {id} not found");

            return _mapper.Map<VariationOptionDto>(option);
        }

        public async Task<VariationOptionDto> CreateAsync(CreateVariationOptionDto createDto)
        {
            await _createValidator.ValidateAndThrowAsync(createDto);

            // 🔍 Variation ID kontrolü
            var variationExists = await _categoryApiClient.VariationExists(createDto.VariationId);
            if (!variationExists)
                throw new ArgumentException($"Variation with id {createDto.VariationId} does not exist.");

            var entity = _mapper.Map<VariationOption>(createDto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();

            return _mapper.Map<VariationOptionDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateVariationOptionDto updateDto)
        {
            var option = await _repository.GetByIdAsync(id);
            if (option == null)
                return false;

            await _updateValidator.ValidateAndThrowAsync(updateDto);

            // 🔍 Variation ID kontrolü (isteğe bağlı; eğer updateDto içinde VariationId varsa)
            var variationExists = await _categoryApiClient.VariationExists(updateDto.VariationId);
            if (!variationExists)
                throw new ArgumentException($"Variation with id {updateDto.VariationId} does not exist.");

            _mapper.Map(updateDto, option);
            await _repository.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var option = await _repository.GetByIdAsync(id);
            if (option == null)
                return false;

            await _repository.DeleteAsync(option);
            await _repository.SaveAsync();

            return true;
        }
    }
}