namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class ProfileSettingsDto
    {
        public string? TimeZoneId { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? StateId { get; set; }
        public long? SchoolId { get; set; }
    }
}
