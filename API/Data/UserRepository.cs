using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
            .FindAsync(id);

            return user;

        }

        public async Task<AppUser> GetUserByUserNameAsync(string username)
        {
            var users = await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.Username == username);

            return users;
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            var users = await _context.Users
            .Include(p => p.Photos)
            .ToListAsync();

            return users;
        }

        // public async Task<MemberDto> GetMemberByIdAsync(int id)
        // {
        //     throw new NotImplementedException();
        // }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            var member = await _context.Users
                .Where(x => x.Username == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            return member;
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            var members = await _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return members;
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}