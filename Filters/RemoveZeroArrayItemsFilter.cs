namespace api_aggregations.Filters;

using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RemoveZeroArrayItemsFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not ObjectResult objectResult || objectResult.Value is null)
        {
            return;
        }

        if (objectResult.Value is IList list)
        {
            RemoveZeroItemsFromList(list);
            return;
        }

        RemoveZeroItemsFromObject(objectResult.Value);
    }

    private static void RemoveZeroItemsFromObject(object value)
    {
        // This looks at every public property in the object.
        // Example: meses, produtos, Items, totalCount, etc.
        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var propertyValue = property.GetValue(value);

            if (propertyValue is IList list)
            {
                RemoveZeroItemsFromList(list);
                continue;
            }

            if (propertyValue is not null && IsSimpleType(propertyValue.GetType()) == false)
            {
                RemoveZeroItemsFromObject(propertyValue);
            }
        }
    }

    private static void RemoveZeroItemsFromList(IList list)
    {
        // Go from the end to the start so removing items does not skip any item.
        for (var index = list.Count - 1; index >= 0; index--)
        {
            var item = list[index];

            if (item is null)
            {
                continue;
            }

            if (ItemHasZeroField(item))
            {
                list.RemoveAt(index);
                continue;
            }

            RemoveZeroItemsFromObject(item);
        }
    }

    private static bool ItemHasZeroField(object item)
    {
        // Only these main fields decide if an array item should be removed.
        // Values like valor, duracao, totalValorMes can be 0 and still be valid.
        var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead ||
                property.GetIndexParameters().Length > 0 ||
                IsFieldThatCanRemoveItem(property.Name) == false)
            {
                continue;
            }

            var value = property.GetValue(item);

            if (IsZero(value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFieldThatCanRemoveItem(string propertyName)
    {
        var name = propertyName.ToLowerInvariant();

        return name == "id" ||
            name == "ano" ||
            name == "mes" ||
            name == "dia" ||
            name == "id_vendedor" ||
            name == "id_entidade";
    }

    private static bool IsZero(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is string text)
        {
            return text == "0";
        }

        var type = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

        if (type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal))
        {
            return Convert.ToDecimal(value) == 0;
        }

        return false;
    }

    private static bool IsSimpleType(Type type)
    {
        var realType = Nullable.GetUnderlyingType(type) ?? type;

        return realType.IsPrimitive ||
            realType == typeof(string) ||
            realType == typeof(decimal) ||
            realType == typeof(DateTime);
    }
}
