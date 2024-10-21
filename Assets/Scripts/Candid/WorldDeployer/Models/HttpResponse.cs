using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using HeaderField = System.ValueTuple<System.String, System.String>;

namespace Candid.WorldDeployer.Models
{
	public class HttpResponse
	{
		[CandidName("body")]
		public List<byte> Body { get; set; }

		[CandidName("headers")]
		public List<HeaderField> Headers { get; set; }

		[CandidName("status_code")]
		public ushort StatusCode { get; set; }

		public HttpResponse(List<byte> body, List<HeaderField> headers, ushort statusCode)
		{
			this.Body = body;
			this.Headers = headers;
			this.StatusCode = statusCode;
		}

		public HttpResponse()
		{
		}
	}
}