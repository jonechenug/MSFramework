using System;

namespace Template.Application.DTO
{
	public class ProductOut
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public DateTimeOffset CreationTime { get; set; }
		public string CreationUserName { get; set; }
	}
}