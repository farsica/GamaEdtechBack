namespace GamaEdtech.Presentation.Api.Areas.Admin.Controllers
{
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Collections.Generic;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAccess.Specification.Impl;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Data.Dto.Contribution;
    using GamaEdtech.Data.Dto.School;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Domain.Specification;
    using GamaEdtech.Domain.Specification.School;
    using GamaEdtech.Presentation.ViewModel.School;
    using GamaEdtech.Presentation.ViewModel.Tag;

    using Microsoft.AspNetCore.Mvc;

    using NetTopologySuite;
    using NetTopologySuite.Geometries;

    [Common.DataAnnotation.Area(nameof(Admin), "Admin")]
    [Route("api/v{version:apiVersion}/[area]/[controller]")]
    [ApiVersion("1.0")]
    [Permission(Roles = [nameof(Role.Admin)])]
    public class SchoolsController(Lazy<ILogger<SchoolsController>> logger, Lazy<ISchoolService> schoolService
        , Lazy<IContributionService> contributionService, Lazy<IFileService> fileService, Lazy<ITagService> tagService)
        : ApiControllerBase<SchoolsController>(logger)
    {
        #region Schools

        [HttpGet, Produces<ApiResponse<ListDataSource<SchoolsResponseViewModel>>>()]
        public async Task<IActionResult> GetSchools([NotNull, FromQuery] SchoolsRequestViewModel request)
        {
            try
            {
                var result = await schoolService.Value.GetSchoolsAsync(new ListRequestDto<School>
                {
                    PagingDto = request.PagingDto,
                });
                return Ok(new ApiResponse<ListDataSource<SchoolsResponseViewModel>>
                {
                    Errors = result.Errors,
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new SchoolsResponseViewModel
                        {
                            Id = t.Id,
                            Name = t.Name,
                            LocalName = t.LocalName,
                            DefaultImageUri = t.DefaultImageUri,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ListDataSource<SchoolsResponseViewModel>> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpGet("{id:long}"), Produces<ApiResponse<SchoolResponseViewModel>>()]
        public async Task<IActionResult<SchoolResponseViewModel>> GetSchool([FromRoute] long id)
        {
            try
            {
                var result = await schoolService.Value.GetSchoolAsync(new IdEqualsSpecification<School, long>(id));
                return Ok<SchoolResponseViewModel>(new(result.Errors)
                {
                    Data = MapFrom(result.Data)
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<SchoolResponseViewModel> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPost, Produces<ApiResponse<ManageSchoolResponseViewModel>>()]
        public async Task<IActionResult> CreateSchool([NotNull] ManageSchoolRequestViewModel request)
        {
            try
            {
                Point? coordinates = null;
                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
                    coordinates = geometryFactory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
                }

                ManageSchoolRequestDto dto = new()
                {
                    Address = request.Address,
                    Name = request.Name,
                    LocalName = request.LocalName,
                    SchoolType = request.SchoolType!,
                    StateId = request.StateId,
                    ZipCode = request.ZipCode,
                    Coordinates = coordinates,
                    WebSite = request.WebSite,
                    LocalAddress = request.LocalAddress,
                    CityId = request.CityId,
                    CountryId = request.CountryId,
                    Email = request.Email,
                    FaxNumber = request.FaxNumber,
                    PhoneNumber = request.PhoneNumber,
                    Quarter = request.Quarter,
                    OsmId = request.OsmId,
                    Tags = request.Tags,
                    Tuition = request.Tuition,
                    Description = request.Description,
                    UserId = User.UserId(),
                    Date = DateTimeOffset.UtcNow,
                };
                var result = await schoolService.Value.ManageSchoolAsync(dto);

                return Ok(new ApiResponse<ManageSchoolResponseViewModel>
                {
                    Errors = result.Errors,
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ManageSchoolResponseViewModel> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPut("{id:long}"), Produces<ApiResponse<ManageSchoolResponseViewModel>>()]
        public async Task<IActionResult> UpdateSchool([FromRoute] long id, [NotNull, FromBody] UpdateSchoolRequestViewModel request)
        {
            try
            {
                Point? coordinates = null;
                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
                    coordinates = geometryFactory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
                }

                ManageSchoolRequestDto dto = new()
                {
                    Id = id,
                    Address = request.Address,
                    Name = request.Name,
                    LocalName = request.LocalName,
                    SchoolType = request.SchoolType!,
                    StateId = request.StateId,
                    ZipCode = request.ZipCode,
                    Coordinates = coordinates,
                    WebSite = request.WebSite,
                    LocalAddress = request.LocalAddress,
                    CityId = request.CityId,
                    CountryId = request.CountryId,
                    Email = request.Email,
                    FaxNumber = request.FaxNumber,
                    PhoneNumber = request.PhoneNumber,
                    Quarter = request.Quarter,
                    OsmId = request.OsmId,
                    Tags = request.Tags,
                    Tuition = request.Tuition,
                    Description = request.Description,
                    UserId = User.UserId(),
                    Date = DateTimeOffset.UtcNow,
                };
                var result = await schoolService.Value.ManageSchoolAsync(dto);

                return Ok(new ApiResponse<ManageSchoolResponseViewModel>
                {
                    Errors = result.Errors,
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ManageSchoolResponseViewModel> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpDelete("{id:long}"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult> RemoveSchool([FromRoute] long id)
        {
            try
            {
                var result = await schoolService.Value.RemoveSchoolAsync(new IdEqualsSpecification<School, long>(id));
                return Ok(new ApiResponse<bool>
                {
                    Errors = result.Errors,
                    Data = result.Data
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        #endregion

        #region Comments

        [HttpGet("comments/contributions"), Produces<ApiResponse<ListDataSource<SchoolCommentContributionListResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<SchoolCommentContributionListResponseViewModel>>> GetSchoolCommentContributionList([NotNull, FromQuery] SchoolCommentContributionListRequestViewModel request)
        {
            try
            {
                ISpecification<Contribution> specification = new CategoryTypeEqualsSpecification<Contribution>(CategoryType.SchoolComment);
                if (request.Status is not null)
                {
                    specification = specification.And(new StatusEqualsSpecification<Contribution>(request.Status));
                }
                var result = await contributionService.Value.GetContributionsAsync<SchoolCommentContributionDto>(new ListRequestDto<Contribution>
                {
                    PagingDto = request.PagingDto,
                    Specification = specification,
                });
                return Ok(new ApiResponse<ListDataSource<SchoolCommentContributionListResponseViewModel>>(result.Errors)
                {
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new SchoolCommentContributionListResponseViewModel
                        {
                            Id = t.Id,
                            CreationUser = t.CreationUser,
                            CreationDate = t.CreationDate,
                            SchoolId = t.IdentifierId.GetValueOrDefault(),
                            Status = t.Status,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ListDataSource<SchoolCommentContributionListResponseViewModel>>(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("comments/contributions/{contributionId:long}"), Produces<ApiResponse<SchoolCommentContributionReviewViewModel>>()]
        public async Task<IActionResult<SchoolCommentContributionReviewViewModel>> GetSchoolCommentContribution([FromRoute] long contributionId)
        {
            try
            {
                var specification = new IdEqualsSpecification<Contribution, long>(contributionId)
                    .And(new CategoryTypeEqualsSpecification<Contribution>(CategoryType.SchoolComment));
                var contributionResult = await contributionService.Value.GetContributionAsync<SchoolCommentContributionDto>(specification);
                if (contributionResult.Data?.Data is null)
                {
                    return Ok(new ApiResponse<SchoolCommentContributionReviewViewModel>(contributionResult.Errors));
                }

                var schoolResult = await schoolService.Value.GetSchoolAsync(new IdEqualsSpecification<School, long>(contributionResult.Data.IdentifierId.GetValueOrDefault()));
                if (schoolResult.OperationResult is not Constants.OperationResult.Succeeded)
                {
                    return Ok(new ApiResponse<SchoolCommentContributionReviewViewModel>(schoolResult.Errors));
                }

                SchoolCommentContributionReviewViewModel result = new()
                {
                    Id = contributionResult.Data.Id,
                    SchoolId = contributionResult.Data.Data!.SchoolId,
                    SchoolName = schoolResult.Data?.Name,
                    ArtisticActivitiesRate = contributionResult.Data.Data!.ArtisticActivitiesRate,
                    AverageRate = contributionResult.Data.Data!.AverageRate,
                    BehaviorRate = contributionResult.Data.Data!.BehaviorRate,
                    ClassesQualityRate = contributionResult.Data.Data!.ClassesQualityRate,
                    Comment = contributionResult.Data.Data!.Comment,
                    EducationRate = contributionResult.Data.Data!.EducationRate,
                    FacilitiesRate = contributionResult.Data.Data!.FacilitiesRate,
                    ITTrainingRate = contributionResult.Data.Data!.ITTrainingRate,
                    SafetyAndHappinessRate = contributionResult.Data.Data!.SafetyAndHappinessRate,
                    TuitionRatioRate = contributionResult.Data.Data!.TuitionRatioRate,
                };

                return Ok(new ApiResponse<SchoolCommentContributionReviewViewModel>
                {
                    Data = result,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<SchoolCommentContributionReviewViewModel>(new Error { Message = exc.Message }));
            }
        }


        [HttpPatch("comments/contributions/{contributionId:long}/confirm"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult> ConfirmSchoolCommentContribution([FromRoute] long contributionId)
        {
            try
            {
                var result = await schoolService.Value.ConfirmSchoolCommentContributionAsync(new ConfirmSchoolCommentContributionRequestDto
                {
                    ContributionId = contributionId,
                });

                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPatch("comments/contributions/{contributionId:long}/reject"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult> RejectSchoolCommentContribution([FromRoute] long contributionId, [NotNull, FromBody] RejectContributionRequestViewModel request)
        {
            try
            {
                var result = await contributionService.Value.RejectContributionAsync(new RejectContributionRequestDto
                {
                    Id = contributionId,
                    Comment = request.Comment,
                });
                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        #endregion

        #region Images

        [HttpGet("images/contributions"), Produces<ApiResponse<ListDataSource<SchoolImageContributionListResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<SchoolImageContributionListResponseViewModel>>> GetSchoolImageContributionList([NotNull, FromQuery] SchoolImageContributionListRequestViewModel request)
        {
            try
            {
                ISpecification<Contribution> specification = new CategoryTypeEqualsSpecification<Contribution>(CategoryType.SchoolImage);
                if (request.Status is not null)
                {
                    specification = specification.And(new StatusEqualsSpecification<Contribution>(request.Status));
                }
                var result = await contributionService.Value.GetContributionsAsync<SchoolImageContributionDto>(new ListRequestDto<Contribution>
                {
                    PagingDto = request.PagingDto,
                    Specification = specification,
                }, true);

                List<SchoolImageContributionListResponseViewModel> lst = [];
                if (result.Data.List is not null)
                {
                    foreach (var item in result.Data.List)
                    {
                        lst.Add(new SchoolImageContributionListResponseViewModel
                        {
                            Id = item.Id,
                            CreationUser = item.CreationUser,
                            CreationDate = item.CreationDate,
                            SchoolId = item.IdentifierId.GetValueOrDefault(),
                            Status = item.Status,
                            FileUri = fileService.Value.GetFileUri(item.Data?.FileId, ContainerType.School).Data,
                            FileType = item.Data?.FileType,
                            IsDefault = item.Data?.IsDefault ?? false,
                        });
                    }
                }

                return Ok(new ApiResponse<ListDataSource<SchoolImageContributionListResponseViewModel>>(result.Errors)
                {
                    Data = new()
                    {
                        List = lst,
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ListDataSource<SchoolImageContributionListResponseViewModel>>(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("images/contributions/{contributionId:long}"), Produces<ApiResponse<SchoolImageContributionReviewViewModel>>()]
        public async Task<IActionResult<SchoolImageContributionReviewViewModel>> GetSchoolImageContribution([FromRoute] long contributionId)
        {
            try
            {
                var specification = new IdEqualsSpecification<Contribution, long>(contributionId)
                    .And(new CategoryTypeEqualsSpecification<Contribution>(CategoryType.SchoolImage));
                var contributionResult = await contributionService.Value.GetContributionAsync<SchoolImageContributionDto>(specification);
                if (contributionResult.Data?.Data is null)
                {
                    return Ok(new ApiResponse<SchoolImageContributionReviewViewModel>(contributionResult.Errors));
                }

                var schoolResult = await schoolService.Value.GetSchoolAsync(new IdEqualsSpecification<School, long>(contributionResult.Data.IdentifierId.GetValueOrDefault()));
                if (schoolResult.OperationResult is not Constants.OperationResult.Succeeded)
                {
                    return Ok(new ApiResponse<SchoolImageContributionReviewViewModel>(schoolResult.Errors));
                }

                string? tagName = null;
                if (contributionResult.Data.Data!.TagId.HasValue)
                {
                    tagName = (await tagService.Value.GetTagNameAsync(new IdEqualsSpecification<Tag, long>(contributionResult.Data.Data!.TagId.Value))).Data;
                }

                SchoolImageContributionReviewViewModel result = new()
                {
                    Id = contributionResult.Data.Id,
                    FileId = contributionResult.Data.Data!.FileId,
                    FileType = contributionResult.Data.Data!.FileType,
                    SchoolId = contributionResult.Data.Data!.SchoolId,
                    IsDefault = contributionResult.Data.Data!.IsDefault,
                    TagId = contributionResult.Data.Data!.TagId,
                    TagName = tagName,
                    SchoolName = schoolResult.Data?.Name,
                };

                return Ok(new ApiResponse<SchoolImageContributionReviewViewModel>
                {
                    Data = result,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<SchoolImageContributionReviewViewModel>(new Error { Message = exc.Message }));
            }
        }

        [HttpPatch("images/contributions/{contributionId:long}/confirm"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult> ConfirmSchoolImageContribution([FromRoute] long contributionId)
        {
            try
            {
                var result = await schoolService.Value.ConfirmSchoolImageContributionAsync(new ConfirmSchoolImageContributionRequestDto
                {
                    ContributionId = contributionId,
                });

                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPatch("images/contributions/{contributionId:long}/reject"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult> RejectSchoolImageContribution([FromRoute] long contributionId, [NotNull, FromBody] RejectContributionRequestViewModel request)
        {
            try
            {
                var result = await contributionService.Value.RejectContributionAsync(new RejectContributionRequestDto
                {
                    Id = contributionId,
                    Comment = request.Comment,
                });
                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPatch("{schoolId:long}/images/{imageId:long}"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> ManageSchoolImage([FromRoute] long schoolId, [FromRoute] long imageId, [NotNull, FromBody] ManageSchoolImageRequestViewModel request)
        {
            try
            {
                var result = await schoolService.Value.ManageSchoolImageAsync(new ManageSchoolImageRequestDto
                {
                    Id = imageId,
                    SchoolId = schoolId,
                    TagId = request.TagId,
                    IsDefault = request.IsDefault,
                });
                return Ok<bool>(new(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<bool>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpDelete("{schoolId:long}/images/{imageId:long}"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult> RemoveImage([FromRoute] long schoolId, [FromRoute] long imageId)
        {
            try
            {
                var specification = new IdEqualsSpecification<SchoolImage, long>(imageId)
                    .And(new SchoolIdEqualsSpecification<SchoolImage>(schoolId));
                var result = await schoolService.Value.RemoveSchoolImageAsync(specification);
                return Ok(new ApiResponse<bool>
                {
                    Errors = result.Errors,
                    Data = result.Data
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        #endregion

        #region Contributions

        [HttpGet("contributions"), Produces<ApiResponse<ListDataSource<SchoolContributionListResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<SchoolContributionListResponseViewModel>>> GetSchoolContributionList([NotNull, FromQuery] SchoolContributionListRequestViewModel request)
        {
            try
            {
                ISpecification<Contribution> specification = new CategoryTypeEqualsSpecification<Contribution>(CategoryType.School);
                if (request.Status is not null)
                {
                    specification = specification.And(new StatusEqualsSpecification<Contribution>(request.Status));
                }

                var result = await contributionService.Value.GetContributionsAsync<SchoolContributionDto>(new ListRequestDto<Contribution>
                {
                    PagingDto = request.PagingDto,
                    Specification = specification,
                });
                return Ok(new ApiResponse<ListDataSource<SchoolContributionListResponseViewModel>>
                {
                    Errors = result.Errors,
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new SchoolContributionListResponseViewModel
                        {
                            Id = t.Id,
                            Comment = t.Comment,
                            CreationUser = t.CreationUser,
                            CreationDate = t.CreationDate,
                            IdentifierId = t.IdentifierId,
                            Status = t.Status,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ListDataSource<SchoolContributionListResponseViewModel>>(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("contributions/{contributionId:long}"), Produces<ApiResponse<SchoolContributionReviewViewModel>>()]
        public async Task<IActionResult<SchoolContributionReviewViewModel>> GetSchoolContribution([FromRoute] long contributionId)
        {
            try
            {
                var specification = new IdEqualsSpecification<Contribution, long>(contributionId)
                    .And(new CategoryTypeEqualsSpecification<Contribution>(CategoryType.School));
                var contributionResult = await contributionService.Value.GetContributionAsync<SchoolContributionDto>(specification);
                if (contributionResult.Data?.Data is null)
                {
                    return Ok(new ApiResponse<SchoolContributionReviewViewModel>(contributionResult.Errors));
                }

                SchoolDto? schoolDto = null;
                if (contributionResult.Data.IdentifierId.HasValue)
                {
                    var data = await schoolService.Value.GetSchoolAsync(new IdEqualsSpecification<School, long>(contributionResult.Data.IdentifierId.Value));
                    if (data.OperationResult is not Constants.OperationResult.Succeeded)
                    {
                        return Ok<SchoolContributionReviewViewModel>(new(data.Errors));
                    }
                    schoolDto = data.Data;
                }

                SchoolContributionReviewViewModel result = new()
                {
                    NewValues = Api.Controllers.SchoolsController.MapFrom(contributionResult.Data.Data),
                    OldValues = MapFrom(schoolDto),
                };

                return Ok(new ApiResponse<SchoolContributionReviewViewModel>
                {
                    Data = result,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<SchoolContributionReviewViewModel>(new Error { Message = exc.Message }));
            }
        }

        [HttpPatch("contributions/{contributionId:long}/confirm"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> ConfirmSchoolContribution([FromRoute] long contributionId, [NotNull, FromBody] ConfirmSchoolContributionRequestViewModel request)
        {
            try
            {
                var result = await schoolService.Value.ConfirmSchoolContributionAsync(new ConfirmSchoolContributionRequestDto
                {
                    ContributionId = contributionId,
                    SchoolId = request.SchoolId.GetValueOrDefault(),
                });

                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPatch("contributions/{contributionId:long}/reject"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> RejectSchoolContribution([FromRoute] long contributionId, [NotNull, FromBody] RejectContributionRequestViewModel request)
        {
            try
            {
                var result = await contributionService.Value.RejectContributionAsync(new RejectContributionRequestDto
                {
                    Id = contributionId,
                    Comment = request.Comment,
                });

                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool>(new Error { Message = exc.Message }));
            }
        }

        #endregion

        #region Issues

        [HttpGet("issues/contributions"), Produces<ApiResponse<ListDataSource<SchoolIssuesContributionReviewResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<SchoolIssuesContributionReviewResponseViewModel>>> GetSchoolIssuesContributionList([NotNull, FromQuery] SchoolIssuesContributionListRequestViewModel request)
        {
            try
            {
                ISpecification<Contribution> specification = new CategoryTypeEqualsSpecification<Contribution>(CategoryType.SchoolIssues);
                if (request.Status is not null)
                {
                    specification = specification.And(new StatusEqualsSpecification<Contribution>(request.Status));
                }

                var result = await contributionService.Value.GetContributionsAsync<string>(new ListRequestDto<Contribution>
                {
                    PagingDto = request.PagingDto,
                    Specification = specification,
                }, false);
                if (result.Data.List is null)
                {
                    return Ok<ListDataSource<SchoolIssuesContributionReviewResponseViewModel>>(new(result.Errors)
                    {
                        Data = new()
                    });
                }

                var schoolsName = await schoolService.Value.GetSchoolsNameAsync(new IdContainsSpecification<School, long>(result.Data.List.Select(t => t.IdentifierId.GetValueOrDefault())));

                return Ok(new ApiResponse<ListDataSource<SchoolIssuesContributionReviewResponseViewModel>>
                {
                    Errors = result.Errors,
                    Data = new()
                    {
                        List = result.Data.List.Select(t => new SchoolIssuesContributionReviewResponseViewModel
                        {
                            Id = t.Id,
                            CreationUser = t.CreationUser,
                            CreationDate = t.CreationDate,
                            SchoolId = t.IdentifierId,
                            SchoolName = schoolsName.Data?.Find(s => s.Key == t.IdentifierId).Value,
                            Status = t.Status,
                            Description = t.Data,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<ListDataSource<SchoolIssuesContributionReviewResponseViewModel>>(new Error { Message = exc.Message }));
            }
        }

        [HttpPatch("issues/contributions/{contributionId:long}/confirm"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> ConfirmSchoolIssuesContribution([FromRoute] long contributionId)
        {
            try
            {
                var result = await schoolService.Value.ConfirmSchoolIssuesContributionAsync(new()
                {
                    ContributionId = contributionId
                });

                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool> { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPatch("issues/contributions/{contributionId:long}/reject"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> RejectSchoolIssuesContribution([FromRoute] long contributionId, [NotNull, FromBody] RejectContributionRequestViewModel request)
        {
            try
            {
                var result = await contributionService.Value.RejectContributionAsync(new RejectContributionRequestDto
                {
                    Id = contributionId,
                    Comment = request.Comment,
                });
                return Ok(new ApiResponse<bool>(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok(new ApiResponse<bool>(new Error { Message = exc.Message }));
            }
        }

        #endregion

        private static SchoolResponseViewModel? MapFrom(SchoolDto? dto) => dto is null ? null : new()
        {
            Id = dto.Id,
            Address = dto.Address,
            LocalAddress = dto.LocalAddress,
            Name = dto.Name,
            LocalName = dto.LocalName,
            SchoolType = dto.SchoolType,
            StateId = dto.StateId,
            StateTitle = dto.StateTitle,
            ZipCode = dto.ZipCode,
            Latitude = dto.Coordinates?.Y,
            Longitude = dto.Coordinates?.X,
            WebSite = dto.WebSite,
            Email = dto.Email,
            CityId = dto.CityId,
            CityTitle = dto.CityTitle,
            CountryId = dto.CountryId,
            CountryTitle = dto.CountryTitle,
            FaxNumber = dto.FaxNumber,
            PhoneNumber = dto.PhoneNumber,
            Quarter = dto.Quarter,
            OsmId = dto.OsmId,
            DefaultImageUri = dto.DefaultImageUri,
            Tuition = dto.Tuition,
            Description = dto.Description,
            Tags = dto.Tags?.Select(t => new TagResponseViewModel
            {
                TagType = t.TagType,
                Name = t.Name,
                Icon = t.Icon,
                Id = t.Id,
            }),
        };
    }
}
