using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace SILab.EntityFrameworkCore.ValueConverters
{
    internal class DateTimeValueConverter : ValueConverter<DateTime?, DateTime?>
    {
        public DateTimeValueConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(Normalize, Normalize, mappingHints)
        {
        }

        private static readonly Expression<Func<DateTime?, DateTime?>> Normalize = x =>
            x.HasValue ? x.Value : x;
    }
}
