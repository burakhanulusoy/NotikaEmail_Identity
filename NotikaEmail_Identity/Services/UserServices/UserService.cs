using AutoMapper;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.UserRepositories;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Services.UserServices
{
    public class UserService(IMapper _mapper, IUserRepository _userRepository) : IUserService
    {
        public async Task CreateAsync(CreateUserDto dto)
        {

            var user = _mapper.Map<AppUser>(dto);
            await _userRepository.CreateAsync(user);

        }

        public async Task DeleteAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<List<ResultUserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<ResultUserDto>>(users);


        }

        public async Task<UpdateUserDto> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UpdateUserDto>(user);

        }

        public async Task<List<ResultUserDto>> GetFilterAsync(Expression<Func<AppUser, bool>> filter)
        {
            var users = await _userRepository.GetAllFiterAsync(filter);
            return _mapper.Map<List<ResultUserDto>>(users);
        }

        public async Task UpdateAsync(UpdateUserDto dto)
        {
            var user = _mapper.Map<AppUser>(dto);
            await _userRepository.UpdateAsync(user);

        }
    }
}

