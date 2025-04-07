namespace MicroServiceNet8.DTO.Common
{
    public class DTOError
    {
        public int? ErrorCode { get; set; }
        public string ErrorString { get; set; } = string.Empty;
        public string ErrorMember { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int IDReturn { get; set; }
        public List<DTOError> Details { get; set; } = new List<DTOError>();
    }
}
