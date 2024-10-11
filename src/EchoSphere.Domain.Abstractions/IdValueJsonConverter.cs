using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Domain.Abstractions;

internal sealed class IdValueJsonConverter<TValue> : JsonConverter<IIdValue<TValue>>
{
	private readonly ConcurrentDictionary<Type, Func<TValue, IIdValue<TValue>>> _idValueConstructors = new();

	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert.IsAssignableTo(typeof(IIdValue<TValue>));

	public override IIdValue<TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
		var idValueConstructor = _idValueConstructors.GetOrAdd(typeToConvert, CreateIdValueConstructor);
		return idValueConstructor(value);
	}

	public override void Write(Utf8JsonWriter writer, IIdValue<TValue> value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Value, options);
	}

	private static Func<TValue, IIdValue<TValue>> CreateIdValueConstructor(Type type)
	{
		var valueProperty = type.GetProperty(nameof(IIdValue<TValue>.Value), BindingFlags.Public | BindingFlags.Instance)!;

		var parameterExpression = Expression.Parameter(typeof(TValue));
		var constructor = type.GetConstructors()
			.First(x =>
			{
				var parameters = x.GetParameters();
				return parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(TValue));
			});

		Expression constructExpression = constructor.GetParameters().Length == 0
			? Expression.MemberInit(Expression.New(constructor), Expression.Bind(valueProperty, parameterExpression))
			: Expression.New(constructor, [parameterExpression]);
		var convertExpression = Expression.Convert(constructExpression, typeof(IIdValue<TValue>));

		return Expression.Lambda<Func<TValue, IIdValue<TValue>>>(convertExpression, [parameterExpression]).Compile();
	}
}