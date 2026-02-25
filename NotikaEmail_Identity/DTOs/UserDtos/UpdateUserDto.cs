namespace NotikaEmail_Identity.DTOs.UserDtos
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }

    }
}
