using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using KatlaSport.Services.HiveManagement;
using KatlaSport.WebApi.CustomFilters;
using Microsoft.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace KatlaSport.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/hives")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class HivesController : ApiController
    {
        private readonly IHiveService _hiveService;
        private readonly IHiveSectionService _hiveSectionService;

        public HivesController(IHiveService hiveService, IHiveSectionService hiveSectionService)
        {
            _hiveService = hiveService ?? throw new ArgumentNullException(nameof(hiveService));
            _hiveSectionService = hiveSectionService ?? throw new ArgumentNullException(nameof(hiveSectionService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hives.", Type = typeof(HiveListItem[]))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHives()
        {
            var hives = await _hiveService.GetHivesAsync();
            return Ok(hives);
        }

        [HttpGet]
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a hive.", Type = typeof(Hive))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHive(int hiveId)
        {
            var hive = await _hiveService.GetHiveAsync(hiveId);
            return Ok(hive);
        }

        [HttpGet]
        [Route("{hiveId:int:min(1)}/sections")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hive sections for specified hive.", Type = typeof(HiveSectionListItem))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHiveSections(int hiveId)
        {
            var hive = await _hiveSectionService.GetHiveSectionsAsync(hiveId);
            return Ok(hive);
        }

        [HttpPut]
        [Route("{hiveId:int:min(1)}/status/{deletedStatus:bool}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Sets deleted status for an existed hive.")]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> SetStatus([FromUri] int hiveId, [FromUri] bool deletedStatus)
        {
            await _hiveService.SetStatusAsync(hiveId, deletedStatus);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new hive.", Type = typeof(Hive))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid request data or validation failure.")]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Hive with the same code already exists.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.")]
        public async Task<IHttpActionResult> AddHive([FromBody] UpdateHiveRequest request)
        {
            if(request == null)
            {
                return BadRequest("Invalid request data");
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hive = await _hiveService.CreateHiveAsync(request);
            var location = $"{Request.RequestUri}/{hive.Id}";
            var response = Request.CreateResponse(HttpStatusCode.Created, hive);
            response.Headers.Location = new Uri(location);
            return ResponseMessage(response);
        }

        [HttpPut]
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid request data or validation failure.")]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Hive with the same code already exists.")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive not found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.")]
        public async Task<IHttpActionResult> UpdateHive([FromUri] int hiveId, [FromBody] UpdateHiveRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingHive = await _hiveService.GetHiveAsync(hiveId);
            if (existingHive == null)
            {
                return NotFound();
            }

            await _hiveService.UpdateHiveAsync(hiveId, request);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        [HttpDelete]
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid hive ID.")]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Hive cannot be deleted.")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive not found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.")]
        public async Task<IHttpActionResult> DeleteHive([FromUri] int hiveId)
        {
            if (hiveId < 1)
            {
                return BadRequest();
            }

            var existingHive = await _hiveService.GetHiveAsync(hiveId);
            if (existingHive == null)
            {
                return NotFound();
            }

            await _hiveService.DeleteHiveAsync(hiveId);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
