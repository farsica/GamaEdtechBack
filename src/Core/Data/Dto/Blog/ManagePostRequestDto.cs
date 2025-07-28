namespace GamaEdtech.Data.Dto.Blog
{
    using System.Collections.Generic;

    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;

    public sealed class ManagePostRequestDto
    {
        public long? Id { get; set; }
        public int CreationUserId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Summary { get; set; }
        public string? Body { get; set; }
        public VisibilityType? VisibilityType { get; set; }
        public DateTimeOffset? PublishDate { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageId { get; set; }
        public IEnumerable<long>? Tags { get; set; }
        public string? Keywords { get; set; }
    }
}
