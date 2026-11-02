using FluentValidation;
using ProductsInfo.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Validators
{
    public sealed class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Наименование товара обязательно.")
                .MaximumLength(200)
                .WithMessage("Наименование не должно превышать 200 символов.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Цена не может быть отрицательной.");

            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .WithMessage("Вес должен быть больше нуля.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Указан неизвестный вид товара.");

            RuleFor(x => x.WarehouseNumber)
                .GreaterThan(0)
                .WithMessage("Номер склада должен быть положительным.");
        }
    }
}
