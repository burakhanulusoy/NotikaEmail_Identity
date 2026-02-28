namespace NotikaEmail_Identity.DTOs.UserDtos
{
    public class CreateUserDto
    {


        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public string City { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string TwitterLink { get; set; }
        public string GitHubLink { get; set; }
        public string LinkedlnLink { get; set; }
        public string AboutMe { get; set; }
        public string Job { get; set; }

        public int? ActivationCode { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CurrentPassword { get; set; }
    }
}
