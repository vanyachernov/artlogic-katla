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
    [RoutePrefix("api/sections")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class HiveSectionsController : ApiController
    {
        private readonly IHiveSectionService _hiveSectionService;

        public HiveSectionsController(IHiveSectionService hiveSectionService)
        {
            _hiveSectionService = hiveSectionService ?? throw new ArgumentNullException(nameof(hiveSectionService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hive sections.", Type = typeof(HiveSectionListItem[]))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHiveSections()
        {
            var hives = await _hiveSectionService.GetHiveSectionsAsync();
            return Ok(hives);
        }

        [HttpGet]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a hive section.", Type = typeof(HiveSection))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHiveSection(int hiveSectionId)
        {
            var hive = await _hiveSectionService.GetHiveSectionAsync(hiveSectionId);
            return Ok(hive);
        }

        [HttpPut]
        [Route("{hiveSectionId:int:min(1)}/status/{deletedStatus:bool}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Sets deleted status for an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> SetStatus([FromUri] int hiveSectionId, [FromUri] bool deletedStatus)
        {
            await _hiveSectionService.SetStatusAsync(hiveSectionId, deletedStatus);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        [HttpPost]
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new hive section.", Type = typeof(Hive))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid request data or validation failure.")]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Hive section with the same code already exists.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.")]
        public async Task<IHttpActionResult> AddHiveSection(int hiveId, [FromBody] UpdateHiveSectionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hive = await _hiveSectionService.CreateHiveSectionAsync(hiveId, request);
            var location = $"{Request.RequestUri}/{hive.Id}";
            var response = Request.CreateResponse(HttpStatusCode.Created, hive);
            response.Headers.Location = new Uri(location);
            return ResponseMessage(response);
        }

        [HttpPut]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid request data or validation failure.")]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Hive section with the same code already exists.")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive section not found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.")]
        public async Task<IHttpActionResult> UpdateHiveSection([FromUri] int hiveSectionId, [FromBody] UpdateHiveSectionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingHive = await _hiveSectionService.GetHiveSectionAsync(hiveSectionId);
            if (existingHive == null)
            {
                return NotFound();
            }

            await _hiveSectionService.UpdateHiveSectionAsync(hiveSectionId, request);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        [HttpDelete]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid hive ID.")]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Hive cannot be deleted.")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive not found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.")]
        public async Task<IHttpActionResult> DeleteHiveSection([FromUri] int hiveSectionId)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest();
            }

            var existingHive = await _hiveSectionService.GetHiveSectionAsync(hiveSectionId);
            if (existingHive == null)
            {
                return NotFound();
            }

            await _hiveSectionService.DeleteHiveSectionAsync(hiveSectionId);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
