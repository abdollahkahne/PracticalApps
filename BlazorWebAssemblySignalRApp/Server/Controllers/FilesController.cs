using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlazorWebAssemblySignalRApp.Shared.Models;

namespace BlazorWebAssemblySignalRApp.Server.Controllers
{
    [ApiController] // thia make [FromBody] the default binding attribute so we need specify form cases explicitly
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IWebHostEnvironment _env;

        public FilesController(ILogger<FilesController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
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

        // Here we can return the list directly too. but since it want to generates other status codes than Ok 200, it uses ActionResult<T> for that purpose.
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadFiles([FromForm] IFormFileCollection files)
        {
            Console.WriteLine("I called and runned");
            // Created Resource Path used:
            var resource = new Uri($"{Request.Scheme}://{Request.Host}/");
            var uploadResults = new List<UploadResult>();
            int filesProcessed = 0;
            int maxFileCount = 3;
            long maxFileSize = 15 * 1024 * 1024;
            // we should do validations here too
            foreach (var item in files)
            {
                //Error Codes:
                // 1- File Length is 0
                // 2- File Length is more than maximum allowed file size
                // 3- File could not copy to file system with msg
                // 4- Number of items are more than max file count
                // 5- File do not uploaded from browser (This error may convert to other one after finishing upload process)
                // 6- IOException in Client side to read file from browser or Maximum Size violated
                string trustedFileForDisplay = System.Net.WebUtility.HtmlEncode(item.FileName);
                var uploadResult = new UploadResult() { FileName = item.FileName };
                if (filesProcessed >= maxFileCount)
                {
                    _logger.LogError("File name:{filename} does not upload because count of uploaded files are more than {maximumAllowedFilesCount} (ErrorCode:4)", trustedFileForDisplay, maxFileCount);
                    uploadResult.ErrorCode = 4;
                }
                else if (item.Length == 0)
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
                    try
                    {
                        var trustedFileForStorage = Path.GetRandomFileName();
                        var path = Path.Combine(_env.ContentRootPath, _env.EnvironmentName, "Uploaded", trustedFileForStorage);
                        using (var fs = new FileStream(path, FileMode.Create))
                        {
                            await item.CopyToAsync(fs);
                        }
                        uploadResult.Uploaded = true;
                        uploadResult.StoredFileName = trustedFileForStorage;
                        _logger.LogInformation("Filename {filename} saved at path: {path}", trustedFileForDisplay, path);
                    }
                    catch (System.Exception ex)
                    {

                        uploadResult.ErrorCode = 3;
                        _logger.LogError("File {filename} do not uploaded with message: {message}", trustedFileForDisplay, ex.Message);
                    }
                }
                filesProcessed++;
                uploadResults.Add(uploadResult);
            }
            return new CreatedResult(resource, uploadResults);

        }
        [HttpPost("{fileToDownload}")]
        public async Task<IActionResult> Download(string fileToDownload)
        {
            if (string.IsNullOrEmpty(fileToDownload))
            {
                return Content("Please Provide file name");
            }
            var path = Path.Combine(_env.ContentRootPath, _env.EnvironmentName, "Uploaded", fileToDownload);
            if (!System.IO.File.Exists(path))
            {
                return Content("File you are searching does not exist!");
            }
            // Option1: use PhysicalFile method of controller
            return PhysicalFile(path, "application/octet-stream", fileToDownload);
            // virtualPath is something special to IIS and used for naming a physical folder as a virtual folder 
            // so the end user do not guess the full file structure and following does not work here
            // return File(virtualPath: $"/Development/Uploaded/{fileToDownload}", "application/octet-stream", fileToDownload);

            // Option 2: Use a memory stream/ File Stream and File method of base controller
            try
            {

                var ms = new MemoryStream();
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    await fileStream.CopyToAsync(ms);

                }
                ms.Position = 0; // this is important and also we should not dispose memory stream!
                return File(ms, "application/octet-stream", fileToDownload);
            }
            catch (FileNotFoundException ex)
            {
                return Content($"File does not exist: ${ex.Message}");
            }
            catch (System.Exception ex)
            {

                return Content($"{ex.Message}");
            }

            // Option 3: Use File.ReadAllBytes and File method of base controller
            var content = await System.IO.File.ReadAllBytesAsync(path);
            return File(content, "application/octet-stream", fileToDownload);
        }

        [HttpGet("{fileToDownload}")]
        public IActionResult GetFile(string fileToDownload)
        {
            if (string.IsNullOrEmpty(fileToDownload))
            {
                return Content("Please Provide file name");
            }
            var path = Path.Combine(_env.ContentRootPath, _env.EnvironmentName, "Uploaded", fileToDownload);
            if (!System.IO.File.Exists(path))
            {
                return Content("File you are searching does not exist!");
            }
            // Option1: use PhysicalFile method of controller
            return PhysicalFile(path, "application/octet-stream", fileToDownload);
        }
    }
}