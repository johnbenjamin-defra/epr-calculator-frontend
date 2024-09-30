﻿using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateLapcapDataErrorDto : ErrorDto
    {
        public required string UniqueReference { get; set; }

        public required string Country { get; set; }

        public required string Material { get; set; }
    }
}
