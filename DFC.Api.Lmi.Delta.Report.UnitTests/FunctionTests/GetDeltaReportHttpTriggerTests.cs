using AutoMapper;
using DFC.Api.Lmi.Delta.Report.Functions;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.FunctionTests
{
    [Trait("Category", "GetDeltaReportHttpTrigger - Http trigger tests")]
    public class GetDeltaReportHttpTriggerTests
    {
        private readonly ILogger<GetDeltaReportHttpTrigger> fakeLogger = A.Fake<ILogger<GetDeltaReportHttpTrigger>>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly IDocumentService<DeltaReportModel> fakeDeltaReportDocumentService = A.Fake<IDocumentService<DeltaReportModel>>();

        [Fact]
        public async Task GetDeltaReportHttpTriggerReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new GetDeltaReportHttpTrigger(fakeLogger, fakeMapper, fakeDeltaReportDocumentService);
            var request = BuildRequestWithValidBody("a request body");

            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(A.Fake<DeltaReportModel>());
            A.CallTo(() => fakeMapper.Map<DeltaReportApiModel>(A<DeltaReportModel>.Ignored)).Returns(A.Fake<DeltaReportApiModel>());

            // Act
            var result = await function.Run(request, Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<DeltaReportApiModel>(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetDeltaReportHttpTriggerReturnsNoContent()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.NoContent);
            var function = new GetDeltaReportHttpTrigger(fakeLogger, fakeMapper, fakeDeltaReportDocumentService);
            var request = BuildRequestWithValidBody("a request body");
            DeltaReportModel? nullDeltaReportModel = null;

            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(nullDeltaReportModel);

            // Act
            var result = await function.Run(request, Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<DeltaReportApiModel>(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        private static HttpRequest BuildRequestWithValidBody(string bodyString)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyString)),
            };
        }
    }
}
