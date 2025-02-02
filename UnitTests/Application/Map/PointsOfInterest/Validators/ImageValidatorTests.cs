using Application.Services.Map.PointsOfInterest.Validators;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace UnitTests.Application.Map.PointsOfInterest.Validators
{
    public class ImageValidatorTests
    {
        private readonly ImageValidator validator;
        private readonly IFormFile testFile;


        public ImageValidatorTests()
        {
            validator = new ImageValidator();
            testFile = Substitute.For<IFormFile>();
        }

        [Theory]
        [InlineData("image/png", "test.png", true)]
        [InlineData("image/jpg", "test.jpg", true)]
        public void Validate_Should_Valid(string contentType, string fileName, bool isValidContent)
        {
            SetupFile(contentType, fileName, isValidContent);

            var result = validator.Validate(testFile);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("application/x-msdownload", "test.exe", false)]
        [InlineData("application/pdf", "test.png", false)]
        [InlineData("image/png", "test.png", false)]
        public void Validate_Should_Fail(string contentType, string fileName, bool isValidContent)
        {
            SetupFile(contentType, fileName, isValidContent);

            var result = validator.Validate(testFile);

            Assert.False(result.IsValid);
        }

        private void SetupFile(string contentType, string fileName, bool isValidContent)
        {
            testFile.ContentType.Returns(contentType);
            testFile.FileName.Returns(fileName);
            SetupStreamForFirstBytes(isValidContent);
        }

        private void SetupStreamForFirstBytes(bool isValid)
        {
            var bytes = new byte[1024];
            if (isValid)
            {
                // png 'magic' bytes
                bytes[0] = 137;
                bytes[1] = 80;
                bytes[2] = 78;
                bytes[3] = 71;
            }
            else
            {
                // exe
                bytes[0] = 77;
                bytes[1] = 90;
            }

            // no need to set all of the 1024 bytes
            for (int i = 4; i < 20; i++)
            {
                bytes[i] = (byte)(i % 255);
            }
            testFile.OpenReadStream().Returns(new MemoryStream(bytes));
            testFile.Length.Returns(1024);
        }
    }
}
