using FluentValidation;
using PLCDataCollector.Model.Classes;
using System.Text.Json;

namespace PLCDataCollector.Model.Validation
{
    public class PlcDataValidator : AbstractValidator<Dictionary<string, object>>
    {
        public PlcDataValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .NotEmpty()
                .WithMessage("PLC data cannot be null or empty");

            RuleFor(x => x)
                .Must(data => data.ContainsKey("ProductionCount"))
                .WithMessage("PLC data must contain ProductionCount");

            RuleFor(x => x)
                .Must(data => data.ContainsKey("PartNumber"))
                .WithMessage("PLC data must contain PartNumber");

            RuleFor(x => x)
                .Must(HaveValidDataTypes)
                .WithMessage("PLC data contains invalid data types");
        }

        private bool HaveValidDataTypes(Dictionary<string, object> data)
        {
            try
            {
                // Validate ProductionCount is a number
                if (data.TryGetValue("ProductionCount", out var count))
                {
                    if (!int.TryParse(count.ToString(), out _))
                        return false;
                }

                // Validate PartNumber is a string
                if (data.TryGetValue("PartNumber", out var partNumber))
                {
                    if (!(partNumber is string))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public class NewPlcDataValidator
    {
        public bool ValidatePlcData(PlcData data)
        {
            if (data == null) return false;
            if (string.IsNullOrEmpty(data.LineId)) return false;
            if (data.Timestamp == default) return false;

            return true;
        }

        public bool ValidateProductionData(ProductionData data)
        {
            if (data == null) return false;
            if (string.IsNullOrEmpty(data.LineId)) return false;
            if (string.IsNullOrEmpty(data.PartNumber)) return false;

            return true;
        }
    }
}