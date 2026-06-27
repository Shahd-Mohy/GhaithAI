using GhaithAI.GaithAI.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestExceptionController : ControllerBase
    {
        [HttpGet("not-found")]
        public IActionResult TestNotFound() => throw new NotFoundException("Test: ÍÇÌÉ ãÔ ãæÌæÏÉ");

        [HttpGet("forbidden")]
        public IActionResult TestForbidden() => throw new ForbiddenException("Test: ããäæÚ ÊÏÎá åäÇ");

        [HttpGet("bad-request")]
        public IActionResult TestBadRequest() => throw new BadRequestException("Test: ÈíÇäÇÊ ÛáØ");

        [HttpGet("conflict")]
        public IActionResult TestConflict() => throw new ConflictException("Test: ÊÚÇÑÖ İí ÇáÈíÇäÇÊ");

        [HttpGet("server-error")]
        public IActionResult TestServerError() => throw new InvalidOperationException("Test: ÎØÃ ÓíÑİÑ ÚÇÏí");

        [HttpGet("unhandled")]
        public IActionResult TestUnhandled()
        {
            string? s = null;
            return Ok(s!.Length);  // NullReferenceException ãä ÛíÑ ŞÕÏ - ÚÔÇä ÊÊÃßÏ ÇáÜ 500 ÔÛÇáÉ
        }
    }
}
