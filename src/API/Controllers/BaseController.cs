using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.API.Controllers
{
    public class BaseController
    {
        [ApiController]
        [Route("api/[controller]")]
        public class BaseApiController : ControllerBase { }
    }
}