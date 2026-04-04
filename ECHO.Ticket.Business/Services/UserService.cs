using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;

namespace ECHO.Ticket.Business.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<User>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return Result<IEnumerable<User>>.Success(users);
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            return Result<User>.Failure("Kullanıcı bulunamadı.");
            
        return Result<User>.Success(user);
    }

    public async Task<Result> AddUserAsync(User newUser)
    {
        // İŞ KURALI: Bu e-posta adresi sistemde zaten var mı?
        // (Şimdilik tüm kullanıcıları çekip hafızada arıyoruz)
        var allUsers = await _userRepository.GetAllAsync();
        var isEmailExists = allUsers.Any(u => u.Email == newUser.Email);

        if (isEmailExists)
        {
            return Result.Failure("Bu e-posta adresi ile zaten bir kayıt mevcut!");
        }

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return Result.Success("Kullanıcı başarıyla oluşturuldu.");
    }
}