using System.Globalization;
using EchoSphere.SharedModels.Extensions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace EchoSphere.SharedModels.LinqToDb.Extensions;

public static class MappingSchemaExtensions
{
	public static MappingSchema AddGuidIdValueConverter<T>(this MappingSchema mappingSchema)
		where T : IIdValue<Guid>, new()
	{
		mappingSchema.SetConvertExpression<T, DataParameter>(x => new DataParameter(string.Empty, x.Value, DataType.Guid));
		mappingSchema.SetConvertExpression<Guid, T>(x => new T { Value = x });
		mappingSchema.SetValueToSqlConverter(typeof(T), (sb, _, p) =>
			sb.Append(CultureInfo.InvariantCulture, $"'{((T)p).ToInnerString()}'"));

		return mappingSchema;
	}

	public static MappingSchema AddLongIdValueConverter<T>(this MappingSchema mappingSchema)
		where T : IIdValue<long>, new()
	{
		mappingSchema.SetConvertExpression<T, DataParameter>(x => new DataParameter(string.Empty, x.Value, DataType.Int64));
		mappingSchema.SetConvertExpression<long, T>(x => new T { Value = x });
		mappingSchema.SetValueToSqlConverter(typeof(T), (sb, _, p) =>
			sb.Append(((T)p).Value.ToString(CultureInfo.InvariantCulture)));

		return mappingSchema;
	}
}