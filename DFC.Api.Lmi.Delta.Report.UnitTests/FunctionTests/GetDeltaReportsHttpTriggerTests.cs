using AutoMapper;
using DFC.Api.Lmi.Delta.Report.Functions;
using DFC.Api.Lmi.Delta.Report.Models.ApiModels;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.FunctionTests
{
    [Trait("Category", "GetDeltaReportsHttpTrigger - Http trigger tests")]
    public class GetDeltaReportsHttpTriggerTests
    {
        private readonly ILogger<GetDeltaReportsHttpTrigger> fakeLogger = A.Fake<ILogger<GetDeltaReportsHttpTrigger>>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly IDocumentService<DeltaReportModel> fakeDeltaReportDocumentService = A.Fake<IDocumentService<DeltaReportModel>>();

        [Fact]
        public async Task GetDeltaReportsHttpTriggerReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new GetDeltaReportsHttpTrigger(fakeLogger, fakeMapper, fakeDeltaReportDocumentService);
            var request = BuildRequestWithValidBody("a request body");

            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(A.CollectionOfFake<DeltaReportModel>(2));
            A.CallTo(() => fakeMapper.Map<IList<DeltaReportSummaryApiModel>>(A<List<DeltaReportModel>>.Ignored)).Returns(A.CollectionOfFake<DeltaReportSummaryApiModel>(2));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<IList<DeltaReportSummaryApiModel>>(A<List<DeltaReportModel>>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetDeltaReportsHttpTriggerReturnsNoContent()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.NoContent);
            var function = new GetDeltaReportsHttpTrigger(fakeLogger, fakeMapper, fakeDeltaReportDocumentService);
            var request = BuildRequestWithValidBody("a request body");
            List<DeltaReportModel>? nullDeltaReportModels = null;

            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(nullDeltaReportModels);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<IList<DeltaReportSummaryApiModel>>(A<List<DeltaReportModel>>.Ignored)).MustNotHaveHappened();
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
