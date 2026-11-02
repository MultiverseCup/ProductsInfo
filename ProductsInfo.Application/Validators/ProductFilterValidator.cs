using FluentValidation;
using ProductsInfo.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Validators
{
    public sealed class ProductFilterValidator
    : AbstractValidator<ProductFilter>
    {
        public ProductFilterValidator()
        {
            RuleFor(x => x.Page)
                .Must((filter, page) =>
                    page > 0 &&
                    ((long)page - 1) * filter.PageSize <= int.MaxValue)
                .WithMessage("Номер страницы должен быть положительным.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Размер страницы должен быть от 1 до 100.");

            RuleFor(x => x.WarehouseNumber)
                .Must(number => !number.HasValue || number.Value > 0)
                .WithMessage("Номер склада должен быть положительным.");

            RuleFor(x => x.Type)
                .Must(type =>
                    !type.HasValue ||
                    Enum.IsDefined(type.Value))
                .WithMessage("Указан неизвестный вид товара.");
        }
    }
}
