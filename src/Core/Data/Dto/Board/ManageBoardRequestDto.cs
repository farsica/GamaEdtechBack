namespace GamaEdtech.Data.Dto.Board
{
    public sealed class ManageBoardRequestDto
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }
}
