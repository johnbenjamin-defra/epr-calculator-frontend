﻿using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingErrorDto : ErrorDto
    {
        public string ParameterUniqueRef { get; set; }

        public string ParameterCategory { get; set; }

        public string ParameterType { get; set; }
    }
}
