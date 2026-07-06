namespace KeyStone_Identity.Core.DTOs.Response
{
    public class UserRegistrationResponseDTO
    {
        public long ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
    }
}
