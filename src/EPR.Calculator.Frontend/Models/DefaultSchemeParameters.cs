using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class DefaultSchemeParameters
    {
        public int Id { get; set; }

        public required string ParameterYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DefaultParameterSettingMasterId { get; set; }

        public required string ParameterUniqueRef { get; set; }

        public required string ParameterType { get; set; }

        public required string ParameterCategory { get; set; }

        public decimal ParameterValue { get; set; }
    }
}
