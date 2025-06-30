using AutoMapper;
using PaymentTypeMicroservice.Entities;
using PaymentTypeMicroservice.Data.Dtos;

namespace PaymentTypeMicroservice.Mapping
{
    public class PaymentTypeProfile : Profile
    {
        public PaymentTypeProfile()
        {
            CreateMap<PaymentType, PaymentTypeDto>().ReverseMap();
            CreateMap<PaymentType, CreatePaymentTypeDto>().ReverseMap();
            CreateMap<PaymentType, UpdatePaymentTypeDto>().ReverseMap();
        }
    }
}

