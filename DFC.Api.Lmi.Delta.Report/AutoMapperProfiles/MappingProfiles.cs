using AutoMapper;
using DFC.Api.Lmi.Delta.Report.Models.ApiModels;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.AutoMapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<JobGroupModel, JobGroupToDeltaModel>();

            CreateMap<DeltaReportModel, DeltaReportSummaryApiModel>();

            CreateMap<FullDeltaReportModel, DeltaReportModel>();

            CreateMap<DeltaReportModel, DeltaReportApiModel>();

            CreateMap<DeltaReportModel, DeltaReportApiModel>();

            CreateMap<DeltaReportSocModel, DeltaReportSocApiModel>()
                .ForMember(d => d.DraftJobGroup, s => s.MapFrom(a => JsonConvert.SerializeObject(a.DraftJobGroup)))
                .ForMember(d => d.PublishedJobGroup, s => s.MapFrom(a => JsonConvert.SerializeObject(a.PublishedJobGroup)));
        }
    }
}
