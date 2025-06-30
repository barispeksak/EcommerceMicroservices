using AutoMapper;
using FluentValidation;
using VariationMicroservice.Data.Entities;
using VariationMicroservice.Data.Repositories;
using VariationMicroservice.Service.DTOs;
using VariationMicroservice.Service.Interfaces;

namespace VariationMicroservice.Service.Services
{
    public class VariationService : IVariationService
    {
        private readonly IVariationRepository _repository;
        private readonly IValidator<CreateVariationDto> _createValidator;
        private readonly IValidator<UpdateVariationDto> _updateValidator;
        private readonly IMapper _mapper;
        private readonly CategoryApiClient _categoryApiClient;

        public VariationService(
            IVariationRepository repository, 
            IMapper mapper, 
            CategoryApiClient categoryApiClient,
            IValidator<CreateVariationDto> createValidator,
            IValidator<UpdateVariationDto> updateValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _categoryApiClient = categoryApiClient;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<VariationDto>> GetAllAsync()
        {
            var variations = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VariationDto>>(variations);
        }

        public async Task<VariationDto> GetAsync(int id)
        {
            var variation = await _repository.GetByIdAsync(id);
            if (variation == null)
            {
                throw new KeyNotFoundException($"Variation {id} not found");
            }
            return _mapper.Map<VariationDto>(variation);
        }

        public async Task<VariationDto> CreateAsync(CreateVariationDto createDto)
        {
            var categoryExists = await _categoryApiClient.CategoryExists(createDto.CategoryId);
            if (!categoryExists)
            {
                throw new Exception("Girilen kategori bulunamadı!");
            }

            await _createValidator.ValidateAndThrowAsync(createDto);

            var variation = _mapper.Map<Variation>(createDto);
            await _repository.AddAsync(variation);
            await _repository.SaveAsync();

            return _mapper.Map<VariationDto>(variation);
        }

        public async Task<bool> UpdateAsync(int id, UpdateVariationDto updateDto)
        {
            var variation = await _repository.GetByIdAsync(id);
            if (variation == null)
            {
                return false;
            }

            await _updateValidator.ValidateAndThrowAsync(updateDto);
            _mapper.Map(updateDto, variation);
            await _repository.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var variation = await _repository.GetByIdAsync(id);
            if (variation == null)
            {
                return false;
            }

            await _repository.DeleteAsync(variation);
            await _repository.SaveAsync();

            return true;
        }
    }
}
