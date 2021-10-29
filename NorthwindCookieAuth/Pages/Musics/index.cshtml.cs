using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;

namespace PracticalApp.NorthwindCookieAuth.Music
{
    public class MusicsPageModel:PageModel {
        private readonly string MusicPath=Path.Combine(Environment.CurrentDirectory,"Musics");
        private static readonly IContentTypeProvider _ctProvider=new FileExtensionContentTypeProvider();

        public class Music {
            public string FileName {get;set;}
            public string Extension {get;set;}
            public string Path {get;set;}
            public string FileNamWOExtension {get;set;}
        }

        public List<Music> Musics=new List<Music>();

        public async Task<IActionResult> OnGetAsync(string filename=null) {
            if (string.IsNullOrEmpty(filename)) {
                foreach (var item in Directory.EnumerateFiles(MusicPath))
                {
                    Musics.Add(new Music {
                        FileName=Path.GetFileName(item),
                        Path=item,
                        Extension=Path.GetExtension(item),
                        FileNamWOExtension=Path.GetFileNameWithoutExtension(item),
                    });
                }
                return Page();
            }
            var filePath=Path.Combine(Environment.CurrentDirectory,"Musics",filename);
            var fileContent=await System.IO.File.ReadAllBytesAsync(filePath);
            _ctProvider.TryGetContentType(filePath,out string contentType);
            // return File(filePath,contentType); // dont work for me

            return File(fileContent,contentType); // work fine
        }
    }
}