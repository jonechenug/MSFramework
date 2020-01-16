using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class ActionResultTypeMapper : IActionResultTypeMapper
	{
		public Type GetResultDataType(Type returnType)
		{
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));
			return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>)
				? returnType.GetGenericArguments()[0]
				: returnType;
		}

		public IActionResult Convert(object value, Type returnType)
		{
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));
			if (value is IConvertToActionResult convertToActionResult)
				return convertToActionResult.Convert();
			return new JsonResult(new ApiResult(value));
		}
	}
}