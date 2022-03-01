using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlazorServerSignalRApp.Data;

namespace BlazorServerSignalRApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        // The following is not useful in case of WebAssembly Host since it is not web host instance
        private readonly IWebHostEnvironment _env;
        private ILogger<FilesController> _logger;

        public FilesController(IWebHostEnvironment env, ILogger<FilesController> logger)
        {
            _env = env;
            _logger = logger;
        }
        [HttpGet]
        public JsonResult Index()
        {
            var obj = new JsonResult(new
            {
                x = 1,
                y = 2
            });
            return obj;
        }
        [HttpPost]
        public async Task<ActionResult<IList<UploadResult>>> UploadFiles([FromForm] IEnumerable<IFormFile> files)
        {
            int maxFileCount = 3;
            int filePropcesed = 0;
            long maxFileSize = 15 * 1024 * 1024;
            // following used in response as an access url to files for example!!!
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
            List<UploadResult> uploadResults = new();
            foreach (var item in files)
            {
                var uploadResult = new UploadResult() { FileName = item.FileName };
                var trustedFileForDisplay = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(item.FileName);
                if (filePropcesed < maxFileCount)
                {
                    if (item.Length == 0)
                    {
                        _logger.LogError("Filename:{filename} Length is 0 (ErrordCode:1)", trustedFileForDisplay);
                        uploadResult.ErrorCode = 1;
                    }
                    else if (item.Length > maxFileSize)
                    {
                        _logger.LogError("File name:{filename} has Length: {fileLength} which is larger than MaxAllowed:{maximumAllowedFileSize}", trustedFileForDisplay, item.Length, maxFileSize);
                        uploadResult.ErrorCode = 2;
                    }
                    else
                    {
                        // Always do unmanaged code in try-catch to prevent unwanted situations
                        try
                        {
                            var trustedFileForStorage = Path.GetRandomFileName() + Path.GetExtension(item.FileName);
                            var path = Path.Combine(_env.ContentRootPath, _env.EnvironmentName, "Uploaded", trustedFileForStorage);
                            using (var fs = new FileStream(path, FileMode.Create))
                            {
                                await item.CopyToAsync(fs);
                            }
                            uploadResult.StoredFileName = trustedFileForStorage;
                            uploadResult.Uploaded = true;
                            _logger.LogInformation("Filename {filename} saved at path: {path}", trustedFileForDisplay, path);

                        }
                        catch (System.Exception ex)
                        {

                            _logger.LogError("File {filename} do not uploaded with message: {message}", trustedFileForDisplay, ex.Message);
                            uploadResult.ErrorCode = 3;
                            // uploadResult.Uploaded = true; // This should covered in API not here!!
                        }
                    }
                }
                else
                {
                    _logger.LogError("File name:{filename} does not upload because count of uploaded files are more than {maximumAllowedFilesCount} (ErrorCode:4)", trustedFileForDisplay, maxFileCount);
                    uploadResult.ErrorCode = 4;
                }
                filePropcesed++;
                uploadResults.Add(uploadResult);

            }
            return new CreatedResult(resourcePath, uploadResults);
        }
    }
}