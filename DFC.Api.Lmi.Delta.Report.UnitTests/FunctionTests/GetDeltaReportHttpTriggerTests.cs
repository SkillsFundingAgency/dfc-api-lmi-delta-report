using AutoMapper;
using Azure.Core;
using DFC.Api.Lmi.Delta.Report.Functions;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IDocumentService<DeltaReportSocModel> fakeDeltaReportSocDocumentService = A.Fake<IDocumentService<DeltaReportSocModel>>();

        [Fact]
        public async Task GetDeltaReportHttpTriggerReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new GetDeltaReportHttpTrigger(fakeLogger, fakeMapper, fakeDeltaReportDocumentService, fakeDeltaReportSocDocumentService);
            var request = BuildRequestWithValidBody("a request body");

            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(A.Fake<DeltaReportModel>());
            A.CallTo(() => fakeMapper.Map<DeltaReportApiModel>(A<DeltaReportModel>.Ignored)).Returns(A.Fake<DeltaReportApiModel>());
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).Returns(A.CollectionOfDummy<DeltaReportSocModel>(2));
            A.CallTo(() => fakeMapper.Map<List<DeltaReportSocApiModel>>(A<List<DeltaReportSocModel>>.Ignored)).Returns(A.CollectionOfFake<DeltaReportSocApiModel>(2).ToList());

            // Act
            var result = await function.Run(request, Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<DeltaReportApiModel>(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<List<DeltaReportSocApiModel>>(A<List<DeltaReportSocModel>>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetDeltaReportHttpTriggerReturnsNoContent()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.NoContent);
            var function = new GetDeltaReportHttpTrigger(fakeLogger, fakeMapper, fakeDeltaReportDocumentService, fakeDeltaReportSocDocumentService);
            var request = BuildRequestWithValidBody("a request body");
            DeltaReportModel? nullDeltaReportModel = null;

            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(nullDeltaReportModel);

            // Act
            var result = await function.Run(request, Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<DeltaReportApiModel>(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<List<DeltaReportSocApiModel>>(A<List<DeltaReportSocModel>>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        private static HttpRequest BuildRequestWithValidBody(string bodyString)
        {
            var context = new DefaultHttpContext
            {
                Request =
                {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyString)),
                },
            };

            return context.Request;
        }
    }
}
