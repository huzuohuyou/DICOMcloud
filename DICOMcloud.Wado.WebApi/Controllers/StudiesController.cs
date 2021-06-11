using DICOMcloud.Wado.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace DICOMcloud.Wado.WebApi.Controllers
{
    public class StudiesController : ApiController
    {
        protected IQidoRsService QidoService { get; set; }
        protected IWebObjectStoreService StorageService { get; set; }
        protected IWadoRsService WadoService { get; set; }

        public StudiesController
        (
            IQidoRsService qidoService, 
            IWebObjectStoreService storageService, 
            IWadoRsService wadoService
        )
        {
            QidoService = qidoService;
            StorageService = storageService;
            WadoService = wadoService;
        }

        /// <summary>
        /// Look up studies (i.e., for a particular patient)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Route("qidors/studies")]
        [Route("api/studies")]
        [HttpGet]
        public HttpResponseMessage SearchForStudies
        (
            [ModelBinder(typeof(QidoRequestModelBinder))]
            IQidoRequestModel request
        )
        {
            return QidoService.SearchForStudies(request);
        }


        [HttpGet]
        //[Route("wadors/studies/{StudyInstanceUID}")]
        [Route("api/studies/{StudyInstanceUID}")]
        //[Route("studies/index/{StudyInstanceUID}")]
        public HttpResponseMessage GetStudies
        (
            [ModelBinder(typeof(RsStudiesRequestModelBinder))]
            IWadoRsStudiesRequest request
        )
        {
            return WadoService.RetrieveStudy(request);
        }

        public class wado_request { 
        public string includefield { get; set; }
            public bool fuzzymatching { get; set; }
            public int offset { get; set; }
            public int limit { get; set; }
        }

        [HttpPost]
        [Route("stowrs/studies/{studyInstanceUID}")]
        //[Route("stowrs")]404
        [Route("api/studies/{studyInstanceUID}")]
        [Route("api/studies/")]
        public async Task<HttpResponseMessage> Post(string studyInstanceUID = null)
        {
            WebStoreRequest webStoreRequest = new WebStoreRequest(Request);
            IStudyId studyId = null;


            if (!string.IsNullOrWhiteSpace(studyInstanceUID))
            {
                studyId = new ObjectId() { StudyInstanceUID = studyInstanceUID };
            }

            if (!Request.Content.IsMimeMultipartContent("related"))
            {//multipart/related; boundary="_boundary_by_mere"
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            await Request.Content.ReadAsMultipartAsync(webStoreRequest);

            return await StorageService.Store(webStoreRequest, studyId);
        }

        [HttpDelete]
        [Route("delowrs/studies/{studyInstanceUID}")]
        [Route("api/studies/{studyInstanceUID}")]
        public async Task<HttpResponseMessage> DeleteStudy
        (
            [ModelBinder(typeof(RsDeleteRequestModelBinder))]
            WebDeleteRequest request
        )
        {
            return await StorageService.Delete(request);
        }
    }
}
