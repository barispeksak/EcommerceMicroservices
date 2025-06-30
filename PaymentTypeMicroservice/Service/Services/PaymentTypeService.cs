// Services/PaymentTypeService.cs
using AutoMapper;
using PaymentTypeMicroservice.Data.Dtos;
using PaymentTypeMicroservice.Data.Repositories;
// using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Entities;
using PaymentTypeMicroservice.Services.Interfaces;

namespace PaymentTypeMicroservice.Services
{
    public class PaymentTypeService : IPaymentTypeService
    {
        private readonly IPaymentTypeRepository _repository;
        private readonly IMapper _mapper;

        public PaymentTypeService(IPaymentTypeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentTypeDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentTypeDto>>(list);
        }

        public async Task<PaymentTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<PaymentTypeDto>(entity);
        }

        public async Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeDto dto)
        {
            var entity = _mapper.Map<PaymentType>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<PaymentTypeDto>(created);
        }

        public async Task<bool> UpdateAsync(UpdatePaymentTypeDto dto)
        {
            var entity = _mapper.Map<PaymentType>(dto);
            return await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
            => await _repository.DeleteAsync(id);
    }
}
