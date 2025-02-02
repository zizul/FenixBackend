using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Application.Services.Map.PointsOfInterest.Validators
{
    internal class ImageValidator : AbstractValidator<IFormFile>
    {
        private static float SIZE_LIMIT_IN_MB = 5;
        private static float SIZE_LIMIT_IN_B = SIZE_LIMIT_IN_MB * 1024 * 1024;
        private static int MINIMUM_SIZE_IN_B = 512;


        public ImageValidator()
        {
            RuleFor(x => x)
                .Must(IsImageFormat)
                .WithMessage("The file must be a valid image.")
                .Must(IsCorrectSize)
                .WithMessage($"The file size must be less than {SIZE_LIMIT_IN_MB} MB.");
        }

        private bool IsImageFormat(IFormFile file)
        {
            if (IsWrongMimeType(file))
            {
                return false;
            }

            if (IsWrongExtension(file))
            {
                return false;
            }

            if (IsWrongFirstBytesCheck(file))
            {
                return false;
            }

            return true;
        }

        private bool IsWrongMimeType(IFormFile file)
        {
            var contentType = file.ContentType.ToLower();
            return (contentType != "image/jpg" &&
                    contentType != "image/jpeg" &&
                    contentType != "image/pjpeg" &&
                    contentType != "image/gif" &&
                    contentType != "image/x-png" &&
                    contentType != "image/png");
        }

        private bool IsWrongExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            return (extension != ".jpg" &&
                    extension != ".png" &&
                    extension != ".jpeg");
        }

        /// <summary>
        /// additional check in case of extension substitution (.exe -> .png/etc.)
        /// </summary>
        private bool IsWrongFirstBytesCheck(IFormFile file)
        {
            try
            {
                if (!file.OpenReadStream().CanRead)
                {
                    return true;
                }

                if (file.Length < MINIMUM_SIZE_IN_B)
                {
                    return true;
                }

                byte[] buffer = new byte[MINIMUM_SIZE_IN_B];
                file.OpenReadStream().Read(buffer, 0, MINIMUM_SIZE_IN_B);

                var bmp = Encoding.ASCII.GetBytes("BM"); // BMP
                var png = new byte[] { 137, 80, 78, 71 }; // PNG
                var tiff = new byte[] { 73, 73, 42 }; // TIFF
                var tiff2 = new byte[] { 77, 77, 42 }; // TIFF
                var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
                var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

                if (!bmp.SequenceEqual(buffer.Take(bmp.Length)) &&
                    !png.SequenceEqual(buffer.Take(png.Length)) &&
                    !tiff.SequenceEqual(buffer.Take(tiff.Length)) &&
                    !tiff2.SequenceEqual(buffer.Take(tiff.Length)) &&
                    !jpeg.SequenceEqual(buffer.Take(jpeg.Length)) &&
                    !jpeg2.SequenceEqual(buffer.Take(jpeg2.Length)))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool IsCorrectSize(IFormFile photo)
        {
            return photo.Length <= SIZE_LIMIT_IN_B;
        }
    }
}
