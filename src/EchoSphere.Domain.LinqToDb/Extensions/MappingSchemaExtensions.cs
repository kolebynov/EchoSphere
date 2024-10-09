using System.Globalization;
using System.Text;
using EchoSphere.Domain.Abstractions.Models;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace EchoSphere.Domain.LinqToDb.Extensions;

public static class MappingSchemaExtensions
{
	private static readonly Dictionary<Type, object> ValueConverterInfos = new()
	{
		{
			typeof(Guid),
			new ValueConverterInfo<Guid>
			{
				DataType = DataType.Guid,
				ValueToSqlConverter = (sb, v) => sb.Append(CultureInfo.InvariantCulture, $"'{v}'"),
			}
		},
		{
			typeof(long),
			new ValueConverterInfo<long>
			{
				DataType = DataType.Int64,
				ValueToSqlConverter = (sb, v) => sb.Append(v.ToString(CultureInfo.InvariantCulture)),
			}
		},
	};

	public static MappingSchema AddIdValueConverter<TValue, T>(this MappingSchema mappingSchema)
		where T : IIdValue<TValue>, new()
	{
		var valueInfo = (ValueConverterInfo<TValue>)ValueConverterInfos[typeof(TValue)];
		mappingSchema.SetConvertExpression<T, DataParameter>(x =>
			new DataParameter(string.Empty, x.Value, valueInfo.DataType));
		mappingSchema.SetConvertExpression<TValue, T>(x => new T { Value = x });
		mappingSchema.SetValueToSqlConverter(typeof(T), (sb, _, p) => valueInfo.ValueToSqlConverter(sb, ((T)p).Value));

		return mappingSchema;
	}

	private sealed class ValueConverterInfo<TValue>
	{
		public required DataType DataType { get; init; }

		public required Action<StringBuilder, TValue> ValueToSqlConverter { get; init; }
	}
}