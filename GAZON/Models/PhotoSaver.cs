namespace GAZON.Models
{
    public static class PhotoSaver
    {
        /// <summary>
        /// Saves photo to the server
        /// </summary>
        /// <param name="photo"></param>
        /// <returns> Returns path to the photo </returns>
        public static async Task<string> SavePhoto(IFormFile photo, string serverPath)
        {
            var fileName = Path.GetFileName(photo.FileName);
            var path = Path.Combine(Environment.CurrentDirectory + "/wwwroot" + serverPath, fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }
            return serverPath + fileName;
        }
    }
}
